using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("DudeWorld/Swordz Player")]
public class SwordzPlayer : MonoBehaviour
{	
	public int humanPlayer;
	public BlockMeter lifeMeter;
	public GUIText statusText;
	public Color playerColor;
	public string characterFile;
	public bool mouseControls = false;

	public int simultaneousAttackers = 2;

	private Dude dude;
	//private DudeController controller;
	private SwordzDude swordzDude;
	private Damageable damageable;
	private Detector detector;
	private int healthPerBlock = 10;
	private List<GameObject> attackers;
	private float lastAttackTime = 0.0f;
	private bool arcadeTwoButtons = false;

	private CharacterController character;
	private Vector3 lastSafePosition;
	private float safePositionUpdateRate = 1.0f;
	private float lastSafePositionUpdate = 0.0f;
	private float runbuttonTime = 0.0f;
	private float sprintDelay = 0.6f;
	
	// urverden stuff
	
	public Vector3 LastSafePosition
	{
		get { return lastSafePosition; }
	}
	
	void Awake() {
		character = GetComponent<CharacterController>();
		dude = gameObject.GetComponent<Dude>();
		swordzDude = gameObject.GetComponent<SwordzDude>();
		damageable = gameObject.GetComponent<Damageable>();
		detector = gameObject.GetComponentInChildren<Detector>();
		//controller = gameObject.GetComponent(typeof(DudeController)) as DudeController;

		attackers = new List<GameObject>();
	}
	
	// Use this for initialization
	void Start () {
		//healthPerBlock = dude.maxHealth / lifeMeter.maxBlocks;
		if(mouseControls) Screen.showCursor = true;
	}

	void FixedUpdate()
	{
		if(transform.position.y >= 0.0f && (Time.fixedTime - lastSafePositionUpdate) >= safePositionUpdateRate)
		{
			lastSafePosition = transform.position + (Vector3.up * 0.1f);
			lastSafePositionUpdate = Time.fixedTime;
		}
	}
	
	void OnFall()
	{
		var respawnPrefab = Resources.Load("Effects/boom_enemy_spawn") as GameObject;
		Instantiate(respawnPrefab, lastSafePosition, Quaternion.identity);
		
		transform.position = lastSafePosition;
	}
	
	void Update ()
	{
		// if there are joysticks connected while in singleplayer mode (mouse controls), then disable them
		if(mouseControls && Input.GetJoystickNames().Length > 0)
		{
			mouseControls = false;
		}
		
		/*
		if(mouseControls &&
		   Input.GetAxis("Horizontal_"+humanPlayer) == 0.0f && Input.GetAxis("Vertical_"+humanPlayer) == 0.0f)
		{
			gameObject.SendMessage("LookAtMouse");
		}
		*/
		
		// snap look if player is performing an action
		/*
		if(Input.GetButtonDown("Attack_"+humanPlayer) ||
		   Input.GetButtonDown("Dodge_"+humanPlayer) ||
		   Input.GetButtonDown("Block_"+humanPlayer))
		{
			dude.Look(controller.GetMoveVec());
		}
		*/
		
		bool canLook = !swordzDude.status.attacking && !swordzDude.status.attackingRecovery;
		
		// allow players to cancel into other actions
		if(Input.GetButtonDown("Block_"+humanPlayer) ||
		   Input.GetButtonDown("Dodge_"+humanPlayer))
		{
			swordzDude.OnCancel();
		}
		
		
		if(Input.GetButtonDown("Attack_"+humanPlayer))
		{
			LookInAttackDirection();
			gameObject.SendMessage("OnAttack");

			//if(dude.blocking)
			//    gameObject.BroadcastMessage("OnBash");
			lastAttackTime = Time.time;
		}
		
		// dodging and sprinting
		if(Input.GetButtonUp("Dodge_"+humanPlayer))
		{
			if(swordzDude.status.running)
			{
				gameObject.BroadcastMessage("OnSprintEnd");
			}
			else if(!Input.GetButton("Attack_"+humanPlayer) && !swordzDude.status.running)
			{
				dude.Look();
				gameObject.BroadcastMessage("OnDodge");
			}
		}
		else if(Input.GetButtonDown("Dodge_"+humanPlayer))
		{
			runbuttonTime = Time.time;
		}
		else if((Input.GetButton("Dodge_"+humanPlayer) && runbuttonTime != 0.0f &&
		         Time.time - runbuttonTime >= sprintDelay))
		{
			gameObject.BroadcastMessage("OnSprint");
			runbuttonTime = 0.0f;
		}
		
		if((Input.GetButton("Block_"+humanPlayer) && !Input.GetButton("Attack_"+humanPlayer)))
		{
			if(!dude.blocking) {
				if(canLook)
				{
					if(mouseControls) gameObject.SendMessage("LookAtMouse");
					else dude.Look();
				}
				gameObject.SendMessage("OnBlock");
			}
		}
		else if(Input.GetButtonUp("Block_"+humanPlayer)) {
			gameObject.BroadcastMessage("OnBlockEnd");
		}
		
		if((Input.GetButton("Block_"+humanPlayer)) && canLook && mouseControls) {
			gameObject.SendMessage("LookAtMouse");
		}
		
		/*
		if(Input.GetButtonDown("Fire4_"+humanPlayer)) {
			gameObject.SendMessage("OnDisarm");
		}
		*/

		if(lifeMeter != null)
		{
			lifeMeter.SetMaxBlocks( (int)Mathf.Ceil(damageable.maxHealth / healthPerBlock) );
			lifeMeter.SetBlocks(damageable.GetHealth()/healthPerBlock);
		}
	}
	
