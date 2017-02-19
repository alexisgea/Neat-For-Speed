using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using nfs.tools;

namespace nfs.layered {
    public class LayeredNetwork {

        public Matrix inputNeurons;
        public Matrix[] hiddenLayersNeurons;
        public Matrix outputNeurons;
        public Matrix[] synapes;

        public LayeredNetwork(int inputLayerSize, int outputLayerSize, int[] hiddenLayersSizes) {
            // each layer is one line of neuron

            // input layer is a matrix of 1 line only 
            inputNeurons = new Matrix(1, inputLayerSize).SetToOne();  // default should be 3

            // output layer is a matrix of 1 line only
            outputNeurons = new Matrix(1, outputLayerSize).SetToOne(); // default should be 2

            // hidden layer is an array of matrix of one line
            hiddenLayersNeurons = new Matrix[hiddenLayersSizes.Length];  // default should be 4 - 1
            for (int i = 0; i < hiddenLayersSizes.Length; i++) {
                hiddenLayersNeurons[i] = new Matrix(1, hiddenLayersSizes[i]).SetToOne();
            }

            // synapes are an array of matrix
            // the number of line (or array of synapes sort of) is equal to the previous layer (neurons coming from)
            // the number of column (or nb of synapes in a row) is equal to the next layer (neurons going to)
            synapes = new Matrix[hiddenLayersSizes.Length + 1];
            for (int i = 0; i < hiddenLayersSizes.Length + 1; i++) {
                if (i == 0)
                    synapes[i] = new Matrix(inputLayerSize, hiddenLayersSizes[i]).SetAsSynapse();
                else if (i == hiddenLayersSizes.Length)
                    synapes[i] = new Matrix(hiddenLayersSizes[i - 1], outputLayerSize).SetAsSynapse();
                else
                    synapes[i] = new Matrix(hiddenLayersSizes[i - 2], hiddenLayersSizes[i - 1]).SetAsSynapse();
            }
        }
    }
}
