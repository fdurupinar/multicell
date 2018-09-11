using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

//TODO: Use microenvironment as a parameter
public class CellBehavior : MonoBehaviour {

    public CellPhenotype phenotype;
   

    public float totalElapsedTime;
    public int id = 1;

    public TestDivisionCycle cellCycle;

    public int totalElongationStepCnt;

    public List<float> secretionRates;
    public List<float> saturationDensities;
    public List<float> uptakeRates;

    public List<float> cellSourceAndSinkSolverTemp1;
    public List<float> cellSourceAndSinkSolverTemp2;

    //public Microenvironment microenvironment;



    bool divisionPhase = false;
    int elongationStepCnt = 0;
    Vector3 randDirection;

    int currentMicroenvironmentVoxelIndex;
    int currentVoxelIndex;

    bool isActive;
    bool volumeIsChanged;


    void Start() {
        totalElapsedTime = 0;
        totalElongationStepCnt = 100; 
        cellCycle = new TestDivisionCycle();
               
        //direction to put the clone cell in
        randDirection = Utilities.GetRandomVector(0, 1);
        randDirection.Normalize();


        secretionRates.Resize(0, 0f);
        uptakeRates.Resize(0, 0f);
        saturationDensities.Resize(0, 0f);


        Microenvironment microenvironment = GameObject.FindWithTag("Microenvironment").GetComponent<MicroenvironmentBehavior>().microenvironment;

        RegisterMicroenvironment(microenvironment);

    }

    void UpdateVoxelIndex() {
        Microenvironment microenvironment = GameObject.FindWithTag("Microenvironment").GetComponent<MicroenvironmentBehavior>().microenvironment;

        if(!microenvironment.mesh.IsPositionValid(transform.position)){
            currentVoxelIndex = -1;
            isActive = false;

        }
        else{
            currentVoxelIndex = microenvironment.GetNearestVoxelIndex(transform.position);            
        }
    }
    /***
     * U: vector of uptake(sink) rates
     * S: vector of secretion(source) rates
     * T: vector of saturation densities
     */
    void SetInternalUptakeConstants(float dt){
        Microenvironment microenvironment = GameObject.FindWithTag("Microenvironment").GetComponent<MicroenvironmentBehavior>().microenvironment;
        // overall form: dp/dt = S*(T-p) - U*p 
        //   p(n+1) - p(n) = dt*S(n)*T(n) - dt*( S(n) + U(n))*p(n+1)
        //   p(n+1)*temp2 =  p(n) + temp1
        //   p(n+1) = (  p(n) + temp1 )/temp2
        //int nearest_voxel= current_voxel_index;
        float  internalConstantToDiscretizeTheDeltaApproximation = dt * phenotype.volume.total / ( (microenvironment.GetVoxel(currentVoxelIndex)).volume ) ; // needs a fix 

        // temp1 = dt*(V_cell/V_voxel)*S*T 
        cellSourceAndSinkSolverTemp1.Assign(secretionRates.Count , 0f );

        for (int i = 0; i < cellSourceAndSinkSolverTemp1.Count; i++){
            cellSourceAndSinkSolverTemp1[i] += secretionRates[i]; 
            cellSourceAndSinkSolverTemp1[i] *= saturationDensities[i]; 
            cellSourceAndSinkSolverTemp1[i] *= internalConstantToDiscretizeTheDeltaApproximation; 
        }

        // temp2 = 1 + dt*(V_cell/V_voxel)*( S + U )
        cellSourceAndSinkSolverTemp2.Assign( secretionRates.Count , 1f ); 
        for (int i = 0; i < cellSourceAndSinkSolverTemp2.Count; i++) {
            cellSourceAndSinkSolverTemp2[i] += internalConstantToDiscretizeTheDeltaApproximation * secretionRates[i];
            cellSourceAndSinkSolverTemp1[i] += internalConstantToDiscretizeTheDeltaApproximation * uptakeRates[i];

        }

        volumeIsChanged = false; 
    }

    void RegisterMicroenvironment(Microenvironment microenvironmentIn){
        Microenvironment microenvironment = GameObject.FindWithTag("Microenvironment").GetComponent<MicroenvironmentBehavior>().microenvironment;

        microenvironment = microenvironmentIn;

        //FUNDA
        //secretionRates.Resize(microenvironment.densityVectors[0].Count, 0f); 
        secretionRates.Resize(microenvironment.densityVectors[0].Count, 10f);

        saturationDensities.Resize(microenvironment.densityVectors[0].Count, 0f);
        uptakeRates.Resize(microenvironment.densityVectors[0].Count, 0f);

        //Some temporary solver variables
        cellSourceAndSinkSolverTemp1.Resize(microenvironment.densityVectors[0].Count, 0f);
        cellSourceAndSinkSolverTemp2.Resize(microenvironment.densityVectors[0].Count, 1f);

    }

    List<float> GetNearestDensityVector(){
        Microenvironment microenvironment = GameObject.FindWithTag("Microenvironment").GetComponent<MicroenvironmentBehavior>().microenvironment;
        return microenvironment.GetNearestDensityVector(currentVoxelIndex);
    }

    // directly access the gradient of substrate n nearest to the cell 
    List<float> GetNearestGradient(int substrateIndex) {
        Microenvironment microenvironment = GameObject.FindWithTag("Microenvironment").GetComponent<MicroenvironmentBehavior>().microenvironment;
        return microenvironment.GetGradientVector(currentVoxelIndex)[substrateIndex]; 
    }

    // directly access a vector of gradients, one gradient per substrate 
    List<List<float>> GetNearestGradientVector() {
        Microenvironment microenvironment = GameObject.FindWithTag("Microenvironment").GetComponent<MicroenvironmentBehavior>().microenvironment;
        return microenvironment.GetGradientVector(currentVoxelIndex);
    }

    public void SimulateSecretionAndUptake(float dt){
        if (!isActive)
            return;

        Microenvironment microenvironment = GameObject.FindWithTag("Microenvironment").GetComponent<MicroenvironmentBehavior>().microenvironment;

        if(volumeIsChanged){
            SetInternalUptakeConstants(dt);
            volumeIsChanged = false;            
        }

        //TODO: is this correct?
        //TODO: could be parallelized
        for (int i = 0; i < microenvironment.densityVectors[currentVoxelIndex].Count; i++){
            microenvironment.densityVectors[currentVoxelIndex][i] += cellSourceAndSinkSolverTemp1[i];    
            microenvironment.densityVectors[currentVoxelIndex][i] /= cellSourceAndSinkSolverTemp2[i];    
        }
    }

    void FixedUpdate() {
        if (!Globals.animationRunning)
            return;

        float radius;

        SimulateSecretionAndUptake(Time.deltaTime);

        UpdateVoxelIndex();

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

                this.transform.localScale = new Vector3(radius * scaleCoef2*2,radius * scaleCoef*2,radius* scaleCoef2*2); //scale in y direction and then rotate
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


    //private void OnDrawGizmos() {


    //    Gizmos.color = Color.magenta;
    //    Gizmos.DrawLine(transform.position, transform.position + randDirection* 5);


    //}

}
