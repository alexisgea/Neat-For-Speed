using UnityEngine;
using System;
using nfs.car;

namespace nfs.controllers {

	// this is an example implementation of a base NeuralNetController

    ///<summary>
	/// Neural network controller class.
    /// Owns an instance of a neural network and updates it (ping forward).
	///</summary>
    [RequireComponent(typeof(CarSensors))]
    public class CarLayeredNetController : CarController {

        // THE Neural Network
        public layered.NeuralNet NeuralNet {set; get;}

        // array to send an receive the data to the network
        [SerializeField] private float[] inputValues;
        [SerializeField] private float[] outputValues;

		// reference to the sensors for input
		private CarSensors sensors;
		// called when the car dies or is triggered for similar purpose otherwise
        public event Action <CarBehaviour> CarDeath;

        // equivalent of start, from the car controller class
		protected override void DerivedStart() {
			InitialiseController ();
		}

		// equivalent of update, from the car controller class
		protected override void DerivedUpdate() {

			UpdateInputValues ();

			if(NeuralNet != null) {
				outputValues = NeuralNet.PingFwd(inputValues);
			}

			UseOutputValues ();

			CheckAliveStatus ();
		}

		/// <summary>
		/// Initialises the neural net controller.
		/// </summary>
		private void InitialiseController() {
			sensors = GetComponent<CarSensors>();
			GetComponent<CarBehaviour>().HitSomething += OnCarHit;
		}

		/// <summary>
		/// Updates the input values from the controller in the neural net.
		/// </summary>
		private void UpdateInputValues() {
			//TODO implement something better with some kind of enums or other
			inputValues[0] = sensors.Wall_NE;
			inputValues[1] = sensors.Wall_N;
			inputValues[2] = sensors.Wall_NW;
		}

		/// <summary>
		/// Uses the output values from the neural net in the controller.
		/// </summary>
		private void UseOutputValues() {
			driveInput = outputValues[0];
			turnInput = outputValues[1];
		}

		/// <summary>
		/// Checks the neural net alive status.
		/// </summary>
		private void CheckAliveStatus() {
			if(driveInput < 0.1f && !GetComponent<CarBehaviour>().Stop) {
				Stop();         
			}
		}

        ///<summary>
        /// Called by each car's colision.
        ///</summary>
        private void OnCarHit (string what) {
            // first we make sure the car hit a wall
            if(what == "wall") {
                Stop();
            }
        }

		/// <summary>
		/// Stop the car and invoke the "CarDeath" event.
		/// </summary>
        public void Stop() {
             GetComponent<CarBehaviour>().Stop = true;

             if(CarDeath != null) {
                 CarDeath.Invoke(GetComponent<CarBehaviour>());
             }
        }
		
    }
}
