﻿using System;

using UnityEngine;

[System.Serializable]
public class CellMechanics {

    public Vector3 orientation;



    public float cellCellAdhesionStrength;
    public float cellBMAdhesionStrength;

    // this is a multiple of the cell (equivalent) radius
    public float relativeMaxAdhesionDistance;

    public Vector3 fAdhesionCC;
    public Vector3 fAdhesionBM;



    //includes motility
    public bool isMotile;

    public float persistenceTime;
    public float migrationSpeed;
    public Vector3 migrationBiasDirection;
    public float migrationBias;
    public Vector3 motilityVector;


    public CellMechanics() {
        
        cellCellAdhesionStrength = 0.4f * 60f / Globals.timeConst;
        cellBMAdhesionStrength = 4.0f * 60f / Globals.timeConst;
        relativeMaxAdhesionDistance = 1.25f;

        isMotile = false;
        persistenceTime = 1f;
        migrationSpeed = 1f;
        migrationBiasDirection = Vector3.zero;
        migrationBias = 0f;
        motilityVector = Vector3.zero;

    }

    public void SetOrientation() {

        float theta = Utilities.GetRandomNumber(0, Mathf.PI * 2);
        float z = Utilities.GetRandomNumber(-1, 1);
        float temp = Mathf.Sqrt(1 - z * z);

        orientation = new Vector3(temp * Mathf.Cos(theta), temp * Mathf.Sin(theta), z);

    }

    public Vector3 ComputeCellBMAdhesion(float radius, float dist, Vector3 distVec) {

        Vector3 f = Vector3.zero;

        float maxAdhesiveInteractiveDistance = relativeMaxAdhesionDistance * radius;

        if (dist < maxAdhesiveInteractiveDistance)
            f = cellBMAdhesionStrength * Mathf.Pow((1f - dist / maxAdhesiveInteractiveDistance), 2) * distVec;

        return f;

    }
    public Vector3 ComputeMotility(float deltaTime) {

        if (isMotile) {

            float val = Utilities.GetRandomNumber(0, 1);
            if (val < deltaTime / persistenceTime || persistenceTime < deltaTime) {
                float tempAngle = 2 * Mathf.PI * Utilities.GetRandomNumber(0, 1);
                float tempPhi = Mathf.PI * Utilities.GetRandomNumber(0, 1);

                Vector3 randVec = new Vector3();
                randVec.x = Mathf.Cos(tempAngle) * Mathf.Sin(tempPhi); ;
                randVec.y = Mathf.Sin(tempAngle) * Mathf.Sin(tempPhi); ;
                randVec.z = Mathf.Cos(tempPhi);

                //migration bias will be updated in another component

                motilityVector = (1 - migrationBias) * randVec + migrationBiasDirection * migrationBias;
                motilityVector = Vector3.Normalize(motilityVector);
                motilityVector *= migrationSpeed;

            }

        }
        return motilityVector;
    }
}
