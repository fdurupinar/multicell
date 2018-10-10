using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;



[System.Serializable]
public struct PhaseT {
    public int ind;
    public string name;
    public Color color;
    public int nextPhaseInd;


    public PhaseT(int ind, string name, int nextPhaseInd, Color color) {
        this.ind = ind;
        this.name = name;
        this.nextPhaseInd = nextPhaseInd;
        this.color = color;

    }
};
[System.Serializable]
public struct PhaseLink {

    public float transitionRate;
    public bool fixedDuration;


    public PhaseLink(float transitionRate, bool fixedDuration) {
        this.transitionRate = transitionRate;
        this.fixedDuration = fixedDuration;

    }
}


[System.Serializable]
public class CellCycle {

    protected CellPhenotype phenotype;

    public List<PhaseT> phases = new List<PhaseT>();
    public int currentPhaseInd = 0;



    public PhaseLink[][] transitionRates;


    public float elapsedTimeInPhase = 0;

    public bool isDivisionPhase = false;


    public CellCycle() {

        this.InitParams();
    }

    public void InitParams(){

        isDivisionPhase = false;

        elapsedTimeInPhase = 0;

        currentPhaseInd = 0; //start in the initial phase

        //phases = new List<PhaseT>();
        transitionRates = new PhaseLink[10][];
        for (int i = 0; i < 10; i++)
            transitionRates[i] = new PhaseLink[10];

    }
    public void SetPhenotype(CellPhenotype pt){
        this.phenotype = pt;
    }

    public void AddPhase(PhaseT phase) {
        phases.Add(phase);
    }


    public PhaseT GetPhaseByName(string name) {

        return phases.Find((obj) => obj.name.Equals(name));
    }

    public PhaseT GetPhaseByInd(int ind) {

        return phases.Find((obj) => obj.ind == ind);
    }


    public PhaseT GetCurrentPhase(){
        return GetPhaseByInd(currentPhaseInd);
    }

    public PhaseT GetNextPhase() {
        PhaseT currentPhase = GetCurrentPhase();
        return phases[currentPhase.nextPhaseInd];
    }

    public void AddTransitionRate(string fromName, string toName, float rate, bool fixedDuration) {
        PhaseT phaseFrom = GetPhaseByName(fromName);
        PhaseT phaseTo = GetPhaseByName(toName);
        transitionRates[phaseFrom.ind][phaseTo.ind] = new PhaseLink(rate, fixedDuration);
    }


    public int ExitPhase(int phaseInd) {

        PhaseT phase = GetPhaseByInd(phaseInd);
        int nextPhaseInd = currentPhaseInd; //do not change phase otherwise

        if (phase.name.Equals(Globals.A)) {
            nextPhaseInd = GetPhaseByName(Globals.D).ind;
        }
        else if (phase.name.Equals(Globals.NS)) {
            //remain in non-lysed state if volume has not exceeded the rapture volume
            if (phenotype.volume.total >  phenotype.volume.ruptureVolume) { 
                nextPhaseInd = phases[phase.nextPhaseInd].ind; //move to the next phase
                elapsedTimeInPhase = 0;
            }
        }
        else if (phase.name.Equals(Globals.NL)) {            
            nextPhaseInd = GetPhaseByName(Globals.D).ind;
        }
        else {
            nextPhaseInd = phases[phase.nextPhaseInd].ind; //move to the next phase
            elapsedTimeInPhase = 0;
        }

        
        return nextPhaseInd;
    }


    public void UpdateTransitionRateForCurrentPhase(float newRate) {
        transitionRates[currentPhaseInd][GetNextPhase().ind].transitionRate = newRate;
    }

