// The target we are following
public var targets : Transform[];

public var offset : Vector3;

public var lockY = false;

public var center : Vector3 = Vector3.zero;

public var lookDirection : Vector3 = Vector3.one;

// How much we 
var followDamping = 2.0;

private var focusPoint : Transform;
private var distance : float;
private var xRot = 0.0f;

// Place the script in the Camera-Control group in the component menu
@script AddComponentMenu("Camera-Control/Smooth Co-op Follow")

function Start()
{
	var focusObj = new GameObject();
	focusPoint = focusObj.transform;
	focusPoint.name = "CameraFocusPoint";
	focusPoint.rotation = transform.rotation;
	distance = (transform.position - focusPoint.position).magnitude;
	xRot = transform.eulerAngles.x;
}

function LateUpdate () {
	// Early out if we don't have a target
	if (targets.length < 1)
		return;
	
	var totalx = 0.0;
	var totaly = 0.0;
	var totalz = 0.0;
	var liveTargets = 0;
	for(var t in targets) {
		// ignore dead targets
		if(t == null)
			continue;
	
		totalx += t.position.x;
		totaly += t.position.y;
		totalz += t.position.z;
		liveTargets += 1;
	}
	
	if(liveTargets == 0) return;
	
	var avgx = totalx / liveTargets;
	var avgz = totalz / liveTargets;
	var avgy = 1.0;
	if(!lockY)
		avgy = totaly / liveTargets;
	var target_pos = new Vector3(avgx, avgy, avgz);
	
	center = target_pos;
	
	focusPoint.position = center;
	if(followDamping > 0.0f)
	{
		transform.position = Vector3.Lerp (transform.position, target_pos + offset,
			followDamping * Time.deltaTime);
	}
	else
	{
		transform.position = target_pos + offset;
	}	
	// Always look at the target
	//transform.LookAt (target);
	
	//transform.eulerAngles.x -= extraPitch;
}

function OnLookDirection(dir : Vector3)
{
	//dir = dir.normalized;
	focusPoint.LookAt(focusPoint.position + dir);
	focusPoint.Rotate(xRot, 0.0f, 0.0f);
	var newOffset = focusPoint.forward * -(distance/2);
	offset = newOffset;
	transform.rotation = focusPoint.rotation;
	transform.position = focusPoint.position + offset;
}

function OnRotateCamera(rot : Vector3)
{
	//focusPoint.LookAt(focusPoint.position + focusPoint.forward);
	//xRot += rot.x;
	//focusPoint.Rotate(xRot, 0.0f, 0.0f);
	focusPoint.Rotate(0.0f, rot.x, 0.0f, Space.World);
	var newOffset = focusPoint.forward * -(distance/2);
	offset = newOffset;
	transform.rotation = focusPoint.rotation;
	transform.position = focusPoint.position + offset;
}

function GetFocus() : Transform
{
	return focusPoint;
}