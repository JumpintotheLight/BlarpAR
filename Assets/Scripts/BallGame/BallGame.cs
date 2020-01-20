using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class BallGame : MonoBehaviour {

  public List<GameObject> Babies = new List<GameObject>(); //List of all babies in scene
  public GameObject BabyPrefab; //The balls that the player pulls
  public GameObject MommaPrefab; //The ball that the player needs to ram the others into
  public GameObject HighScorePrefab; //
  public GameObject   Momma;
  public GameObject StartButton;
  public GameObject StartButtonPrefab;

  public GameObject Hand;
  public GameObject Shield;
  public GameObject Controller;
  public GameObject ShieldController;
  public GameObject ScoreText;
  public GameObject Platform;
  public GameObject CameraRig;
  public GameObject Title;

  public GameObject tutButton;
  public GameObject tutorialButtonPrefab;

  public GameObject TitlePrefab;

  public Material PlatformMat;

  public AudioClip blarpClip;
  public AudioClip restartClip;
  private GameObject empty;

  private GameObject LearningBlarp;
  private GameObject tutTrigger;
  private GameObject tutPartial;
  private GameObject tutHit1;
  private GameObject tutHit2;
  private GameObject tutHit3;
  private GameObject tutStart;
  private GameObject tutShield;

  public Shader PlatformShader;
  public GameObject Room;
  public float score;

  private bool triggerDown;
  private AudioSource restartSound;
  private AudioSource blarpSound;
  private SteamVR_PlayArea PlayArea;

  private Vector3 v1;
  private Vector3 v2;
  private Vector3 roomSize;
  private Vector4 MommaInfo;

  public AudioClip[] AudioList;
  public AudioClip[] HighScoreAudioList;
  public AudioClip[] MommaHitAudioList;
  public List<AudioSource> AudioSources = new List<AudioSource>();

  private AudioSource MommaHitSound;


  private GameObject[] highScoreBalls;


  private bool tutorialFinished;
  private bool triggerPulled;
  private bool shieldTriggerPulled;
  private bool readyToPlay;
  private float notDeadVal;

	// Use this for initialization
	void Start () {

    SaveLoad.Load();


    if( Game.current == null){
      Game.current = new Game();
    }



    EventManager.OnTriggerDown += OnTriggerDown;
    EventManager.OnTriggerUp += OnTriggerUp;
    //EventManager.StayTrigger += StayTrigger;

    restartSound = gameObject.AddComponent<AudioSource>();
    blarpSound = gameObject.AddComponent<AudioSource>();

    restartSound.clip = restartClip;
    blarpSound.clip = blarpClip;

    empty = new GameObject();

    PlayArea = CameraRig.GetComponent<SteamVR_PlayArea>();

        Debug.Log("" + PlayArea.vertices[0]);
    Vector3 v = PlayArea.vertices[0];
    Platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
    Platform.transform.localScale = new Vector3( Mathf.Abs( v.x )  * 1.5f ,1.0f ,  Mathf.Abs( v.z ) * 1.5f);
    Platform.transform.position = new Vector3( 0f , -0.49f , 0f );

    Material m = PlatformMat; //new Material( PlatformShader );

    Platform.GetComponent<MeshRenderer>().material = m ;
    //m = PlatformMat;
    Platform.GetComponent<MeshRenderer>().material.SetVector("_Size" , Platform.transform.localScale ); 
    Platform.GetComponent<MeshRenderer>().material.SetFloat("_Learning" , 0 );

    Vector3 tPos =  new Vector3( 0f , 1.5f , -3f );
    Title = (GameObject) Instantiate( TitlePrefab, tPos , new Quaternion());
    Title.transform.localEulerAngles = new Vector3(0,180,0);
    //m = TitleMat;
    Title.GetComponent<MeshRenderer>().material.SetVector("_Scale" , Title.transform.localScale );


    AudioList =  new AudioClip[]{ (AudioClip)Resources.Load("Audio/hydra/TipHit1"),
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


    for( var i = 0; i < AudioList.Length; i ++ ){
      print(AudioList[i]);

       AudioSource audioSource = gameObject.AddComponent<AudioSource>();
       audioSource.clip = AudioList[i];
       audioSource.Play();
       audioSource.volume = 0;
       AudioSources.Add( audioSource );
    }



    highScoreBalls = new GameObject[10];
    for( int i = 0; i < 10; i++){


      float rad = Random.Range( 4 , 10 );
      Vector3 p =  Random.onUnitSphere * rad;


      highScoreBalls[i] = (GameObject) Instantiate( HighScorePrefab, p , new Quaternion());
      highScoreBalls[i].transform.localScale = new Vector3( rad/4 , rad / 4 , rad /4);

      int rand = Random.Range( 0 , 1 );
      highScoreBalls[i].GetComponent<AudioSource>().clip = HighScoreAudioList[ rand ];
      highScoreBalls[i].GetComponent<AudioSource>().pitch = .5f;

    }

    setHighScoreBalls( Game.current.highScore );

    Momma = (GameObject) Instantiate( MommaPrefab, new Vector3() , new Quaternion());
    Momma.GetComponent<Momma>().BallGameObj = transform.gameObject;
    Momma.transform.position = new Vector3( 0 , 3 , 0 );

    StartButton = (GameObject) Instantiate( StartButtonPrefab, new Vector3() , new Quaternion());
    StartButton.GetComponent<StartButton>().BallGameObj = transform.gameObject;
    StartButton.transform.position = new Vector3(0 , 1  , -Platform.transform.localScale.z * .55f );

    


    //HandL.GetComponent<HandScript>().BallGameObj = transform.gameObject;
    //HandR.GetComponent<HandScript>().BallGameObj = transform.gameObject;
    ScoreText = Momma.transform.Find("Score").gameObject;//.GetComponent<TextMesh>();
    score = 0;

        // Make a new object
    LearningBlarp = (GameObject) Instantiate( BabyPrefab, new Vector3() , new Quaternion());
    LearningBlarp.transform.position = new Vector3( 0 , 1 , -2);
    LearningBlarp.GetComponent<SpringJoint>().connectedBody = Hand.GetComponent<Rigidbody>();
    //AudioSource audioSource = LearningBlarp.GetComponent<AudioSource>();
    LearningBlarp.GetComponent<Rigidbody>().drag = .7f - (score / 100);
    LearningBlarp.GetComponent<Rigidbody>().mass = .2f - (score / 340);
    LearningBlarp.GetComponent<Rigidbody>().angularDrag = 200;
    LearningBlarp.GetComponent<Rigidbody>().freezeRotation = true;
    //LearningBlarp.GetComponent<Collider>().enabled = false;
    LearningBlarp.transform.localScale = LearningBlarp.transform.localScale * (2.0f - (score/30));
    LearningBlarp.GetComponent<MeshRenderer>().material.SetFloat("_Score" , (float)score );
    LearningBlarp.GetComponent<MeshRenderer>().material.SetFloat("_Learning" , 0 );
    LearningBlarp.GetComponent<TrailRenderer>().enabled = false;


    setTutorialObjects();

    if( tutorialFinished == false ){ 
      startTutorial(); 
    }else{
      addTutorialButton();
    }

    restart( transform.gameObject );



    

	}

   

  void setHighScoreBalls( float theScore ){

    float base100 = Mathf.Floor( theScore / 100 );
    float base10  = Mathf.Floor( (theScore - ( base100 * 100 )) / 10 );
    float base1   = theScore - (base10 * 10);
    //print( base1 );

    for( var i = 0; i < 10; i++ ){
      highScoreBalls[i].GetComponent<MeshRenderer>().material.SetInt( "_Digit1" , (int)base1 );
      highScoreBalls[i].GetComponent<MeshRenderer>().material.SetInt( "_Digit2" , (int)base10 );
    }

  }

  void removeHighScoreBalls(){

    for( var i = 0; i < 10; i++ ){
      highScoreBalls[i].GetComponent<MeshRenderer>().enabled = false;
      highScoreBalls[i].GetComponent<Collider>().enabled = false;
    }


  }

  void addHighScoreBalls(){

    for( var i = 0; i < 10; i++ ){
      highScoreBalls[i].GetComponent<MeshRenderer>().enabled = true;
      highScoreBalls[i].GetComponent<Collider>().enabled = true;
    }


  }

  void addTutorialButton(){
     // tutButton.GetComponent<MeshRenderer>().enabled = false;
     // tutButton.GetComponent<Collider>().enabled = false;
      tutButton.SetActive( true );
  }

  void removeTutorialButton(){
      //tutButton.GetComponent<MeshRenderer>().enabled = true;;
      //tutButton.GetComponent<Collider>().enabled = true;

      tutButton.SetActive( false );
  }

  void setTutorialObjects(){

    tutButton  = (GameObject) Instantiate( tutorialButtonPrefab, new Vector3(Platform.transform.localScale.x*.5f,2,Platform.transform.localScale.z*.5f) , new Quaternion()); 
    tutButton.GetComponent<tutorialButton>().ballGame = this;
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

  public void startTutorial(){

    removeHighScoreBalls();

    removeTutorialButton();

       Title.GetComponent<MeshRenderer>().enabled = false;
      //Title.GetComponent<BoxCollider>().enabled = false;


    MommaHitSound.clip = MommaHitAudioList[ Random.Range( 0 , MommaHitAudioList.Length)];
    MommaHitSound.Play();

    Game.current.finishedTutorial = false;
    tutorialFinished = false;
    triggerPulled = false;
    shieldTriggerPulled = false;
    readyToPlay = false;
    notDeadVal = -5;

    LearningBlarp.GetComponent<MeshRenderer>().material.SetFloat("_Learning" , 1 );
    Platform.GetComponent<MeshRenderer>().material.SetFloat("_Learning" , 1 );
    Title.GetComponent<MeshRenderer>().material.SetFloat("_Learning" , 1 );
    //Shield.transform.Find("Shield").GetComponent<MeshRenderer>().material.SetFloat("_Learning" , 1 );
    
    StartButton.SetActive( false );

    tutTrigger.GetComponent<MeshRenderer>().enabled = true;
    tutShield.GetComponent<MeshRenderer>().enabled = true;
 

  }

  void tutorialHandHit(){

    tutHit1.GetComponent<MeshRenderer>().enabled =  false ;
    tutHit2.GetComponent<MeshRenderer>().enabled =  false ;
    tutHit3.GetComponent<MeshRenderer>().enabled =  false ;
    tutPartial.GetComponent<MeshRenderer>().enabled =  false ;


    notDeadVal = 0.0f;

    float v = Random.Range( 0.0001f , 3.99999f );

    if( v <= 1.0f){
      tutHit1.GetComponent<MeshRenderer>().enabled = true ;
    }else if( 1.0f < v && v <= 2.0f){
      tutHit2.GetComponent<MeshRenderer>().enabled = true ;
    }else if( 2.0f < v && v <= 3.0f){
      tutHit3.GetComponent<MeshRenderer>().enabled = true ;
    }else if( 3.0f < v && v <= 4.0f){
      tutPartial.GetComponent<MeshRenderer>().enabled = true ;
    }   

  }

  void finishTutorial(){
    tutStart.GetComponent<MeshRenderer>().enabled = false;
    tutShield.GetComponent<MeshRenderer>().enabled =  false;;
    LearningBlarp.GetComponent<MeshRenderer>().material.SetFloat("_Learning" , 0 );
    Platform.GetComponent<MeshRenderer>().material.SetFloat("_Learning" , 0 );
    Title.GetComponent<MeshRenderer>().material.SetFloat("_Learning" , 0 );
    Shield.transform.Find("Shield").GetComponent<MeshRenderer>().material.SetFloat("_Learning" , 0 );

    tutorialFinished = true;
    Game.current.finishedTutorial = true;
    SaveLoad.Save();
    addHighScoreBalls();


    MommaHitSound.clip = MommaHitAudioList[ Random.Range( 0 , MommaHitAudioList.Length)];
    MommaHitSound.Play();

  }

  void getReadyToPlay(){
    
    readyToPlay = true;

    tutHit1.GetComponent<MeshRenderer>().enabled =  false;
    tutHit2.GetComponent<MeshRenderer>().enabled =  false;
    tutHit3.GetComponent<MeshRenderer>().enabled =  false;
    tutPartial.GetComponent<MeshRenderer>().enabled =  false;

    tutStart.GetComponent<MeshRenderer>().enabled =  true ;


    MommaHitSound.clip = MommaHitAudioList[ Random.Range( 0 , MommaHitAudioList.Length)];
    MommaHitSound.Play();

    StartButton.SetActive( true );
  }
	
  void UpdateTutorial(){

    tutButton.transform.LookAt( new Vector3( 0 , 2 , 0 ) );
    if( tutorialFinished == false ){
      if( triggerPulled == false && Controller.GetComponent<controllerInfo>().triggerVal > 0.8 ){
        triggerPulled = true;
        notDeadVal = 0.0f;

        MommaHitSound.clip = MommaHitAudioList[ Random.Range( 0 , MommaHitAudioList.Length)];
        MommaHitSound.Play();
        tutTrigger.GetComponent<MeshRenderer>().enabled =  false;
      } 

      if( shieldTriggerPulled == false && ShieldController.GetComponent<controllerInfo>().triggerVal > 0.8 ){
        shieldTriggerPulled = true;
        notDeadVal = 0.0f;

        MommaHitSound.clip = MommaHitAudioList[ Random.Range( 0 , MommaHitAudioList.Length)];
        MommaHitSound.Play();
        tutShield.GetComponent<MeshRenderer>().enabled =  false;;
      } 
    }

    notDeadVal += .001f;

    if( notDeadVal > 1.0f && readyToPlay == false && triggerPulled == true ){

      getReadyToPlay();

    }

  }
	// Update is called once per frame
	void Update () {

    UpdateTutorial();

    float base100 = Mathf.Floor( score / 100 );
    float base10  = Mathf.Floor( (score - ( base100 * 100 )) / 10 );
    float base1   = score - (base10 * 10);
    //print( base1 );

    Momma.GetComponent<MeshRenderer>().material.SetInt( "_Digit1" , (int)base1 );
    Momma.GetComponent<MeshRenderer>().material.SetInt( "_Digit2" , (int)base10 );

    //if( triggerDown == false ){
      foreach( GameObject baby in Babies ){



        v1 = baby.transform.position - Hand.transform.position;
        float l = v1.magnitude;

        float w = (1.0f / (1.0f + l)) * (1.0f / (1.0f + l))  * (1.0f / (1.0f + l));

        float lineWidth = w * .05f;

         LineRenderer r = baby.GetComponent<LineRenderer>();
         Material m = r.material;
          r.SetPosition( 0 , baby.transform.position );
          r.SetPosition( 1 , Hand.transform.position );
            //r.SetWidth(lineWidth, lineWidth) ;
          r.startWidth = lineWidth;
            r.endWidth = lineWidth;
          //r.SetWidth(1, lineWidth);
          //r.SetColors( Color.red , Color.green );
            r.startColor = Color.red;
            r.endColor = Color.green;
          m.SetVector( "startPoint" , Hand.transform.position );
          m.SetVector( "endPoint" , baby.transform.position );
          m.SetFloat( "trigger" , Controller.GetComponent<controllerInfo>().triggerVal );
         

        m = baby.GetComponent<TrailRenderer>().material;
        m.SetVector("_Size" , roomSize );
        m.SetVector("_MommaInfo" , MommaInfo );

        Vector3 v = baby.GetComponent<Rigidbody>().velocity;
        baby.transform.LookAt( baby.transform.position + v , Vector3.up );
        v = baby.transform.InverseTransformDirection( v );
        m = baby.GetComponent<MeshRenderer>().material;
        m.SetVector( "_Velocity" , v );




      }

      if( LearningBlarp != null ){

        v1 = LearningBlarp.transform.position - Hand.transform.position;
        float l = v1.magnitude;

        float w = (1.0f / (1.0f + l)) * (1.0f / (1.0f + l))  * (1.0f / (1.0f + l));

        float lineWidth = w * .05f;

         LineRenderer r = LearningBlarp.GetComponent<LineRenderer>();
         Material m = r.material;
          r.SetPosition( 0 , LearningBlarp.transform.position );
          r.SetPosition( 1 , Hand.transform.position );
          //r.SetWidth(lineWidth, lineWidth) ;
          r.startWidth = lineWidth;
          r.endWidth = lineWidth;
          //r.SetWidth(1, lineWidth);
          //r.SetColors( Color.red , Color.green );
          r.startColor = Color.red;
          r.endColor = Color.green;
          m.SetVector( "startPoint" , Hand.transform.position );
          m.SetVector( "endPoint" , LearningBlarp.transform.position );
          m.SetFloat( "trigger" , Controller.GetComponent<controllerInfo>().triggerVal );
         

        m = LearningBlarp.GetComponent<TrailRenderer>().material;
        m.SetVector("_Size" , roomSize );
        m.SetVector("_MommaInfo" , MommaInfo );

        Vector3 v = LearningBlarp.GetComponent<Rigidbody>().velocity;
        LearningBlarp.transform.LookAt( LearningBlarp.transform.position + v , Vector3.up );
        v = LearningBlarp.transform.InverseTransformDirection( v );
        m = LearningBlarp.GetComponent<MeshRenderer>().material;
        m.SetVector( "_Velocity" , v );

      }
    //}

      //Hand.transform.localScale = new Vector3()

    
	
	}

  void FixedUpdate(){

    UpdateBabyForces( Hand );
  }

  public void MommaHit( GameObject goHit ){

    // Make a new object
    GameObject go = (GameObject) Instantiate( BabyPrefab, new Vector3() , new Quaternion());
    go.transform.position = goHit.transform.position;
    go.GetComponent<SpringJoint>().connectedBody = Hand.GetComponent<Rigidbody>();
    AudioSource audioSource = go.GetComponent<AudioSource>();
    go.GetComponent<Rigidbody>().drag = .7f - (score / 100);
    go.GetComponent<Rigidbody>().mass = .2f - (score / 340);
    go.GetComponent<Rigidbody>().angularDrag = 200;
    go.GetComponent<Rigidbody>().freezeRotation = true;
    go.transform.localScale = go.transform.localScale * (2.0f - (score/30));
    go.GetComponent<MeshRenderer>().material.SetFloat("_Score" , (float)score );
  
    //audioSource.clip = AudioList[(int)score];
    audioSource.clip = AudioList[(int)score%4];
    audioSource.pitch = .25f * Mathf.Pow(2 , (int)(score /4 )); 
    audioSource.volume =  Mathf.Pow(.7f , (int)(score /4 )); 
    audioSource.Play();

//    go.GetComponent<SpringJoint>().enabled = false; connectedBody = HandR.GetComponent<Rigidbody>();
    Babies.Add( go );
    
    resizeRoom();
    moveMomma();

    MommaHitSound.clip = MommaHitAudioList[ Random.Range( 0 , MommaHitAudioList.Length)];
    MommaHitSound.Play();


    score ++;
    Game.current.lastScore = score;

    if( score > Game.current.highScore ){ Game.current.highScore = score; }

    ScoreText.GetComponent<TextMesh>().text = score.ToString();
    



  }

  float getSizeFromScore(){
    return 3.0f + score / 3;
  }

  public void moveMomma(){

    float size = getSizeFromScore();


    
    Momma.transform.position = new Vector3( Random.Range(  size/4 , size/2 ), 
                                            Random.Range(   0 + .15f , size/2 + .15f ),
                                            Random.Range(  size/4 , size/2 ));

    float xSign = Random.value < .5? 1 : -1;
    float zSign = Random.value < .5? 1 : -1;
    Vector3 v = new Vector3( 
      xSign,
      1,
      zSign
    );

    Momma.transform.position = Vector3.Scale(Momma.transform.position , v);



    Momma.transform.localScale = new Vector3( .3f , .3f , .3f );

    MommaInfo = new Vector4(
      Momma.transform.position.x,
      Momma.transform.position.y,
      Momma.transform.position.z,
      Momma.transform.localScale.x
    );

    Momma.GetComponent<AudioSource>().Play();

    waitMomma();
  }


  void waitMomma(){ 

    Momma.SetActive( false );
    StartCoroutine (WaitToMakeMommaReal());
    
  }

   IEnumerator WaitToMakeMommaReal(){
       while (true) {
          //print("unwaited");
           yield return new WaitForSeconds(2.0f);
          //print("WAITERD");
           makeMommaReal();
            //return false;
            yield return false;

        }
    }

  void makeMommaReal(){
    Momma.SetActive( true );
    Momma.GetComponent<AudioSource>().Play();
  }

  void resizeRoom(){

    float size = getSizeFromScore();
    
    Room.transform.localScale = new Vector3( size , size/2 + .6f , size );
    Room.transform.position = new Vector3( 0 , size/4 + .3f , 0 );


    roomSize = Room.transform.localScale;

  }

  void resizeRoomLarge(){

    float size = getSizeFromScore();
    
    Room.transform.localScale = new Vector3( 10000 , 10000 , 100000 );
    Room.transform.position = new Vector3( 0 , 500 , 0 );


    roomSize = Room.transform.localScale;

  }


  public void HandHit( GameObject handHit ){
    //print("fUh!");

    if( tutorialFinished == false && readyToPlay == false ){
      tutorialHandHit();
    }

    //if( score > 1 ){

     restart( handHit );

    //}


  }

  void restart(GameObject handHit ){
     
      SaveLoad.Save();

      foreach( GameObject baby in Babies ){

        
        Destroy(baby);


      }

      Babies.Clear();
      score = 0;
      blarpSound.Play();

      setHighScoreBalls( Game.current.highScore );
     

      LearningBlarp.SetActive(true); 
      LearningBlarp.transform.position = new Vector3( 0 , 1 , 2);//transform.position = new Vector3( 0 , 1 , -2);
      LearningBlarp.GetComponent<Rigidbody>().velocity = new Vector3( 0 , 0, 0);
//      LearningBlarp.GetComponent<MeshRenderer>().enabled = true;

      if( tutorialFinished == true ){ addTutorialButton(); addHighScoreBalls();  
      Title.GetComponent<MeshRenderer>().enabled = true;};
//      Title.GetComponent<BoxCollider>().enabled = false;};

      StartButton.GetComponent<MeshRenderer>().enabled = true;
      StartButton.GetComponent<BoxCollider>().enabled = true;
     

      Momma.GetComponent<MeshRenderer>().enabled = true;
      Momma.GetComponent<Collider>().enabled = false;

      Momma.transform.position = new Vector3(0, -1000000.0f,0);
      
      Room.GetComponent<Room>().active = false;
      resizeRoomLarge();
      //startGame( handHit );
  }

  public void startGame( GameObject go ){

    finishTutorial();
    restartSound.Play();
    removeHighScoreBalls();

  
    resizeRoom();
    Room.GetComponent<Room>().active = true;

    removeTutorialButton();


    MommaHitSound.clip = MommaHitAudioList[ Random.Range( 0 , MommaHitAudioList.Length)];
    MommaHitSound.Play();

   

    
    //LearningBlarp.GetComponent<MeshRenderer>().enabled = false;
    //LearningBlarp.GetComponent<Collider>().enabled = false;

      LearningBlarp.SetActive(false);

    StartButton.GetComponent<MeshRenderer>().enabled = false;
    StartButton.GetComponent<BoxCollider>().enabled = false;
    Momma.GetComponent<MeshRenderer>().enabled = true;

    Title.GetComponent<MeshRenderer>().enabled = false;
    //Title.GetComponent<BoxCollider>().enabled = false;


    float size = getSizeFromScore();
    empty.transform.position = new Vector3( Random.Range(  -size/4 , size/4 ), 
                                            Random.Range(   size/4 + .15f , size/4 + .15f ),
                                            Random.Range(  -size/4 , size/4 ));

    empty.transform.position = new Vector3( 0 ,
                                            1 ,
                                            -size/2 );


    MommaHit( empty );

    //StartButton.transform.position = new Vector3( 0 , 1 , 0 );

  }

  void OnTriggerDown( GameObject go ){

   // triggerDown = true;

  }

  void OnTriggerUp( GameObject go ){

    //triggerDown = false;

  }


    /// <summary>
    /// Updates baby to move towards controller when trigger is down.
    /// </summary>
    /// <param name="go"></param>
  void UpdateBabyForces( GameObject go ){

    float triggerVal = Controller.GetComponent<controllerInfo>().triggerVal;
    if( triggerVal > 0 ){ triggerDown = true; }else{ triggerDown = false; }

    //print( Controller.GetComponent<controllerInfo>().velocity );

    foreach( GameObject baby in Babies ){

      v1 = baby.transform.position - go.transform.position;
      float lV1 = v1.magnitude;
      v1.Normalize();
      

      Vector3 v = Controller.GetComponent<controllerInfo>().velocity;
      float lVel = v.magnitude;
      float dot = Vector3.Dot( v , v1 );

      v1 = -.5f * triggerVal * v1 * lVel * ( -dot + 1 );
      baby.GetComponent<Rigidbody>().AddForce( v1 );

      SpringJoint sj = baby.GetComponent<SpringJoint>();
      sj.spring = 1 * triggerVal;


      float w = (1.0f / (1.0f + lV1)) * (1.0f / (1.0f + lV1));

      float lineWidth = w * .15f;

      
      Color c = new Color( w , w , w );





    }

    //For tutorial use
    //print( LearningBlarp);
    if( LearningBlarp != null ){
      //print("YYUP");

      v1 = LearningBlarp.transform.position - go.transform.position;
      float lV1 = v1.magnitude;
      v1.Normalize();
      

      Vector3 v = Controller.GetComponent<controllerInfo>().velocity;
      float lVel = v.magnitude;
      float dot = Vector3.Dot( v , v1 );

      v1 = -.5f * triggerVal * v1 * lVel * ( -dot + 1 );
      LearningBlarp.GetComponent<Rigidbody>().AddForce( v1 );

      SpringJoint sj = LearningBlarp.GetComponent<SpringJoint>();
      sj.spring = 1 * triggerVal;


      float w = (1.0f / (1.0f + lV1)) * (1.0f / (1.0f + lV1));

      float lineWidth = w * .15f;

      
      Color c = new Color( w , w , w );


    }

  }
}
