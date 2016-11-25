using UnityEngine;
using System.Collections;

public class Enemy : MovingObject {

	public int playerDamage;
	public int hp = 2;

	private Animator animator;
	private Transform target;
	private bool skipMove;

	public AudioClip enemyAttack1;
	public AudioClip enemyAttack2;

	// Use this for initialization
	protected override void Start () {
		GameManager.instance.AddEnemyToList (this);
		animator = GetComponent<Animator> ();
		target = GameObject.FindGameObjectWithTag ("Player").transform;
		base.Start();
	}

	protected override void AttemptMove(int xDir,int yDir){
		if (skipMove) {
			skipMove = false;
			return;
		}

		base.AttemptMove(xDir, yDir);

		skipMove = true;
	}

	public void MoveEnemy(){
		int xDir = 0, yDir = 0;

		if (Mathf.Abs (target.position.x - transform.position.x) < float.Epsilon)
			yDir = target.position.y > transform.position.y ? 1 : -1;
		else 
			xDir = target.position.x > transform.position.x ? 1 : -1;

		AttemptMove (xDir, yDir);
	}

	protected override void Interact(InteractableObject hitObject){
		base.Interact (hitObject);
		if (hitObject is Player) {
			Player hitPlayer = hitObject as Player;
			hitPlayer.LoseFood (playerDamage);
		}
		animator.SetTrigger ("enemyAttack");
	}

	public void DamageEnemy(int loss){
		hp -= loss;
		if (hp <= 0)
			GameObject.Destroy (gameObject);
	}
}
