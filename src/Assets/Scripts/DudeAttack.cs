using UnityEngine;
using System.Collections;

[System.Serializable]
public class SwingEvent
{
	public AnimationClip animation;
	public GameObject prefab;
	public float rate;
	public float step;
	//public int damage;
	//public int knockback;
	public DamageEvent damage;
	public float reach = 0.0f;
}

[AddComponentMenu("DudeWorld/Dude Attack")]
public class DudeAttack : MonoBehaviour
{
	public int damage = 10;
	public int knockback = 10;
	public SwingEvent[] comboChain;
	
	public bool debug = false;
	public AudioClip swingSound;
	
	public Transform hotspot;
	
	private Dude owner;
	private SwordzDude ownerSwordz;
	private Damageable damageable;
	private float cooldown = 0.0f;
	private float comboMultiplier = 0.2f;
	private int swingNumber = 0;
	
	void Start()
	{
		owner = GetComponent<Dude>();
		ownerSwordz = GetComponent<SwordzDude>();
		damageable = GetComponent<Damageable>();

		if(hotspot == null)
		{
			hotspot = transform;
		}
	}
	
	void FixedUpdate()
	{
		if(cooldown > 0.0f)
		{
			cooldown -= Time.fixedDeltaTime;
		}
	}
		
	public bool CanFire()
	{
		if(cooldown > 0.0f) return false;
		return true;
	}
		
	public SwingEvent GetSwingEvent(int swing)
	{
		var swingEvent = comboChain[swing];
		
		var d = new DamageEvent();
		d.damage = CalculateDamage(damage, swing);
		d.knockback = knockback;
		swingEvent.damage = d;

		return swingEvent;
	}
		
	public bool OnFire()
	{
		bool success = OnFire(GetSwingEvent(swingNumber));

		swingNumber += 1;
		if(swingNumber >= comboChain.Length)
		{
			swingNumber = 0;
		}

		return success;
	}
	
	public bool OnFire(SwingEvent swingEvent)
	{
		if(cooldown > 0.0f) return false;
		
		cooldown = swingEvent.rate;
		
		StartCoroutine(DoSwing(swingEvent));
		
		return true;
	}
	
	void OnSwingBegan()
	{
	}
		
	private IEnumerator DoSwing(SwingEvent swing)
	{
		gameObject.SendMessage("OnSwingBegan");
		
		float warmupTime = swing.rate / 3.0f;
		float recoveryTime = warmupTime * 2.0f; // FIXME: this shouldn't be hardcoded! makes combat boring!
		
		yield return new WaitForSeconds(warmupTime);
		
		var bulletPos = hotspot.position;
		
		if(swing.reach != 0.0f)
			bulletPos += owner.transform.forward * swing.reach;
		
		var clone = Instantiate(swing.prefab, bulletPos, hotspot.rotation) as GameObject;
		var b = clone.GetComponent(typeof(Bullet)) as Bullet;
		if(b != null)
		{
			swing.damage.team = damageable.team;
			swing.damage.owner = owner.gameObject;
			b.data = swing.damage;
			Physics.IgnoreCollision(clone.collider, owner.collider);
		}
		
		// turn on debug to see swing colliders
		if(debug)
		{
			clone.renderer.enabled = true;
		}
		
		yield return new WaitForSeconds(recoveryTime);
		
		gameObject.SendMessage("OnSwingEnded");
	}
		
	void OnSwingEnded()
	{
	}
	
	public IEnumerator OnDoSwing()
	{
		yield return StartCoroutine("doSwing", null);
	}
		
	/*
	public void OnShotBlocked(DamageEvent d)
	{
		blockPower -= d.damage;
		
		if( blockPower <= 0 )
		{
			owner.SendMessage("OnBlockEnd");
			var shatterEffect = Resources.Load("Effects/effect_blockshatter") as GameObject;
			Instantiate(shatterEffect, owner.transform.position, Quaternion.identity);
			if(owner.CompareTag("Player"))
				owner.SendMessage("OnDisarm");
			else
			{
				// in effect this makes it take longer to recharge up to 0
				blockPower = -(maxBlockPower/2);
				owner.SendMessage("OnStun", stunPower);
			}
		}
	}
	*/

	void OnDisable()
	{
		StopAllCoroutines();
	}
	
	public int CalculateDamage(int baseDamage, int swingNumber)
	{
		return baseDamage;// + (int)Mathf.Ceil((swingNumber-1) * comboMultiplier * baseDamage);
	}
	
	static public float RateToSpeed(float rate)
	{
		//return (int)((1.0f / rate) * 100.0f);
		return 1.0f / rate;
	}

}
