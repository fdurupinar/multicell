using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks; 
using System.Linq;

//[System.Serializable]
public class MicroenvironmentOptions{
    public Microenvironment microenvironment;
    public string name;
    public string timeUnits;
    public string spatialUnits;
    public float dX, dY, dZ;

    public bool outerDirichletConditions;
    public List<float> dirichletConditionVector ;
    public List<bool> dirichletActivationVector;

    public List<float> xRange;
    public List<float> yRange;
    public List<float> zRange;

    public bool calculateGradients;
    public bool useOxygenAsFirstField;


    public MicroenvironmentOptions(){
        dirichletConditionVector = new List<float>();
        dirichletActivationVector = new List<bool>();

        xRange = new List<float>();
        yRange = new List<float>();
        zRange = new List<float>();


        useOxygenAsFirstField = true;
        name = "microenvironment";
        timeUnits = "min";
        spatialUnits = "micron";
        //funda
        //dX = 20;
        //dY = 20;
        //dZ = 20;

        dX = 20;
        dY = 20;
        dZ = 20;
        outerDirichletConditions = false;

        dirichletConditionVector.Add(38f); // 5% o2


        //xRange.Add(50f);
        //xRange.Add(50f);
        //xRange[0] *= -1f;

        //yRange.Add(50f);
        //yRange.Add(50f);
        //yRange[0] *= -1f;

        //zRange.Add(50f);
        //zRange.Add(50f);
        //zRange[0] *= -1f;


        xRange.Add(100f);
        xRange.Add(100f);
        xRange[0] *= -1f;

        yRange.Add(100f);
        yRange.Add(100f);
        yRange[0] *= -1f;

        zRange.Add(100f);
        zRange.Add(100f);
        zRange[0] *= -1f;

        //TODO: Funda

        //xRange.Add(500f);
        //xRange.Add(500f);
        //xRange[0] *= -1f;

        //yRange.Add(500f);
        //yRange.Add(500f);
        //yRange[0] *= -1f;

        //zRange.Add(500f);
        //zRange.Add(500f);
        //zRange[0] *= -1f;

        //calculateGradients = false;
        calculateGradients = true;
    }
}

//[System.Serializable]
public class Microenvironment {

    public string name;
    public string timeUnits;
    public string spatialUnits;


    public CartesianMesh mesh;

    public List <float> zero;
    public List<float> one;

    public List<List<float>> dirichletValueVectors;
    public List<float> dirichletConditionVector;
    public List<bool> dirichletActivationVector;

    public List<List<float>> densityVectors;
    //public List<List<float>> temporaryDensityVectors1;
    //public List<List<float>> temporaryDensityVectors2;


    public List<List<float>> bulkSourceAndSinkSolverTemp1;
    public List<List<float>> bulkSourceAndSinkSolverTemp2;
    public List<List<float>> bulkSourceAndSinkSolverTemp3;
    public List<List<float>> bulkSourceAndSinkSolverTemp4;

    public List<List<float>> supplyTargetDensitiesTimesSupplyRates;
    public List<List<float>> supplyRates;

    public List<List<float>> uptakeRates;
    public List<List<float>> supplyTargetDensities;


    public List<List<float>> thomasTemp1;
    public List<List<float>> thomasTemp2;

    public List<Vector3> thomasConstant1Vec;
    public List<Vector3> thomasNegConstant1Vec;

    public List<float> thomasConstant1;
    public List<float> thomasConstant1a;
    public List<float> thomasConstant2;
    public List<float> thomasConstant3;
    public List<float> thomasConstant3a;

    public List<List<float>> thomasDenomX;
    public List<List<float>> thomasDenomY;
    public List<List<float>> thomasDenomZ;
    public List<List<float>> thomasCX;
    public List<List<float>> thomasCY;
    public List<List<float>> thomasCZ;


    public bool thomasSetupDone;
    public int thomasIJump;
    public int thomasJJump;
    public int thomasKJump;


    bool bulkSourceAndSinkSolverSetupDone;



    public List<List<List<float>>> gradientVectors;
    public List<bool> gradientVectorComputed;

    public List<float> diffusionCoefficients;
    public List<float> decayRates;

    public bool diffusionSolverSetupDone;

    public List<string> densityNames;
    public List<string> densityUnits;



