
using System;


[System.Serializable]
public class Volume {

    //Initialized with breast epithelial cell parameters

    public float total = 2494; //microm3


    public float targetNuclear = 540f;
    public float nuclear = 540f;
    public float cytoplasmic;

    public float fluid;
    public float solid;

    public float nuclearFluid;
    public float nuclearSolid;

    public float cytoplasmicFluid;
    public float cytoplasmicSolid;

    public float targetNuclearSolid = 135f;

    public float fCytoplasmicToNuclear = 3.6f;
    public float fFluid = 0.75f;


    //rate constants for fluid, nuclear and cytoplasmic
    public float rFluid = 3f / Globals.timeConst;
    public float rNuclear = 0.33f / Globals.timeConst;
    public float rCytoplasmic = 0.27f / Globals.timeConst;


    public float ruptureVolume;
    public float relativeRuptureVolume  = 0;

    public float densityWater = 1f; //kg/m3
    public float densitySolid = 1.3f; //kg/m3
        




    public Volume() {

        fluid = fFluid * total;
        solid = total - fluid;

        nuclearFluid = fFluid * nuclear;
        nuclearSolid = nuclear - nuclearFluid;

        cytoplasmicFluid = fFluid * cytoplasmic;
        cytoplasmicSolid = cytoplasmic - cytoplasmicFluid;

    }

    /***
     * Copy the fields of vol
     */
    public void DeepCopy(Volume vol) {
        total = vol.total;
        fluid = vol.fluid;
        solid = vol.solid;

        nuclear = vol.nuclear;
        nuclearFluid = vol.nuclearFluid;
        nuclearSolid = vol.nuclearSolid;

        cytoplasmic = vol.cytoplasmic;
        cytoplasmicFluid = vol.cytoplasmicFluid;
        cytoplasmicSolid = vol.cytoplasmicSolid;

        targetNuclearSolid = vol.targetNuclearSolid;

    }

    public float GetRadius() {


        const float four_thirds_pi = 0.42441318157816f;

        return (float)Math.Pow(total * four_thirds_pi, 0.3333333333333f);
    }



    public float GetNuclearRadius() {


        const float four_thirds_pi = 0.42441318157816f;

        return (float)Math.Pow(nuclear * four_thirds_pi, 0.3333333333333f);
    }

    public double GetMass(){
        return densitySolid * solid + densityWater * fluid;
    }
    public void UpdateVolume(float deltaTime) {


        fluid += deltaTime * rFluid * (fFluid * total - fluid);
        if (fluid < 0f)
            fluid = 0f;

        nuclearSolid += deltaTime * rNuclear * (targetNuclearSolid - nuclearSolid);
        if (nuclearSolid < 0f)
            nuclearSolid = 0f;

        nuclearFluid = fluid * nuclear / total;


        cytoplasmicSolid += deltaTime * rCytoplasmic * (fCytoplasmicToNuclear * targetNuclearSolid - cytoplasmicSolid);
        if (cytoplasmicSolid < 0f)
            cytoplasmicSolid = 0f;

        cytoplasmicFluid = fluid - nuclearFluid;

        if (cytoplasmicFluid < 0)
            cytoplasmicFluid = 0f;

        nuclear = nuclearSolid + nuclearFluid;

        cytoplasmic = cytoplasmicSolid + cytoplasmicFluid;

        total = cytoplasmic + nuclear;

    }

    public void Divide(float ratio) {
        if (total < Globals.volumeThreshold)
            return;

        total *= ratio;
        fluid *= ratio;
        solid *= ratio;

        nuclear *= ratio;
        nuclearFluid *= ratio;
        nuclearSolid *= ratio;

        cytoplasmic *= ratio;
        cytoplasmicFluid *= ratio;
        cytoplasmicSolid *= ratio;

        targetNuclearSolid *= ratio;



    }

}
