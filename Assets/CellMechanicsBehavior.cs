using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.IO;


public class CellMechanicsBehavior : MonoBehaviour {


    //public CellMechanics cellMechanics;


    public Rigidbody rb;

    private ArrayList collidingCells = new ArrayList();

    private Vector3 cellBMContactVertex;
    Matrix4x4 transformMatrixBM;

    private GameObject basementMembrane;




    // Use this for initialization
    void Start() {

        rb = GetComponent<Rigidbody>();
        basementMembrane = GameObject.Find("BM");

    


        transformMatrixBM = basementMembrane.transform.localToWorldMatrix;
    
    }



    // Update is called once per frame
    public void FixedUpdate() {

        if (!Globals.animationRunning)
            return;

        CellPhenotype phenotype = GetComponent<CellBehavior>().phenotype;

        float dist;
        Vector3 distVec;

        float radius = phenotype.volume.GetRadius();

        Vector3 fAdhesionCC = ComputeCellCellAdhesion(radius);

        MeshFilter [] meshFilters = basementMembrane.GetComponentsInChildren<MeshFilter>();

        Vector3 fAdhesionBM = Vector3.zero;
        foreach (MeshFilter mf in meshFilters) {
            cellBMContactVertex = GetCellBMContact(mf.mesh, out dist, out distVec);
            fAdhesionBM += phenotype.cellMechanics.ComputeCellBMAdhesion(radius, dist, distVec);
        }
         


        rb.AddForce(fAdhesionCC + fAdhesionBM, ForceMode.Acceleration);


        //motility                   
        rb.velocity += phenotype.cellMechanics.ComputeMotility(Time.deltaTime);

    }

   
    //Requires other cells' components so computed here
    private Vector3 ComputeCellCellAdhesion(float radius) {

        Vector3 f = Vector3.zero;
        CellPhenotype phenotype = GetComponent<CellBehavior>().phenotype;


        foreach (GameObject other in this.collidingCells) {
            if (other && other.GetComponent<CellBehavior>()) {
                CellMechanics otherMechanics = other.GetComponent<CellBehavior>().phenotype.cellMechanics;
                CellPhenotype otherPhenotype = other.GetComponent<CellBehavior>().phenotype;
                float rOther = otherPhenotype.volume.GetRadius();

                Vector3 distVec = other.transform.position - this.transform.position;
                float dist = Vector3.Magnitude(distVec);

                float maxAdhesiveInteractiveDistance = phenotype.cellMechanics.relativeMaxAdhesionDistance * radius + otherMechanics.relativeMaxAdhesionDistance * rOther;

                if (dist < maxAdhesiveInteractiveDistance)
                    f += Mathf.Sqrt(phenotype.cellMechanics.cellCellAdhesionStrength * otherMechanics.cellCellAdhesionStrength) * Mathf.Pow((1f - dist / maxAdhesiveInteractiveDistance), 2) * distVec;
            }
        }

        return f;
    }



    /***
     * Returns the closest vertex position on the mesh without transformation to global coordinates
     */
    private Vector3 GetCellBMContact(Mesh bmMesh, out float dist, out Vector3 distVec) {
        
        dist = float.MaxValue;
        distVec = new Vector3(dist, dist, dist);
        Vector3 contactVertex = basementMembrane.transform.position;

        foreach (Vector3 vertex in bmMesh.vertices) {
            Vector3 tmpVec = transformMatrixBM.MultiplyPoint3x4(vertex) - transform.position;
            float tmp = Vector3.Magnitude(tmpVec);
            if (tmp < dist) {
                dist = tmp;
                distVec = tmpVec;
                contactVertex = vertex;
            }
        }
        return contactVertex;
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
        CellPhenotype phenotype = GetComponent<CellBehavior>().phenotype;

        if (Application.isPlaying && Globals.animationRunning) {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position + phenotype.cellMechanics.fAdhesionCC);

            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + phenotype.cellMechanics.fAdhesionBM);

            Gizmos.DrawSphere(transformMatrixBM.MultiplyPoint3x4(cellBMContactVertex), 1f);
        }

        //if (basementMembrane) {
        //    Mesh bmMesh = basementMembrane.GetComponent<MeshFilter>().mesh;


        //    Gizmos.color = Color.blue;

        //    foreach (Vector3 vertex in bmMesh.vertices) {
                
        //        Gizmos.DrawSphere(transformMatrixBM.MultiplyPoint3x4(vertex), 1f);
        //    }
        //}
    }
}
