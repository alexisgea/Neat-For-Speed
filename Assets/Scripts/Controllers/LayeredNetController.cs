using UnityEngine;
using nfs.car;
using nfs.layered;

namespace nfs.controllers {

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

            // if the car is not going forward or going backward, kill the car
            if(DriveInput < 0.6f && !GetComponent<CarBehaviour>().Stop)
                GetComponent<CarBehaviour>().RaiseHitSomething("wall");

            // Debug.Log("drive: " + DriveInput);
            // Debug.Log("turn: " + TurnInput);
            
        }

        public void InitializeNeuralNetwork(int inputSize = 3, int outputSize = 2, int[] hiddenSizes = null) {
            if (hiddenSizes == null)
                hiddenSizes = new int[] { 4 };
            neuralNet = new LayeredNetwork(inputSize, outputSize, hiddenSizes);
        }

        public LayeredNetwork GetLayeredNetCopy () {
            return neuralNet.GetClone();
        }

        public void SetLayeredNework (LayeredNetwork newNetwork) {
            neuralNet = newNetwork;
        }
		
    }
}
