using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombCollisionTrigger : MonoBehaviour
{
    public bool collidedWithPlayer;
    public GameObject otherPlayer;

    private void Start()
    {
        collidedWithPlayer = false;
        otherPlayer = null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && other.gameObject.GetComponent<NewPlayerScript>())
        {
            collidedWithPlayer = true;
            otherPlayer = other.gameObject;
        }

    }
}