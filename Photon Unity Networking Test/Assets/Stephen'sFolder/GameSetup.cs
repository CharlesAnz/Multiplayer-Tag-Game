using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class GameSetup : MonoBehaviour
{
    TransferData transferData;

    // Start is called before the first frame update
    void Start()
    {
        transferData = FindObjectOfType<TransferData>();
    }

    // Update is called once per frame
    void Update()
    {
        print(PhotonNetwork.IsConnected);
    }
}