    //assigned outside
    public  MicroenvironmentOptions microenvironmentOptions;


    public Microenvironment(){


        mesh = new CartesianMesh(); 
        mesh.Resize(1, 1, 1);

        zero = new List<float>();
        one = new List<float>();
        zero.Resize(1, 0f);
        one.Resize(1, 1f);

        dirichletValueVectors = new List<List<float>>();
        dirichletConditionVector = new List<float>();
        dirichletActivationVector = new List<bool>();

        densityVectors = new List<List<float>>();

        densityVectors.Resize(GetNumberOfVoxels(), zero);

        bulkSourceAndSinkSolverTemp1 = new List<List<float>>();
        bulkSourceAndSinkSolverTemp2 = new List<List<float>>();
        bulkSourceAndSinkSolverTemp3 = new List<List<float>>();
        bulkSourceAndSinkSolverTemp4 = new List<List<float>>();

        supplyTargetDensitiesTimesSupplyRates = new List<List<float>>();
        supplyRates = new List<List<float>>();
        supplyTargetDensities = new List<List<float>>();
        uptakeRates = new List<List<float>>();

        thomasTemp1 = new List<List<float>>();
        thomasTemp2 = new List<List<float>>();

        thomasConstant1Vec = new List<Vector3>();
        thomasNegConstant1Vec = new List<Vector3>();

        thomasConstant1 = new List<float>();
        thomasConstant1a = new List<float>();
        thomasConstant2 = new List<float>();
        thomasConstant3 = new List<float>();
        thomasConstant3a = new List<float>();

        thomasDenomX = new List<List<float>>();
        thomasDenomY = new List<List<float>>();
        thomasDenomZ = new List<List<float>>();

        thomasCX = new List<List<float>>();
        thomasCY = new List<List<float>>();
        thomasCZ = new List<List<float>>();

        gradientVectors = new List<List<List<float>>>();

        gradientVectorComputed = new List<bool>();

        diffusionCoefficients = new List<float>();

        decayRates = new List<float>();

        densityNames = new List<string>();
        densityUnits = new List<string>();

        name = "unnamed";
        spatialUnits = "none";
        timeUnits = "none";

        bulkSourceAndSinkSolverSetupDone = false;
        thomasSetupDone = false;
        diffusionSolverSetupDone = false;

        gradientVectors.Resize(GetNumberOfVoxels());
        for (int i = 0; i < GetNumberOfVoxels(); i++) {
            gradientVectors[i].Resize(1);
            gradientVectors[i][0].Resize(3, 0f);
        }

        gradientVectorComputed.Resize(GetNumberOfVoxels(), false);
                              
        densityNames.Add("unnamed");
        densityUnits.Add("none");



        diffusionCoefficients.Assign(GetNumberOfDensities(), 0f);
        decayRates.Assign(GetNumberOfDensities(), 0f);

        dirichletValueVectors.Assign(GetNumberOfVoxels(), one);
        dirichletActivationVector.Assign(1, true);


    }

    //Calls the other one
    public Microenvironment(string name):this(){

        this.name = name;

    }

    public void AddDirichletNode(int voxelIndex, List<float> value) {
        
        mesh.voxels[voxelIndex].isDirichlet = true;

        //TODO: check this pass by ref?
        //dirichletValueVectors[voxelIndex] = value;
        for (int i = 0; i < dirichletValueVectors[voxelIndex].Count; i++)
            dirichletValueVectors[voxelIndex][i] = value[i];


    }

    public void RemoveDirichletNode(int voxelIndex) {
        mesh.voxels[voxelIndex].isDirichlet = false;
    }

    public bool IsDirichletNode(int voxelIndex) {
        return mesh.voxels[voxelIndex].isDirichlet;
    }


    public void SetSubstrateDirichletActivation(int substrateIndex, bool newValue) {
        dirichletActivationVector[substrateIndex] = newValue;
    }


