using UnityEngine;
using System.Collections.Generic;

public class AISettings : MonoBehaviour
{
	public float attackDistance = 1.0f;
	public float dangerDistance = 2.0f;

	public float attackRate = 10.0f;
	public float attackRateFluctuation = 0.0f;

	public float separation = 1.25f;
	public float moveSpeed = 2.0f;

	private List<EnemyMob> ais;

	void Start()
	{
		ais = new List<EnemyMob>();

		// record everything spawned at the beginning of the game
		foreach(EnemyMob e in GameObject.FindObjectsOfType<EnemyMob>())
		{
			OnSpawned(e.gameObject);
		}
	}

	void OnSpawned(GameObject obj)
	{
		var enemy = obj.GetComponent<EnemyMob>();
		if(enemy != null && !ais.Contains(enemy))
		{
			ais.Add(enemy);
		}
	}

	void OnDeath(Damageable victim)
	{
		var enemy = victim.GetComponent<EnemyMob>();
		if(enemy != null && ais.Contains(enemy))
		{
			ais.Remove(enemy);
		}
	}

	void FixedUpdate()
	{
		var dead = new List<int>();

		for(int i = 0; i < ais.Count; i++)
		{
			EnemyMob ai = ais[i];
			if(ai == null)
			{
				dead.Add(i);
				continue;
			}

			ai.dangerDistance = dangerDistance;
			ai.attackDistance = attackDistance;
			ai.attackRate = attackRate;
			ai.attackRateFluctuation = attackRateFluctuation;
			ai.dude.moveSpeed = moveSpeed;

			// HACK: this is probably slow
			ai.GetComponentInChildren<SphereCollider>().radius = separation;
		}

		foreach(int j in dead)
		{
			ais.RemoveAt(j);
		}
	}
}
