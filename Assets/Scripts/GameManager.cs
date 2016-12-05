using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

	public float levelStartDelay = 2f;
	public float turnDelay = 0.02f;
	public static GameManager instance = null;
	public BoardCreator boardScript;
	public int playerFoodPoints = 100;
	[HideInInspector] public bool playersTurn = true;
	public Text foodText = null;

	private Text levelText;
	private GameObject levelImage;
	private bool doingSetup;
	private int level = 1;
	public Player player;
	private List<Enemy> enemies;
	private bool enemiesMoving;

	// Use this for initialization
	void Awake () {
		if (instance == null)
			instance = this;
		else if (instance != this)
			Destroy (gameObject);

		DontDestroyOnLoad (gameObject);

		enemies = new List<Enemy> ();
		boardScript = GetComponent<BoardCreator> ();
		InitGame ();
	}

	void Start()
	{
		SceneManager.sceneLoaded += delegate (Scene scene, LoadSceneMode mode)
		{
			level++;
			InitGame();
		};
	}

	void InitGame(){
		doingSetup = true;

		levelImage = GameObject.Find ("LevelImage");
		levelText = GameObject.Find ("LevelText").GetComponent<Text> ();
		levelText.text = "Level " + level;
		levelImage.SetActive(true);
		Invoke ("HideLevelImage", levelStartDelay);

		foodText = GameObject.Find ("FoodText").GetComponent<Text> ();
		player = GameObject.Find ("Player").GetComponent<Player> ();
		enemies.Clear ();
		boardScript.SetupScene (level);
	}

	private void HideLevelImage(){
		levelImage.SetActive (false);
		doingSetup = false;
	}

	public void GameOver(){
		levelText.text = "After " + level + " days, you starved.";
		levelImage.SetActive (true);
		enabled = false;
	}

	// Update is called once per frame
	void Update () {
		if (playersTurn || enemiesMoving || doingSetup)
			return;

		StartCoroutine (MoveEnemies ());
	}

	public void AddEnemyToList(Enemy script){
		enemies.Add (script);
	}

	IEnumerator MoveEnemies ()
	{
		enemiesMoving = true;
		yield return new WaitForSeconds (turnDelay);
		if (enemies.Count == 0)
			yield return new WaitForSeconds (turnDelay);

		for (int i = 0; i < enemies.Count; i++) {
			if (enemies [i] != null)
				enemies [i].MoveEnemy();
			yield return new WaitForSeconds (enemies [i].moveTime);
		}
			
		playersTurn = true;
		enemiesMoving = false;
	}

	public void NextLevel(){
		SceneManager.LoadScene (SceneManager.GetActiveScene ().name);
	}

	public Player GetPlayer(){
		return player;
	}

	public void SetFoodText(int text){
		foodText.text = "Food: " + text;
	}
}
