using UnityEngine;
using System.Collections.Generic;

public class DamageEvent
{
	public GameObject bullet;
	public GameObject owner;
	public int damage = 0;
	public float knockback = 0.0f;
	public int penetration = 0;
	public int team = 0;
	public GameObject victim;  // this is only not null if the event actually hit something
	public bool ranged = false;
	public Dictionary<string,object> effects;
	
	public DamageEvent()
	{
		this.effects = new Dictionary<string, object>();
	}
	
	public DamageEvent(GameObject owner, int damage, float knockback, int penetration, int team)
		: this()
	{
		this.owner = owner;
		this.damage = damage;
		this.knockback = knockback;
		this.penetration = penetration;
		this.team = team;
	}
	
	static public int CalculateDamage(int damage, int penetration, int armor)
	{
		int total = 0;
		
		if(damage > 0) {
			// potential damage formula
			//var totaldamage = amount - Mathf.Max(0, armor - penetration);
			total = Mathf.Max(1, damage - Mathf.Max(0, armor - penetration) );
		}
		
		return total;
	}
}

