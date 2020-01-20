using UnityEngine;
using System.Collections;
//
using Mirror;

public enum ObjectSnap
{
    Grip,
    Exact
}

public class InteractableObject : NetworkBehaviour
{

    public ObjectSnap snapType;
    public bool isUsable;
	public bool isGrabbable;

	//[SyncVar]
	//public uint touchingControllerId;

	[ClientRpc]
	public void RpcAttachToHand(uint handId)
    {
        var hand = ClientScene.FindLocalObject(handId);
        if (hand == null)
            return;
        AttachToHand(hand);
    }

    // this should be run on Client & Server!
    public void AttachToHand(GameObject hand)
    {
        //var controller = hand.transform.parent;
        var attachpoint = hand.transform.Find("Attachpoint");
        switch (snapType)
        {
            case ObjectSnap.Exact:
                transform.parent = attachpoint.transform;
                break;
            case ObjectSnap.Grip:
                Helper.AttachAtGrip(attachpoint, transform);
                break;
        }
        GetComponent<Rigidbody>().isKinematic = true;
        GetComponent<NetworkTransform>().enabled = false;
    }
    
    [ClientRpc]
	public void RpcDetachFromHand(Vector3 currentHolderVelocity)
	{
        DetachFromHand(currentHolderVelocity);
    }

    public void DetachFromHand(Vector3 currentHolderVelocity)
    {
        transform.parent = null;
        var rigidbodyOfObject = GetComponent<Rigidbody>();
        rigidbodyOfObject.isKinematic = false;
        rigidbodyOfObject.velocity = currentHolderVelocity*1.5f;
        GetComponent<NetworkTransform>().enabled = true;
    }
}