using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
/* IMPORTANT NOTE
 * Tutorial Functions have currently been disabled in the networked version of this game. 
 * Code from the standalone version is still present if there is time to covert that function to the networkd project.
 */


public enum GameState
{
    Initialize,
    Menu,
    Playing
}

class SyncListGO : SyncList<GameObject> { }
class SyncListUInt : SyncList<uint> { }

public class NetworkedBallGame : NetworkBehaviour
{
    public static NetworkedBallGame nBallGame;

    readonly SyncListUInt BabyIDs = new SyncListUInt();
    //readonly SyncListGO Babies = new SyncListGO(); //List of all babies in scene
    List<GameObject> Babies = new List<GameObject>();

    [SyncVar]
    public uint menuBabyID; 

    public GameObject menuBaby; //The baby instance used for the menu
    public GameObject BabyPrefab; //The balls that the player pulls KEEP As is; set in inspector
    public GameObject MommaPrefab; //The ball that the player needs to ram the others into KEEP As is; set in inspector
    public GameObject HighScorePrefab; //Version of momma ball that displays the highscore KEEP As is; set in inspector

    [SyncVar]
    public uint mommaID;

    public GameObject Momma; // the current, instantiated momma Momma ball (check if it is destroyed or just teleported) KEEP
    public GameObject ScoreText; // TextMesh of the score for the momma ball

    [SyncVar]
    public uint startButtonId;

    public GameObject StartButton; //the instantiated Start button KEEP
    public GameObject StartButtonPrefab; //Start button prefab, looks like Momma ball KEEP As is; set in inspector

    [SyncVar]
    public uint platformId;

    public GameObject Platform; //Main game playform
    public GameObject PlatformPrefab; //Prefab of game platform
    public Vector3 defaultPlatformVertex; //default vertex value

    [SyncVar]
    private Vector3 targetPlayAreaVertex; //vertex value set from target play area

    //TODO: Remove hand fields that are not needed. Any that are should be references to current player objects.
    //public GameObject Hand; //Right hand object, which tethers the baby balls [IN SCENE; Contains hand script] REMOVE
    //public GameObject Shield; //Left hand, which holds the shield [IN SCENE; contains shield script] REMOVE
    //public GameObject Controller; //right handed controller; child of camera rig
    //public GameObject ShieldController; //left handed controller; child of camera rig
    //public GameObject CameraRig; //The VR camera rig; parent of controllers

    //private GameObject activePlayerHand; //reference to the hand of the active player
    [SyncVar(hook = nameof(ChangeActivePlayer))]
    public uint activePlayerId;

    public GameObject activePlayer;

    [SyncVar]
    private bool aPlayerLocked = false; //bool to lock in the current active player; set to false when the player releases the trigger button.

    [SyncVar]
    public uint titleId;

    public GameObject Title; //Game title, [instantiated]
    public GameObject TitlePrefab; //title prefab KEEP

    //Tutorial fields. Currently disabled
    /*
    [SyncVar]
    public GameObject tutButton; //instantiated tutorial button KEEP
    public GameObject tutorialButtonPrefab; //tutorial button prefab KEEP

    private GameObject LearningBlarp;
    private GameObject tutTrigger;
    private GameObject tutPartial;
    private GameObject tutHit1;
    private GameObject tutHit2;
    private GameObject tutHit3;
    private GameObject tutStart;
    private GameObject tutShield;
    */


    public Material PlatformMat;
    public AudioClip blarpClip; //sound effect when hitting mother ball KEEP
    public AudioClip restartClip; //sound effect when losing KEEP
    private GameObject empty;


    //public Shader PlatformShader;
    [SyncVar]
    public uint roomId;

    public GameObject Room; //GO of game's room
    public GameObject roomPrefab; //Prefab of game room

    [SyncVar(hook = nameof(UpdateMommaMesh))]
    public float score;

    private bool triggerDown; //TODO: make a syncvar?
    private AudioSource restartSound;
    private AudioSource blarpSound;
    private SteamVR_PlayArea PlayArea; //Find a way to get this when server is initialized

    private Vector3 v1;
    private Vector3 v2;

    [SyncVar(hook = nameof(SetRoomScale))]
    private Vector3 roomSize;

    [SyncVar(hook = nameof(SetMommaScale))]
    private Vector3 mommaSize;

    //[SyncVar] Figure out if this needs to be synced
    private Vector4 MommaInfo;

    public AudioClip[] AudioList;
    public AudioClip[] HighScoreAudioList;
    public AudioClip[] MommaHitAudioList;
    public List<AudioSource> AudioSources = new List<AudioSource>();

    private AudioSource MommaHitSound;

    [SyncVar]
    private bool syncedObejctsSpawned = false;

    private bool syncedObjectsSetUp = false;

    [SyncVar]
    private bool gameSetUp = false;

    //Balls that show the high score in the main menu

    readonly SyncListUInt highScoreBallIDs = new SyncListUInt();
    List<GameObject> highScoreBalls = new List<GameObject>();
    //readonly SyncListGO highScoreBalls = new SyncListGO();


    //private bool tutorialFinished;
    //private bool readyToPlay;

    private bool triggerPulled;
    private bool shieldTriggerPulled;
    private float notDeadVal;
    
    [SyncVar]
    public bool isPlaying = false;

    //Set singleton
    private void Awake()
    {
        if (nBallGame != null)
        {
            if (nBallGame != this)
            {
                Debug.Log("nBG Destroyed");
                GameObject.Destroy(this.gameObject);
            }
        }
        nBallGame = this;
        GameObject.DontDestroyOnLoad(this.gameObject);
    }

    void Start()
    {
        BabyIDs.Callback += OnBabyIDListUpdated;
        if (!isServer)
        {
            StartSetUp();
        }
    }


