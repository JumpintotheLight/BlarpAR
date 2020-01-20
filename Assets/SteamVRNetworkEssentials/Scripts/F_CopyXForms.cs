using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class F_CopyXForms : MonoBehaviour {

  	public Transform target;
	public bool copyCamera;
	public bool copyPosition;
	public bool copyRotation;
	public bool copyScale;
    //public bool findCam = true;
	// Use this for initialization
	void Start () {
//        if (copyCamera || target == null)
//        {
//            //if(findCam)
//                target = Camera.main.transform;
//        }
	}

	public void ForceUpdate(){
		if (target != null)
		{
			if (copyCamera && target != Camera.main.transform)
				target = Camera.main.transform;
			if(copyPosition)
				this.transform.position = target.position;
			if (copyRotation)
				this.transform.rotation = target.rotation;
			if (copyScale)
				this.transform.localScale = target.lossyScale;
		}
		else if (target == null && copyCamera)
		{
			target = Camera.main.transform;
		}
	}
	// Update is called once per frame
	void Update () {
		ForceUpdate ();
    }
}
