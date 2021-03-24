using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;


public class MultiplayerGameManager : MonoBehaviourPun
{
    public const float time_to_become_ghost = 4.0f;
    public const float cheer_timer = 5.0f;

    public bool GameStarted = false;
    public float StartTime = 0f;
    public GameObject TheBomb;
    public GameObject MyPlayer;

    public void Start()
    {
        
    }

    public void Update()
    {
        if (PhotonNetwork.IsConnectedAndReady && PhotonNetwork.IsMasterClient)
        {
            if (!GameStarted)
            {
                if (PhotonNetwork.CurrentRoom.PlayerCount >= 3)
                {
                    print("Game started");
                    StartTime = Time.time;
                    SetupGame();

                    GameStarted = true;
                }
            }
            else
            {

            }
        }
        
    }

    void SetupGame()
    {
        TheBomb = PhotonNetwork.Instantiate("Bomb", new Vector3(Random.Range(-15, 15), 1, Random.Range(-15, 15)), Quaternion.Euler(0, Random.Range(-180, 180), 0), 0);
        TheBomb.GetComponent<BombControl>().SetupBomb(StartTime, MyPlayer);
    }
}
