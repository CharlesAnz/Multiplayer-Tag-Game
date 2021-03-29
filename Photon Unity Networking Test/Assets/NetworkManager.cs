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

    public GameObject player = null;

    ChatManager chatManager;
    MultiplayerGameManager MultiplayerManager;

    TransferData transferData;
    int MatID;

    public GameObject BombPrefab;
    public GameObject[] SpawnPoints;

    public Camera MainCam, LocalCam;
    public GameObject WinnerPanel;
    public float GameEndTime = 0f;

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
        else PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 5, PublishUserId = true });
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
        if (transferData) Destroy(transferData.gameObject);
        PhotonNetwork.LeaveRoom();
        OnLeftRoom();
    }
    public void OnLeftRoom()
    {
        PhotonNetwork.LoadLevel(0);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("Oops, tried to join a room and failed. Calling CreateRoom!");

        // failed to join a random room, so create a new one
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = maxPlayersPerRoom });
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Room joined!");
        status.text = "Room joined!";
        buttonPlay.gameObject.SetActive(false);
        playerName.gameObject.SetActive(false);
        buttonLeave.gameObject.SetActive(true);
        //PhotonNetwork.Instantiate(player.name, new Vector3(Random.Range(-15, 15), 1, Random.Range(-15, 15)), Quaternion.Euler(0, Random.Range(-180, 180), 0), 0);
        chatManager.Connect(transferData.PlayerName);
        
        StartCoroutine(BuildTimer());
        BombControl TheBomb = FindObjectOfType<BombControl>(); //Instantiate(BombPrefab);
        if (PhotonNetwork.IsMasterClient)
        {
            
            double time = PhotonNetwork.Time;
            photonView.RPC("SetTimey", RpcTarget.AllBuffered, time, PhotonNetwork.LocalPlayer.ActorNumber);

            //player.GetComponent<NewPlayerScript>().HasBomb = true;
            //StartCoroutine(player.GetComponent<NewPlayerScript>().GiveBomb());
        }
    }

    private IEnumerator BuildTimer()
    {
        /* get timer from MultiplayerGameManager */
        yield return new WaitForSeconds(1);

        BuildCharacter();
    }

    [PunRPC]
    void SetTimey(double TimeLeft, int target)
    {
        FindObjectOfType<BombControl>().settime(TimeLeft, target);
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        status.text = newPlayer.NickName + " just entered.";
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        status.text = otherPlayer.NickName + " just left.";
    }

    // Update is called once per frame
    void Update()
    {
        if (PhotonNetwork.InRoom)
        {
            nickname.text = "Hello, " + PhotonNetwork.NickName;
            room.text = "Room: " + PhotonNetwork.CurrentRoom.Name.Substring(0, 5);
            players.text = "Players: " + PhotonNetwork.CurrentRoom.PlayerCount + " of " + PhotonNetwork.CurrentRoom.MaxPlayers;

            players.text += ":\n";
            //Dictionary<int, Player> mydict = PhotonNetwork.CurrentRoom.Players;
            //int i = 1;
            //foreach (var item in mydict)
            //    players.text += string.Format("{0,2}. {1}\n", (i++), item.Value.NickName);

            int numofalive = 0, numofdead = 0;
            string Winner = "";
            foreach (NewPlayerScript playerScript in FindObjectsOfType<NewPlayerScript>())
            {
                players.text += playerScript.GetComponent<PhotonView>().Owner.NickName;
                if (playerScript.IsGhost)
                {
                    players.text += " - Dead";
                    numofdead++;
                }
                else
                {
                    players.text += " - Alive";
                    numofalive++;
                    Winner = playerScript.GetComponent<PhotonView>().Owner.NickName;
                }
                players.text += "\n";
            }
            if (numofalive == 1 && numofdead > 0)
            {
                photonView.RPC("GameEnd", RpcTarget.AllBufferedViaServer, Winner);
            }

            if (GameEndTime > 0 && Time.time - GameEndTime >= 5)
            {
                Leave();
            }
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

    public void BuildCharacter()
    {
        player = PhotonNetwork.Instantiate(transferData.MyMonsterName, GetSpawn().transform.position, Quaternion.Euler(0, 0, 0), 0);
        player.GetComponent<SetupSkin>().SetMat(MatID);
        player.GetComponent<NewPlayerScript>().MyCam = LocalCam.gameObject;
        MainCam.enabled = false;
        LocalCam.enabled = true;
        chatManager.myCharacter = player.GetComponent<NewPlayerScript>();
        MultiplayerManager.MyPlayer = player;

        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(player.GetComponent<NewPlayerScript>().GiveBomb());
        }

        //player.GetComponent<NewPlayerScript>().HasBeenSetup = true;
    }

    GameObject GetSpawn()
    {
        List<GameObject> CanGoSpawns = new List<GameObject>();
        foreach (GameObject spawn in SpawnPoints)
        {
            bool IsClose = false;
            //print("Num of players is: " + FindObjectsOfType<NewPlayerScript>().Length);
            foreach (NewPlayerScript player in FindObjectsOfType<NewPlayerScript>())
            {
                if (Vector3.Distance(player.gameObject.transform.position, spawn.transform.position) <= 20)
                {
                    IsClose = true;
                }
            }
            if (!IsClose) CanGoSpawns.Add(spawn);
        }
        if (CanGoSpawns.Count > 0)
        {
            int random = Random.Range(0, CanGoSpawns.Count);
            return CanGoSpawns[random];
        }
        else
        {
            print("All spawns covered");
            return SpawnPoints[0];
        }
    }

    public void Collided(int TargetID)
    {
        photonView.RPC("UpdateAll", RpcTarget.All, TargetID);
    }

    [PunRPC]
    void UpdateAll(int TargetID)
    {
        FindObjectOfType<BombControl>().BombHolderId = TargetID;
        if (PhotonNetwork.LocalPlayer.ActorNumber == TargetID)
        {
            StartCoroutine(player.GetComponent<NewPlayerScript>().GiveBomb());
        }
    }

    [PunRPC]
    void GameEnd(string Winner)
    {
        WinnerPanel.SetActive(true);
        WinnerPanel.transform.Find("Text").GetComponent<Text>().text = Winner + " has won!";
        GameEndTime = Time.time;
    }

    public void BombExploded(double t)
    {
        List<NewPlayerScript> PList = new List<NewPlayerScript>();
        foreach (NewPlayerScript playerScript in FindObjectsOfType<NewPlayerScript>())
        {
            if (!playerScript.IsGhost)
            {
                PList.Add(playerScript);
            }
        }
        if (PList.Count > 0)
        {
            int random = Random.Range(0, PList.Count);
            int AN = PList[random].gameObject.GetComponent<PhotonView>().Owner.ActorNumber;
            photonView.RPC("SetTimey", RpcTarget.AllBuffered, t, AN);
        }
        else
        {
            photonView.RPC("SetTimey", RpcTarget.AllBuffered, t, PhotonNetwork.LocalPlayer.ActorNumber);
        }
        
    }
}
