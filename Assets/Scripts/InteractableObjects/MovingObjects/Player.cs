using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Player : MovingObject {

	public AudioClip moveSound1, moveSound2, dieSound, attackEnemySound, attackWallSound, attackRubbleSound1, attackRubbleSound2;

	private int maxHealth = 200;
	private Animator animator;
	private Vector2 touchOrigin = -Vector2.one;
	private bool isMoving;

	protected override void Start () {
		animator = GetComponent<Animator> ();

		health = GameManager.instance.playerHealth;
		GameManager.instance.SetLifeText(health);

		base.Start ();
	}

	void OnDisable(){
		GameManager.instance.playerHealth = health;
	}

	void Update () {
		if (!GameManager.instance.playersTurn)
			return;

		int horizontal = 0, vertical = 0;

		#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBPLAYER
		if (Input.GetKeyDown ("space"))
			GameManager.instance.playersTurn = false;
		horizontal = (int)Input.GetAxisRaw ("Horizontal");
		vertical = (int)Input.GetAxisRaw ("Vertical");

		#else
		if(Input.touchCount > 0){
			Touch myTouch = Input.touches[0];
			if(myTouch.phase == TouchPhase.Began){
				touchOrigin = myTouch.position;
			} else if(myTouch.phase == TouchPhase.Ended && touchOrigin.x >= 0){
				Vector2 touchEnd = myTouch.position;
				float x = touchEnd.x - touchOrigin.x;
				float y = touchEnd.y - touchOrigin.y;
				touchOrigin.x = -1;
				if(Mathf.Abs(x) > Mathf.Abs(y))
					horizontal = x > 0 ? 1 : -1;
				else
					vertical = y > 0 ? 1 : -1;
			}
		}
		#endif

		if (horizontal != 0)
			vertical = 0;

		if (horizontal != 0 || vertical != 0)
			MoveOrInteract (horizontal, vertical);
	}

	protected override void MoveOrInteract(int xDir, int yDir){
		RaycastHit2D hit = CheckForCollision (xDir, yDir);
		UpdateSpriteDirection (xDir);

		if (hit.transform == null) {
			Move (xDir, yDir);
			return;
		}

		InteractableObject hitObject = hit.transform.GetComponent (typeof(InteractableObject)) as InteractableObject;

		if (hitObject != null )
			Interact(hitObject);
	}
		
	private void OnTriggerEnter2D (Collider2D other){
		InteractableObject steppedObject = other.transform.GetComponent (typeof(InteractableObject)) as InteractableObject;
		steppedObject.SteppedOn();
	}

	protected override void Interact(InteractableObject hitObject){
		PlaySound (hitObject);
		animator.SetTrigger ("playerAttack");
		base.Interact (hitObject);
		GameManager.instance.playersTurn = false;
	}

	public override void Hit(int damage){
		animator.SetTrigger ("playerHit");
		base.Hit(damage);
	}

	public override void ChangeHealth (int change){		
		if (health <= 0)
			return;
		health += change;
		if (health > maxHealth)
			health = maxHealth;
		GameManager.instance.SetLifeText(health);
		CheckIfGameOver();
	}

	private void CheckIfGameOver(){
		if (health <= 0){
			SoundManager.instance.musicSource.Stop ();
			SoundManager.instance.PlaySoundEffect (efxSource, dieSound);
			GameManager.instance.GameOver ();
		}
	}

	protected override void Move(int xDir, int yDir){
		Vector3 end = transform.position + new Vector3(xDir, yDir, 0);
		StartCoroutine (SmoothMovement (end));
	}

	protected IEnumerator SmoothMovement(Vector3 end){
		if (isMoving)
			yield break;
		isMoving = true;
		if (health > 100)
			ChangeHealth (-2);
		SoundManager.instance.PlaySoundEffectWithRandomPitch(efxSource, moveSound1, moveSound2);
		float sqrRemainingDistance = (transform.position - end).sqrMagnitude;

		while (sqrRemainingDistance > 0) {
			Vector3 newPosition = Vector3.MoveTowards (rb2D.position, end, 1f / moveTime * Time.deltaTime);
			rb2D.MovePosition (newPosition);
			sqrRemainingDistance = (transform.position - end).sqrMagnitude;
			yield return null;
		}
		isMoving = false;
		GameManager.instance.playersTurn = false;
	}

	private void PlaySound(InteractableObject hitObject){
		if(hitObject is Rubble)
			SoundManager.instance.PlaySoundEffectWithRandomPitch(efxSource, attackRubbleSound1, attackRubbleSound2);
		else if(hitObject is Enemy)
			SoundManager.instance.PlaySoundEffectWithRandomPitch(efxSource, attackEnemySound);
		else if(hitObject is Wall)
			SoundManager.instance.PlaySoundEffectWithRandomPitch(efxSource, attackWallSound);
	}
}