    public void ApplyDirichletConditions() {
        //TODO openmp Uses parallelization here
        //for (int i = 0; i < mesh.voxels.Count; i++) {
        //    if (mesh.voxels[i].isDirichlet) {
        //        for (int j = 0; j < dirichletValueVectors[i].Count; j++) {
        //            if (dirichletActivationVector[j])
        //                densityVectors[i][j] = dirichletValueVectors[i][j];   
        //        }
        //    }
        //}


        //TODO
        Parallel.For(0, mesh.voxels.Count, i => {
            if (mesh.voxels[i].isDirichlet) {                
                
                for (int j = 0; j < dirichletValueVectors[i].Count; j++) {
                    if (dirichletActivationVector[j])
                        densityVectors[i][j] = dirichletValueVectors[i][j];
                }
            }
        });
    }

    void ResizeVoxels(int newNumberOfVoxels){
    
        mesh.voxels.Resize(newNumberOfVoxels);
        densityVectors.Resize(GetNumberOfVoxels(), zero);

        gradientVectors.Resize(GetNumberOfVoxels());

        for (int k = 0; k < GetNumberOfVoxels(); k++) {
            gradientVectors[k].Resize(GetNumberOfDensities());
            for (int i = 0; i < GetNumberOfDensities(); i++) {
                gradientVectors[k][i].Resize(3, 0f);
            }
        }
        gradientVectorComputed.Resize(GetNumberOfVoxels(), false);
        dirichletValueVectors.Assign(GetNumberOfVoxels(), one);
    }

    void ResizeSpaceRest() {

        densityVectors.Assign(GetNumberOfVoxels(), zero);
        gradientVectors.Resize(GetNumberOfVoxels());

        for (int k = 0; k < GetNumberOfVoxels(); k++) {
            
            gradientVectors[k].Resize(GetNumberOfDensities());
            for (int i = 0; i < GetNumberOfDensities(); i++) {
                gradientVectors[k][i].Resize(3, 0f);
            }
        }

        gradientVectorComputed.Resize(GetNumberOfVoxels(), false);
        dirichletValueVectors.Assign(GetNumberOfVoxels(), one);
    }

    public void ResizeSpace(int xNodes, int yNodes, int zNodes) {
        mesh.Resize(xNodes, yNodes, zNodes);
        ResizeSpaceRest();

    }

    public void ResizeSpace(float xStart, float xEnd, float yStart, float yEnd, float zStart, float zEnd, int xNodes, int yNodes, int zNodes) {
        mesh.Resize(xStart, xEnd, yStart, yEnd, zStart, zEnd, xNodes, yNodes, zNodes);
        ResizeSpaceRest();
    }


    public void ResizeSpace(float xStart, float xEnd, float yStart, float yEnd, float zStart, float zEnd, float dXNew, float dYNew, float dZNew) {
        mesh.Resize(xStart, xEnd, yStart, yEnd, zStart, zEnd, dXNew, dYNew, dZNew);
    }


    public void ResizeSpaceUniform(float xStart, float xEnd, float yStart, float yEnd, float zStart, float zEnd, float dXNew) {
        ResizeSpace(xStart, xEnd, yStart, yEnd, zStart, zEnd, dXNew, dXNew, dXNew);
    }

    public void ResizeDensities(int newSize) {

        zero.Assign(newSize, 0f);
        one.Assign(newSize, 1f);

        densityVectors.Assign(GetNumberOfVoxels(), zero);


        //TODO: this was missing in the original
        gradientVectors.Resize(GetNumberOfVoxels());

        for (int k = 0; k < GetNumberOfVoxels(); k++) {

            gradientVectors[k].Resize(GetNumberOfDensities());

            for (int i = 0; i < GetNumberOfDensities(); i++) {
                gradientVectors[k][i].Resize(3, 0.0f);
            }

        }

        gradientVectorComputed.Resize(GetNumberOfVoxels(), false);

        //FUNDA????
//        diffusionCoefficients.Assign(newSize, 0f);
 //       decayRates.Assign(newSize, 0f);


        densityNames.Assign(newSize, "unnamed");
        densityUnits.Assign(newSize, "none");

        dirichletValueVectors.Assign(GetNumberOfVoxels(), one);
        dirichletActivationVector.Assign(newSize, true);

        //FUNDA closed this?
        //microenvironmentOptions.dirichletConditionVector.Assign(newSize, 1f);
        //microenvironmentOptions.dirichletActivationVector.Assign(newSize, true);

    }


