using UnityEngine;
using System.Threading.Tasks; 
using System.Linq;
using System.Collections.Generic;

public static class Solvers {

    //LOD: Locally one dimensional
    //For each dimension, the resulting tridiagonal systems are solved with the Thomas algorithm

    public static void DiffusionDecaySolverConstantCoefficientsLOD3D(Microenvironment M, float dt) {

        int xCount = M.mesh.xCoordinates.Count;
        int yCount = M.mesh.yCoordinates.Count;
        int zCount = M.mesh.zCoordinates.Count;


        // define constants and pre-computed quantities 

        if (!M.diffusionSolverSetupDone) {
            Debug.Log("Using implicit 3-D LOD with Thomas Algorithm)");



            //M.thomasDenomY = new List<List<float>>();
            //M.thomasCY = new List<List<float>>();

            //for (int i = 0; i < yCount; i++) {
            //    M.thomasDenomY[i] = new List<float>();
            //    M.thomasCY[i] = new List<float>();
            //    for (int j = 0; j < M.zero.Count; j++) {
            //        M.thomasDenomY[i].Add(0f);
            //        M.thomasCY[i].Add(0f);
            //    }
            //}


            //M.thomasDenomZ = new List<List<float>>();
            //M.thomasCZ = new List<List<float>>();

            //for (int i = 0; i < yCount; i++) {
            //    M.thomasDenomZ[i] = new List<float>();
            //    M.thomasCZ[i] = new List<float>();
            //    for (int j = 0; j < M.zero.Count; j++) {
            //        M.thomasDenomZ[i].Add(0f);
            //        M.thomasCZ[i].Add(0f);
            //    }
            //}


                


            //M.thomasDenomX.Assign(xCount, M.zero);
            //M.thomasCX.Assign(xCount, M.zero);

            //M.thomasDenomY.Assign(yCount, M.zero);
            //M.thomasCY.Assign(yCount, M.zero);

            //M.thomasDenomZ.Assign(zCount, M.zero);
            //M.thomasCZ.Assign(zCount, M.zero);

            M.thomasIJump = 1;
            M.thomasJJump = xCount;
            M.thomasKJump = M.thomasJJump * yCount;

            //M.thomasConstant1 = M.diffusionCoefficients; // dt*D/dx^2 
            //M.thomasConstant1a = M.zero; // -dt*D/dx^2; 
            //M.thomasConstant2 = M.decayRates; // (1/3)* dt*lambda 

            //M.thomasConstant3 = M.one; // 1 + 2*constant1 + constant2; 
            //M.thomasConstant3a = M.one; // 1 + constant1 + constant2;      


            M.thomasConstant1  = new List<float>();   // dt*D/dx^2 
            for (int i = 0; i < M.diffusionCoefficients.Count; i++) 
                M.thomasConstant1.Add(M.diffusionCoefficients[i]);

            M.thomasConstant1a = new List<float>();  // -dt*D/dx^2; 
            for (int i = 0; i < M.zero.Count; i++)
                M.thomasConstant1a.Add(0f);

            M.thomasConstant2 = new List<float>();// (1/3)* dt*lambda 
            for (int i = 0; i < M.decayRates.Count; i++)
                M.thomasConstant2.Add(M.decayRates[i]);

            M.thomasConstant3 = new List<float>(); // 1 + 2*constant1 + constant2; 
            M.thomasConstant3a = new List<float>();// 1 + constant1 + constant2;    
            for (int i = 0; i < M.one.Count; i++) {
                M.thomasConstant3.Add(1f);
                M.thomasConstant3a.Add(1f);
            }
                
    
            //M.thomasConstant1 = M.diffusionCoefficients.ToList(); // dt*D/dx^2 
            //M.thomasConstant1a = M.zero.ToList(); // -dt*D/dx^2; 

            //M.thomasConstant2 = M.decayRates.ToList(); // (1/3)* dt*lambda 

            //M.thomasConstant3 = M.one.ToList(); // 1 + 2*constant1 + constant2; 
            //M.thomasConstant3a = M.one.ToList(); // 1 + constant1 + constant2;      


            for (int i = 0; i < M.thomasConstant1.Count; i++) {// dt*D/dx^2 
                M.thomasConstant1[i] *= dt;
                M.thomasConstant1[i] /= M.mesh.dX;
                M.thomasConstant1[i] /= M.mesh.dX;


                M.thomasConstant1a[i] = -M.thomasConstant1[i];

            }

            //TODO: copy by ref?
            //M.thomasConstant1a = M.thomasConstant1;
            //M.thomasConstant1a = M.thomasConstant1.ToList();

            //for (int i = 0; i < M.thomasConstant1a.Count; i++)
                //M.thomasConstant1a[i] *= -1.0f;


            for (int i = 0; i < M.thomasConstant2.Count; i++) { // (1/3)* dt*lambda 
                M.thomasConstant2[i] *= dt;
                M.thomasConstant2[i] /= 3f; //for the LOD splitting of the source
            }

            for (int i = 0; i < M.thomasConstant3.Count; i++) { // 1 + 2*constant1 + constant2; 
                M.thomasConstant3[i] += M.thomasConstant1[i];
                M.thomasConstant3[i] += M.thomasConstant1[i];
                M.thomasConstant3[i] += M.thomasConstant2[i];
            }

            for (int i = 0; i < M.thomasConstant3a.Count; i++) { // dt*D/dx^2 
                M.thomasConstant3a[i] += M.thomasConstant1[i];
                M.thomasConstant3a[i] += M.thomasConstant2[i];
            }

            // Thomas solver coefficients 



            M.thomasDenomX = new List<List<float>>();
            M.thomasCX = new List<List<float>>();

            for (int i = 0; i < xCount; i++) {
               
                M.thomasCX.Add(new List<float>());
                for (int j = 0; j < M.thomasConstant1a.Count; j++) {                    
                    M.thomasCX[i].Add(M.thomasConstant1a[j]);
                }

                M.thomasDenomX.Add(new List<float>());
                for (int j = 0; j < M.thomasConstant3.Count; j++) {
                    M.thomasDenomX[i].Add(M.thomasConstant3[j]);
                }
                             
            }
            for (int i = 0; i < xCount; i++) {
                for (int j = 0; j < M.thomasConstant3a.Count; j++) {
                    M.thomasDenomX[0][j] = M.thomasConstant3a[j];
                    M.thomasDenomX[xCount - 1][j] = M.thomasConstant3a[j];
                }
            }



            //M.thomasCX.Assign(xCount, M.thomasConstant1a);
            //M.thomasDenomX.Assign(xCount, M.thomasConstant3);

            ////M.thomasDenomX[0] = M.thomasConstant3a;
            ////M.thomasDenomX[xCount - 1] = M.thomasConstant3a;
            //M.thomasDenomX[0] = M.thomasConstant3a.ToList();
            //M.thomasDenomX[xCount - 1] = M.thomasConstant3a.ToList();

            if (xCount == 1) {
                //M.thomasDenomX[0] = M.one;
                //M.thomasDenomX[0] = M.one.ToList(); 

                for (int j = 0; j < M.one.Count; j++) 
                    M.thomasDenomX[0][j] = 1f;


                for (int i = 0; i < M.thomasDenomX[0].Count; i++)
                    M.thomasDenomX[0][i] += M.thomasConstant2[i];
            }
            for (int i = 0; i < M.thomasCX[0].Count; i++)
                M.thomasCX[0][i] /= M.thomasDenomX[0][i];

            for (int i = 1; i <= xCount - 1; i++) {
                for (int j = 0; j < M.thomasDenomX[i].Count; j++) {
                    M.thomasDenomX[i][j] += M.thomasConstant1[j] * M.thomasCX[i - 1][j];
                    M.thomasCX[i][j] /= M.thomasDenomX[i][j]; // the value at  size - 1 is not actually used


                }
            }





            //M.thomasCY.Assign(yCount, M.thomasConstant1a);
            //M.thomasDenomY.Assign(yCount, M.thomasConstant3);
            ////M.thomasDenomY[0] = M.thomasConstant3a;
            //M.thomasDenomY[0] = M.thomasConstant3a.ToList();

            ////M.thomasDenomY[yCount - 1] = M.thomasConstant3a;
            //M.thomasDenomY[yCount - 1] = M.thomasConstant3a.ToList();



            M.thomasDenomY = new List<List<float>>();
            M.thomasCY = new List<List<float>>();

            for (int i = 0; i < yCount; i++) {

                M.thomasCY.Add( new List<float>());

                for (int j = 0; j < M.thomasConstant1a.Count; j++) {
                    M.thomasCY[i].Add(M.thomasConstant1a[j]);
                }

                M.thomasDenomY.Add( new List<float>());
                for (int j = 0; j < M.thomasConstant3.Count; j++) {
                    M.thomasDenomY[i].Add(M.thomasConstant3[j]);
                }

            }
            for (int i = 0; i < yCount; i++) {
                for (int j = 0; j < M.thomasConstant3a.Count; j++) {
                    M.thomasDenomY[0][j] = M.thomasConstant3a[j];
                    M.thomasDenomY[yCount - 1][j] = M.thomasConstant3a[j];
                }
            }



            if (yCount == 1) {
                //M.thomasDenomY[0] = M.one;
                //M.thomasDenomY[0] = M.one.ToList();

                for (int j = 0; j < M.one.Count; j++)
                    M.thomasDenomY[0][j] = 1f;
                
                for (int i = 0; i < M.thomasDenomY[0].Count; i++)
                    M.thomasDenomY[0][i] += M.thomasConstant2[i];
            }
            for (int i = 0; i < M.thomasCY[0].Count; i++)
                M.thomasCY[0][i] /= M.thomasDenomY[0][i];

            for (int i = 1; i <= yCount - 1; i++) {
                for (int j = 0; j < M.thomasDenomY[i].Count; j++) {
                    M.thomasDenomY[i][j] += M.thomasConstant1[j] * M.thomasCY[i - 1][j];
                    M.thomasCY[i][j] /= M.thomasDenomY[i][j]; // the value at  size - 1 is not actually used


                }
            }


            //M.thomasCZ.Assign(zCount, M.thomasConstant1a);
            //M.thomasDenomZ.Assign(zCount, M.thomasConstant3);
            ////M.thomasDenomZ[0]=  M.thomasConstant3a;
            ////M.thomasDenomZ[zCount - 1] = M.thomasConstant3a;
            //M.thomasDenomZ[0] =  M.thomasConstant3a.ToList();
            //M.thomasDenomZ[zCount - 1] = M.thomasConstant3a.ToList();

            M.thomasDenomZ = new List<List<float>>();
            M.thomasCZ = new List<List<float>>();

            for (int i = 0; i < zCount; i++) {

                M.thomasCZ.Add(new List<float>());
                for (int j = 0; j < M.thomasConstant1a.Count; j++) {
                    M.thomasCZ[i].Add(M.thomasConstant1a[j]);
                }

                M.thomasDenomZ.Add(new List<float>());
                for (int j = 0; j < M.thomasConstant3.Count; j++) {
                    M.thomasDenomZ[i].Add(M.thomasConstant3[j]);
                }

               
            }

            for (int i = 0; i < zCount; i++) {
                for (int j = 0; j < M.thomasConstant3a.Count; j++) {
                    M.thomasDenomZ[0][j] = M.thomasConstant3a[j];
                    M.thomasDenomZ[zCount - 1][j] = M.thomasConstant3a[j];
                }
            }


            if (zCount == 1) {
                //M.thomasDenomZ[0] = M.one;
                //M.thomasDenomZ[0] = M.one.ToList();

                for (int j = 0; j < M.one.Count; j++)
                    M.thomasDenomZ[0][j] = 1f;
                
                for (int i = 0; i < M.thomasDenomZ[0].Count; i++)
                    M.thomasDenomZ[0][i] += M.thomasConstant2[i];
            }
            for (int i = 0; i < M.thomasCZ[0].Count; i++)
                M.thomasCZ[0][i] /= M.thomasDenomZ[0][i];

            for (int i = 1; i <= zCount - 1; i++) {
                for (int j = 0; j < M.thomasDenomZ[i].Count; j++) {
                    M.thomasDenomZ[i][j] += M.thomasConstant1[j] * M.thomasCZ[i - 1][j];
                    M.thomasCZ[i][j] /= M.thomasDenomZ[i][j]; // the value at  size - 1 is not actually used


                }
            }

            M.diffusionSolverSetupDone = true;

        }




        // x-diffusion 

                        
        M.ApplyDirichletConditions();


        ////TODO OMP parallel
        //for (int k = 0; k < zCount; k++) {
        //    for (int j = 0; j < yCount; j++) {
        //        // Thomas solver, x-direction

        //        //do for i = 0;
        //        // remaining part of forward elimination, using pre-computed quantities 
        //        int n = M.GetVoxelIndex(0, j, k);
        //        for (int m = 0; m < M.densityVectors[n].Count; m++){
        //            M.densityVectors[n][m] /= M.thomasDenomX[0][m];     
        //        }

        //        if (n == 55)
        //            Debug.Log("just inside solver " + M.densityVectors[55][0]);
        //        for (int i = 1; i < xCount; i++) {
        //            n = M.GetVoxelIndex(i, j, k);

        //            if(n == 55)
        //                Debug.Log("inside solver " + M.densityVectors[55][0]  + " " + M.thomasDenomX[0][0]);
        //            for (int m = 0; m < M.densityVectors[n].Count; m++) {
        //                M.densityVectors[n][m] += M.thomasConstant1[m] * M.densityVectors[n - M.thomasIJump][m];
        //                M.densityVectors[n][m] /= M.thomasDenomX[i][m];
        //            }

                   
        //        }

        //        for (int i = xCount - 2; i >= 0; i--) {
        //            n = M.GetVoxelIndex(i, j, k);
        //            for (int m = 0; m < M.densityVectors[n].Count; m++) {
        //                M.densityVectors[n][m] -= M.thomasCX[i][m] * M.densityVectors[n + M.thomasIJump][m];

        //            }
        //        }

        //    }
        //}



        Parallel.For(0, zCount * yCount, total => {
            int j = total % yCount;
            int k = total / yCount;

            // remaining part of forward elimination, using pre-computed quantities 
            int n = M.GetVoxelIndex(0, j, k);

            for (int m = 0; m < M.densityVectors[n].Count; m++) {
                M.densityVectors[n][m] /= M.thomasDenomX[0][m];
            }


            for (int i = 1; i < xCount; i++) {
                n = M.GetVoxelIndex(i, j, k);

                for (int m = 0; m < M.densityVectors[n].Count; m++) {
                    M.densityVectors[n][m] += M.thomasConstant1[m] * M.densityVectors[n - M.thomasIJump][m];
                    M.densityVectors[n][m] /= M.thomasDenomX[i][m];

                }

            }

            for (int i = xCount - 2; i >= 0; i--) {
                n = M.GetVoxelIndex(i, j, k);
                for (int m = 0; m < M.densityVectors[n].Count; m++) {
                    M.densityVectors[n][m] -= M.thomasCX[i][m] * M.densityVectors[n + M.thomasIJump][m];

                }
            }

        });


        // y-diffusion 
        M.ApplyDirichletConditions();


        //TODO: openmp
        //for (int k = 0; k < zCount; k++) {
        //    for (int i = 0; i < xCount; i++) {
        //        // Thomas solver, y-direction

        //        // remaining part of forward elimination, using pre-computed quantities 
        //        int n = M.GetVoxelIndex(i, 0, k);

        //        for (int m = 0; m < M.densityVectors[n].Count; m++) {
        //            M.densityVectors[n][m] /= M.thomasDenomY[0][m];
        //        }


        //        for (int j = 1; j < yCount; j++) {
        //            n = M.GetVoxelIndex(i, j, k);


        //            for (int m = 0; m < M.densityVectors[n].Count; m++) {
        //                M.densityVectors[n][m] += M.thomasConstant1[m] * M.densityVectors[n - M.thomasJJump][m];
        //                M.densityVectors[n][m] /= M.thomasDenomY[i][m];
        //            }

        //        }

        //        // back substitution 
        //        // n = voxel_index( mesh.x_coordinates.size()-2 ,j,k); 

        //        for (int j = yCount - 2; j >= 0; j--) {
        //            n = M.GetVoxelIndex(i, j, k);
        //            for (int m = 0; m < M.densityVectors[n].Count; m++) {
        //                M.densityVectors[n][m] -= M.thomasCY[j][m] * M.densityVectors[n + M.thomasJJump][m];

        //            }
        //        }

        //    }
        //}



        Parallel.For(0, zCount * xCount, total => {
            int i = total % xCount;
            int k = total / xCount;

            // Thomas solver, y-direction

            // remaining part of forward elimination, using pre-computed quantities 
            int n = M.GetVoxelIndex(i, 0, k);

            for (int m = 0; m < M.densityVectors[n].Count; m++) {
                M.densityVectors[n][m] /= M.thomasDenomY[0][m];
            }


            for (int j = 1; j < yCount; j++) {
                n = M.GetVoxelIndex(i, j, k);


                for (int m = 0; m < M.densityVectors[n].Count; m++) {
                    M.densityVectors[n][m] += M.thomasConstant1[m] * M.densityVectors[n - M.thomasJJump][m];
                    M.densityVectors[n][m] /= M.thomasDenomY[i][m];
                }

            }

            // back substitution 
            // n = voxel_index( mesh.x_coordinates.size()-2 ,j,k); 

            for (int j = yCount - 2; j >= 0; j--) {
                n = M.GetVoxelIndex(i, j, k);
                for (int m = 0; m < M.densityVectors[n].Count; m++) {
                    M.densityVectors[n][m] -= M.thomasCY[j][m] * M.densityVectors[n + M.thomasJJump][m];

                }
            }

        });
                 

        // z-diffusion 

        M.ApplyDirichletConditions();
        ////TODO: openmp
        //for (int j = 0; j < yCount; j++) {
        //    for (int i = 0; i < xCount; i++) {
        //        // Thomas solver, y-direction

        //        // remaining part of forward elimination, using pre-computed quantities 
        //        int n = M.GetVoxelIndex(i, j, 0);

        //        for (int m = 0; m < M.densityVectors[n].Count; m++) {
        //            M.densityVectors[n][m] /= M.thomasDenomZ[0][m];
        //        }

        //        // should be an empty loop if mesh.z_coordinates.size() < 2  
        //        for (int k = 1; k < zCount; k++) {
        //            n = M.GetVoxelIndex(i, j, k);


        //            for (int m = 0; m < M.densityVectors[n].Count; m++) {
        //                M.densityVectors[n][m] += M.thomasConstant1[m] * M.densityVectors[n - M.thomasKJump][m];
        //                M.densityVectors[n][m] /= M.thomasDenomZ[k][m];
        //            }

        //        }

        //        // back substitution 

        //        // should be an empty loop if mesh.z_coordinates.size() < 2 
        //        for (int k = zCount - 2; k >= 0; k--) {
        //            n = M.GetVoxelIndex(i, j, k);
        //            for (int m = 0; m < M.densityVectors[n].Count; m++) {
        //                M.densityVectors[n][m] -= M.thomasCZ[j][m] * M.densityVectors[n + M.thomasKJump][m];

        //            }
        //        }

        //    }
        //}


        Parallel.For(0, zCount * xCount, total => {
            int i = total % xCount;
            int j = total / xCount;

            // Thomas solver, y-direction

            // remaining part of forward elimination, using pre-computed quantities 
            int n = M.GetVoxelIndex(i, j, 0);

            for (int m = 0; m < M.densityVectors[n].Count; m++) {
                M.densityVectors[n][m] /= M.thomasDenomZ[0][m];
            }

            // should be an empty loop if mesh.z_coordinates.size() < 2  
            for (int k = 1; k < zCount; k++) {
                n = M.GetVoxelIndex(i, j, k);


                for (int m = 0; m < M.densityVectors[n].Count; m++) {
                    M.densityVectors[n][m] += M.thomasConstant1[m] * M.densityVectors[n - M.thomasKJump][m];
                    M.densityVectors[n][m] /= M.thomasDenomZ[k][m];
                }

            }

            // back substitution 

            // should be an empty loop if mesh.z_coordinates.size() < 2 
            for (int k = zCount - 2; k >= 0; k--) {
                n = M.GetVoxelIndex(i, j, k);
                for (int m = 0; m < M.densityVectors[n].Count; m++) {
                    M.densityVectors[n][m] -= M.thomasCZ[j][m] * M.densityVectors[n + M.thomasKJump][m];

                }
            }

        });

        M.ApplyDirichletConditions();
        // reset gradient vectors 
        //  M.reset_all_gradient_vectors(); 

        return;
    }



}
