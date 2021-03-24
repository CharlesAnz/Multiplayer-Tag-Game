using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BombControl : MonoBehaviourPun
{
    public float StartTime = 0f;
    bool active = false;
    GameObject target;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (active)
        {
            transform.position = new Vector3(20, 0, 20 + (Time.time - StartTime));
        }
    }

    [PunRPC]
    public void SetupBomb(float t)
    {
        StartTime = t;
        active = true;
    }
}
