using UnityEngine;

public class Apple : InteractableObject {
	public int healthRestored = 10;
	public AudioClip eatSound1, eatSound2;

	public override void SteppedOn(){
		GameManager.instance.player.ChangeHealth (healthRestored);
		SoundManager.instance.PlayClipFromList(eatSound1, eatSound2);
		Death ();
	}
}
