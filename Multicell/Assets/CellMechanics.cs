using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;


public class CellMechanics : MonoBehaviour {


    public Vector3 orientation;

    public CellPhenotype phenotype;
    public Rigidbody rb;

    public float cellCellAdhesionStrength = 0.4f * 60f / Globals.timeConst;
    public float cellBMAdhesionStrength = 4.0f * 60f / Globals.timeConst;

    public float cellCellRepulsionStrength = 10f * 60f /Globals.timeConst;
    public float cellBMRepulsionStrength = 10.0f * 60f / Globals.timeConst;

    // this is a multiple of the cell (equivalent) radius
    public float relativeMaxAdhesionDistance = 1.25f;




    //includes motility
    public bool isMotile = false; 
    
    public float persistenceTime = 1f;
    public float migrationSpeed = 1f;
    public Vector3 migrationBiasDirection = new Vector3(3f, 0f, 0f);
    public float migrationBias = 0f;
    public Vector3 motilityVector = new Vector3(3f, 0f, 0f);


    public ArrayList collidingCells = new ArrayList();

    public Vector3 fRepulsion;
    public Vector3 fAdhesion;

    // Use this for initialization
    void Start() {

        rb = GetComponent<Rigidbody>();


        phenotype = GetComponent<CellBehavior>().phenotype;

    }

    public void SetOrientation() {

        float theta = Utilities.GetRandomNumber(0, Mathf.PI * 2);
        float z = Utilities.GetRandomNumber(-1, 1);
        float temp = Mathf.Sqrt(1 - z * z);

        orientation = new Vector3(temp * Mathf.Cos(theta), temp * Mathf.Sin(theta), z);

    }

    // Update is called once per frame
    public void FixedUpdate() {

        fRepulsion = Vector3.zero;
        fAdhesion = Vector3.zero;


        foreach(GameObject other in collidingCells){
            CellMechanics otherMechanics = other.GetComponent<CellMechanics>();
            CellPhenotype otherPhenotype = other.GetComponent<CellBehavior>().phenotype;
            float rOther = otherPhenotype.volume.GetRadius();
            float rThis = phenotype.volume.GetRadius();
            float R = rOther + rThis;

            Vector3 distVec = other.transform.position - this.transform.position;
            float dist = Vector3.Magnitude(distVec);

            float rep = 1;

            if(dist < R){
                rep = (1f - dist / R) * (1f - dist / R);

              
                

            }
            fRepulsion += -rep * Mathf.Sqrt(cellCellRepulsionStrength * otherMechanics.cellCellRepulsionStrength)/dist * distVec ;


            float maxAdhesiveInteractiveDistance = relativeMaxAdhesionDistance * rThis + otherMechanics.relativeMaxAdhesionDistance  * rOther;


            if (dist < maxAdhesiveInteractiveDistance) {
                
                fAdhesion += Mathf.Sqrt(cellCellAdhesionStrength * otherMechanics.cellCellAdhesionStrength) * (1f - dist / maxAdhesiveInteractiveDistance) * distVec;
            }
        }

        //Repulsion is handled by the rigidBody controller automatically. 

        //rb.AddForce(fRepulsion + fAdhesion, ForceMode.Acceleration);


        rb.AddForce(fAdhesion, ForceMode.Acceleration);



    }

    private void OnTriggerEnter(Collider other) {
        
        if (other.gameObject.CompareTag("Cell") && !collidingCells.Contains(other))             
            collidingCells.Add(other.gameObject);
    }

    private void OnTriggerExit(Collider other) {        
        if (other.gameObject.CompareTag("Cell"))             
            collidingCells.Remove(other.gameObject);
    }


    private void OnDrawGizmosSelected() {
        

        Vector3 fVec = fRepulsion + fAdhesion;

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + fAdhesion);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + fRepulsion);


        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + fVec);

    }


}
