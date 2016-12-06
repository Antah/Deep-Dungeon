using UnityEngine;
using System.Collections;

public abstract class MovingObject : InteractableObject {

	public float moveTime = 0.1f;
	public LayerMask blockingLayer;
	public int wallDamage = 1;

	protected int distanceFromTarget = 0;
	private BoxCollider2D boxCollider;
	private Rigidbody2D rb2D;

	// Use this for initialization
	protected virtual void Start () {
		boxCollider = GetComponent<BoxCollider2D> ();
		rb2D = GetComponent<Rigidbody2D> ();
	}

	protected bool Move(int xDir, int yDir, out RaycastHit2D hit){
		Vector2 start = transform.position;
		Vector2 end = start +new Vector2(xDir, yDir);

		boxCollider.enabled = false;
		hit = Physics2D.Linecast(start, end, blockingLayer);
		boxCollider.enabled = true;

		if(hit.transform == null){
			MakeMove (end);
			return true;
		}
		return false;
	}

	protected virtual void AttemptMove(int xDir, int yDir)
	{
		RaycastHit2D hit;
		bool canMove = Move (xDir, yDir, out hit);

		if (hit.transform == null)
			return;

		InteractableObject hitObject = hit.transform.GetComponent (typeof(InteractableObject)) as InteractableObject;

		if (!canMove && hitObject != null )
			Interact(hitObject);
	}

	protected virtual void Interact(InteractableObject hitObject){
		if (hitObject is Wall) {
			Wall hitWall = hitObject as Wall;
			hitWall.DamageWall (wallDamage, distanceFromTarget);
		}
	}

	protected virtual void MakeMove(Vector2 end){
		StartCoroutine (SmoothMovement (end));
	}

	protected IEnumerator SmoothMovement(Vector3 end){
		float sqrRemainingDistance = (transform.position - end).sqrMagnitude;

		while (sqrRemainingDistance > float.Epsilon) {
			Vector3 newPosition = Vector3.MoveTowards (rb2D.position, end, 1f / moveTime * Time.deltaTime);
			rb2D.MovePosition (newPosition);
			sqrRemainingDistance = (transform.position - end).sqrMagnitude;
			yield return null;
		}
	}
}
