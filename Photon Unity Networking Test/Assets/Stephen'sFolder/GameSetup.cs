using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class GameSetup : MonoBehaviourPunCallbacks
{
    TransferData transferData;
    GameObject MyMonster;
    int MatID;

    // Start is called before the first frame update
    void Start()
    {
        transferData = FindObjectOfType<TransferData>();

        PhotonNetwork.NickName = transferData.PlayerName;
        if (transferData.JoinRoomID != "New Room") PhotonNetwork.JoinRoom(transferData.JoinRoomID);
        else PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 5 });
        MatID = transferData.MaterialID;
    }

    public override void OnJoinedRoom()
    {
        BuildCharacter();
    }

    void BuildCharacter()
    {
        MyMonster = PhotonNetwork.Instantiate(transferData.MyMonsterName, new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 0), 0);
        MyMonster.GetComponent<SetupSkin>().SetMat(MatID);
    }
}
