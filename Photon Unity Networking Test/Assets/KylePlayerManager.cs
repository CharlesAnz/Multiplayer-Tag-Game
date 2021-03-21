// --------------------------------------------------------------------------------------------------------------------
// Roughly based on Photon Unity Networking Demos
// Adaped for Kingston University students
// --------------------------------------------------------------------------------------------------------------------

using UnityEngine;
using Photon.Pun;

public class KylePlayerManager : MonoBehaviourPun
{
    Animator animator;

	void Start () 
	{
	    animator = GetComponent<Animator>();
	}
	        
	void Update () 
	{
        if (photonView.IsMine)
        {
            // only allow jumping if we are running.
            if (animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Run") && Input.GetButtonDown("Fire2"))
                animator.SetTrigger("Jump");

            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");

            v = Mathf.Max(v, 0);    // prevent negative speed ("S" key)

            animator.SetFloat("Speed", h * h + v * v);
            animator.SetFloat("Direction", h, 0.25f, Time.deltaTime);
        }
	}
}
