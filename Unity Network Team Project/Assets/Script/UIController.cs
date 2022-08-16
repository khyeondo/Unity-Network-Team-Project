using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public GameObject loginPanel;
    public GameObject roomListPanel;
    public InputField nameInputField;

    public GameObject createRoomPanel;
            
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetCreateRoomPanel(bool b)
    {
        createRoomPanel.SetActive(b);
    }

    public void LoginToRoom()
    {
        if(nameInputField.text.Length != 0)
        {
            loginPanel.SetActive(false);
            roomListPanel.SetActive(true);
        }
    }
}
