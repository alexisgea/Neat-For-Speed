using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace airace {

    public class Matrix {

        public float[][] Mtx { set; get; }
		public int I { private set; get; }
		public int J { private set; get; }

        public Matrix(int i, int j) {
            I = i;
            J = j;

            Mtx = new float[i][];
			
			for(int k = 0; k < i; k++) {
                Mtx[k] = new float[j];
            }
        }
		
		public Matrix Multiply (Matrix other) {
            if (J == other.I) { 
				Matrix newMat = new Matrix(I, other.J);

                for (int i = 0; i < I; i++) {
					for(int j = 0; j < other.J; j++) {

                        float weightedSum = 0f;
						for(int k = 0; k < J; k ++) {
                            weightedSum += Mtx[k][i] * other.Mtx[j][k];
                        }
                        newMat.Mtx[i][j] = weightedSum;
                    }
				}
                return newMat;

            } else {
                Debug.LogWarning("Matrix multiplication error due to size missmatch. Base matrix returned.");
                return this;
			}
        }
		

	}
}
