using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

//TODO: Use microenvironment as a parameter
public class CellBehavior : MonoBehaviour {

    public CellPhenotype phenotype;
   

    public float totalElapsedTime;


    public Ki67Advanced cellCycle;
    public NecrosisModel cellDeath;

    float o2NecrosisThreshold = 5.0f; 
    float o2NecrosisMax = 2.5f; 


    //float o2HypoxicThreshold = 15.0f; // HIF-1alpha at half-max around 1.5-2%, and tumors often are below 2%
    //float o2HypoxicResponse = 8.0f; // genomic / proteomic changes observed at 7-8 mmHg 
    //float o2HypoxicSaturation = 4.0f; // maximum HIF-1alpha at 0.5% o2 (McKeown)
    

    //float o2ProliferationThreshold = 5.0f; // assume no proliferation at same level as starting necrosis 
    //float o2ProliferationSaturation = 160.0f; // 5% = 38, 21% = 160 mmHg 
    //float o2Reference = 160.0f; // assume all was measured in normoxic 21% o2 

    // necrosis parameters 
    
    //float  maxNecrosisRate = 1.0f / (6.0f * 60.0f); // assume cells survive 6 hours in very low oxygen 



    public int totalElongationStepCnt;

    public List<float> secretionRates;
    public List<float> saturationDensities;
    public List<float> uptakeRates;

    public List<float> cellSourceAndSinkSolverTemp1;
    public List<float> cellSourceAndSinkSolverTemp2;

    bool divisionPhase = false;
    int elongationStepCnt = 0;
    Vector3 randDirection;

    int currentMicroenvironmentVoxelIndex;
    int currentVoxelIndex;

    bool isActive = true;
    bool volumeIsChanged = true;


    void Start() {
        totalElapsedTime = 0;
        totalElongationStepCnt = 100;
        cellCycle = new Ki67Advanced(this.phenotype);

        cellDeath = new NecrosisModel(this.phenotype);

        divisionPhase = false;

        //cellCycle = new DormantCycle();
               
        //direction to put the clone cell in
        randDirection = Utilities.GetRandomVector(0, 1);
        randDirection.Normalize();



        secretionRates.Resize(0, 0f);
        uptakeRates.Resize(0, 0f);
        saturationDensities.Resize(0, 0f);

        RegisterMicroenvironment();

    }

    public void SetId(int id){
        phenotype.id = id;
    }

    public int GetId(){
        return phenotype.id;
    }

