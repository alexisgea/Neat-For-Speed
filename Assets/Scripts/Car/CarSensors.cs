using System;
using UnityEngine;

namespace nfs.car {

	///<summary>
	/// Car sensor manager.
	/// Creates sensors, checks them each update and store the distance.
	/// There are a total of 19 sensors. 8 for the wall tag and 8 for the car tag and 3 for the car controls
	/// The sensors default size is 5 from the center of the car.
	/// The sensors are named as per the cardinal position to which they point (N, NE, E, SE, S, SW, W, NW).
	/// Sensor values are public attributes as float from 0 to 1.
	///</summary>
	[RequireComponent (typeof (CarBehaviour))]
	public class CarSensors : MonoBehaviour {

		private int sensorLength = 10;

        // Car internal control sensors
        private CarBehaviour car;

        public float SpeedSensor {private set; get;}
        public float DirectionSensor {private set; get;}
        public float SteerSensor {private set; get;}
		public float DriveSensor {private set; get;}

		// Wall sensors
		public float Wall_N {private set; get;}
		public float Wall_NE {private set; get;}
		public float Wall_E {private set; get;}
		public float Wall_SE {private set; get;}
		public float Wall_S {private set; get;}
		public float Wall_SW {private set; get;}
		public float Wall_W {private set; get;}
		public float Wall_NW {private set; get;}

		// Car sensors
		public float Car_N {private set; get;}
		public float Car_NE {private set; get;}
		public float Car_E {private set; get;}
		public float Car_SE {private set; get;}
		public float Car_S {private set; get;}
		public float Car_SW {private set; get;}
		public float Car_W {private set; get;}
		public float Car_NW {private set; get;}

		// fitness sensor
		public float DistanceDriven { private set; get; }

        private void Start() {
            car = GetComponent<CarBehaviour>();
        }

		// Update is called once per frame
		private void Update () {
            
			SteerSensor = car.TurnForce;
			DriveSensor = car.DriveForce;
			SpeedSensor = car.NormalizedSpeed;
            DirectionSensor = car.Direction;

            NorthSensor();
			NorthEastSensor();
			EastSensor();
			SouthEastSensor();
			SouthSensor();
			SouthWestSensor();
			WestSensor();
			NorthWestSensor();

            DistanceDriven += car.DistanceDriven;
        }

		/// <summary>
		/// The function creates a raycast from the transform center to the sensor direction.
		/// Check hit with the tag "wall" and "car".
		/// </summary>
		private void NorthSensor() {
			Debug.DrawRay(transform.position, transform.forward * sensorLength, Color.green);

			RaycastHit hit;
			if (Physics.Raycast(transform.position, transform.forward, out hit, sensorLength)) {
				if (hit.collider.gameObject.tag == "wall")
					Wall_N = hit.distance / sensorLength;
				else
                    Wall_N = 1f;

                if (hit.collider.gameObject.tag == "car")
					Car_N = hit.distance / sensorLength;
				else
                    Car_N = 1f;
			}
		}
		
		private void NorthEastSensor() {
			Debug.DrawRay(transform.position, (transform.forward + transform.right) * sensorLength, Color.green);

			RaycastHit hit;
			if (Physics.Raycast(transform.position, (transform.forward + transform.right), out hit, sensorLength)) {
				if (hit.collider.gameObject.tag == "wall")
					Wall_NE = hit.distance / sensorLength;
				else
                    Wall_NE = 1f;

                if (hit.collider.gameObject.tag == "car")
					Car_NE = hit.distance / sensorLength;
				else
                    Car_NE = 1f;
			}
		}

		private void EastSensor() {
			//Debug.DrawRay(transform.position, transform.right * sensorLength, Color.green);

			RaycastHit hit;
			if (Physics.Raycast(transform.position, transform.right, out hit, sensorLength)) {
				if (hit.collider.gameObject.tag == "wall")
					Wall_E = hit.distance / sensorLength;
				else
                    Wall_E = 1f;

				if (hit.collider.gameObject.tag == "car")
					Car_E = hit.distance / sensorLength;
				else
                    Car_E = 1f;
			}
		}

		private void SouthEastSensor() {
			//Debug.DrawRay(transform.position, (-transform.forward + transform.right) * sensorLength, Color.green);

			RaycastHit hit;
			if (Physics.Raycast(transform.position, (-transform.forward + transform.right), out hit, sensorLength)) {
				if (hit.collider.gameObject.tag == "wall")
					Wall_SE = hit.distance / sensorLength;
				else
                    Wall_SE = 1f;

				if (hit.collider.gameObject.tag == "car")
					Car_SE = hit.distance / sensorLength;
				else
                    Car_SE = 1f;
			}
		}

		private void SouthSensor() {
			//Debug.DrawRay(transform.position, -transform.forward * sensorLength, Color.green);

			RaycastHit hit;
			if (Physics.Raycast(transform.position, -transform.forward, out hit, sensorLength)) {
				if (hit.collider.gameObject.tag == "wall")
					Wall_S = hit.distance / sensorLength;
				else
                    Wall_S = 1f;

				if (hit.collider.gameObject.tag == "car")
					Car_S = hit.distance / sensorLength;
				else
                    Car_S = 1f;
			}
		}

		private void SouthWestSensor() {
			//Debug.DrawRay(transform.position, -(transform.forward + transform.right) * sensorLength, Color.green);

			RaycastHit hit;
			if (Physics.Raycast(transform.position, -(transform.forward + transform.right), out hit, sensorLength)) {
				if (hit.collider.gameObject.tag == "wall")
					Wall_SW = hit.distance / sensorLength;
				else
                    Wall_SW = 1f;
					
				if (hit.collider.gameObject.tag == "car")
					Car_SW = hit.distance / sensorLength;
				else
                    Car_SW = 1f;
			}
		}

		private void WestSensor() {
			//Debug.DrawRay(transform.position, -transform.right * sensorLength, Color.green);

			RaycastHit hit;
			if (Physics.Raycast(transform.position, -transform.right, out hit, sensorLength)) {
				if (hit.collider.gameObject.tag == "wall")
					Wall_W = hit.distance / sensorLength;
				else
                    Wall_W = 1f;

				if (hit.collider.gameObject.tag == "car")
					Car_W = hit.distance / sensorLength;
				else
                    Car_W = 1f;
			}
		}

		private void NorthWestSensor() {
			Debug.DrawRay(transform.position, (transform.forward - transform.right) * sensorLength, Color.green);

			RaycastHit hit;
			if (Physics.Raycast(transform.position, (transform.forward - transform.right), out hit, sensorLength)) {
				if (hit.collider.gameObject.tag == "wall")
					Wall_NW = hit.distance / sensorLength;
				else
                    Wall_NW = 1f;

				if (hit.collider.gameObject.tag == "car")
					Car_NW = hit.distance / sensorLength;
				else
                    Car_NW = 1f;
			}
		}

	}
}