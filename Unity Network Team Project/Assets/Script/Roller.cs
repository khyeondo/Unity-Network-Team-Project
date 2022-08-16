using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;


public class Roller : MonoBehaviourPunCallbacks, IPunObservable
{
    float localAngleY = 0.0f;
    float syncedAngleY = 0.0f;

    PhotonView pv;
    Vector3 curRot;
    Quaternion localAngle;

    Quaternion sourceAngle;

    // Start is called before the first frame update
    void Start()
    {
        localAngle = transform.rotation;
        pv = GetComponent<PhotonView>();
        if(PhotonNetwork.IsMasterClient)
            StartCoroutine(CallRPC());
    }

    // Update is called once per frame
    void Update()
    {
        if(PhotonNetwork.IsMasterClient)
            sourceAngle.eulerAngles += new Vector3(0, 15f * Time.deltaTime, 0);

        transform.rotation = Quaternion.Lerp(transform.rotation, localAngle, 10f * Time.deltaTime);
    }

    IEnumerator CallRPC()
    {
        while(true)
        {
            yield return new WaitForSeconds(0.1f);            
            pv.RPC("SyncAngle", RpcTarget.All, sourceAngle);
        }
    }

    [PunRPC]
    private void SyncAngle(Quaternion sourceAngle)
    {
        localAngle = sourceAngle;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
    }
}
