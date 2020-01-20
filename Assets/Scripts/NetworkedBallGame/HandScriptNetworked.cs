using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class HandScriptNetworked : NetworkBehaviour
{
    [SyncVar]
    private float triggerVal;
    [SyncVar]
    private Vector3 velocity;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        if (isServer)
        {
            velocity = this.GetComponent<Rigidbody>().velocity;
        }
    }

    public float TriggerVal
    {
        get { return triggerVal; }
        set { triggerVal = value; }
    }

    public Vector3 Velocity
    {
        get { return velocity; }
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
        if (isServer)
        {
            
            if (other.gameObject.name.Contains("Baby"))
            {
                Debug.Log("HandHit-Collison");
                GameObject.FindObjectOfType<NetworkedBallGame>().HandHit(other.gameObject);
                //NetworkedBallGame.nBallGame.HandHit(other.gameObject);
            }
            else if (other.gameObject.name.Contains("Momma"))
            {
                GameObject.FindObjectOfType<NetworkedBallGame>().moveMomma();
                //NetworkedBallGame.nBallGame.moveMomma();
            }
        }
    }
}
