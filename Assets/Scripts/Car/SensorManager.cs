using System;
using UnityEngine;

namespace airace {

	///<summary>
	/// Car sensor manager.
	/// Creates sensors, checks them each update and send signals with the distance.
	/// There are a total of 19 sensors. 8 for the wall tag and 8 for the car tag and 3 for the car controls
	/// The sensors default size is 5 from the center of the car.
	/// The sensors are named as per the cardinal position to which they point (N, NE, E, SE, S, SW, W, NW).
	/// To get the signal just register your function to any of the sensors.
	/// The function must take a float in parameter for the distance, as value from 0 to 1.
	///</summary>
	[RequireComponent (typeof (CarController))]
	public class SensorManager : MonoBehaviour {

		private int sensorLength = 5;

        // Car internal control sensors
        private CarController car;

        public event Action<float> SpeedSensor;
		private void RaiseSpeedSensor(float speed) {
			if(SpeedSensor != null)
				SpeedSensor.Invoke(speed);
		}

		public event Action<float> SteerSensor;
		private void RaiseSteerSensor(float turn) {
			if(SteerSensor != null)
				SteerSensor.Invoke(turn);
		}

		public event Action<float> DriveSnesor;
		private void RaiseDriveSensor(float drive) {
			if(DriveSnesor != null)
				DriveSnesor.Invoke(drive);
		}

		// Wall sensors events
		public event Action<float> Wall_N;
		private void RaiseWall_N(float dist) {
			if(Wall_N != null)
				Wall_N.Invoke(dist);
		}

		public event Action<float> Wall_NE;
		private void RaiseWall_NE(float dist) {
			if(Wall_NE != null)
				Wall_NE.Invoke(dist);
		}

		public event Action<float> Wall_E;
		private void RaiseWall_E(float dist) {
			if(Wall_E != null)
				Wall_E.Invoke(dist);
		}

		public event Action<float> Wall_SE;
		private void RaiseWall_SE(float dist) {
			if(Wall_SE != null)
				Wall_SE.Invoke(dist);
		}

		public event Action<float> Wall_S;
		private void RaiseWall_S(float dist) {
			if(Wall_S != null)
				Wall_S.Invoke(dist);
		}

		public event Action<float> Wall_SW;
		private void RaiseWall_SW(float dist) {
			if(Wall_SW != null)
				Wall_SW.Invoke(dist);
		}

		public event Action<float> Wall_W;
		private void RaiseWall_W(float dist) {
			if(Wall_W != null)
				Wall_W.Invoke(dist);
		}

		public event Action<float> Wall_NW;
		private void RaiseWall_NW(float dist) {
			if(Wall_NW != null)
				Wall_NW.Invoke(dist);
		}

		// Car sensors events
		public event Action<float> Car_N;
		private void RaiseCar_N(float dist) {
			if(Car_N != null)
				Car_N.Invoke(dist);
		}

		public event Action<float>Car_NE;
		private void RaiseCar_NE(float dist) {
			if(Car_NE != null)
				Car_NE.Invoke(dist);
		}

		public event Action<float>Car_E;
		private void RaiseCar_E(float dist) {
			if(Car_E != null)
				Car_E.Invoke(dist);
		}

		public event Action<float>Car_SE;
		private void RaiseCar_SE(float dist) {
			if(Car_SE != null)
				Car_SE.Invoke(dist);
		}

		public event Action<float>Car_S;
		private void RaiseCar_S(float dist) {
			if(Car_S != null)
				Car_S.Invoke(dist);
		}

		public event Action<float>Car_SW;
		private void RaiseCar_SW(float dist) {
			if(Car_SW != null)
				Car_SW.Invoke(dist);
		}

		public event Action<float>Car_W;
		private void RaiseCar_W(float dist) {
			if(Car_W != null)
				Car_W.Invoke(dist);
		}

		public event Action<float>Car_NW;
		private void RaiseCar_NW(float dist) {
			if(Car_NW != null)
				Car_NW.Invoke(dist);
		}

		private void Start() {
            car = GetComponent<CarController>();
        }

		// Update is called once per frame
		private void Update () {
            RaiseSpeedSensor(car.NormalizedSpeed);
			RaiseSteerSensor(car.TurnForce);
			RaiseDriveSensor(car.DriveForce);

            NorthSensor();
			NorthEastSensor();
			EastSensor();
			SouthEastSensor();
			SouthSensor();
			SouthWestSensor();
			WestSensor();
			NorthWestSensor();
		}

