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
    //string[] LobbyCodes;
    List<RoomInfo> Rooms = new List<RoomInfo>();

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

    public override void OnConnectedToMaster()
    {
        Debug.Log("OnConnectedToMaster was called by PUN.");
        PhotonNetwork.JoinLobby();
    }

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

        SceneManager.LoadScene("MultiplayerScreen");
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
        transferData.JoinRoomID = Rooms[id].Name; //LobbyCodes[id];
        if (NicknameInputText.text != "") transferData.PlayerName = NicknameInputText.text;
        else transferData.PlayerName = "No Name";

        SceneManager.LoadScene("MultiplayerScreen");
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        base.OnRoomListUpdate(roomList);

        if (Rooms.Count == 0)
        {
            NoRoomsText.SetActive(true);
        }
        else NoRoomsText.SetActive(false);

        //LobbyCodes = new string[roomList.Count];

        foreach (GameObject lobby in LobbyUIS)
        {
            lobby.SetActive(false);
        }

        for (int i = 0; i < roomList.Count; i++)
        {
            RoomInfo room = roomList[i];
            int index = Rooms.FindIndex(x => x.Name == room.Name);
            if (index == -1)
            {
                //new room
                Rooms.Add(room);
            }
            else
            {
                Rooms[index] = room;
            }
        }
            

        string Output = "";
        for (int i = 0; i < Rooms.Count; i++)
        {
            RoomInfo room = Rooms[i];
            if (room.PlayerCount > 0)
            {
                Output += room.Name + ":" + room.PlayerCount + "\n";
                LobbyUIS[i].transform.Find("Text").GetComponent<Text>().text = room.Name.Substring(0, 5) + ": " + room.PlayerCount + "/" + room.MaxPlayers;
                LobbyUIS[i].SetActive(true);
            }
            

            //LobbyCodes[i] = room.Name;
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
