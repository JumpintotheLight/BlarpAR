using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class BlarpNetworkManager : NetworkManager
{
    public GameObject vrPlayerPrefab;

    public Transform playerSpawn;
    public GameObject gameDriverPrefab;

    public override void OnStartServer()
    {
        base.OnStartServer();

        NetworkServer.RegisterHandler<CreateVrBlarpPlayerMessage>(OnCreatePlayer);

        //GameObject gDriver = (GameObject)Instantiate(spawnPrefabs.Find(prefab => prefab.name == gameDriverPrefab.name), Vector3.zero, Quaternion.identity);
        //NetworkServer.Spawn(gDriver);
    }

    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);



        CreateVrBlarpPlayerMessage newPlayerMessage = new CreateVrBlarpPlayerMessage
        {
            isVrPlayer = UnityEngine.XR.XRDevice.isPresent
        };

        conn.Send(newPlayerMessage);
    }

    void OnCreatePlayer(NetworkConnection conn, CreateVrBlarpPlayerMessage message)
    {
        //Set corret start position
        Transform start = playerSpawn;

        GameObject newPlayer;
        if (message.isVrPlayer)
        {
            newPlayer = (GameObject)Instantiate(this.vrPlayerPrefab, start.position, start.rotation);
        }
        else
        {
            newPlayer = (GameObject)Instantiate(this.playerPrefab, new Vector3(start.position.x, start.position.y + 1, start.position.z), start.rotation);
        }
        NetworkServer.AddPlayerForConnection(conn, newPlayer);

    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        //TODO: call to PlayerDisconnected() on nBallGame

        // call base functionality (actually destroys the player)
        base.OnServerDisconnect(conn);
    }

    public override void OnStopServer()
    {
        base.OnStopServer();

        GameObject.Destroy(NetworkedBallGame.nBallGame.gameObject);
    }
}

public class CreateVrBlarpPlayerMessage : MessageBase
{
    public bool isVrPlayer;
}
