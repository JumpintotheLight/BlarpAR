using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkedSingleton : MonoBehaviour
{

    public static NetworkedSingleton singleton;


    //Set singleton
    private void Awake()
    {
        if (singleton != null)
        {
            if (singleton != this)
            {
                Debug.Log("nBG Destroyed");
                GameObject.Destroy(this.gameObject);
                return;
            }
        }
        singleton = this;
        GameObject.DontDestroyOnLoad(this.gameObject);
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
