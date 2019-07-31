using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManagerTest : MonoBehaviour 
{

	// Use this for initialization
	void Start () 
	{
		if (AudioManager.instance)
			AudioManager.instance.SetTrackVolume("Zombies", 10, 5);	
	}
	

}