    public void StartSetUp()
    {
        //BabyIDs.Callback += OnBabyIDListUpdated;
        //highScoreBallIDs.Callback += OnHighScoreBallIDListUpdated;

        if (isServer)
        {
            SaveLoad.Load();

            if (Game.current == null)
            {
                Game.current = new Game();
            }
        }

        //EventManager.OnTriggerDown += OnTriggerDown;
        //EventManager.OnTriggerUp += OnTriggerUp;
        //EventManager.StayTrigger += StayTrigger;

        //TODO: check if these line cause only one sound to play; and if so and two proper Audio Sources
        //TODO: also, this needs to be done for every client.
        restartSound = gameObject.AddComponent<AudioSource>();
        blarpSound = gameObject.AddComponent<AudioSource>();

        restartSound.clip = restartClip;
        blarpSound.clip = blarpClip;

        empty = new GameObject();

        //TODO: Need to do this for every client
        AudioList = new AudioClip[]{ (AudioClip)Resources.Load("Audio/hydra/TipHit1"),
                                (AudioClip)Resources.Load("Audio/hydra/TipHit2"),
                                (AudioClip)Resources.Load("Audio/hydra/TipHit3"),
                                (AudioClip)Resources.Load("Audio/hydra/TipHit4"), };

        HighScoreAudioList = new AudioClip[]{  (AudioClip)Resources.Load("Audio/hydra/BaseHit"),
                                      //(AudioClip)Resources.Load("Audio/hydra/ArmStroke2"),
                                      //(AudioClip)Resources.Load("Audio/hydra/ArmStroke3"), 
                                      };

        MommaHitAudioList = new AudioClip[]{  (AudioClip)Resources.Load("Audio/hydra/ArmStroke1"),
                                          (AudioClip)Resources.Load("Audio/hydra/ArmStroke2"),
                                          (AudioClip)Resources.Load("Audio/hydra/ArmStroke3"),
    };




        MommaHitSound = gameObject.AddComponent<AudioSource>();


        for (var i = 0; i < AudioList.Length; i++)
        {
            print(AudioList[i]);

            AudioSource audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = AudioList[i];
            audioSource.Play();
            audioSource.volume = 0;
            AudioSources.Add(audioSource);
        }

        score = 0;


        //HandL.GetComponent<HandScript>().BallGameObj = transform.gameObject;
        //HandR.GetComponent<HandScript>().BallGameObj = transform.gameObject;
        //TODO: Sync this
        //.GetComponent<TextMesh>();


        // Disable tutorial objects
        /*
        LearningBlarp = (GameObject)Instantiate(BabyPrefab, new Vector3(), new Quaternion());
        LearningBlarp.transform.position = new Vector3(0, 1, -2);
        LearningBlarp.GetComponent<SpringJoint>().connectedBody = Hand.GetComponent<Rigidbody>();
        //AudioSource audioSource = LearningBlarp.GetComponent<AudioSource>();
        LearningBlarp.GetComponent<Rigidbody>().drag = .7f - (score / 100);
        LearningBlarp.GetComponent<Rigidbody>().mass = .2f - (score / 340);
        LearningBlarp.GetComponent<Rigidbody>().angularDrag = 200;
        LearningBlarp.GetComponent<Rigidbody>().freezeRotation = true;
        //LearningBlarp.GetComponent<Collider>().enabled = false;
        LearningBlarp.transform.localScale = LearningBlarp.transform.localScale * (2.0f - (score / 30));
        LearningBlarp.GetComponent<MeshRenderer>().material.SetFloat("_Score", (float)score);
        LearningBlarp.GetComponent<MeshRenderer>().material.SetFloat("_Learning", 0);
        LearningBlarp.GetComponent<TrailRenderer>().enabled = false;


        setTutorialObjects();

        if (tutorialFinished == false)
        {
            startTutorial();
        }
        else
        {
            addTutorialButton();
        }
        */

        

        //TODO: Call method with stuff that needs to happen only in server
        if (isServer)
        {
            if (!syncedObejctsSpawned)
            {
                SetupSyncedObjects();
                restart(transform.gameObject);
            }
        }
        else
        {
            ClientSetupSyncObjects();
        }

        gameSetUp = true;
    }

