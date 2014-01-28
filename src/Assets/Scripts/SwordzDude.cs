using UnityEngine;
using System.Collections;

public class DudeState {
	public bool dodging = false;
	public bool dodgingRecovery = false;
	public bool blocking = false;
	public bool attacking = false;
	public bool attackingRecovery = false;
	public bool stunned = false;
	public bool running = false;
	public bool unbalanced = false;
	
	public void Reset()
	{
		this.dodging = false;
		this.dodgingRecovery = false;
		this.blocking = false;
		this.attacking = false;
		this.attackingRecovery = false;
		this.stunned = false;
		this.running = false;
		this.unbalanced = false;
	}
}


[AddComponentMenu("DudeWorld/Swordz Dude")]
public class SwordzDude : MonoBehaviour {
	
	public GameObject baseAbilities;
	public float maxStamina = 30.0f;
	public Transform animated;
	public Transform weaponHand;
	public Transform offHand;
	public float followUpWindow = 0.2f;
	public bool telegraphAttacks = false;
	public bool cannotBeStunned = false;
	public float stunResistance = 0.0f;
	public GameObject telegraphDecal;
	public float telegraphRate = 0.6f;
	public DudeState status = new DudeState();
	
	private Dude dude;
	private DudeController controller;
	private DudeAttack attackChain;
	private float followUpTimer = 0.0f;
	private float attackCooldown = 0.0f;
	private float attackDelay = 0.0f; // disabled 'cause it kinda sucks
	private float stamina;
	
	private float attackCancelWindow = 0.25f;
	private const float attackAnimationRatio = 1.0f; // magic number, shouldn't change= 
	private const float defaultStun = 2.0f;
	
	private float blockStartTime = 0.0f;
	private Transform blockDecal;
	private Transform chargeDecal;
	private bool disabled = false;
	private bool cancelable = false; // in a state where actions can cancel into others
	private Transform head;
	private Transform hotspot;
	private Transform mount;
	private Transform activeTelegraph = null;
	private Transform activeTelegraphCircle = null;
	private GameObject circlePrefab;
	
	private Vector3 attachPos = Vector3.zero;
	private Quaternion attachRot = Quaternion.identity;
	private bool alwaysFollowUp = false;

	private Color originalTelegraphColor;
	
	public int swingNumber
	{
		get; private set;
	}
	
	public bool followUpQueued
	{
		get; private set;
	}
	
	void Awake()
	{	
		dude = gameObject.GetComponent<Dude>();
		attackChain = GetComponent<DudeAttack>();
		mount = weaponHand;//weaponHand.Find("weaponMount");
		hotspot = transform.Find("weapon_hotspot");
		
		swingNumber = 0;
		followUpQueued = false;
	}
	
	void Start ()
	{
		controller = gameObject.GetComponent(typeof(DudeController)) as DudeController;
		blockDecal = transform.Find("blockDecal");
		chargeDecal = transform.Find("chargeDecal");
		if(mount == null)
		{
			Debug.Log(gameObject.name + " is missing its mount hand!");
		}
		
		circlePrefab = Resources.Load("Effects/effect_telegraphCircle") as GameObject;
		
		//if(gameObject.CompareTag("EnemyMob"))
		//	alwaysFollowUp = true;

		attachRot.eulerAngles = new Vector3(0.0f, -180.0f, 30.0f);
		
		if(blockDecal != null)
			blockDecal.active = false;
		if(chargeDecal != null)
			chargeDecal.active = false;
		
		stamina = maxStamina;
		head = animated.Find("torso/head");

		if(telegraphAttacks && telegraphDecal != null)
		{
			var clone = Instantiate(telegraphDecal, hotspot.transform.position, hotspot.transform.rotation) as GameObject;
			clone.transform.parent = transform;
			activeTelegraph = clone.transform;

			/*
			var circle = Instantiate(circlePrefab, transform.position, transform.rotation) as GameObject;
			circle.transform.parent = transform;
			activeTelegraphCircle = circle.transform;
			*/

			// save original material for resetting later
			originalTelegraphColor = activeTelegraph.GetComponentInChildren<MeshRenderer>().sharedMaterial.GetColor("_TintColor");

			activeTelegraph.gameObject.SetActive(false);
			//activeTelegraphCircle.gameObject.SetActive(false);
		}
	}
	