    void AddDensityRest() {

        //update 1, 0
        zero.Add(0f);
        one.Add(1f);

        for(int i = 0; i < densityVectors.Count; i++) {
            densityVectors[i].Add(0f);  
        }


        for (int k = 0; k < mesh.voxels.Count; k++) {
            gradientVectors[k].Resize(GetNumberOfDensities());
            for (int i = 0; i < GetNumberOfDensities(); i++) {
                gradientVectors[k][i].Resize(3, 0f);
            }
        }

        gradientVectorComputed.Resize(GetNumberOfVoxels(), false);

        dirichletValueVectors.Assign(GetNumberOfVoxels(), one);
        dirichletActivationVector.Assign(GetNumberOfDensities(), true);

        microenvironmentOptions.dirichletConditionVector.Add(1f);
        microenvironmentOptions.dirichletActivationVector.Add(true);

    }

    public void AddDensity(){

        densityNames.Add("unnamed");
        densityUnits.Add("none");

        diffusionCoefficients.Add(0);
        decayRates.Add(0);

        AddDensityRest();

    }


    public void AddDensity(string dName, string units) {
        densityNames.Add(dName);
        densityUnits.Add(units);

        diffusionCoefficients.Add(0);
        decayRates.Add(0);

        AddDensityRest();
    }

    public void AddDensity(string dName, string units, float diffusionConstant, float decayRate) {
        densityNames.Add(dName);
        densityUnits.Add(units);

        diffusionCoefficients.Add(diffusionConstant);
        decayRates.Add(decayRate);

        AddDensityRest();
    }

    public int FindDensityIndex(string dName){

        int val = -1;
        for (int i = 0; i < densityNames.Count; i++)
            if (densityNames[i].Equals(dName))
                val = i;

        return val;
    }



    public void SetDensity(int index, string dName, string units){
        if (index == 0)
            microenvironmentOptions.useOxygenAsFirstField = false;

        densityNames[index] = dName;
        densityUnits[index] = units;
    }

    public void SetDensity(int index, string dName, string units, float diffusionConstant, float decayRate) {
        SetDensity(index, dName, units);
        diffusionCoefficients[index] = diffusionConstant;
        decayRates[index] = decayRate;
    }

    public int GetNumberOfDensities(){
        
        return densityVectors[0].Count;

    }

    public int GetVoxelIndex(int i, int j, int k){
        return mesh.GetVoxelIndex(i, j, k);
    }

    public int[] GetCartesianIndices(int n) {
        return mesh.GetCartesianIndices(n);
    }

    public int GetNearestVoxelIndex(Vector3 pos){
        return mesh.GetNearestVoxelIndex(pos);
    }

    public Voxel GetVoxel(int voxelIndex){
        return mesh.voxels[voxelIndex];
    }

    public int [] GetNearestCartesianIndices(Vector3 pos){
        return mesh.GetNearestCartesianIndices(pos);
    }

    public Voxel GetNearestVoxel(Vector3 pos){
        return mesh.GetNearestVoxel(pos);
    }

    public List<float> GetDensityVector(int i, int j, int k){
        return densityVectors[GetVoxelIndex(i, j, k)];        
    }
	

    public List<float> GetDensityVector(int i, int j) {
        return densityVectors[GetVoxelIndex(i, j, 0)];
    }

    public List<float> GetDensityVector(int n) {
        return densityVectors[n];
    }

    public List<float> GetNearestDensityVector(int voxelIndex) {
        return densityVectors[voxelIndex];
    }
    public List<float> GetNearestDensityVector(Vector3 pos) {
        return densityVectors[mesh.GetNearestVoxelIndex(pos)];
    }


    public int GetNumberOfVoxels(){

        return mesh.voxels.Count;
    }

    //TODO: will be filled in
    public void SimulateDiffusionDecay(float deltaTime){
        Solvers.DiffusionDecaySolverConstantCoefficientsLOD3D(this, deltaTime);
    }

