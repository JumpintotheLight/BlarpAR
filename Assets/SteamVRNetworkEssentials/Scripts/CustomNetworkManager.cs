using UnityEngine;
using System.Collections;
//
using Mirror;
using System.IO;
using UnityEngine.XR;

public class CustomNetworkManager : NetworkManager {
	public bool ShouldBeServer;

	public GameObject vrPlayerPrefab;
	private int playerCount = 0;

    //Override OnStartServer to add handler for Player Message
    public override void OnStartServer()
    {
        base.OnStartServer();

        NetworkServer.RegisterHandler<CreateVrPlayerMessage>(OnCreatePlayer);
    }

    //Override OnClientConnect to detect if the player is using VR
    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);

        CreateVrPlayerMessage newPlayerMessage = new CreateVrPlayerMessage
        {
            isVrPlayer = UnityEngine.XR.XRDevice.isPresent
        };

        conn.Send(newPlayerMessage);

        //SpawnMessage extraMessage = new SpawnMessage ();
        //extraMessage.isVrPlayer = UnityEngine.XR.XRSettings.enabled;

        // NetworkConnection readyConn, byte[] extraData
        //ClientScene.AddPlayer (client.connection, 0, extraMessage);
        //ClientScene.AddPlayer(conn, null);
    }

    //Custom method for setting up Vr and Non-Vr players
    void OnCreatePlayer(NetworkConnection conn, CreateVrPlayerMessage message)
    {
        GameObject emptyGO = new GameObject();
        Transform newTransform = emptyGO.transform;
        Transform spawnPoint = newTransform;

        GameObject newPlayer;
        if (message.isVrPlayer)
        {
            newPlayer = (GameObject)Instantiate(this.vrPlayerPrefab, spawnPoint.position, spawnPoint.rotation);
        }
        else
        {
            newPlayer = (GameObject)Instantiate(this.playerPrefab, spawnPoint.position, spawnPoint.rotation);
        }
        NetworkServer.AddPlayerForConnection(conn, newPlayer);
        playerCount++;
    }

    //public override void OnServerAddPlayer (NetworkConnection conn, short playerControllerId, NetworkReader extraMessageReader) AddPlayerMessage extraMessageReader
    /*public override void OnServerAddPlayer(NetworkConnection conn, AddPlayerMessage extraMessageReader)

    {
        //      SpawnMessage message = new SpawnMessage ();
        //message.Deserialize (extraMessageReader);

        //bool isVrPlayer = message.isVrPlayer;
        //bool isVrPlayer = UnityEngine.XR.XRDevice.isPresent;

        //short playerControllerId = (short) conn.connectionId;

        //Transform spawnPoint = this.startPositions [playerCount];
        GameObject emptyGO = new GameObject();
        Transform newTransform = emptyGO.transform;
        Transform spawnPoint = newTransform;

		GameObject newPlayer;
		if (isVrPlayer) {
			newPlayer = (GameObject)Instantiate (this.vrPlayerPrefab, spawnPoint.position, spawnPoint.rotation);
		} else {
			newPlayer = (GameObject)Instantiate (this.playerPrefab, spawnPoint.position, spawnPoint.rotation);
		}
		NetworkServer.AddPlayerForConnection (conn, newPlayer);
		playerCount++;
	}*/

    // OnServerRemovePlayer(NetworkConnection conn, NetworkIdentity player)
    public override void OnServerRemovePlayer (NetworkConnection conn, NetworkIdentity player)
	{
		base.OnServerRemovePlayer (conn, player);
		playerCount--;
	}

	void Start () {
		//var settingsPath = Application.dataPath + "/settings.cfg";
  //      if (File.Exists (settingsPath)) {
		//	StreamReader textReader = new StreamReader (settingsPath, System.Text.Encoding.ASCII);
		//	ShouldBeServer = textReader.ReadLine () == "Server";
  //          networkAddress = textReader.ReadLine();
  //          textReader.Close ();
		//}
		
		//Debug.Log ("Starting Network");
		//if (ShouldBeServer)
		//{
		//	StartHost ();
		//}
		//else
		//{
		//	StartClient ();
		//}
	}

	
}

//MessageBase class for setting player type (Vr/Non-Vr)
public class CreateVrPlayerMessage: MessageBase
{
    public bool isVrPlayer;
}


