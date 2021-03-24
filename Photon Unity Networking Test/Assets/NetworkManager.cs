using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;


public class NetworkManager : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private Text nickname, status, room, players;
    [SerializeField]
    private Button buttonPlay, buttonLeave;
    [SerializeField]
    public InputField playerName;
    [SerializeField]
    private byte maxPlayersPerRoom = 4;

    public GameObject player;

    ChatManager chatManager;
    MultiplayerGameManager MultiplayerManager;

    TransferData transferData;
    int MatID;

    // Start is called before the first frame update
    void Start()
    {
        status.text = "Connecting...";
        buttonPlay.gameObject.SetActive(false);
        buttonLeave.gameObject.SetActive(false);
        playerName.gameObject.SetActive(false);

        chatManager = FindObjectOfType<ChatManager>();
        MultiplayerManager = FindObjectOfType<MultiplayerGameManager>();

        if (!PhotonNetwork.IsConnected)
            PhotonNetwork.ConnectUsingSettings();

        transferData = FindObjectOfType<TransferData>();
        PhotonNetwork.NickName = transferData.PlayerName;
        if (transferData.JoinRoomID != "New Room") PhotonNetwork.JoinRoom(transferData.JoinRoomID);
        else PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 5 });
        MatID = transferData.MaterialID;
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("OnConnectedToMaster was called by PUN.");
        status.text = "Connected to Photon.";
        buttonPlay.gameObject.SetActive(true);
        playerName.gameObject.SetActive(true);
        buttonLeave.gameObject.SetActive(false);
        playerName.text = PlayerPrefs.GetString("PlayerName");
    }

    public void Play()
    {
        PlayerPrefs.SetString("PlayerName", playerName.text);
        PhotonNetwork.NickName = playerName.text;
        PhotonNetwork.JoinRandomRoom();
        //PhotonNetwork.JoinOrCreateRoom("OurGreatRoomv2", new RoomOptions { MaxPlayers = 20 }, null);
    }

    public void Leave()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("Oops, tried to join a room and failed. Calling CreateRoom!");

        // failed to join a random room, so create a new one
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = maxPlayersPerRoom });
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Yep, you managed to join a room!");
        status.text = "Yep, you managed to join a room!";
        buttonPlay.gameObject.SetActive(false);
        playerName.gameObject.SetActive(false);
        buttonLeave.gameObject.SetActive(true);
        //PhotonNetwork.Instantiate(player.name, new Vector3(Random.Range(-15, 15), 1, Random.Range(-15, 15)), Quaternion.Euler(0, Random.Range(-180, 180), 0), 0);
        chatManager.Connect(transferData.PlayerName);
        BuildCharacter();
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        status.text = newPlayer.NickName + " has just entered.";
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        status.text = otherPlayer.NickName + " has just left.";
    }

    // Update is called once per frame
    void Update()
    {
        // Stephen's code for seeing the list of names
        //string NameString = "";
        //Dictionary<int, Photon.Realtime.Player> mydict = PhotonNetwork.CurrentRoom.Players;
        //foreach (var item in mydict)
        //{
        //    NameString += ": " + item.Value.NickName + "\n";
        //}

        if (PhotonNetwork.InRoom)
        {
            nickname.text = "Hello, " + PhotonNetwork.NickName;
            room.text = "Room: " + PhotonNetwork.CurrentRoom.Name;
            players.text = "Players: " + PhotonNetwork.CurrentRoom.PlayerCount + " of " + PhotonNetwork.CurrentRoom.MaxPlayers;

            players.text += ":\n";
            Dictionary<int, Player> mydict = PhotonNetwork.CurrentRoom.Players;
            int i = 1;
            foreach (var item in mydict)
                players.text += string.Format("{0,2}. {1}\n", (i++), item.Value.NickName);
        }
        else if (PhotonNetwork.IsConnected)
        {
            nickname.text = "Type your name below and hit PLAY!";
            room.text = "Not yet in a room...";
            players.text = "Players: 0";
        }
        else
            nickname.text = room.text = players.text = "";
    }

    void BuildCharacter()
    {
        player = PhotonNetwork.Instantiate(transferData.MyMonsterName, new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 0), 0);
        player.GetComponent<SetupSkin>().SetMat(MatID);
        chatManager.myCharacter = player.GetComponent<NewPlayerScript>();
        MultiplayerManager.MyPlayer = player;

        //player.GetComponent<NewPlayerScript>().HasBeenSetup = true;
    }
}
