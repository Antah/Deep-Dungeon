using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Player : MovingObject {

	public int enemyDamage = 1;
	public int pointsPerFood = 10;
	public int pointsPerSoda = 20;
	public float restartLevelDelay = 1f;
	public int food;

	public AudioClip moveSound1;
	public AudioClip moveSound2;
	public AudioClip eatSound1;
	public AudioClip eatSound2;
	public AudioClip drinkSound1;
	public AudioClip drinkSound2;
	public AudioClip gameOverSound;

	private Animator animator;
	private Vector2 touchOrigin = -Vector2.one;

	// Use this for initialization
	protected override void Start () {
		animator = GetComponent<Animator> ();

		food = GameManager.instance.playerFoodPoints;
		GameManager.instance.SetFoodText(food);

		base.Start ();
	}

	void OnDisable(){
		GameManager.instance.playerFoodPoints = food;
	}
	
	// Update is called once per frame
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
			AttemptMove (horizontal, vertical);
	}

	protected override void AttemptMove (int xDir, int yDir){
		food--;
		GameManager.instance.SetFoodText(food);

		base.AttemptMove(xDir, yDir);

		CheckIfGameOver ();

		GameManager.instance.playersTurn = false;
	}

	private void OnTriggerEnter2D (Collider2D other){
		if (other.tag == "Exit") {
			Invoke ("Restart", restartLevelDelay);
			enabled = false;
		} else if (other.tag == "Food") {
			food += pointsPerFood;
			GameManager.instance.SetFoodText(food);
			other.gameObject.SetActive (false);
		} else if (other.tag == "Soda") {
			food += pointsPerSoda;
			GameManager.instance.SetFoodText(food);
			other.gameObject.SetActive (false);
		}
	}

	protected override void Interact(InteractableObject hitObject){
		base.Interact (hitObject);
		if (hitObject is Enemy) {
			Enemy hitEnemy = hitObject as Enemy;
			hitEnemy.DamageEnemy (enemyDamage);
		}
		if(hitObject is Enemy || hitObject is Wall)
			animator.SetTrigger ("playerChop");
	}

	private void Restart(){
		//SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
		GameManager.instance.NextLevel ();
	}

	public void LoseFood (int loss){
		animator.SetTrigger ("playerHit");
		food -= loss;
		GameManager.instance.SetFoodText(food);
		CheckIfGameOver();
	}

	private void CheckIfGameOver(){
		if (food <= 0){
			SoundManager.instance.PlaySingle (gameOverSound);
			SoundManager.instance.musicSource.Stop ();
			GameManager.instance.GameOver ();
		}
	}
}
