using UnityEngine;
using System.Collections.Generic;
using nfs.tools;
using System.Linq;

namespace nfs.nets.layered {

    ///<summary>
    /// Neural network class. This is a fully connected deep layered network
    /// It can have varying number of neurons and layers.
    /// The network always has a single bias input neuron.
    ///</summary>
    public class Network {

        public string Nickname {set; get;}
        public string Id {private set; get;}
		//public Queue Lineage = new Queue();
        public Color Colorisation {set; get;}
		public string[] Ancestors {set; get;}
        // public SerializedNetwork[] SpeciesLineage {set; get;}
        public Stack<SrNetwork> SpeciesLineage = new Stack<SrNetwork>();

		/// <summary>
		/// Gets or sets the fitness score of this neural net.
		/// </summary>
		/// <value>The fitness score.</value>
        public float FitnessScore {set; get; }

        public string[] InputsNames {set; get;}
        public string[] OutputsNames {set; get;}

        ///<summary>
        /// Get the total number of layers including input and output.
        ///</summary>
        public int NumberOfNeuronLayers { get{ return hiddenLayersNeurons.Length + 2; } }
        public int NumberOfSynapseLayers { get{ return hiddenLayersNeurons.Length + 1; } }
        public int NumberOfNeurons {get {return LayersSizes.Sum(); } }
        public int NumberOfSynapses {
            get {
                int synapseNb = 0;
                for(int i = 0; i < NumberOfSynapseLayers; i++) {
                    synapseNb += LayersSizes[i]*LayersSizes[i+1];
                }
                return synapseNb;
            }
        }

