using UnityEngine;

public class Shake : MonoBehaviour
{
	public float maxShakeAmount = 1.0f;
	public float shakeDuration = 1.0f;

	private float cooldown = 0.0f;
	private float currentShakeAmount = 0.0f;
	private Vector3 originalPosition;

	private float currentShakeDuration;
	private float currentMaxShake;

	public bool isShaking
	{
		get
		{
			return (cooldown == 0f);
		}
	}

	public void Awake() {
		originalPosition = transform.localPosition;
	}

	public void Update()
	{
		if(cooldown > 0)
		{
			cooldown -= Time.deltaTime;
			do_shake();
		}
		else if (cooldown < 0)
		{
			cooldown = 0;
		}
	}

	public void DoEffect() {
		DoEffect(0.0f);
	}
	
	public void DoEffect(float magnitude) {
		if(magnitude > 0) {
			currentShakeDuration = magnitude;
			currentMaxShake = magnitude;
		}
		else {
			currentShakeDuration = shakeDuration;
			currentMaxShake = maxShakeAmount;
		}
		cooldown = currentShakeDuration;
	}

    public void do_shake() {
		currentShakeAmount = Mathf.Lerp(0.0f, currentMaxShake, cooldown/currentShakeDuration);
		Vector3 offset = Random.insideUnitCircle * currentShakeAmount;

		transform.localPosition = originalPosition + offset;
	}

}