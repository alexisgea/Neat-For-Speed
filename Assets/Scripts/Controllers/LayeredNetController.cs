using UnityEngine;
using nfs.car;
using nfs.layered;

namespace nfs.controllers {

    ///<summary>
	/// Neural network controller class.
    /// Owns an instance of a neural network and updates it (ping forward).
	///</summary>
    [RequireComponent(typeof(CarSensors))]
    public class LayeredNetController : CarController {

        // THE Neural Network
        private LayeredNetwork neuralNet;
        // reference to the sensors for input
        private CarSensors sensors;
        // array to send an receive the data to the network
        private float[] inputValues;
        private float[] outputValues;

        // equivalent of start, from the car controller class
        protected override void DerivedStart() {
            sensors = GetComponent<CarSensors>();
            InitializeNeuralNetwork();
        }

        // equivalent of update, from the car controller class
        protected override void DerivedUpdate() {
            // we get input from the sensors in the array and pass it to the network who returns an array of output
            inputValues = new float[] {sensors.Wall_NE, sensors.Wall_N, sensors.Wall_NW};
            outputValues = neuralNet.PingFwd(inputValues);

            // we send the received output to the car
            DriveInput = outputValues[0];
            TurnInput = outputValues[1];

            // if the car is not going forward or going backward, kill the car
            if(DriveInput < 0.1f && !GetComponent<CarBehaviour>().Stop)
                GetComponent<CarBehaviour>().RaiseHitSomething("wall");          
        }

        ///<summary>
        /// Creates a default or specified neural network.
        ///</summary>
        public void InitializeNeuralNetwork(int inputSize = 3, int biasNeuron = 1, int outputSize = 2, int[] hiddenSizes = null) {
            if (hiddenSizes == null)
                hiddenSizes = new int[] { 4 };
            neuralNet = new LayeredNetwork(inputSize + biasNeuron, outputSize, hiddenSizes);
        }

        ///<summary>
        /// Get a reference to the Layered Net.
        ///</summary>
        public LayeredNetwork GetLayeredNet () {
            return neuralNet;
        }

        ///<summary>
        /// Request a deep clone from the network.
        ///</summary>
        public LayeredNetwork GetLayeredNetClone () {
            return neuralNet.GetClone();
        }

        ///<summary>
        /// Replace the neural network with a new one.
        ///</summary>
        public void SetLayeredNework (LayeredNetwork newNetwork) {
            neuralNet = newNetwork;
        }
		
    }
}