    public void Advance(float deltaTime) {
        
        elapsedTimeInPhase += deltaTime;

        int nextPhaseInd = GetNextPhase().ind;

        if (transitionRates[currentPhaseInd][nextPhaseInd].fixedDuration) {
            if (elapsedTimeInPhase >= 1f/transitionRates[currentPhaseInd][nextPhaseInd].transitionRate) {
                MoveToNextPhase();
            }
        }
        else { //stochastic
            float rate = transitionRates[currentPhaseInd][nextPhaseInd].transitionRate * deltaTime;

            if (rate > 0) { 
                float randVal = Utilities.GetRandomNumber(0f, 1f);
                if (randVal <= rate) {
                    MoveToNextPhase();

                }
            }          
        }
    }

    public void MoveToNextPhase(){
        if (GetCurrentPhase().name.Equals(Globals.M) || GetCurrentPhase().name.Equals(Globals.Ki67PosPreMitotic)) //we are moving out of mitosis -- time to create the clone
            this.isDivisionPhase = true;
            
        currentPhaseInd = ExitPhase(currentPhaseInd); //current is now the next        

        EnterPhase(currentPhaseInd);
    }


    public virtual void EnterPhase(int phaseInd) {
        return;
    }
}





[System.Serializable]
public class ApoptosisModel : CellCycle {

    public ApoptosisModel(CellPhenotype phenotype) : base(){
        base.InitParams();
        base.phenotype = phenotype;

        //divided by 1 as it is fixed rate
        float transitionRateApoptosis = 1f /(8.6f * Globals.timeConst) ;
               
        AddPhase(new PhaseT(0, Globals.A, 1, Color.red));
        AddPhase(new PhaseT(1, Globals.D, 1, Color.green));

        AddTransitionRate(Globals.A, Globals.D, transitionRateApoptosis, true);
    }

    public override void EnterPhase(int phaseInd){
        PhaseT phase = GetPhaseByInd(phaseInd);
        if (phase.name.Equals(Globals.A)) {
            base.phenotype.isDying = true;
            //update volume constants
            base.phenotype.volume.rFluid = 3f / Globals.timeConst;
            base.phenotype.volume.rCytoplasmic = 1f / Globals.timeConst;
            base.phenotype.volume.rNuclear = 0.35f / Globals.timeConst;

            base.phenotype.volume.nuclearSolid = 0;
            base.phenotype.volume.fCytoplasmicToNuclear = 0;
            base.phenotype.volume.fFluid = 0;

            base.phenotype.volume.relativeRuptureVolume = 2f;
            base.phenotype.volume.ruptureVolume = phenotype.volume.total * base.phenotype.volume.relativeRuptureVolume;
            base.phenotype.calcificationRate = 0; 
        }
    }
}



[System.Serializable]
public class NecrosisModel : CellCycle {

    public NecrosisModel(CellPhenotype phenotype): base() {
        base.InitParams();
        base.phenotype = phenotype;

        //divided by 1 as it is fixed rate
        float transitionRateNecrosis0 = float.MaxValue;// set high so it's always evaluating against the "arrest" 

        float transitionRateNecrosis1 = 1f/(24f * 60f * Globals.timeConst);// 60 days max

        AddPhase(new PhaseT(0, Globals.G0, 1, Color.red)); //could be any state

        AddPhase(new PhaseT(1, Globals.NS, 2, new Color(0.5f,0.5f,0.5f)));
        AddPhase(new PhaseT(2, Globals.NL, 3, new Color(0.2f, 0.2f, 0.2f)));
        AddPhase(new PhaseT(3, Globals.D, 3, Color.black));

        AddTransitionRate(Globals.G0, Globals.NS, -1, false); //not fixed rate, depends on the current conditions
        AddTransitionRate(Globals.NS, Globals.NL, transitionRateNecrosis0, false);
        AddTransitionRate(Globals.NL, Globals.D,  transitionRateNecrosis1, true);
    }

