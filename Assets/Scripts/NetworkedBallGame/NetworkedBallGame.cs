using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;



public enum GameState
{
    Initialize,
    Menu,
    Playing
}

class SyncListGO : SyncList<GameObject> { }
class SyncListUInt : SyncList<uint> { }
class SyncListVector3 : SyncList<Vector3> { }

public class NetworkedBallGame : NetworkBehaviour
{
    public static NetworkedBallGame nBallGame;


    public GameObject menuBaby; //The baby instance used for the menu
    public GameObject babyPrefab; //The balls that the player pulls
    public GameObject highScorePrefab; //The balls spwaned around the menu that display the high score

    public GameObject momma; // the momma ball that the player needs to hit
    public GameObject scoreText; // TextMesh of the score for the momma ball

  
    public GameObject startButton; //The ball used to start the game

    public GameObject platform; //Main game playform
    public Vector3 defaultPlatformVertex; //default vertex value

    [SyncVar]
    private Vector3 targetPlayAreaVertex; //vertex value set from target play area

    [SyncVar(hook = nameof(ChangeActivePlayer))]
    public uint activePlayerId; //netdID of the active player object

    public GameObject activePlayer; //active player object

    [SyncVar]
    private bool aPlayerLocked = false; //bool to lock in the current active player; set to false when the player releases the trigger button.

    public GameObject title; //Game title

    public Material platformMat; //platform material
    public AudioClip blarpClip; //sound effect when hitting mother ball KEEP
    public AudioClip restartClip; //sound effect when losing KEEP
    private GameObject empty; //used as a dummy to start up the game
    
    [SyncVar(hook = nameof(UpdateMommaMesh))]
    public float score = 0; //current score
    [SyncVar]
    public float highScore = 0; //used to sync the client high score ball on connect

    private AudioSource restartSound;
    private AudioSource blarpSound;
    private SteamVR_PlayArea playArea; //VR play area

    private Vector3 v1;
    private Vector3 v2;

    [SyncVar(hook = nameof(SetRoomScale))]
    private Vector3 roomSize;

    [SyncVar(hook = nameof(SetMommaScale))]
    private Vector3 mommaSize;

    private Vector4 mommaInfo;

    public AudioClip[] audioList;
    public AudioClip[] highScoreAudioList;
    public AudioClip[] mommaHitAudioList;
    public List<AudioSource> audioSources = new List<AudioSource>();

    private AudioSource mommaHitSound;

    [SyncVar]
    private bool syncedObejctsSpawned = false;

    private bool syncedObjectsSetUp = false;

    [SyncVar]
    private bool gameSetUp = false;

    private bool triggerPulled;
    private bool shieldTriggerPulled;
    private float notDeadVal;
    

    //Set singleton
    private void Awake()
    {
        if (nBallGame != null)
        {
            if (nBallGame != this)
            {
                Debug.Log("nBG Destroyed");
                GameObject.Destroy(this.gameObject);
                return;
            }
        }
        nBallGame = this;
        GameObject.DontDestroyOnLoad(this.gameObject);
    }

    

    void Start()
    {
        if (!isServer)
        {
            StartSetUp();
        }
    }