    //TODO: ?????
    public void SimulateBulkSourceAndSink(float deltaTime) {

        //if (!bulkSourceAndSinkSolverSetupDone) {                                      
        //    bulkSourceAndSinkSolverTemp1.Resize(GetNumberOfVoxels(), zero);
        //    bulkSourceAndSinkSolverTemp2.Resize(GetNumberOfVoxels(), zero);
        //    bulkSourceAndSinkSolverTemp3.Resize(GetNumberOfVoxels(), zero);
        //    bulkSourceAndSinkSolverSetupDone = true;
        //}

        //TODO openmp

        //TODO: not a match with the original function

        //for (int i = 0; i < GetNumberOfVoxels(); i++) {
        //Parallel.For(0, GetNumberOfVoxels(), i => {


        //    for (int j = 0; j < supplyRates[i].Count; j++)   //temp1 = S
        //        bulkSourceAndSinkSolverTemp1[i][j] = supplyRates[i][j];

        //    for (int j = 0; j < supplyTargetDensities[i].Count; j++) //temp2 = T
        //        bulkSourceAndSinkSolverTemp2[i][j] = supplyTargetDensities[i][j];

        //    for (int j = 0; j < uptakeRates[i].Count; j++)  //temp3 = U
        //        bulkSourceAndSinkSolverTemp3[i][j] = uptakeRates[i][j];
        //});


        //p = (p + S * T * dt) / (1 +(U +S) *dt)
        Parallel.For(0, GetNumberOfVoxels(), i => {
        ////for (int i = 0; i < GetNumberOfVoxels(); i++) {
            //why not?
            for(int j = 0; j < densityVectors[i].Count; j++){                
                densityVectors[i][j] =(densityVectors[i][j] + supplyRates[i][j] * supplyTargetDensities[i][j] * deltaTime) / (1f + (supplyRates[i][j] + uptakeRates[i][j]) * deltaTime);        

            }



            //for (int j = 0; j < bulkSourceAndSinkSolverTemp1[i].Count; j++) // temp2 = S*T
            //    bulkSourceAndSinkSolverTemp2[i][j] *= bulkSourceAndSinkSolverTemp1[i][j];

            //for (int j = 0; j < bulkSourceAndSinkSolverTemp2[i].Count; j++) // out = out + dt*temp2 = out + dt*S*T
            //    densityVectors[i][j] += deltaTime * bulkSourceAndSinkSolverTemp2[i][j];

            //for (int j = 0; j < bulkSourceAndSinkSolverTemp3[i].Count; j++) {
            //    bulkSourceAndSinkSolverTemp3[i][j] += bulkSourceAndSinkSolverTemp1[i][j];// temp3 = U+S
            //    bulkSourceAndSinkSolverTemp3[i][j] *= deltaTime; // temp3 = dt*(U+S)
            //    bulkSourceAndSinkSolverTemp3[i][j] += 1f;  //temp3 = 1 + dt * (U + S)

            //    //Debug.Log(bulkSourceAndSinkSolverTemp3[i][j] + " " + bulkSourceAndSinkSolverTemp1[i][j] + " " + bulkSourceAndSinkSolverTemp2[i][j]);
            //}

            //for (int j = 0; j < densityVectors[i].Count; j++)
                //densityVectors[i][j] /= bulkSourceAndSinkSolverTemp3[i][j];
        });
         
    }


    //public void UpdateBulkRates(List<float> supplyRatesVal, List<float> supplyTargetDensitiesTimesSupplyRatesVal, List<float> uptakeRatesVal) {
    //    if (supplyTargetDensitiesTimesSupplyRates.Count != mesh.voxels.Count) {
    //        supplyTargetDensitiesTimesSupplyRates.Assign(GetNumberOfVoxels(), zero);
    //    }

    //    if (supplyRates.Count != mesh.voxels.Count) {
    //        supplyRates.Assign(GetNumberOfVoxels(), zero);
    //    }

    //    if (uptakeRates.Count != mesh.voxels.Count) {
    //        uptakeRates.Assign(GetNumberOfVoxels(), zero);
    //    }

    //    //TODO: openmp parallel
    //    //for (int i = 0; i < GetNumberOfVoxels(); i++){
    //    //    uptakeRates[i] = new List<float>(uptakeRatesVal);
    //    //    supplyRates[i] = new List<float>(supplyRatesVal);
    //    //    supplyTargetDensitiesTimesSupplyRates[i] = new List<float>(supplyTargetDensitiesTimesSupplyRatesVal);

    //    //    for (int j = 0; j < supplyTargetDensitiesTimesSupplyRates[i].Count; j++)
    //    //        supplyTargetDensitiesTimesSupplyRates[i][j] *= supplyRates[i][j];
    //    //}