		/// <summary>
		/// The function creates a raycast from the transform center to the sensor direction.
		/// Check hit with the tag "wall" and "car" and raise the relevant Action call, passing a float from 0 to 1.
		/// </summary>
		private void NorthSensor() {
			Debug.DrawRay(transform.position, transform.forward * sensorLength, Color.green);

			RaycastHit hit;
			if (Physics.Raycast(transform.position, transform.forward, out hit, sensorLength)) {
				if (hit.collider.gameObject.tag == "wall")
					RaiseWall_N(hit.distance / sensorLength);
				else if (hit.collider.gameObject.tag == "car")
					RaiseCar_N(hit.distance / sensorLength);
			}
		}
		
		private void NorthEastSensor() {
			Debug.DrawRay(transform.position, (transform.forward + transform.right) * sensorLength, Color.green);

			RaycastHit hit;
			if (Physics.Raycast(transform.position, (transform.forward + transform.right), out hit, sensorLength)) {
				if (hit.collider.gameObject.tag == "wall")
					RaiseWall_NE(hit.distance / sensorLength);
				else if (hit.collider.gameObject.tag == "car")
					RaiseCar_NE(hit.distance / sensorLength);
			}
		}

		private void EastSensor() {
			Debug.DrawRay(transform.position, transform.right * sensorLength, Color.green);

			RaycastHit hit;
			if (Physics.Raycast(transform.position, transform.right, out hit, sensorLength)) {
				if (hit.collider.gameObject.tag == "wall")
					RaiseWall_E(hit.distance / sensorLength);
				else if (hit.collider.gameObject.tag == "car")
					RaiseCar_E(hit.distance / sensorLength);
			}
		}

		private void SouthEastSensor() {
			Debug.DrawRay(transform.position, (-transform.forward + transform.right) * sensorLength, Color.green);

			RaycastHit hit;
			if (Physics.Raycast(transform.position, (-transform.forward + transform.right), out hit, sensorLength)) {
				if (hit.collider.gameObject.tag == "wall")
					RaiseWall_SE(hit.distance / sensorLength);
				else if (hit.collider.gameObject.tag == "car")
					RaiseCar_SE(hit.distance / sensorLength);
			}
		}

		private void SouthSensor() {
			Debug.DrawRay(transform.position, -transform.forward * sensorLength, Color.green);

			RaycastHit hit;
			if (Physics.Raycast(transform.position, -transform.forward, out hit, sensorLength)) {
				if (hit.collider.gameObject.tag == "wall")
					RaiseWall_S(hit.distance / sensorLength);
				else if (hit.collider.gameObject.tag == "car")
					RaiseCar_S(hit.distance / sensorLength);
			}
		}

		private void SouthWestSensor() {
			Debug.DrawRay(transform.position, -(transform.forward + transform.right) * sensorLength, Color.green);

			RaycastHit hit;
			if (Physics.Raycast(transform.position, -(transform.forward + transform.right), out hit, sensorLength)) {
				if (hit.collider.gameObject.tag == "wall")
					RaiseWall_SW(hit.distance / sensorLength);
				else if (hit.collider.gameObject.tag == "car")
					RaiseCar_SW(hit.distance / sensorLength);
			}
		}

		private void WestSensor() {
			Debug.DrawRay(transform.position, -transform.right * sensorLength, Color.green);

			RaycastHit hit;
			if (Physics.Raycast(transform.position, -transform.right, out hit, sensorLength)) {
				if (hit.collider.gameObject.tag == "wall")
					RaiseWall_W(hit.distance / sensorLength);
				else if (hit.collider.gameObject.tag == "car")
					RaiseCar_W(hit.distance / sensorLength);
			}
		}

		private void NorthWestSensor() {
			Debug.DrawRay(transform.position, (transform.forward - transform.right) * sensorLength, Color.green);

			RaycastHit hit;
			if (Physics.Raycast(transform.position, (transform.forward - transform.right), out hit, sensorLength)) {
				if (hit.collider.gameObject.tag == "wall")
					RaiseWall_NW(hit.distance / sensorLength);
				else if (hit.collider.gameObject.tag == "car")
					RaiseCar_NW(hit.distance / sensorLength);
			}
		}

	}
}