    public void StartSetUp()
    {
        if (isServer)
        {
            SaveLoad.Load();

            if (Game.current == null)
            {
                Game.current = new Game();
            }
            highScore = Game.current.highScore;
        }

        //TODO: check if these line cause only one sound to play; and if so and two proper Audio Sources
        //TODO: also, this needs to be done for every client.
        restartSound = gameObject.AddComponent<AudioSource>();
        blarpSound = gameObject.AddComponent<AudioSource>();

        restartSound.clip = restartClip;
        blarpSound.clip = blarpClip;

        empty = new GameObject();

        //TODO: Need to do this for every client
        audioList = new AudioClip[]{ (AudioClip)Resources.Load("Audio/hydra/TipHit1"),
                                (AudioClip)Resources.Load("Audio/hydra/TipHit2"),
                                (AudioClip)Resources.Load("Audio/hydra/TipHit3"),
                                (AudioClip)Resources.Load("Audio/hydra/TipHit4"), };

        highScoreAudioList = new AudioClip[]{  (AudioClip)Resources.Load("Audio/hydra/BaseHit"),};

        mommaHitAudioList = new AudioClip[]{  (AudioClip)Resources.Load("Audio/hydra/ArmStroke1"),
                                          (AudioClip)Resources.Load("Audio/hydra/ArmStroke2"),
                                          (AudioClip)Resources.Load("Audio/hydra/ArmStroke3"),
        };




        mommaHitSound = gameObject.AddComponent<AudioSource>();


        for (var i = 0; i < audioList.Length; i++)
        {
            print(audioList[i]);

            AudioSource audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = audioList[i];
            audioSource.Play();
            audioSource.volume = 0;
            audioSources.Add(audioSource);
        }

        

        //TODO: Call method with stuff that needs to happen only in server
        if (isServer)
        {
            if (!syncedObjectsSetUp)
            {
                SetupSyncedObjects();
                Restart(transform.gameObject);
            }
        }
        else
        {
            ClientSyncObjects();
        }

        gameSetUp = true;
    }

    [Server]
    private void SetupSyncedObjects()
    {

        //SetUp Platform
        GameObject targetCameraRig = GameObject.Find("[CameraRig](Clone)");
        if (targetCameraRig != null)
        {
            targetPlayAreaVertex = targetCameraRig.GetComponent<SteamVR_PlayArea>().vertices[0];
        }
        else
        {
            targetPlayAreaVertex = defaultPlatformVertex;
        }
        platform.transform.localScale = new Vector3(Mathf.Abs(targetPlayAreaVertex.x) * 1.5f, 1.0f, Mathf.Abs(targetPlayAreaVertex.z) * 1.5f);
        platform.GetComponent<MeshRenderer>().material.SetVector("_Size", platform.transform.localScale);
        platform.GetComponent<MeshRenderer>().material.SetFloat("_Learning", 0);

        title.GetComponent<MeshRenderer>().material.SetVector("_Scale", title.transform.localScale);


        //Setup Highscore balls
        float rad = 0;
        Vector3 p = Vector3.zero;
        for (int i = 0; i < 10; i++)
        {
            rad = Random.Range(4, 10);
            p = Random.onUnitSphere * rad;

            GameObject nHSBall = (GameObject)Instantiate(highScorePrefab, p, new Quaternion());
            nHSBall.transform.localScale = new Vector3(rad / 4, rad / 4, rad / 4);
            SetHighScoreBallPitch(nHSBall, Random.Range(0, 1));
            NetworkServer.Spawn(nHSBall);
        }
        BlarpEventManager.SetHighScoreBalls(Game.current.highScore);


        //Setup MenuBaby
        menuBaby.GetComponent<Rigidbody>().isKinematic = false;
        menuBaby.GetComponent<Rigidbody>().drag = .7f - (score / 100);
        menuBaby.GetComponent<Rigidbody>().mass = .2f - (score / 340);
        menuBaby.GetComponent<Rigidbody>().angularDrag = 200;
        menuBaby.transform.localScale = menuBaby.transform.localScale * (2.0f - (score / 30));
        menuBaby.GetComponent<MeshRenderer>().material.SetFloat("_Score", (float)score);

        //SetUp Start Button
        startButton.transform.position = new Vector3(0, 1, -platform.transform.localScale.z * .55f);

        
        //Set active player to first spawn player object
        activePlayer = GameObject.Find("VRBlarpPlayer(Clone)");
        if(activePlayer == null)
        {
            activePlayer = GameObject.Find("NonVRBlarpPlayer(Clone)");
        }
        activePlayerId = activePlayer.GetComponent<NetworkIdentity>().netId;

        GameObject.FindObjectOfType<RoomNetworked>().handL = activePlayer.GetComponent<NetworkedPlayer>().shield;
        GameObject.FindObjectOfType<RoomNetworked>().handR = activePlayer.GetComponent<NetworkedPlayer>().hand;
        syncedObjectsSetUp = true;
    }

