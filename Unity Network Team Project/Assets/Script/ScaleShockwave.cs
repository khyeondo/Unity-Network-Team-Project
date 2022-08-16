using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ScaleShockwave : MonoBehaviour
{
    private GameObject shock;
    public GameObject pickedUpPlayer;
    private bool pickedUp;

    public float scaleSize = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        shock = gameObject.transform.GetChild(0).gameObject;
        shock.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (pickedUp)
        {
            StartCoroutine(ScalingShockwave());
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            pickedUp = true;
            gameObject.GetComponent<MeshRenderer>().enabled = false;
            
            PhotonManagerInGame.inGameManager.RequestGetItem();

            pickedUpPlayer = other.gameObject;
        }
    }

    public void Boom(int actorNumber)
    {
        shock.SetActive(true);
        shock.transform.localScale *= scaleSize;
        shock.GetComponent<Shockwave>().pickedUpActorNumber = actorNumber;
    }

    IEnumerator ScalingShockwave()
    {
        yield return new WaitForSeconds(0.3f);

        Destroy(gameObject);
    }
}