using System;

public static class Globals {

    public static int maxCellCnt = 1000;
    public static bool animateDivision = false;


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

  

}
