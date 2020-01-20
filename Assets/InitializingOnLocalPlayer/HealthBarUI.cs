using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Mirror;

public class HealthBarUI : MonoBehaviour
{


    [Tooltip("Image used as fill to show players health percent.")]
    [SerializeField]
    public Image _healthFillbar;


    private HealthPercent _playerHealthPercent;

    public void SetPlayerHealthPercent(HealthPercent hp) { }

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
        _healthFillbar.fillAmount = _playerHealthPercent.CurrentPercent;
       // Debug.Log("playerHealthPercent is " + _playerHealthPercent.CurrentPercent);

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
            _playerHealthPercent = localPlayer.GetComponent<HealthPercent>();

        this.enabled = (localPlayer != null);  
    }
}
 