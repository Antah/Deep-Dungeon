using UnityEngine;
using System.Collections;

public class Wall : InteractableObject {

	public Sprite dmgSprite1, dmgSprite2;
	public int hp = 3;

	public AudioClip chopSound1, chopSound2;

	private SpriteRenderer spriteRenderer;

	// Use this for initialization
	void Awake () {
		spriteRenderer = GetComponent<SpriteRenderer> ();
	}

	public void DamageWall(int loss){
		SoundManager.instance.RandomizeSfx(chopSound1, chopSound2);

		hp -= loss;

		if(hp == 2)
			spriteRenderer.sprite = dmgSprite1;
		if(hp == 1)
			spriteRenderer.sprite = dmgSprite2;
		
		if (hp <= 0)
			gameObject.SetActive (false);
	}
}
