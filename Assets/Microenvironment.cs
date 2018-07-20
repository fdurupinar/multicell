using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MicroenvironmentOptions{
    public Microenvironment microenvironment;
    public string name;
    public string timeUnits;
    public string spatialUnits;
    public float dX, dY, dZ;

    public bool outerDirichletConditions;
    public List<float> dirichletConditionVector;
    public List<bool> dirichletActivationVector;

    List<float> xRange;
    List<float> yRange;
    List<float> zRange;

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
        dX = 20;
        dY = 20;
        dZ = 20;

        outerDirichletConditions = false;
        dirichletConditionVector.Add(38f);

        xRange.Add(500f);
        xRange.Add(500f);
        xRange[0] *= -1f;

        yRange.Add(500f);
        yRange.Add(500f);
        yRange[0] *= -1f;

        zRange.Add(500f);
        zRange.Add(500f);
        zRange[0] *= -1f;

        calculateGradients = false;
    }
}

public class Microenvironment {

    public string name;
    public string timeUnits;
    public string spatialUnits;

    public CartesianMesh mesh;

    List<float> zero;
    List<float> one;

    List<List<float>> dirichletValueVectors;
    List<float> dirichletConditionVector;
    List<bool> dirichletActivationVector;

    List<List<float>> densityVectors;
    List<List<float>> temporaryDensityVectors1;
    List<List<float>> temporaryDensityVectors2;


    List<List<float>> bulkSourceAndSinkSolverTemp1;
    List<List<float>> bulkSourceAndSinkSolverTemp2;
    List<List<float>> bulkSourceAndSinkSolverTemp3;
    List<List<float>> bulkSourceAndSinkSolverTemp4;

    List<List<float>> supplyTargetDensitiesTimesSupplyRates;
    List<List<float>> supplyRates;
    List<List<float>> uptakeRates;


    List<List<float>> thomasTemp1;
    List<List<float>> thomasTemp2;

    List<Vector3> thomasConstant1Vec;
    List<Vector3> thomasNegConstant1Vec;

    List<float> thomasConstant1;
    List<float> thomasConstant1a;
    List<float> thomasConstant2;
    List<float> thomasConstant3;
    List<float> thomasConstant3a;

    List<List<Vector3>> thomasDenom;
    List<List<Vector3>> thomasC;


    bool thomasSetupDone;
    int thomasIJump;
    int thomasJJump;
    int thomasKJump;


    bool bulkSourceAndSinkSolverSetupDone;



    List<List<List<float>>> gradientVectors;
    List<bool> gradientVectorComputed;

    List<float> diffusionCoefficients;
    List<float> decayRates;

    bool diffusionSolverSetupDone;

    List<string> densityNames;
    List<string> densityUnits;


     //TODO:
    //should this be static?
    MicroenvironmentOptions defaultMicroenvironmentOptions;


