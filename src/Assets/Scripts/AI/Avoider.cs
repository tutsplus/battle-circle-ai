using UnityEngine;
using System.Collections;

public class Avoider : MonoBehaviour
{	
	public string packTag = "EnemyMob";
	public Transform avoidEnemy;

	public void OnTriggerEnter(Collider collider)
	{
		if(collider.gameObject.CompareTag(packTag))
		{
			avoidEnemy = collider.transform; //Avoid(collider.transform.position - transform.position);
		}
	}
	
	public void OnTriggerExit(Collider collider)
	{
		if(collider.gameObject.CompareTag(packTag))
		{
			avoidEnemy = null;
		}
	}
}
