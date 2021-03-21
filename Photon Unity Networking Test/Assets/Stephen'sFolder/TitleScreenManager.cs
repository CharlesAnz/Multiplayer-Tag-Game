using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TitleScreenManager : MonoBehaviourPunCallbacks
{
    [SerializeField]
    public Button FindLobbiesButton, CreateLobbyButton, CustomisationMenuButton;
    public GameObject MainMenu, LobbyMenu, CustomMenu;
    public Text TestText, OutputColourText, OutputMonsterText, NicknameInputText;
    public GameObject NoRoomsText;

    public GameObject[] LobbyUIS;
    string[] LobbyCodes;

    public GameObject MyMonster;
    public GameObject[] MonsterPrefabs;
    public Material[] MaterialPrefabs;
    int CurrentMon = 0;
    int CurrentMat = 0;
    public GameObject SpawnPoint;

    public TransferData transferData;

    // Start is called before the first frame update
    void Start()
    {
        LobbyMenu.SetActive(false);

        if (!PhotonNetwork.IsConnected)
            PhotonNetwork.ConnectUsingSettings();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("OnConnectedToMaster was called by PUN.");
        PhotonNetwork.JoinLobby();
        //status.text = "Connected to Photon.";
        //buttonPlay.gameObject.SetActive(true);
        //playerName.gameObject.SetActive(true);
        //buttonLeave.gameObject.SetActive(false);
        //playerName.text = PlayerPrefs.GetString("PlayerName");
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        //Debug.Log("Oops, tried to join a room and failed. Calling CreateRoom!");

        //// failed to join a random room, so create a new one
        //PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = maxPlayersPerRoom });
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Yep, you managed to join a room!");
        //status.text = "Yep, you managed to join a room!";
        //buttonPlay.gameObject.SetActive(false);
        //playerName.gameObject.SetActive(false);
        //buttonLeave.gameObject.SetActive(true);
        //PhotonNetwork.Instantiate(player.name, new Vector3(Random.Range(-15, 15), 1, Random.Range(-15, 15)), Quaternion.Euler(0, Random.Range(-180, 180), 0), 0);
    }

    //public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    //{
    //    status.text = newPlayer.NickName + " has just entered.";
    //}

    //public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    //{
    //    status.text = otherPlayer.NickName + " has just left.";
    //}

    public void RefreshLobbies()
    {
        //MainMenu.SetActive(false);
        LobbyMenu.SetActive(true);
        CustomMenu.SetActive(false);
    }

    public void CreateRoom()
    {
        transferData.MyMonsterName = MonsterPrefabs[CurrentMon].name;
        //transferData.MyMaterial = MaterialPrefabs[CurrentMat];
        transferData.MaterialID = CurrentMat;
        transferData.JoinRoomID = "New Room";
        if (NicknameInputText.text != "") transferData.PlayerName = NicknameInputText.text;
        else transferData.PlayerName = "No Name";

        SceneManager.LoadScene("TestTagRoom");
    }

    public void ShowCustom()
    {
        LobbyMenu.SetActive(false);
        CustomMenu.SetActive(true);
    }

    public void JoinRoom(int id)
    {
        transferData.MyMonsterName = MonsterPrefabs[CurrentMon].name;
        //transferData.MyMaterial = MaterialPrefabs[CurrentMat];
        transferData.MaterialID = CurrentMat;
        transferData.JoinRoomID = LobbyCodes[id];
        if (NicknameInputText.text != "") transferData.PlayerName = NicknameInputText.text;
        else transferData.PlayerName = "No Name";

        SceneManager.LoadScene("TestTagRoom");
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        base.OnRoomListUpdate(roomList);

        if (roomList.Count == 0)
        {
            NoRoomsText.SetActive(true);
        }
        else NoRoomsText.SetActive(false);

        LobbyCodes = new string[roomList.Count];

        foreach (GameObject lobby in LobbyUIS)
        {
            lobby.SetActive(false);
        }

        string Output = "";
        for (int i = 0; i < roomList.Count; i++)
        {
            RoomInfo room = roomList[i];
            Output += room.Name + ":" + room.PlayerCount + "\n";
            LobbyUIS[i].transform.Find("Text").GetComponent<Text>().text = room.Name.Substring(0, 5) + ": " + room.PlayerCount + "/" + room.MaxPlayers;
            LobbyUIS[i].SetActive(true);

            LobbyCodes[i] = room.Name;
        }
        TestText.text = Output;
    }

    //Customisation
    public void NextMonster()
    {
        SetMonster(1);
    }

    public void LastMonster()
    {
        SetMonster(-1);
    }

    void SetMonster(int num)
    {
        Destroy(MyMonster);
        CurrentMon += num;
        if (CurrentMon < 0) CurrentMon = MonsterPrefabs.Length - 1;
        else if (CurrentMon >= MonsterPrefabs.Length) CurrentMon = 0;
        MyMonster = Instantiate(MonsterPrefabs[CurrentMon]);
        MyMonster.transform.SetPositionAndRotation(SpawnPoint.transform.position, SpawnPoint.transform.rotation);
        switch (CurrentMon)
        {
            case 0:
                OutputMonsterText.text = "Slime"; break;
            case 1:
                OutputMonsterText.text = "Shell"; break;
            case 2:
                OutputMonsterText.text = "Spider"; break;
            case 3:
                OutputMonsterText.text = "Skeleton"; break;
            default:
                break;
        }
        
        UpdateMaterial();
    }

    public void NextColour()
    {
        SetMaterial(1);
    }

    public void LastColour()
    {
        SetMaterial(-1);
    }

    void SetMaterial(int num)
    {
        CurrentMat += num;
        if (CurrentMat < 0) CurrentMat = MaterialPrefabs.Length - 1;
        else if (CurrentMat >= MaterialPrefabs.Length) CurrentMat = 0;
        switch (CurrentMat)
        {
            case 0:
                OutputColourText.text = "Default Colour"; break;
            case 1:
                OutputColourText.text = "Red"; break;
            case 2:
                OutputColourText.text = "Blue"; break;
            case 3:
                OutputColourText.text = "Yellow"; break;
            case 4:
                OutputColourText.text = "Green"; break;
            default:
                break;
        }
        UpdateMaterial();
    }

    void UpdateMaterial()
    {
        foreach (Transform transform in MyMonster.transform)
        {
            if (transform.gameObject.name != "Body")
            {
                transform.GetComponent<SkinnedMeshRenderer>().material = MaterialPrefabs[CurrentMat];
            }
        }
        
    }
}
