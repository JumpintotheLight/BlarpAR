using UnityEngine;
//
using Mirror;

public class Health : NetworkBehaviour 
{
	public const int maxHealth = 100;
	public bool destroyOnDeath;
	private NetworkStartPosition[] spawnPoints;

	[SyncVar(hook = "OnChangeHealth")]
	public int currentHealth = maxHealth;

	void Start ()
	{
		if (isLocalPlayer)
		{
			spawnPoints = FindObjectsOfType<NetworkStartPosition>();
		}
	}

	public void TakeDamage(int amount)
	{
		if (!isServer)
		{
			return;
		}
		currentHealth -= amount;

		if (currentHealth <= 0)
		{
			if (destroyOnDeath) {
				Destroy (gameObject);
			} else {
				currentHealth = maxHealth;
				RpcRespawn ();
			}
		}
	}

	void OnChangeHealth (int currentHealth)
	{
		GetComponentInChildren<TextMesh> ().text = currentHealth.ToString();
	}

	[ClientRpc]
	void RpcRespawn()
	{
		if (isLocalPlayer) {
			// Set the spawn point to origin as a default value
			Vector3 spawnPoint = Vector3.zero;

			// If there is a spawn point array and the array is not empty, pick one at random
			if (spawnPoints != null && spawnPoints.Length > 0)
			{
				spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)].transform.position;
			}

			// Set the player’s position to the chosen spawn point
			//transform.p
			Transform ancestor = GetAncestor(transform);
			ancestor.position = spawnPoint;
		}
	}

	Transform GetAncestor(Transform child)
	{
		Transform currentObject = child;
		while (currentObject.parent != null) {
			currentObject = currentObject.parent;
		}
		return currentObject;
	}
}