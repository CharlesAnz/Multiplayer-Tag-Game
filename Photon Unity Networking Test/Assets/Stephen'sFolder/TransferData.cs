using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransferData : MonoBehaviour
{
    public string JoinRoomID = "";
    public GameObject MyMonster;
    public Material MyMaterial;

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }
}