    public Microenvironment(){

        mesh = new CartesianMesh();
        mesh.Resize(1, 1, 1);

        zero = new List<float>();
        one = new List<float>();

        dirichletValueVectors = new List<List<float>>();
        dirichletConditionVector = new List<float>();
        dirichletActivationVector = new List<bool>();

        densityVectors = new List<List<float>>();
        temporaryDensityVectors1 = new List<List<float>>();
        temporaryDensityVectors2 = new List<List<float>>();


        bulkSourceAndSinkSolverTemp1 = new List<List<float>>();
        bulkSourceAndSinkSolverTemp2 = new List<List<float>>();
        bulkSourceAndSinkSolverTemp3 = new List<List<float>>();
        bulkSourceAndSinkSolverTemp4 = new List<List<float>>();

        supplyTargetDensitiesTimesSupplyRates = new List<List<float>>();
        supplyRates = new List<List<float>>();
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

        thomasDenom = new List<List<Vector3>>();
        thomasC = new List<List<Vector3>>();


        gradientVectors = new List<List<List<float>>>();
        gradientVectorComputed = new List<bool>();

        diffusionCoefficients = new List<float>();

        decayRates = new List<float>();

        densityNames = new List<string>();
        densityUnits = new List<string>();

        zero.Add(0f);
        one.Add(1f);


        name = "unnamed";
        spatialUnits = "none";
        timeUnits = "none";

        bulkSourceAndSinkSolverSetupDone = false;
        thomasSetupDone = false;
        diffusionSolverSetupDone = false;

        for (int i = 0; i < mesh.voxels.Count; i++) {
            temporaryDensityVectors1[i] = new List<float>(zero);
            temporaryDensityVectors2[i] = new List<float>(zero);

            gradientVectors[i] = new List<List<float>>();
            List<float> ones = new List<float>();
            ones.Add(0);
            ones.Add(0);
            ones.Add(0);

            gradientVectors[i].Add(ones);

            gradientVectorComputed[i] = false;

            dirichletValueVectors[i] = new List<float>(one);


            dirichletActivationVector.Add(true);
        }

        densityNames.Add("unnamed");
        densityUnits.Add("none");

        for (int i = 0; i < GetNumberOfDensities(); i++) {
            diffusionCoefficients.Add(0f);
            decayRates.Add(0f);
        }


        //TODO:
        //should this be static?
        defaultMicroenvironmentOptions = new MicroenvironmentOptions();
    }

    //Calls the other one
    public Microenvironment(string name):this(){

        this.name = name;

    }

    void AddDirichletNode(int voxelIndex, List<float> value) {
        mesh.voxels[voxelIndex].isDirichlet = true;

        //TODO: check this pass by ref?
        dirichletValueVectors[voxelIndex] = value;

    }

    void RemoveDirichletNode(int voxelIndex) {
        mesh.voxels[voxelIndex].isDirichlet = false;
    }

    bool IsDirichletNode(int voxelIndex) {
        return mesh.voxels[voxelIndex].isDirichlet;
    }


    void SetSubstrateDirichletActivation(int substrateIndex, bool newValue) {
        dirichletActivationVector[substrateIndex] = newValue;
    }

    //openmp Uses parallelization here
    void ApplyDirichletConditions() {
        for (int i = 0; i < mesh.voxels.Count; i++) {
            if (mesh.voxels[i].isDirichlet) {
                for (int j = 0; j < dirichletValueVectors[i].Count; j++) {
                    if (dirichletActivationVector[j])
                        GetDensityVector(i)[j] = dirichletValueVectors[i][j];
                }
            }
        }
    }


    void ResizeSpaceRest() {
        for (int k = 0; k < mesh.voxels.Count; k++) {
            temporaryDensityVectors1[k] = new List<float>(zero);
            temporaryDensityVectors2[k] = new List<float>(zero);

            gradientVectors[k] = new List<List<float>>();
            for (int i = 0; i < GetNumberOfDensities(); i++) {
                gradientVectors[k][i] = new List<float>();
                gradientVectors[k][i].Add(0);
                gradientVectors[k][i].Add(0);
                gradientVectors[k][i].Add(0);
            }

            gradientVectorComputed.Add(false);
            dirichletValueVectors[k] = new List<float>(one);


        }
    }
    void ResizeSpace(int xNodes, int yNodes, int zNodes) {
        mesh.Resize(xNodes, yNodes, zNodes);
        ResizeSpaceRest();


    }

    void ResizeSpace(float xStart, float xEnd, float yStart, float yEnd, float zStart, float zEnd, int xNodes, int yNodes, int zNodes) {
        mesh.Resize(xStart, xEnd, yStart, yEnd, zStart, zEnd, xNodes, yNodes, zNodes);
        ResizeSpaceRest();
    }



    void ResizeSpace(float xStart, float xEnd, float yStart, float yEnd, float zStart, float zEnd, float dXNew, float dYNew, float dZNew) {
        mesh.Resize(xStart, xEnd, yStart, yEnd, zStart, zEnd, dXNew, dYNew, dZNew);
    }