	void FixedUpdate () {
		if(status.dodging && !status.dodgingRecovery)
		{
			//var finalMove = transform.forward  * dodgeSpeed;
			//dude.RawMovement(finalMove, false);
		}
		
		if(followUpTimer > 0.0f)
			followUpTimer -= Time.fixedDeltaTime;
		
		if(attackCooldown > 0.0f)
			attackCooldown -= Time.fixedDeltaTime;
		
		var staminaRecovery = 1.0f;
		if(stamina < maxStamina)
		{
			stamina += Time.fixedDeltaTime * staminaRecovery;
			if(stamina > maxStamina)
				stamina = maxStamina;
		}
		/*
		if(status.blocking)
		{
			var scale = (mainWeapon.blockPower / mainWeapon.maxBlockPower) + 0.1f;
			blockDecal.localScale = new Vector3(scale,scale,scale);
		}
		*/
	}

	public void OnShot(DamageEvent d)
	{
		if(dude != null && d.team == dude.team)
			return;
		
		// see if I'm blocking in the correct direction
		/*
		if(blocking)
		{
			Instantiate(blockEffect, Util.flatten(transform.position), transform.rotation);
			gameObject.BroadcastMessage("OnShotBlocked", d);
			return;
		}
		*/
		/*
		if(hitEffect != null)
			Instantiate(hitEffect, Util.flatten(transform.position), transform.rotation);
		*/
		
		// do stun damage
		stamina -= d.knockback;//d.damage;
		if(stamina <= 0)
		{
			//StartCoroutine("OnStun", 3.0f);
		}
	}
	
	public void OnBash()
	{
		/*
		if(disabled || status.dodging || status.attacking) return;
		
		OnBlockEnd();
		StartCoroutine( "doBash" );
		*/
	}
	
	public bool OnAttack()
	{
		// actually just see if I'm disabled or not
		if(dude.blocking || status.stunned || attackCooldown > 0.0f) return false;
		
		if(status.dodging) dude.velocity = Vector3.zero;
		
		int maxSwings = 3;
		
		// in certain states, if attack is pressed,
		// queue up another attack
		if(disabled && !status.stunned && (status.attacking || status.attackingRecovery || status.dodging))// && followUpTimer <= followUpWindow/2)
		{
			if( swingNumber+1 < maxSwings)
				followUpQueued = true;
			else
				followUpQueued = false;
			//dude.Look();
			return false;
		}
		
		if(swingNumber <= maxSwings && alwaysFollowUp)
		{
			followUpQueued = true;
		}
		
		//gameObject.BroadcastMessage("OnFire");
		if( true ) //mainWeapon.CanFire() )
		{
			// ran out of time for follow-up swings, so act like this is the first
			if(followUpTimer <= 0.0f)
			{
				swingNumber = 0;
				followUpTimer = followUpWindow + 0.001f;
			}
			
			// 1st swing
			if(followUpTimer > 0.0f && swingNumber < maxSwings)
			{
				swingNumber += 1;
				SwingEvent swing = attackChain.comboChain[0];
				//SwingEvent swing = new SwingEvent();
				
				// it would be better to move this logic to the weapon
				/*
				if(mainWeapon.lastSwingStuns && swingNumber == maxSwings)
				{
					swing.damage.effects["OnStun"] = mainWeapon.stunPower;
				}
				*/
				StartCoroutine( "doSwing", swing );
				
				if(swingNumber < maxSwings)
				{
					followUpTimer = swing.rate + followUpWindow;
				}
				else
				{
					// final swing -- reset swinging
					swingNumber = 0;
					followUpTimer = 0.0f;
					followUpQueued = false;
					attackCooldown = attackDelay;
				}
				return true;
			}
		}
		
		return false;
	}
	
	public void OnBlock()
	{
		//if(status.attacking || status.dodging || status.running || mainWeapon.blockPower <= 0) return;

		if(status.dodging) 
		{
			dude.velocity = Vector3.zero;
		}
		
		status.blocking = true;
		blockStartTime = Time.time;
		blockDecal.active = true;
		dude.blocking = true;
	}
	
