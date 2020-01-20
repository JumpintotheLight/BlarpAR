using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;


// This is the wrong way to do things!!!
// This should be on Player 
public class FindAndSetupObjects : NetworkBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //public override void OnStartLocalPlayer()
    //{
    //    base.OnStartLocalPlayer();


    //    // DO NOT USE FIND METHODS!

    //    HealthPercent hp = GetComponent<HealthPercent>();
    //    HealthBarUI meter = FindObjectOfType<HealthBarUI>();
    //    if (meter != null)
    //        meter.SetPlayerHealthPercent(hp);

    //    // Tell enemies player exist so they know when to follow
    //    EnemyFollowPlayer[] enemies = FindObjectsOfType<EnemyFollowPlayer>();
    //    foreach (EnemyFollowPlayer enemy in enemies)
    //        enemy.SetPlayerHealthPercent(hp);
         
    //}
}
