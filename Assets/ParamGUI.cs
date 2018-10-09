using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;


public class ParamGUI : MonoBehaviour {
    private Vector2 scrollPosition;

    public bool mechanicsMode = true;
    public bool isMotile = false;
    public string maxCellCnt = Globals.maxCellCnt.ToString();




    public int shape;
    public float cellRadius = 1; //default radius by the volume 
    public float sphereRadius = 5;


    public float timeConst = 0.1f;

    GameObject[] cells;

	
	void Start () {
        mechanicsMode = true;
        isMotile = false;
	}

	void OnGUI () {
        
		scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(250), GUILayout.Height(Screen.height * 0.98f));



        //Generate cells
        GUILayout.BeginHorizontal();

        GUILayout.Label("Cell radius:");
        cellRadius = float.Parse(GUILayout.TextField(cellRadius.ToString(), 25));

        GUILayout.Label("Sphere radius:");
        sphereRadius = float.Parse(GUILayout.TextField(sphereRadius.ToString(), 25));

        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Generate cells"))
            GenerateCells();
        GUILayout.EndHorizontal();


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
            
            cells = GameObject.FindGameObjectsWithTag("Cell");

            foreach(GameObject cell in cells){
                cell.GetComponent<CellMechanicsBehavior>().enabled = true;
            }

        }
        else{
            cells = GameObject.FindGameObjectsWithTag("Cell");

            foreach (GameObject cell in cells) {
                cell.GetComponent<CellMechanicsBehavior>().enabled = false;
            }
        }
        GUILayout.EndHorizontal();


        //Enable/disable motility
        GUILayout.BeginHorizontal();
        isMotile = GUILayout.Toggle(isMotile, "Enable motility");

        cells = GameObject.FindGameObjectsWithTag("Cell");

        foreach (GameObject cell in cells) {
            cell.GetComponent<CellBehavior>().SetMotility(isMotile);
        }

        GUILayout.EndHorizontal();


        //Start animation
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Start"))
            RunAnimation();


        if (GUILayout.Button("Pause"))
            PauseAnimation();

        if(GUILayout.Button("Stop")){
            StopAnimation();

        }

        GUILayout.EndHorizontal();

        //Start animation
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Save"))
            SaveCurrentState();

        if (GUILayout.Button("Load"))
            LoadState();
        
        GUILayout.EndHorizontal();



        //TIME CONSTANT
        GUILayout.Label("");
        GUILayout.BeginHorizontal();
        GUILayout.Label("Time constant (min):");
        timeConst = float.Parse(GUILayout.TextField(timeConst.ToString(), 5));
        Globals.timeConst = timeConst;
        GUILayout.EndHorizontal();


		GUILayout.EndScrollView();
	}

    void PauseAnimation(){
    
        Globals.animationRunning = false;
        //stop rigidbodies from running
        cells = GameObject.FindGameObjectsWithTag("Cell");
        foreach (GameObject cell in cells) {
            cell.GetComponent<Rigidbody>().detectCollisions = false;
        }
    }

    void RunAnimation() {
        
        Globals.animationRunning = true;
        Globals.timePassedInAnimation = 0f;

        //stop rigidbodies from running
        cells = GameObject.FindGameObjectsWithTag("Cell");
        foreach (GameObject cell in cells) {
            cell.GetComponent<Rigidbody>().detectCollisions = true;           
        }
    }

    void StopAnimation() {
        Globals.timePassedInAnimation = 0f;

        PauseAnimation();

        ClearAllCells();
        Globals.cellCnt = 1;
        //Create a new cell
        GameObject cell1 = (GameObject)Instantiate(UnityEngine.Resources.Load("Cell"), Vector3.zero, Quaternion.identity);
        cell1.name = "Cell1";        
    }

    void ClearAllCells(){
        //Delete all gameobjects
        cells = GameObject.FindGameObjectsWithTag("Cell");
        foreach (GameObject cell in cells) {
            DestroyImmediate(cell);
        }
    }

    void SaveCurrentState() {
        
        System.Xml.Serialization.XmlSerializer writer = new System.Xml.Serialization.XmlSerializer(typeof(List<CellPhenotype>));  

        FileStream file = File.Create("currentState.xml");  

        cells = GameObject.FindGameObjectsWithTag("Cell");
        List<CellPhenotype> cellPhenotypes = new List<CellPhenotype>();
        foreach (GameObject cell in cells) {
            cellPhenotypes.Add(cell.GetComponent<CellBehavior>().phenotype);
        }

        writer.Serialize(file, cellPhenotypes);

        file.Close();  
    }

    void LoadState(){
        PauseAnimation();

        System.Xml.Serialization.XmlSerializer reader = new System.Xml.Serialization.XmlSerializer(typeof(List<CellPhenotype>));  
        StreamReader file = new StreamReader("currentState.xml");  
        List<CellPhenotype> cellPhenotypes =  (List<CellPhenotype>)reader.Deserialize(file);  
        file.Close();

        //Clear current state
        ClearAllCells();

        //Load from file
        foreach(CellPhenotype phenotype in cellPhenotypes){
            GameObject cell = (GameObject)Instantiate(Resources.Load("Cell"), Vector3.zero, Quaternion.identity);
            cell.GetComponent<CellBehavior>().phenotype = phenotype;

            cell.transform.Translate(phenotype.position);
            cell.transform.localScale = phenotype.scale;
            cell.transform.rotation = phenotype.rotation;
            cell.GetComponent<Renderer>().material.color = phenotype.color;

        }

    }


    void GenerateCells(){
        if (shape == Globals.SPHEROID) {
            int xc = 0;
            int yc = 0;
            int zc = 0;

            float xSpacing = this.cellRadius * Mathf.Sqrt(3);
            float ySpacing = this.cellRadius * 2;
            float zSpacing = this.cellRadius * Mathf.Sqrt(3);
            Vector3 tempPoint = Vector3.zero;
            for (float z = -sphereRadius; z < sphereRadius; z += zSpacing, zc++)
                for (float x = -sphereRadius; x < sphereRadius; x += xSpacing, xc++)
                    for (float y = -sphereRadius; y < sphereRadius; y += ySpacing, yc++) {
                        tempPoint.x = x + (zc % 2) * 0.5f * cellRadius;
                        tempPoint.y = y + (xc % 2) * cellRadius;
                        tempPoint.z = z;

                        if (Vector3.Magnitude(tempPoint) < sphereRadius) {

                            GameObject cell1 = GameObject.FindGameObjectsWithTag("Cell")[0];
                            GameObject clone = (GameObject)Instantiate(cell1.gameObject, cell1.transform.position, cell1.transform.rotation);
                            clone.GetComponent<CellBehavior>().SetId(++Globals.cellCnt);
                            clone.name = "Cell" + clone.GetComponent<CellBehavior>().GetId();

                            clone.transform.Translate(tempPoint.x, tempPoint.y, tempPoint.z);

                        }

                    }
        }
        
    }

}	
	
