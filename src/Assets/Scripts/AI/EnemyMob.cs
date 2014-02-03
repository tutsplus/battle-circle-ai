using UnityEngine;
using System.Collections;

[AddComponentMenu("DudeWorld/AI/EnemyMob")]
public class EnemyMob : MonoBehaviour
{
	/*
	Using a computer model, researchers had each virtual “wolf” follow two rules:
	(1) move towards the prey until a certain distance is reached, and
	(2) when other wolves are close to the prey, move away from [the other wolves].
	These rules cause the pack members to behave in a way that resembles real wolves, circling up around the animal,
	and when the prey tries to make a break for it, one wolf sometimes circles around and sets up an ambush, no communication required.
	*/

	public float attackDistance = 1.0f;
	public float dangerDistance = 2.0f;

	public float trackSpeed = 0.1f;
	public float attackRate = 10.0f;
	public float attackRateFluctuation = 0.0f;
	
	private Dude prey;
	private SwordzDude preySwordz;
	
	public Dude dude { get; private set; }
	private SwordzDude swordzDude;
	private Damageable damageable;
	private Vector3 destination;
	private Vector3 moveVec;

	private float lastAttackTime = 0.0f;

	private bool disabled = false;
	private float lastThought = 0.0f;
	private float lastReact = 0.0f;
	private float actualAttackRate = 0.0f;

	private Transform animated;

	private float thinkPeriod = 1.5f;
	private float reactPeriod = 0.4f;
	private GameObject combattant;
	
	private GameObject preyObject = null;
	private Vector3 distVec;
	private Vector3 avoidVec = Vector3.zero;
	private float distance;
	private float sqrDistance;
	private float sqrAttackDistance;
	private float sqrDangerDistance;
	private bool engagePrey = false;
	private float strafeDir = 1.0f;
	private float strafeCooldown = 0f;
	private float strafeRate = 3.0f;

	private Avoider avoider;

	private float attackCooldown
	{
		get
		{
			return Mathf.Max(actualAttackRate - (Time.fixedTime - lastAttackTime), 0f);
		}
	}

	void Start()
	{
		dude = GetComponent<Dude>();
		swordzDude = GetComponent<SwordzDude>();
		damageable = GetComponent<Damageable>();

		actualAttackRate = attackRate + (Random.value - 0.5f) * attackRateFluctuation;
		lastAttackTime = -actualAttackRate;

		// HACK : get me outta here!	
		animated = swordzDude.animated; // transform.Find("graphics/bjorn");
		
		avoider = gameObject.GetComponentInChildren<Avoider>();
		if(avoider != null)
		{
			Physics.IgnoreCollision(collider, avoider.collider);
		}
		
		sqrAttackDistance = Mathf.Pow(attackDistance, 2);
		sqrDangerDistance = Mathf.Pow(dangerDistance, 2);

		// offset the start of the think ticks to spread the load out a little
		lastThought += thinkPeriod * Random.value;
		lastReact += reactPeriod * Random.value;
		
		StartCoroutine("SummoningSickness");
	}
	
	IEnumerator SummoningSickness()
	{
		this.OnDisable();
		yield return new WaitForSeconds(1.0f);
		this.OnEnable();
	}

	void UpdateDistance()
	{
		distVec = (destination - transform.position);
		sqrDistance = distVec.sqrMagnitude;
		if(sqrDistance > sqrAttackDistance)// && engagePrey)
		{
			OnAttackComplete();
		}
	}

	void FixedUpdate ()
	{
		if(engagePrey && !swordzDude.status.attacking)
		{
			OnAttackComplete();
		}

		// keep looking at the target even if it's disabled
		if(disabled)
		{
			if(prey != null)
			{
				if(trackSpeed > 0.0f && swordzDude.status.attacking && !swordzDude.status.stunned)
				{
					Vector3 lookVec = prey.transform.position - transform.position;
					dude.Look(lookVec, trackSpeed);
				}

				// quick-react, just get the data we need
				lastReact = Time.fixedTime;
		
				UpdateDistance();
			}
			return;
		}
		
		if(strafeCooldown > 0.0f)
		{
			strafeCooldown -= Time.fixedDeltaTime;
		}
		
		// during each thinkperiod,
		if(prey == null || (Time.fixedTime - lastThought) > thinkPeriod)
		{
			lastThought = Time.fixedTime;
			Think();
		}
		if(prey == null)
		{
			return;
		}

		if((Time.fixedTime - lastReact) > reactPeriod)
		{
			React();	
		}

		UpdateDistance();
		
		bool shouldAvoid = (avoidVec != Vector3.zero && sqrDistance <= sqrDangerDistance);
		bool shouldStrafe = (!shouldAvoid && !engagePrey && sqrDistance <= sqrAttackDistance);
		bool shouldAttack = (engagePrey && sqrDistance <= sqrAttackDistance);

		if(shouldAvoid)
		{
			Avoid(avoidVec);
		}
		else if(shouldAttack)
		{
			/*
			if(!engagePrey)
			{
				prey.gameObject.SendMessage("OnRequestAttack", gameObject);
			}
			*/
			// I have permission
			Attack(prey.transform.position);
		}
		else if(shouldStrafe)
		{
			// I don't have permission, so I'll just strafe around
			Strafe(prey.transform.position, strafeDir);
		}
		else
		{
			// I'm outside the danger zone so seek the target
			Seek(distVec);
		}
	}
	
