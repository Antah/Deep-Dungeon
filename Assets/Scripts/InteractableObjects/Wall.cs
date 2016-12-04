using UnityEngine;
using System.Collections;

public class Wall : InteractableObject {

	public Sprite dmgSprite;
	public int hp = 3;

	public AudioClip chopSound1, chopSound2;

	private SpriteRenderer spriteRenderer;

	// Use this for initialization
	void Awake () {
		spriteRenderer = GetComponent<SpriteRenderer> ();
	}

	public void DamageWall(int loss){
		SoundManager.instance.RandomizeSfx(chopSound1, chopSound2);
		spriteRenderer.sprite = dmgSprite;
		hp -= loss;
		if (hp <= 0)
			gameObject.SetActive (false);
	}
}
