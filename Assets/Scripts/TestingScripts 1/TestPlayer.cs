using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class TestPlayer : NetworkBehaviour
{

    private void OnConnectedToServer()
    {
        //CmdGetFields();
    }

    // Update is called once per frame
    void Update()
    {
        if (isLocalPlayer)
        {
            
            if (Input.GetKeyDown(KeyCode.M))
            {
                Debug.Log("'M' has been pressed.");
                CmdAddPoint();
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                Debug.Log("'R' has been pressed.");
                CmdChangeBallColor();
            }
            if (Input.GetKeyDown(KeyCode.A))
            {
                CmdSetBallActive();
            }
        }

       
    }

    

    [Command]
    void CmdAddPoint()
    {
        TestDriver.testDriver.AddPoint();
    }

    [Command]
    void CmdChangeBallColor()
    {
        TestDriver.testDriver.SetRandomColor();
    }

    [Command]
    void CmdSetBallActive()
    {
        TestDriver.testDriver.SetBallActive();
    }

    /*[Command]
    void CmdGetFields()
    {
        TestDriver.testDriver.SendFields(connectionToClient);
    }*/
    
}
