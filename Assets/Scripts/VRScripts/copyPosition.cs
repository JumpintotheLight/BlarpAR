using UnityEngine;
using System.Collections;

public class copyPosition : MonoBehaviour {
  public GameObject objectToCopy;

  // Use this for initialization
  void Start () {
  
  }
  
  // Update is called once per frame
  void Update () {

        if(objectToCopy != null)
        {
            gameObject.transform.position = objectToCopy.transform.position;
            gameObject.transform.rotation = objectToCopy.transform.rotation;
        }
  }
}