        ///<summary>
        /// Get the number of neurons in each layers.
        ///</summary>
        public int[] LayersSizes {
            get {
                int[] layersSizes = new int[NumberOfNeuronLayers];

                int[] hiddenLayersSizes = HiddenLayersSizes;

                for (int i = 0; i < layersSizes.Length; i++) {

                    if (i == 0) {
                        layersSizes[i] = InputSize;
                    } else if (i == NumberOfNeuronLayers-1) {
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

        // layer properties
        private Matrix inputNeurons;
        private Matrix[] hiddenLayersNeurons;
        private Matrix outputNeurons;
        private Matrix[] synapses;


        ///<summary>
        /// Layered neural network constructor.
        /// Requires a given number of input, number given of output
        /// and an array for the hidden layers with each element being the size of a different hidden layer.!--
        ///</summary>
        public Network (int[] layersSizes) {
            ConstructTopology(layersSizes);
        }

		public Network (int[] layersSizes, string id) {
            ConstructTopology(layersSizes);
            Id = id;
        }

        public Network (int[] layersSizes, string id, Color color, string[] inputsNames, string[] outputsNames) {
            ConstructTopology(layersSizes);
            Id = id;
            Colorisation = color;
            InputsNames = inputsNames;
            OutputsNames = outputsNames;
        }

        public Network (SrNetwork srNetwork) {
            ConstructTopology(srNetwork.LayersSizes);
            Nickname = srNetwork.Nickname;
            Id = srNetwork.Id;
            Colorisation = srNetwork.Colorisation;
            Ancestors = srNetwork.Ancestors;
            FitnessScore = srNetwork.FitnessScore;
            InputsNames = srNetwork.InputsNames;
            OutputsNames = srNetwork.OutputsNames;

            // set synapse values
            for(int l = 0; l < srNetwork.SynapseValues.Layers.Length; l++){
                for(int n = 0; n < srNetwork.SynapseValues.Layers[l].Neurons.Length; n++) {
                    synapses[l].SetLineValues(n, srNetwork.SynapseValues.Layers[l].Neurons[n].Synapes);
                }
            }

        }

        private void ConstructTopology(int[] layersSizes) {

            Debug.Assert(layersSizes != null && layersSizes.Length >= 2, "Cannot create a network that has less than 2 layer.");

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

			Network clone = new Network(LayersSizes, Id);
            clone.Colorisation = Colorisation;
			clone.Ancestors = Ancestors;
            clone.SpeciesLineage.Push(Serializer.SerializeNetwork(this));
            clone.InsertSynapses(GetSynapsesClone());
            clone.FitnessScore = FitnessScore;

            return clone;
        }

		/// <summary>
		/// Processes the activation function on a neuron value.
		/// </summary>
		/// <param name="mat">Mat.</param>
        private void ProcessActivation (Matrix mat) {
			// TODO enable chosing a random fitness function
			// a layer cannot have more than one line so we don't loop through the I
            for(int j=0; j < mat.J; j++) {
                //mat.Mtx[0][j] = Sigmoid(mat.Mtx[0][j]);
                //mat.Mtx[0][j] = Linear(mat.Mtx[0][j]);
                //mat.Mtx[0][j] = TanH(mat.Mtx[0][j]);
                mat.SetValue(0, j, TanH(mat.GetValue(0, j)));
            }
        }

		/// <summary>
		/// Tans the h.
		/// </summary>
		/// <returns>The h.</returns>
		/// <param name="t">T.</param>
        private float TanH (float t) {
            return (2f / (1f + Mathf.Exp(-2f*t))) - 1f;
        }

		/// <summary>
		/// Sigmoid the specified t.
		/// </summary>
		/// <param name="t">T.</param>
        private float Sigmoid(float t) {
            return 1f / (1f + Mathf.Exp(-t));
        }

		/// <summary>
		/// Linear the specified t.
		/// </summary>
		/// <param name="t">T.</param>
        private float Linear(float t) {
            return t;
        }

        ///<summary>
        /// Process the inputs forward to get outputs in the network.
        ///</summary>
		public float[] PingFwd(float[] sensorsValues) {

            Debug.Assert(sensorsValues.Length > 0, "Input values are null, returning from ping forward.");
            
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
            Debug.Assert(layer < NumberOfNeuronLayers, "Neuron layer requested is not in the neural net (too high).");

            if (layer == 0){
                return GetInputValues();

            } else if (layer == NumberOfNeuronLayers-1) {
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
            Debug.Assert(layer < NumberOfSynapseLayers , "Synapse layer requested is not in the neural net (too high).");

            return synapses[layer].GetAllValues();
        }

        ///<summary>
        /// Get all synapses values of the network.
        ///</summary>
        public float[][][] GetAllSynapseLayerValues() {
            float[][][] allSynapseValues = new float[synapses.Length][][];

            for (int i = 0; i < synapses.Length; i++) {
                allSynapseValues[i] = synapses[i].GetAllValues();
            }

            return allSynapseValues;
        }

        ///<summary>
        /// Get all out synapses values of a neuron.
        ///</summary>
        public float[] GetNeuronSynapsesValues(int layer, int neuron) {
            Debug.Assert(layer < NumberOfSynapseLayers, "Synapse layer is not in the neural net (too high).");            
            Debug.Assert(neuron < synapses[layer].I, "Neuron requested is not in the layer (too high).");

            return synapses[layer].GetLineValues(neuron);
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
            Debug.Assert(synapses.Length == newSynapses.Length, "The number of synapses matrices to insert does not match the number of this network: " + newSynapses.Length + " vs " + synapses.Length  + ".");

            for (int i = 0; i < synapses.Length; i++) {
                synapses[i].SetAllValues(newSynapses[i]);
            } 
        }

        ///<summary>
        /// Replace the synapse values with new ones from an array of Matrices.
        ///</summary>
        public void InsertSynapses(float[][][] newSynapses) {
            Debug.Assert(synapses.Length == newSynapses.Length, "The number of synapses matrices to insert does not match the number of this network: " + newSynapses.Length + " vs " + synapses.Length  + ".");

            for (int i = 0; i < synapses.Length; i++) {
                synapses[i].SetAllValues(newSynapses[i]);
            }
        }

    }
}
