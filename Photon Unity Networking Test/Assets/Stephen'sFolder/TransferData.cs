using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransferData : MonoBehaviour
{
    public string JoinRoomID = "", PlayerName = "";
    //public GameObject MyMonster;
    public string MyMonsterName, MyMonsterMaterial;
    //public Material MyMaterial;

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }
}
