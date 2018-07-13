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

        //Animate division
        GUILayout.BeginHorizontal();
        Globals.animateDivision = GUILayout.Toggle(Globals.animateDivision, "Animate division");
        GUILayout.EndHorizontal();

        //Maximum cell count
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
     

        //Enable/disable mechanics component
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

        //Start animation
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Start"))
            RunAnimation();


        if (GUILayout.Button("Pause"))
            PauseAnimation();

        if(GUILayout.Button("Stop")){
            PauseAnimation();


            //Delete all gameobjects
            GameObject[] cells = GameObject.FindGameObjectsWithTag("Cell");
            foreach (GameObject cell in cells) {
                DestroyImmediate(cell);
            }

            Globals.cellCnt = 1;
            //Create a new cell
            GameObject cell1 = (GameObject)Instantiate(UnityEngine.Resources.Load("Cell"), Vector3.zero, Quaternion.identity);
            cell1.name = "Cell1";
        

        }
        GUILayout.EndHorizontal();


     

		GUILayout.EndScrollView();
	}

    void PauseAnimation(){
    
        Globals.animationRunning = false;
        //stop rigidbodies from running
        GameObject[] cells = GameObject.FindGameObjectsWithTag("Cell");
        foreach (GameObject cell in cells) {
            cell.GetComponent<Rigidbody>().detectCollisions = false;
        }
    }

    void RunAnimation() {

        Globals.animationRunning = true;
        //stop rigidbodies from running
        GameObject[] cells = GameObject.FindGameObjectsWithTag("Cell");
        foreach (GameObject cell in cells) {
            cell.GetComponent<Rigidbody>().detectCollisions = true;
           

        }
    }




}	
	