	void Seek(Vector3 distVec)
	{
		Seek(distVec, true);
	}
	
	void Seek(Vector3 distVec, bool align)
	{
		// whenever I decide to move, I am giving up permission to attack
		if(engagePrey)
		{
			OnAttackComplete();
		}

		// can't move if I'm getting pushed around
		if(dude.GetForce() != Vector3.zero) return;
	
		destination = prey.transform.position;
		moveVec = distVec.normalized;
		moveVec.y = 0.0f;

		dude.RawMovement(moveVec, align);
	}
	
	void Avoid(Vector3 distVec)
	{
		Seek(distVec * -1, false);
	}
	
	void Strafe(Vector3 distVec, float direction)
	{
		var perpendicularVec = Vector3.Cross(Vector3.up, distVec);
		Seek(perpendicularVec * direction, false);
	}
	
	IEnumerator OnWait(float delay)
	{
		this.OnDisable();
		yield return new WaitForSeconds(delay);
		this.OnEnable();
	}
	
	void Attack(Vector3 target)
	{
		if(!swordzDude.status.attacking && attackCooldown <= 0.0f)
		{
			dude.Look(prey.transform.position - transform.position);
		    bool success = false;

			success = swordzDude.OnAttack();
			
			if(success)
			{
				lastAttackTime = Time.fixedTime;
				actualAttackRate = attackRate + (Random.value - 0.5f) * attackRateFluctuation;
			}
		}
	}

	GameObject GetClosestTarget()
	{
		GameObject[] allPrey = GameObject.FindGameObjectsWithTag("Player");
		GameObject target = null;

		var closestDist = Mathf.Infinity;
		foreach(var p in allPrey)
		{
			if(p == null || p == gameObject || p.name == gameObject.name ) continue;
			
			var dirVec = transform.position - p.transform.position;
			var d = dirVec.sqrMagnitude;
			if(d < closestDist)
			{
				target = p;
				closestDist = d;
			}
		}

		return target;
	}

	void Think()
	{
		preyObject = GetClosestTarget();
		
		// durr... nothing to kill!
		if(preyObject == null)
		{
			return;
		}
		
		prey = preyObject.GetComponent<Dude>();
		preySwordz = preyObject.GetComponent<SwordzDude>();
		var damageable = preyObject.GetComponent<Damageable>();
		
		// don't kill dead things
		// WHAT IS DEAD MAY NEVER DIE
		if(damageable != null && damageable.GetHealth() <= 0)
		{
			return;
		}
		
		// for enemy pack avoidance
		if(avoider != null && avoider.avoidEnemy != null)
		{
			avoidVec = avoider.avoidEnemy.transform.position - transform.position;
			avoidVec = Vector3.Slerp(distVec.normalized, avoidVec.normalized, 0.5f);
		}
		else
		{
			avoidVec = Vector3.zero;
		}
		
		// for strafing, if I decide to strafe
		if(!engagePrey && strafeCooldown <= 0f)
		{
			strafeCooldown = strafeRate;
			strafeDir = 1.0f;
			if(Random.value > 0.5f) strafeDir = -1.0f;
		}
	}

	void React()
	{
		lastReact = Time.fixedTime;
		
		distVec = (destination - transform.position);
		sqrDistance = distVec.sqrMagnitude;
		//distVec = distVec.normalized;

		
		if( sqrDistance != 0 && sqrDistance <= sqrAttackDistance )
		{
			if(!engagePrey)
			{
				prey.gameObject.SendMessage("OnRequestAttack", gameObject);

			}
		}
	}

	void OnAllowAttack(GameObject target)
	{
		if(prey != null && target == prey.gameObject)
			engagePrey = true;
	}
	
	void OnAttackComplete()
	{
		// disengage when completing an attack to give other enemies a chance
		// CAVEAT: this currently only happens if I am stunned or am not in range of my prey
		//   and NOT when I complete an attack
		engagePrey = false;
		if( prey != null )
			prey.gameObject.SendMessage("OnCancelAttack", gameObject, SendMessageOptions.DontRequireReceiver);
	}
	
	void OnStun(float d)
	{
		OnAttackComplete();
	}

	void OnDrawGizmos()
	{
		if(avoidVec != Vector3.zero && sqrDistance <= sqrDangerDistance)
		{
			var radius = avoider.GetComponent<SphereCollider>().radius;

			Gizmos.color = Color.blue;
			Gizmos.DrawWireSphere(transform.position, radius);
		}
	}

	void OnEnable()
	{
		disabled = false;
	}
	
	void OnDisable()
	{
		disabled = true;
	}
}
