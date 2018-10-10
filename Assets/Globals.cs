using System;

public static class Globals {

    public static bool animationRunning = false;
    public static int maxCellCnt = 1000;
    public static bool animateDivision = false;

    public static float timePassedInAnimation = 0f;


    //public static float timeConst = 0.1f; //funda
    public static float timeConst = 1f; //hour

	public static int cellCnt = 1 ;

	public static float volumeThreshold = 0.000001f;

    public static string G0 = "G0"; //gap
    public static string G1 = "G1"; //gap
    public static string G2 = "G2"; //gap
    public static string S = "S";  //synthesis
    public static string M = "M";  //mitosis
    public static string A = "A";  //apoptosis
    public static string NL = "NL";  //necrosis lysed
    public static string NS = "NS";  //necrosis swelling
    public static string D = "D";  //dead
    public static string Ki67Neg = "Ki67Neg";  //negative Ki67
    public static string Ki67Pos = "Ki67Pos";  //pos Ki67
    public static string Ki67PosPreMitotic = "Ki67PosPreMitotic";  //pre mitosis
    public static string Ki67PosPostMitotic = "Ki67PosPostMitotic";  //pre mitosis

    public static float massVolumeConst = 0.0004f; //1/2494


    public static float diffusionDt = 0.01f;
    public static float mechanicsDt = 0.1f;
    public static float phenotypeDt = 6f;


    public static int meshMinXIndex = 0;
    public static int meshMinYIndex = 1;
    public static int meshMinZIndex = 2;
    public static int meshMaxXIndex = 3;
    public static int meshMaxYIndex = 4;
    public static int meshMaxZIndex = 5;

    public static int meshLXFaceIndex = 0;
    public static int meshLYFaceIndex = 1;
    public static int meshLZFaceIndex = 2;
    public static int meshUXFaceIndex = 3;
    public static int meshUYFaceIndex = 4;
    public static int meshUZFaceIndex = 5;

    public static int SPHEROID = 0; 
    public static int BREASTDUCT = 1; 


    public static int OXYGEN = 0; //oxygen concentration in density vectors


    //public static float oxygenDiffusionCoefficient = 100000f; //FUNDA????
    public static float oxygenDiffusionCoefficient = 10f;
    public static float oxygenDecayRate = 0.1f;
    public static float oxygenNecrosisThreshold =  5.0f; 
    public static float oxygenNecrosisMax =  2.5f;
    public static float cellSecretionRate = 0f;
    public static float cellSaturationDensity = 38f;
    public static float cellUptakeRate = 10f;//Value for DCIS 
  

}
