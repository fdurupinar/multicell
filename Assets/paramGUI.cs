using UnityEngine;
using System.Collections.Generic;

//Attached to the agent
public class ParamGUI : MonoBehaviour {
	private Vector2 _scrollPosition;

	// Use this for initialization
	void Start () {

	}

	void OnGUI () {
		_scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUILayout.Width(100), GUILayout.Height(Screen.height * 0.98f));

		GUILayout.Label("Volume");


//		GUILayout.HorizontalSlider().Truncate(1);

		GUILayout.EndScrollView();
	}



}	
	
