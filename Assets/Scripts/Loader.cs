using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Loader : MonoBehaviour {
	public AudioClip doorSound;
	public GameObject gameManager;
	public SoundManager soundManager;

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
