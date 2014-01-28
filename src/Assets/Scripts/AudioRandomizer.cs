using UnityEngine;
using System.Collections;

[AddComponentMenu("Audio/Audio Randomizer")]
public class AudioRandomizer : MonoBehaviour {
	
	public float minVolume = 1.0f;
	public float maxVolume = 1.0f;
	public float minPitch = 1.0f;
	public float maxPitch = 1.0f;
	public AudioClip[] audioClips;
	
	// Use this for initialization
	void Start () {
		PlayRandom();
	}
	
	public void PlayRandom()
	{
		Randomize();
		audio.Play();
	}
	
	public void Randomize()
	{
		var originalPitch = audio.pitch;
		var pitch = (Random.value * (maxPitch - minPitch)) + minPitch;
		audio.pitch = pitch;
		
		var originalVolume = audio.volume;
		var volume = (Random.value * (maxVolume - minVolume)) + minVolume;
		audio.volume = volume;
		
		if(audioClips.Length > 0)
		{
			var index = Random.Range(0, audioClips.Length);
			audio.clip = audioClips[index];
		}
	}
	
}
