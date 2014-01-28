using UnityEngine;
using System.Collections;
using System.Collections.Generic;


[AddComponentMenu("DudeWorld/Dude")]
public class Dude : MonoBehaviour
{
	public float moveSpeed = 5.0f;
	public float turnSpeed = 40.0f;
	
	public int team = 0;
	
	public GameObject hitEffect;
	public GameObject blockEffect;

	public bool blocking = false;
	public float drag = 10.0f;
	public bool applyGravity = true;
	public bool flying = false;
	public bool invincibleWhenHit = false;
	public bool directionalBlock = false;
	
	private float backstabAngle = 0.4f;
	private float boardDistance = 1.0f;
	private Vector3 moveVec;
	private Vector3 forward;
	public Vector3 velocity
	{
		get; set;
	}
	private bool grounded = false;
	
	private bool invincible = false;
	
	private CharacterController controller;
	
	public GameObject attacker
	{
		get; private set;
	}
	
	void Start() {
		velocity = Vector3.zero;
		controller = (CharacterController)GetComponent(typeof(CharacterController));
	}
	
	void FixedUpdate()
	{
		Vector3 forcesVec = Vector3.zero;
		forcesVec += this.Gravity();
		//velocity += this.Gravity();
		forcesVec += this.Forces();
		
		if(forcesVec == Vector3.zero)
		{
			forcesVec = Vector3.up * 0.0001f;
		}
		
		//if(forcesVec != Vector3.zero)
		//{
			var flags = controller.Move(forcesVec * Time.fixedDeltaTime);
			grounded = (flags & CollisionFlags.CollidedBelow) != 0;
		//}
	}
	
	/// <summary>
	/// Apply Gravity
	/// </summary>
	public Vector3 Gravity() {
		Vector3 grav = Vector3.zero;
		if(applyGravity && !grounded)
			grav = new Vector3(0.0f,-5.0f,0.0f);
			//controller.Move(new Vector3(0.0f,-6.0f * Time.fixedDeltaTime,0.0f));
		if(flying && transform.position.y <= 0.25f)
		{
			grav = Vector3.zero;
		}
		return grav;
	}
	
	/// <summary>
	/// Apply velocity and other forces
	/// </summary>
	public Vector3 Forces() {
		Vector3 vec = Vector3.zero;
		
		if(velocity != Vector3.zero)
		{
			//controller.Move(velocity * Time.fixedDeltaTime);
			vec = velocity;
			
			if(velocity.magnitude > 0.01f)
				velocity -= (velocity * drag * Time.fixedDeltaTime);
			else
				velocity = Vector3.zero;
		}
		
		return vec;
	}
	
	public void AddForce(Vector3 force) {
		velocity += force;
	}
	
	public Vector3 GetForce() {
		return velocity;
	}
	
	public void Movement(float xaxis, float yaxis) {
		if(xaxis == 0 && yaxis == 0) return;

		//forward = transform.TransformDirection(Vector3.forward);
		moveVec = transform.forward * moveSpeed * yaxis;
		
		transform.position += moveVec;
		
		transform.Rotate(0,xaxis * turnSpeed * Time.fixedDeltaTime,0);
	}

	public void RawMovement(Vector3 move) {
		RawMovement(move, true, false);
	}
	
	public void RawMovement(Vector3 move, bool align)
	{
		RawMovement(move, align, false);
	}
	
	public void RawMovement(Vector3 move, bool align, bool forceLook) {
		moveVec = move * moveSpeed;
		//transform.position += moveVec;
		if(align && move.magnitude > 0.15 && (transform.forward != move))
		{
			Vector3 lookVec = move;
			//transform.rotation.SetFromToRotation(transform.forward, lookVec);
			//var rotAngle = Vector3.Angle(transform.forward, lookVec);
			//transform.Rotate(0.0f, rotAngle, 0.0f);
			//var lookVec = moveVec;
			Look(lookVec, !forceLook);
		}
		
		//controller.Move(moveVec);
		var flags = controller.Move(moveVec * Time.fixedDeltaTime);
		grounded = (flags & CollisionFlags.CollidedBelow) != 0;
	}
	
	public void RawMotion(Vector3 move)
	{
		var originalSpeed = moveSpeed;
		moveSpeed = 1.0f;
		RawMovement(move);
		moveSpeed = originalSpeed;
	}
	
	public void Look()
	{
		this.Look(this.moveVec, 0.0f);
	}
	
	public void Look(Vector3 lookVec)
	{
		this.Look(lookVec, 0.0f);
	}
	
	// legacy : try to convert usages to the (vec, float) version
	public void Look(Vector3 lookVec, bool smooth) {
		var speed = 0.0f;
		if(smooth)
		{
			speed = 0.2f;
		}
		this.Look(lookVec, speed);
	}
	
	public void Look(Vector3 lookVec, float speed) {
		lookVec.y = 0.0f;
		if(transform.forward == lookVec || lookVec == Vector3.zero) return;
		
		if(speed > 0.0f)
		{
			// HACK : commented out for now, doesn't work on deployed builds
			lookVec = Vector3.RotateTowards(transform.forward, lookVec, speed, 0.0f);
		}
		//iTween.Stop(gameObject, "LookTo");
		//iTween.LookTo(gameObject, transform.position + moveVec, 0.5f);
		//iTween.RotateTo(gameObject, 
		//transform.LookAt(transform.position + moveVec);
		transform.rotation = Quaternion.LookRotation(lookVec, Vector3.up);
	}
	
	public void CardinalMovement(float xaxis, float yaxis) {
		if(xaxis == 0 && yaxis == 0) return;
		
		moveVec = new Vector3(xaxis, 0, yaxis);
		moveVec *= moveSpeed;
		
		//transform.position += moveVec;
		var flags = controller.Move(moveVec * Time.fixedDeltaTime);
		grounded = (flags & CollisionFlags.CollidedBelow) != 0;
		
		if(moveVec.magnitude > 0.15f)
		{
			//iTween.Stop(gameObject, "LookTo");
			//iTween.LookTo(gameObject, transform.position + moveVec, 1);
			transform.LookAt(transform.position + moveVec);
			//transform.rotation = Quaternion.RotateTowards(transform.rotation, );
		}
	}
	
	public void OnKill()
	{
		Destroy(gameObject);
	}
	
	/// FIXME: this can become some static class someplace
	public Vector3 GetGridPos(Vector3 pos) {
		Vector3 snappedPos = pos;
		snappedPos.x = Mathf.Round(snappedPos.x);
		snappedPos.z = Mathf.Round(snappedPos.z);
		
		return snappedPos;
	}
	
	/// FIXME: so can this
	public Vector3 GetForwardGridPos() {
		forward = transform.TransformDirection(Vector3.forward);
		return GetGridPos(transform.position+forward);
	}
	
	/// return a list of colliders directly in front of me
	public List<Collider> GetForwardColliders() {
		// do a collision test in front of me
		// snap collision to AABB grid
		Vector3 snappedPos = GetForwardGridPos();
		
		// if it collides, use Item and WorldObject as inputs to interaction rule logic
		List<Collider> hits = new List<Collider>(Physics.OverlapSphere(snappedPos,0.25f));
		hits.Remove(this.collider); // doesn't throw exception

		return hits;
	}
	
	/*
	public void OnTriggerEnter(Collider other) {
		Debug.Log("oof! I bumped into a " + other.gameObject.name);
	}
	*/
	
	public void SetMoveVec(Vector3 vec)
	{
		this.moveVec = vec;
	}
}