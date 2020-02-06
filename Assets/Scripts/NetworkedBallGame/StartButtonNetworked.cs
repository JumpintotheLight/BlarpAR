using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class StartButtonNetworked : MonoBehaviour
{
    void OnTriggerEnter(Collider c)
    {

        print(c.gameObject.tag);
        if (!c.gameObject.name.Contains("hand") && !c.gameObject.name.Contains("Shield"))
        {
            GameObject.FindObjectOfType<NetworkedBallGame>().StartGame(transform.gameObject);
        }

    }
}
