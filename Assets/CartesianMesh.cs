using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Voxel {

    public int meshIndex;
    public float volume;
    public Vector3 center;
    public bool isDirichlet;

    public Voxel() {

        meshIndex = 0;
        volume = 10 * 10 * 10;
        center = Vector3.zero;
        isDirichlet = false;

    }
}


public class VoxelFace {
    public int meshIndex;
    public float surfaceArea;
    public Vector3 center;
    public Vector3 inwardNormal;
    public Vector3 outwardNormal;

    public VoxelFace() {
        meshIndex = 0;
        surfaceArea = 10 * 10;
        center = Vector3.zero;
        inwardNormal = Vector3.zero;
        outwardNormal = Vector3.zero;

    }
}

[System.Serializable]
public class CartesianMesh {


    public List<float> xCoordinates = new List<float>();
    public List<float> yCoordinates = new List<float>();
    public List<float> zCoordinates = new List<float>();

    // [xmin ymin zmin xmax ymax zmax ]
    public float[] boundingBox = new float[6];

    public float dX, dY, dZ;
    float dV, dS;
    float dSXY, dSXZ, dSYZ;

    public List<Voxel> voxels = new List<Voxel>();
    public List<VoxelFace> voxelFaces = new List<VoxelFace>();

    public List<List<int>> connectedVoxelIndices = new List<List<int>>();
    public List<List<int>> mooreConnectedVoxelIndices = new List<List<int>>(); // Keeps the list of voxels in the Moore nighborhood 


    public bool uniformMesh;
    public bool regularMesh;
    public bool useVoxelFaces;

    public string units;

    public CartesianMesh() {
        units = "none";
        uniformMesh = true;
        regularMesh = true;
        useVoxelFaces = false;

        xCoordinates.Assign(1, 0f);
        yCoordinates.Assign(1, 0f);
        zCoordinates.Assign(1, 0f);

        dX = boundingBox[3] - boundingBox[0];
        dY = boundingBox[3] - boundingBox[0];
        dZ = boundingBox[3] - boundingBox[0];

        dV = dX * dY * dZ;
        dS = dX * dY;
        dSXY = dX * dY;
        dSXZ = dX * dZ;
        dSYZ = dY * dZ;

        for (int i = 0; i < (xCoordinates.Count * yCoordinates.Count * zCoordinates.Count); i++) {
            voxels.Add(new Voxel());
            voxels[i].volume = dV;
        }


        //Voxel templateVoxel = new Voxel();
        //templateVoxel.volume = dV;

        //voxels.Assign(xCoordinates.Count * yCoordinates.Count * zCoordinates.Count, templateVoxel);

        voxels[0].center.x = xCoordinates[0];
        voxels[0].center.y = yCoordinates[0];
        voxels[0].center.z = zCoordinates[0];



    }

    public CartesianMesh(int xNodes, int yNodes, int zNodes) {

        units = "none";

        uniformMesh = true;
        regularMesh = true;
        useVoxelFaces = false;

        dX = 1;
        dY = 1;
        dZ = 1;

        dV = dX * dY * dZ;
        dS = dX * dY;
        dSXY = dX * dY;
        dSXZ = dX * dZ;
        dSYZ = dY * dZ;

        xCoordinates.Assign(xNodes, 0f);
        yCoordinates.Assign(yNodes, 0f);
        zCoordinates.Assign(zNodes, 0f);

        for (int i = 0; i < xCoordinates.Count; i++)
            xCoordinates[i] = i * dX;
        for (int i = 0; i < yCoordinates.Count; i++)
            yCoordinates[i] = i * dY;
        for (int i = 0; i < yCoordinates.Count; i++)
            zCoordinates[i] = i * dZ;


        boundingBox[0] = xCoordinates[0] - dX / 2f;
        boundingBox[3] = xCoordinates[xCoordinates.Count - 1] + dX / 2f;
        boundingBox[1] = yCoordinates[0] - dY / 2f;
        boundingBox[4] = yCoordinates[yCoordinates.Count - 1] + dY / 2f;
        boundingBox[2] = zCoordinates[0] - dZ / 2f;
        boundingBox[5] = zCoordinates[zCoordinates.Count - 1] + dZ / 2f;



        for (int i = 0; i < (xCoordinates.Count * yCoordinates.Count * zCoordinates.Count); i++){
            voxels.Add(new Voxel());
            voxels[i].volume = dV;
        }



          //  Voxel templateVoxel = new Voxel();
        //templateVoxel.volume = dV;

        //voxels.Assign(xCoordinates.Count * yCoordinates.Count * zCoordinates.Count, templateVoxel);

        //initializing and connecting voxels
        int n = 0;
        for (int i = 0; i < xCoordinates.Count; i++) {
            for (int j = 0; j < yCoordinates.Count; j++) {
                for (int k = 0; k < zCoordinates.Count; k++) {
                    voxels[n].center.x = xCoordinates[i];
                    voxels[n].center.y = xCoordinates[j];
                    voxels[n].center.z = xCoordinates[k];
                    voxels[n].meshIndex = n;
                    voxels[n].volume = dV;
                    n++;
                }
            }
        }

        //make connections
        connectedVoxelIndices.Resize(voxels.Count);

        int iJump = 1;
        int jJump = xCoordinates.Count;
        int kJump = xCoordinates.Count * yCoordinates.Count;


        // x-aligned connections 
        for (int k = 0; k < zCoordinates.Count; k++) {
            for (int j = 0; j < yCoordinates.Count; j++) {
                for (int i = 0; i < xCoordinates.Count - 1; i++) {
                    n = GetVoxelIndex(i, j, k);
                    ConnectVoxelsIndicesOnly(n, n + iJump, dSYZ);
                }
            }
        }
        // y-aligned connections 
        for (int k = 0; k < zCoordinates.Count; k++) {
            for (int i = 0; i < xCoordinates.Count; i++) {
                for (int j = 0; j < yCoordinates.Count - 1; j++) {
                    n = GetVoxelIndex(i, j, k);
                    ConnectVoxelsIndicesOnly(n, n + jJump, dSXZ);
                }
            }
        }
        // z-aligned connections 
        for (int j = 0; j < yCoordinates.Count; j++) {
            for (int i = 0; i < xCoordinates.Count; i++) {
                for (int k = 0; k < zCoordinates.Count - 1; k++) {
                    n = GetVoxelIndex(i, j, k);
                    ConnectVoxelsIndicesOnly(n, n + kJump, dSXY);
                }
            }
        }
    }

