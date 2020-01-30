using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class HandScriptNetworked : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /*void OnCollisionEnter(Collision c)
    {
        if (isServer)
        {
            if (c.gameObject.name.Contains("Baby"))
            {
                Debug.Log("HandHit-Collison");
                NetworkedBallGame.nBallGame.HandHit(c.gameObject);
            }
            else if (c.gameObject.name.Contains("Momma"))
            {
                NetworkedBallGame.nBallGame.moveMomma();
            }
        }
    }*/

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
            //GameObject.FindObjectOfType<NetworkedBallGame>().moveMomma();
            //NetworkedBallGame.nBallGame.moveMomma();
        }
    }
    
}
