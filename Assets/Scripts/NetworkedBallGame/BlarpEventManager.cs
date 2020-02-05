using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlarpEventManager : MonoBehaviour
{
    public delegate void SetHighScoreBallsDelegate(float newScore);
    public static event SetHighScoreBallsDelegate OnSetHighScoreBalls;

    public delegate void EnableHighScoreBallsDelegate();
    public static event EnableHighScoreBallsDelegate OnEnableHighScoreBalls;

    public delegate void DisableHighScoreBallsDelegate();
    public static event DisableHighScoreBallsDelegate OnDisableHighScoreBalls;

    public delegate void UpdateBabyRendersDelegate(GameObject activePlayer, Vector3 roomSize, Vector4 mommaInfo);
    public static event UpdateBabyRendersDelegate OnUpdateBabyRenders;

    public delegate void UpdateBabyPhysicsDelegate(GameObject activePlayer, float aPlayerTriggerVal, Vector3 aPlayerVelocity);
    public static event UpdateBabyPhysicsDelegate OnUpdateBabyPhysics;

    public delegate void ChangeBabySpringJointDelegate(Rigidbody newConnectedBody);
    public static event ChangeBabySpringJointDelegate OnChangeBabySpringJoint;

    public static void SetHighScoreBalls(float newScore)
    {
        if(OnSetHighScoreBalls != null)
        {
            OnSetHighScoreBalls(newScore);
        }
    }

    public static void EnableHighScoreBalls()
    {
        if (OnEnableHighScoreBalls != null)
        {
            OnEnableHighScoreBalls();
        }
    }

    public static void DisableHighScoreBalls()
    {
        if (OnDisableHighScoreBalls != null)
        {
            OnDisableHighScoreBalls();
        }
    }

    public static void UpdateBabyRenders(GameObject activePlayer, Vector3 roomSize, Vector4 mommaInfo)
    {
        if(OnUpdateBabyRenders != null)
        {
            OnUpdateBabyRenders(activePlayer,roomSize,mommaInfo);
        }
    }

    public static void UpdateBabyPhysics(GameObject activePlayer, float triggerVal, Vector3 velocity)
    {
        if(OnUpdateBabyPhysics != null)
        {
            OnUpdateBabyPhysics(activePlayer, triggerVal, velocity);
        }
    }

    public static void ChangeBabySpringJoint(Rigidbody newConnectedBody)
    {
        if(OnChangeBabySpringJoint != null)
        {
            OnChangeBabySpringJoint(newConnectedBody);
        }
    }
}