    private void ClientSyncObjects()
    {
        //Sync Platform
        platform.transform.localScale = new Vector3(Mathf.Abs(targetPlayAreaVertex.x) * 1.5f, 1.0f, Mathf.Abs(targetPlayAreaVertex.z) * 1.5f);
        platform.GetComponent<MeshRenderer>().material.SetVector("_Size", platform.transform.localScale);
        platform.GetComponent<MeshRenderer>().material.SetFloat("_Learning", 0);

        //Sync Menu Baby
        menuBaby.GetComponent<Rigidbody>().drag = .7f - (score / 100);
        menuBaby.GetComponent<Rigidbody>().mass = .2f - (score / 340);
        menuBaby.GetComponent<Rigidbody>().angularDrag = 200;
        menuBaby.transform.localScale = menuBaby.transform.localScale * (2.0f - (score / 30));
        menuBaby.GetComponent<MeshRenderer>().material.SetFloat("_Score", (float)score);

        //Sync Title
        title.GetComponent<MeshRenderer>().material.SetVector("_Scale", title.transform.localScale);

        BlarpEventManager.EnableHighScoreBalls();
        BlarpEventManager.SetHighScoreBalls(highScore);

        ChangeActivePlayer(activePlayerId);

        syncedObjectsSetUp = true;
    }


    //CONVERTED
    void Update()
    {
        if (isServer)
        {
            if(aPlayerLocked && activePlayer == null)
            {
                UnlockActivePlayer();
            }
        }

        #if UNITY_EDITOR
        UpdateMommaMesh(score);
        #endif

        if (!gameSetUp)
        {
            Debug.Log("Server no set up yet, wait to update.");
            return;
        }

        if(activePlayer != null)
        {
            BlarpEventManager.UpdateBabyRenders(activePlayer, roomSize, mommaInfo);
           
        }
        
    }


    //CONVERTED
    void FixedUpdate()
    {
        if (isServer)
        {
            if (!gameSetUp)
            {
                Debug.Log("Server not set up yet, wait to fixed update");
                return;
            }
            if(activePlayer != null)
            {
                BlarpEventManager.UpdateBabyPhysics(activePlayer, activePlayer.GetComponent<NetworkedPlayer>().GetHandTriggerVal(), activePlayer.GetComponent<NetworkedPlayer>().GetHandVelocity());
            }
        }
    }

    [Server]
    public void SetActivePlayerID(uint playerNetID)
    {
        if(!aPlayerLocked)
        {
            activePlayer.GetComponent<NetworkedPlayer>().isActivePlayer = false;
            activePlayerId = playerNetID;
            ChangeActivePlayer(playerNetID);
            RpcChangeActivePlayer(playerNetID);
        }
    }

    private void ChangeActivePlayer(uint newID)
    {
        foreach(NetworkIdentity netId in GameObject.FindObjectsOfType<NetworkIdentity>())
        {
            if(netId.netId == newID)
            {
                activePlayer = netId.gameObject;
                if (isServer)
                {
                    activePlayer.GetComponent<NetworkedPlayer>().isActivePlayer = true;
                    aPlayerLocked = true;
                    if (!menuBaby.activeSelf)
                    {
                        menuBaby.GetComponent<SpringJoint>().connectedBody = activePlayer.GetComponent<NetworkedPlayer>().GetHand().GetComponent<Rigidbody>();
                    }
                    BlarpEventManager.ChangeBabySpringJoint(activePlayer.GetComponent<NetworkedPlayer>().GetHand().GetComponent<Rigidbody>());
                }
                Debug.Log("Active Player changed to netID " + newID);
                break;
            }
        }
    }

    public void LockActivePlayer()
    {
        aPlayerLocked = true;
    }

    public void UnlockActivePlayer()
    {
       aPlayerLocked = false;
    }

