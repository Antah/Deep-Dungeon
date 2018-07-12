using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Loader : MonoBehaviour {
	public AudioClip doorSound;
	public GameManager gameManager;
	public SoundManager soundManager;
    public UIBaseInterface baseUI;
    public UIBTSettings btUI;
    public UICASettings caUI;
    public UIDTSettings dtUI;
    public UISGSettings sgUI;

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

	public void ExitGame(){
		Application.Quit ();
	}
}