    public bool IsPositionValid(Vector3 pos) {
        if (pos.x < boundingBox[Globals.meshMinXIndex] || pos.x > boundingBox[Globals.meshMaxXIndex])
            return false;
        if (pos.y < boundingBox[Globals.meshMinYIndex] || pos.y > boundingBox[Globals.meshMaxYIndex])
            return false;
        if (pos.z < boundingBox[Globals.meshMinZIndex] || pos.z > boundingBox[Globals.meshMaxZIndex])
            return false;

        return true;
    }

    public int GetVoxelIndex(int i, int j, int k) {
        return (k * yCoordinates.Count + j) * xCoordinates.Count + i;
    }

    public void ConnectVoxelsIndicesOnly(int i, int j, float sA) {
        connectedVoxelIndices[i].Add(j);
        connectedVoxelIndices[j].Add(i);
    }

    public void ConnectVoxelsFacesOnly(int i, int j, float sA) {
        // create a new Voxel_Face connecting i to j

        VoxelFace vF1 = new VoxelFace();
        int k = voxelFaces.Count;
        vF1.meshIndex = k;
        vF1.surfaceArea = sA;
        vF1.outwardNormal = voxels[j].center - voxels[i].center;
        vF1.outwardNormal = Vector3.Normalize(vF1.outwardNormal);
        vF1.inwardNormal = vF1.outwardNormal;
        vF1.inwardNormal *= -1.0f;

        // convention: face is oriented from lower index to higher index 
        if (j < i) {
            vF1.outwardNormal *= -1.0f;
            vF1.inwardNormal *= -1.0f;
        }

        // add it to the vector of voxel faces         
        voxelFaces.Add(vF1);
    }

    public int[] GetCartesianIndices(int n) {
        int xCount = xCoordinates.Count;
        int yCount = yCoordinates.Count;

        int[] outVec = new int[3];
        int xy = xCount * yCount;

        outVec[2] = Mathf.FloorToInt(n / xy);
        outVec[1] = Mathf.FloorToInt((n - outVec[2] * xy) / xCount);
        outVec[0] = n - xCount * (outVec[1] + yCount * outVec[2]);

        return outVec;

    }

    public void CreateMooreNeighborhood() {

        mooreConnectedVoxelIndices.Resize(voxels.Count);


        for (int j = 0; j < yCoordinates.Count; j++) {
            for (int i = 0; i < xCoordinates.Count; i++) {
                for (int k = 0; k < zCoordinates.Count; k++) {
                    int centerIndex = GetVoxelIndex(i, j, k);

                    for (int ii = -1; ii <= 1; ii++)
                        for (int jj = -1; jj <= 1; jj++)
                            for (int kk = -1; kk <= 1; kk++)
                                if (i + ii >= 0 && i + ii < xCoordinates.Count &&
                                    j + jj >= 0 && j + jj < yCoordinates.Count &&
                                    k + kk >= 0 && k + kk < zCoordinates.Count &&
                                    !(ii == 0 && jj == 0 && kk == 0)) {
                                    int neighborIndex = GetVoxelIndex(i + ii, j + jj, k + kk);
                                    mooreConnectedVoxelIndices[centerIndex].Add(neighborIndex);
                                }
                }
            }
        }
    }

