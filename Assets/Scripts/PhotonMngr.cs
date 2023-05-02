using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine.SceneManagement;
using System;

public class PhotonMngr : MonoBehaviourPunCallbacks, IOnEventCallback
{


    string nextSceneName = "Game";
    private bool canStartGame = false;


    [Header("inputfield")]
    public TMP_InputField username;
    public TMP_InputField roomNameText;
    public TMP_InputField maxPlayer;


    [Header("Buttons")]

    public Button loginbtn;
    public Button JoinRoomBtn;
    public Button GotoCreateR_Panel;
    public Button CancleBtn;
    public Button CreateRoomBtn;
    public Button BackBtn;
    public Button Back_CRPanel;
    public Button LeaveRoomBtn;
    public Button PlayBtnToStartGame;

    [Header("Panels")]

    public GameObject EnterPanel;
    public GameObject ConnectingPanel;
    public GameObject LobbyPanel;
    public GameObject CreateRoomPanel;
    public GameObject PlayPanel;
    public GameObject RoomListPanel;
    public GameObject playBtn;



    [Header("Prefabs")]

    public GameObject roomListPrefab;
    public GameObject roomListParent;


    [Header("Inside Room Panel")]
    public GameObject InsideRoomPanel;
    public GameObject playerListPrefab;
    public GameObject playerListPrefabParent;

    // dict for saving room information
    private Dictionary<string, RoomInfo> roomData;               //create dict for store roomInfo
    private Dictionary<string, GameObject> roomListGameobject;   //create dict for destroy the gameobject
    private Dictionary<int, GameObject> playerListGameObject;


    


    #region Unity_Methods
    // Start is called before the first frame update
    void Start()
    {
        // init. of dist
        roomData = new Dictionary<string, RoomInfo>();
        roomListGameobject = new Dictionary<string, GameObject>();
        playerListGameObject = new Dictionary<int, GameObject>();

        // activate first panel
        ActivatePanel(EnterPanel.name);

        // login btn click
        loginbtn.onClick.AddListener(OnLoginClick);

        // goto create room panel
        GotoCreateR_Panel.onClick.AddListener(OnGotoCreateR_PanelClick);

        // cancle button to go previous panel
        CancleBtn.onClick.AddListener(OnCancleBtnClick);

        // create room btn
        CreateRoomBtn.onClick.AddListener(CreationRoom);

        // btn to go all rooms list
        JoinRoomBtn.onClick.AddListener(GotoAllRoomList);


        // goto back
        BackBtn.onClick.AddListener(GotoBackPanel);

        // goto back
        Back_CRPanel.onClick.AddListener(GotoBackPanel);

        // leave the room
        LeaveRoomBtn.onClick.AddListener(LeaveRoom);

        PlayBtnToStartGame.onClick.AddListener(OnPlayBtnClick);

        // add callback
        PhotonNetwork.AddCallbackTarget(this);
    }


    // Find the LoadScene script in the scene


    // Call the LoadNextScene method

    public void LoadNextScene()
    {
        // Raise the custom event to signal the scene load
        //object[] data = new object[] { nextSceneName };
        RaiseEventOptions options = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(1, nextSceneName, options, SendOptions.SendReliable);
    }


