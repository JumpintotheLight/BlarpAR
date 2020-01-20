using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class MommaNetworked : NetworkBehaviour
{
    public GameObject Score;

    // Use this for initialization
    void Start()
    {
        Score = transform.Find("Score").gameObject;//.GetComponent<TextMesh>();
        Score.GetComponent<MeshRenderer>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {

        transform.LookAt(Camera.main.gameObject.transform);

    }


    void OnCollisionEnter(Collision c)
    {
        if (isServer && c.gameObject.name.Contains("Baby"))
        {
            NetworkedBallGame.nBallGame.MommaHit(c.gameObject);
        }
    }
}
