﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class NewPlayerScript : MonoBehaviour
{
    /* Player variables */
    public float MovementSpeed = 2;
    public float turnSpeed = 180;
    public bool controllable = true;

    /* PhotonView component */
    private PhotonView photonView;

    /* Player components */
    private new Rigidbody rigidbody;
    private new Collider collider;
    Animator animator;
    public ParticleSystem PlayerDeath;

    public bool HasBeenSetup = false;
    float SetupTime = 0, StartTime = 0;

    public bool HasBomb = false;
    GameObject LastHit;
    float LastHitTime = 0f;

    public void Awake()
    {
        /* Grab the following components */
        photonView = GetComponent<PhotonView>();
        rigidbody = GetComponent<Rigidbody>();
        collider = GetComponent<Collider>();
        PlayerDeath = GetComponent<ParticleSystem>();

        StartTime = Time.time;
        //gameObject.tag = photonView.Owner.ActorNumber + "";
    }

    public void Update()
    {
        if (!photonView.IsMine || !controllable)
        {
            return;
        }

        if (Time.time - StartTime >= 10) HasBeenSetup = true;
        if (Time.time - LastHitTime >= 5) LastHit = null;

        if(photonView.IsMine)
        {
            float forward = Input.GetAxis("Vertical");
            float turn = Input.GetAxis("Horizontal");
            transform.Translate(new Vector3(0, 0, forward * MovementSpeed * Time.deltaTime));
            transform.Rotate(new Vector3(0, turn * turnSpeed * Time.deltaTime, 0));
            //Debug.Log("you should be moving!");
            // TEST DEATH CODE
            if (Input.GetKeyDown(KeyCode.T))
            {
                photonView.RPC("KillPlayer", RpcTarget.AllViaServer);
            }
        }
    }

    private IEnumerator WaitToBecomeGhost()
    {
        /* get timer from MultiplayerGameManager */
        yield return new WaitForSeconds(MultiplayerGameManager.time_to_become_ghost);

        /* Make it possible for all remote clients to become ghosts on server */
        photonView.RPC("BecomeGhost", RpcTarget.AllViaServer);
    }

    private IEnumerator Cheer()
    {
        // INPUT CHEER LOGIC HERE
        rigidbody.velocity = Vector3.zero;

        yield return new WaitForSeconds(MultiplayerGameManager.cheer_timer);
    }

    public void OnCollisionEnter(Collision otherPlayer)
    {
        if (HasBeenSetup && photonView.IsMine)
        {
            if (otherPlayer.gameObject.CompareTag("Player") && Vector3.Distance(FindObjectOfType<BombControl>().gameObject.transform.position, gameObject.transform.position) <= 10)
            {
                LastHit = otherPlayer.gameObject;
                //print("Collided!");
                //photonView.RPC("KillPlayer", RpcTarget.AllViaServer);
                if (HasBomb)
                {
                    photonView.RPC("Collided", RpcTarget.MasterClient, otherPlayer.gameObject);
                    photonView.RPC("SetHasBomb", RpcTarget.AllViaServer);
                    HasBomb = false;
                }
            }
        }
    }

    [PunRPC]
    public void KillPlayer()
    {
        /* Disable relevant player elements */
        rigidbody.velocity = Vector3.zero;
        rigidbody.constraints = RigidbodyConstraints.FreezePosition;
        collider.enabled = false;
        //renderer.enabled = false;
        foreach (Transform transform in gameObject.transform)
        {
            if (transform.gameObject.name != "Body")
            {
                transform.GetComponent<SkinnedMeshRenderer>().enabled = false;
            }
        }
        controllable = false;

        /* Play the particle system effect */
        PlayerDeath.Play();

        if(photonView.IsMine)
        {
            /* Start up Coroutine to become a ghost */
            StartCoroutine("WaitToBecomeGhost");
        }
    }

    [PunRPC]
    public void BecomeGhost()
    {
        foreach (Transform transform in gameObject.transform)
        {
            if (transform.gameObject.name != "Body")
            {
                transform.GetComponent<SkinnedMeshRenderer>().enabled = true;
            }
        }

        /* make it controllable again */
        controllable = true;

        /* Stop the particle system */
        PlayerDeath.Stop();

        if (photonView.IsMine)
        {
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                StartCoroutine("Cheer");
            }
        }
    }

    [PunRPC]
    public void Collided(GameObject op)
    {
        FindObjectOfType<BombControl>().Collided(op);
    }

    [PunRPC]
    public void SetHasBomb()
    {
        //Debug.Log(ID + ":" + photonView.Owner.UserId);
        //print(ID + ":" + photonView.Owner.UserId);
        //if (photonView.Owner.UserId == ID)
        
        if (LastHit && !HasBomb)
        {
            HasBomb = true;
            print("SetBomb");
        }
        
    }
}
