using UnityEngine;
using nfs.tools;

namespace nfs.nets.layered {

    ///<summary>
    /// Neural network class. This is a fully connected deep layered network
    /// It can have varying number of neurons and layers.
    /// The network always has a single bias input neuron.
    ///</summary>
    public class Network {

        public float Id {set; get; }

        //public Queue[] Lineage = new Queue();

        public float FitnessScore { set; get; }

        // layer properties
        private Matrix inputNeurons;
        private Matrix[] hiddenLayersNeurons;
        private Matrix outputNeurons;
        private Matrix[] synapses;

        ///<summary>
        /// Get the total number of layers including input and output.
        ///</summary>
        public int NumberOfLayers { get{ return hiddenLayersNeurons.Length + 2; } }
        
        ///<summary>
        /// Get the number of neurons in each layers.
        ///</summary>
        public int[] LayersSizes {
            get {
                int[] layersSizes = new int[NumberOfLayers];

                int[] hiddenLayersSizes = HiddenLayersSizes;

                for (int i = 0; i < layersSizes.Length; i++) {

                    if (i == 0) {
                        layersSizes[i] = InputSize;
                    } else if (i == NumberOfLayers-1) {
                        layersSizes[i] = OutputSize;
                    } else {
                        layersSizes[i] = hiddenLayersSizes[i-1];
                    }
                }

                return layersSizes;
            }
        }

        ///<summary>
        /// Get the number of input neurons.
        ///</summary>
        public int InputSize { get {return this.inputNeurons.J;} }

        ///<summary>
        /// Get the number of outpu neurons.
        ///</summary>
        public int OutputSize{ get{ return this.outputNeurons.J;} }

        ///<summary>
        /// Get the number of neurons in each hidden layers.
        ///</summary>
        public int[] HiddenLayersSizes {
            get {
                int[] hiddenLayersSizes = new int[hiddenLayersNeurons.Length];
                for (int i = 0; i < hiddenLayersNeurons.Length; i++) {
                    hiddenLayersSizes[i] = hiddenLayersNeurons[i].J;
                }

                return hiddenLayersSizes;
            }
        }

        ///<summary>
        /// Layered neural network constructor.
        /// Requires a given number of input, number given of output
        /// and an array for the hidden layers with each element being the size of a different hidden layer.!--
        ///</summary>
        public Network(int[] layersSizes) {

            if(layersSizes == null || layersSizes.Length < 2) {
                Debug.LogError("Cannot create a network that has less than 2 layer.");
                return;
            }

            Id = Time.time;
            // each layer is one line of neuron
            inputNeurons = new Matrix(1, layersSizes[0]).SetToOne();
            outputNeurons = new Matrix(1, layersSizes[layersSizes.Length-1]).SetToOne();

            // hidden layer is an array of matrix of one line
            hiddenLayersNeurons = new Matrix[layersSizes.Length-2];
            for (int i = 0; i < hiddenLayersNeurons.Length; i++) {
                hiddenLayersNeurons[i] = new Matrix(1, layersSizes[i+1]).SetToOne();
            }

            // synapses are an array of matrix
            // the number of line (or array of synapses sort of) is equal to the previous layer (neurons coming from)
            // the number of column (or nb of synapses in a row) is equal to the next layer (neurons going to)
            synapses = new Matrix[layersSizes.Length-1];
            for (int i = 0; i < synapses.Length; i++) {
                synapses[i] = new Matrix(layersSizes[i], layersSizes[i+1]).SetAsSynapse();
            }
        }

        ///<summary>
        /// Creates and return a deep clone of the network.
        ///</summary>
        public Network GetClone () {

            Network clone = new Network(this.LayersSizes);
            clone.InsertSynapses(this.GetSynapsesClone());
            clone.FitnessScore = this.FitnessScore;

            return clone;
        }