	public void OnBlockEnd()
	{
		if(status.dodging || status.running || status.attacking) return;
		
		status.blocking = false;
		blockStartTime = 0.0f;
		if(blockDecal != null)
			blockDecal.active = false;
		dude.blocking = false;
		//animated.animation["bjorn_block"].normalizedTime = 1.0f;
		//animated.animation["bjorn_block"].speed = -1;
		//animated.animation.Play("bjorn_block");
		
		ResetAnimation();
	}
	
	
	IEnumerator doSwing(SwingEvent swingEvent)
	{
		if(status.dodging)
		{
			//animated.animation.Stop();// Stop("bjorn_dodge");
			//animated.animation.Play("bjorn_idle");
			status.dodging = false;
			// dumb hack because Unity animation system is dumb dumb dumb
			//yield return new WaitForSeconds(0.02f);
		}
		
		/*
		bool stunAttack = (swingEvent.damage.effects.ContainsKey("OnStun"));
		if(stunAttack)
		{
			var fx = Resources.Load("Effects/effect_fire_sword") as GameObject;
			
			var fireEffect = Instantiate(fx, mainWeapon.transform.position, mainWeapon.transform.rotation) as GameObject;
			fireEffect.transform.parent = mainWeapon.transform;
			fireEffect.name = "stunEffect";
		}
		*/
		
		status.attacking = true;
		gameObject.BroadcastMessage("OnDisable");
		
		if(telegraphAttacks && swingNumber <= 1)
		{
			animated.animation.Play(swingEvent.animation.name + "_telegraph");

			activeTelegraph.gameObject.SetActive(true);
			//activeTelegraphCircle.gameObject.SetActive(true);
			//activeTelegraphCircle.localScale = Vector3.one;
			//activeTelegraphCircle.SendMessage("OnTelegraph", telegraphRate * 2.0f);
			yield return new WaitForSeconds( telegraphRate - 0.2f );
			// this is a bit heavy and shouldn't be living here
			foreach(Transform decalChild in activeTelegraph)
			{
				decalChild.renderer.material.SetColor("_TintColor", Color.white);
			}
			status.unbalanced = true;
			yield return new WaitForSeconds( 0.2f );
		}
		
		// if we don't telegraph, set unbalanced for the duration of the attack
		status.unbalanced = true;
		
		// don't remove telegraph until the attack chain is over
		int maxSwings = 1;//mainWeapon.swingPrefabs.Length;
		if(activeTelegraph != null && (swingNumber == maxSwings || maxSwings <= 1))
		{
			activeTelegraph.gameObject.SetActive(false);
			//activeTelegraphCircle.gameObject.SetActive(false);
			foreach(Transform decalChild in activeTelegraph)
			{
				decalChild.renderer.material.SetColor("_TintColor", originalTelegraphColor);
			}
		}

		// step into the attack
		//iTween.MoveTo(gameObject, transform.position + (transform.forward * 10.0f), 0.2f);
		dude.AddForce(transform.forward * swingEvent.step * Time.fixedDeltaTime);
		
		// fire actual weapon

		gameObject.SendMessage("OnFire");
		/*
		if(mainWeapon.swingSound != null)
		{
			float originalPitch = audio.pitch;
			audio.pitch = (1.0f - swingEvent.rate);// + 0.5f;
			//audio.PlayOneShot(mainWeapon.swingSound);
			audio.pitch = originalPitch;
		}
		*/
		// dumb hack because Unity animation system is dumb dumb dumb
		//animated.animation.Stop();
		//animated.animation.Play("bjorn_idle");
		//yield return new WaitForSeconds(0.02f);
		yield return new WaitForEndOfFrame();
		
		//string animName = mainWeapon.swingAnimations[swingNumber - 1].name;
		if(swingEvent.animation != null)
		{
			string animName = swingEvent.animation.name;
			animated.animation[animName].speed = attackAnimationRatio / swingEvent.rate;
			animated.animation.Play(animName, PlayMode.StopAll);//CrossFade(animName);
		}

		yield return new WaitForSeconds( swingEvent.rate * (1.0f - attackCancelWindow) );
		
		if(swingNumber < maxSwings)
		{
			status.attacking = false;
			status.unbalanced = false;
			status.attackingRecovery = true;
		}
		
		yield return new WaitForSeconds( swingEvent.rate * attackCancelWindow );
		gameObject.BroadcastMessage("OnEnable");
		
		status.attacking = false;
		status.unbalanced = false;
		status.attackingRecovery = false;
		/*
		if(stunAttack)
		{
			Transform fx = mainWeapon.transform.FindChild("stunEffect");
			fx.particleEmitter.emit = false;
			fx.parent = null;
		}
		*/
		
		// reset swing animation entirely
		ResetAnimation();
		
		gameObject.SendMessage("OnFollowUp");
	}
	
	void ResetAnimation()
	{
		// reset swing animation entirely
		animated.animation.Stop();
		animated.animation.Play("idle");
	}
	
	public void OnKill()
	{
		Transform corpse = transform.Find("graphics");
		if(corpse != null && animated != null)
		{
			corpse.parent = null;
			corpse.gameObject.AddComponent<Rigidbody>();

			var launchVec = Vector3.up * 400.0f;
			var deviation = Random.insideUnitCircle * 400f;
			launchVec.x += deviation.x;
			launchVec.z += deviation.y;

			corpse.rigidbody.AddForce(launchVec);
			corpse.rigidbody.AddTorque(Random.insideUnitSphere * 1000f);
			Destroy(corpse.gameObject, 3.0f);
		}

		Director.Instance.OnDeath(GetComponent<Damageable>());
	}
	
