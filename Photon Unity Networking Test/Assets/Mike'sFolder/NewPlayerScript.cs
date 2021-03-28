using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TrailsFX;

public class NewPlayerScript : MonoBehaviour
{
    /* Player variables */
    public float MovementSpeed = 2;
    public float turnSpeed = 180;
    public bool controllable = true;
    //public bool movementSpeedBuff = false;

    /* PhotonView component */
    private PhotonView photonView;

    /* Player components */
    private new Rigidbody rigidbody;
    private new Collider collider;
    Animator animator;
    public ParticleSystem PlayerDeath;
    public GameObject MyCam;
    private CheerBoostTrigger cheerBoost;

    public bool HasBeenSetup = false;
    float StartTime = 0;

    float speedBoostStartTime = 0f;

    public bool HasBomb = false, CanGiveBomb = false, IsGhost = false, SpeedFx = false;

    public void Awake()
    {
        /* Grab the following components */
        photonView = GetComponent<PhotonView>();
        rigidbody = GetComponent<Rigidbody>();
        collider = GetComponent<Collider>();
        PlayerDeath = GetComponent<ParticleSystem>();

        cheerBoost = GetComponentInChildren<CheerBoostTrigger>();

        StartTime = Time.time;
    }

    [PunRPC]
    public void Update()
    {
        if (!photonView.IsMine || !controllable)
        {
            return;
        }

        if (Time.time - StartTime >= 10) HasBeenSetup = true;

        if (Time.time - speedBoostStartTime <= 10)
        {
            MovementSpeed = 5;
        }
        else
        {
            MovementSpeed = 2;
        }

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

            if (SpeedFx == true)
            {
                var speedBoostScript = GetComponentInChildren<TrailsFX.TrailEffect>();
                speedBoostScript.enabled = true;
            }
            else
            {
                var speedBoostScript = GetComponentInChildren<TrailsFX.TrailEffect>();
                speedBoostScript.enabled = false;
            }

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
        }
    }


    public void OnCollisionEnter(Collision otherPlayer)
    {
        if (otherPlayer.gameObject.CompareTag("Player") && otherPlayer.gameObject.GetComponent<NewPlayerScript>())
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
        }
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
        //collider.enabled = false;
        //renderer.enabled = false;
        foreach (NewPlayerScript item in FindObjectsOfType<NewPlayerScript>())
        {
            if (this != item)
            {
                Physics.IgnoreCollision(gameObject.GetComponent<BoxCollider>(), item.gameObject.GetComponent<BoxCollider>());
            }
        }

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
            if (transform.gameObject.name != "Body" && transform.GetComponent<SkinnedMeshRenderer>())
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
        rigidbody.velocity = Vector3.zero;
        controllable = false;
        //GetComponent<Animator>().SetTrigger("CheerTrig");
        photonView.RPC("Cheer", RpcTarget.AllViaServer);
        yield return new WaitForSeconds(MultiplayerGameManager.cheer_timer);
        controllable = true;
        SpeedFx = false;
    }

    public IEnumerator GiveBomb()
    {
        CanGiveBomb = false;
        HasBomb = true;
        yield return new WaitForSeconds(3.0f);
        CanGiveBomb = true;
        print("Should be able to pass bomb now");
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

        if (cheerBoost.playerNearby)
        {
            cheerBoost.otherPlayer.GetComponent<NewPlayerScript>().speedBoostStartTime = Time.time;
            cheerBoost.otherPlayer.GetComponent<NewPlayerScript>().SpeedFx = true;
        }
    }
}