    void ResizeSpaceUniform(float xStart, float xEnd, float yStart, float yEnd, float zStart, float zEnd, float dXNew) {
        ResizeSpace(xStart, xEnd, yStart, yEnd, zStart, zEnd, dXNew, dXNew, dXNew);
    }

    void ResizeDensities(int newSize) {


        zero = new List<float>();
        one = new List<float>();
        for (int j = 0; j < newSize; j++) {
            zero.Add(0f);
            one.Add(1f);

        }
        for(int k = 0; k < mesh.voxels.Count; k++) {
            temporaryDensityVectors1[k] = new List<float>(zero);
            temporaryDensityVectors2[k] = new List<float>(zero);

            gradientVectors[k] = new List<List<float>>();
            for (int i = 0; i < GetNumberOfDensities(); i++) {
                gradientVectors[k][i] = new List<float>();
                gradientVectors[k][i].Add(0);
                gradientVectors[k][i].Add(0);
                gradientVectors[k][i].Add(0);
            }

            gradientVectorComputed.Add(false);

            dirichletValueVectors[k] = new List<float>(one);


        }

        for(int j = 0; j < newSize; j++) {
            densityNames.Add("unnamed");
            densityUnits.Add("none");
            diffusionCoefficients.Add(0);
            decayRates.Add(0);
            dirichletActivationVector.Add(true);
            dirichletConditionVector.Add(1f);
            dirichletActivationVector.Add(true);
        }
    }



    void AddDensityRest() {

        zero.Add(0f);
        one.Add(1f);

        for(int i = 0; i < temporaryDensityVectors1.Count; i++) {
            temporaryDensityVectors1[i].Add(0f);
            temporaryDensityVectors2[i].Add(0f);
        }

        for (int k = 0; k < mesh.voxels.Count; k++){
            for (int i = 0; i < GetNumberOfDensities(); i++) {
                gradientVectors[k][i] = new List<float>();
                gradientVectors[k][i].Add(0);
                gradientVectors[k][i].Add(0);
                gradientVectors[k][i].Add(0);
            }


            gradientVectorComputed.Add(false);
            dirichletValueVectors[k] = new List<float>(one);


        }
        for (int i = 0; i < GetNumberOfDensities(); i++) {
            dirichletActivationVector.Add(true);
        }

        defaultMicroenvironmentOptions.dirichletConditionVector.Add(1f);
        defaultMicroenvironmentOptions.dirichletActivationVector.Add(true);        

    }

    void AddDensity(){

        densityNames.Add("unnamed");
        densityUnits.Add("none");

        diffusionCoefficients.Add(0);
        decayRates.Add(0);

        AddDensityRest();

    }


    void AddDensity(string dName, string units) {
        densityNames.Add(dName);
        densityUnits.Add(units);

        diffusionCoefficients.Add(0);
        decayRates.Add(0);

        AddDensityRest();
    }

    void AddDensity(string dName, string units, float diffusionConstant, float decayRate) {
        densityNames.Add(dName);
        densityUnits.Add(units);

        diffusionCoefficients.Add(diffusionConstant);
        decayRates.Add(decayRate);

        AddDensityRest();
    }

    int FindDensityIndex(string dName){

        int val = -1;
        for (int i = 0; i < densityNames.Count; i++)
            if (densityNames[i].Equals(dName))
                val = i;

        return val;
    }



    void SetDensity(int index, string dName, string units){
        if (index == 0)
            defaultMicroenvironmentOptions.useOxygenAsFirstField = false;

        densityNames[index] = dName;
        densityUnits[index] = units;
    }

    void SetDensity(int index, string dName, string units, float diffusionConstant, float decayRate) {
        SetDensity(index, dName, units);
        diffusionCoefficients[index] = diffusionConstant;
        decayRates[index] = decayRate;
    }

    int GetNumberOfDensities(){
        return densityVectors[0].Count;
    }

    int GetVoxelIndex(int i, int j, int k){
        return mesh.GetVoxelIndex(i, j, k);
    }

    int[] GetCartesianIndices(int n) {
        return mesh.GetCartesianIndices(n);
    }

