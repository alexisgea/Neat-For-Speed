using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using nfs.tools;
using nfs.car;

namespace nfs.layered {

    // NOTES TO MYSELF
    // potentially there could be one neural network piloting every car of the same type in the same time
    // we could gather all input in one matrix and use matrix multiplication to compute all the output in the same time
    // I guess the issue is that it implies they all have the same weights and thus probably the same exact output
    // although this mean we can probably run simultaneous race with one neural network driving multiple cars
    // even if the race track is different, thus we could maybe train it faster. We'll have to see.
    // NOTES 2
    // Make a seperate neural net class from the controller class
    // Make it so it has the same basic as a Neat
    // Make it so a visualiser would work in any case
    [RequireComponent(typeof(CarSensors))]
    public class VariableLayeredNet : CarController {

        private LayeredNetwork neuralNet;
        private CarSensors sensors;
        private float[] inputValues;
        private float[] outputValues;


        protected override void ChildStart() {
            sensors = GetComponent<CarSensors>();
            InitializeNeuralNetwork();
        }

        protected override void ChildUpdate() {
            inputValues = new float[] {sensors.Wall_NE, sensors.Wall_N, sensors.Wall_NW};
            outputValues = PingFwd(inputValues);
            DriveInput = outputValues[0];
            TurnInput = outputValues[1];
        }

        public void InitializeNeuralNetwork(int inputSize = 3, int outputSize = 2, int[] hiddenSizes = null) {
            if (hiddenSizes == null)
                hiddenSizes = new int[] { 4 };
            neuralNet = new LayeredNetwork(inputSize, outputSize, hiddenSizes);
            //inputValues = new float[inputSize];
            //outputValues = new float[outputSize];
        }

        // this is to pass the activation function (sigmoid here) on the neuron value and on each layer
        // a layer cannot have more than one line so we don't loop through the J
        private void ProcessActivation (Matrix mat) {
            for(int j=0; j < mat.J; j++) {
                mat.Mtx[0][j] = Sigmoid(mat.Mtx[0][j]);
            }
        }

        private float Sigmoid(float t) {
            return 1f / (1 + Mathf.Exp(-(t*2-1)));
        }

        // process the input forward to get output in the network
		public float[] PingFwd(float[] sensors) {

            // we check the sensors have a correct size
            if (sensors.Length < neuralNet.inputNeurons.J) {
                Debug.LogError("The sensors length is less than the number of input neurons! Network ping returning null.");
                return null;
            } else if (sensors.Length > neuralNet.inputNeurons.J) {
                Debug.LogWarning("The sensors length is higer than the number of input neurons! Some sensors will be ignored.");
            }

            // we process the sensors givent into the input
            for (int j = 0; j < neuralNet.inputNeurons.J; j++) {
                neuralNet.inputNeurons.Mtx[0][j] = sensors[j];
            }

            // we ping the network
            for (int i = 0; i < neuralNet.hiddenLayersNeurons.Length + 1; i++)
                {
                    if (i == 0)
                    {
                        neuralNet.hiddenLayersNeurons[0] = neuralNet.inputNeurons.Multiply(neuralNet.synapes[0]);
                        ProcessActivation(neuralNet.hiddenLayersNeurons[0]);
                    }
                    else if (i == neuralNet.hiddenLayersNeurons.Length)
                    {
                        neuralNet.outputNeurons = neuralNet.hiddenLayersNeurons[i - 1].Multiply(neuralNet.synapes[i]);
                        ProcessActivation(neuralNet.outputNeurons);
                    }
                    else
                    {
                        neuralNet.hiddenLayersNeurons[i] = neuralNet.hiddenLayersNeurons[i - 1].Multiply(neuralNet.synapes[i]);
                        ProcessActivation(neuralNet.hiddenLayersNeurons[i]);
                    }
                }

            return neuralNet.outputNeurons.GetLineValues();

            // return float

            // neuralNet.inputNeurons.Mtx[0][0] = sensors.Wall_NW;
            // neuralNet.inputNeurons.Mtx[0][1] = sensors.Wall_N;
            // neuralNet.inputNeurons.Mtx[0][2] = sensors.Wall_NE;

            // DriveInput = outputNeurons.Mtx[0][0];
            // TurnInput = outputNeurons.Mtx[0][1];
        }

        public Matrix[] GetSynapses () {
            return neuralNet.synapes;
        }

        public void InsertSynapes(Matrix[] newSynapses) {
            bool synapsesCheck = false;

            if(neuralNet.synapes.Length == newSynapses.Length){
                synapsesCheck = true;
                for (int i = 0; i < neuralNet.synapes.Length; i++){
                    synapsesCheck = synapsesCheck && neuralNet.synapes[i].I == newSynapses[i].I && neuralNet.synapes[i].J == newSynapses[i].J;
                }
            }

            if (synapsesCheck) {
                neuralNet.synapes = newSynapses;
            } else {
                Debug.LogWarning("The predefined Synapses dimention did not match the constructed synapses dimention and could not be inserted");
            }
        }

        public LayeredNetwork GetLayeredNetworkCopy () {
            int inputN = neuralNet.inputNeurons.J;
            int outputN = neuralNet.outputNeurons.J;
            int hiddenL = neuralNet.hiddenLayersNeurons.Length;
            int[] hiddenN = new int[hiddenL];
            for(int i=0; i<hiddenL; i++) {
                hiddenN[i] = neuralNet.hiddenLayersNeurons[i].J;
            }
            LayeredNetwork neuralNetCopy = new LayeredNetwork(inputN, outputN, hiddenN);
            neuralNetCopy.synapes = neuralNet.synapes;
            return neuralNetCopy;
        }

        public void SetLayeredNework (LayeredNetwork newNetwork) {
            neuralNet = newNetwork;
        }
		
    }
}