    //CONVERTED
    [Server]
    private void SetupSyncedObjects()
    {
        //Spawn and Sync Room
        GameObject nRoom = (GameObject)Instantiate(roomPrefab, Vector3.zero, Quaternion.identity);
        NetworkServer.Spawn(nRoom);
        Room = nRoom;
        roomId = Room.GetComponent<NetworkIdentity>().netId;

        //Spawn and Sync Platform
        GameObject targetCameraRig = GameObject.Find("[CameraRig](Clone)");
        if (targetCameraRig != null)
        {
            targetPlayAreaVertex = targetCameraRig.GetComponent<SteamVR_PlayArea>().vertices[0];
        }
        else
        {
            targetPlayAreaVertex = defaultPlatformVertex;
        }

        GameObject nPlat = (GameObject)Instantiate(PlatformPrefab, new Vector3(0f, -0.49f, 0f), Quaternion.identity);
        nPlat.transform.localScale = new Vector3(Mathf.Abs(targetPlayAreaVertex.x) * 1.5f, 1.0f, Mathf.Abs(targetPlayAreaVertex.z) * 1.5f);
        nPlat.GetComponent<MeshRenderer>().material.SetVector("_Size", nPlat.transform.localScale);
        nPlat.GetComponent<MeshRenderer>().material.SetFloat("_Learning", 0);
        NetworkServer.Spawn(nPlat);
        Platform = nPlat;
        platformId = Platform.GetComponent<NetworkIdentity>().netId;



        //Spawn and Sync Title
        Vector3 tPos = new Vector3(0f, 1.5f, -3f);
        GameObject nTitle = (GameObject)Instantiate(TitlePrefab, tPos, Quaternion.Euler(0, 180f, 0));
        nTitle.GetComponent<MeshRenderer>().material.SetVector("_Scale", nTitle.transform.localScale);
        NetworkServer.Spawn(nTitle);
        Title = nTitle;
        titleId = Title.GetComponent<NetworkIdentity>().netId;
        

        //Spawn and Sync Highscore balls
        for (int i = 0; i < 10; i++)
        {
            float rad = Random.Range(4, 10);
            Vector3 p = Random.onUnitSphere * rad;

            GameObject nHSBall = (GameObject)Instantiate(HighScorePrefab, p, new Quaternion());
            nHSBall.transform.localScale = new Vector3(rad / 4, rad / 4, rad / 4);
            NetworkServer.Spawn(nHSBall);
            highScoreBalls.Add(nHSBall);
            SetHighScoreBallPitch(nHSBall);
            highScoreBallIDs.Add(nHSBall.GetComponent<NetworkIdentity>().netId);
        }
        setHighScoreBalls(Game.current.highScore);
        //RpcSetHighScoreBalls(Game.current.highScore);


        //Spawn and Sync Momma Ball
        GameObject nMom = (GameObject)Instantiate(MommaPrefab, new Vector3(0,3,0), new Quaternion());
        NetworkServer.Spawn(nMom);
        Momma = nMom;
        mommaID = Momma.GetComponent<NetworkIdentity>().netId;
        ScoreText = Momma.transform.Find("Score").gameObject;

        //Spawn and Sync MenuBaby
        GameObject nMBaby = (GameObject)Instantiate(BabyPrefab, new Vector3(0, 1, -2), Quaternion.identity);
        nMBaby.GetComponent<Rigidbody>().drag = .7f - (score / 100);
        nMBaby.GetComponent<Rigidbody>().mass = .2f - (score / 340);
        nMBaby.GetComponent<Rigidbody>().angularDrag = 200;
        nMBaby.GetComponent<Rigidbody>().freezeRotation = true;
        nMBaby.transform.localScale = nMBaby.transform.localScale * (2.0f - (score / 30));
        nMBaby.GetComponent<MeshRenderer>().material.SetFloat("_Score", (float)score);
        nMBaby.GetComponent<TrailRenderer>().enabled = false;
        NetworkServer.Spawn(nMBaby);
        menuBaby = nMBaby;
        menuBabyID = menuBaby.GetComponent<NetworkIdentity>().netId;

        //Spawn and Sync Start Button
        GameObject nSB = (GameObject)Instantiate(StartButtonPrefab, new Vector3(0, 1, -Platform.transform.localScale.z * .55f), new Quaternion());
        NetworkServer.Spawn(nSB);
        StartButton = nSB;
        startButtonId = StartButton.GetComponent<NetworkIdentity>().netId;

        
        //Set active player to first spawn player object
        activePlayer = GameObject.Find("VRBlarpPlayer(Clone)");
        if(activePlayer == null)
        {
            activePlayer = GameObject.Find("NonVRBlarpPlayer(Clone)");
        }
        activePlayerId = activePlayer.GetComponent<NetworkIdentity>().netId;

        Room.GetComponent<RoomNetworked>().handL = activePlayer.GetComponent<NetworkedPlayer>().shield;
        Room.GetComponent<RoomNetworked>().handR = activePlayer.GetComponent<NetworkedPlayer>().hand;
        //activePlayerHand = GameObject.Find("handR");
        syncedObejctsSpawned = true;
        syncedObjectsSetUp = true;
    }

    private void ClientSetupSyncObjects()
    {
        //Grab all Network IDs
        NetworkIdentity[] netIds = GameObject.FindObjectsOfType<NetworkIdentity>();

        int objectsFound = 0; //Used to break out of foreachLoop
        int hsBallsFound = 0; //Used stop going into forloop

        //Find and Sync Room
        foreach(NetworkIdentity nI in netIds)
        {
            if(nI.netId == roomId)
            {
                Room = nI.gameObject;
                objectsFound++;
            }
            else if (nI.netId == platformId)
            {
                Platform = nI.gameObject;
                Platform.transform.localScale = new Vector3(Mathf.Abs(targetPlayAreaVertex.x) * 1.5f, 1.0f, Mathf.Abs(targetPlayAreaVertex.z) * 1.5f);
                Platform.GetComponent<MeshRenderer>().material.SetVector("_Size", Platform.transform.localScale);
                Platform.GetComponent<MeshRenderer>().material.SetFloat("_Learning", 0);
                objectsFound++;
            }
            else if (nI.netId == titleId)
            {
                Title = nI.gameObject;
                Title.GetComponent<MeshRenderer>().material.SetVector("_Scale", Title.transform.localScale);
                objectsFound++;
            }
            else if(nI.netId == mommaID)
            {
                Momma = nI.gameObject;
                ScoreText = Momma.transform.Find("Score").gameObject;
                objectsFound++;
            }
            else if (nI.netId == menuBabyID)
            {
                menuBaby = nI.gameObject;
                menuBaby.GetComponent<Rigidbody>().drag = .7f - (score / 100);
                menuBaby.GetComponent<Rigidbody>().mass = .2f - (score / 340);
                menuBaby.GetComponent<Rigidbody>().angularDrag = 200;
                menuBaby.GetComponent<Rigidbody>().freezeRotation = true;
                menuBaby.transform.localScale = menuBaby.transform.localScale * (2.0f - (score / 30));
                menuBaby.GetComponent<MeshRenderer>().material.SetFloat("_Score", (float)score);
                menuBaby.GetComponent<TrailRenderer>().enabled = false;
                objectsFound++;
            }
            else if(nI.netId == startButtonId)
            {
                StartButton = nI.gameObject;
                objectsFound++;
            }
            else if(nI.netId == activePlayerId)
            {
                activePlayer = nI.gameObject;
                objectsFound++;
            }
            else if(hsBallsFound < highScoreBallIDs.Count)
            {
                for(int i = 0; i < highScoreBallIDs.Count; i++)
                {
                    if(nI.netId == highScoreBallIDs[i])
                    {
                        highScoreBalls.Add(nI.gameObject);
                        SetHighScoreBallPitch(nI.gameObject);
                        hsBallsFound++;
                        if(hsBallsFound >= highScoreBallIDs.Count)
                        {
                            setHighScoreBalls(score);
                            objectsFound++;
                        }
                        i = 100;
                    }
                }
            }

            if(objectsFound >= 8)
            {
                break;
            }
        }

        //TODO: Test if Highscoreballs must be found here
        //RpcSetHighScoreBalls(Game.current.highScore);
        
        Room.GetComponent<RoomNetworked>().handL = activePlayer.GetComponent<NetworkedPlayer>().shield;
        Room.GetComponent<RoomNetworked>().handR = activePlayer.GetComponent<NetworkedPlayer>().hand;
        //activePlayerHand = GameObject.Find("handR");
        syncedObjectsSetUp = true;
    }