	public void OnStopAttack()
	{
		dude.velocity = Vector3.zero;
		animated.animation.Stop();
		animated.animation.Play("bjorn_idle");
		StopCoroutine("doSwing");
		gameObject.BroadcastMessage("OnEnable");
		status.attacking = false;
		status.unbalanced = false;
	}
	
	public void OnInterrupt()
	{
		gameObject.SendMessage("OnStun", -0.3f);
	}
	
	public IEnumerator OnStun(float stunDuration)
	{
		if(status.stunned || cannotBeStunned) yield break;
		
		if(stunDuration == 0.0f)
			stunDuration = defaultStun;
		
		// HACK: stunDuration of < 0 means "force a stun of this amount"
		if(stunDuration < 0.0f)
		{
			stunDuration *= -1.0f;
		}
		else
		{
			stunDuration -= stunResistance;
			if(stunDuration <= 0.0f)
				stunDuration = 0.5f; //yield break;
		
			// tell the Director to maintain the bloodlust meter
			var fakeData = new DamageEvent();
			fakeData.damage = 20;
			var director = GameObject.FindGameObjectWithTag("Director");
			if(director != null)
			{
				director.SendMessage("OnDamageDealt", fakeData);
			}
		}
		
		var trailEffect = Instantiate(
			Resources.Load("Effects/effect_stun") as GameObject, Vector3.zero, Quaternion.identity
		) as GameObject;
		trailEffect.transform.parent = transform;
		trailEffect.transform.localPosition = Vector3.zero;
		
		if(stunDuration >= 1.0f)
		{
			animated.animation["bjorn_die"].speed = 1;
			animated.animation.Play("bjorn_die");
			//animated.parent.SendMessage("DoEffect", stunDuration);
		}
		
		// stop all active processes
		gameObject.BroadcastMessage("OnDisable");
		StopCoroutine("doSwing");
		if(activeTelegraph != null)
		{
			activeTelegraph.gameObject.SetActive(false);
			//activeTelegraphCircle.gameObject.SetActive(false);
		}
		
		this.status.stunned = true;
		yield return new WaitForSeconds( stunDuration );
		Destroy(trailEffect);
		
		if(stunDuration >= 1.0f)
		{
			// get up
			animated.animation["bjorn_die"].normalizedTime = 1.0f;
			animated.animation["bjorn_die"].speed = -1;
			animated.animation.Play("bjorn_die");
			
			yield return new WaitForSeconds( 0.5f );
		}
			
		gameObject.BroadcastMessage("OnEnable");
		this.status.stunned = false;
			
		// have to reset other statuses as well
		// I really shouldn't have to do this...
		this.status.Reset();
	}

	/*
	//public void OnCollisionEnter(Collision c)
	public void OnControllerColliderHit(ControllerColliderHit c)
	//public void OnTriggerEnter(Collider c)
	{
		// push back enemies if you run into them while blocking
		if(this.status.blocking)// && c.gameObject.CompareTag("EnemyMob"))
		{
			Debug.Log("bash : " + c.gameObject.tag);
			// counter test
			var otherDude = c.gameObject.GetComponent(typeof(Dude)) as Dude;
			if(otherDude != null)
			{
				var knockbackForce = 10;//mainWeapon.swingKnockback[0];
				var forceVec = (otherDude.transform.position - transform.position).normalized;
				otherDude.AddForce(forceVec * knockbackForce);
			}
		}
	}
	*/
	
	public void OnFollowUp()
	{
		var currentFacing = transform.forward;
		if(controller != null)
		{
			currentFacing = controller.GetCurrentMoveVec();
		}
			
		if(followUpQueued && !disabled && !status.stunned)
		{
			dude.Look(currentFacing);
			/*
			var attackChain = mainWeapon.GetComponent<MeleeAttackChain>();
			if(false)//attackChain != null)
			{
				attackChain.charge = 1.0f;
				attackChain.OnChargeAttack();
			}
			else
			{
				OnAttack();
			}
			*/
			OnAttack();
			if(!alwaysFollowUp)
				followUpQueued = false;
		}
	}
	
	public void OnCancel()
	{
		this.followUpQueued = false;
	}
	
	public void OnEnable()
	{
		disabled = false;
	}
	
	public void OnDisable()
	{
		disabled = true;
	}
	
	public bool isDisabled
	{
		get{ return disabled; }
	}
}
