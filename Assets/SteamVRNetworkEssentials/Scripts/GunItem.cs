using UnityEngine;
//
using Mirror;

public class GunItem : NetworkBehaviour, IUsable {
    public GameObject projectilePrefab;
    private Transform barrel;
    public float speed = 6f;

    void Start()
    {
        barrel = transform.Find("Barrel");
    }

	public void StartUsing(uint handId)
    {
        var projectile = (GameObject)Instantiate(projectilePrefab, barrel.position, barrel.rotation);
        //projectile.GetComponent<Rigidbody>().AddForce(barrel.forward * speed, ForceMode.VelocityChange);  // is asynchronously and won't work here
        projectile.GetComponent<Rigidbody>().velocity = barrel.forward * speed;

        NetworkServer.Spawn(projectile);
    }
	public void StopUsing(uint handId)
	{
	}
}