    //CONVERTED
    void setHighScoreBalls(float theScore)
    {

        float base100 = Mathf.Floor(theScore / 100);
        float base10 = Mathf.Floor((theScore - (base100 * 100)) / 10);
        float base1 = theScore - (base10 * 10);
        //print( base1 );

        for (var i = 0; i < 10; i++)
        {
            highScoreBalls[i].GetComponent<MeshRenderer>().material.SetInt("_Digit1", (int)base1);
            highScoreBalls[i].GetComponent<MeshRenderer>().material.SetInt("_Digit2", (int)base10);
        }

    }

    //CONVERTED
    void removeHighScoreBalls()
    {

        for (var i = 0; i < 10; i++)
        {
            highScoreBalls[i].GetComponent<MeshRenderer>().enabled = false;
            highScoreBalls[i].GetComponent<Collider>().enabled = false;
        }


    }

    //CONVERTED
    void addHighScoreBalls()
    {

        for (var i = 0; i < 10; i++)
        {
            highScoreBalls[i].GetComponent<MeshRenderer>().enabled = true;
            highScoreBalls[i].GetComponent<Collider>().enabled = true;
        }


    }

    //Tutorial methods currently disabled
    /*
    void addTutorialButton()
    {
        // tutButton.GetComponent<MeshRenderer>().enabled = false;
        // tutButton.GetComponent<Collider>().enabled = false;
        tutButton.SetActive(true);
    }

    void removeTutorialButton()
    {
        //tutButton.GetComponent<MeshRenderer>().enabled = true;;
        //tutButton.GetComponent<Collider>().enabled = true;

        tutButton.SetActive(false);
    }

    //SPAWN THESE OBJECTS
    void setTutorialObjects()
    {

        tutButton = (GameObject)Instantiate(tutorialButtonPrefab, new Vector3(Platform.transform.localScale.x * .5f, 2, Platform.transform.localScale.z * .5f), new Quaternion());
        tutButton.GetComponent<NetworkedTutorialButton>().ballGame = this;
        removeTutorialButton();

        tutorialFinished = Game.current.finishedTutorial;
        triggerPulled = false;
        shieldTriggerPulled = false;
        readyToPlay = false;
        notDeadVal = -5;

        triggerDown = false;


        tutShield = Shield.transform.Find("tutShield").gameObject;

        tutTrigger = Hand.transform.Find("tutTrigger").gameObject;
        tutPartial = Hand.transform.Find("tutPartial").gameObject;
        tutHit1 = Hand.transform.Find("tutHit1").gameObject;
        tutHit2 = Hand.transform.Find("tutHit2").gameObject;
        tutHit3 = Hand.transform.Find("tutHit3").gameObject;
        tutStart = Hand.transform.Find("tutStart").gameObject;

        tutShield.GetComponent<MeshRenderer>().enabled = false;
        tutTrigger.GetComponent<MeshRenderer>().enabled = false;
        tutHit1.GetComponent<MeshRenderer>().enabled = false;
        tutHit2.GetComponent<MeshRenderer>().enabled = false;
        tutHit3.GetComponent<MeshRenderer>().enabled = false;
        tutPartial.GetComponent<MeshRenderer>().enabled = false;
        tutStart.GetComponent<MeshRenderer>().enabled = false;

    }

    public void startTutorial()
    {

        removeHighScoreBalls();

        removeTutorialButton();

        Title.GetComponent<MeshRenderer>().enabled = false;
        //Title.GetComponent<BoxCollider>().enabled = false;


        MommaHitSound.clip = MommaHitAudioList[Random.Range(0, MommaHitAudioList.Length)];
        MommaHitSound.Play();

        Game.current.finishedTutorial = false;
        tutorialFinished = false;
        triggerPulled = false;
        shieldTriggerPulled = false;
        readyToPlay = false;
        notDeadVal = -5;

        LearningBlarp.GetComponent<MeshRenderer>().material.SetFloat("_Learning", 1);
        Platform.GetComponent<MeshRenderer>().material.SetFloat("_Learning", 1);
        Title.GetComponent<MeshRenderer>().material.SetFloat("_Learning", 1);
        //Shield.transform.Find("Shield").GetComponent<MeshRenderer>().material.SetFloat("_Learning" , 1 );

        StartButton.SetActive(false);

        tutTrigger.GetComponent<MeshRenderer>().enabled = true;
        tutShield.GetComponent<MeshRenderer>().enabled = true;


    }

    void tutorialHandHit()
    {

        tutHit1.GetComponent<MeshRenderer>().enabled = false;
        tutHit2.GetComponent<MeshRenderer>().enabled = false;
        tutHit3.GetComponent<MeshRenderer>().enabled = false;
        tutPartial.GetComponent<MeshRenderer>().enabled = false;


        notDeadVal = 0.0f;

        float v = Random.Range(0.0001f, 3.99999f);

        if (v <= 1.0f)
        {
            tutHit1.GetComponent<MeshRenderer>().enabled = true;
        }
        else if (1.0f < v && v <= 2.0f)
        {
            tutHit2.GetComponent<MeshRenderer>().enabled = true;
        }
        else if (2.0f < v && v <= 3.0f)
        {
            tutHit3.GetComponent<MeshRenderer>().enabled = true;
        }
        else if (3.0f < v && v <= 4.0f)
        {
            tutPartial.GetComponent<MeshRenderer>().enabled = true;
        }

    }

    void finishTutorial()
    {
        tutStart.GetComponent<MeshRenderer>().enabled = false;
        tutShield.GetComponent<MeshRenderer>().enabled = false; ;
        LearningBlarp.GetComponent<MeshRenderer>().material.SetFloat("_Learning", 0);
        Platform.GetComponent<MeshRenderer>().material.SetFloat("_Learning", 0);
        Title.GetComponent<MeshRenderer>().material.SetFloat("_Learning", 0);
        Shield.transform.Find("Shield").GetComponent<MeshRenderer>().material.SetFloat("_Learning", 0);

        tutorialFinished = true;
        Game.current.finishedTutorial = true;
        SaveLoad.Save();
        addHighScoreBalls();


        MommaHitSound.clip = MommaHitAudioList[Random.Range(0, MommaHitAudioList.Length)];
        MommaHitSound.Play();

    }
    */
    /*
    void getReadyToPlay()
    {

        readyToPlay = true;

        tutHit1.GetComponent<MeshRenderer>().enabled = false;
        tutHit2.GetComponent<MeshRenderer>().enabled = false;
        tutHit3.GetComponent<MeshRenderer>().enabled = false;
        tutPartial.GetComponent<MeshRenderer>().enabled = false;

        tutStart.GetComponent<MeshRenderer>().enabled = true;


        MommaHitSound.clip = MommaHitAudioList[Random.Range(0, MommaHitAudioList.Length)];
        MommaHitSound.Play();

        StartButton.SetActive(true);
    }
    */

