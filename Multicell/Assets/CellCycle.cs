using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;



[System.Serializable]
public struct PhaseT {
    public int ind;
    public string name;
    public int nextPhaseInd;


    public PhaseT(int ind, string name, int nextPhaseInd) {
        this.ind = ind;
        this.name = name;
        this.nextPhaseInd = nextPhaseInd;

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


    float elapsedTimeInPhase = 0;
    float currentTime;
    float phaseDuration;



    public CellCycle() {

        elapsedTimeInPhase = 0;
        currentTime = 0;
        //name will be filled in by the specific model

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

    public void AddTransitionRate(string fromName, string toName, float rate, bool fixedDuration) {


        PhaseT phaseFrom = GetPhaseByName(fromName);
        PhaseT phaseTo = GetPhaseByName(toName);
        transitionRates[phaseFrom.ind][phaseTo.ind] = new PhaseLink(rate, fixedDuration);
    }


    public int ExitPhase(int phaseInd) {

        PhaseT phase = GetPhaseByInd(phaseInd);
        PhaseT nextPhase;
        
        if (phase.name.Equals(Globals.A)) {
            nextPhase = GetPhaseByName(Globals.D);
        }
        else {
            nextPhase = phases[phase.nextPhaseInd]; //move to the next phase
            elapsedTimeInPhase = 0;
        }

        
        return nextPhase.ind;
    }

    public void Advance(float deltaTime) {
        currentTime += deltaTime;
        elapsedTimeInPhase += deltaTime;

        for (int i = 0; i < transitionRates[currentPhaseInd].Length; i++) {
            if (transitionRates[currentPhaseInd][i].fixedDuration && elapsedTimeInPhase >= transitionRates[currentPhaseInd][i].transitionRate) {
                currentPhaseInd = ExitPhase(currentPhaseInd);
                EnterPhase(currentPhaseInd);
            }
            else {
                float rate = transitionRates[currentPhaseInd][i].transitionRate * deltaTime;
                if (rate > 0) {
                    float randVal = Utilities.GetRandomNumber(0f, 1f);
                    if (randVal <= rate) {
                        currentPhaseInd = ExitPhase(currentPhaseInd);
                        EnterPhase(currentPhaseInd);
                    }

                }
            }
        }
    }

    public virtual void EnterPhase(int phaseInd) {
        return;
    }
}

[System.Serializable]
public class LiveCells : CellCycle {

    public LiveCells() {
        //float transitionRateApoptosis = 0.116297f / Globals.timeConst;
        float transitionRateDivision = 0.0432f / Globals.timeConst;

        AddPhase(new PhaseT(0, Globals.G1, 1));
        AddPhase(new PhaseT(1, Globals.M, 0));

        AddTransitionRate(Globals.G1, Globals.M, transitionRateDivision, false);
        //AddTransitionRate(Globals.G1, Globals.A, 1f / transitionRateApoptosis, true);
    }

}

[System.Serializable]
public class ApoptosisModel : CellCycle {

    public ApoptosisModel(CellPhenotype phenotype) {
        
        base.phenotype = phenotype;

        //divided by 1 as it is fixed rate
        float transitionRateApoptosis = 0.116297f / Globals.timeConst ;

       

        AddPhase(new PhaseT(0, Globals.A, 1));
        AddPhase(new PhaseT(1, Globals.D, 1));

        AddTransitionRate(Globals.A, Globals.S, 1f / transitionRateApoptosis, true);
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

            base.phenotype.relativeRuptureVolume = 2f;
            base.phenotype.calcificationRate = 0; 
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

        AddPhase(new PhaseT(0, Globals.G1, 1));
        AddPhase(new PhaseT(1, Globals.S, 2));
        AddPhase(new PhaseT(2, Globals.M, 0));


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
