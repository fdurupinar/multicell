using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CellBehavior : MonoBehaviour {

    public CellPhenotype phenotype;
   

    public float totalElapsedTime;
    public int id = 1;

    public ApoptosisModel cellCycle;


    // Use this for initialization
    void Start() {
        totalElapsedTime = 0;
        cellCycle = new ApoptosisModel(phenotype);
    }


    void FixedUpdate() {


        //Volume and scale updates
        this.phenotype.volume.Update(Time.deltaTime);

        float radius = this.phenotype.volume.GetRadius();

        this.transform.localScale = new Vector3(radius, radius, radius);



        ////cell cycle updates
        this.cellCycle.Advance(Time.deltaTime);


        //if (cellCycle.GetCurrentPhase().name == Globals.D) {
        //    this.KillCell();
        //}

        if (cellCycle.GetCurrentPhase().name == Globals.M) {
            this.Divide();
        }

        totalElapsedTime += Time.deltaTime;

    }


    //Make a clone of this sphere and reduce its volume
    void Divide() {

        //create clone with the same volume and radius
        GameObject clone = (GameObject)Instantiate(this.gameObject, transform.position, transform.rotation);
        clone.GetComponent<CellBehavior>().id = ++Globals.cellCnt;
        clone.name = "Cell" + clone.GetComponent<CellBehavior>().id;

        this.phenotype.volume.Divide(0.5f);
        clone.GetComponent<CellBehavior>().phenotype.volume.Divide(0.5f);

        float radius = this.phenotype.volume.GetRadius();

        Vector3 randVec = Utilities.GetRandomVector(0, 1);
        randVec.Normalize();

        Vector3 deltaPos = randVec * radius;

        clone.transform.Translate(deltaPos.x, deltaPos.y, deltaPos.z);

    }
       
    void KillCell() {
        Destroy(this.gameObject);
    }
}
