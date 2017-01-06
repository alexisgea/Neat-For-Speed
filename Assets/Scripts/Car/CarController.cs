using UnityEngine;

namespace airace {

	/// <summary>
	/// Script controlling the car, acceleration, braking and turning.
	/// The player controller or ANN can call the control function with an intensity optional parameter:
	/// The intensity is 0.75 by default for a simple integration and can be between 0 and 1 for more complexity.
	/// Available controls are Accelerate(), Brake(), Reverse(), TurnRight(), TurnLeft()
	/// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class CarController : MonoBehaviour {

		// car behavior related variables
        private float steeringRate = 100f;
        private float brakingRate = 20f;
        private float acceleration = 30f;
        private float maxForwardSpeed = 30f;
        private float maxReverseSpeed = -10f;
        private float frictionBrake = 10f;
        private float aboutZero = 0.01f;
        private float speed = 0f;
        private float Speed {
            set {
                speed = Mathf.Clamp(value, maxReverseSpeed, maxForwardSpeed);
                if ((Mathf.Abs(speed) - aboutZero) < aboutZero)
                    speed = 0f;
            }
			get { return speed; }
        }

		/// <summary>
		/// Will return speed value from -1 to 1.
		/// </summary>
		public float NormalizedSpeed {
			get {
				if(speed >= 0)
					return speed/maxForwardSpeed;
				else
                    return -speed / maxReverseSpeed;
			}
		}

		// bool use to initiate the rest car process
        private bool reset = false; // reset will block controls
		
		// reference to the car Rigidbody
        private Rigidbody car;

        private void Start() {
            car = GetComponent<Rigidbody>();
        }

        // Updates the car movement if speed not at 0 and reset the car if necessary
        private void Update() {
            if (reset) {
                Brake(1f);

                if (Speed == 0f)
                    Reset();
            }

            if (Speed != 0f) {
                FrictionEffect();
                MoveCar();
            }
        }

		// Default slow down effect running each frame
        private void FrictionEffect() {
			if(Speed > 0)
            	Speed -= frictionBrake * Time.deltaTime;
			else
				Speed += frictionBrake * Time.deltaTime;
        }

		// Updates the car position each frame depending on speed.
        private void MoveCar() {
            car.MovePosition(transform.position + transform.forward * Speed * Time.deltaTime);
        }

		// Is called from the control methods to update the speed value
		private void UpdateSpeed(float dir, float amount) {
            if (!reset)
                Speed += dir * amount * Time.deltaTime;
        }

		// Is called from the control methods to turn the car
		private void Turn(float dir, float amount) {
            if (!reset)
                transform.Rotate(0f, dir * amount * Time.deltaTime, 0f);
        }

		// called when there is a collision to reset the car
        private void OnTriggerEnter(Collider other) {
            reset = true;
            if(other.gameObject.tag == "wall"){
				reset = true;
				// Destroy(gameObject);
			} else if(other.gameObject.tag == "car"){
				// reset = true;
				// Destroy(gameObject);
			}
                
        }


		// resets the car to the start state
        private void Reset() {
            transform.position = new Vector3(0f, 0.5f, 0f);
            transform.rotation = Quaternion.identity;
            car.velocity = Vector3.zero;
            reset = false;
        }

		// Public Control Intention Methods

		/// <summary>
		/// Raise the speed by the intensity and acceleration factor.
		/// If the car is going in reverse it will first brake.
		/// </summary>
        public void Accelerate(float intensity = 0.75f) {
            if (Speed >= 0f && Speed <= maxForwardSpeed)
				UpdateSpeed(1, acceleration * Mathf.Clamp01(intensity));
            else
                Brake();
        }

		/// <summary>
		/// Lower the speed to negative by the intensity and acceleration factor.
		/// If the car is going forward it will first brake.
		/// </summary>
        public void Reverse(float intensity = 0.75f) {
            if (Speed <= 0f && speed >= maxReverseSpeed)
                UpdateSpeed(-1, acceleration * Mathf.Clamp01(intensity));
            else
                Brake();
        }

		/// <summary>
		/// Get the speed closer to 0 by the intensity and brake rate.
		/// </summary>
        public void Brake(float intensity = 0.75f) {
            float brakeValue = brakingRate * Mathf.Clamp01(intensity);

            if (Mathf.Abs(Speed) < brakeValue)
                Speed = 0f;
            else if (Speed > 0f)
                UpdateSpeed(-1, brakeValue);
            else
                UpdateSpeed(1, brakeValue);
        }
        
		/// <summary>
		/// Rotate the car by the intensity and steer rate.
		/// </summary>
        public void TurnRight(float intensity = 0.75f) {
            Turn(1, steeringRate * Mathf.Clamp01(intensity));
        }

		/// <summary>
		/// Rotate the car by the intensity and steer rate.
		/// </summary>
        public void TurnLeft(float intensity = 0.75f) {
            Turn(-1, steeringRate * Mathf.Clamp01(intensity));
        }

    }
}
