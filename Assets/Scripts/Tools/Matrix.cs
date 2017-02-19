using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace nfs.tools {

    ///<summary>
	/// Standard math matrix class.
    /// Can be multiplied with another matrix.
    /// Can be set to different predefined matrix.
    /// Can be randomized for synapse initialization.
	///</summary>
    public class Matrix {

        // the matrix itself as an array of array
        public float[][] Mtx { set; get; }
        // the number of row
		public int I { private set; get; }
        // the number of column
		public int J { private set; get; }


        public Matrix(int i, int j) {
            I = i;
            J = j;

            Mtx = new float[i][];
			
			for(int k = 0; k < i; k++) {
                Mtx[k] = new float[j];
            }
        }

		///<summary>
	    /// Standard matrix multiply.
        /// Return a matrix of size I from original and J from other matrix.
        /// Does a weighted sum of original lines times other colums.
        /// IF there is a mismatch of size, the original matrix will be returned.
	    ///</summary>
		public Matrix Multiply (Matrix other) {
            if (J == other.I) { 
				Matrix newMat = new Matrix(I, other.J);

                for (int i = 0; i < I; i++) {
					for(int j = 0; j < other.J; j++) {

                        float weightedSum = 0f;
						for(int k = 0; k < J; k ++) {
                            weightedSum += Mtx[i][k] * other.Mtx[k][j];
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

        ///<summary>
	    /// Set all values to 0.
	    ///</summary>
        public Matrix SetToZero() {
            for (int i = 0; i < I; i++) {
                for(int j = 0; j < J; j++) {
                    Mtx[i][j] = 0f;
                }
            }
            return this;
        }

        ///<summary>
	    /// Set all values to 1.
	    ///</summary>
        public Matrix SetToOne() {
            for (int i = 0; i < I; i++) {
                for(int j = 0; j < J; j++) {
                    Mtx[i][j] = 1f;
                }
            }
            return this;
        }

        ///<summary>
	    /// Set diagonal values to 1 and others to 0.
	    ///</summary>
        public Matrix SetToIdentiy() {
            for (int i = 0; i < I; i++) {
                for(int j = 0; j < J; j++) {
                    if (i==j)
                        Mtx[i][j] = 1f;
                    else
                        Mtx[i][j] = 0f;
                }
            }
            return this;
        }

        ///<summary>
	    /// Randomize all values.
	    ///</summary>
        public Matrix SetAsSynapse() {
            for (int i = 0; i < I; i++) {
                for(int j = 0; j < J; j++) {
                    Mtx[i][j] = Random.Range(0f, 1f);
                }
            }
            return this;
        }

        public float[] GetLineValues(int line = 0) {
            float[] lineValues = new float[line];

            if(line <= I) {
                for(int j=0; j<J; j++){
                    lineValues[j] = Mtx[line][j];
                }
                return lineValues;

            } else {
                Debug.LogError("There is no line " + line + " in this matrix. Returning null.");
                return null;
            }
        }

        public float[] GetColumnValues(int col = 0) {
            float[] colValues = new float[col];

            if(col <= J) {
                for(int i=0; i<I; i++){
                    colValues[i] = Mtx[i][col];
                }
                return colValues;

            } else {
                Debug.LogError("There is no line " + col + " in this matrix. Returning null.");
                return null;
            }
        }

        ///<summary>
	    /// Return all values of the matrix as a string separated by comma and a space.
	    ///</summary>
        public string GetValuesAsString() {
            string values = "";
            for (int i = 0; i < I; i++) {
                for(int j = 0; j < J; j++) {
                    values += Mtx[i][j] + ", ";
                }
            }
            return values.Substring(0, values.Length - 2);
        }
		
	}
}
