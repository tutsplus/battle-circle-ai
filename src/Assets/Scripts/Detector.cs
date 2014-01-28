using UnityEngine;
using System.Collections;

public class Detector : MonoBehaviour
{	
	public string detectTag = "EnemyMob";
	public Transform target;

	public void OnTriggerEnter(Collider collider)
	{
		if(target == null && collider.gameObject.CompareTag(detectTag))
		{
			target = collider.transform; //Avoid(collider.transform.position - transform.position);
		}
	}
	
	public void OnTriggerExit(Collider collider)
	{
		if(collider.transform == target)//collider.gameObject.CompareTag(detectTag))
		{
			target = null;
		}
	}
}
