
using UnityEngine;


[System.Serializable]
public class CellPhenotype {

    public int id;

    public Volume volume;
    public CellMechanics cellMechanics;

    public bool isDying = false;


    public float calcificationRate;

    //Properties to store transform information so that they can be loaded from file
    public Vector3 position; 
    public Vector3 scale; 
    public Quaternion rotation;
    public Color color;
}
