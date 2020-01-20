using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System; 


// Place on Player
public class LocalPlayerAnnouncer : NetworkBehaviour
{

    #region Public
    /// <summary>
    ///  Dispatched when the local player changes, providign the new localPlayer
    /// </summary>
    public static event Action<NetworkIdentity> OnLocalPlayerUpdated;
    #endregion


    #region Start/Update
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    #endregion

    #region Start/Destroy 

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        OnLocalPlayerUpdated?.Invoke(base.netIdentity);
    }

    private void OnDestroy()
    {
        if (base.isLocalPlayer)
            OnLocalPlayerUpdated?.Invoke(null); 
    }

    #endregion


    //private void OnEnable()
    //{

    //}

    //private void OnDisable()
    //{

    //}
}