    void OnDestroy()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
        //Debug.Log("s");
    }

    
    // Update is called once per frame
    void Update()
    {
        
        //Debug.Log("Netwerk State : " + PhotonNetwork.NetworkClientState);  another way to check connection
    }
    #endregion

    #region UI_Methods
    public void OnLoginClick()
    {
        string name = username.text;
        if (string.IsNullOrEmpty(name))
        {
            Debug.Log("Please Enter your name");
        }
        else
        {
            PhotonNetwork.LocalPlayer.NickName = name;  // taking ref of name from input field
            PhotonNetwork.ConnectUsingSettings();       // making connection
            ActivatePanel(ConnectingPanel.name);        // activate connecting panel while connecting to the Photon
        }
    }


    public void OnGotoCreateR_PanelClick()
    {
        ActivatePanel(CreateRoomPanel.name);
    }


    public void CreationRoom()
    {
        string roomName = roomNameText.text;
        if (string.IsNullOrEmpty(roomName))
        {
            Debug.Log("Plaese Enter Room Name First");              //roomName = roomName + Random.Range(200, 550);
        }
        RoomOptions roomOptions = new RoomOptions();                // add other things like maxplayer etc.
        roomOptions.MaxPlayers = (byte)int.Parse(maxPlayer.text);   // for giving max player during room creation
        PhotonNetwork.CreateRoom(roomName, roomOptions);            // create the room
    }

    public void OnCancleBtnClick()
    {
        ActivatePanel(LobbyPanel.name);
    }


    public void GotoAllRoomList()
    {
        
        // if PhotonNetwork.Inlobby means if the player is not in the lobby then make it possible to joinLobby() that make the connection
        // btwn all the players to see all rooms created by each other
        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
            Debug.Log(PhotonNetwork.LocalPlayer.NickName + " Joined the lobby");
        }
        ActivatePanel(RoomListPanel.name);
    }


    public void GotoBackPanel()
    {
        if (PhotonNetwork.InLobby)
        {
            PhotonNetwork.LeaveLobby();                 // WHENEVER WE LEAVE A LOBBY A PHOTON CALLBACK CALLED NAMED (OnLeftLobby())
            Debug.Log(PhotonNetwork.LocalPlayer.NickName + " out from the lobby");
        }
        ActivatePanel(LobbyPanel.name);
    }


    public void LeaveRoom()
    {
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
            Debug.Log(PhotonNetwork.LocalPlayer.NickName + " leave the room");
        }
        //PhotonNetwork.JoinLobby();
        ActivatePanel(RoomListPanel.name);
    }

    public void OnPlayBtnClick()
    {
        if (PhotonNetwork.IsMasterClient && canStartGame)
        {

            // LOAD THE GAMESCENE THAT LOAD THE "GAME" SCENE THROUGH OnEvent()
            LoadNextScene();
             
        }
        else
        {
            Debug.Log("There is only " + PhotonNetwork.CurrentRoom.PlayerCount + " players joined, wait until the player count be " + PhotonNetwork.CurrentRoom.MaxPlayers);
        }

    }

    #endregion

    #region PHOTON_CALLBACKS

    // CALL WHEN CONNECTED TO THE INTERNET
    public override void OnConnected()
    {
        Debug.Log("Connected to Internet: ");
    }

    // WHEN PLAYER CONNECTED TO MASTER
    public override void OnConnectedToMaster()
    {
        Debug.Log(PhotonNetwork.LocalPlayer.NickName + " is connected to photon");
        ActivatePanel(LobbyPanel.name);
    }

    // CALL WHEN CREATED SUCCESSFULLY
    public override void OnCreatedRoom()
    {
        Debug.Log(PhotonNetwork.CurrentRoom.Name + " is created!");
    }


    // CALL WHEN JOIN THE ROOM
    public override void OnJoinedRoom()
    {
        
        Debug.Log(PhotonNetwork.LocalPlayer.NickName + " Joined the room!");
        ActivatePanel(InsideRoomPanel.name);
        
        
        // getting player list
        foreach(Player p in PhotonNetwork.PlayerList)
        {
            GameObject players = Instantiate(playerListPrefab);
            players.transform.SetParent(playerListPrefabParent.transform);

            players.transform.localScale = Vector3.one;

            players.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text = p.NickName;
            if(p.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
            {
                players.transform.GetChild(1).gameObject.SetActive(true); 
            }
            else
            {
                players.transform.GetChild(1).gameObject.SetActive(false);
            }
            playerListGameObject.Add(p.ActorNumber, players);
        }
        Debug.Log("players in the room " + PhotonNetwork.CurrentRoom.PlayerCount);

        // ENABALING PLAY BUTTON FOR MASTER(ROOM CREATER)
        if (PhotonNetwork.IsMasterClient)
        {
            playBtn.SetActive(true);
        }
        else
        {
            playBtn.GetComponent<Button>().interactable = false;
        }

    }

    // REMOTE PLAYER JOIN / LEAVE
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            canStartGame = true;
        }
        
        GameObject players = Instantiate(playerListPrefab);
        players.transform.SetParent(playerListPrefabParent.transform);

        players.transform.localScale = Vector3.one;

        players.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text = newPlayer.NickName;
        if (newPlayer.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
        {
            players.transform.GetChild(1).gameObject.SetActive(true);
        }
        else
        {
            players.transform.GetChild(1).gameObject.SetActive(false);
        }

        playerListGameObject.Add(newPlayer.ActorNumber, players);
        
    }


    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount != PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            playBtn.SetActive(false);
        }
        Destroy(playerListGameObject[otherPlayer.ActorNumber]);
        playerListGameObject.Remove(otherPlayer.ActorNumber);
        
        if (PhotonNetwork.IsMasterClient)
        {
            playBtn.SetActive(true);
        }
        else
        {
            playBtn.SetActive(false);
        }
    }

    public override void OnLeftRoom()
    {
        ActivatePanel(LobbyPanel.name);
        if(playerListGameObject.Values != null) 
        { 
            foreach (GameObject obj in playerListGameObject.Values)
            {
                Destroy(obj);
            }
        }
    }

    // FOR UPDATE THE ROOM LIST
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        // clear list
        ClearRoomList();
        foreach (RoomInfo rooms in roomList) 
        {
            Debug.Log("Room name is " + rooms.Name);
            if (!rooms.IsOpen || !rooms.IsVisible || rooms.RemovedFromList)
            {
                if (roomData.ContainsKey(rooms.Name))
                {
                    roomData.Remove(rooms.Name);
                }
            }
            else
            {
                if (roomData.ContainsKey(rooms.Name))
                {
                    // update list
                    roomData[rooms.Name] = rooms;
                }
                else
                {
                    roomData.Add(rooms.Name, rooms);
                }
            }
        }

        // generate list item
        foreach (RoomInfo roomItem in roomData.Values)
        {
            GameObject roomListItemObject = Instantiate(roomListPrefab);                //, roomListParent.transform
            roomListItemObject.transform.SetParent(roomListParent.transform);   
            roomListItemObject.transform.localScale = Vector3.one;

            //get values
            roomListItemObject.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text = roomItem.Name;
            roomListItemObject.transform.GetChild(1).gameObject.GetComponent<TMP_Text>().text = roomItem.PlayerCount + "/"  + roomItem.MaxPlayers;

            // button
            if (roomItem.PlayerCount == roomItem.MaxPlayers)
            {
                //roomListItemObject.transform.GetChild(2).gameObject.GetComponent<Button>().interactable(false);
                roomListItemObject.transform.GetChild(2).gameObject.GetComponent<Button>().interactable = false;
            }
            else
            {
                roomListItemObject.transform.GetChild(2).gameObject.GetComponent<Button>().onClick.AddListener(() => RoomJoinFromList(roomItem.Name));
                
            }
            roomListGameobject.Add(roomItem.Name, roomListItemObject);

        }

        

    }

    // CALL WHEN LEAVE THE LOBBY
    public override void OnLeftLobby()
    {
        ClearRoomList();
        roomData.Clear();
    }

    
    public void OnEvent(EventData eventData)
    {
        // Check if this is the custom event
        if (eventData.Code == 1)
        {
            // Get the scene name from the event data
            var sceneName = (string)eventData.CustomData;
            Debug.Log("Custom Data Type: " + eventData.CustomData.GetType());
            // Load the new scene for all players
            SceneManager.LoadScene(sceneName);

            
        }
    }
    // Room join failed
    


    #endregion


    #region public_methods

    


    public void RoomJoinFromList(string roomName)
    {
        
        if (PhotonNetwork.InLobby)
        {
            PhotonNetwork.LeaveLobby();
            Debug.Log(PhotonNetwork.LocalPlayer.NickName +" Leave the lobby after joined a Room: ");
        }
        PhotonNetwork.JoinRoom(roomName);
    }


    public void ClearRoomList() 
    {
        if (roomListGameobject.Count > 0)
        {
            foreach (var v in roomListGameobject.Values)
            {
                Destroy(v);
            }
            roomListGameobject.Clear();
        }
    }


    public void ActivatePanel(string PanelName)
    {
        EnterPanel.SetActive(PanelName.Equals(EnterPanel.name));
        ConnectingPanel.SetActive(PanelName.Equals(ConnectingPanel.name));
        LobbyPanel.SetActive(PanelName.Equals(LobbyPanel.name));
        PlayPanel.SetActive(PanelName.Equals(PlayPanel.name));
        CreateRoomPanel.SetActive(PanelName.Equals(CreateRoomPanel.name));
        RoomListPanel.SetActive(PanelName.Equals(RoomListPanel.name));
        InsideRoomPanel.SetActive(PanelName.Equals(InsideRoomPanel.name));
    }


    #endregion
}
