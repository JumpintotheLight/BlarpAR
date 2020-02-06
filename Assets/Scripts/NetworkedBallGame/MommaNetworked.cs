using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class MommaNetworked : MonoBehaviour
{
    public GameObject score;

    // Use this for initialization
    void Start()
    {
        score = transform.Find("Score").gameObject;//.GetComponent<TextMesh>();
        score.GetComponent<MeshRenderer>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(Camera.main.gameObject.transform);
    }


    void OnCollisionEnter(Collision c)
    {
        if (c.gameObject.name.Contains("Baby"))
        {
            GameObject.FindObjectOfType<NetworkedBallGame>().MommaHit(c.gameObject);
        }
    }
}
