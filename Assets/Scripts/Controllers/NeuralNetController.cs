using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace nfs.controllers {

	/* I am not sure how to write this calss but I will write how it should be conceptually
	 * This class includes everything that is mandatory to own and control a neural net
	 * The example here requires in reality to inherit Car Controller which I don't like at all
	 * I would prefer some kind of interface or composition.
	 */
	/// <summary>
	/// Base implementation for a neural net controller.
	/// </summary>
	public class NeuralNetController : MonoBehaviour {

		// THE Neural Network
		public layered.NeuralNet NeuralNet {set; get;}

		// array to send an receive the data to the network
		[SerializeField] private float[] inputValues;
		[SerializeField] private float[] outputValues;

		// stuff for the example
		//private CarSensors sensors;
		//public event Action <CarBehaviour> CarDeath;


		// start
		private void Start() {
			InitialiseController ();
		}

		// update
		private void Update() {

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
			// implement whatever needs to be initialized

			//sensors = GetComponent<CarSensors>();
			//GetComponent<CarBehaviour>().HitSomething += OnCarHit;
		}

		/// <summary>
		/// Updates the input values from the controller in the neural net.
		/// </summary>
		private void UpdateInputValues() {
			// implement a function where the input values are update before being provided to the neural net

			//inputValues[0] = sensors.Wall_NE;
			//inputValues[1] = sensors.Wall_N;
			//inputValues[2] = sensors.Wall_NW;
		}

		/// <summary>
		/// Uses the output values from the neural net in the controller.
		/// </summary>
		private void UseOutputValues() {
			// implement a function where the output values from the neural net are used for somehting

			//driveInput = outputValues[0];
			//turnInput = outputValues[1];
		}

		/// <summary>
		/// Checks the neural net alive status.
		/// </summary>
		private void CheckAliveStatus() {
			// implement what you want to check if the neural net is still alive

			//if(driveInput < 0.1f && !GetComponent<CarBehaviour>().Stop) {
			//	Stop();         
			//}
		}

		// methods used by the example

//		///<summary>
//		/// Called by each car's colision.
//		///</summary>
//		private void OnCarHit (string what) {
//			// first we make sure the car hit a wall
//			if(what == "wall") {
//				Stop();
//			}
//		}

//		/// <summary>
//		/// Stop the car and invoke the "CarDeath" event.
//		/// </summary>
//		public void Stop() {
//			GetComponent<CarBehaviour>().Stop = true;
//
//			if(CarDeath != null) {
//				CarDeath.Invoke(GetComponent<CarBehaviour>());
//			}
//		}


	}
}