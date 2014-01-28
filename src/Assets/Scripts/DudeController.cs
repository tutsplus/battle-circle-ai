using UnityEngine;
using System.Collections;

[AddComponentMenu("DudeWorld/Dude Controller")]
public class DudeController : MonoBehaviour
{
	public int humanPlayer;
	public bool cardinalMovement;
	public string aimMethod = "mouse";
	public Transform animated;
		
	private Vector3 forward = Vector3.forward;
	
	private Dude dude;
	private SwordzDude swordz;
	
	private bool itemUsed = false;
	
	private Camera cam;
	
	private Vector3 axisX = new Vector3( 0.5f, 0.0f, 0.5f);
	private Vector3 axisY = new Vector3(-0.5f, 0.0f, 0.5f);
	private Vector3 moveVec;
	
	private bool disabled = false;
	
	public void Start() {
		dude = GetComponent(typeof(Dude)) as Dude;
		swordz = GetComponent(typeof(SwordzDude)) as SwordzDude;
		
		cam = Camera.main;
		
		if( aimMethod == "mouse" ) Screen.showCursor = true;
		
		// automatically set up move axes to match the camera
		SetMovementAxes(cam.transform.right * 0.5f, cam.transform.forward * 0.5f);
	}
	
	public void SetMovementAxes(Vector3 x, Vector3 y)
	{
		axisX = x;
		axisY = y;
	}
	
	public void SetMovementDirection(Vector3 newForward)
	{
		Vector3 cameraLook = newForward - transform.position;
		var right = Vector3.Cross(Vector3.up, cameraLook);
		SetMovementAxes(right.normalized * 0.5f, cameraLook.normalized * 0.5f);
	}
	
	public Vector3 GetCurrentMoveVec()
	{
		var movex = Input.GetAxis("Horizontal_"+humanPlayer);
		var movey = Input.GetAxis("Vertical_"+humanPlayer);

		// clamp to maximum
		/*
		if( movex != 0.0f )
			movex = Mathf.Sign(movex) * 1.0f;
		if( movey != 0.0f )
			movey = Mathf.Sign(movey) * 1.0f;
		*/
		
		var finalMove = Vector3.zero;
		
		if(movex != 0.0f || movey != 0.0f)
		{
			finalMove = (movex * axisX) + (movey * axisY);
			if(finalMove.sqrMagnitude < 1.0f) finalMove = finalMove.normalized;
		}
		
		return finalMove;
	}


	public void FixedUpdate()
	//function Update()
	{
		if(disabled) return;
	
		if(cardinalMovement)
		{
			var finalMove = GetCurrentMoveVec();
			
			if(finalMove != Vector3.zero)
			{
				moveVec = finalMove;
				
				if(swordz != null)
				{
					if(swordz.status.blocking)
						finalMove *= 0.6f;
					
					if(!swordz.status.dodging)
					{
						//animated.animation.Blend("bjorn_walk");
					}
				}
				/*
				movex = cam.transform.right.x * movex;
				movey = cam.transform.up.y * movey;
				
				var move_up = transform.position - cam.transform.position;
				move_up.y = 0;
				move_up = move_up.normalized;
				var move_right = cam.transform.right;
				
				var finalMove = (move_right * movex) + (move_up * movey);
				*/
				
				if(aimMethod == "hold_direction")
				{
					if(swordz != null && !swordz.status.attacking && !swordz.status.attackingRecovery)
					{
						if(Input.GetButtonDown("Attack_"+humanPlayer) ||
						   Input.GetButtonDown("Dodge_"+humanPlayer) ||
						   Input.GetButtonDown("Block_"+humanPlayer) || swordz.status.dodging)
						{
							dude.RawMovement(finalMove, true, true);
						}
						else if(Input.GetButton("Attack_"+humanPlayer) || Input.GetButton("Block_"+humanPlayer) || swordz.status.dodging)
						{
							dude.RawMovement(finalMove, false, true);
						}
						else if(!Input.GetButtonUp("Attack_"+humanPlayer))
						{
							dude.RawMovement(finalMove, true, false);
						}
					}
				}
				else if(aimMethod == "mouse")
				{
					dude.RawMovement(finalMove, false);
				}
				else
				{
					dude.RawMovement(finalMove, true);
				}
			}
			else
			{
				// when not moving,
				//animated.animation.Stop("bjorn_walk");
				
				// if pressing an action button, snap the look
				if(Input.GetButtonDown("Attack_"+humanPlayer) ||
				   Input.GetButtonDown("Dodge_"+humanPlayer) ||
				   Input.GetButtonDown("Block_"+humanPlayer))
				{
					dude.Look();//moveVec, false);
				}
				// otherwise, continue to rotate into the direction the player last pointed to
				else if(!Input.GetButton("Attack_"+humanPlayer) &&
				   !Input.GetButton("Block_"+humanPlayer) &&
				   !Input.GetButtonUp("Attack_"+humanPlayer))
				{
					//dude.Look(moveVec, true);
					dude.Look();//moveVec, false);
				}
			}
		}
		else
			dude.Movement(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
		
		if(aimMethod == "mouse")
		{
			LookAtMouse();
		}
	}
	
	public void LookAtMouse()
	{
		transform.LookAt(GetMousePoint());
	}
	
	public Vector3 GetMousePoint()
	{
		// shoot ray where the mouse is
		// see if the dude can see the collision point
		//
		var cam = Camera.main;
		
		RaycastHit hit;
		var camRay = cam.ScreenPointToRay(Input.mousePosition);
		
		Debug.DrawRay(camRay.origin, camRay.direction);
		
		if( Physics.Raycast(camRay, out hit) )
		{
			var lookpoint = hit.point;
			// ensure the player is looking at something equal to a certain y-level
			//lookpoint.y = transform.position.y;
			// when aiming at a dude, just aim
			// otherwise, aim slightly above (such as with terrain)
			var hitDude = hit.collider.GetComponent(typeof(Dude)) as Dude;
			if(hitDude == null)
			{
				lookpoint.y = transform.position.y;//+= 0.5;
			}
			
			Debug.DrawLine(transform.position, lookpoint);
			return lookpoint;
			
			//cubepoint = new Vector3(Mathf.Round(lookpoint.x), Mathf.Floor(lookpoint.y)-1, Mathf.Round(lookpoint.z));
			//selectorCube.position = cubepoint;
		}
		
		// intersect the ray with the camera's far look plane, I guess
		return cam.ScreenToWorldPoint(Input.mousePosition);
	}
	

	public Vector3 GetMoveVec()
	{
		return moveVec;
	}
	
	public void OnEnable()
	{
		disabled = false;
	}
	
	public void OnDisable()
	{
		disabled = true;
	}

}
