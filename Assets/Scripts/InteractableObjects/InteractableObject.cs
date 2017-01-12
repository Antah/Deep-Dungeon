using UnityEngine;

public abstract class InteractableObject : MonoBehaviour {
	public int health;

	protected AudioSource efxSource;

	void Awake(){
		efxSource = GetComponent<AudioSource> ();
	}

	public virtual void SteppedOn(){}

	public virtual void Hit(int damage){
		ChangeHealth (-damage);
	}

	public virtual void ChangeHealth(int change){
		health += change;
		if (health <= 0)
			Death ();
	}

	protected virtual void Death(){
		Destroy (gameObject);
	}

	public int GetDistanceFromTarget(Transform target){
		return (int)(Mathf.Abs (target.position.x - transform.position.x) + Mathf.Abs (target.position.y - transform.position.y));
	}
}
