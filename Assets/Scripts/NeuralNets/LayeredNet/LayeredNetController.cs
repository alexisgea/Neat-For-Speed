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
    public class LayeredNetController : CarController {

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
            outputValues = neuralNet.PingFwd(inputValues);
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

        public LayeredNetwork GetLayeredNetCopy () {
            return neuralNet.GetClone();
        }

        public void SetLayeredNework (LayeredNetwork newNetwork) {
            neuralNet = newNetwork;
        }
		
    }
}
