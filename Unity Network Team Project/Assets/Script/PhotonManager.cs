using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PhotonManager : MonoBehaviourPunCallbacks
{
    public static PhotonManager instance;
  

    public static PhotonManager Instance
    {
        get
        {
            if (!instance)
            {
                instance = FindObjectOfType(typeof(PhotonManager)) as PhotonManager;

                if (instance == null)
                    Debug.Log("no singleton obj");
            }
            return instance;
        }
    }

    public Toggle pwToggle;
    public InputField playerNickNameInputField;
    public InputField roomPwInputField;
    public InputField roomNameInputField;
    public GameObject joinRoomButtonContent;
    public GameObject joinRoomButtonPrefab;

    List<RoomInfo> roomList = new List<RoomInfo>();

    private void Awake()
    {
        PhotonNetwork.GameVersion = "1.0";
        //PhotonNetwork.NickName = "hyeon";
        PhotonNetwork.ConnectUsingSettings();

        DontDestroyOnLoad(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
    }


    public void JoinLobby()
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.JoinLobby();
            PhotonNetwork.LocalPlayer.NickName = playerNickNameInputField.text;
        }
        else
        {
            Debug.Log("서버 접속 실패 재접속 중...");
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public void CreateNewRoom()
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 10;
        roomOptions.IsVisible = true;
        roomOptions.CustomRoomProperties = new Hashtable()
        {
            { "password", roomPwInputField.text}
        };
        roomOptions.CustomRoomPropertiesForLobby = new string[] { "password" };

        if (roomNameInputField.text.Length != 0)
        {
            if (pwToggle.isOn)
            {
                PhotonNetwork.CreateRoom(roomNameInputField.text,
                    roomOptions);

            }
            else
            {
                PhotonNetwork.CreateRoom(roomNameInputField.text,
                    new RoomOptions { MaxPlayers = 10, IsVisible = true });
            }
        }

    }


    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        base.OnRoomListUpdate(roomList);
        Debug.Log("OnRoomListUpdate:" + roomList.Count);
        int roomCount = roomList.Count;
        for (int i = 0; i < roomCount; i++)
        {
            if (!roomList[i].RemovedFromList)
            {
                if (!this.roomList.Contains(roomList[i])) this.roomList.Add(roomList[i]);
                else this.roomList[this.roomList.IndexOf(roomList[i])] = roomList[i];
            }
            else if (this.roomList.IndexOf(roomList[i]) != -1)
            {
                this.roomList.RemoveAt(this.roomList.IndexOf(roomList[i]));
            }
        }
        RoomButtonListRenewal();
    }
    public void RoomButtonListRenewal()
    {
        for (int i = 0; i < roomList.Count; i++)
        {

            GameObject button = Instantiate(joinRoomButtonPrefab, joinRoomButtonContent.transform);
            button.GetComponent<JoinRoomButton>().roomInfo = roomList[i];
        }
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        Debug.Log("Succesee to join the room");
        PhotonNetwork.LoadLevel("RoomScene");
    }
    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();
        Debug.Log("Success to create room");
    }
    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();
        Debug.Log("Connected to lobby");
    }
    public override void OnLeftLobby()
    {
        base.OnLeftLobby();
        Debug.Log("leave to lobby");
    }
    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        Debug.Log("Connected!");
    }
}