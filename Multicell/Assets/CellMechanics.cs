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


    // this is a multiple of the cell (equivalent) radius
    public float relativeMaxAdhesionDistance = 1.25f;


    public Vector3 fAdhesionCC;
    public Vector3 fAdhesionBM;

    private GameObject basementMembrane;


    //includes motility
    public bool isMotile = false;

    public float persistenceTime = 1f;
    public float migrationSpeed = 1f;
    public Vector3 migrationBiasDirection = new Vector3(3f, 0f, 0f);
    public float migrationBias = 0f;
    public Vector3 motilityVector = new Vector3(3f, 0f, 0f);


    private ArrayList collidingCells = new ArrayList();

    private Vector3 cellBMContactVertex;
    Matrix4x4 transformMatrixBM;


    // Use this for initialization
    void Start() {

        rb = GetComponent<Rigidbody>();
        basementMembrane = GameObject.Find("BM");

        phenotype = GetComponent<CellBehavior>().phenotype;

        transformMatrixBM = basementMembrane.transform.localToWorldMatrix;

    }

    public void SetOrientation() {

        float theta = Utilities.GetRandomNumber(0, Mathf.PI * 2);
        float z = Utilities.GetRandomNumber(-1, 1);
        float temp = Mathf.Sqrt(1 - z * z);

        orientation = new Vector3(temp * Mathf.Cos(theta), temp * Mathf.Sin(theta), z);

    }

    // Update is called once per frame
    public void FixedUpdate() {

        if (!Globals.animationRunning)
            return;


        float dist;
        Vector3 distVec;


        float radius = phenotype.volume.GetRadius();
        fAdhesionCC = ComputeCellCellAdhesion(radius);


        cellBMContactVertex = GetCellBMContact(out dist, out distVec);

        fAdhesionBM = ComputeCellBMAdhesion(radius, dist, distVec);

        rb.AddForce(fAdhesionCC + fAdhesionBM, ForceMode.Acceleration);


        //Add force on basement membrane
        basementMembrane.GetComponent<MeshDeformer>().AddDeformingForce(cellBMContactVertex, Vector3.Magnitude(fAdhesionBM));


    }

    private Vector3 ComputeCellCellAdhesion(float radius) {

        Vector3 f = Vector3.zero;


        foreach (GameObject other in collidingCells) {
            CellMechanics otherMechanics = other.GetComponent<CellMechanics>();
            CellPhenotype otherPhenotype = other.GetComponent<CellBehavior>().phenotype;
            float rOther = otherPhenotype.volume.GetRadius();



            Vector3 distVec = other.transform.position - this.transform.position;
            float dist = Vector3.Magnitude(distVec);

            float maxAdhesiveInteractiveDistance = relativeMaxAdhesionDistance * radius + otherMechanics.relativeMaxAdhesionDistance * rOther;


            if (dist < maxAdhesiveInteractiveDistance) {
                f += Mathf.Sqrt(cellCellAdhesionStrength * otherMechanics.cellCellAdhesionStrength) * Mathf.Pow((1f - dist / maxAdhesiveInteractiveDistance), 2) * distVec;
            }
        }

        return f;
    }


    private Vector3 GetCellBMContact(out float dist, out Vector3 distVec) {
        Mesh bmMesh = basementMembrane.GetComponent<MeshFilter>().mesh;
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
    private Vector3 ComputeCellBMAdhesion(float radius, float dist, Vector3 distVec) {

        Vector3 f = Vector3.zero;

        float maxAdhesiveInteractiveDistance = relativeMaxAdhesionDistance * radius;

        if (dist < maxAdhesiveInteractiveDistance)
            f = cellBMAdhesionStrength * Mathf.Pow((1f - dist / maxAdhesiveInteractiveDistance), 2) * distVec;


        return f;

    }
    private void AddForceOnBasementMembrane(Vector3 contactPoint, float fMagnitude) {
        basementMembrane.GetComponent<MeshDeformer>().AddDeformingForce(contactPoint, fMagnitude);
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

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + fAdhesionCC);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + fAdhesionBM);

        Gizmos.DrawSphere(transformMatrixBM.MultiplyPoint3x4(cellBMContactVertex), 1f);



        //if (basementMembrane) {
        //    Mesh bmMesh = basementMembrane.GetComponent<MeshFilter>().mesh;


        //    Gizmos.color = Color.blue;

        //    foreach (Vector3 vertex in bmMesh.vertices) {
                
        //        Gizmos.DrawSphere(transformMatrixBM.MultiplyPoint3x4(vertex), 1f);
        //    }
        //}
    }
}
