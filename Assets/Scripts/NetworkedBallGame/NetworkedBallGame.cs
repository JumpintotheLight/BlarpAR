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
class SyncListVector3 : SyncList<Vector3> { }

public class NetworkedBallGame : NetworkBehaviour
{
    public static NetworkedBallGame nBallGame;

    //readonly SyncListUInt BabyIDs = new SyncListUInt();
    //readonly SyncListGO Babies = new SyncListGO(); //List of all babies in scene
    //List<GameObject> Babies = new List<GameObject>();

    public GameObject menuBaby; //The baby instance used for the menu
    public GameObject BabyPrefab; //The balls that the player pulls KEEP As is; set in inspector
    //public GameObject MommaPrefab; //The ball that the player needs to ram the others into KEEP As is; set in inspector
    public GameObject HighScorePrefab; //Version of momma ball that displays the highscore KEEP As is; set in inspector

    /*[SyncVar]
    public uint mommaID;*/

    public GameObject Momma; // the current, instantiated momma Momma ball (check if it is destroyed or just teleported) KEEP
    public GameObject ScoreText; // TextMesh of the score for the momma ball

    /*[SyncVar]
    public uint startButtonId;*/

    public GameObject StartButton; //the instantiated Start button KEEP
    //public GameObject StartButtonPrefab; //Start button prefab, looks like Momma ball KEEP As is; set in inspector

    /*[SyncVar]
    public uint platformId;*/

    public GameObject Platform; //Main game playform
    //public GameObject PlatformPrefab; //Prefab of game platform
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

    /*[SyncVar]
    public uint titleId;*/

    public GameObject Title; //Game title, [instantiated]
    //public GameObject TitlePrefab; //title prefab KEEP

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
    /*[SyncVar]
    public uint roomId;

    public GameObject Room; //GO of game's room*/
    //public GameObject roomPrefab; //Prefab of game room

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

