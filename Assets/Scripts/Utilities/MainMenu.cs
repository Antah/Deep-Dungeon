using UnityEngine;
using System.Collections;

public class MainMenu : MonoBehaviour {
	public Texture background;

	void Start(){
		Camera.main.aspect = 544f / 416f;
	}

	void OnGUI(){
	}
}
