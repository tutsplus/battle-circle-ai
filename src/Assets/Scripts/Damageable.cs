using UnityEngine;
using System.Collections;
using System.Collections.Generic;


[AddComponentMenu("DudeWorld/Damageable")]
[RequireComponent(typeof(Dude))]
public class Damageable : MonoBehaviour
{
	public int maxHealth = 100;
	public int team = 0;
	public float armor = 0.0f;
	public float invincibilityDuration = 1.0f;
	
	public GameObject hitEffect;
	public GameObject blockEffect;
	
	public bool invincibleWhenHit = false;
	public bool directionalBlock = false;
	
	private int health = 100;
	private float backstabAngle = 0.4f;
	private bool invincible = false;
	
	private Dude dude;
	private SwordzDude swordz;
	
	public int Health
	{
		get; private set;
	}
	
	// last thing that attacked me
	public GameObject attacker
	{
		get; private set;
	}
	
	// what killed me... pretty much always null unless I just got killed
	public GameObject killer
	{
		get; private set;
	}
	
	void Awake()
	{
		health = maxHealth;
		dude = GetComponent<Dude>();
		swordz = GetComponent<SwordzDude>();
	}
	
	public void OnShot(DamageEvent d)
	{
		// heal bullet!
		if(d.damage < 0)
		{
			// heal my own team, do nothing to other people
			if(d.team == team)
			{
				AddHealth(-d.damage);
			}
			return;
		}
		if(d.team == team || invincible)
			return;
		
		Vector3 attackVec = Vector3.zero;
		if(d.owner != null)
		{
			Vector3 ownerPos = d.owner.transform.position;
			// bring them to the same y level as us, to make sure we don't knock them up over terrain 
			ownerPos.y = transform.position.y;
			attackVec = (transform.position - ownerPos).normalized;
		}
			
		if(d.knockback > 0.0f)
		{
			Vector3 forceVec;
			// for melee weapons, knockback should be from the attacker
			/*
			if(d.owner != null)
				forceVec = (transform.position - d.owner.transform.position).normalized;
			else
				// for ranged weapons, knockback should come form the bullet
				forceVec = (transform.position - d. .position).normalized;
			*/
			dude.AddForce(attackVec * d.knockback);
		}
		
		// certain effects bypass blocking, so we apply them now
		/*
		var bypassEffects = new string[]
		{
			"OnWeakened"
		};
		if(d.effects.Count > 0)
		{
			foreach(var effect in bypassEffects)
			{
				if(d.effects.ContainsKey(effect))
				{
					gameObject.SendMessage(effect, d.effects[effect], SendMessageOptions.DontRequireReceiver);
					d.effects.Remove(effect);
				}
			}
		}
		*/
		
		// calculate angle-affected stuff
		var angle = Vector3.Dot(attackVec, transform.forward);
		
		// see if I'm blocking in the correct direction
		if(dude.blocking)
		{
			if(directionalBlock && angle > 0.0f)
			{
				// not blocking in the right direction!
			}
			else
			{
				Instantiate(blockEffect, transform.position, transform.rotation);
				gameObject.BroadcastMessage("OnShotBlocked", d);
				return;
			}
		}
		
		// start calculating bonus damage, etc. //////////////////////////
		/*
		if(d.damage > 0)
		{
			// backstab
			if(swordz.status.stunned || angle > backstabAngle)
			{
				d.damage *= 2;
				Debug.Log("CRITICAL HIT! " + d.damage + " - " + angle);
			}
		}
		*/

		d.damage -= (int)(d.damage * armor);
		
		// no more damage calculation below this point ///////////////////
		
		// handle armor
		//swordz.armor;
		
		// the hit has officially caused damage, we're spawning an effect ////////
		if(hitEffect != null && d.damage > 0)
		{
			//var blood = Instantiate(hitEffect, Util.flatten(transform.position), transform.rotation) as GameObject;
			var blood = Instantiate(hitEffect, transform.position, transform.rotation) as GameObject;
		}
		
		// calculate inflicted damage... if this is a killing blow we only count damage inflicted to kill
		int inflicted = Mathf.Min(health, d.damage);
		
		attacker = d.owner;
		health -= d.damage;
		if(health <= 0)
		{
			killer = d.owner;
			gameObject.BroadcastMessage("OnKill", killer);
		}
		else if(health > maxHealth)
		{
			health = maxHealth;
		}
		
		// record metrics
		
		// send special effects
		foreach(KeyValuePair<string, object> pair in d.effects)
		{
			gameObject.SendMessage(pair.Key, pair.Value, SendMessageOptions.DontRequireReceiver);
		}
		
		// report to the owner that the hit was successful
		// FIXME: I am screwing with damage here 'cause it's just a lot easier than copying
		d.damage = inflicted;
		if(d.owner != null)
			d.owner.BroadcastMessage("OnShotHit", d, SendMessageOptions.DontRequireReceiver);
		
		//if(!invincibleWhenHit)
		//	gameObject.SendMessage("OnInterrupt");
		
		if(invincibleWhenHit && d.damage > 0 && health > 0)
			StartCoroutine("doInvincible", invincibilityDuration);
		
		if(gameObject.CompareTag("Player") && d.damage > 0)
		{
			/*
			GameObject director = GameObject.FindGameObjectWithTag("Director");
			if(director != null)
				director.SendMessage("OnPlayerHit", d);
			*/
		}
	}
	
	IEnumerator doInvincible(float delay)
	{
		var graphics = transform.Find("graphics");
		
		invincible = true;
		graphics.BroadcastMessage("StartEffect");
		yield return new WaitForSeconds(delay);
		invincible = false;
		graphics.BroadcastMessage("StopEffect");
	}
	
	/*
	public void OnTriggerEnter(Collider other) {
		Debug.Log("oof! I bumped into a " + other.gameObject.name);
	}
	*/
	
	public int GetHealth()
	{
		return health;
	}
	
	public void AddHealth(int hp)
	{
		/*
		// heals!
		var heals = Resources.Load("Effects/effect_heal");
		Instantiate(heals, transform.position, transform.rotation);
		*/
		
		health += hp;
		if(health > maxHealth)
			health = maxHealth;
	}
}