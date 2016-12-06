using UnityEngine;
using System.Collections;

public class Enemy : MovingObject {

	public int playerDamage;
	public int hp = 2;
	public int skipChance = 30;

	private Animator animator;
	private Transform target;

	public AudioClip enemyAttack1;
	public AudioClip enemyAttack2;

	// Use this for initialization
	protected override void Start () {
		GameManager.instance.AddEnemyToList (this);
		animator = GetComponent<Animator> ();
		target = GameManager.instance.GetPlayer().transform;
		wallDamage = 2;
		base.Start();
	}

	public bool MoveEnemy(){
		if (IsWaiting ())
			return false;

		int xDir = 0, yDir = 0;
		GetDistanceFromTarget ();

		if (distanceFromTarget > 16)
			moveTime = 0.02f;

		AttemptMove (xDir, yDir);
		return true;
	}

	protected override void AttemptMove (int xDir, int yDir){
		if (distanceFromTarget > 16) {
			xDir = Random.Range (-1, 1);
			if (xDir == 0)
				yDir = Random.Range (-1, 1);
		} else {
			moveTime = 0.1f;	
			if (Mathf.Abs (target.position.x - transform.position.x) < Mathf.Abs (target.position.y - transform.position.y) )
				yDir = target.position.y > transform.position.y ? 1 : -1;
			else
				xDir = target.position.x > transform.position.x ? 1 : -1;
		}

		if (Stuck (xDir, yDir)) {
			if(yDir == 0){
				yDir = target.position.y > transform.position.y ? 1 : -1;
				xDir = 0;
			} else {
				xDir = target.position.x > transform.position.x ? 1 : -1;
				yDir = 0;
			}
				
			if(Stuck(xDir, yDir)){
				xDir = Random.Range (-1, 1);
				if (xDir == 0)
					yDir = Random.Range (-1, 1);
				Stuck(xDir, yDir);
			}
		}
	}

	private bool Stuck(int xDir, int yDir){
		RaycastHit2D hit;
		bool canMove = Move (xDir, yDir, out hit);

		if (hit.transform == null)
			return false;

		InteractableObject hitObject = hit.transform.GetComponent (typeof(InteractableObject)) as InteractableObject;

		if (!canMove && hitObject != null) {
			if (hitObject is OuterWall || hitObject is Enemy)
				return true;
			Interact (hitObject);
		}
		return false;
	}

	protected override void Interact(InteractableObject hitObject){
		base.Interact (hitObject);
		if (hitObject is Player) {
			Player hitPlayer = hitObject as Player;
			hitPlayer.LoseFood (playerDamage);
		}
		if(hitObject is Player || hitObject is Wall)
			animator.SetTrigger ("enemyAttack");
	}

	public void DamageEnemy(int loss){
		hp -= loss;
		if (hp <= 0)
			GameObject.Destroy (gameObject);
	}

	private void GetDistanceFromTarget(){
		distanceFromTarget = (int)(Mathf.Abs (target.position.x - transform.position.x) + Mathf.Abs (target.position.y - transform.position.y));
	}

	private bool IsWaiting(){
		int random = Random.Range (0, 100);
		if (random + 1 < skipChance)
			return true;
		return false;
	}

	protected override void MakeMove(Vector2 end){
		transform.position = end;
	}
}
