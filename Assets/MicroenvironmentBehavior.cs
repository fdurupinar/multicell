using UnityEngine;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine.UI;

public class MicroenvironmentBehavior : MonoBehaviour {

    public Microenvironment microenvironment;
    public MicroenvironmentOptions defaultMicroenvironmentOptions;

    //Different colors for different substrates
    public Vector4[] densityColors = { new Vector4(255f,0f,0f), new Vector4(0f, 255f, 0f), new Vector4(0f, 0f, 255f), new Vector4(255f, 0f, 255f), new Vector4(0f, 255f, 255f), new Vector4(255f, 255f, 0f)};


    public Text winText;



    // Use this for initialization
    void Awake() {


        //Unity does this in the inspector
        microenvironment = new Microenvironment();
        defaultMicroenvironmentOptions = new MicroenvironmentOptions();

        microenvironment.microenvironmentOptions = defaultMicroenvironmentOptions;


        //microenvironment.AddDensity("oxygen", "mmHg", 10000f, 10f);

        InitializeMicroenvironment();


    }

    void InitializeMicroenvironment() {
        microenvironment.name = defaultMicroenvironmentOptions.name;

        if (defaultMicroenvironmentOptions.useOxygenAsFirstField) {
            microenvironment.SetDensity(0, "oxygen", "mmHg");
            microenvironment.diffusionCoefficients[0] = Globals.oxygenDiffusionCoefficient;
            microenvironment.decayRates[0] = Globals.oxygenDecayRate;//  //100 micron length scale //0.1f; 
        }


        microenvironment.ResizeSpace(defaultMicroenvironmentOptions.xRange[0], defaultMicroenvironmentOptions.xRange[1],
                                     defaultMicroenvironmentOptions.yRange[0], defaultMicroenvironmentOptions.yRange[1],
                                     defaultMicroenvironmentOptions.zRange[0], defaultMicroenvironmentOptions.zRange[1],
                                     defaultMicroenvironmentOptions.dX, defaultMicroenvironmentOptions.dY, defaultMicroenvironmentOptions.dZ);


        ////TODO FUNDA added ?????
        microenvironment.ResizeDensities(1);

        //TODO: added these
        //TODO: arbitrary values
        List<float> supplyVec = new List<float>();
        supplyVec.Assign(microenvironment.GetNumberOfDensities(), 10f);
        microenvironment.supplyRates.Resize(microenvironment.GetNumberOfVoxels(), supplyVec);


        List<float> supplyTargetDensitiesVec = new List<float>();
        supplyTargetDensitiesVec.Assign(microenvironment.GetNumberOfDensities(), 80f);
        microenvironment.supplyTargetDensities.Resize(microenvironment.GetNumberOfVoxels(), supplyTargetDensitiesVec);


        List<float> uptakeVec = new List<float>();
        uptakeVec.Assign(microenvironment.GetNumberOfDensities(), 10f);
        microenvironment.uptakeRates.Resize(microenvironment.GetNumberOfVoxels(), uptakeVec);



        // set units
        microenvironment.spatialUnits = defaultMicroenvironmentOptions.spatialUnits;
        microenvironment.timeUnits = defaultMicroenvironmentOptions.timeUnits;
        microenvironment.mesh.units = defaultMicroenvironmentOptions.spatialUnits;

        // set the initial 1ies to the values set in the Dirichlet_condition_vector
        for (int n = 0; n < microenvironment.GetNumberOfVoxels(); n++) {
            microenvironment.densityVectors[n] = defaultMicroenvironmentOptions.dirichletConditionVector.ToList();

        }

        if (defaultMicroenvironmentOptions.outerDirichletConditions) {
            
            for (int k = 0; k < microenvironment.mesh.zCoordinates.Count; k++) {
                // set Dirichlet conditions along the 4 outer edges 
                for (int i = 0; i < microenvironment.mesh.xCoordinates.Count; i++) {
                    int J = microenvironment.mesh.yCoordinates.Count - 1;
                    microenvironment.AddDirichletNode(microenvironment.GetVoxelIndex(i, 0, k), defaultMicroenvironmentOptions.dirichletConditionVector);
                    microenvironment.AddDirichletNode(microenvironment.GetVoxelIndex(i, J, k), defaultMicroenvironmentOptions.dirichletConditionVector);
                }
                int I = microenvironment.mesh.xCoordinates.Count - 1;
                for (int j = 1; j < microenvironment.mesh.yCoordinates.Count - 1; j++) {
                    microenvironment.AddDirichletNode(microenvironment.GetVoxelIndex(0, j, k), defaultMicroenvironmentOptions.dirichletConditionVector);
                    microenvironment.AddDirichletNode(microenvironment.GetVoxelIndex(I, j, k), defaultMicroenvironmentOptions.dirichletConditionVector);
                }
            }

            int K = microenvironment.mesh.zCoordinates.Count - 1;
            for (int j = 1; j < microenvironment.mesh.yCoordinates.Count - 1; j++) {
                for (int i = 1; i < microenvironment.mesh.xCoordinates.Count - 1; i++) {
                    microenvironment.AddDirichletNode(microenvironment.GetVoxelIndex(i, j, 0), defaultMicroenvironmentOptions.dirichletConditionVector);
                    microenvironment.AddDirichletNode(microenvironment.GetVoxelIndex(i, j, K), defaultMicroenvironmentOptions.dirichletConditionVector);
                }
            }
        }
    }



    // This should be called before cell updates
    void FixedUpdate() {
        // update the microenvironment
        if (Globals.animationRunning) {

            Globals.timePassedInAnimation += Time.deltaTime ;
            winText.text = "Time (hours): " +  Globals.timePassedInAnimation / (Globals.timeConst * Time.deltaTime * 60f)+"\n";
            winText.text += "# Cells = " + Globals.cellCnt;

            //microenvironment.SimulateBulkSourceAndSink(Time.deltaTime); //not done in physicell
            microenvironment.SimulateDiffusionDecay(Time.deltaTime); 

            //TODO: FUNDA: these may be just for visualization
            //if (defaultMicroenvironmentOptions.calculateGradients)
                //microenvironment.ComputeAllGradientVectors();
        }
    }

    //private void RenderMicroenvironment(){
    //    if (Application.isPlaying && Globals.animationRunning) {
                        
            
    //    }
    //}

    private void OnDrawGizmos() {
        //Gizmos.color = Color.magenta;

        if (Application.isPlaying && Globals.animationRunning) {

            for (int i = 0; i < microenvironment.GetNumberOfVoxels(); i++) {

                //Parallel.For(0, microenvironment.GetNumberOfVoxels(), i => {

                Voxel voxel = microenvironment.GetVoxel(i);
                float voxelSize = Mathf.Pow(voxel.volume, 0.333333f);

                //Draw densities for oxygen



                float c = microenvironment.densityVectors[i][0] / 38f; /// max 38f; 
                Gizmos.color = Color.Lerp(Color.yellow, Color.blue, c);
                Gizmos.color = new Vector4(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, 0.05f); //make it transparent


                Gizmos.DrawCube(voxel.center, new Vector3(voxelSize, voxelSize, voxelSize));


                //Gizmos.color = Color.blue;
                //Gizmos.DrawWireCube(voxel.center, new Vector3(voxelSize, voxelSize, voxelSize));



            }


        }

    }
}

