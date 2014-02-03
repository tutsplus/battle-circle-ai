using UnityEngine;
using System.Collections;

public class Director : MonoBehaviour
{
	public Damageable player;
	public GUIText biffedGUI;
	public GUIText healthGUI;
	public Transform gameOverGUI;
	public Transform restartGUI;
	public AISettings settings;
	public GameObject enemyPrefab;
	public Transform[] spawnPoints;

	public int biffed = 0;
	
	public bool demoMode = false;

	private int waveNum = 1;
	private int baseNumDudes = 3;

	private bool isGameOver = false;
	private bool canQuit = false;

	private SwordzPlayer playerSwordz;


	// singleton stuff
	static private Director singleton;
	void Awake() {
		// if there's already a director here, it's probably been ported from the last level
		// so destroy myself
		if(singleton != null && singleton != this)
		{
			Debug.Log("Director already exists! Destroying myself!");
			Destroy(gameObject);
			return;
		}
		
		singleton = this;
	}
	public static Director Instance
	{
		get
		{
			return singleton;
		}
	}
	//  singleton stuff

	void Start ()
	{
		biffedGUI.text = biffed.ToString();
		playerSwordz = player.GetComponent<SwordzPlayer>();

		if(demoMode)
		{
			audio.Stop();
			player.maxHealth = 999999;
			player.AddHealth(999999);
		}
	
	}
	
	// Update is called once per frame
	void Update ()
	{
		if(demoMode)
		{
			return;
		}

		if(!isGameOver)
		{
			healthGUI.text = (player.GetHealth() / 10).ToString();

			if(player.GetHealth() <= 0)
			{
				StartCoroutine(OnGameOver());
			}
		}
		else if(canQuit)
		{
			if(Input.GetButtonDown("Attack_1"))
			{
				Application.LoadLevel(0);
			}
		}
	}

	void OnGUI()
	{
		if(!demoMode)
		{
			return;
		}

		Vector2 origin = new Vector2(0f, 0f);
		Vector2 rowSize = new Vector2(160f, 26f);
		
		int row = 0;
		int totalRows = 8;

		Rect boxRect = new Rect(origin.x, origin.y, rowSize.x, totalRows * rowSize.y);
		GUI.Box(boxRect, "");

		GUILayout.BeginArea(boxRect);
		GUILayout.BeginVertical();

		if(GUILayout.Button("Spawn dudes"))
		{
			SpawnWave();
		}

		GUILayout.BeginHorizontal();
		GUILayout.Label("Attackers");
		var simulString 		= GUILayout.TextField(playerSwordz.simultaneousAttackers.ToString());
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		GUILayout.Label("Danger Range");
		var dangerRangeString	= GUILayout.TextField(settings.dangerDistance.ToString());
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Attack Range");
		var attackRangeString	= GUILayout.TextField(settings.attackDistance.ToString());
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Attack Rate");
		var attackRateString    = GUILayout.TextField(settings.attackRate.ToString());
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Atk Rate Fluctuation");
		var attackRateFluctString = GUILayout.TextField(settings.attackRateFluctuation.ToString());
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Separation");
		var separationString	= GUILayout.TextField(settings.separation.ToString());
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		GUILayout.Label("Move Speed");
		var moveSpeedString		= GUILayout.TextField(settings.moveSpeed.ToString());
		GUILayout.EndHorizontal();

		GUILayout.EndVertical();
		GUILayout.EndArea();

		// update actual data
		int simul = 0;
		if(int.TryParse(simulString, out simul))
		{
			playerSwordz.simultaneousAttackers = simul;
		}

		float dangerRange = 0f;
		if(float.TryParse(dangerRangeString, out dangerRange))
		{
			settings.dangerDistance = dangerRange;
		}

		float attackRange = 0f;
		if(float.TryParse(attackRangeString, out attackRange))
		{
			settings.attackDistance = attackRange;
		}

		float attackRate = 0f;
		if(float.TryParse(attackRateString, out attackRate))
		{
			settings.attackRate = attackRate;
		}
		
		float attackRateFluctuation = 0f;
		if(float.TryParse(attackRateFluctString, out attackRateFluctuation))
		{
			settings.attackRateFluctuation = attackRateFluctuation;
		}
		
		float moveSpeed = 0f;
		if(float.TryParse(moveSpeedString, out moveSpeed))
		{
			settings.moveSpeed = moveSpeed;
		}

		float separation = 0f;
		if(float.TryParse(separationString, out separation))
		{
			settings.separation = separation;
		}

		/*
		settings.attackDistance = float.Parse(attackRange);
		settings.attackRate = float.Parse(attackRate);
		settings.attackRateFluctuation = float.Parse(attackRateFluct);
		settings.moveSpeed = float.Parse(moveSpeed);
		*/
	}

	public void OnDeath(Damageable victim)
	{
		if(victim.team != player.team && !demoMode)
		{
			biffed += 1;
			biffedGUI.text = biffed.ToString();

			Camera.main.gameObject.SendMessage("DoEffect");

			GameObject[] remaining = GameObject.FindGameObjectsWithTag("EnemyMob");
			int remainingCount = remaining.Length - 1;
			Debug.Log("remaining : " + remainingCount);

			if( remainingCount <= 0 )
		   	{
				Debug.Log("next wave!");
				StartCoroutine(NextWave());
			}
		}
	}

	public IEnumerator NextWave()
	{
		yield return new WaitForSeconds(3.0f);
		
		waveNum += 1;
		SpawnWave();
	}

	public void SpawnWave()
	{
		int numDudes = waveNum + baseNumDudes;
		
		for(int i=0; i < numDudes; i++)
		{
			int index = Random.Range(0, spawnPoints.Length);
			Vector3 spawnPos = spawnPoints[index].position;
			
			spawnPos += Random.insideUnitSphere;
			
			var obj = Instantiate(enemyPrefab, spawnPos, Quaternion.identity) as GameObject;
			gameObject.BroadcastMessage("OnSpawned", obj, SendMessageOptions.DontRequireReceiver);
		}
	}

	public IEnumerator OnGameOver()
	{
		isGameOver = true;
		gameOverGUI.gameObject.SetActive(true);

		Vector3 zoomFromPos = gameOverGUI.position;
		zoomFromPos += gameOverGUI.forward * -100f;

		iTween.MoveFrom(gameOverGUI.gameObject, zoomFromPos, 1.0f);

		yield return new WaitForSeconds(3.0f);

		canQuit = true;
		restartGUI.gameObject.SetActive(true);
	}
}
