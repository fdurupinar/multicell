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


    public CellCycle() {

        elapsedTimeInPhase = 0;

        currentPhaseInd = 0; //start in the initial phase

        //phases = new List<PhaseT>();
        transitionRates = new PhaseLink[10][];
        for (int i = 0; i < 10; i++)
            transitionRates[i] = new PhaseLink[10];

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
        int nextPhaseInd = 0;

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

    public void Advance(float deltaTime) {
        
        elapsedTimeInPhase += deltaTime;

        int nextPhaseInd = GetNextPhase().ind;

        if (transitionRates[currentPhaseInd][nextPhaseInd].fixedDuration) {
            if (elapsedTimeInPhase >= transitionRates[currentPhaseInd][nextPhaseInd].transitionRate) {
                currentPhaseInd = ExitPhase(currentPhaseInd);
                EnterPhase(currentPhaseInd);
            }
        }
        else 
        {
            float rate = transitionRates[currentPhaseInd][nextPhaseInd].transitionRate * deltaTime;
            if (rate > 0) {
                float randVal = Utilities.GetRandomNumber(0f, 1f);
                if (randVal <= rate) {
                    currentPhaseInd = ExitPhase(currentPhaseInd);
                    EnterPhase(currentPhaseInd);
                }

            }

        }

    }

    public virtual void EnterPhase(int phaseInd) {
        return;
    }
}


[System.Serializable]
public class TestDivisionCycle : CellCycle {

    public TestDivisionCycle() {
        float transitionRateDivision = 0.432f / Globals.timeConst;

        AddPhase(new PhaseT(0, Globals.G1, 1, Color.Lerp(Color.yellow, Color.white, 0.7f)));
        AddPhase(new PhaseT(1, Globals.M, 0, Color.Lerp(Color.green, Color.white, 0.7f)));

        AddTransitionRate(Globals.G1, Globals.M, transitionRateDivision, false);
        AddTransitionRate(Globals.M, Globals.G1, 0, true);
    }

}


[System.Serializable]
public class LiveCells : CellCycle {

    public LiveCells() {        
        float transitionRateDivision = 0.0432f / Globals.timeConst;

        AddPhase(new PhaseT(0, Globals.G1, 1, Color.Lerp(Color.yellow, Color.white, 0.7f)));
        AddPhase(new PhaseT(1, Globals.M, 0, Color.Lerp(Color.green, Color.white, 0.7f)));

        AddTransitionRate(Globals.G1, Globals.M, transitionRateDivision, false);
        AddTransitionRate(Globals.M, Globals.G1, 0, true);
    }

}

[System.Serializable]
public class ApoptosisModel : CellCycle {

    public ApoptosisModel(CellPhenotype phenotype) {
        
        base.phenotype = phenotype;

        //divided by 1 as it is fixed rate
        float transitionRateApoptosis = 8.6f * Globals.timeConst ;
               
        AddPhase(new PhaseT(0, Globals.A, 1, Color.red));
        AddPhase(new PhaseT(1, Globals.D, 1, Color.green));

        AddTransitionRate(Globals.A, Globals.D, transitionRateApoptosis, true);
    }

    public override void EnterPhase(int phaseInd){
        PhaseT phase = GetPhaseByInd(phaseInd);
        if (phase.name.Equals(Globals.A)) {
            
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

    public NecrosisModel(CellPhenotype phenotype) {

        base.phenotype = phenotype;

        //divided by 1 as it is fixed rate
        float transitionRateNecrosis0 = float.MaxValue;// set high so it's always evaluating against the "arrest" 
        float transitionRateNecrosis1 = 24f * 60f * Globals.timeConst;// 60 days max
        AddPhase(new PhaseT(0, Globals.NS, 1, Color.red));
        AddPhase(new PhaseT(1, Globals.NL, 1, Color.green));

        AddTransitionRate(Globals.NS, Globals.NL, transitionRateNecrosis0, false);
        AddTransitionRate(Globals.NL, Globals.D,  transitionRateNecrosis1, true);

    }

    public override void EnterPhase(int phaseInd) {
        PhaseT phase = GetPhaseByInd(phaseInd);
        if (phase.name.Equals(Globals.NS)) { //standard necrosis entry

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

    public FlowCytometry(CellPhenotype phenotype) {

        base.phenotype = phenotype;

        float G1SRate = 0.00335f / Globals.timeConst;
        float SMRate = 0.00208f / Globals.timeConst;
        float MG1Rate = 0.00333f / Globals.timeConst;

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

    public Ki67Advanced(CellPhenotype phenotype) {

        base.phenotype = phenotype;

        float rate01 = 0.276234f / Globals.timeConst;
        float rate12 = 13 * Globals.timeConst;
        float rate23 = 0.00333f / Globals.timeConst;
        float rate30 = 2.5f * Globals.timeConst;

        AddPhase(new PhaseT(0, Globals.Ki67Neg, 1, Color.red));
        AddPhase(new PhaseT(1, Globals.Ki67PosPreMitotic, 2, Color.green));
        AddPhase(new PhaseT(2, Globals.M, 3, Color.blue));
        AddPhase(new PhaseT(3, Globals.Ki67PosPostMitotic, 0, Color.blue));


        AddTransitionRate(Globals.Ki67Neg, Globals.Ki67PosPreMitotic, rate01, false);
        AddTransitionRate(Globals.Ki67PosPreMitotic, Globals.M, rate12, true);
        AddTransitionRate(Globals.M, Globals.Ki67PosPostMitotic, rate23, false);
        AddTransitionRate(Globals.Ki67PosPostMitotic, Globals.Ki67Neg, rate30, true);


    }

    public override void EnterPhase(int phaseInd) {
        PhaseT phase = GetPhaseByInd(phaseInd);
        if (phase.name.Equals(Globals.S)) {
            base.phenotype.volume.nuclearSolid *= 2;
            base.phenotype.volume.cytoplasmicSolid *= 2;
        }
    }
}