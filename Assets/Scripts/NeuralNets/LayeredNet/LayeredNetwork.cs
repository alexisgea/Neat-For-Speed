using UnityEngine;
using nfs.tools;

namespace nfs.layered {
    public class LayeredNetwork {

        public float FitnessScore { set; get; }

        private Matrix inputNeurons;
        private Matrix[] hiddenLayersNeurons;
        private Matrix outputNeurons;
        private Matrix[] synapses;

        public LayeredNetwork(int inputLayerSize, int outputLayerSize, int[] hiddenLayersSizes) {
            // each layer is one line of neuron

            // input layer is a matrix of 1 line only 
            inputNeurons = new Matrix(1, inputLayerSize).SetToZero();

            // output layer is a matrix of 1 line only
            outputNeurons = new Matrix(1, outputLayerSize).SetToZero();

            // hidden layer is an array of matrix of one line
            hiddenLayersNeurons = new Matrix[hiddenLayersSizes.Length];
            for (int i = 0; i < hiddenLayersSizes.Length; i++) {
                hiddenLayersNeurons[i] = new Matrix(1, hiddenLayersSizes[i]).SetToZero();
            }

            // synapses are an array of matrix
            // the number of line (or array of synapses sort of) is equal to the previous layer (neurons coming from)
            // the number of column (or nb of synapses in a row) is equal to the next layer (neurons going to)
            synapses = new Matrix[hiddenLayersSizes.Length + 1];
            for (int i = 0; i < synapses.Length; i++) {
                if (i == 0) // input synapses
                    synapses[i] = new Matrix(inputLayerSize, hiddenLayersSizes[i]).SetAsSynapse();
                else if (i == synapses.Length - 1) // synapses to output
                    synapses[i] = new Matrix(hiddenLayersSizes[i - 1], outputLayerSize).SetAsSynapse();
                else // middle synapses
                    synapses[i] = new Matrix(hiddenLayersSizes[i - 1], hiddenLayersSizes[i]).SetAsSynapse();
            }
        }

        public LayeredNetwork GetClone () {
            int hiddenLayers = this.hiddenLayersNeurons.Length;
            int[] hiddenLayerSizes = new int[hiddenLayers];
            for(int i=0; i<hiddenLayers; i++) {
                hiddenLayerSizes[i] = this.hiddenLayersNeurons[i].J;
            }

            LayeredNetwork clone = new LayeredNetwork(this.inputNeurons.J, this.outputNeurons.J, hiddenLayerSizes);

            clone.InsertSynapses(this.GetSynapsesCopy());
            clone.FitnessScore = FitnessScore;

            return clone;
        }

        // this is to pass the activation function (sigmoid here) on the neuron value and on each layer
        // a layer cannot have more than one line so we don't loop through the J
        private void ProcessActivation (Matrix mat) {
            for(int j=0; j < mat.J; j++) {
                //mat.Mtx[0][j] = Sigmoid(mat.Mtx[0][j]);
                //mat.Mtx[0][j] = Linear(mat.Mtx[0][j]);
                mat.Mtx[0][j] = TanH(mat.Mtx[0][j]);
            }
        }

        private float TanH (float t) {
            return (2f / (1f + Mathf.Exp(-2f*t))) - 1f;
        }

        private float Sigmoid(float t) {
            return 1f / (1f + Mathf.Exp(-t));
        }

        private float Linear(float t) {
            return t;
        }

        // process the input forward to get output in the network
		public float[] PingFwd(float[] sensors) {

            inputNeurons.SetLineValues(0, sensors, true); // we ignore the missmatch as there is a bias neuron

            // we ping the network
            for (int i = 0; i < hiddenLayersNeurons.Length + 1; i++) {
                    if (i == 0) {
                        hiddenLayersNeurons[0] = inputNeurons.Multiply(synapses[0]);

                        //Debug.Log("N1:" + hiddenLayersNeurons[0].Mtx[0][0] + " N2:" + hiddenLayersNeurons[0].Mtx[0][1] + " N3:" + hiddenLayersNeurons[0].Mtx[0][2] + " N4:" + hiddenLayersNeurons[0].Mtx[0][3]);

                        ProcessActivation(hiddenLayersNeurons[0]);

                        //Debug.Log("N1:" + hiddenLayersNeurons[0].Mtx[0][0] + " N2:" + hiddenLayersNeurons[0].Mtx[0][1] + " N3:" + hiddenLayersNeurons[0].Mtx[0][2] + " N4:" + hiddenLayersNeurons[0].Mtx[0][3]);
                        
                    } else if (i == hiddenLayersNeurons.Length) {
                        outputNeurons = hiddenLayersNeurons[i - 1].Multiply(synapses[i]);

                        //Debug.Log("N1:" + outputNeurons.Mtx[0][0] + " N2:" + outputNeurons.Mtx[0][1]);
                        
                        ProcessActivation(outputNeurons);
                        
                        //Debug.Log("N1:" + outputNeurons.Mtx[0][0] + " N2:" + outputNeurons.Mtx[0][1]);
                        
                    } else {
                        hiddenLayersNeurons[i] = hiddenLayersNeurons[i - 1].Multiply(synapses[i]);
                        ProcessActivation(hiddenLayersNeurons[i]);
                    }
                }

            return outputNeurons.GetLineValues();
        }

        public int GetInputSize () {
            return this.inputNeurons.J;
        }

        public int GetOutputSize () {
            return this.outputNeurons.J;
        }

        public int[] GetHiddenLayersSizes() {
            int[] hiddenLayerSizes = new int[hiddenLayersNeurons.Length];
            for (int i = 0; i < hiddenLayersNeurons.Length; i++) {
                hiddenLayerSizes[i] = hiddenLayersNeurons[i].J;
            }

            return hiddenLayerSizes;
        }

        public float[] GetOutputValues() {
            return this.outputNeurons.GetLineValues(0);
        }

        public Matrix[] GetSynapsesCopy () {
            Matrix[] synapsesCopy = new Matrix[synapses.Length];
            for (int i = 0; i < synapses.Length; i++) {
                synapsesCopy[i] = synapses[i].GetClone();
            }
            return synapsesCopy;
        }

        public void InsertSynapses(Matrix[] newSynapses) {

            if(synapses.Length == newSynapses.Length){
                for (int i = 0; i < synapses.Length; i++) {
                    synapses[i].SetAllValues(newSynapses[i]);
                }  
            } else {
                Debug.LogWarning("The number of synapses matrices to insert does not match the number of this network: "
                                + newSynapses.Length + " vs " + synapses.Length  + ", doing nothing.");
            }
        }
    }
}
