using UnityEngine;
using System.Collections;

public class ParticleAutodestruct : MonoBehaviour
{
	void Start ()
	{
		if(!particleSystem.loop)
		{
			Destroy(gameObject, particleSystem.duration);
		}
	}
	
	public void DestroyGracefully()
	{
		DestroyGracefully(gameObject);
	}
	
	static public void DestroyGracefully(GameObject go)
	{
		go.transform.parent = null;
		go.particleSystem.loop = false;
		go.particleSystem.enableEmission = false;
		Destroy(go, go.particleSystem.duration);
	}
}
