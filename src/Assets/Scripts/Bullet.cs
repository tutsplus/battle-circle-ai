using UnityEngine;
using System.Collections.Generic;

[AddComponentMenu("DudeWorld/Bullet")]
public class Bullet : MonoBehaviour {
	
	public int damage = 100;
	public float speed = 500.0f;
	public float range = 9999.0f;
	public float knockbackForce = 0.0f;
	public GameObject boom;
	public DamageEvent data;
	
	private float life = 9999.0f;
	public GameObject owner;
	public int team = 0;
	public bool environmental = false;
	public bool shakesCamera = false;
	public bool penetrates = false;
	public string[] effects;
	
	private float distanceTravelled = 0.0f;
	
	void Start() {
		if(owner == null)
			owner = gameObject;
		
		// this is a horrible hack
		if(speed == 0.0f)
		{
			life = range;
		}
		else if(!environmental)
		{
			// must push the bullet here so that life calculation is correct
			rigidbody.AddForce(transform.forward * speed);
				
			//float fakespeed = speed * 0.02f;
			//life = (range / fakespeed);
			life = (range / speed);
		}
		
		if( shakesCamera )
		{
			Camera.main.SendMessage("DoEffect", 0.1f);
		}
	}
	
	void FixedUpdate()
	{
		if(!environmental)
		{
			life -= Time.fixedDeltaTime;
			if(life <= 0.0)
			{
				OnKill();
			}
		}
	}
	
	void OnCollisionEnter(Collision c)
	{
		OnTriggerEnter(c.collider);
	}
	
	void OnTriggerEnter(Collider other)
	{	
		if(other.isTrigger) return;
		
		if(this.data == null)
			this.data = new DamageEvent(this.owner, damage, knockbackForce, 0, team);
		
		if(this.data.owner == null)
			this.data.owner = gameObject;
		
		if(effects.Length > 0)
		{
			foreach(string effect in effects)
			{
				string[] bits = effect.Split('=');
				this.data.effects[bits[0]] = float.Parse(bits[1]);
			}
		}
		
		this.data.bullet = gameObject;
		this.data.victim = other.gameObject;
		
		if(speed > 0.0f)
			this.data.ranged = true;
		
		other.gameObject.SendMessage("OnShot", this.data, SendMessageOptions.DontRequireReceiver);
		//this.data.owner.BroadcastMessage("OnShotHit", this.data, SendMessageOptions.DontRequireReceiver);
		
		if(!penetrates)
			OnKill();
		
		//if(other.gameObject.GetComponent(Dude) == null)
		//{
		//	OnKill();
		//}
		
	}
	
	void OnKill()
	{
		if(environmental) return;
		
		if(boom != null)
		{
			var boomObj = Instantiate(boom, transform.position, transform.rotation) as GameObject;
		}
		
		if(GetComponentInChildren(typeof(TrailRenderer)) != null)	
			transform.DetachChildren();
		
		if(GetComponentInChildren(typeof(ParticleEmitter)) != null)
		{
			foreach(ParticleEmitter particle in GetComponentsInChildren(typeof(ParticleEmitter)))
			{
				particle.emit = false;
			}
			transform.DetachChildren();
		}
		
		Destroy(gameObject);
	}
}