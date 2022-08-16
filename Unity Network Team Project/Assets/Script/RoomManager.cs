using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class RoomManager : MonoBehaviourPunCallbacks
{
    Room room;
    public Button startButton;
    public Button pre;
    public Button next;
    public Text[] playerNames;
    public Sprite[] mapSprite;
    public Image map;
    public string[] sceneTitle;


    PhotonView pv;
    int mapIdx = 0;

    // Start is called before the first frame update
    void Start()
    {
        pv = GetComponent<PhotonView>();
        room = PhotonNetwork.CurrentRoom;
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            //room.SetMasterClient(PhotonNetwork.LocalPlayer);
            room.MaxPlayers = 8;
            pre.gameObject.SetActive(true);
            next.gameObject.SetActive(true);
            startButton.gameObject.SetActive(true);
        }

        MachName();
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

    }

    // Update is called once per frame
    void Update()
    {
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        MachName();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);

        MachName();
    }

    public void StartGame()
    {
        Debug.Log("start");
        //room.IsVisible = false;
        room.IsOpen = false;
        pv.RPC("StartGameRPC", RpcTarget.All);
    }

    [PunRPC]
    public void StartGameRPC()
    {
        PhotonNetwork.LoadLevel(sceneTitle[mapIdx]);
    }

    void SetMapIndex(int idx)
    {        
        pv.RPC("SetMapIndexRPC", RpcTarget.All,(int)Mathf.Clamp(idx,0,sceneTitle.Length));
           
    }

    [PunRPC]
    public void SetMapIndexRPC(int idx)
    {
        mapIdx = idx;
        map.sprite = mapSprite[idx];
    }

    private void MachName()
    {
        foreach (Text text in playerNames)
        {
            text.text = "";
        }

        int i = 0;
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            playerNames[i].text = player.NickName;
            i++;
        }

        if (PhotonNetwork.IsMasterClient)
        {
            startButton.gameObject.SetActive(true);
        }

    }

    public void OnPreButtonClick()
    {
        SetMapIndex(mapIdx - 1);
    }

    public void OnNextButtonClick()
    {
        SetMapIndex(mapIdx + 1);
    }
}
