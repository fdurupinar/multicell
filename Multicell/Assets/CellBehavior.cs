using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CellBehavior : MonoBehaviour {

    public CellPhenotype phenotype;
   

    public float totalElapsedTime;
    public int id = 1;

    public TestDivisionCycle cellCycle;




    bool divisionPhase = false;
    int elongationStepCnt = 0;
    Vector3 randDirection;
    public int totalElongationStepCnt; 




    // Use this for initialization
    void Start() {
        totalElapsedTime = 0;
        totalElongationStepCnt = 100; 
        cellCycle = new TestDivisionCycle();

       
        //direction to put the clone cell in
        randDirection = Utilities.GetRandomVector(0, 1);
        randDirection.Normalize();
    }


    void FixedUpdate() {


        float radius;

        if (!divisionPhase) {
            this.phenotype.volume.UpdateVolume(Time.deltaTime);

            radius = this.phenotype.volume.GetRadius();

            //scale for 1 is radius*2 --> different from collider
            this.transform.localScale = new Vector3(radius*2, radius*2, radius*2);


            this.cellCycle.Advance(Time.deltaTime);

            PhaseT currentPhase = cellCycle.GetCurrentPhase();
            this.GetComponent<Renderer>().material.color = currentPhase.color;

            if (currentPhase.name.Equals(Globals.D)) {
                this.KillCell();
            }

            if (Globals.cellCnt < Globals.maxCellCnt) {
                if (currentPhase.name.Equals(Globals.M)) {
                    if (Globals.animateDivision) {
                        divisionPhase = true;
                        elongationStepCnt = 0;
                    }
                    else //divide abruptly without animation
                        this.Divide(randDirection);
                }
            }
        }
        else{
            //division step with animation
            if (elongationStepCnt < totalElongationStepCnt) {
                radius = this.phenotype.volume.GetRadius();
                float scaleCoef = ((float)elongationStepCnt/totalElongationStepCnt)* 0.5f + 1f; //elongate in one dimension
                float scaleCoef2 = Mathf.Sqrt((4f/7f)/ scaleCoef); //shrink other dimensions to keep volume fixed

                this.transform.localScale = new Vector3(radius * scaleCoef2,radius * scaleCoef,radius* scaleCoef2); //scale in y direction and then rotate
                transform.rotation = Quaternion.LookRotation(randDirection);

                elongationStepCnt++;
            }
            else {
                this.Divide(Vector3.up);
                divisionPhase = false;
            }               
        }

        totalElapsedTime += Time.deltaTime;
    }


    //Make a clone of this sphere and reduce its volume
    //new cell will be to the randDirection
    void Divide(Vector3 direction) {

        //create clone with the same volume and radius
        GameObject clone = (GameObject)Instantiate(this.gameObject, transform.position, transform.rotation);
        clone.GetComponent<CellBehavior>().id = ++Globals.cellCnt;
        clone.name = "Cell" + clone.GetComponent<CellBehavior>().id;

        this.phenotype.volume.Divide(0.5f);
        clone.GetComponent<CellBehavior>().phenotype.volume.Divide(0.5f);

        float radius = this.phenotype.volume.GetRadius();

        Vector3 deltaPos = direction * radius;

        clone.transform.Translate(deltaPos.x, deltaPos.y, deltaPos.z);
        transform.Translate(-deltaPos.x, -deltaPos.y, -deltaPos.z);

    }
       
    void KillCell() {
        Destroy(this.gameObject);
    }


    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(transform.position, transform.position + randDirection* 5);


    }

}