    public void SetMotility(bool isMotile) {
        phenotype.cellMechanics.isMotile = isMotile;
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
    ///***
    // * U: vector of uptake(sink) rates
    // * S: vector of secretion(source) rates
    // * T: vector of saturation densities
    // */
    //void SetInternalUptakeConstants(float dt){
    //    Microenvironment microenvironment = GameObject.FindWithTag("Microenvironment").GetComponent<MicroenvironmentBehavior>().microenvironment;
    //    // overall form: dp/dt = S*(T-p) - U*p 
    //    //   p(n+1) - p(n) = dt*S(n)*T(n) - dt*( S(n) + U(n))*p(n+1)
    //    //   p(n+1)*temp2 =  p(n) + temp1
    //    //   p(n+1) = (  p(n) + temp1 )/temp2

    //    //   p(n+1) = ( p(n) + S * T * dt )/( 1+ (S + U) * dt) 

    //    //int nearest_voxel= current_voxel_index;
    //    //FUNDA 
    //    //float  internalConstantToDiscretizeTheDeltaApproximation = dt * phenotype.volume.total / ( (microenvironment.GetVoxel(currentVoxelIndex)).volume ) ; // needs a fix 
    //    float internalConstantToDiscretizeTheDeltaApproximation = 1f;


    //    // temp1 = dt*(V_cell/V_voxel)*S*T 
    //    cellSourceAndSinkSolverTemp1.Assign(secretionRates.Count , 0f );

    //    for (int i = 0; i < cellSourceAndSinkSolverTemp1.Count; i++){
    //        cellSourceAndSinkSolverTemp1[i] += secretionRates[i]; 
    //        cellSourceAndSinkSolverTemp1[i] *= saturationDensities[i]; 
    //        cellSourceAndSinkSolverTemp1[i] *= internalConstantToDiscretizeTheDeltaApproximation; 
    //    }

    //    // temp2 = 1 + dt*(V_cell/V_voxel)*( S + U )
    //    cellSourceAndSinkSolverTemp2.Assign( secretionRates.Count , 1f ); 
    //    for (int i = 0; i < cellSourceAndSinkSolverTemp2.Count; i++) {
    //        cellSourceAndSinkSolverTemp2[i] += internalConstantToDiscretizeTheDeltaApproximation * secretionRates[i];
    //        cellSourceAndSinkSolverTemp2[i] += internalConstantToDiscretizeTheDeltaApproximation * uptakeRates[i];

    //    }

    //    volumeIsChanged = false; 
    //}


    void RegisterMicroenvironment(){
        
        Microenvironment microenvironment = GameObject.FindWithTag("Microenvironment").GetComponent<MicroenvironmentBehavior>().microenvironment;


        //NO oxygen secretion
        // set default uptake and secretion for oncocells
        secretionRates.Resize(microenvironment.densityVectors[0].Count, 0f);
        saturationDensities.Resize(microenvironment.densityVectors[0].Count, 38f);

        //uptakeRates.Resize(microenvironment.densityVectors[0].Count, 0.8f);    
        uptakeRates.Resize(microenvironment.densityVectors[0].Count, 10f); //Value for DCIS    



        //secretionRates.Resize(microenvironment.densityVectors[0].Count, 10f);
        //saturationDensities.Resize(microenvironment.densityVectors[0].Count, 1f);
        //    //uptakeRates.Resize(microenvironment.densityVectors[0].Count, 0f);    
        /// 
        /// Make cells consume oxygen
       
            

        //Some temporary solver variables
        //cellSourceAndSinkSolverTemp1.Resize(microenvironment.densityVectors[0].Count, 0f);
        //cellSourceAndSinkSolverTemp2.Resize(microenvironment.densityVectors[0].Count, 1f);

    }

    /***
    * U: vector of uptake(sink) rates
    * S: vector of secretion(source) rates
    * T: vector of saturation densities
    */
    public void SimulateSecretionAndUptake(float dt) {
        if (!isActive)
            return;

        Microenvironment microenvironment = GameObject.FindWithTag("Microenvironment").GetComponent<MicroenvironmentBehavior>().microenvironment;

        //if(volumeIsChanged){
        //    SetInternalUptakeConstants(dt);
        //    volumeIsChanged = false;            
        //}

        //TODO: is this correct?
        for (int i = 0; i < microenvironment.densityVectors[currentVoxelIndex].Count; i++) {            
            microenvironment.densityVectors[currentVoxelIndex][i] = (microenvironment.densityVectors[currentVoxelIndex][i] + secretionRates[i] * saturationDensities[i] * dt) / (1f + (secretionRates[i] + uptakeRates[i]) * dt);
        }

        //FUNDA
        //for (int i = 0; i < microenvironment.densityVectors[currentVoxelIndex].Count; i++){
        //    microenvironment.densityVectors[currentVoxelIndex][i] += cellSourceAndSinkSolverTemp1[i];    
        //    microenvironment.densityVectors[currentVoxelIndex][i] /= cellSourceAndSinkSolverTemp2[i];        
        //}
    }

   

    public float ComputeOxygenation(){
        float rate = 0f;
        Microenvironment microenvironment = GameObject.FindWithTag("Microenvironment").GetComponent<MicroenvironmentBehavior>().microenvironment;
        float o2 = microenvironment.densityVectors[currentVoxelIndex][Globals.OXYGEN];
        if (o2 < o2NecrosisMax)
            rate = o2NecrosisMax;
        else if (o2 < o2NecrosisThreshold)
            rate = o2NecrosisMax * (o2NecrosisThreshold - o2) / (o2NecrosisThreshold - o2NecrosisMax);

        return rate;
                        
    }

    void FixedUpdate() {
        if (!Globals.animationRunning)
            return;

        float radius;

        SimulateSecretionAndUptake(Time.deltaTime);

        UpdateVoxelIndex();

        if (!divisionPhase) {
            this.phenotype.volume.UpdateVolume(Time.deltaTime );
            //this.phenotype.volume.UpdateVolume(Time.deltaTime * 10f ); //make volume change faster

            radius = this.phenotype.volume.GetRadius();

 
            //scale for 1 is radius*2 --> different from collider
            this.transform.localScale = new Vector3(radius * 2, radius * 2, radius * 2);
  
            this.cellCycle.Advance(Time.deltaTime); 


            //compute oxygenation
            float necrosisRate = this.ComputeOxygenation();
            this.cellDeath.UpdateTransitionRateForCurrentPhase(necrosisRate);
            this.cellDeath.Advance(Time.deltaTime); 


            PhaseT currentPhase = cellCycle.GetCurrentPhase();
            PhaseT deathPhase = cellDeath.GetCurrentPhase();

            this.GetComponent<Renderer>().material.color = currentPhase.color;
            if (deathPhase.name.Equals(Globals.D)) {                
                this.KillCell();
            }

            if(phenotype.isDying){                
                this.GetComponent<Renderer>().material.color = deathPhase.color;
            }

            else if (Globals.cellCnt < Globals.maxCellCnt) {
                if (cellCycle.isDivisionPhase) {
                    if (Globals.animateDivision) {
                        divisionPhase = true;
                        elongationStepCnt = 0;
                    }
                    else { //divide abruptly without animation
                        
                        this.Divide(randDirection);
                    }
                }
            }
        }
        else {
            if (!phenotype.isDying) {
                //division step with animation
                if (elongationStepCnt < totalElongationStepCnt) {
                    radius = this.phenotype.volume.GetRadius();
                    float scaleCoef = ((float)elongationStepCnt / totalElongationStepCnt) * 0.5f + 1f; //elongate in one dimension
                    float scaleCoef2 = Mathf.Sqrt((4f / 7f) / scaleCoef); //shrink other dimensions to keep volume fixed

                    this.transform.localScale = new Vector3(radius * scaleCoef2 * 2, radius * scaleCoef * 2, radius * scaleCoef2 * 2); //scale in y direction and then rotate
                    transform.rotation = Quaternion.LookRotation(randDirection);

                    elongationStepCnt++;
                }
                else {
                    this.Divide(Vector3.up);
                    divisionPhase = false;
                }
            }
        }


        //to be able to retrieve position when loaded from file
        this.phenotype.position = this.transform.position;
        this.phenotype.scale = this.transform.localScale;
        this.phenotype.rotation = this.transform.rotation;
        this.phenotype.color = this.GetComponent<Renderer>().material.color;

        totalElapsedTime += Time.deltaTime;
    }


    //Make a clone of this sphere and reduce its volume
    //new cell will be to the randDirection
    void Divide(Vector3 direction) {

        //create clone with the same volume and radius
        GameObject clone = (GameObject)Instantiate(this.gameObject, transform.position, transform.rotation);
        clone.GetComponent<CellBehavior>().SetId(++Globals.cellCnt);
        clone.name = "Cell" + clone.GetComponent<CellBehavior>().GetId();

        this.phenotype.volume.Divide(0.5f);
        clone.GetComponent<CellBehavior>().phenotype.volume.Divide(0.5f);

        float radius = this.phenotype.volume.GetRadius();

        Vector3 deltaPos = direction * radius;

        clone.transform.Translate(deltaPos.x, deltaPos.y, deltaPos.z);
        transform.Translate(-deltaPos.x, -deltaPos.y, -deltaPos.z);


        this.GetComponent<CellBehavior>().cellCycle.isDivisionPhase = false;
        clone.GetComponent<CellBehavior>().cellCycle.isDivisionPhase = false;

        //Reset the current phases of this cell and its clone
        //clone.GetComponent<CellBehavior>().cellCycle.MoveToNextPhase();
        //this.GetComponent<CellBehavior>().cellCycle.MoveToNextPhase();
        //clone.GetComponent<CellBehavior>().cellCycle.currentPhaseInd = 3;
        //this.GetComponent<CellBehavior>().cellCycle.currentPhaseInd = 3;
        //Debug.Log(clone.GetComponent<CellBehavior>().cellCycle.currentPhaseInd);
    }
       
    void KillCell() {
        Destroy(GetComponent<Collider>());
        Destroy(this.gameObject);
    }


    //private void OnDrawGizmos() {


    //    Gizmos.color = Color.magenta;
    //    Gizmos.DrawLine(transform.position, transform.position + randDirection* 5);


    //}

}
