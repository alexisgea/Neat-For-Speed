using UnityEngine;
using System;

namespace nfs.car {

    /// <summary>
    /// Class controlling the car, acceleration, braking and turning.
    /// The player controller or ANN can call the control function.
    /// Available controls are Drive(), Turn(), Brake()
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class CarBehaviour : MonoBehaviour {

        // car behavior related constants and accessors
        [SerializeField] private const float steeringRate = 100f;
		[SerializeField] private const float brakingRate = 20f;
		[SerializeField] private const float acceleration = 30f;

		[SerializeField] private const float maxForwardSpeed = 30f;
        public float MaxForwardSpeed {get { return maxForwardSpeed; } }

		[SerializeField] private const float maxReverseSpeed = -10f;
		public float MaxReverseSpeed {get { return maxReverseSpeed; } }

		// I made a script to check rate of value change of a keyboard axis and it was ~0.0495
		private const float controlChangeRate = 0.05f; 
		private const float aboutZero = 0.01f;

		// car behaviour related variables
        private float speed = 0f;
        private float Speed {
            set {
                speed = value;
                if ((Mathf.Abs(speed) - aboutZero) < aboutZero)
                    speed = 0f;
            }
			get { return speed; }
        }
			
		// car controle intensity related variable (goes from -1 to 1)
		private float driveIntensity = 0f;
        /// <summary>
        /// How much the car controller is pressing on the accelerating pedal.
        /// </summary>
        public float DriveIntensity {
            get { return NormalizeControl(driveIntensity); }
        }
		private float turnIntensity = 0f;
		/// <summary>
		/// How much the car controller is turning the wheel.
		/// </summary>
        public float TurnIntensity {
            get { return NormalizeControl(turnIntensity); }
        }
		/// <summary>
		/// Gets the normalized speed.
		/// </summary>
		/// <value>The normalized speed.</value>
		public float NormalizedSpeed {
			get { 
                if(Speed >= 0f)
                    return Speed / maxForwardSpeed;
                else
                    return Speed / maxReverseSpeed;
            }
		}
		/// <summary>
		/// Forward or backward direction. (1 or 0)
		/// </summary>
        public float Direction {
            get {
                if(Speed >= 0f)
                    return 1f;
                else
                    return 0f;
            }
        }
			
		/// <summary>
		/// Distance driven is used by the genetic algorythm.
		/// </summary>
        public float DistanceDriven { private set; get; }      

        // Reference to the car Rigidbody.
        private Rigidbody car;

		/// <summary>
		/// Occurs when the car hit something.
		/// </summary>
        public event Action <string>HitSomething;


        /// <summary>
        /// Monobehaviour start.
        /// </summary>
        private void Start() {
            car = GetComponent<Rigidbody>();
            DistanceDriven = 0;
        }

		/// <summary>
		/// Monobehaviour update.
		/// </summary>
        private void Update() {

			if (Speed != 0f) {
                MoveCar();
                FrictionEffect();
            }
        }

		/// <summary>
		/// Both control (turn and drive) are actually axis values from -1 to 1 which we can normalize for the genetic algo.
		/// </summary>
		/// <returns>The control.</returns>
		/// <param name="force">Force.</param>
        private float NormalizeControl(float force) {
            return (force + 1f) / 2f;
        }

		/// <summary>
		/// Default friction slow down effect running each frame.
		/// It will completly compensate acceleration toward max speed, enforcing it.
		/// </summary>
        private void FrictionEffect() {
			if (Speed > 0) {
				Speed -= acceleration * Speed / maxForwardSpeed * Time.deltaTime;

			} else {
				Speed += acceleration * Speed / maxReverseSpeed * Time.deltaTime;
			}
        }

		/// <summary>
		/// Updates the car position each frame depending on speed.
		/// </summary>
        private void MoveCar() {
            car.MovePosition(transform.position + transform.forward * Speed * Time.deltaTime);
			DistanceDriven += Speed * Time.deltaTime;
        }

		/// <summary>
		/// Modifies an axis intensity by reference. The idea being that an ANN should not be able
		/// to jump from one side of the wheel to the other to keep a human behavior.
		/// This is done naturally with a keyboard or joystick, so this function enforces
		/// this gradual change of value. I checked with a script to make sure this rate
		/// is similar to the one with a keyboard in unity.
		/// </summary>
		/// <returns>The control intensity.</returns>
		/// <param name="currentIntensity">Current intensity.</param>
		/// <param name="targeIntensity">Targe intensity.</param>
		private float GetControlIntensity(ref float currentIntensity, float targeIntensity) {

            // if the target value is above the current value
            if(targeIntensity > currentIntensity && (targeIntensity - currentIntensity) < controlChangeRate)
                currentIntensity = targeIntensity;

            else if(targeIntensity > currentIntensity)
                currentIntensity += controlChangeRate;

            // if the target value is under the current value
            else if(targeIntensity < currentIntensity && (currentIntensity - targeIntensity) < controlChangeRate)
                currentIntensity = targeIntensity;
            
            else if(targeIntensity < currentIntensity)
                currentIntensity -= controlChangeRate;
            
            currentIntensity = Mathf.Clamp(currentIntensity, -1f, 1f);

            return currentIntensity;
        }

        /// <summary>
		/// Raises the trigger enter event when there is a collision.
        /// </summary>
        /// <param name="other">Other.</param>
		private void OnTriggerEnter(Collider other) {
            RaiseHitSomething(other.gameObject.tag);
        }

        /// <summary>
		/// Sends this object and the tag of the other object in the collision.
		/// </summary>
        private void RaiseHitSomething(string tag){
            if(HitSomething != null)
                HitSomething.Invoke(tag);
        }

		/// <summary>
		/// Resets the car state to start again.
		/// </summary>
        public void Reset() {
            car.velocity = Vector3.zero;
            DistanceDriven = 0;
        }

		/// <summary>
		/// Is called from the control methods to update the speed value
		/// </summary>
		public void Drive(float targetIntensity) {

            // gets the actual force gradually changed toward the targetForce
			float intensity = GetControlIntensity(ref driveIntensity, targetIntensity);

			if ((Speed < 0f && intensity >= 0f) || (Speed > 0f && intensity < 0f)) {
				Brake (intensity);

			} else {
				Speed += intensity * acceleration * Time.deltaTime;
			}
        }

        // TODO Need to somehow improve the function to not be just rotating on the axis but dependent on where the wheels are pointing
        /// <summary>
		/// Is called from the control methods to turn the car
		/// </summary>
		public void Turn(float targetIntensity) {

            // gets the actual force gradually changed toward the targetForce
			float intensity = GetControlIntensity(ref turnIntensity, targetIntensity);

            // if the speed is about zero we can't turn
            if ((Mathf.Abs(Speed)-aboutZero)> 0f) {
                float relativeSpeed = Speed >= 0 ? Speed / maxForwardSpeed : Speed / maxReverseSpeed;
                
                float turnValue = 0f;
                // if the speed is high it wil be harder to turn due to coriolis effects
                if(relativeSpeed > 0.5)
                    turnValue = intensity * steeringRate * (1-relativeSpeed/2.5f) * Time.deltaTime;
                // but if the speed is low we don't want the car to turn on it's axis
                else
                    turnValue = intensity * steeringRate * relativeSpeed * Time.deltaTime;

                transform.Rotate(0f, turnValue, 0f);
            }
        }

		/// <summary>
		/// Get the speed closer to 0 by the force and brake rate.
		/// </summary>
        public void Brake(float intensity = 0.75f) {
            float brakeValue = brakingRate * Mathf.Abs(intensity) * Time.deltaTime;
            
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
