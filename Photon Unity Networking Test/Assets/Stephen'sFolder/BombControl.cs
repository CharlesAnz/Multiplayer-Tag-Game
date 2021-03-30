using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class BombControl : MonoBehaviourPun
{
    public const float bomb_timer = 30.0f;
    public double StartTime = 0.0;
    bool active = false;
    public GameObject target;
    public Text DisplayText;

    float delaytime;
    public int BombHolderId = -1;

    // Start is called before the first frame update
    void Start()
    {
        delaytime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (active)
        {
            int Num = (int)(PhotonNetwork.Time - StartTime);
            DisplayText.text = ((bomb_timer - Num) + "");
            if (target)
            {
                gameObject.transform.position = target.transform.position + new Vector3(0, 3, 0);
            }
            if (Num >= bomb_timer && PhotonNetwork.IsMasterClient)
            {
                if (target)
                {
                    target.GetComponent<NewPlayerScript>().IsGhost = true;
                    target.GetComponent<NewPlayerScript>().CallKillMe();
                }
                StartTime += bomb_timer;
                FindObjectOfType<NetworkManager>().BombExploded(StartTime);
            }

            if (Time.time - delaytime >= 0.5f)
            {
                foreach (NewPlayerScript item in FindObjectsOfType<NewPlayerScript>())
                {
                    if (BombHolderId == item.GetComponent<PhotonView>().Owner.ActorNumber)
                    {
                        target = item.gameObject;
                    }
                }
                delaytime = Time.time;
            }
        }
    }

    public void settime(double t, int id)
    {
        StartTime = t;
        BombHolderId = id;
        active = true;
    }
}
