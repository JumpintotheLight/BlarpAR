using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class TestDriver : NetworkBehaviour
{
    public static TestDriver testDriver;
    

    [SyncVar]
    public GameObject momma;
    public GameObject mommaPrefab;
    public Transform mStartPosition;

    [SyncVar]
    public GameObject colorBall;
    [SyncVar(hook = nameof(SetBallColor))]
    public Color cbColor;
    public GameObject cBPrefab;
    public Transform cbStartPosition;

    [SyncVar(hook =nameof(UpdateMomma))]
    public int score = 0;

    readonly SyncListGO babies = new SyncListGO();
    public GameObject babyPrefab;

    readonly SyncListFloat floatTester = new SyncListFloat();

    private void Awake()
    {
        if (testDriver != null)
        {
            if (testDriver != this)
            {
                Debug.Log("TD Destroyed");
                GameObject.Destroy(this.gameObject);
            }
        }
        testDriver = this;
        GameObject.DontDestroyOnLoad(this.gameObject);
    }

    

    void Start()
    {
        babies.Callback += OnBabiesListUpdated;
        if (isServer)
        {
            GameObject newM = (GameObject)Instantiate(mommaPrefab, mStartPosition.position, mStartPosition.rotation);
            NetworkServer.Spawn(newM);
            momma = newM;

            GameObject newCB = (GameObject)Instantiate(cBPrefab, cbStartPosition.position, cbStartPosition.rotation);
            NetworkServer.Spawn(newCB);
            colorBall = newCB;
            SetBallColor(Color.red);
            cbColor = Color.red;

            for(int i = 0; i < 5; i++)
            {
                floatTester.Add(0);
            }
        }
    }

   

    // Update is called once per frame
    void Update()
    {
        #if UNITY_EDITOR
        UpdateMomma(score);
        #endif

    }


    [Server]
    public void AddPoint()
    {
        score += 1;
        UpdateMomma(score);
        AddBaby();
    }

    private void AddBaby()
    {
        GameObject nB = (GameObject)Instantiate(babyPrefab, new Vector3(Random.Range(-5f, 5f), Random.Range(-5f, 5f), Random.Range(-5f, 5f)), Quaternion.identity);
        Debug.Log("Baby Instantiated");
        NetworkServer.Spawn(nB);
        Debug.Log("Baby Spawned");
        babies.Add(nB);
        Debug.Log("Baby added to List");
        //RpcPrintAddBaby(babies.Count - 1);
    }

    void OnBabiesListUpdated(SyncListGO.Operation op, int index, GameObject baby)
    {
        switch (op)
        {
            case SyncListGO.Operation.OP_ADD:
                Debug.Log("Index of new baby is " + index);
                break;
            default:
                break;
        }
    }


    void UpdateMomma(int newScore)
    {
        //Debug.Log("Updating Momma ball");
        float base100 = Mathf.Floor(newScore / 100);
        float base10 = Mathf.Floor((newScore - (base100 * 100)) / 10);
        float base1 = newScore - (base10 * 10);
        //print( base1 );

        momma.GetComponent<MeshRenderer>().material.SetInt("_Digit1", (int)base1);
        momma.GetComponent<MeshRenderer>().material.SetInt("_Digit2", (int)base10);


    }

    void SetBallColor(Color nColor)
    {
        colorBall.GetComponent<MeshRenderer>().material.color = nColor;
    }

    [Server]
    public void ShuffleFloatList()
    {

    }
    

    [Server]
    public void SetRandomColor()
    {
        Color rc = Random.ColorHSV();
        colorBall.GetComponent<MeshRenderer>().material.color = rc;
        cbColor = rc;
    }

    [Server]
    public void SetBallActive()
    {
        colorBall.SetActive(!colorBall.activeSelf);
    }

    [ClientRpc]
    void RpcPrintAddBaby(int nBId)
    {
        if (!isServer)
        {
            Debug.Log("New Baby found?: " + babies[nBId] != null);
        }
    }


}