    public override void EnterPhase(int phaseInd) {
        PhaseT phase = GetPhaseByInd(phaseInd);
        if (phase.name.Equals(Globals.NS)) { //standard necrosis entry
            Debug.Log("dying");
            base.phenotype.isDying = true;
            //update volume constants
            base.phenotype.volume.rFluid = 0.67f / Globals.timeConst;  //unlysed
            base.phenotype.volume.rCytoplasmic = 0.0032f / Globals.timeConst;
            base.phenotype.volume.rNuclear = 0.013f / Globals.timeConst;

            base.phenotype.volume.nuclearSolid = 0;
            base.phenotype.volume.fCytoplasmicToNuclear = 0;
            base.phenotype.volume.fFluid = 1f;

            base.phenotype.volume.relativeRuptureVolume = 2f;
            base.phenotype.volume.ruptureVolume = phenotype.volume.total * base.phenotype.volume.relativeRuptureVolume;
            base.phenotype.calcificationRate = 0.0042f;
        }
        else if (phase.name.Equals(Globals.NL)) { //standard lysis entry
            Debug.Log("lysis has started");
            base.phenotype.isDying = true;
            //update volume constants
            base.phenotype.volume.rFluid = 0.05f / Globals.timeConst; //lysed
            base.phenotype.volume.rCytoplasmic = 0.0032f / Globals.timeConst;
            base.phenotype.volume.rNuclear = 0.013f / Globals.timeConst;

            base.phenotype.volume.nuclearSolid = 0;
            base.phenotype.volume.fCytoplasmicToNuclear = 0;
            base.phenotype.volume.fFluid = 0;

            base.phenotype.volume.relativeRuptureVolume = float.MaxValue; 
            base.phenotype.volume.ruptureVolume = phenotype.volume.total * base.phenotype.volume.relativeRuptureVolume;
            base.phenotype.calcificationRate = 0.0042f;
        }

    }
}


[System.Serializable]
public class FlowCytometry : CellCycle {

    public FlowCytometry(CellPhenotype phenotype): base() {
        base.InitParams();
        base.phenotype = phenotype;

        float G1SRate = (0.00324f* 60f) / Globals.timeConst;
        float SMRate = (0.00208f* 60f) / Globals.timeConst;
        float MG1Rate = (0.00333f*60f) / Globals.timeConst;

        AddPhase(new PhaseT(0, Globals.G1, 1, Color.red));
        AddPhase(new PhaseT(1, Globals.S, 2, Color.green));
        AddPhase(new PhaseT(2, Globals.M, 0, Color.blue));

        AddTransitionRate(Globals.G1, Globals.S, G1SRate, false);
        AddTransitionRate(Globals.S, Globals.M, SMRate, false);
        AddTransitionRate(Globals.M, Globals.G1, MG1Rate, false);
    }

    public override void EnterPhase(int phaseInd) {
        PhaseT phase = GetPhaseByInd(phaseInd);
        if (phase.name.Equals(Globals.S)) {
            base.phenotype.volume.nuclearSolid *= 2;
            base.phenotype.volume.cytoplasmicSolid *= 2;
        }
    }
}

[System.Serializable]
public class Ki67Advanced : CellCycle {

    public Ki67Advanced(CellPhenotype phenotype): base() {
        
        base.InitParams();

        base.phenotype = phenotype;

        float rate01 = 1f/( 3.62f * Globals.timeConst);
        float rate12 = 1f /(13f * Globals.timeConst);
        float rate20 = 1f/(2.5f * Globals.timeConst);

        AddPhase(new PhaseT(0, Globals.Ki67Neg, 1, Color.cyan));
        AddPhase(new PhaseT(1, Globals.Ki67PosPreMitotic, 2, new Color(1f, 0, 1f)));
        AddPhase(new PhaseT(3, Globals.Ki67PosPostMitotic, 0, Color.blue));

        AddTransitionRate(Globals.Ki67Neg, Globals.Ki67PosPreMitotic, rate01, false);
        AddTransitionRate(Globals.Ki67PosPreMitotic, Globals.Ki67PosPostMitotic, rate12, true);
        AddTransitionRate(Globals.Ki67PosPostMitotic, Globals.Ki67Neg, rate20, true);

    }

    public override void EnterPhase(int phaseInd) {
        PhaseT phase = GetPhaseByInd(phaseInd);
      
    }
}