using UnityEngine;
using System;

namespace nfs.car {

    /// <summary>
    /// Class controlling the car, acceleration, braking and turning.
    /// The player controller or ANN can call the control function with an intensity optional parameter:
    /// The intensity is 0.75 by default for a simple integration and can be between 0 and 1 for more complexity.
    /// Available controls are Drive(), Turn(), Brake()
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class CarBehaviour : MonoBehaviour {

        // car behavior related variables
        private float steeringRate = 100f;
        private float brakingRate = 20f;
        private float acceleration = 30f;
        private float maxForwardSpeed = 30f;
        public float MaxForwardSpeed {get { return maxForwardSpeed; } }
        private float maxReverseSpeed = -10f;
        private float aboutZero = 0.01f;
        private float speed = 0f;
        private float Speed {
            set {
                speed = value;
                if ((Mathf.Abs(speed) - aboutZero) < aboutZero)
                    speed = 0f;
            }
			get { return speed; }
        }
        public bool Stop { set; get; }

        // axis force related variable
        private float forceChangeRate = 0.05f; // I made a script to check rate of value change of a keyboard axis and it was ~0.0495
        private float driveForce = 0f;
        private float turnForce = 0f;

        // public acces to the current values of the axis in case it is need for the AI
        public float DriveForce {
            get { return NormalizeForce(driveForce); }
        }
        public float TurnForce {
            get { return NormalizeForce(turnForce); }
        }
		public float NormalizedSpeed {
			get { 
                if(Speed >= 0f)
                    return Speed / maxForwardSpeed;
                else
                    return Speed / maxReverseSpeed;
            }
		}
        public float Direction {
            get {
                if(Speed >= 0f)
                    return 1f;
                else
                    return 0f;
            }
        }

        // Distance driven is used by the genetic algorythm
        public float DistanceDriven { private set; get; }      

        // reference to the car Rigidbody
        private Rigidbody car;
        public event Action <CarBehaviour, string>HitSomething;
        //public UnityEvent test2;

        // initiate all sort of stuff
        private void Start() {
            car = GetComponent<Rigidbody>();
            Stop = false;
            DistanceDriven = 0;
        }

        // Updates the car movement if speed not at 0 and reset the car if necessary
        private void Update() {
            if (Stop) {
                Brake(1f);

                // if (Speed == 0f)
                //     Reset();
            }

            if (Speed != 0f) {
                MoveCar();
                FrictionEffect();
            }
        }

        // Both forces are actually axis values from -1 to 1
        private float NormalizeForce(float force) {
            return (force + 1f) / 2f;
        }

		// Default friction slow down effect running each frame
        // it will completly compensate acceleration toward max speed, enforcing it
        private void FrictionEffect() {
			if(Speed > 0)
                Speed -= acceleration * Speed/maxForwardSpeed * Time.deltaTime;
			else
				Speed += acceleration * Speed/maxReverseSpeed * Time.deltaTime;
        }


		// Updates the car position each frame depending on speed.
        private void MoveCar() {
            car.MovePosition(transform.position + transform.forward * Speed * Time.deltaTime);
            if(!Stop)
                DistanceDriven += Speed * Time.deltaTime;
        }

		// Modifies an axis force by reference. The idea being that an ANN should not be able
        // to jump from one side of the wheel to the other to keep a human behavior.
        // This is done naturally with a keyboard or joystick, so this function enforces
        // this gradual change of value. I checked with a script to make sure this rate
        // is similar to the one with a keyboard in unity.
        private float GetForce(ref float force, float targetForce) {

            // if the target value is above the current value
            if(targetForce > force && (targetForce - force) < forceChangeRate)
                force = targetForce;

            else if(targetForce > force)
                force += forceChangeRate;

            // if the target value is under the current value
            else if(targetForce < force && (force - targetForce) < forceChangeRate)
                force = targetForce;
            
            else if(targetForce < force)
                force -= forceChangeRate;
            
            force = Mathf.Clamp(force, -1f, 1f);

            return force;
        }

		// called when there is a collision
        private void OnTriggerEnter(Collider other) {
            RaiseHitSomething(other.gameObject.tag);
        }

        /// <summary>
		/// Sends this object and the tag of the other object in the collision.
		/// </summary>
        public void RaiseHitSomething(string tag){
            if(HitSomething != null)
                HitSomething.Invoke(this, tag);
        }

		/// <summary>
		/// Resets the car state to start again.
		/// </summary>
        public void Reset(Vector3 position, Quaternion rotation) {
            car.velocity = Vector3.zero;
            transform.position = position;
            transform.rotation = rotation;
            DistanceDriven = 0;
            Stop = false;
        }


		// Public Control Intention Methods

		/// <summary>
		/// Is called from the control methods to update the speed value
		/// </summary>
		public void Drive(float targetForce) {

            // gets the actual force gradually changed toward the targetForce
            float force = GetForce(ref driveForce, targetForce);

            if (!Stop){
                if((Speed < 0f && force >= 0f) || (Speed > 0f && force < 0f))
                    Brake(force);
                else
                    Speed += force * acceleration * Time.deltaTime;
            }
        }

        // TODO Need to somehow improve the function to not be just rotating on the axis but dependent on where the wheels are pointing
        /// <summary>
		/// Is called from the control methods to turn the car
		/// </summary>
		public void Turn(float targetForce) {

            // gets the actual force gradually changed toward the targetForce
            float force = GetForce(ref turnForce, targetForce);

            // if the speed is about zero we can't turn
            if (!Stop && (Mathf.Abs(Speed)-aboutZero)> 0f) {
                float relativeSpeed = Speed >= 0 ? Speed / maxForwardSpeed : Speed / maxReverseSpeed;
                
                float turnValue = 0f;
                // if the speed is high it wil be harder to turn due to coriolis effects
                if(relativeSpeed > 0.5)
                    turnValue = force * steeringRate * (1-relativeSpeed/2.5f) * Time.deltaTime;
                // but if the speed is low we don't want the car to turn on it's axis
                else
                    turnValue = force * steeringRate * relativeSpeed * Time.deltaTime;

                transform.Rotate(0f, turnValue, 0f);
            }
        }

		/// <summary>
		/// Get the speed closer to 0 by the force and brake rate.
		/// </summary>
        public void Brake(float force = 0.75f) {
            float brakeValue = brakingRate * Mathf.Abs(force) * Time.deltaTime;
            
            if(Speed > 0){
                Speed -= brakeValue;
                if(Speed < 0f)
                    Speed = 0f;
            }
            else{
                Speed += brakeValue;
                if(Speed > 0f)
                    Speed = 0f;
            }
        }

    }
}
