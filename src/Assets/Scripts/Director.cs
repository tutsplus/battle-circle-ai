using UnityEngine;
using System.Collections;

public class Director : MonoBehaviour
{
	public Damageable player;
	public GUIText biffedGUI;
	public GUIText healthGUI;
	public Transform gameOverGUI;
	public Transform restartGUI;
	public GameObject enemyPrefab;
	public Transform[] spawnPoints;

	public int biffed = 0;

	private int waveNum = 1;
	private int baseNumDudes = 3;

	private bool isGameOver = false;
	private bool canQuit = false;
	
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

	void Start () {
		biffedGUI.text = biffed.ToString();
	
	}
	
	// Update is called once per frame
	void Update () {
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

	public void OnDeath(Damageable victim)
	{
		if(victim.team != player.team)
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
		int numDudes = waveNum + baseNumDudes;

		for(int i=0; i < numDudes; i++)
		{
			int index = Random.Range(0, spawnPoints.Length);
			Vector3 spawnPos = spawnPoints[index].position;

			spawnPos += Random.insideUnitSphere;

			var obj = Instantiate(enemyPrefab, spawnPos, Quaternion.identity) as GameObject;
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
