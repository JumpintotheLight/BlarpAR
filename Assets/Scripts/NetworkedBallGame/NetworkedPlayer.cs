using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Valve.VR;

public class NetworkedPlayer : NetworkBehaviour
{
    [SerializeField]
    private bool isVRPlayer = true;
    public GameObject vrCameraRigPrefab;
    [SyncVar]
    public GameObject hand;
    public GameObject handPrefab;
    [SyncVar]
    public GameObject shield;
    public GameObject shieldPrefab;
    public GameObject networkedBallGamePrefab;
    //public float handTriggerDeadVal = 0.001f;

    private GameObject vrCameraRigInstance;
    private SteamVR_Controller.Device handDevice;
    private SteamVR_Controller.Device shieldDevice;
    //private GameObject handInstance;
    //private GameObject shieldInstance;
    private float handTriggerVal = 0;
    private float shieldTriggerVal = 0;

    private bool isHandTriggerPressed = false;
    private bool hasHandTriggerBeenPressed = false;
    private bool hasHandTriggerBeenReleased = false;

    private bool devicesSet = false;
    [SyncVar]
    public bool isActivePlayer = false;
    private MouseLook mouseLook;


    void Start()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        if (isVRPlayer)
        {
            if (Camera.main.gameObject != null)
            {
                DestroyImmediate(Camera.main.gameObject);
            }


            vrCameraRigInstance = (GameObject)Instantiate(vrCameraRigPrefab, transform.position, transform.rotation);

            //Spawn Hand
            GameObject nh = (GameObject)Instantiate(handPrefab, this.transform.position, Quaternion.identity);
            NetworkServer.Spawn(nh);
            hand = nh;

            //Spawn Shield
            GameObject ns = (GameObject)Instantiate(shieldPrefab, this.transform.position, Quaternion.identity);
            NetworkServer.Spawn(ns);
            shield = ns;

            TryDetectControllers();
        }
        else
        {
            Camera.main.transform.parent = transform;
            Camera.main.transform.localPosition = new Vector3(0, 1.33f, -0.69f);
            Camera.main.transform.localRotation = Quaternion.Euler(6.31f, 0, 0);

            mouseLook = new MouseLook();
            mouseLook.Init(transform, Camera.main.transform);
            devicesSet = true;

            //Spawn Hand
            GameObject nh = (GameObject)Instantiate(handPrefab, this.transform.position, Quaternion.identity);
            NetworkServer.Spawn(nh);
            hand = nh;
            hand.transform.parent = this.transform;

            //Spawn Shield
            GameObject ns = (GameObject)Instantiate(shieldPrefab, this.transform.position + new Vector3(0,0, 0.394f), Quaternion.identity);
            NetworkServer.Spawn(ns);
            shield = ns;
            shield.transform.parent = this.transform;

        }
        

        if (isServer)
        {
            GameObject nbg = (GameObject)Instantiate(networkedBallGamePrefab, Vector3.zero, Quaternion.identity);
            NetworkServer.Spawn(nbg);
            nbg.GetComponent<NetworkedBallGame>().StartSetUp();
        }

        
    }

    void TryDetectControllers()
    {
        var controllers = vrCameraRigInstance.GetComponentsInChildren<SteamVR_TrackedObject>();
        if (controllers != null && controllers.Length == 2 && controllers[0] != null && controllers[1] != null)
        {
            GameObject handController = vrCameraRigInstance.transform.Find("Controller (right)").gameObject;
            hand.GetComponent<copyPosition>().objectToCopy = handController;
            handDevice = SteamVR_Controller.Input((int)handController.GetComponent<SteamVR_TrackedObject>().index);

            GameObject shieldController = vrCameraRigInstance.transform.Find("Controller (left)").gameObject;
            shield.GetComponent<copyPosition>().objectToCopy = shieldController;
            shieldDevice = SteamVR_Controller.Input((int)shieldController.GetComponent<SteamVR_TrackedObject>().index);

            devicesSet = true;
        }
        else
        {
            Invoke("TryDetectControllers", 2f);
        }
    }

    void Update()
    {
        if(isLocalPlayer && !isVRPlayer)
        {
            var x = Input.GetAxis("Horizontal") * Time.deltaTime * 3.0f;
            var z = Input.GetAxis("Vertical") * Time.deltaTime * 3.0f;

            transform.Translate(x, 0, z);

            mouseLook.LookRotation(transform, Camera.main.transform);

            transform.rotation = Camera.main.transform.rotation;
        }
    }

    void FixedUpdate()
    {
        if (isLocalPlayer && devicesSet)
        {
            QueryControllers();
            
            CmdUpdateHandTrigger(handTriggerVal);
            CmdUpdateShield(shieldTriggerVal);

            if (hasHandTriggerBeenPressed)
            {
                CmdSetActivePlayer();
            }
            if (hasHandTriggerBeenReleased)
            {
                if (isActivePlayer)
                {
                    CmdUnlockActivePlayer();
                }   
            }

        }
    }

    private void QueryControllers()
    {
        hasHandTriggerBeenPressed = false;
        hasHandTriggerBeenReleased = false;

        if (isVRPlayer)
        { 
            //handTriggerVal = handDevice.GetTouchDown(SteamVR_Controller.ButtonMask.Trigger) ? 1f : 0;
            handTriggerVal = handDevice.GetState().rAxis1.x;
            Debug.Log("HTRIGGER: " + handTriggerVal);
            //shieldTriggerVal = shieldDevice.GetTouchDown(SteamVR_Controller.ButtonMask.Trigger) ? 1f : 0;
            shieldTriggerVal = shieldDevice.GetState().rAxis1.x;
        }
        else
        {
            handTriggerVal = Input.GetKey(KeyCode.Space) ? 1f : 0f;
            shieldTriggerVal = Input.GetKey(KeyCode.LeftShift) ? 1f : 0f;
        }

        if (!isHandTriggerPressed)
        {
            if (handTriggerVal > 0)
            {
                isHandTriggerPressed = true;
                hasHandTriggerBeenPressed = true;
            }
        }
        else
        {
            if (handTriggerVal <= 0)
            {
                isHandTriggerPressed = false;
                hasHandTriggerBeenReleased = true;
            }
        }
    }

    public bool IsVrPlayer
    {
        get { return isVRPlayer; }
    }

    public HandScriptNetworked GetHandScript()
    {
       return hand.GetComponent<HandScriptNetworked>();
    }

    public GameObject GetHand()
    {
        return hand;
    }

    public Vector3 GetHandPosition()
    {
        return hand.transform.position;
    }

    public float GetHandTriggerVal()
    {
        return handTriggerVal;
    }

    public Vector3 GetHandVelocity()
    {
        return hand.GetComponent<HandScriptNetworked>().Velocity;
    }

    [Command]
    void CmdUpdateHandTrigger(float tV)
    {
        hand.GetComponent<HandScriptNetworked>().TriggerVal = tV;
    }

    [Command]
    void CmdUpdateShield(float tV)
    {
        shield.GetComponent<ShieldNetworked>().UpdateShield(tV);
    }

    [Command]
    void CmdSetActivePlayer()
    {
        GameObject.FindObjectOfType<NetworkedBallGame>().SetActivePlayerID(netId);
        //NetworkedBallGame.nBallGame.SetActivePlayerID(netId);
    }

    [Command]
    void CmdUnlockActivePlayer()
    {
        GameObject.FindObjectOfType<NetworkedBallGame>().UnlockActivePlayer();
        //NetworkedBallGame.nBallGame.UnlockActivePlayer();
        isActivePlayer = false;
    }
    

}