    /*
    void UpdateTutorial()
    {

        tutButton.transform.LookAt(new Vector3(0, 2, 0));
        if (tutorialFinished == false)
        {
            if (triggerPulled == false && Controller.GetComponent<controllerInfo>().triggerVal > 0.8)
            {
                triggerPulled = true;
                notDeadVal = 0.0f;

                MommaHitSound.clip = MommaHitAudioList[Random.Range(0, MommaHitAudioList.Length)];
                MommaHitSound.Play();
                tutTrigger.GetComponent<MeshRenderer>().enabled = false;
            }

            if (shieldTriggerPulled == false && ShieldController.GetComponent<controllerInfo>().triggerVal > 0.8)
            {
                shieldTriggerPulled = true;
                notDeadVal = 0.0f;

                MommaHitSound.clip = MommaHitAudioList[Random.Range(0, MommaHitAudioList.Length)];
                MommaHitSound.Play();
                tutShield.GetComponent<MeshRenderer>().enabled = false; ;
            }
        }

        notDeadVal += .001f;

        if (notDeadVal > 1.0f && readyToPlay == false && triggerPulled == true)
        {

            getReadyToPlay();

        }

    }
    */

    //CONVERTED
    void Update()
    {

        //UpdateTutorial();

        #if UNITY_EDITOR
        UpdateMommaMesh(score);
        #endif

        if (!gameSetUp)
        {
            Debug.Log("Server no set up yet, wait to update.");
            return;
        }

        //if( triggerDown == false ){
        foreach (GameObject baby in Babies)
        {
            BabyUpdate(baby);
        }

        if (menuBaby.activeSelf)
        {
            BabyUpdate(menuBaby);
        }

        //Hand.transform.localScale = new Vector3(
    }

