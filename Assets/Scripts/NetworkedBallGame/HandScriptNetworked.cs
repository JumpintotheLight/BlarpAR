using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class HandScriptNetworked : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("HandTriggerEntered");
       if (other.gameObject.name.Contains("Baby"))
        {
            Debug.Log("HandHit-Collison");
            transform.parent.gameObject.GetComponent<NetworkedPlayer>().HandHit(other.gameObject);
            //NetworkedBallGame.nBallGame.HandHit(other.gameObject);
        }
        else if (other.gameObject.name.Contains("Momma"))
        {
            transform.parent.gameObject.GetComponent<NetworkedPlayer>().MoveMomma();
        }
    }
    
}
