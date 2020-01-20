using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class MommaNetworked : MonoBehaviour
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

        //UpdateMesh();
    }

    /*private void UpdateMesh()
    {
        if(NetworkedBallGame.nBallGame != null)
        {
            float score = NetworkedBallGame.nBallGame.score;

            float base100 = Mathf.Floor(score / 100);
            float base10 = Mathf.Floor((score - (base100 * 100)) / 10);
            float base1 = score - (base10 * 10);

            this.GetComponent<MeshRenderer>().material.SetInt("_Digit1", (int)base1);
            this.GetComponent<MeshRenderer>().material.SetInt("_Digit2", (int)base10);
        }
    }*/

    void OnCollisionEnter(Collision c)
    {
        /*if (isServer && c.gameObject.name.Contains("Baby"))
        {
            NetworkedBallGame.nBallGame.MommaHit(c.gameObject);
        }*/

        if (c.gameObject.name.Contains("Baby"))
        {
            GameObject.FindObjectOfType<NetworkedBallGame>().MommaHit(c.gameObject);
            //NetworkedBallGame.nBallGame.MommaHit(c.gameObject);
        }
    }
}