        // this is to pass the activation function on the neuron value and on each layer
        // a layer cannot have more than one line so we don't loop through the I
        private void ProcessActivation (Matrix mat) {
            for(int j=0; j < mat.J; j++) {
                //mat.Mtx[0][j] = Sigmoid(mat.Mtx[0][j]);
                //mat.Mtx[0][j] = Linear(mat.Mtx[0][j]);
                //mat.Mtx[0][j] = TanH(mat.Mtx[0][j]);
                mat.SetValue(0, j, TanH(mat.GetValue(0, j)));
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

        ///<summary>
        /// Process the inputs forward to get outputs in the network.
        ///</summary>
		public float[] PingFwd(float[] sensorsValues) {
            
            // we set the inputs neurons values and ignore the missmatch as there is a bias neuron
            inputNeurons.SetLineValues(0, sensorsValues, true); 

            // we ping the network
            for (int i = 0; i < hiddenLayersNeurons.Length + 1; i++) {
                    if (i == 0) {
                        hiddenLayersNeurons[0] = Matrix.Multiply(inputNeurons, synapses[0]);
                        ProcessActivation(hiddenLayersNeurons[0]);
                    } else if (i == hiddenLayersNeurons.Length) {
                        outputNeurons = Matrix.Multiply(hiddenLayersNeurons[i - 1], synapses[i]);
                        ProcessActivation(outputNeurons);
                        
                    } else {
                        hiddenLayersNeurons[i] = Matrix.Multiply(hiddenLayersNeurons[i - 1], synapses[i]);
                        ProcessActivation(hiddenLayersNeurons[i]);
                    }
                }

            return outputNeurons.GetLineValues();
        }

        ///<summary>
        /// Get all values of a layer of neurons.
        ///</summary>
        public float[] GetNeuronLayerValues(int layer) {
            if(layer >= NumberOfLayers) {
                Debug.LogError("Neuron layer requested is not in the neural net (too high). Returning empty array.");
                return new float[] {};

            } else if (layer == 0){
                return GetInputValues();

            } else if (layer == NumberOfLayers-1) {
                return GetOutputValues();

            } else {
                return hiddenLayersNeurons[layer-1].GetLineValues();
            }
        }

        ///<summary>
        /// Get all output values.
        ///</summary>
        public float[] GetOutputValues() {
            return this.outputNeurons.GetLineValues(0);
        }

        ///<summary>
        /// Get all input values.
        ///</summary>
        public float[] GetInputValues() {
            return this.inputNeurons.GetLineValues(0);
        }

        ///<summary>
        /// Get a specific neuron's value.
        ///</summary>
        public float GetNeuronValue(int layer, int neuron) {
            return  GetNeuronLayerValues(layer)[neuron];

        }

        ///<summary>
        /// Get all out synapses values of a layer of neurons.
        ///</summary>
        public float[][] GetSynapseLayerValues(int layer) {
            if(layer >= NumberOfLayers-1) {
                Debug.LogError("Synapse layer requested is not in the neural net (too high). Returning emtpy array of array.");
                return new float[][]{}; // is it possible to create an array of lenght 0?

            } else {
                return synapses[layer].GetAllValues();
            }
        }

        ///<summary>
        /// Get all out synapses values of a neuron.
        ///</summary>
        public float[] GetNeuronSynapsesValues(int layer, int neuron) {
            if(layer >= NumberOfLayers-1 || neuron >= synapses[layer].J) {
                Debug.LogError("Synapse layer or neuron requested is not in the neural net (too high). Returning empty array.");
                return new float[]{};

            } else {
                return synapses[layer].GetLineValues(neuron);
            }
        }

        ///<summary>
        /// Get a specific synapse value from a synapses layer or neuron layer (for out synapse of said neuron).
        ///</summary>
        public float GetSynapseValue (int layer, int neuron, int synapse) {
            return synapses[layer].GetValue(neuron, synapse);
        }

        ///<summary>
        /// Get a deep clone of all the synapses as an array of Matrices.
        ///</summary>
        public Matrix[] GetSynapsesClone () {
            Matrix[] synapsesCopy = new Matrix[synapses.Length];
            for (int i = 0; i < synapses.Length; i++) {
                synapsesCopy[i] = synapses[i].GetClone();
            }
            return synapsesCopy;
        }

        ///<summary>
        /// Replace the synapse values with new ones from an array of Matrices.
        ///</summary>
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