    //readonly SyncListUInt highScoreBallIDs = new SyncListUInt();
   // public List<GameObject> highScoreBalls = new List<GameObject>();
    //readonly SyncListFloat hsBallScales = new SyncListFloat();
    //readonly SyncListVector3 hsBallPositions = new SyncListVector3();
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
                return;
            }
        }
        nBallGame = this;
        GameObject.DontDestroyOnLoad(this.gameObject);
    }

    void Start()
    {
        //BabyIDs.Callback += OnBabyIDListUpdated;
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
            if (!syncedObjectsSetUp)
            {
                SetupSyncedObjects();
                restart(transform.gameObject);
            }
        }
        else
        {
            ClientSyncObjects();
        }

        gameSetUp = true;
    }

    //CONVERTED
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
        Platform.transform.localScale = new Vector3(Mathf.Abs(targetPlayAreaVertex.x) * 1.5f, 1.0f, Mathf.Abs(targetPlayAreaVertex.z) * 1.5f);
        Platform.GetComponent<MeshRenderer>().material.SetVector("_Size", Platform.transform.localScale);
        Platform.GetComponent<MeshRenderer>().material.SetFloat("_Learning", 0);




        //SetUp Title
          //Vector3 tPos = new Vector3(0f, 1.5f, -3f);
          //GameObject nTitle = (GameObject)Instantiate(TitlePrefab, tPos, Quaternion.Euler(0, 180f, 0));
        Title.GetComponent<MeshRenderer>().material.SetVector("_Scale", Title.transform.localScale);


        //Setup Highscore balls
        float rad = 0;
        Vector3 p = Vector3.zero;
        for (int i = 0; i < 10; i++)
        {
            rad = Random.Range(4, 10);
            p = Random.onUnitSphere * rad;

            GameObject nHSBall = (GameObject)Instantiate(HighScorePrefab, p, new Quaternion());
            nHSBall.transform.localScale = new Vector3(rad / 4, rad / 4, rad / 4);
            SetHighScoreBallPitch(nHSBall, Random.Range(0, 1));
            NetworkServer.Spawn(nHSBall);
        }
        //RpcSetHighScoreBalls(Game.current.highScore);
        /*foreach(GameObject hsb in highScoreBalls)
        {
            rad = Random.Range(4, 10);
            p = Random.onUnitSphere * rad;

            hsb.transform.position = p;
            hsb.transform.localScale = new Vector3(rad / 4, rad / 4, rad / 4);
            hsBallScales.Add(rad);
            hsBallPositions.Add(hsb.transform.localScale);
            SetHighScoreBallPitch(hsb, Random.Range(0, 1));
        }*/
        BlarpEventManager.SetHighScoreBalls(Game.current.highScore);


        //Momma already SetUp
        //ScoreText = Momma.transform.Find("Score").gameObject;

        //Setup MenuBaby
        menuBaby.GetComponent<Rigidbody>().isKinematic = false;
        menuBaby.GetComponent<Rigidbody>().drag = .7f - (score / 100);
        menuBaby.GetComponent<Rigidbody>().mass = .2f - (score / 340);
        menuBaby.GetComponent<Rigidbody>().angularDrag = 200;
        //menuBaby.GetComponent<Rigidbody>().freezeRotation = true;
        menuBaby.transform.localScale = menuBaby.transform.localScale * (2.0f - (score / 30));
        menuBaby.GetComponent<MeshRenderer>().material.SetFloat("_Score", (float)score);
        //menuBaby.GetComponent<TrailRenderer>().enabled = false;

        //SetUp Start Button
        StartButton.transform.position = new Vector3(0, 1, -Platform.transform.localScale.z * .55f);

        
        //Set active player to first spawn player object
        activePlayer = GameObject.Find("VRBlarpPlayer(Clone)");
        if(activePlayer == null)
        {
            activePlayer = GameObject.Find("NonVRBlarpPlayer(Clone)");
        }
        activePlayerId = activePlayer.GetComponent<NetworkIdentity>().netId;

        GameObject.FindObjectOfType<RoomNetworked>().handL = activePlayer.GetComponent<NetworkedPlayer>().shield;
        GameObject.FindObjectOfType<RoomNetworked>().handR = activePlayer.GetComponent<NetworkedPlayer>().hand;
        //activePlayerHand = GameObject.Find("handR");
        syncedObjectsSetUp = true;
    }

    private void ClientSyncObjects()
    {
        //Sync Platform
        Platform.transform.localScale = new Vector3(Mathf.Abs(targetPlayAreaVertex.x) * 1.5f, 1.0f, Mathf.Abs(targetPlayAreaVertex.z) * 1.5f);
        Platform.GetComponent<MeshRenderer>().material.SetVector("_Size", Platform.transform.localScale);
        Platform.GetComponent<MeshRenderer>().material.SetFloat("_Learning", 0);

        //Sync Menu Baby
        menuBaby.GetComponent<Rigidbody>().drag = .7f - (score / 100);
        menuBaby.GetComponent<Rigidbody>().mass = .2f - (score / 340);
        menuBaby.GetComponent<Rigidbody>().angularDrag = 200;
        //menuBaby.GetComponent<Rigidbody>().freezeRotation = true;
        menuBaby.transform.localScale = menuBaby.transform.localScale * (2.0f - (score / 30));
        menuBaby.GetComponent<MeshRenderer>().material.SetFloat("_Score", (float)score);

        //Sync Title
        Title.GetComponent<MeshRenderer>().material.SetVector("_Scale", Title.transform.localScale);

        //Sync HSBalls
        /*for(int i = 0; i < highScoreBalls.Count; i++)
        {
            highScoreBalls[i].transform.position = hsBallPositions[i];
            highScoreBalls[i].transform.localScale = new Vector3(hsBallScales[i] / 4, hsBallScales[i] / 4, hsBallScales[i] / 4);

            SetHighScoreBallPitch(highScoreBalls[i], Random.Range(0, 1));
        }*/

        //Room.GetComponent<RoomNetworked>().handL = activePlayer.GetComponent<NetworkedPlayer>().shield;
        //Room.GetComponent<RoomNetworked>().handR = activePlayer.GetComponent<NetworkedPlayer>().hand;
        //activePlayerHand = GameObject.Find("handR");
        BlarpEventManager.EnableHighScoreBalls();
        BlarpEventManager.SetHighScoreBalls(score);

        ChangeActivePlayer(activePlayerId);

        syncedObjectsSetUp = true;
    }



    //CONVERTED
    /*void setHighScoreBalls(float theScore)
    {

        float base100 = Mathf.Floor(theScore / 100);
        float base10 = Mathf.Floor((theScore - (base100 * 100)) / 10);
        float base1 = theScore - (base10 * 10);
        //print( base1 );

        foreach(GameObject hsb in highScoreBalls)
        {
            hsb.GetComponent<MeshRenderer>().material.SetInt("_Digit1", (int)base1);
            hsb.GetComponent<MeshRenderer>().material.SetInt("_Digit2", (int)base10);
        }

    }*/

    //CONVERTED
    /*void removeHighScoreBalls()
    {
        foreach (GameObject hsb in highScoreBalls)
        {
            hsb.GetComponent<MeshRenderer>().enabled = false;
            hsb.GetComponent<Collider>().enabled = false;
        }


    }*/

    //CONVERTED
    /*void addHighScoreBalls()
    {
        foreach (GameObject hsb in highScoreBalls)
        {
            hsb.GetComponent<MeshRenderer>().enabled = true;
            hsb.GetComponent<Collider>().enabled = true;
        }

    }*/

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
        if (isServer)
        {
            if(aPlayerLocked && activePlayer == null)
            {
                UnlockActivePlayer();
            }
        }
        //UpdateTutorial();

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
            BlarpEventManager.UpdateBabyRenders(activePlayer, roomSize, MommaInfo);
            /*foreach (GameObject baby in Babies)
            {
                BabyUpdate(baby);
            }

            if (menuBaby.activeSelf)
            {
                BabyUpdate(menuBaby);
            }*/
        }
        //if( triggerDown == false ){
        

        //Hand.transform.localScale = new Vector3(
    }

    /*private void BabyUpdate(GameObject baby)
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
    }*/

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
                    /*foreach (GameObject baby in Babies)
                    {
                        baby.GetComponent<SpringJoint>().connectedBody = activePlayer.GetComponent<NetworkedPlayer>().GetHand().GetComponent<Rigidbody>();
                    }*/
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
        if (!isServer)
        {
            return;
        }
        //Update Score and Move Momma Ball
        score++;
        Game.current.lastScore = score;
        if (score > Game.current.highScore) { Game.current.highScore = score; }

        ScoreText.GetComponent<TextMesh>().text = score.ToString();

        resizeRoom();
        RpcResizeRoom(false);
        moveMomma();

        // Make a new Baby Ball object
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
        
        AudioSource aS = go.GetComponent<AudioSource>();
        aS.clip = AudioList[(int)score % 4];
        aS.pitch = .25f * Mathf.Pow(2, (int)(score / 4));
        aS.volume = Mathf.Pow(.7f, (int)(score / 4));
        aS.Play();
        NetworkServer.Spawn(go);
        go.GetComponent<Rigidbody>().isKinematic = false;

        

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
            yield return new WaitForSeconds(1.0f);
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
        GameObject room = GameObject.Find("Room");
        if(room != null)
        {
            float size = getSizeFromScore();

            room.transform.localScale = new Vector3(size, size / 2 + .6f, size);
            room.transform.position = new Vector3(0, size / 4 + .3f, 0);


            roomSize = room.transform.localScale;
        }
        
    }

    //CONVERTED
    void resizeRoomLarge()
    {
        GameObject room = GameObject.Find("Room");
        if(room != null)
        {
            float size = getSizeFromScore();

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
        restart(handHit);
    }

    //CONVERTED
    void restart(GameObject handHit)
    {
        if (isServer)
        {
            SaveLoad.Save();
            //RpcClearBabies();

            foreach (BabyNetworked baby in GameObject.FindObjectsOfType<BabyNetworked>())
            {

                if (!baby.IsMenuBaby)
                {
                    NetworkServer.Destroy(baby.gameObject);
                }
            }
            //BabyIDs.Clear();
            //Babies.Clear();
            score = 0;

            blarpSound.Play();
            RpcPlayBlarpSound();

            BlarpEventManager.EnableHighScoreBalls();
            RpcEnalbeHighScoreBalls();

            BlarpEventManager.SetHighScoreBalls(Game.current.highScore);
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

            GameObject.Find("Room").GetComponent<RoomNetworked>().active = false;
            RpcSetRoomActive(false);
            resizeRoomLarge();
            RpcResizeRoom(true);

            menuBaby.transform.position = new Vector3(0, 1, 2);
            menuBaby.GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
            menuBaby.SetActive(true);
            RpcSetMenuBabyActive(true);

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
            BlarpEventManager.DisableHighScoreBalls();
            RpcDisableHighScoreBalls();


            resizeRoom();
            RpcResizeRoom(false);
            GameObject.Find("Room").GetComponent<RoomNetworked>().active = true;
            RpcSetRoomActive(true);


            int hitSoundIndex = Random.Range(0, MommaHitAudioList.Length);
            MommaHitSound.clip = MommaHitAudioList[hitSoundIndex];
            MommaHitSound.Play();
            RpcPlayMommaHitAudio(hitSoundIndex);




            //LearningBlarp.GetComponent<MeshRenderer>().enabled = false;
            //LearningBlarp.GetComponent<Collider>().enabled = false;

            menuBaby.SetActive(false);
            RpcSetMenuBabyActive(false);
            menuBaby.transform.position = new Vector3(0, 1, 2);
            menuBaby.GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);

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
    /*void UpdateBabyForces(GameObject go)
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

    }*/

    //-------Delegates and Callbacks-----------
    /*void OnBabyIDListUpdated(SyncListUInt.Operation op, int index, uint oldId, uint newId)
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
    }*/

    private void PlayNewBabyAudio(GameObject newBaby)
    {
        AudioSource audioSource = newBaby.GetComponent<AudioSource>();
        audioSource.clip = AudioList[(int)score % 4];
        audioSource.pitch = .25f * Mathf.Pow(2, (int)(score / 4));
        audioSource.volume = Mathf.Pow(.7f, (int)(score / 4));
        audioSource.Play();
    }

    private void SetHighScoreBallPitch(GameObject hsBall, int index)
    {
        //int rand = Random.Range(0, 1);
        hsBall.GetComponent<AudioSource>().clip = HighScoreAudioList[index];
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

    /*[ClientRpc]
    void RpcClearBabies()
    {
        Babies.Clear();
    }*/

    [ClientRpc]
    void RpcResizeRoom(bool resizeLarge)
    {
        if (!resizeLarge)
        {
            resizeRoom();
        }
        else
        {
            resizeRoomLarge();
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
