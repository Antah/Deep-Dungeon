using UnityEngine;
using System.Collections;

public class Enemy : MovingObject {

	public int skipChance = 30;

	private Animator animator;
	private Transform target;
	private int distanceFromTarget;

	public AudioClip attackPlayerSound1, attackRubbleSound1, attackRubbleSound2, dieSound;


	protected override void Start () {
		GameManager.instance.AddEnemyToList (this);
		animator = GetComponent<Animator> ();
		target = GameManager.instance.GetPlayer().transform;
		digDamage = 2;
		health = 20;
		base.Start();
	}

	public void MoveEnemy(){
		if (IsWaiting ())
			return;

		MoveOrInteract (0, 0);
	}

	protected override void MoveOrInteract(int xDir, int yDir){
		UpdateDistanceFromTarget ();

		if (distanceFromTarget > 7) {
			RandomMove ();
			return;
		}

		SetDirectionTowardsTarget (out xDir, out yDir);

		RaycastHit2D hit = CheckForCollision (xDir, yDir);

		if (hit.transform == null) {
			Move (xDir, yDir);
			return;
		}

		InteractableObject hitObject = hit.transform.GetComponent (typeof(InteractableObject)) as InteractableObject;

		if (hitObject is Wall || hitObject is Enemy) {
			SetDirectionTowardsTarget (out xDir, out yDir, true);

			hit = CheckForCollision (xDir, yDir);

			if (hit.transform == null) {
				Move (xDir, yDir);
				return;
			}

			hitObject = hit.transform.GetComponent (typeof(InteractableObject)) as InteractableObject;

			if (hitObject is Wall || hitObject is Enemy) {
				RandomMove ();
				return;
			}
		}

		if (hitObject != null) {
			UpdateSpriteDirection (xDir);
			Interact (hitObject);
		}
	}

	protected override void Interact(InteractableObject hitObject){
		base.Interact (hitObject);
		PlaySound (hitObject);
		animator.SetTrigger ("enemyAttack");
	}

	protected override void UpdateSpriteDirection(int xDir){
		if (xDir < 0)
			spriteRenderer.flipX = false;
		else if(xDir > 0)
			spriteRenderer.flipX = true;
	}

	private void UpdateDistanceFromTarget(){
		distanceFromTarget = GetDistanceFromTarget (target);
	}

	private bool IsWaiting(){
		int random = Random.Range (1, 101);
		if (random < skipChance)
			return true;
		return false;
	}

	private void SetDirectionTowardsTarget(out int xDir, out int yDir, bool reverse = false){
		if (reverse) {
			if (Mathf.Abs (target.position.x - transform.position.x) < Mathf.Abs (target.position.y - transform.position.y)) {
				xDir = target.position.x > transform.position.x ? 1 : -1;
				yDir = 0;
			} else {
				yDir = target.position.y > transform.position.y ? 1 : -1;
				xDir = 0;
			}
			return;
		}

		if (Mathf.Abs (target.position.x - transform.position.x) < Mathf.Abs (target.position.y - transform.position.y)) {
			xDir = 0;
			yDir = target.position.y > transform.position.y ? 1 : -1;
		} else {
			yDir = 0;
			xDir = target.position.x > transform.position.x ? 1 : -1;
		}
	}

	private void RandomMove(){
		int xDir = 0, yDir = 0;

		xDir = Random.Range (-1, 2);
		if (xDir == 0)
			yDir = Random.Range (-1, 2);
		else
			yDir = 0;

		RaycastHit2D hit = CheckForCollision (xDir, yDir);

		if (hit.transform == null) {
			Move (xDir, yDir);
			return;
		}

		InteractableObject hitObject = hit.transform.GetComponent (typeof(InteractableObject)) as InteractableObject;

		if (hitObject != null) {
			if (hitObject is Enemy)
				return;
			UpdateSpriteDirection (xDir);
			Interact (hitObject);
		}
	}

	protected override void Death(){
		SoundManager.instance.PlayClip(dieSound);
		Destroy (gameObject);
	}

	private void PlaySound(InteractableObject hitObject){
		int distanceFromPlayer = GetDistanceFromTarget (GameManager.instance.player.transform);
		if(hitObject is Rubble || hitObject is Wall)
			SoundManager.instance.PlaySoundEffectWithRandomPitch(distanceFromPlayer, efxSource, attackRubbleSound1, attackRubbleSound2);
		else if(hitObject is Player)
			SoundManager.instance.PlaySoundEffectWithRandomPitch(distanceFromPlayer, efxSource, attackPlayerSound1);
	}
}
