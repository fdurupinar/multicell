using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellFactory : MonoBehaviour {

    public int cellCnt;
    public GameObject[] cells;
    private GameObject _cellFactory;

    // Use this for initialization
    void Start() {

        cells = new GameObject[cellCnt];

        for (int i = 0; i < this.cellCnt; i++) {
            cells[i] = (GameObject)Instantiate(UnityEngine.Resources.Load("Cell"), transform.position, transform.rotation);



        }

        //Assign some cells velocity
        for (int i = 0; i < 10; i++)
            cells[i].GetComponent<CellMechanics>().v = Utilities.GetRandomVector(0.1f, 1f);

    }

    //	// Update is called once per frame
    //	void Update () {
    //		
    //	}
}