    void CreateVoxelFaces() {

        for (int i = 0; i < voxels.Count; i++)
            connectedVoxelIndices.Add(new List<int>());

        int iJump = 1;
        int jJump = xCoordinates.Count;
        int kJump = xCoordinates.Count * yCoordinates.Count;

        int n;
        // x-aligned connections 
        for (int k = 0; k < zCoordinates.Count; k++) {
            for (int j = 0; j < yCoordinates.Count; j++) {
                for (int i = 0; i < xCoordinates.Count - 1; i++) {
                    n = GetVoxelIndex(i, j, k);
                    ConnectVoxelsFacesOnly(n, n + iJump, dSYZ);
                }
            }
        }
        // y-aligned connections 
        for (int k = 0; k < zCoordinates.Count; k++) {
            for (int i = 0; i < xCoordinates.Count; i++) {
                for (int j = 0; j < yCoordinates.Count - 1; j++) {
                    n = GetVoxelIndex(i, j, k);
                    ConnectVoxelsFacesOnly(n, n + jJump, dSXZ);
                }
            }
        }
        // z-aligned connections 
        for (int j = 0; j < yCoordinates.Count; j++) {
            for (int i = 0; i < xCoordinates.Count; i++) {
                for (int k = 0; k < zCoordinates.Count - 1; k++) {
                    n = GetVoxelIndex(i, j, k);
                    ConnectVoxelsFacesOnly(n, n + kJump, dSXY);
                }
            }
        }
    }



    public void Resize(float xStart, float xEnd, float yStart, float yEnd, float zStart, float zEnd, int xNodes, int yNodes, int zNodes) {

        xCoordinates.Assign(xNodes, 0f);
        yCoordinates.Assign(yNodes, 0f);
        zCoordinates.Assign(zNodes, 0f);


        dX = (xEnd - xStart) / ((float)xNodes);
        if (xNodes < 2) { dX = 1; }
        dY = (yEnd - yStart) / ((float)yNodes);
        if (yNodes < 2) { dY = 1; }
        dZ = (zEnd - zStart) / ((float)zNodes);
        if (zNodes < 2) { dZ = 1; }


        for (int i = 0; i < xCoordinates.Count; i++)
            xCoordinates[i] = xStart + (i + 0.5f) * dX;
        for (int i = 0; i < yCoordinates.Count; i++)
            yCoordinates[i] = yStart + (i + 0.5f) * dY;
        for (int i = 0; i < zCoordinates.Count; i++)
            zCoordinates[i] = zStart + (i + 0.5f) * dZ;

        boundingBox[0] = xStart;
        boundingBox[3] = xEnd;
        boundingBox[1] = yStart;
        boundingBox[4] = yEnd;
        boundingBox[2] = zStart;
        boundingBox[5] = zEnd;

        dV = dX * dY * dZ;
        dS = dX * dY;

        dSXY = dX * dY;
        dSYZ = dY * dZ;
        dSXZ = dX * dZ;

        //Voxel templateVoxel = new Voxel();
        //templateVoxel.volume = dV;

        //voxels.Assign(xCoordinates.Count * yCoordinates.Count * zCoordinates.Count, templateVoxel);

        for (int i = 0; i < (xCoordinates.Count * yCoordinates.Count * zCoordinates.Count); i++) {
            voxels.Add(new Voxel());
            voxels[i].volume = dV;
        }


        int n = 0;
        for (int k = 0; k < zCoordinates.Count; k++) {
            for (int j = 0; j < yCoordinates.Count; j++) {
                for (int i = 0; i < xCoordinates.Count; i++) {
                    voxels[n].center[0] = xCoordinates[i];
                    voxels[n].center[1] = yCoordinates[j];
                    voxels[n].center[2] = zCoordinates[k];
                    voxels[n].meshIndex = n;
                    voxels[n].volume = dV;

                    Debug.Log(n + " " + voxels[n].center[0] + " " + voxels[n].center[1] + " " + voxels[n].center[2]);

               
                    n++;
                }
            }
        }


                                                                                       
        Debug.Log("------------------------------------");
        n = 0;
        for (int k = 0; k < zCoordinates.Count; k++) {
            for (int j = 0; j < yCoordinates.Count; j++) {
                for (int i = 0; i < xCoordinates.Count; i++) {
                    Debug.Log(n + " " + voxels[n].center[0] + " " + voxels[n].center[1] + " " + voxels[n].center[2]);

                    n++;
                }
            }
        }



        // make connections 
        connectedVoxelIndices.Resize(voxels.Count);


  

        voxelFaces.Clear();



    

        for (int i = 0; i < connectedVoxelIndices.Count; i++) {
            if (connectedVoxelIndices[i] != null)
                connectedVoxelIndices[i].Clear();
        }

        int i_jump = 1;
        int j_jump = xCoordinates.Count;
        int k_jump = xCoordinates.Count * yCoordinates.Count;

        // x-aligned connections 
        int count = 0;
        for (int k = 0; k < zCoordinates.Count; k++) {
            for (int j = 0; j < yCoordinates.Count; j++) {
                for (int i = 0; i < xCoordinates.Count - 1; i++) {
                    n = GetVoxelIndex(i, j, k);
                    ConnectVoxelsIndicesOnly(n, n + i_jump, dSYZ);
                    count++;
                }
            }
        }
        // y-aligned connections 
        for (int k = 0; k < zCoordinates.Count; k++) {
            for (int i = 0; i < xCoordinates.Count; i++) {
                for (int j = 0; j < yCoordinates.Count - 1; j++) {
                    n = GetVoxelIndex(i, j, k);
                    ConnectVoxelsIndicesOnly(n, n + j_jump, dSXZ);
                }
            }
        }
        // z-aligned connections 
        for (int j = 0; j < yCoordinates.Count; j++) {
            for (int i = 0; i < xCoordinates.Count; i++) {
                for (int k = 0; k < zCoordinates.Count - 1; k++) {
                    n = GetVoxelIndex(i, j, k);
                    ConnectVoxelsIndicesOnly(n, n + k_jump, dSXY);
                }
            }
        }


   

        if (useVoxelFaces)
            CreateVoxelFaces();

        CreateMooreNeighborhood();





        return;
    }