    private void BabyUpdate(GameObject baby)
    {
        //this requires access to the players hand position
        //v1 = baby.transform.position - activePlayer.GetComponent<NetworkedPlayer>().GetHand().transform.position;
        v1 = baby.transform.position - activePlayer.GetComponent<NetworkedPlayer>().GetHandPosition();
        float l = v1.magnitude;

        float w = (1.0f / (1.0f + l)) * (1.0f / (1.0f + l)) * (1.0f / (1.0f + l));

        float lineWidth = w * .05f;

        //Line and trail will need to be set on ecah client.
        LineRenderer r = baby.GetComponent<LineRenderer>();
        Material m = r.material;
        r.SetPosition(0, baby.transform.position);
        //r.SetPosition(1, activePlayer.GetComponent<NetworkedPlayer>().GetHand().transform.position);
        r.SetPosition(1, activePlayer.GetComponent<NetworkedPlayer>().GetHandPosition());
        //r.SetWidth(lineWidth, lineWidth);
        r.startWidth = lineWidth;
        r.endWidth = lineWidth;
        //r.SetWidth(1, lineWidth);
        //r.SetColors(Color.red, Color.green);
        r.startColor = Color.red;
        r.endColor = Color.green;
        //m.SetVector("startPoint", activePlayer.GetComponent<NetworkedPlayer>().GetHand().transform.position);
        m.SetVector("startPoint", activePlayer.GetComponent<NetworkedPlayer>().GetHandPosition());
        m.SetVector("endPoint", baby.transform.position);
        //m.SetFloat("trigger", Controller.GetComponent<controllerInfo>().triggerVal);
        //m.SetFloat("trigger", activePlayer.GetComponent<NetworkedPlayer>().GetHandScript().TriggerVal);
        m.SetFloat("trigger", activePlayer.GetComponent<NetworkedPlayer>().GetHandTriggerVal());


        m = baby.GetComponent<TrailRenderer>().material;
        m.SetVector("_Size", roomSize);
        m.SetVector("_MommaInfo", MommaInfo);

        //Set transform on server only;
        Vector3 v = baby.GetComponent<Rigidbody>().velocity;
        if (isServer)
        {
            baby.transform.LookAt(baby.transform.position + v, Vector3.up);
        }
        v = baby.transform.InverseTransformDirection(v);
        m = baby.GetComponent<MeshRenderer>().material;
        m.SetVector("_Velocity", v);
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
            UpdateBabyForces(activePlayer);
        }
    }

    public void SetActivePlayerID(uint playerNetID)
    {
        if(!aPlayerLocked)
        {
            activePlayerId = playerNetID;
            ChangeActivePlayer(playerNetID);
        }
    }

    private void ChangeActivePlayer(uint newID)
    {
        foreach(NetworkIdentity netId in GameObject.FindObjectsOfType<NetworkIdentity>())
        {
            if(netId.netId == newID)
            {
                activePlayer = netId.gameObject;
                activePlayer.GetComponent<NetworkedPlayer>().isActivePlayer = true;
                aPlayerLocked = true;
                menuBaby.GetComponent<SpringJoint>().connectedBody = activePlayer.GetComponent<NetworkedPlayer>().GetHand().GetComponent<Rigidbody>();
                foreach (GameObject baby in Babies)
                {
                    baby.GetComponent<SpringJoint>().connectedBody = activePlayer.GetComponent<NetworkedPlayer>().GetHand().GetComponent<Rigidbody>();
                }
                break;
            }
        }
    }

    public void UnlockActivePlayer()
    {
       aPlayerLocked = false;
    }

    //CONVERTED
    private void UpdateMommaMesh(float newScore)
    {
        if(Momma != null)
        {
            float base100 = Mathf.Floor(score / 100);
            float base10 = Mathf.Floor((score - (base100 * 100)) / 10);
            float base1 = score - (base10 * 10);

            Momma.GetComponent<MeshRenderer>().material.SetInt("_Digit1", (int)base1);
            Momma.GetComponent<MeshRenderer>().material.SetInt("_Digit2", (int)base10);
        }
    }

    //CONVERTED
    public void MommaHit(GameObject goHit)
    {

        // Make a new object
        GameObject go = (GameObject)Instantiate(BabyPrefab, new Vector3(), new Quaternion());
        go.transform.position = goHit.transform.position;
        //TODO: Create reference for active player rigidbody
        go.GetComponent<SpringJoint>().connectedBody = activePlayer.GetComponent<NetworkedPlayer>().GetHand().GetComponent<Rigidbody>();
       
        go.GetComponent<Rigidbody>().drag = .7f - (score / 100);
        go.GetComponent<Rigidbody>().mass = .2f - (score / 340);
        go.GetComponent<Rigidbody>().angularDrag = 200;
        go.GetComponent<Rigidbody>().freezeRotation = true;
        go.transform.localScale = go.transform.localScale * (2.0f - (score / 30));
        go.GetComponent<MeshRenderer>().material.SetFloat("_Score", (float)score);

        //audioSource.clip = AudioList[(int)score];

        //    go.GetComponent<SpringJoint>().enabled = false; connectedBody = HandR.GetComponent<Rigidbody>();
        NetworkServer.Spawn(go);
        BabyIDs.Add(go.GetComponent<NetworkIdentity>().netId);

        score++;
        Game.current.lastScore = score;
        if (score > Game.current.highScore) { Game.current.highScore = score; }

        ScoreText.GetComponent<TextMesh>().text = score.ToString();

        resizeRoom();
        moveMomma();

        int aLIndex = Random.Range(0, MommaHitAudioList.Length);
        MommaHitSound.clip = MommaHitAudioList[aLIndex];
        MommaHitSound.Play();
        RpcPlayMommaHitAudio(aLIndex);
        

    }

    //CONVERTED
    float getSizeFromScore()
    {
        return 3.0f + score / 3;
    }

    //CONVERTED
    public void moveMomma()
    {

        float size = getSizeFromScore();



        Momma.transform.position = new Vector3(Random.Range(size / 4, size / 2),
                                                Random.Range(0 + .15f, size / 2 + .15f),
                                                Random.Range(size / 4, size / 2));

        float xSign = Random.value < .5 ? 1 : -1;
        float zSign = Random.value < .5 ? 1 : -1;
        Vector3 v = new Vector3(
          xSign,
          1,
          zSign
        );

        Momma.transform.position = Vector3.Scale(Momma.transform.position, v);

        Momma.transform.localScale = new Vector3(.3f, .3f, .3f);
        mommaSize = Momma.transform.localScale;

        MommaInfo = new Vector4(
          Momma.transform.position.x,
          Momma.transform.position.y,
          Momma.transform.position.z,
          Momma.transform.localScale.x
        );

        Momma.GetComponent<AudioSource>().Play();
        RpcPlayMommaAudio();

        waitMomma();
    }

    //CONVERTED
    void waitMomma()
    {

        Momma.SetActive(false);
        RpcEnableMomma(false,0);
        StartCoroutine(WaitToMakeMommaReal());

    }

    //CONVERTED
    IEnumerator WaitToMakeMommaReal()
    {
        while (true)
        {
            //print("unwaited");
            yield return new WaitForSeconds(2.0f);
            //print("WAITERD");
            makeMommaReal();
            //return false;
            yield return false;

        }
    }

    //CONVERTED
    void makeMommaReal()
    {
        Debug.Log("Making Momma Real");
        Momma.SetActive(true);
        RpcEnableMomma(true,0);
        Momma.GetComponent<AudioSource>().Play();
        RpcPlayMommaAudio();
    }

    //CONVERTED
    private void SetMommaScale(Vector3 newScale)
    {
        if(Momma != null)
        {
            Momma.transform.localScale = newScale;
        }
    }

    //CONVERTED
    void resizeRoom()
    {

        float size = getSizeFromScore();

        Room.transform.localScale = new Vector3(size, size / 2 + .6f, size);
        Room.transform.position = new Vector3(0, size / 4 + .3f, 0);


        roomSize = Room.transform.localScale;
    }

    //CONVERTED
    void resizeRoomLarge()
    {

        float size = getSizeFromScore();

        Room.transform.localScale = new Vector3(10000, 10000, 100000);
        Room.transform.position = new Vector3(0, 500, 0);


        roomSize = Room.transform.localScale;

    }

    //CONVERTED
    private void SetRoomScale(Vector3 newScale)
    {
        if(Room != null)
        {
            Room.transform.localScale = newScale;
        }
        
    }

    //TODO: Replace instance of this method call with restart?
    public void HandHit(GameObject handHit)
    {
        restart(handHit);
    }

    //CONVERTED
    void restart(GameObject handHit)
    {
        if (isServer)
        {
            SaveLoad.Save();
            RpcClearBabies();
            foreach (GameObject baby in Babies)
            {


                NetworkServer.Destroy(baby);


            }
            BabyIDs.Clear();
            Babies.Clear();
            score = 0;

            blarpSound.Play();
            RpcPlayBlarpSound();

            addHighScoreBalls();
            RpcAddHighScoreBalls();

            setHighScoreBalls(Game.current.highScore);
            RpcSetHighScoreBalls(Game.current.highScore);

            Title.GetComponent<MeshRenderer>().enabled = true;
            RpcEnableTitle(true);

            StartButton.GetComponent<MeshRenderer>().enabled = true;
            StartButton.GetComponent<BoxCollider>().enabled = true;
            RpcEnableStartButton(true);


            Momma.GetComponent<MeshRenderer>().enabled = false;
            Momma.GetComponent<Collider>().enabled = false;
            RpcEnableMomma(true, 1);

            Momma.transform.position = new Vector3(0, -1000000.0f, 0);

            Room.GetComponent<RoomNetworked>().active = false;
            RpcSetRoomActive(false);
            resizeRoomLarge();

            menuBaby.SetActive(true);
            RpcSetMenuBabyActive(true);
            menuBaby.transform.position = new Vector3(0, 1, 2);
            menuBaby.GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);

            isPlaying = false;
        }

        /*
        LearningBlarp.SetActive(true);
        LearningBlarp.transform.position = new Vector3(0, 1, 2);//transform.position = new Vector3( 0 , 1 , -2);
        LearningBlarp.GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
        //      LearningBlarp.GetComponent<MeshRenderer>().enabled = true;

        if (tutorialFinished == true)
        {
            addTutorialButton(); addHighScoreBalls();
            Title.GetComponent<MeshRenderer>().enabled = true;
        };*/
        //      Title.GetComponent<BoxCollider>().enabled = false;};

        
        //startGame( handHit );
    }

    //CONVERTED
    public void startGame(GameObject go)
    {
        if (isServer)
        {
            restartSound.Play();
            RpcPlayRestartAudio();
            removeHighScoreBalls();
            RpcRemoveHighScoreBalls();


            resizeRoom();
            Room.GetComponent<RoomNetworked>().active = true;
            RpcSetRoomActive(true);


            int hitSoundIndex = Random.Range(0, MommaHitAudioList.Length);
            MommaHitSound.clip = MommaHitAudioList[hitSoundIndex];
            MommaHitSound.Play();
            RpcPlayMommaHitAudio(hitSoundIndex);




            //LearningBlarp.GetComponent<MeshRenderer>().enabled = false;
            //LearningBlarp.GetComponent<Collider>().enabled = false;

            menuBaby.SetActive(false);
            RpcSetMenuBabyActive(false);

            StartButton.GetComponent<MeshRenderer>().enabled = false;
            StartButton.GetComponent<BoxCollider>().enabled = false;
            RpcEnableStartButton(false);


            Momma.GetComponent<MeshRenderer>().enabled = true;
            RpcEnableMomma(true, 1);

            Title.GetComponent<MeshRenderer>().enabled = false;
            //Title.GetComponent<BoxCollider>().enabled = false;
            RpcEnableTitle(false);


            float size = getSizeFromScore();
            empty.transform.position = new Vector3(Random.Range(-size / 4, size / 4),
                                                    Random.Range(size / 4 + .15f, size / 4 + .15f),
                                                    Random.Range(-size / 4, size / 4));

            empty.transform.position = new Vector3(0,
                                                    1,
                                                    -size / 2);


            MommaHit(empty);

            isPlaying = true;
            //StartButton.transform.position = new Vector3( 0 , 1 , 0 );
        }
    }

    void OnTriggerDown(GameObject go)
    {

        // triggerDown = true;

    }

    void OnTriggerUp(GameObject go)
    {

        //triggerDown = false;

    }


    /// <summary>
    /// Updates baby to move towards controller when trigger is down.
    /// </summary>
    /// <param name="go"></param>
    void UpdateBabyForces(GameObject go)
    {
        //Move this to update method on the player/hand script, then send change to BallGame through Command
        //float triggerVal = Controller.GetComponent<controllerInfo>().triggerVal;
        //Debug.Log("AP: " + activePlayer == null);
        //Debug.Log("NP: " + activePlayer.GetComponent<NetworkedPlayer>() == null);
        float triggerVal = activePlayer.GetComponent<NetworkedPlayer>().GetHandTriggerVal();
        if (triggerVal > 0) { triggerDown = true; } else { triggerDown = false; }


        foreach (GameObject baby in Babies)
        {

            v1 = baby.transform.position - go.transform.position;
            float lV1 = v1.magnitude;
            v1.Normalize();


            //Vector3 v = Controller.GetComponent<controllerInfo>().velocity;
            //Vector3 v = activePlayer.GetComponent<NetworkedPlayer>().GetHandScript().Velocity;
            Vector3 v = activePlayer.GetComponent<NetworkedPlayer>().GetHandVelocity();
            float lVel = v.magnitude;
            float dot = Vector3.Dot(v, v1);

            v1 = -.5f * triggerVal * v1 * lVel * (-dot + 1);
            baby.GetComponent<Rigidbody>().AddForce(v1);

            SpringJoint sj = baby.GetComponent<SpringJoint>();
            sj.spring = 1 * triggerVal;

            /*
            float w = (1.0f / (1.0f + lV1)) * (1.0f / (1.0f + lV1));

            float lineWidth = w * .15f;


            Color c = new Color(w, w, w);
            */
        }

        if (menuBaby.activeSelf)
        {
            v1 = menuBaby.transform.position - go.transform.position;
            float lV1 = v1.magnitude;
            v1.Normalize();


            //Vector3 v = Controller.GetComponent<controllerInfo>().velocity;
            //float lVel = v.magnitude;
            Vector3 v = activePlayer.GetComponent<NetworkedPlayer>().GetHandVelocity();
            float lVel = v.magnitude;
            float dot = Vector3.Dot(v, v1);

            v1 = -.5f * triggerVal * v1 * lVel * (-dot + 1);
            menuBaby.GetComponent<Rigidbody>().AddForce(v1);

            SpringJoint sj = menuBaby.GetComponent<SpringJoint>();
            sj.spring = 1 * triggerVal;
        }

        //For tutorial use
        //print( LearningBlarp);
        /*if (LearningBlarp != null)
        {
            //print("YYUP");

            v1 = LearningBlarp.transform.position - go.transform.position;
            float lV1 = v1.magnitude;
            v1.Normalize();


            Vector3 v = Controller.GetComponent<controllerInfo>().velocity;
            float lVel = v.magnitude;
            float dot = Vector3.Dot(v, v1);

            v1 = -.5f * triggerVal * v1 * lVel * (-dot + 1);
            LearningBlarp.GetComponent<Rigidbody>().AddForce(v1);

            SpringJoint sj = LearningBlarp.GetComponent<SpringJoint>();
            sj.spring = 1 * triggerVal;


            float w = (1.0f / (1.0f + lV1)) * (1.0f / (1.0f + lV1));

            float lineWidth = w * .15f;


            Color c = new Color(w, w, w);


        }*/

    }

    //-------Delegates and Callbacks-----------
    void OnBabyIDListUpdated(SyncListUInt.Operation op, int index, uint oldId, uint newId)
    {
        switch (op)
        {
            case SyncListUInt.Operation.OP_ADD:
                foreach(NetworkIdentity netId in GameObject.FindObjectsOfType<NetworkIdentity>())
                {
                    if(netId.netId == newId)
                    {
                        Babies.Add(netId.gameObject);
                        PlayNewBabyAudio(netId.gameObject);
                        break;
                    }
                }
                break;
            default:
                break;
        }
    }

    private void PlayNewBabyAudio(GameObject newBaby)
    {
        AudioSource audioSource = newBaby.GetComponent<AudioSource>();
        audioSource.clip = AudioList[(int)score % 4];
        audioSource.pitch = .25f * Mathf.Pow(2, (int)(score / 4));
        audioSource.volume = Mathf.Pow(.7f, (int)(score / 4));
        audioSource.Play();
    }

    private void SetHighScoreBallPitch(GameObject hsBall)
    {
        int rand = Random.Range(0, 1);
        hsBall.GetComponent<AudioSource>().clip = HighScoreAudioList[rand];
        hsBall.GetComponent<AudioSource>().pitch = .5f;
    }

    [ClientRpc]
    void RpcPlayMommaHitAudio(int index)
    {
        if (!isServer)
        {
            MommaHitSound.clip = MommaHitAudioList[index];
            MommaHitSound.Play();
        }
    }

    [ClientRpc]
    void RpcPlayMommaAudio()
    {
        if (!isServer)
        {
            Momma.GetComponent<AudioSource>().Play();
        }
    }

    [ClientRpc]
    void RpcEnableMomma(bool enable,int enableMode)
    {
        if(enableMode == 1)
        {
            Momma.GetComponent<MeshRenderer>().enabled = enable;
            Momma.GetComponent<Collider>().enabled = enable;
        }
        else
        {
            Momma.SetActive(enable);
        }
    }

    [ClientRpc]
    void RpcSetMenuBabyActive(bool active)
    {
        menuBaby.SetActive(active);
    }

    [ClientRpc]
    void RpcSetHighScoreBalls(float newScore)
    {
        setHighScoreBalls(newScore);
    }

    [ClientRpc]
    void RpcRemoveHighScoreBalls()
    {
        removeHighScoreBalls();
    }

    [ClientRpc]
    void RpcAddHighScoreBalls()
    {
        addHighScoreBalls();
    }

    [ClientRpc]
    void RpcPlayRestartAudio()
    {
        restartSound.Play();
    }

    [ClientRpc]
    void RpcSetRoomActive(bool enable)
    {
        Room.GetComponent<RoomNetworked>().active = enable;
    }

    [ClientRpc]
    void RpcEnableStartButton(bool enable)
    {
        StartButton.GetComponent<MeshRenderer>().enabled = enable;
        StartButton.GetComponent<BoxCollider>().enabled = enable;
    }

    [ClientRpc]
    void RpcEnableTitle(bool enable)
    {
        Title.GetComponent<MeshRenderer>().enabled = enable;
    }

    [ClientRpc]
    void RpcPlayBlarpSound()
    {
        blarpSound.Play();
    }

    [ClientRpc]
    void RpcClearBabies()
    {
        Babies.Clear();
    }
}
