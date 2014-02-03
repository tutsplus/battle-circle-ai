using UnityEngine;

public class Flicker : MonoBehaviour
{
	//public bool effectEnabled = false;
	public float totalDuration = 3.0f;
	public float timeOn = 0.5f;
	public float timeOff = 0.5f;
	public bool flickerChildren = false;
	
	private float cooldown = 0.0f;
	private float totalCooldown = 0.0f;
	private bool showing = true;
	
	private Component[] renderers;
	
	void Start()
	{
		enabled = false;
	}
	
	public void Update()
	{
		cooldown -= Time.deltaTime;
		
		// 0 duration means play forever
		if(totalDuration > 0)
		{
			totalCooldown -= Time.deltaTime;
				
			if(totalCooldown <= 0.0f) {
				StopEffect();
				return;
			}
		}
			
		if(cooldown <= 0.0f) {
			if(showing)
			{
				SetRender(false);
				cooldown += timeOff;
			}
			else
			{
				SetRender(true);
				cooldown += timeOn;
			}
		}
	}
	
	private void SetRender(bool state)
	{
		if(flickerChildren && renderers != null)
		{
			foreach(Renderer r in renderers)
				r.enabled = state;
		}
		else if(renderer != null)
		{
			renderer.enabled = state;
		}
		
		showing = state;
	}
	
	public void StartEffect()
	{
		if(flickerChildren)
			renderers = transform.GetComponentsInChildren<Renderer>();
		
		totalCooldown = totalDuration;
		enabled = true;
	}
	
	public void StopEffect()
	{
		SetRender(true);
		cooldown = 0.0f;
		totalCooldown = 0.0f;
		enabled = false;
		
		renderers = null;
	}
}