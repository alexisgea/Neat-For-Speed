using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace airace {

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
	public class BasicNeuralNet : CarController {

        [SerializeField] int inputLayerSize;
        [SerializeField] int[] hiddenLayersSizes;
        [SerializeField] int outputLayerSize;

        Matrix inputNeurons;
		Matrix[] hiddenLayersNeurons;
        Matrix outputNeurons;

        Matrix[] synapes;

        CarSensors sensors;


        protected override void ChildStart () {
            sensors = GetComponent<CarSensors>();

            InitNeuralNet();
            FwdPing();
        }

        protected override void ChildUpdate() {
            ProcessSensors();
            FwdPing();

            DriveInput = outputNeurons.Mtx[0][0];
            TurnInput = outputNeurons.Mtx[0][1];

        }

        private void InitNeuralNet () {
            // each layer is one line of neuron

            // input layer is a matrix of 1 line only 
            inputNeurons = new Matrix(1, inputLayerSize).SetToOne();  // default should be 3

            // hidden layer is an array of matrix of one line
            hiddenLayersNeurons = new Matrix[hiddenLayersSizes.Length];  // default should be 4 - 1
            for (int i = 0; i < hiddenLayersSizes.Length; i++) { 
                hiddenLayersNeurons[i] = new Matrix(1, hiddenLayersSizes[i]).SetToOne();
            }

            // output layer is a matrix of 1 line only
            outputNeurons = new Matrix(1, outputLayerSize).SetToOne(); // default should be 2

            // synapes are an array of matrix
            // the number of line (or array of synapes sort of) is equal to the previous layer (neurons coming from)
            // the number of column (or nb of synapes in a row) is equal to the next layer (neurons going to)
            synapes = new Matrix[hiddenLayersSizes.Length + 1];
            for (int i = 0; i < hiddenLayersSizes.Length + 1; i++) {
                if(i==0)
                    synapes[i] = new Matrix(inputLayerSize, hiddenLayersSizes[i]).SetAsSynapse();
                else if (i == hiddenLayersSizes.Length)
                    synapes[i] = new Matrix(hiddenLayersSizes[i-1], outputLayerSize).SetAsSynapse();
                else
                    synapes[i] = new Matrix(hiddenLayersSizes[i-2], hiddenLayersSizes[i-1]).SetAsSynapse();
            }
			
        }
        
		// this is to pass the activation function (sigmoid here) on the neuron value and on each layer
        // a layer cannot have more than one line so we don't loop through the J
        private void ProcessActivation (Matrix mat) {
            for(int j=0; j < mat.J; j++) {
                mat.Mtx[0][j] = Sigmoid(mat.Mtx[0][j]);
            }
        }

        private float Sigmoid(float t) {
            return 1f / (1 + Mathf.Exp(-t));
        }

        // process the input forward to get output in the network
		private void FwdPing() {
            for (int i = 0; i < hiddenLayersSizes.Length + 1; i++) {
                if(i==0){
                    hiddenLayersNeurons[0] = inputNeurons.Multiply(synapes[0]);
                    ProcessActivation(hiddenLayersNeurons[0]);
                }
                else if (i == hiddenLayersSizes.Length) {
                    outputNeurons = hiddenLayersNeurons[i-1].Multiply(synapes[i]);
                    ProcessActivation(outputNeurons);
                }
                else {
                    hiddenLayersNeurons[i] = hiddenLayersNeurons[i-1].Multiply(synapes[i]);
                    ProcessActivation(hiddenLayersNeurons[i]);
                }
            }
		}

        // get the input from the sensors
        private void ProcessSensors() {
            inputNeurons.Mtx[0][0] = sensors.Wall_NW;
            inputNeurons.Mtx[0][1] = sensors.Wall_N;
            inputNeurons.Mtx[0][2] = sensors.Wall_NE;
        }

        private void PrintANN () {
            Debug.Log("Input neurons: " + inputNeurons.GetValuesAsString());
            Debug.Log("First synapes: " + synapes[0].GetValuesAsString());
            Debug.Log("Hidden neurons: " + hiddenLayersNeurons[0].GetValuesAsString());
            Debug.Log("Second synapes: " + synapes[1].GetValuesAsString());
            Debug.Log("Output neurons: " + outputNeurons.GetValuesAsString());
        }

		
    }
}
