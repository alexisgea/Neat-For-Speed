using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace airace {

    // NOTES TO MYSELF
    // potentially there could be one neural network piloting every car of the same type in the same time
    // we could gather all input in one matrix and use matrix multiplication to compute all the outpu in the same time
    // I guess the issue is that it implies they all have the same weights and thus probably the same exact output
    // although this mean we can probably run simultaneous race with one neural network driving multiple cars
    // even if the race track is different, thus we could maybe train it faster. We'll have to see
	public class BasicNeuralNet : CarController {

        int inputLayerSize;
        
        int outputLayerSize;

        int hiddenLayersNumber;
		int[] hiddenLayersSizes;

        
        Matrix inputNeurons;
		Matrix[] hiddenNeurons;
        Matrix outputNeurons;

        Matrix[] synapes;



        protected override void ChildStart () {
            initializeNeuralNetwork();

        }

        protected override void ChildUpdate() {

            // some form of get the new inputs to be here

            PingNetwork();

            DriveInput = outputNeurons.Mtx[0][0];
            TurnInput = outputNeurons.Mtx[1][0];

        }

        // TODO add random value initialization for the synapes
        private void initializeNeuralNetwork () {
            // input layer is a matrix of 1 column only
            inputLayerSize = 3;
            inputNeurons = new Matrix(inputLayerSize, 1);

            // hidden layer is an array of matrix of one colum
            hiddenLayersNumber = 1;
            hiddenLayersSizes = new int[hiddenLayersNumber];
            hiddenLayersSizes[0] = 4; // for bla bla bla

            hiddenNeurons = new Matrix[hiddenLayersNumber];
            for (int i = 0; i < hiddenLayersNumber; i++) { 
                hiddenNeurons[i] = new Matrix(hiddenLayersSizes[i], 1);
            }

            // output layer is a matrix of 1 column only
            outputLayerSize = 2;
            outputNeurons = new Matrix(outputLayerSize, 1);

            // synapes are an array of matrix
            // the number of line is equal to the previous layer
            // the number of column is equal to the next layer
            synapes = new Matrix[hiddenLayersNumber + 1];
            for (int i = 0; i < hiddenLayersNumber + 1; i++) {
                if(i==0)
                    synapes[i] = new Matrix(inputLayerSize, hiddenLayersSizes[i]);
                else if (i == hiddenLayersNumber)
                    synapes[i] = new Matrix(hiddenLayersSizes[i], outputLayerSize);
                else
                    synapes[i] = new Matrix(hiddenLayersSizes[i-1], hiddenLayersSizes[i]);
            }
			
        }
        
        // this is to sigmoidize the neuron value and will be called on each layer
        // a layer cannot have more than one colum so we don't loop through the J
        private void ProcessActivation (Matrix mat) {
            for(int i=0; i < mat.I; i++) {
                mat.Mtx[i][0] = Sigmoid(mat.Mtx[i][0]);
            }
        }

        private float Sigmoid(float t) {
            return 1f / (1 + Mathf.Exp(-t));
        }

        // process the input forward to get output in the network
		private void PingNetwork() {
            for (int i = 0; i < hiddenLayersNumber + 1; i++) {
                if(i==0){
                    hiddenNeurons[0] = inputNeurons.Multiply(synapes[0]);
                    ProcessActivation(hiddenNeurons[0]);
                }
                else if (i == hiddenLayersNumber) {
                    outputNeurons = hiddenNeurons[i-1].Multiply(synapes[i]);
                    ProcessActivation(outputNeurons);
                }
                else {
                    hiddenNeurons[i] = hiddenNeurons[i-1].Multiply(synapes[i]);
                    ProcessActivation(hiddenNeurons[i]);
                }
            }
		}

		
    }
}
