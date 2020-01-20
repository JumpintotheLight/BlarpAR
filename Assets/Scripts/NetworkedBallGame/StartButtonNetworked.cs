using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class StartButtonNetworked : NetworkBehaviour
{
    void OnTriggerEnter(Collider c)
    {

        print(c.gameObject.tag);
        if (!c.gameObject.name.Contains("hand") && !c.gameObject.name.Contains("Shield") && isServer)
        {
            NetworkedBallGame.nBallGame.startGame(transform.gameObject);
        }

    }
}
