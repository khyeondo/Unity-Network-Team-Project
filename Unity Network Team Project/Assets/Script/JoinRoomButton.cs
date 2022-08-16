using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class JoinRoomButton : MonoBehaviour
{
    public RoomInfo roomInfo;
    Button button;
    public Text roomName;
    GameObject inputPwPanel;
    Button pwButton;

    // Start is called before the first frame update
    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(JoinRoom);
        roomName.text = roomInfo.Name;
        inputPwPanel = GameObject.FindGameObjectWithTag("PwPanel");
        pwButton = inputPwPanel.transform.GetChild(1).GetComponent<Button>();
        
    }

    void JoinRoom()
    {
        if(roomInfo.PlayerCount >= 8)
        {
            Debug.Log("πÊ¿Ã ∞°µÊ¬¸");
        }
        else if(roomInfo.CustomProperties["password"] != null)
        {
            inputPwPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
            pwButton.onClick.RemoveAllListeners();
            pwButton.onClick.AddListener(CheckPw);            
        }
        else
        {
            PhotonNetwork.JoinRoom(roomInfo.Name);
        }
    }

    public void CheckPw()
    {
        if(inputPwPanel.transform.GetChild(0).GetComponent<InputField>().text == 
            roomInfo.CustomProperties["password"] as string)
        {
            PhotonNetwork.JoinRoom(roomInfo.Name);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
