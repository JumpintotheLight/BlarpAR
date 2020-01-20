using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ShieldNetworked : NetworkBehaviour
{
    public GameObject shieldObj;

    public Vector3 startScale;
    public Vector3 startPos;

    private Material mat;

    // Use this for initialization
    void Start()
    {
        startScale = shieldObj.transform.localScale;
        startPos = shieldObj.transform.localPosition;
        mat = shieldObj.GetComponent<MeshRenderer>().material;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void UpdateShield(float triggerVal)
    {
        if (isServer)
        {
            shieldObj.transform.localPosition = triggerVal * startPos;

            shieldObj.transform.localScale = triggerVal * startScale;
            mat.SetVector("_Size", shieldObj.transform.localScale);
            RpcUpdateShield(triggerVal);
        }
    }


    [ClientRpc]
    void RpcUpdateShield(float triggerVal)
    {
        shieldObj.transform.localScale = triggerVal * startScale;
        mat.SetVector("_Size", shieldObj.transform.localScale);
    }

}
