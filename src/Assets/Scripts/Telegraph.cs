using UnityEngine;
using System.Collections;

[AddComponentMenu("DudeWorld/Telegraph")]
public class Telegraph : MonoBehaviour {
	public Transform circle;
	public Transform decal;
	
	public GameObject attackerObject;
	public string attackMessage;
	
	// Use this for initialization
	void Start () {
		//OnTelegraph(1.2f);
	}
	
	void OnTelegraph(float duration)
	{
		iTween.ScaleTo(circle.gameObject, Vector3.zero, duration);
		//iTween.FadeTo(circle.gameObject, 0.0f, duration);
		
		StartCoroutine(doKill(duration));
	}
	
	IEnumerator doKill(float duration) {
		yield return new WaitForSeconds(duration);
		gameObject.SetActive(false); //Destroy(gameObject);
	}
}
