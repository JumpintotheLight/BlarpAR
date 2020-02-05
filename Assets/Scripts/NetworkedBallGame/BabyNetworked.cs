using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BabyNetworked : MonoBehaviour
{
    [SerializeField]
    private bool isMenuBaby = false;

    public bool IsMenuBaby
    {
        get { return isMenuBaby; }
    }

    private void OnEnable()
    {
        BlarpEventManager.OnUpdateBabyRenders += UpdateRenders;
        BlarpEventManager.OnUpdateBabyPhysics += UpdatePhysics;
        BlarpEventManager.OnChangeBabySpringJoint += SetConnectedSpringJointBody;
    }

    private void OnDisable()
    {
        BlarpEventManager.OnUpdateBabyRenders -= UpdateRenders;
        BlarpEventManager.OnUpdateBabyPhysics -= UpdatePhysics;
        BlarpEventManager.OnChangeBabySpringJoint -= SetConnectedSpringJointBody;
    }

    private void UpdateRenders(GameObject aPlayer, Vector3 roomSize, Vector4 mommaInfo)
    {
        Vector3 v1 = transform.position - aPlayer.GetComponent<NetworkedPlayer>().GetHandPosition();
        float l = v1.magnitude;

        float w = (1.0f / (1.0f + l)) * (1.0f / (1.0f + l)) * (1.0f / (1.0f + l));

        float lineWidth = w * .05f;

        //Line and trail will need to be set on ecah client.
        LineRenderer r = gameObject.GetComponent<LineRenderer>();
        Material m = r.material;
        r.SetPosition(0, transform.position);
        r.SetPosition(1, aPlayer.GetComponent<NetworkedPlayer>().GetHandPosition());
        r.startWidth = lineWidth;
        r.endWidth = lineWidth;
        r.startColor = Color.red;
        r.endColor = Color.green;
        m.SetVector("startPoint", aPlayer.GetComponent<NetworkedPlayer>().GetHandPosition());
        m.SetVector("endPoint", transform.position);
        m.SetFloat("trigger", aPlayer.GetComponent<NetworkedPlayer>().GetHandTriggerVal());


        m = gameObject.GetComponent<TrailRenderer>().material;
        m.SetVector("_Size", roomSize);
        m.SetVector("_MommaInfo", mommaInfo);

        //Set transform on server only?
        Vector3 v = gameObject.GetComponent<Rigidbody>().velocity;
        transform.LookAt(transform.position + v, Vector3.up);

        v = transform.InverseTransformDirection(v);
        m = gameObject.GetComponent<MeshRenderer>().material;
        m.SetVector("_Velocity", v);
    }

    private void UpdatePhysics(GameObject activePlayer, float triggerVal, Vector3 handVelocity)
    {
        Vector3 v1 = transform.position - activePlayer.transform.position;
        float lV1 = v1.magnitude;
        v1.Normalize();

        Vector3 v = handVelocity;
        float lVel = v.magnitude;
        float dot = Vector3.Dot(v, v1);

        v1 = -.5f * triggerVal * v1 * lVel * (-dot + 1);
        gameObject.GetComponent<Rigidbody>().AddForce(v1);

        SpringJoint sj = gameObject.GetComponent<SpringJoint>();
        sj.spring = 1 * triggerVal;
    }

    private void SetConnectedSpringJointBody(Rigidbody newCB)
    {
        gameObject.GetComponent<SpringJoint>().connectedBody = newCB;
    }
}
