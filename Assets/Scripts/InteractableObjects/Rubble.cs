using UnityEngine;

public class Rubble : InteractableObject {
	public Sprite dmgSprite1, dmgSprite2;
	public AudioClip breakSound;

	private SpriteRenderer spriteRenderer;

	void Start () {
		spriteRenderer = GetComponent<SpriteRenderer> ();
		health = 3;
	}

	public override void ChangeHealth(int change){
		health += change;

		if (health == 2)
			spriteRenderer.sprite = dmgSprite1;
		else if (health == 1)
			spriteRenderer.sprite = dmgSprite2;
		else if (health <= 0)
			Death ();
	}

	protected override void Death(){
		int distanceFromPlayer = GetDistanceFromTarget (GameManager.instance.player.transform);
		SoundManager.instance.PlayClip(distanceFromPlayer, breakSound);
		Destroy (gameObject);
	}
}
