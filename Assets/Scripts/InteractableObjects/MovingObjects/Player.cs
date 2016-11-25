using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Player : MovingObject {

	public int enemyDamage = 1;
	public int pointsPerFood = 10;
	public int pointsPerSoda = 20;
	public float restartLevelDelay = 1f;
	public Text foodText;

	public AudioClip moveSound1;
	public AudioClip moveSound2;
	public AudioClip eatSound1;
	public AudioClip eatSound2;
	public AudioClip drinkSound1;
	public AudioClip drinkSound2;
	public AudioClip gameOverSound;

	private Animator animator;
	public int food;

	// Use this for initialization
	protected override void Start () {
		animator = GetComponent<Animator> ();

		food = GameManager.instance.playerFoodPoints;

		foodText = GameManager.instance.foodText;
		foodText.text = "Food: " + food;

		base.Start ();
	}

	void OnDisable(){
		GameManager.instance.playerFoodPoints = food;
	}
	
	// Update is called once per frame
	void Update () {
		if (!GameManager.instance.playersTurn)
			return;

		int horizontal = 0, verical = 0;

		horizontal = (int)Input.GetAxisRaw ("Horizontal");
		verical = (int)Input.GetAxisRaw ("Vertical");

		if (horizontal != 0)
			verical = 0;

		if (horizontal != 0 || verical != 0)
			AttemptMove (horizontal, verical);
	}

	protected override void AttemptMove (int xDir, int yDir){
		food--;
		foodText.text = "Food: " + food;

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
			foodText.text = "Food: " + food;
			other.gameObject.SetActive (false);
		} else if (other.tag == "Soda") {
			food += pointsPerSoda;
			foodText.text = "Food: " + food;
			other.gameObject.SetActive (false);
		}
	}

	protected override void Interact(InteractableObject hitObject){
		base.Interact (hitObject);
		if (hitObject is Enemy) {
			Enemy hitEnemy = hitObject as Enemy;
			hitEnemy.DamageEnemy (enemyDamage);
		}
		animator.SetTrigger ("playerChop");
	}

	private void Restart(){
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}

	public void LoseFood (int loss){
		animator.SetTrigger ("playerHit");
		food -= loss;
		foodText.text = "Food: " + food;
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
