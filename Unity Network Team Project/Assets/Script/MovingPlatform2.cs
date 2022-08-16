using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class MovingPlatform2 : MonoBehaviour, IPunObservable
{
    public GameObject destination1, destination2;
    public float movingSpeed = 1.0f;
    public bool directionIsForward = true;
    private bool isWaiting = false;
    private float waitTimer = 0.0f;
    private float maxWaitTime = 5.0f;

    float sourceXPos;
    float localXPos;
    float rpcTimer = 0.0f; 

    PhotonView pv;

    // Start is called before the first frame update
    void Start()
    {
        pv = GetComponent<PhotonView>();
        sourceXPos = transform.position.x;        
    }

    // Update is called once per frame
    void Update()
    {
        if (isWaiting)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0)
            {
                isWaiting = false;
            }
        }
        else
        {
            MovePlatform();
        }

        float x = Mathf.Lerp(transform.position.x, localXPos, 10f * Time.deltaTime);
        Debug.Log(x);
        transform.position = new Vector3(x, transform.position.y, transform.position.z);
    }

    void MovePlatform()
    {
        if(PhotonNetwork.IsMasterClient)
        {
            if (directionIsForward)
            {
                sourceXPos += movingSpeed * Time.deltaTime;
                if (sourceXPos > destination2.transform.position.x)
                {
                    directionIsForward = false;
                    isWaiting = true;
                    waitTimer = maxWaitTime;
                }

                //transform.Translate(Vector3.right * Time.deltaTime * movingSpeed);
                //if (transform.position.x > destination2.transform.position.x)
                //{
                //
                //}
            }
            else
            {
                sourceXPos -= movingSpeed * Time.deltaTime;
                if (sourceXPos < destination1.transform.position.x)
                {
                    directionIsForward = true;
                    isWaiting = true;
                    waitTimer = maxWaitTime;
                }

                //transform.Translate(Vector3.left * Time.deltaTime * movingSpeed);
                //if (transform.position.x < destination1.transform.position.x)
                //{
                //    directionIsForward = true;
                //    isWaiting = true;
                //    waitTimer = maxWaitTime;
                //}
            }
            rpcTimer += Time.deltaTime;

            if(rpcTimer >= 0.1f)
            {
                pv.RPC("SyncPos", RpcTarget.All, sourceXPos);
                rpcTimer -= 0.1f;
            }
        }
    }

    [PunRPC]
    void SyncPos(float x)
    {
        localXPos = x;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            other.transform.parent = transform;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            other.transform.parent = null;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
    }
}
