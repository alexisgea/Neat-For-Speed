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

		// getting a ref to the car
        private void Start() {
            car = GetComponent<CarBehaviour>();
        }

		// Update is called once per frame
		private void Update () {
            
			// udpating car behaviour sensors
			SteerSensor = car.TurnForce;
			DriveSensor = car.DriveForce;
			SpeedSensor = car.NormalizedSpeed;
            DirectionSensor = car.Direction;

			// updating external sensors
            NorthSensor();
			NorthEastSensor();
			EastSensor();
			SouthEastSensor();
			SouthSensor();
			SouthWestSensor();
			WestSensor();
			NorthWestSensor();

			// updating fitness
            DistanceDriven += car.DistanceDriven;
        }

		// This function (and all similar ones) creates a raycast from the transform center to the sensor direction.
		// Check hit with the tag "wall" and "car".
		private void NorthSensor() {
			Debug.DrawRay(transform.position, transform.forward * sensorLength, Color.green);

			RaycastHit[] hits;

			hits = Physics.RaycastAll(transform.position, transform.forward, sensorLength);

			Wall_N = 1f;
			Car_N = 1f;

			for (int i = 0; i < hits.Length; i++) {
				RaycastHit hit = hits[i];
				
				if (hit.collider.gameObject.tag == "wall")
					Wall_N = hit.distance / sensorLength;

                else if (hit.collider.gameObject.tag == "car")
					Car_N = hit.distance / sensorLength;
			}						
		}
		
		private void NorthEastSensor() {
			Debug.DrawRay(transform.position, (transform.forward + transform.right) * sensorLength, Color.green);

			RaycastHit[] hits;

			hits = Physics.RaycastAll(transform.position, (transform.forward + transform.right), sensorLength);

			Wall_NE = 1f;
			Car_NE = 1f;

			for (int i = 0; i < hits.Length; i++) {
				RaycastHit hit = hits[i];
				
				if (hit.collider.gameObject.tag == "wall")
					Wall_NE = hit.distance / sensorLength;

                else if (hit.collider.gameObject.tag == "car")
					Car_NE = hit.distance / sensorLength;
			}	
		}

		private void EastSensor() {
			//Debug.DrawRay(transform.position, transform.right * sensorLength, Color.green);

			RaycastHit[] hits;

			hits = Physics.RaycastAll(transform.position, transform.right, sensorLength);

			Wall_E = 1f;
			Car_E = 1f;

			for (int i = 0; i < hits.Length; i++) {
				RaycastHit hit = hits[i];
				
				if (hit.collider.gameObject.tag == "wall")
					Wall_E = hit.distance / sensorLength;

                else if (hit.collider.gameObject.tag == "car")
					Car_E = hit.distance / sensorLength;
			}
		}

		private void SouthEastSensor() {
			//Debug.DrawRay(transform.position, (-transform.forward + transform.right) * sensorLength, Color.green);

			RaycastHit[] hits;

			hits = Physics.RaycastAll(transform.position, (-transform.forward + transform.right), sensorLength);

			Wall_SE = 1f;
			Car_SE = 1f;

			for (int i = 0; i < hits.Length; i++) {
				RaycastHit hit = hits[i];
				
				if (hit.collider.gameObject.tag == "wall")
					Wall_SE = hit.distance / sensorLength;

                else if (hit.collider.gameObject.tag == "car")
					Car_SE = hit.distance / sensorLength;
			}
		}

		private void SouthSensor() {
			//Debug.DrawRay(transform.position, -transform.forward * sensorLength, Color.green);

			RaycastHit[] hits;

			hits = Physics.RaycastAll(transform.position, -transform.forward, sensorLength);

			Wall_S = 1f;
			Car_S = 1f;

			for (int i = 0; i < hits.Length; i++) {
				RaycastHit hit = hits[i];
				
				if (hit.collider.gameObject.tag == "wall")
					Wall_S = hit.distance / sensorLength;

                else if (hit.collider.gameObject.tag == "car")
					Car_S = hit.distance / sensorLength;
			}
		}

		private void SouthWestSensor() {
			//Debug.DrawRay(transform.position, -(transform.forward + transform.right) * sensorLength, Color.green);

			RaycastHit[] hits;

			hits = Physics.RaycastAll(transform.position, -(transform.forward + transform.right), sensorLength);

			Wall_SW = 1f;
			Car_SW = 1f;

			for (int i = 0; i < hits.Length; i++) {
				RaycastHit hit = hits[i];
				
				if (hit.collider.gameObject.tag == "wall")
					Wall_SW = hit.distance / sensorLength;

                else if (hit.collider.gameObject.tag == "car")
					Car_SW = hit.distance / sensorLength;
			}
		}

		private void WestSensor() {
			//Debug.DrawRay(transform.position, -transform.right * sensorLength, Color.green);

			RaycastHit[] hits;

			hits = Physics.RaycastAll(transform.position, -transform.right, sensorLength);

			Wall_W = 1f;
			Car_W = 1f;

			for (int i = 0; i < hits.Length; i++) {
				RaycastHit hit = hits[i];
				
				if (hit.collider.gameObject.tag == "wall")
					Wall_W = hit.distance / sensorLength;

                else if (hit.collider.gameObject.tag == "car")
					Car_W = hit.distance / sensorLength;
			}
		}

		private void NorthWestSensor() {
			Debug.DrawRay(transform.position, (transform.forward - transform.right) * sensorLength, Color.green);

			RaycastHit[] hits;

			hits = Physics.RaycastAll(transform.position, (transform.forward - transform.right), sensorLength);

			Wall_NW = 1f;
			Car_NW = 1f;

			for (int i = 0; i < hits.Length; i++) {
				RaycastHit hit = hits[i];
				
				if (hit.collider.gameObject.tag == "wall")
					Wall_NW = hit.distance / sensorLength;

                else if (hit.collider.gameObject.tag == "car")
					Car_NW = hit.distance / sensorLength;

			}
		}

	}
}