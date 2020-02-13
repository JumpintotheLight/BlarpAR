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
    public GameObject hand;
    public GameObject handPrefab;
    public GameObject shield;
    public GameObject shieldPrefab;
    public GameObject networkedBallGamePrefab;

    private GameObject vrCameraRigInstance;
    private SteamVR_Controller.Device handDevice;
    private SteamVR_Controller.Device shieldDevice;
    [SyncVar]
    private float handTriggerVal = 0;
    private float shieldTriggerVal = 0;
    [SyncVar]
    private Vector3 handVelocity = Vector3.zero;

    private bool isHandTriggerPressed = false;
    private bool hasHandTriggerBeenPressed = false;
    private bool hasHandTriggerBeenReleased = false;

    private bool devicesSet = false;
    [SyncVar]
    public bool isActivePlayer = false;
    private MouseLook mouseLook;

    private float hTV = 0;


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

            hand.GetComponent<copyPosition>().enabled = true;
            shield.GetComponent<copyPosition>().enabled = true;
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


            shield.transform.position = this.transform.position + new Vector3(0, 0, 0.394f);
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
        if (isLocalPlayer && !isVRPlayer)
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
            handVelocity = hand.GetComponent<Rigidbody>().velocity;

            CmdUpdateShield(shieldTriggerVal);

            if (hasHandTriggerBeenPressed)
            {
                if (isActivePlayer)
                {
                    CmdLockActivePlayer();
                }
                else
                {
                    CmdSetActivePlayer();
                }
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
            hTV = handDevice.GetState().rAxis1.x;
            shieldTriggerVal = shieldDevice.GetState().rAxis1.x;
        }
        else
        {
            hTV = Input.GetKey(KeyCode.Space) ? 1f : 0f;
            shieldTriggerVal = Input.GetKey(KeyCode.LeftShift) ? 1f : 0f;
        }

        CmdUpdateHandValues(hTV, hand.GetComponent<Rigidbody>().velocity);

        if (!isHandTriggerPressed)
        {
            if (hTV > 0)
            {
                isHandTriggerPressed = true;
                hasHandTriggerBeenPressed = true;
            }
        }
        else
        {
            if (hTV <= 0)
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
        return handVelocity;
    }

    public void HandHit(GameObject other)
    {
        if (isServer)
        {
            GameObject.FindObjectOfType<NetworkedBallGame>().HandHit(other);
        }
    }

    public void MoveMomma()
    {
        if (isServer)
        {
            GameObject.FindObjectOfType<NetworkedBallGame>().MoveMomma();
        }
    }


    [Command]
    void CmdUpdateShield(float tV)
    {
        //Debug.Log("S.Trigger = " + tV.ToString());
        shield.GetComponent<ShieldNetworked>().UpdateShield(tV);
        RpcUpdateShield(tV);
    }

    [ClientRpc]
    void RpcUpdateShield(float tV)
    {
        shield.GetComponent<ShieldNetworked>().UpdateShield(tV);
    }

    [Command]
    void CmdUpdateHandValues(float handTrig, Vector3 handVel)
    {
        handTriggerVal = handTrig;
        handVelocity = handVel;
    }

    [Command]
    void CmdSetActivePlayer()
    {
        GameObject.FindObjectOfType<NetworkedBallGame>().SetActivePlayerID(netId);
    }

    [Command]
    void CmdLockActivePlayer()
    {
        GameObject.FindObjectOfType<NetworkedBallGame>().LockActivePlayer();
    }

    [Command]
    void CmdUnlockActivePlayer()
    {
        GameObject.FindObjectOfType<NetworkedBallGame>().UnlockActivePlayer();
    }


}
