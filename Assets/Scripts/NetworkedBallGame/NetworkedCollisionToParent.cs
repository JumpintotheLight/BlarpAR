using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkedCollisionToParent : NetworkBehaviour
{
    public GameObject parent;
    // Use this for initialization
    void Start()
    {

    }

    void OnCollisionEnter(Collision c)
    {
        if (c.gameObject.name.Contains("Baby"))
        {
            parent.GetComponent<RoomNetworked>().BabyHit(c);
        }
    }
}
