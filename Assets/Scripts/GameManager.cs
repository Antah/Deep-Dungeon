using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

	public float levelStartDelay = 2f;
	public float turnDelay;
	public static GameManager instance = null;
    public BoardCreator boardScript;
    public GeneratorType generatorType;
    public int playerHealth;
	public Text foodText = null;
	public int level;
	public bool gameInProgress;

	[HideInInspector] public bool playersTurn = true;

	private Text levelText;
	private GameObject levelImage;
	private bool doingSetup;
	public Player player;
	private List<Enemy> enemies;
	private bool enemiesMoving;

    public enum GeneratorType
    {
        RoomsAndCorridors, CellularAutomaton, BSP, DelunayTriangulation
    }

    void Awake () {
		if (instance == null)
			instance = this;
		else if (instance != this)
			DestroyObject (gameObject);

		DontDestroyOnLoad (gameObject);

		turnDelay = 0.2f;
		enemies = new List<Enemy> ();

        switch (generatorType)
        {
            case GeneratorType.RoomsAndCorridors:
                boardScript = GetComponent<OldBoardCreator>();
                break;
            case GeneratorType.CellularAutomaton:
                boardScript = GetComponent<CABoardCreator>();
                break;
            case GeneratorType.BSP:
                boardScript = GetComponent<BSP>();
                break;
            case GeneratorType.DelunayTriangulation:
                boardScript = GetComponent<Deluanay>();
                break;
        }
            

		SceneManager.sceneLoaded += delegate (Scene scene, LoadSceneMode mode)
		{
			level++;
			InitGame();
			playersTurn = true;
		};
	}

	void InitGame(){
		if (!gameInProgress)
			return;
		doingSetup = true;

		foodText = GameObject.Find ("FoodText").GetComponent<Text> ();
		player = GameObject.Find ("Player").GetComponent<Player> ();
		levelImage = GameObject.Find ("LevelImage");
		levelText = GameObject.Find ("LevelText").GetComponent<Text> ();
		levelText.text = "Level " + level;
		levelImage.SetActive(true);

		enemies.Clear ();
        boardScript.SetupScene (level);

		Invoke ("HideLevelImage", levelStartDelay);
	}

	private void HideLevelImage(){
		levelImage.SetActive (false);
		doingSetup = false;
	}

	public void GameOver(){
		GameManager.instance.gameInProgress = false;
		levelText.text = "After " + level + " levels, you have died.";
		levelImage.SetActive (true);
		StartCoroutine (ExitToMenu ());
	}
		
	void Update () {
		if (playersTurn || enemiesMoving || doingSetup)
			return;

		StartCoroutine (MoveEnemies ());
	}

	public void AddEnemyToList(Enemy enemy){
		enemies.Add (enemy);
	}

	IEnumerator ExitToMenu(){
		yield return new WaitForSeconds (3f);
		SceneManager.LoadScene ("MainMenu");
		SoundManager.instance.musicSource.Play ();
	}

	IEnumerator MoveEnemies ()
	{
		enemiesMoving = true;
		yield return new WaitForSeconds (turnDelay/2);

		for (int i = 0; i < enemies.Count; i++) {
			if (enemies [i] != null)
				enemies [i].MoveEnemy();
		}
			
		yield return new WaitForSeconds (turnDelay/2);
		playersTurn = true;
		enemiesMoving = false;
	}

	public void NextLevel(){
		SceneManager.LoadScene (SceneManager.GetActiveScene ().name);
	}

	public Player GetPlayer(){
		return player;
	}

	public void SetLifeText(int text){
		foodText.text = "Life: " + text;
	}
}
