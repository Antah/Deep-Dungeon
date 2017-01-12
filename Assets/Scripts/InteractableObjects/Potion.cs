using UnityEngine;

public class Potion : InteractableObject{
	public int healthRestored = 20;
	public AudioClip eatSound1, eatSound2;

	public override void SteppedOn(){
		GameManager.instance.player.ChangeHealth (healthRestored);
		SoundManager.instance.PlayClipFromList(eatSound1, eatSound2);
		Death ();
	}
}


