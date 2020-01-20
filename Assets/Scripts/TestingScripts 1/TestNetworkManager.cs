using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class TestNetworkManager : NetworkManager
{
    public GameObject testDriverPrefab;

    public override void OnStartServer()
    {
        base.OnStartServer();

        

        GameObject tDriver = (GameObject)Instantiate(testDriverPrefab, Vector3.zero, Quaternion.identity);
        NetworkServer.Spawn(tDriver);
    }
}
