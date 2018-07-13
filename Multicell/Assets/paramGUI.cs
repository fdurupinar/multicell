using UnityEngine;
using System;
using System.Collections.Generic;


public class ParamGUI : MonoBehaviour {
    private Vector2 scrollPosition;

    public bool mechanicsMode = true;
    public string maxCellCnt = Globals.maxCellCnt.ToString();

	
	void Start () {
        mechanicsMode = true;
	}

	void OnGUI () {
		scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(200), GUILayout.Height(Screen.height * 0.98f));

        GUILayout.BeginHorizontal();
        Globals.animateDivision = GUILayout.Toggle(Globals.animateDivision, "Animate division");
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Maximum cell count:");
        maxCellCnt = GUILayout.TextField(maxCellCnt, 25);

        try {
            Globals.maxCellCnt = Int32.Parse(maxCellCnt);
        }
        catch(Exception e){
            Debug.Log(e + "Cell count should be an integer");
            Globals.maxCellCnt = 1000;
        }
        GUILayout.EndHorizontal();
     

        GUILayout.BeginHorizontal();


        mechanicsMode = GUILayout.Toggle(mechanicsMode, "Include mechanics");

        if(mechanicsMode){ //enable all the machanics components of cells
            
            GameObject[] cells = GameObject.FindGameObjectsWithTag("Cell");

            foreach(GameObject cell in cells){
                cell.GetComponent<CellMechanics>().enabled = true;
            }

        }
        else{
            GameObject[] cells = GameObject.FindGameObjectsWithTag("Cell");

            foreach (GameObject cell in cells) {
                cell.GetComponent<CellMechanics>().enabled = false;
            }
        }


        GUILayout.EndHorizontal();
     

		GUILayout.EndScrollView();
	}



}	
	