    //    Parallel.For(0, GetNumberOfVoxels(), i => {
    //        uptakeRates[i] = new List<float>(uptakeRatesVal);
    //        supplyRates[i] = new List<float>(supplyRatesVal);
    //        supplyTargetDensitiesTimesSupplyRates[i] = new List<float>(supplyTargetDensitiesTimesSupplyRatesVal);

    //        for (int j = 0; j < supplyTargetDensitiesTimesSupplyRates[i].Count; j++)
    //            supplyTargetDensitiesTimesSupplyRates[i][j] *= supplyRates[i][j];
    //    });
    //}


    //TODO: should return reference
    public List<List<float>> GetGradientVector(int n) {

        if (gradientVectorComputed[n] == false)
            ComputeGradientVector(n);

        return gradientVectors[n];
    }

    public List<List<float>> GetGradientVector(int i, int j, int k) {
        int n = GetVoxelIndex(i, j, k);

        return GetGradientVector(n);

    }
    public List<List<float>> GetGradientVector(int i, int j) {

        return GetGradientVector(i, j, 0);
    }

    public List<List<float>> GetNearestGradientVector(Vector3 pos) {

        int n = GetNearestVoxelIndex(pos);
        return GetGradientVector(n);
    }


    public void ComputeGradientVector(int n){
        float twoDx = mesh.dX; 
        float twoDy = mesh.dY; 
        float twoDz = mesh.dZ; 
        bool gradientConstantsDefined = false;
        int[] indices = new int[3];
        
        if( gradientConstantsDefined == false ){
            twoDx *= 2f; 
            twoDy *= 2f; 
            twoDz *= 2f;
            gradientConstantsDefined = true; 
        }   
        
        indices = GetCartesianIndices(n);
        
        // d/dx 
        if( indices[0] > 0 && indices[0] < mesh.xCoordinates.Count-1 ) {
            for(int q = 0; q < GetNumberOfDensities() ; q++ ){
                gradientVectors[n][q][0] = densityVectors[n + thomasIJump][q];
                gradientVectors[n][q][0] -= densityVectors[n-thomasIJump][q]; 
                gradientVectors[n][q][0] /= twoDx; 
                gradientVectorComputed[n] = true; 
            }
        }

        // d/dy 
        if (indices[1] > 0 && indices[1] < mesh.yCoordinates.Count - 1) {
            for (int q = 0; q < GetNumberOfDensities(); q++) {
                gradientVectors[n][q][1] = densityVectors[n + thomasJJump][q];
                gradientVectors[n][q][1] -= densityVectors[n - thomasJJump][q];
                gradientVectors[n][q][1] /= twoDy;
                gradientVectorComputed[n] = true;
            }
        }
        
        // d/dz 
        if (indices[2] > 0 && indices[2] < mesh.zCoordinates.Count - 1) {
            for (int q = 0; q < GetNumberOfDensities(); q++) {
                gradientVectors[n][q][2] = densityVectors[n + thomasKJump][q];
                gradientVectors[n][q][2] -= densityVectors[n - thomasKJump][q];
                gradientVectors[n][q][2] /= twoDz;
                gradientVectorComputed[n] = true;
            }
        }


    }


