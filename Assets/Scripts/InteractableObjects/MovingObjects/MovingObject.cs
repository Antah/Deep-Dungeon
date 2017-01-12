using UnityEngine;
using System.Collections;

public abstract class MovingObject : InteractableObject {

	public float moveTime = 0.1f;
	public LayerMask blockingLayer;
	public int digDamage = 1, attackDamage = 10;

	protected BoxCollider2D boxCollider;
	protected Rigidbody2D rb2D;
	protected SpriteRenderer spriteRenderer;

	protected virtual void Start () {
		boxCollider = GetComponent<BoxCollider2D> ();
		rb2D = GetComponent<Rigidbody2D> ();
		spriteRenderer = GetComponent<SpriteRenderer> ();
	}

	protected virtual void MoveOrInteract(int xDir, int yDir){
	}

	protected RaycastHit2D CheckForCollision(int xDir, int yDir){
		Vector2 start = transform.position;
		Vector2 end = start + new Vector2(xDir, yDir);

		boxCollider.enabled = false;
		RaycastHit2D hit = Physics2D.Linecast(start, end, blockingLayer);
		boxCollider.enabled = true;

		return hit;
	}

	protected virtual void Interact(InteractableObject hitObject){
		if (hitObject is MovingObject) {
			hitObject.Hit (attackDamage);
		}
		else {
			hitObject.Hit (digDamage);
		}
	}

	protected virtual void Move(int xDir, int yDir){
		UpdateSpriteDirection (xDir);
		Vector3 end = transform.position + new Vector3(xDir, yDir, 0);
		transform.position = end;
	}

	protected virtual void UpdateSpriteDirection(int xDir){
		if (xDir > 0)
			spriteRenderer.flipX = false;
		else if(xDir < 0)
			spriteRenderer.flipX = true;
	}
}
