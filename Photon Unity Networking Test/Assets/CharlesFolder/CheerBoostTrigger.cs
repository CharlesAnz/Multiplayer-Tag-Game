using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheerBoostTrigger : MonoBehaviour
{
    public bool playerNearby;
    public GameObject otherPlayer;

    private void Start()
    {
        playerNearby = false;
        otherPlayer = null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && other.gameObject.GetComponent<NewPlayerScript>())
        {
            playerNearby = true;
            otherPlayer = other.gameObject;
        }

    }

    private void OnTriggerExit(Collider other)
    {
        NewPlayerScript exitingPlayer = other.gameObject.GetComponent<NewPlayerScript>();

        if (other.gameObject.CompareTag("Player") && exitingPlayer != null)
        {
            playerNearby = false;
            otherPlayer = null;
        }
    }

}