    //CONVERTED
    private void UpdateMommaMesh(float newScore)
    {
        if(momma != null)
        {
            float base100 = Mathf.Floor(score / 100);
            float base10 = Mathf.Floor((score - (base100 * 100)) / 10);
            float base1 = score - (base10 * 10);

            momma.GetComponent<MeshRenderer>().material.SetInt("_Digit1", (int)base1);
            momma.GetComponent<MeshRenderer>().material.SetInt("_Digit2", (int)base10);
        }
    }

    //CONVERTED
    public void MommaHit(GameObject goHit)
    {
        if (!isServer)
        {
            return;
        }
        //Update Score and Move Momma Ball
        score++;
        Game.current.lastScore = score;
        if (score > Game.current.highScore) { Game.current.highScore = score; }

        scoreText.GetComponent<TextMesh>().text = score.ToString();

        ResizeRoom();
        RpcResizeRoom(false);
        MoveMomma();

        // Make a new Baby Ball object
        GameObject go = (GameObject)Instantiate(babyPrefab, new Vector3(), new Quaternion());
        go.transform.position = goHit.transform.position;
        go.GetComponent<SpringJoint>().connectedBody = activePlayer.GetComponent<NetworkedPlayer>().GetHand().GetComponent<Rigidbody>();
       
        go.GetComponent<Rigidbody>().drag = .7f - (score / 100);
        go.GetComponent<Rigidbody>().mass = .2f - (score / 340);
        go.GetComponent<Rigidbody>().angularDrag = 200;
        go.GetComponent<Rigidbody>().freezeRotation = true;
        go.transform.localScale = go.transform.localScale * (2.0f - (score / 30));
        go.GetComponent<MeshRenderer>().material.SetFloat("_Score", (float)score);
        
        AudioSource aS = go.GetComponent<AudioSource>();
        aS.clip = audioList[(int)score % 4];
        aS.pitch = .25f * Mathf.Pow(2, (int)(score / 4));
        aS.volume = Mathf.Pow(.7f, (int)(score / 4));
        aS.Play();
        NetworkServer.Spawn(go);
        go.GetComponent<Rigidbody>().isKinematic = false;

        

        int aLIndex = Random.Range(0, mommaHitAudioList.Length);
        mommaHitSound.clip = mommaHitAudioList[aLIndex];
        mommaHitSound.Play();
        RpcPlayMommaHitAudio(aLIndex);
    }

    //CONVERTED
    float GetSizeFromScore()
    {
        return 3.0f + score / 3;
    }

    //CONVERTED
    public void MoveMomma()
    {

        float size = GetSizeFromScore();



        momma.transform.position = new Vector3(Random.Range(size / 4, size / 2),
                                                Random.Range(0 + .15f, size / 2 + .15f),
                                                Random.Range(size / 4, size / 2));

        float xSign = Random.value < .5 ? 1 : -1;
        float zSign = Random.value < .5 ? 1 : -1;
        Vector3 v = new Vector3(
          xSign,
          1,
          zSign
        );

        momma.transform.position = Vector3.Scale(momma.transform.position, v);

        momma.transform.localScale = new Vector3(.3f, .3f, .3f);
        mommaSize = momma.transform.localScale;

        mommaInfo = new Vector4(
          momma.transform.position.x,
          momma.transform.position.y,
          momma.transform.position.z,
          momma.transform.localScale.x
        );

        momma.GetComponent<AudioSource>().Play();
        RpcPlayMommaAudio();
        RpcMoveMomma(momma.transform.position);

        WaitMomma();
    }

    //CONVERTED
    void WaitMomma()
    {

        momma.SetActive(false);
        RpcEnableMomma(false,0);
        StartCoroutine(WaitToMakeMommaReal());

    }

    //CONVERTED
    IEnumerator WaitToMakeMommaReal()
    {
        while (true)
        {
            yield return new WaitForSeconds(1.0f);
            MakeMommaReal();
            yield return false;

        }
    }

    //CONVERTED
    void MakeMommaReal()
    {
        Debug.Log("Making Momma Real");
        momma.SetActive(true);
        RpcEnableMomma(true,0);
        momma.GetComponent<AudioSource>().Play();
        RpcPlayMommaAudio();
    }