	/*void OnSetPlayer(int playerNum)
	{
		humanPlayer = playerNum;
		
	}*/

	void OnShotHit(DamageEvent data)
	{
		// we send damagedealt in Dude now ... that might not be a good idea
		//Director.GetSingleton().SendMessage("OnDamageDealt", data);
	}
	
	void OnKill()
	{
		//swordzDude.OnDisarm();
		//lifeMeter.SetBlocks(0);
		//Director.GetSingleton().SendMessage("OnPlayerDeath", humanPlayer);
	}
	
	void OnRequestAttack(GameObject requestor)
	{
		attackers.RemoveAll(item => item == null);
		
		if(attackers.Count < simultaneousAttackers)
		{
			if(!attackers.Contains(requestor))
				attackers.Add(requestor);
			requestor.SendMessage("OnAllowAttack", gameObject);
			//Debug.Log("Attack accepted, current attackers: " + attackers.Count);
		}
		else
		{
			//Debug.Log("Attack REJECTED, current attackers: " + attackers.Count);
		}
	}
	
	void OnCancelAttack(GameObject requestor)
	{
		attackers.Remove(requestor);
	}
	
	void OnFollowUp()
	{
		if(detector.target != null)
		{
			LookAtTarget();
		}
		else if(mouseControls)
		{
			gameObject.SendMessage("LookAtMouse");
		}
		else
		{
			//dude.Look(currentFacing);
		}
	}
	
	// FIXME: this function and the next look amazingly similar
	void LookAtTarget()
	{
		if(detector.target != null)
		{
			var lookVec = detector.target.position - transform.position;
			if(Vector3.Angle(transform.forward, lookVec) < 90.0f)
				dude.Look(lookVec);
			else if(mouseControls)
				gameObject.SendMessage("LookAtMouse");
			else
				dude.Look();
		}
	}	
	
	public void LookInAttackDirection()
	{
		bool canLook = !swordzDude.status.attacking && !swordzDude.status.attackingRecovery;
		if(canLook)
		{
			if(detector.target != null)
			{
				LookAtTarget();
			}
			else if(mouseControls) gameObject.SendMessage("LookAtMouse");
			else dude.Look();
		}
	}
	
	void OnDrawGizmos()
	{
		if(attackers != null)
		{
			foreach(var attacker in attackers)
			{
				if(attacker != null)
				{
					Gizmos.color = Color.magenta;
					Gizmos.DrawWireSphere(attacker.transform.position, 1.0f);
				}
			}
		}
		if(lastSafePosition != null)
		{
			Gizmos.DrawWireCube(lastSafePosition, Vector3.one * 0.1f);
		}
	}
}
