using UnityEngine;
using System;
using nfs.car;

namespace nfs.controllers {

    ///<summary>
	/// Neural network controller class.
    /// Owns an instance of a neural network and updates it (ping forward).
	///</summary>
    [RequireComponent(typeof(CarSensors))]
    public class CarLayeredNetController : CarController {

        // THE Neural Network
        public layered.NeuralNet NeuralNet {set; get;}
        // reference to the sensors for input
        private CarSensors sensors;
        // array to send an receive the data to the network
        private float[] inputValues = new float[] {0f, 0f, 0f};
        private float[] outputValues = new float[] {0f, 0f};

        public event Action <CarBehaviour> CarDeath;

        // equivalent of start, from the car controller class
        protected override void DerivedStart() {
            sensors = GetComponent<CarSensors>();
            GetComponent<CarBehaviour>().HitSomething += OnCarHit;
        }

        // equivalent of update, from the car controller class
        protected override void DerivedUpdate() {

            // TODO change this bit to be more flexible (enum stuff?)
            // we get input from the sensors in the array and pass it to the network who returns an array of output
            inputValues[0] = sensors.Wall_NE;
            inputValues[1] = sensors.Wall_N;
            inputValues[2] = sensors.Wall_NW;
            
            if(NeuralNet != null) {
                outputValues = NeuralNet.PingFwd(inputValues);
            }

            // we send the received output to the car
            DriveInput = outputValues[0];
            TurnInput = outputValues[1];

            // if the car is not going forward or going backward, kill the car
            if(DriveInput < 0.1f && !GetComponent<CarBehaviour>().Stop) {
                Stop();         
            }
        }

        ///<summary>
        /// Called by each car's colision.
        /// Updates the car alive counter and add the car to the deadCars array to not count them multiple tiem.
        ///</summary>
        private void OnCarHit (string what) {
            // first we make sure the car hit a wall
            if(what == "wall") {
                Stop();
            }
        }

        public void Stop() {
             GetComponent<CarBehaviour>().Stop = true;

             if(CarDeath != null) {
                 CarDeath.Invoke(GetComponent<CarBehaviour>());
             }
        }

        
		
    }
}
