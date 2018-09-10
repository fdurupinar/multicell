using System.Collections;
using System;
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
    public Vector3 migrationBiasDirection = Vector3.zero;
    public float migrationBias = 0f;
    public Vector3 motilityVector = Vector3.zero;


    private ArrayList collidingCells = new ArrayList();

    private Vector3 cellBMContactVertex;
    Matrix4x4 transformMatrixBM;



    // Use this for initialization
    void Start() {

        rb = GetComponent<Rigidbody>();
        basementMembrane = GameObject.Find("BM");

        phenotype = GetComponent<CellBehavior>().phenotype;

        transformMatrixBM = basementMembrane.transform.localToWorldMatrix;
    
        //Represent bm as a cloth

        ////Capsule colliders is a property 
        //int colliderCnt = basementMembrane.GetComponent<Cloth>().capsuleColliders.Length;
        //var tmp = new CapsuleCollider[colliderCnt + 1];
        //tmp[colliderCnt] = this.GetComponent<CapsuleCollider>();
        //for (int i = 0; i < colliderCnt; i++)
        //    tmp[i] = basementMembrane.GetComponent<Cloth>().capsuleColliders[i];

        //basementMembrane.GetComponent<Cloth>().capsuleColliders = tmp; //Unity API passes arrays by value


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


        //motility
        if(isMotile){
            float val = Utilities.GetRandomNumber(0, 1);
            if(val < Time.deltaTime/ persistenceTime || persistenceTime < Time.deltaTime){
                float tempAngle = 2* Mathf.PI * Utilities.GetRandomNumber(0, 1);
                float tempPhi = Mathf.PI * Utilities.GetRandomNumber(0, 1);

                Vector3 randVec = new Vector3();
                randVec.x = Mathf.Cos(tempAngle) * Mathf.Sin(tempPhi);;
                randVec.y = Mathf.Sin(tempAngle) * Mathf.Sin(tempPhi);;
                randVec.z = Mathf.Cos(tempPhi);

                //migration bias will be updated in another component

                motilityVector = (1 - migrationBias) * randVec + migrationBiasDirection * migrationBias;
                motilityVector = Vector3.Normalize(motilityVector);
                motilityVector *= migrationSpeed;

                rb.velocity += motilityVector;
            }
        }


        //Skip BM deformation for now
        ////Add force on basement membrane
        //Vector3 curInverse = inverseTransformMatrixBM.MultiplyPoint3x4(this.transform.position);
        //Vector3 forcePoint =  2f * cellBMContactVertex - curInverse;                                
        //basementMembrane.GetComponent<MeshDeformer>().AddDeformingForce(-forcePoint, Vector3.Magnitude(fAdhesionBM) * 1000f);

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


    /***
     * Returns the closest vertex position on the mesh without transformation to global coordinates
     */
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
