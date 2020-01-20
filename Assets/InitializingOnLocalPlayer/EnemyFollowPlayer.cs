using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class EnemyFollowPlayer : NetworkBehaviour
{
    public float speed = 1.0f;
    private HealthPercent _playerHealthPercent;


    private Transform target;


    private void Awake()
    {
        PlayerUpdated(ClientScene.localPlayer);

        // Listen for additional local player updates
        LocalPlayerAnnouncer.OnLocalPlayerUpdated += PlayerUpdated;

    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (_playerHealthPercent == null)
        {
            Debug.Log("playerHealthPercent is null.");
            return;
        }

        float step = speed * Time.deltaTime; // calculate distance to move



        if (_playerHealthPercent.CurrentPercent < 0.5)
        {
            transform.position = Vector3.MoveTowards(transform.position, target.position, step);

        }

    }

    private void OnDestroy()
    {
        LocalPlayerAnnouncer.OnLocalPlayerUpdated -= PlayerUpdated;
    }

    /// <summary>
    /// Received when the local player is updated.
    /// </summary>
    /// <param name="localPlayer"></param>
    private void PlayerUpdated(NetworkIdentity localPlayer)
    {
        if (localPlayer != null)
        {
            _playerHealthPercent = localPlayer.GetComponent<HealthPercent>();
            target = localPlayer.gameObject.transform;
        }

        this.enabled = (localPlayer != null);
    }


}