    int GetNearestVoxelIndex(Vector3 pos){
        return mesh.GetNearestVoxelIndex(pos);
    }

    Voxel GetVoxel(int voxelIndex){
        return mesh.voxels[voxelIndex];
    }

    int [] GetNearestCartesianIndices(Vector3 pos){
        return mesh.GetNearestCartesianIndices(pos);
    }

    Voxel GetNearestVoxel(Vector3 pos){
        return mesh.GetNearestVoxel(pos);
    }

    List<float> GetDensityVector(int i, int j, int k){
        return densityVectors[GetVoxelIndex(i, j, k)];        
    }
	

    List<float> GetDensityVector(int i, int j) {
        return densityVectors[GetVoxelIndex(i, j, 0)];
    }

    List<float> GetDensityVector(int n) {
        return densityVectors[n];
    }

    List<float> GetNearestDensityVector(int voxelIndex) {
        return densityVectors[voxelIndex];
    }
    List<float> GetNearestDensityVector(Vector3 pos) {
        return densityVectors[mesh.GetNearestVoxelIndex(pos)];
    }


    //TODO: will be filled in
    void SimulateDiffusionDecay(float deltaTime){
        
    }


    void SimulateSourceAndSink(float deltaTime, List<float> supplyRatesVal, List<float>supplyTargetDensitiesVal, List<float>uptakeRatesVal){
        if(!bulkSourceAndSinkSolverSetupDone){
            for (int i = 0; i < mesh.voxels.Count; i++){
                bulkSourceAndSinkSolverTemp1[i] = new List<float>(zero);


                bulkSourceAndSinkSolverTemp2[i] = new List<float>(zero);

                bulkSourceAndSinkSolverTemp3[i] = new List<float>(zero);

            }
            bulkSourceAndSinkSolverSetupDone = true;
        }

        //TODO make these functions
        for (int i = 0; i < mesh.voxels.Count; i++) {

            bulkSourceAndSinkSolverTemp1[i] = new List<float>(supplyRatesVal);
            bulkSourceAndSinkSolverTemp2[i] = new List<float>(supplyTargetDensitiesVal);
            bulkSourceAndSinkSolverTemp3[i] = new List<float>(uptakeRatesVal);

            for (int j = 0; j < bulkSourceAndSinkSolverTemp1[i].Count; j++) // temp2 = S*T
                bulkSourceAndSinkSolverTemp2[i][j] *= bulkSourceAndSinkSolverTemp1[i][j];

            for (int j = 0; j < bulkSourceAndSinkSolverTemp2[i].Count; j++) // out = out + dt*temp2 = out + dt*S*T
                densityVectors[i][j] += deltaTime * bulkSourceAndSinkSolverTemp2[i][j];

            for (int j = 0; j < bulkSourceAndSinkSolverTemp3[i].Count; j++) { 
                bulkSourceAndSinkSolverTemp3[i][j] += bulkSourceAndSinkSolverTemp1[i][j];// temp3 = U+S
                bulkSourceAndSinkSolverTemp3[i][j] *= deltaTime; // temp3 = dt*(U+S)
                bulkSourceAndSinkSolverTemp3[i][j] += 1f;  //temp3 = 1 + dt * (U + S)
            }

            for (int j = 0; j < densityVectors[i].Count; j++) 
                densityVectors[i][j] /= bulkSourceAndSinkSolverTemp3[i][j];

        }

    }


