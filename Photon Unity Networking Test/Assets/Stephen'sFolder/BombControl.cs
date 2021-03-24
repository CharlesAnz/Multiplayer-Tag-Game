using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class BombControl : MonoBehaviourPun
{
    public float StartTime = 0f;
    bool active = false;
    GameObject target;
    public Text DisplayText;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (active && PhotonNetwork.IsMasterClient)
        {
            //transform.position = new Vector3(0, 0, -10 + (Time.time - StartTime));
            transform.position = target.transform.position + new Vector3(0, 3, 0);
            photonView.RPC("SetTime", RpcTarget.AllViaServer, Time.time - StartTime);
        }
    }

    [PunRPC]
    public void SetTime(float TimeLeft)
    {
        DisplayText.text = TimeLeft + "";
    }

    public void SetupBomb(float t, GameObject p)
    {
        StartTime = t;
        target = p;
        active = true;
    }

    public void Collided(GameObject op)
    {
        target = op;
    }
}
