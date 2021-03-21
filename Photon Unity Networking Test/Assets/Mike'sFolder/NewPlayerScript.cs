using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class NewPlayerScript : MonoBehaviour
{
    /* Player variables */
    public float RotationSpeed = 90.0f;
    public float MovementSpeed = 2.0f;
    public float MaxSpeed = 0.2f;
    private float rotation = 0.0f;
    private float acceleration = 0.0f;
    private bool controllable = true;

    /* PhotonView component */
    private PhotonView photonView;

    /* Player components */
    private new Rigidbody rigidbody;
    private new Collider collider;
    private new Renderer renderer;
    Animator animator;
    public ParticleSystem PlayerDeath;

    public void Awake()
    {
        /* Grab the following components */
        photonView = GetComponent<PhotonView>();
        rigidbody = GetComponent<Rigidbody>();
        collider = GetComponent<Collider>();
        renderer = GetComponent<Renderer>();
    }

    public void Update()
    {
        if (!photonView.IsMine || !controllable)
        {
            return;
        }

        rotation = Input.GetAxis("Horizontal");
        acceleration = Input.GetAxis("Vertical");

        // TEST DEATH CODE
        if(photonView.IsMine)
        {
            if(Input.GetKeyDown(KeyCode.T))
            {
                KillPlayer();
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

    [PunRPC]
    public void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            if(photonView.IsMine)
            {
                KillPlayer();
            }
        }
    }

    [PunRPC]
    public void KillPlayer()
    {
        /* Disable relevant player elements */
        rigidbody.velocity = Vector3.zero;
        collider.enabled = false;
        renderer.enabled = false;
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
        /* Reenable renderer and set opacity to 50% */
        renderer.enabled = true;
        //renderer.material.color.a = 0.5;

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
}
