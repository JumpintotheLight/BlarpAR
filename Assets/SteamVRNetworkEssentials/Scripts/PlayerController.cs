using UnityEngine;
//
using Mirror;


public class PlayerController : NetworkBehaviour
{
	public GameObject bulletPrefab;
	private MouseLook mouseLook;
	public Transform bulletSpawn;


	public override void OnStartLocalPlayer()
	{
		GetComponent<Renderer>().material.color = Color.blue;

		// attach camera to player.. 3rd person view..
		Camera.main.transform.parent = transform;
		Camera.main.transform.localPosition = new Vector3 (0, 1.33f, -0.69f);
		Camera.main.transform.localRotation = Quaternion.Euler (6.31f, 0, 0);

		mouseLook = new MouseLook ();
		mouseLook.Init (transform, Camera.main.transform);
	}

	void Update()
	{
		
		if (!isLocalPlayer)
		{
			return;
		}


		// non vr player input here
		var x = Input.GetAxis ("Horizontal") * Time.deltaTime * 3.0f;
		var z = Input.GetAxis ("Vertical") * Time.deltaTime * 3.0f;

		transform.Translate (x, 0, z);

		mouseLook.LookRotation (transform, Camera.main.transform);

		transform.rotation = Camera.main.transform.rotation;

		// common input here
		if (Input.GetKeyDown(KeyCode.Space))
		{
			CmdFire();
		}
	}


	// This [Command] code is called on the Client …
	// … but it is run on the Server!
	[Command]
	protected void CmdFire()
	{
		// Create the Bullet from the Bullet Prefab
		var bullet = (GameObject)Instantiate(
			bulletPrefab,
			bulletSpawn.position,
			bulletSpawn.rotation);

		// Add velocity to the bullet
		bullet.GetComponent<Rigidbody>().velocity = bullet.transform.forward * 6;

		// Spawn the bullet on the Clients
		NetworkServer.Spawn(bullet);

		// Destroy the bullet after 2 seconds
		Destroy(bullet, 2.0f);
	}
}