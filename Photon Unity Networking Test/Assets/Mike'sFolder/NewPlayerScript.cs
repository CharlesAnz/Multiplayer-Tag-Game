using System.Collections;
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
    public GameObject MyCam;
    private BombCollisionTrigger bombCollisionTrigger;

    public bool HasBeenSetup = false;
    float StartTime = 0;

    public bool HasBomb = false, CanGiveBomb = false, IsGhost = false;

    public void Awake()
    {
        /* Grab the following components */
        photonView = GetComponent<PhotonView>();
        rigidbody = GetComponent<Rigidbody>();
        collider = GetComponent<Collider>();
        PlayerDeath = GetComponent<ParticleSystem>();

        bombCollisionTrigger = GetComponentInChildren<BombCollisionTrigger>();

        StartTime = Time.time;
    }

    public void Update()
    {
        if (!photonView.IsMine || !controllable)
        {
            return;
        }

        if (Time.time - StartTime >= 10) HasBeenSetup = true;

        if (MyCam) MyCam.transform.position = gameObject.transform.position + new Vector3(0, 10, -8);

        if(photonView.IsMine)
        {
            float forward = Input.GetAxis("Vertical");
            float turn = Input.GetAxis("Horizontal");
            transform.Translate(new Vector3(0, 0, forward * MovementSpeed * Time.deltaTime));
            transform.Rotate(new Vector3(0, turn * turnSpeed * Time.deltaTime, 0));
            if (forward != 0) GetComponent<Animator>().SetBool("IsRunning", true);
            else GetComponent<Animator>().SetBool("IsRunning", false);
            //Debug.Log("you should be moving!");

            // TEST DEATH CODE
            if (Input.GetKeyDown(KeyCode.T))
            {
                photonView.RPC("KillPlayer", RpcTarget.AllViaServer);
            }

            if (photonView.IsMine && IsGhost == true)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    StartCoroutine(CheerTimer());
                }
            }

            if (bombCollisionTrigger.collidedWithPlayer)
            {
                Collided(bombCollisionTrigger.otherPlayer);
            }


        }
    }

   
    public void Collided(GameObject otherPlayer)
    {
        if (HasBeenSetup && photonView.IsMine)
        {
            print("Collided!" + HasBomb);
            //photonView.RPC("KillPlayer", RpcTarget.AllViaServer);
            int TargetId = otherPlayer.gameObject.GetComponent<PhotonView>().Owner.ActorNumber;
            if (HasBomb && CanGiveBomb && !IsGhost)
            {
                HasBomb = false;
                CanGiveBomb = false;
                FindObjectOfType<NetworkManager>().Collided(TargetId);
            }
        }

        bombCollisionTrigger.otherPlayer = null;
        bombCollisionTrigger.collidedWithPlayer = false;
    }


    public void CallKillMe()
    {
        photonView.RPC("KillPlayer", RpcTarget.AllViaServer);
    }

    [PunRPC]
    public void KillPlayer()
    {
        IsGhost = true;

        /* Disable relevant player elements */
        rigidbody.velocity = Vector3.zero;
        rigidbody.constraints = RigidbodyConstraints.FreezePosition;
        collider.enabled = false;
        //renderer.enabled = false;
        foreach (Transform transform in gameObject.transform)
        {
            if (transform.gameObject.name != "Body" && transform.GetComponent<SkinnedMeshRenderer>())
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
        //rigidbody.constraints = RigidbodyConstraints.None;

        foreach (Transform transform in gameObject.transform)
        {
            if (transform.gameObject.name != "Body")
            {
                transform.GetComponent<SkinnedMeshRenderer>().enabled = true;
            }
        }

        /* make it controllable again */
        controllable = true;
        //collider.enabled = true; //temp
        

        /* Stop the particle system */
        PlayerDeath.Stop();
    }

    private IEnumerator CheerTimer()
    {
        // INPUT CHEER LOGIC HERE
        rigidbody.velocity = Vector3.zero;
        controllable = false;
        //GetComponent<Animator>().SetTrigger("CheerTrig");
        photonView.RPC("Cheer", RpcTarget.AllViaServer);
        yield return new WaitForSeconds(MultiplayerGameManager.cheer_timer);
        controllable = true;
    }

    public IEnumerator GiveBomb()
    {
        HasBomb = true;
        CanGiveBomb = false;
        yield return new WaitForSeconds(2);
        CanGiveBomb = true;
        //HasBomb = true;
    }

    private IEnumerator WaitToBecomeGhost()
    {
        /* get timer from MultiplayerGameManager */
        yield return new WaitForSeconds(MultiplayerGameManager.time_to_become_ghost);

        /* Make it possible for all remote clients to become ghosts on server */
        photonView.RPC("BecomeGhost", RpcTarget.AllViaServer);
    }

    [PunRPC]
    void Cheer()
    {
        GetComponent<Animator>().SetTrigger("CheerTrig");
    }
}