    //CONVERTED
    private void SetMommaScale(Vector3 newScale)
    {
        if(momma != null)
        {
            momma.transform.localScale = newScale;
        }
    }

    //CONVERTED
    void ResizeRoom()
    {
        GameObject room = GameObject.Find("Room");
        if(room != null)
        {
            float size = GetSizeFromScore();

            room.transform.localScale = new Vector3(size, size / 2 + .6f, size);
            room.transform.position = new Vector3(0, size / 4 + .3f, 0);


            roomSize = room.transform.localScale;
        }
        
    }

    //CONVERTED
    void ResizeRoomLarge()
    {
        GameObject room = GameObject.Find("Room");
        if(room != null)
        {
            float size = GetSizeFromScore();

            room.transform.localScale = new Vector3(10000, 10000, 100000);
            room.transform.position = new Vector3(0, 500, 0);


            roomSize = room.transform.localScale;
        }
    }

    //CONVERTED
    private void SetRoomScale(Vector3 newScale)
    {
        GameObject room = GameObject.Find("Room");
        if (room != null)
        {
           room.transform.localScale = newScale;
        }
        
    }

    //TODO: Replace instance of this method call with restart?
    public void HandHit(GameObject handHit)
    {
        Restart(handHit);
    }

    //CONVERTED
    void Restart(GameObject handHit)
    {
        if (isServer)
        {
            SaveLoad.Save();

            foreach (BabyNetworked baby in GameObject.FindObjectsOfType<BabyNetworked>())
            {

                if (!baby.IsMenuBaby)
                {
                    NetworkServer.Destroy(baby.gameObject);
                }
            }
            score = 0;

            blarpSound.Play();
            RpcPlayBlarpSound();

            BlarpEventManager.EnableHighScoreBalls();
            RpcEnalbeHighScoreBalls();

            BlarpEventManager.SetHighScoreBalls(Game.current.highScore);
            RpcSetHighScoreBalls(Game.current.highScore);

            title.GetComponent<MeshRenderer>().enabled = true;
            RpcEnableTitle(true);

            startButton.GetComponent<MeshRenderer>().enabled = true;
            startButton.GetComponent<BoxCollider>().enabled = true;
            RpcEnableStartButton(true);


            momma.GetComponent<MeshRenderer>().enabled = false;
            momma.GetComponent<Collider>().enabled = false;
            RpcEnableMomma(true, 1);

            momma.transform.position = new Vector3(0, -1000000.0f, 0);

            GameObject.Find("Room").GetComponent<RoomNetworked>().active = false;
            RpcSetRoomActive(false);
            ResizeRoomLarge();
            RpcResizeRoom(true);

            menuBaby.transform.position = new Vector3(0, 1, 2);
            menuBaby.GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
            menuBaby.SetActive(true);
            RpcSetMenuBabyActive(true);

            //TODO: Look into method to deny new client connections during a game in progress
            //GameObject.Find("NetworkManager").GetComponent<BlarpNetworkManager>().AccepNewConnections = true;
        }

    }

