using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class PhotonManagerInGame : MonoBehaviour, IPunObservable
{
    public FollowingCamera followingCamera;
    public PlayerPointer playerPointer;
    public Button toLobbyButton;
    public Text winner;

    private List<GameObject> players;

    private GameObject player;
    public GameObject spawnPos;

    public GameObject itemPrefab;
    public GameObject itemSpawnPos;
    bool isItemSpawned = false;
    float itemTimer = 0f;
    ScaleShockwave spawnedItem;

    bool isPickedUpItem = false;

    static public PhotonManagerInGame inGameManager;

    PhotonView pv;

    // Start is called before the first frame update
    void Start()
    {
        players = new List<GameObject>();
        toLobbyButton.gameObject.SetActive(false);
        winner.gameObject.SetActive(false);

        inGameManager = this;
        StartCoroutine(CreatePlayer());
        StartCoroutine(SetCameraTarget());
        pv = GetComponent<PhotonView>();
    }

    // Update is called once per frame
    void Update()
    {
        if (PhotonNetwork.IsMasterClient && isItemSpawned == false)
        {
            itemTimer += Time.deltaTime;

            if (itemTimer >= 10f)
            {
                SpawnItem();
                isItemSpawned = true;
                itemTimer -= 10f;
            }
        }
    }

    IEnumerator CreatePlayer()
    {
        Vector3 pos = spawnPos.transform.GetChild(PhotonNetwork.LocalPlayer.ActorNumber).position;
        player = PhotonNetwork.Instantiate(
            "PlayerCharacter",
            pos,
            Quaternion.identity,
            0);

        yield return null;
    }

    IEnumerator SetCameraTarget()
    {
        while (player == null)
        {
            yield return new WaitForSeconds(0.1f);
        }
        followingCamera.target = player.transform;
        playerPointer.target = player.transform;
        player.GetComponent<CharacterController>().cameraBasisPos = followingCamera.transform;

        yield return null;
    }

    public void AddPlayer(GameObject player)
    {
        players.Add(player);
    }

    public void OnDeathPlayer(GameObject player)
    {
        players.Remove(player);
        Debug.Log(players.Count);
        toLobbyButton.gameObject.SetActive(true);
        if (players.Count == 1 &&
            PhotonNetwork.LocalPlayer.ActorNumber == players[0].GetComponent<CharacterController>().actorNumber)
        {
            OnWin();
        }
    }

    public void OnWin()
    {
        Debug.Log(PhotonNetwork.LocalPlayer.NickName);
        pv.RPC("OnWinRPC", RpcTarget.All, PhotonNetwork.LocalPlayer.NickName);
    }



    [PunRPC]
    public void OnWinRPC(string nickName)
    {
        Debug.Log(nickName);
        winner.text = "Winner!\n" + nickName;
        winner.gameObject.SetActive(true);
        toLobbyButton.gameObject.SetActive(true);

    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
    }

    public void ToLobby()
    {
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene("LobbyScene");
    }

    void SpawnItem()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            int idx = Random.Range(0, itemSpawnPos.transform.GetChildCount());
            pv.RPC("RPCSpawnItem", RpcTarget.All, idx);
        }
    }

    [PunRPC]
    void RPCSpawnItem(int spawnPosIdx)
    {
        spawnedItem = Instantiate(
            itemPrefab,
            (itemSpawnPos.transform.GetChild(spawnPosIdx).position),
            Quaternion.identity
            ).GetComponent<ScaleShockwave>();
        isPickedUpItem = false;
    }

    public void RequestGetItem()
    {                
        pv.RPC("CheckFirstGetItem", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.ActorNumber);
    }

    [PunRPC]
    void CheckFirstGetItem(int actorNumber)
    {
        if(isPickedUpItem == false)
        {
            pv.RPC("allowGetTime", RpcTarget.All, actorNumber);
            isPickedUpItem = true;
        }
    }

    [PunRPC]
    void allowGetTime(int actorNumber)
    {
        isItemSpawned = false;
        spawnedItem.Boom(actorNumber);
    }

    public void OnPickedItem()
    {
        if(PhotonNetwork.IsMasterClient)
        {
            if(isPickedUpItem == false)
            {
                isPickedUpItem = true;
            }
        }
    }
}