    public void Resize(float xStart, float xEnd, float yStart, float yEnd, float zStart, float zEnd, float dXNew, float dYNew, float dZNew) {

        dX = dXNew;
        dY = dYNew;
        dZ = dZNew;
        float eps = 1e-16f;

        int xNodes = (int)Mathf.Ceil(eps + (xEnd - xStart) / dX);
        int yNodes = (int)Mathf.Ceil(eps + (yEnd - yStart) / dY);
        int zNodes = (int)Mathf.Ceil(eps + (zEnd - zStart) / dZ);


        Resize(xStart, xEnd, yStart, yEnd, zStart, zEnd, xNodes, yNodes, zNodes);
    }

    public void Resize(int xNodes, int yNodes, int zNodes) {
        Resize(-0.5f, xNodes - 0.5f, -0.5f, yNodes - 0.5f, -0.5f, zNodes - 0.5f, xNodes, yNodes, zNodes);
    }

    public void ResizeUniform(float xStart, float xEnd, float yStart, float yEnd, float zStart, float zEnd, float dXNew) {
        Resize(xStart, xEnd, yStart, yEnd, zStart, zEnd, dXNew, dXNew, dXNew);

    }

    public int[] GetNearestCartesianIndices(Vector3 pos) {
        int xCount = xCoordinates.Count;
        int yCount = yCoordinates.Count;
        int zCount = zCoordinates.Count;

        int[] outVec = new int[3];

        outVec[0] = Mathf.FloorToInt((pos.x - boundingBox[0]) / dX);
        outVec[1] = Mathf.FloorToInt((pos.y - boundingBox[1]) / dY);
        outVec[2] = Mathf.FloorToInt((pos.z - boundingBox[2]) / dZ);


        if (outVec[0] >= xCount)
            outVec[0] = xCount - 1;
        if (outVec[0] < 0)
            outVec[0] = 0;

        if (outVec[1] >= yCount)
            outVec[1] = yCount - 1;
        if (outVec[1] < 0)
            outVec[1] = 0;

        if (outVec[2] >= zCount)
            outVec[2] = zCount - 1;
        if (outVec[2] < 0)
            outVec[2] = 0;


        return outVec;

    }

    public int GetNearestVoxelIndex(Vector3 pos) {
        int xCount = xCoordinates.Count;
        int yCount = yCoordinates.Count;
        int zCount = zCoordinates.Count;

        int i = Mathf.FloorToInt((pos.x - boundingBox[0]) / dX);
        int j = Mathf.FloorToInt((pos.y - boundingBox[1]) / dY);
        int k = Mathf.FloorToInt((pos.z - boundingBox[2]) / dZ);

        if (i >= xCount)
            i = xCount - 1;
        if (i < 0)
            i = 0;

        if (j >= yCount)
            j = yCount - 1;
        if (j < 0)
            j = 0;

        if (k >= zCount)
            k = zCount - 1;
        if (k < 0)
            k = 0;

        return (k * yCount + j) * xCount + i;
    }

    public Voxel GetNearestVoxel(Vector3 pos) {
        return voxels[GetNearestVoxelIndex(pos)];
    }


}
