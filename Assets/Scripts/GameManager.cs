using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class GameManager : MonoBehaviour {

	public float levelStartDelay = 2f;
	public float turnDelay;
	public static GameManager instance = null;
    public LevelGenerator boardScript;
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
        Simple, CellularAutomaton, BinaryTree, DelunayTriangulation
    }

    void Awake () {
		if (instance == null)
			instance = this;
		else if (instance != this)
			Destroy (gameObject);

		DontDestroyOnLoad (gameObject);

		turnDelay = 0.2f;
		enemies = new List<Enemy> ();

        TestLogger.CreateFile();

        SceneManager.sceneLoaded += delegate (Scene scene, LoadSceneMode mode)
		{
            UpdateGeneratorType();
            level++;
			InitGame();
			playersTurn = true;
		};
	}

    public void UpdateGeneratorType()
    {
        switch (generatorType)
        {
            case GeneratorType.Simple:
                boardScript = GetComponent<OGBoardCreator>();
                break;
            case GeneratorType.CellularAutomaton:
                boardScript = GetComponent<CALevelGenerator>();
                break;
            case GeneratorType.BinaryTree:
                boardScript = GetComponent<BTLevelGenerator>();
                break;
            case GeneratorType.DelunayTriangulation:
                boardScript = GetComponent<DTLevelGenerator>();
                break;
        }
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
        UIBaseInterface.instance.SetupUI();
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

    public void SetLevelGenerator(String gt)
    {
        switch (gt)
        {
            case "ca":
                generatorType = GameManager.GeneratorType.CellularAutomaton;
                break;
            case "bt":
                generatorType = GameManager.GeneratorType.BinaryTree;
                break;
            case "dt":
                generatorType = GameManager.GeneratorType.DelunayTriangulation;
                break;
            case "sg":
                generatorType = GameManager.GeneratorType.Simple;
                break;
            default:
                generatorType = GameManager.GeneratorType.DelunayTriangulation;
                break;
        }
    }
}