    public void ComputeAllGradientVectors(){
        float twoDx = mesh.dX;
        float twoDy = mesh.dY;
        float twoDz = mesh.dZ;
        bool gradientConstantsDefined = false;
        int[] indices = new int[3];

        if (gradientConstantsDefined == false) {
            twoDx *= 2f;
            twoDy *= 2f;
            twoDz *= 2f;
            gradientConstantsDefined = true;
        }


        int xCnt = mesh.xCoordinates.Count;
        int yCnt = mesh.yCoordinates.Count;
        int zCnt = mesh.zCoordinates.Count;
        int totalCnt = xCnt * yCnt * zCnt;


        //n is the voxel index
        Parallel.For(0, totalCnt, n => {
            // d/dx 
            if (indices[0] > 0 && indices[0] < mesh.xCoordinates.Count - 1) {
                for (int q = 0; q < GetNumberOfDensities(); q++) {
                    gradientVectors[n][q][0] = densityVectors[n + thomasIJump][q];
                    gradientVectors[n][q][0] -= densityVectors[n - thomasIJump][q];
                    gradientVectors[n][q][0] /= twoDx;
                    gradientVectorComputed[n] = true;
                }
            }
   
        });

        ////TODO: done in parallel with openmp
        //for (int k = 0; k < mesh.zCoordinates.Count; k++) {
        //    for (int j = 0; j < mesh.yCoordinates.Count; j++) {
        //        for (int i = 0; i < mesh.xCoordinates.Count; i++) {
        //            int n = GetVoxelIndex(i, j, k);
        //            // d/dx 
        //            if (indices[0] > 0 && indices[0] < mesh.xCoordinates.Count - 1) {
        //                for (int q = 0; q < GetNumberOfDensities(); q++) {
        //                    gradientVectors[n][q][0] = densityVectors[n + thomasIJump][q];
        //                    gradientVectors[n][q][0] -= densityVectors[n - thomasIJump][q];
        //                    gradientVectors[n][q][0] /= twoDx;
        //                    gradientVectorComputed[n] = true;
        //                }
        //            }

        //        }

        //    }
        //}

        ////TODO: done in parallel with openmp
        //for (int k = 0; k < mesh.zCoordinates.Count; k++) {
        //    for (int j = 0; j < mesh.yCoordinates.Count; j++) {
        //        for (int i = 0; i < mesh.xCoordinates.Count; i++) {
        //            int n = GetVoxelIndex(i, j, k);
        //            // d/dy 
        //            if (indices[1] > 0 && indices[1] < mesh.yCoordinates.Count - 1) {
        //                for (int q = 0; q < GetNumberOfDensities(); q++) {
        //                    gradientVectors[n][q][1] = densityVectors[n + thomasJJump][q];
        //                    gradientVectors[n][q][1] -= densityVectors[n - thomasJJump][q];
        //                    gradientVectors[n][q][1] /= twoDy;
        //                    gradientVectorComputed[n] = true;
        //                }
        //            }

        //        }

        //    }
        //}

        //n is the voxel index
        Parallel.For(0, totalCnt, n => {
            // d/dy 
            if (indices[1] > 0 && indices[1] < mesh.yCoordinates.Count - 1) {
                for (int q = 0; q < GetNumberOfDensities(); q++) {
                    gradientVectors[n][q][1] = densityVectors[n + thomasJJump][q];
                    gradientVectors[n][q][1] -= densityVectors[n - thomasJJump][q];
                    gradientVectors[n][q][1] /= twoDy;
                    gradientVectorComputed[n] = true;
                }
            }

        });

        ////TODO: done in parallel with openmp
        //for (int k = 0; k < mesh.zCoordinates.Count; k++) {
        //    for (int j = 0; j < mesh.yCoordinates.Count; j++) {
        //        for (int i = 0; i < mesh.xCoordinates.Count; i++) {
        //            int n = GetVoxelIndex(i, j, k);
        //            // d/dz 
        //            if (indices[2] > 0 && indices[2] < mesh.zCoordinates.Count - 1) {
        //                for (int q = 0; q < GetNumberOfDensities(); q++) {
        //                    gradientVectors[n][q][2] = densityVectors[n + thomasKJump][q];
        //                    gradientVectors[n][q][2] -= densityVectors[n - thomasKJump][q];
        //                    gradientVectors[n][q][2] /= twoDz;
        //                    gradientVectorComputed[n] = true;
        //                }
        //            }

        //        }

        //    }
        //}


        //n is the voxel index
        Parallel.For(0, totalCnt, n => {
            // d/dz 
            if (indices[2] > 0 && indices[2] < mesh.zCoordinates.Count - 1) {
                for (int q = 0; q < GetNumberOfDensities(); q++) {
                    gradientVectors[n][q][2] = densityVectors[n + thomasKJump][q];
                    gradientVectors[n][q][2] -= densityVectors[n - thomasKJump][q];
                    gradientVectors[n][q][2] /= twoDz;
                    gradientVectorComputed[n] = true;
                }
            }

        });

        
    }

    public void ResetAllGradientVectors(){
        for (int k = 0; k < mesh.voxels.Count; k++) {
            for (int i = 0; i < GetNumberOfDensities(); i++) {
                gradientVectors[k][i].Assign(3, 0f);
            }


        }

        gradientVectorComputed.Assign(GetNumberOfVoxels(), false);
    }
	



  
}