    void UpdateRates(List<float> supplyRatesVal, List<float> supplyTargetDensitiesTimesSupplyRatesVal, List<float> uptakeRatesVal) {
        if (supplyTargetDensitiesTimesSupplyRates.Count != mesh.voxels.Count) {
            for (int i = 0; i < mesh.voxels.Count; i++)
                supplyTargetDensitiesTimesSupplyRates[i] = new List<float>(zero);
        }

        if (supplyRates.Count != mesh.voxels.Count) {
            for (int i = 0; i < mesh.voxels.Count; i++)
                supplyRates[i] = new List<float>(zero);
        }

        if (uptakeRates.Count != mesh.voxels.Count) {
            for (int i = 0; i < mesh.voxels.Count; i++)
                uptakeRates[i] = new List<float>(zero);
        }



        for (int i = 0; i < mesh.voxels.Count; i++){
            uptakeRates[i] = new List<float>(uptakeRatesVal);
            supplyRates[i] = new List<float>(supplyRatesVal);
            supplyTargetDensitiesTimesSupplyRates[i] = new List<float>(supplyTargetDensitiesTimesSupplyRatesVal);

            for (int j = 0; j < supplyTargetDensitiesTimesSupplyRates[i].Count; j++)
                supplyTargetDensitiesTimesSupplyRates[i][j] *= supplyRates[i][j];
        }

    }

   

    //TODO: should return reference
    List<List<float>> GetGradientVector(int n) {

        if (gradientVectorComputed[n] == false)
            ComputeGradientVector(n);

        return gradientVectors[n];
    }

    List<List<float>> GetGradientVector(int i, int j, int k) {
        int n = GetVoxelIndex(i, j, k);

        return GetGradientVector(n);

    }
    List<List<float>> GetGradientVector(int i, int j) {

        return GetGradientVector(i, j, 0);
    }

    List<List<float>> GetNearestGradientVector(Vector3 pos) {

        int n = GetNearestVoxelIndex(pos);
        return GetGradientVector(n);
    }


   
    void ComputeGradientVector(int n){
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


    void ComputeAllGradientVectors(){
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

        //TODO: done in parallel with openmp
        for (int k = 0; k < mesh.zCoordinates.Count; k++) {
            for (int j = 0; j < mesh.yCoordinates.Count; j++) {
                for (int i = 0; i < mesh.xCoordinates.Count; i++) {
                    int n = GetVoxelIndex(i, j, k);
                    // d/dx 
                    if (indices[0] > 0 && indices[0] < mesh.xCoordinates.Count - 1) {
                        for (int q = 0; q < GetNumberOfDensities(); q++) {
                            gradientVectors[n][q][0] = densityVectors[n + thomasIJump][q];
                            gradientVectors[n][q][0] -= densityVectors[n - thomasIJump][q];
                            gradientVectors[n][q][0] /= twoDx;
                            gradientVectorComputed[n] = true;
                        }
                    }

                }

            }
        }

        //TODO: done in parallel with openmp
        for (int k = 0; k < mesh.zCoordinates.Count; k++) {
            for (int j = 0; j < mesh.yCoordinates.Count; j++) {
                for (int i = 0; i < mesh.xCoordinates.Count; i++) {
                    int n = GetVoxelIndex(i, j, k);
                    // d/dy 
                    if (indices[1] > 0 && indices[1] < mesh.yCoordinates.Count - 1) {
                        for (int q = 0; q < GetNumberOfDensities(); q++) {
                            gradientVectors[n][q][1] = densityVectors[n + thomasJJump][q];
                            gradientVectors[n][q][1] -= densityVectors[n - thomasJJump][q];
                            gradientVectors[n][q][1] /= twoDy;
                            gradientVectorComputed[n] = true;
                        }
                    }

                }

            }
        }

        //TODO: done in parallel with openmp
        for (int k = 0; k < mesh.zCoordinates.Count; k++) {
            for (int j = 0; j < mesh.yCoordinates.Count; j++) {
                for (int i = 0; i < mesh.xCoordinates.Count; i++) {
                    int n = GetVoxelIndex(i, j, k);
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

            }
        }
        
    }

    void ResetAllGradientVectors(){
        for (int k = 0; k < mesh.voxels.Count; k++) {
            for (int i = 0; i < GetNumberOfDensities(); i++) {
                gradientVectors[k][i] = new List<float>();
                gradientVectors[k][i].Add(0f);
                gradientVectors[k][i].Add(0f);
                gradientVectors[k][i].Add(0f);
            }

            gradientVectorComputed[k] = false;
        }

    }
	



  
}
