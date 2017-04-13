using UnityEngine;
using System;

namespace nfs.car {

	// this is an example implementation of a base layered net controller

    ///<summary>
	/// Implementation of a layered neural network controller.
    /// Owns an instance of a neural network and updates it (ping forward).
	///</summary>
    [RequireComponent(typeof(CarSensors))]
	[RequireComponent(typeof(CarBehaviour))]
    public class CarLayeredNetController : nets.layered.Controller {

		// reference to the car
		private CarBehaviour car;
		// reference to the sensors for input
		private CarSensors sensors;
		// called when the car dies or is triggered for similar purpose otherwise
        //public event Action <CarBehaviour> CarDeath;

		private float driveInput;
		private float turnInput;

		/// <summary>
		/// Generation start time which needs to be set in the trainer implementation.
		/// </summary>
		/// <value>The start time.</value>
		public float StartTime {set; get;}
		/// <summary>
		/// Distance use to get a normalised value in the fitness calculation, needs to be set in the trainer implementation.
		/// </summary>
		/// <value>The max dist fitness.</value>
		public float MaxDistFitness {set; get;} 


		/// <summary>
		/// Initialises the neural net controller.
		/// We get the references to the necessary component and suscribe to some events.
		/// </summary>
		protected override void InitialiseController() {
			car = GetComponent<CarBehaviour>();
			sensors = GetComponent<CarSensors>();
			car.HitSomething += OnCarHit;
		}

		/// <summary>
		/// Updates the input values from the controller in the neural net.
		/// </summary>
		protected override void UpdateInputValues() {
			//TODO implement something better with some kind of enums or other
			inputValues[0] = sensors.Wall_NE;
			inputValues[1] = sensors.Wall_N;
			inputValues[2] = sensors.Wall_NW;
		}

		/// <summary>
		/// Uses the output values from the neural net in the controller.
		/// </summary>
		protected override void UseOutputValues() {
			driveInput = outputValues[0];
			turnInput = outputValues[1];

			car.Drive(driveInput);
			car.Turn(turnInput);
		}

		/// <summary>
		/// Checks the neural net alive status.
		/// </summary>
		protected override void CheckAliveStatus() {
			if(driveInput < 0.1f || Mathf.Abs (transform.position.y) > 1f) {
				Kill();         
			}
		}

        ///<summary>
        /// Called by each car's colision.
        ///</summary>
        private void OnCarHit (string what) {
            // first we make sure the car hit a wall
            if(what == "wall") {
                Kill();
            }
        }

		/// <summary>
		/// Reset the specified position and orientation and other values of the controller.
		/// </summary>
		/// <param name="position">Position.</param>
		/// <param name="orientation">Orientation.</param>
		public override void Reset(Vector3 position, Quaternion orientation) {
			base.Reset(position, orientation);
			car.Reset();
		}

		/// <summary>
		/// Computes the fitness value of a network. 
		/// In this case from a mix distance and average speed where distance is more important than speed.
		/// </summary>
        protected override float CalculateFitness () {
            float distanceFitness = car.DistanceDriven < 1f ? 0f :car.DistanceDriven / MaxDistFitness;

            float timeElapsed = (Time.unscaledTime - StartTime);
            float speedFitness = timeElapsed <= 1? 0f : (car.DistanceDriven / timeElapsed) / car.MaxForwardSpeed;

            float fitness = distanceFitness + distanceFitness * speedFitness;

            return fitness;
        }
		
    }
}
