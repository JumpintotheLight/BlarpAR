using UnityEngine;
using System.Collections;
//
using Mirror;


public class Bullet : NetworkBehaviour {
	public float lifeTime = 20f;

    // UNET uses uint's but Mirror uses uint
    //public uint projectileSourceId;

    [SyncVar]
    public uint projectileSourceId;

    void Awake()
	{
		if (!isServer)
			return;
		Invoke ("DestroyMe", lifeTime);
	}
	void DestroyMe()
	{
		NetworkServer.Destroy (gameObject);
	}
	void OnCollisionEnter(Collision collision)
	{
		if (!isServer)
			return;

        if (collision.gameObject.tag == "Weapon")
            return;

		NetworkServer.Destroy(gameObject);
		var hit = collision.gameObject;
		var health = hit.GetComponent<Health>();
		if (health != null)
		{
			health.TakeDamage(10);
		}
	}
}