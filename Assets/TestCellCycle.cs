using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;


public class TestCellCycle  {
    
    [Test]
    public void TestPhaseNames(){
        Assert.AreEqual(Globals.G0, "G0");
        Assert.AreEqual(Globals.G1, "G1");
        Assert.AreEqual(Globals.G2, "G2");
        Assert.AreEqual(Globals.S, "S");
        Assert.AreEqual(Globals.M, "M");
        Assert.AreEqual(Globals.A, "A");
        Assert.AreEqual(Globals.D, "D");
        Assert.AreEqual(Globals.NS, "NS");
        Assert.AreEqual(Globals.NL, "NL");
    }
    [Test]
    public void TestCellCycleInitialization(){
        CellCycle cellCycle = new CellCycle();


        Assert.AreEqual(cellCycle.phases.Count, 0);
        Assert.AreEqual(cellCycle.transitionRates.Length, 10);

        PhaseT phase = cellCycle.GetCurrentPhase();

        Assert.AreEqual(phase.ind, 0);
        Assert.AreEqual(phase.name, Globals.G1);

        cellCycle = null;
    }
    [Test]
    public void TestFlowCytometryModel(){
        CellCycle cellCycle = new CellCycle();
        cellCycle.AddPhase(new PhaseT(0, Globals.G1, 1, Color.red));
        cellCycle.AddPhase(new PhaseT(1, Globals.S, 2, Color.green));
        cellCycle.AddPhase(new PhaseT(2, Globals.M, 0, Color.blue));

        Assert.AreEqual(cellCycle.phases.Count, 3);

        PhaseT phase0 = cellCycle.GetPhaseByName(Globals.G1);
        Assert.AreEqual(phase0.name, Globals.G1);
        Assert.AreEqual(phase0.ind, 0);
        Assert.AreEqual(phase0.nextPhaseInd, 1);

        PhaseT phase1 = cellCycle.GetPhaseByName(Globals.S);
        Assert.AreEqual(phase1.name, Globals.S);
        Assert.AreEqual(phase1.ind, 1);
        Assert.AreEqual(phase1.nextPhaseInd, 2);

        PhaseT phase2 = cellCycle.GetPhaseByName(Globals.M);
        Assert.AreEqual(phase2.name, Globals.M);
        Assert.AreEqual(phase2.ind, 2);
        Assert.AreEqual(phase2.nextPhaseInd, 0);

        //phase should be {1, s, 2}
        int nextPhaseInd = cellCycle.ExitPhase(0);
        PhaseT phase = cellCycle.GetPhaseByInd(nextPhaseInd);
        Assert.AreEqual(phase.ind, phase0.nextPhaseInd);
        Assert.AreEqual(phase.name, Globals.S);
        Assert.AreEqual(phase.nextPhaseInd, 2);


        cellCycle.AddTransitionRate(Globals.G1, Globals.S, 10f, false);
        Assert.AreEqual(cellCycle.transitionRates[0][1].fixedDuration, false);
        Assert.AreEqual(cellCycle.transitionRates[0][1].transitionRate, 10f);

        cellCycle = null;
               
    }




}
