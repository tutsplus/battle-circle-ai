using UnityEngine;
using System.Collections;

public class TitleMenu : MonoBehaviour
{
	void Update ()
	{
		if(Input.GetButtonDown("Attack_1"))
		{
			Application.LoadLevel(Application.loadedLevel+1);
		}
	}
}
