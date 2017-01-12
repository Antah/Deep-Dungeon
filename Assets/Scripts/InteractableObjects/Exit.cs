using System;
using UnityEngine;

public class Exit : InteractableObject{
	public float restartLevelDelay = 1f;
	public AudioClip doorSound;

	public override void SteppedOn(){
		SoundManager.instance.PlayClip(doorSound);
		Invoke ("Restart", restartLevelDelay);
		GameManager.instance.player.enabled = false;
	}

	private void Restart(){
		GameManager.instance.NextLevel ();
	}
}