    //CONVERTED
    public void StartGame(GameObject go)
    {
        if (isServer)
        {
            restartSound.Play();
            RpcPlayRestartAudio();
            BlarpEventManager.DisableHighScoreBalls();
            RpcDisableHighScoreBalls();


            ResizeRoom();
            RpcResizeRoom(false);
            GameObject.Find("Room").GetComponent<RoomNetworked>().active = true;
            RpcSetRoomActive(true);


            int hitSoundIndex = Random.Range(0, mommaHitAudioList.Length);
            mommaHitSound.clip = mommaHitAudioList[hitSoundIndex];
            mommaHitSound.Play();
            RpcPlayMommaHitAudio(hitSoundIndex);


            menuBaby.SetActive(false);
            RpcSetMenuBabyActive(false);
            menuBaby.transform.position = new Vector3(0, 1, 2);
            menuBaby.GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);

            startButton.GetComponent<MeshRenderer>().enabled = false;
            startButton.GetComponent<BoxCollider>().enabled = false;
            RpcEnableStartButton(false);


            momma.GetComponent<MeshRenderer>().enabled = true;
            RpcEnableMomma(true, 1);

            title.GetComponent<MeshRenderer>().enabled = false;
            RpcEnableTitle(false);


            float size = GetSizeFromScore();
            empty.transform.position = new Vector3(Random.Range(-size / 4, size / 4),
                                                    Random.Range(size / 4 + .15f, size / 4 + .15f),
                                                    Random.Range(-size / 4, size / 4));

            empty.transform.position = new Vector3(0,1,-size / 2);
            MommaHit(empty);

            //TODO: Look into method to deny new client connections during a game in progress
            //GameObject.Find("NetworkManager").GetComponent<BlarpNetworkManager>().AccepNewConnections = false;
        }
    }
    

    private void PlayNewBabyAudio(GameObject newBaby)
    {
        AudioSource audioSource = newBaby.GetComponent<AudioSource>();
        audioSource.clip = audioList[(int)score % 4];
        audioSource.pitch = .25f * Mathf.Pow(2, (int)(score / 4));
        audioSource.volume = Mathf.Pow(.7f, (int)(score / 4));
        audioSource.Play();
    }

    private void SetHighScoreBallPitch(GameObject hsBall, int index)
    {
        hsBall.GetComponent<AudioSource>().clip = highScoreAudioList[index];
        hsBall.GetComponent<AudioSource>().pitch = .5f;
    }

    [ClientRpc]
    void RpcPlayMommaHitAudio(int index)
    {
        if (!isServer)
        {
            mommaHitSound.clip = mommaHitAudioList[index];
            mommaHitSound.Play();
        }
    }

    [ClientRpc]
    void RpcPlayMommaAudio()
    {
        if (!isServer)
        {
            momma.GetComponent<AudioSource>().Play();
        }
    }

    [ClientRpc]
    void RpcEnableMomma(bool enable,int enableMode)
    {
        if(enableMode == 1)
        {
            momma.GetComponent<MeshRenderer>().enabled = enable;
            momma.GetComponent<Collider>().enabled = enable;
        }
        else
        {
            momma.SetActive(enable);
        }
    }

    [ClientRpc]
    void RpcMoveMomma(Vector3 p)
    {
        momma.transform.position = p;
    }

    [ClientRpc]
    void RpcSetMenuBabyActive(bool active)
    {
        menuBaby.SetActive(active);
    }

    [ClientRpc]
    void RpcSetHighScoreBalls(float newScore)
    {
        BlarpEventManager.SetHighScoreBalls(newScore);
    }

    [ClientRpc]
    void RpcDisableHighScoreBalls()
    {
        BlarpEventManager.DisableHighScoreBalls();
    }

    [ClientRpc]
    void RpcEnalbeHighScoreBalls()
    {
        BlarpEventManager.EnableHighScoreBalls();
    }

    [ClientRpc]
    void RpcPlayRestartAudio()
    {
        restartSound.Play();
    }

    [ClientRpc]
    void RpcSetRoomActive(bool enable)
    {
        GameObject.Find("Room").GetComponent<RoomNetworked>().active = enable;
    }

    [ClientRpc]
    void RpcEnableStartButton(bool enable)
    {
        startButton.GetComponent<MeshRenderer>().enabled = enable;
        startButton.GetComponent<BoxCollider>().enabled = enable;
    }

    [ClientRpc]
    void RpcEnableTitle(bool enable)
    {
        title.GetComponent<MeshRenderer>().enabled = enable;
    }

    [ClientRpc]
    void RpcPlayBlarpSound()
    {
        blarpSound.Play();
    }

    [ClientRpc]
    void RpcResizeRoom(bool resizeLarge)
    {
        if (!resizeLarge)
        {
            ResizeRoom();
        }
        else
        {
            ResizeRoomLarge();
        }
        
    }

    [ClientRpc]
    void RpcChangeActivePlayer(uint nID)
    {
        if (!isServer)
        {
            ChangeActivePlayer(nID);
        }
    }
}
