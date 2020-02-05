using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighScoreBallNetworked : MonoBehaviour
{
    private float[] octaves;
    // Use this for initialization
    void Start()
    {
        octaves = new float[4];
        octaves[0] = .5f;
        octaves[1] = 1;
        octaves[2] = .75f;
        octaves[3] = 1.25f;

    }

    void OnEnable()
    {
        BlarpEventManager.OnSetHighScoreBalls += SetHighScore;
        BlarpEventManager.OnEnableHighScoreBalls += EnableColliderAndMesh;
        BlarpEventManager.OnDisableHighScoreBalls += DisableColliderAndMesh;
    }

    void OnDisable()
    {
        BlarpEventManager.OnSetHighScoreBalls -= SetHighScore;
        BlarpEventManager.OnEnableHighScoreBalls -= EnableColliderAndMesh;
        BlarpEventManager.OnDisableHighScoreBalls -= DisableColliderAndMesh;
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(Camera.main.gameObject.transform);
    }

    void OnCollisionEnter(Collision c)
    {

        GetComponent<AudioSource>().pitch = octaves[Random.Range(0, octaves.Length)];
        GetComponent<AudioSource>().Play();

    }

    private void SetHighScore(float newScore)
    {
        float base100 = Mathf.Floor(newScore / 100);
        float base10 = Mathf.Floor((newScore - (base100 * 100)) / 10);
        float base1 = newScore - (base10 * 10);
        //print( base1 );

        gameObject.GetComponent<MeshRenderer>().material.SetInt("_Digit1", (int)base1);
        gameObject.GetComponent<MeshRenderer>().material.SetInt("_Digit2", (int)base10);   
    }

    private void EnableColliderAndMesh()
    {
        gameObject.GetComponent<MeshRenderer>().enabled = true;
        gameObject.GetComponent<Collider>().enabled = true;
    }

    private void DisableColliderAndMesh()
    {
        gameObject.GetComponent<MeshRenderer>().enabled = false;
        gameObject.GetComponent<Collider>().enabled = false;
    }
}
