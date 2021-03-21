using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class GameSetup : MonoBehaviourPunCallbacks
{
    TransferData transferData;

    GameObject MyMonster;

    // Start is called before the first frame update
    void Start()
    {
        transferData = FindObjectOfType<TransferData>();

        PhotonNetwork.NickName = transferData.PlayerName;
        if (transferData.JoinRoomID != "New Room") PhotonNetwork.JoinRoom(transferData.JoinRoomID);
        else PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 5 });

    }

    public override void OnJoinedRoom()
    {
        BuildCharacter();
    }

    void BuildCharacter()
    {
        //PhotonNetwork.Instantiate(player.name, new Vector3(Random.Range(-15, 15), 1, Random.Range(-15, 15)), Quaternion.Euler(0, Random.Range(-180, 180), 0), 0);

        PhotonNetwork.Instantiate(transferData.MyMonsterName, new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 0), 0);
        //MyMonster = Instantiate(transferData.MyMonster);
        //foreach (Transform transform in MyMonster.transform)
        //{
        //    if (transform.gameObject.name != "Body")
        //    {
        //        transform.GetComponent<SkinnedMeshRenderer>().material = transferData.MyMaterial;
        //    }
        //}
    }

    // Update is called once per frame
    void Update()
    {

    }
}
