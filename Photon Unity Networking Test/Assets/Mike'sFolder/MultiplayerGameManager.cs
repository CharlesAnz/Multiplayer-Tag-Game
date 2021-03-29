using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;


public class MultiplayerGameManager : MonoBehaviourPun
{
    public const float time_to_become_ghost = 4.0f;
    public const float cheer_timer = 3.0f;

    public bool GameStarted = false;
    public float StartTime = 0f;
    public GameObject BombPrefab;
    public GameObject TheBomb;
    public GameObject MyPlayer;

    public void Start()
    {

    }

    public void Update()
    {
        
    }

    
}
