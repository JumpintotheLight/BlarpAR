using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkedTutorialButton : MonoBehaviour
{
    public NetworkedBallGame ballGame;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter(Collider c)
    {

        print(c);
        print(c.gameObject.tag);
        if (c.gameObject.tag == "Hand")
        {

            print("YUP YUP");

            //ballGame.startTutorial();

        }
    }
}
