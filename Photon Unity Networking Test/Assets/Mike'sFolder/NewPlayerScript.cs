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

        // only allow jumping if we are running.
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Run") && Input.GetButtonDown("Fire2"))
            animator.SetTrigger("Jump");

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        v = Mathf.Max(v, 0);    // prevent negative speed ("S" key)

        animator.SetFloat("Speed", h * h + v * v);
        animator.SetFloat("Direction", h, 0.25f, Time.deltaTime);

    }

    private IEnumerator WaitToBecomeGhost()
    {
        /* get timer from MultiplayerGameManager */
        yield return new WaitForSeconds(MultiplayerGameManager.time_to_become_ghost);

        /* Make it possible for all remote clients to become ghosts on server */
        photonView.RPC("BecomeGhost", RpcTarget.AllViaServer);
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
        renderer.enabled = true;
        controllable = true;
        PlayerDeath.Stop();
    }
}
