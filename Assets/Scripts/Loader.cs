using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Loader : MonoBehaviour {
	public AudioClip doorSound;
	public GameObject gameManager;
	public SoundManager soundManager;
    public GameObject boardManager;
    public GameManager.GeneratorType generator = GameManager.GeneratorType.DelunayTriangulation;

    void Awake () {
		if (GameManager.instance == null)
			Instantiate (gameManager);
		if (SoundManager.instance == null)
			Instantiate (soundManager);    
    }

	public void EnterGame(){
		GameManager.instance.playerHealth = 100;
		GameManager.instance.level = 0;
        GameManager.instance.gameInProgress = true;
		SceneManager.LoadScene ("Main");
	}

    public void SetLevelGenerator(String gt)
    {
        switch (gt)
        {
            case "ca":
                generator = GameManager.GeneratorType.CellularAutomaton;
                break;
            case "bt":
                generator = GameManager.GeneratorType.BinaryTree;
                break;
            case "dt":
                generator = GameManager.GeneratorType.DelunayTriangulation;
                break;
            case "sim":
                generator = GameManager.GeneratorType.Simple;
                break;
            default:
                generator = GameManager.GeneratorType.DelunayTriangulation;
                break;
        }

        GameManager.instance.generatorType = generator;
    }

	public void ExitGame(){
		Application.Quit ();
	}
}
