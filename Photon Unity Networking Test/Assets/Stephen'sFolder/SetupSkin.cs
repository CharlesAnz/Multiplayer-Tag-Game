using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class SetupSkin : MonoBehaviourPunCallbacks
{
    public Material[] MatPrefabs;
    public Material GhostMat;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetMat(int id)
    {
        photonView.RPC("SetMaterial", RpcTarget.AllBuffered, id);
    }

    public void TurnGhost()
    {
        foreach (Transform transform in gameObject.transform)
        {
            if (transform.gameObject.name != "Body" && transform.GetComponent<SkinnedMeshRenderer>())
            {
                transform.GetComponent<SkinnedMeshRenderer>().material = GhostMat;
            }
        }
    }

    [PunRPC]
    void SetMaterial(int id)
    {
        foreach (Transform transform in gameObject.transform)
        {
            if (transform.gameObject.name != "Body" && transform.GetComponent<SkinnedMeshRenderer>())
            {
                transform.GetComponent<SkinnedMeshRenderer>().material = MatPrefabs[id];
            }
        }
    }
}
