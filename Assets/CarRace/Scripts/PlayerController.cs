using UnityEngine;

namespace nfs.car {

	///<summary>
	/// Basic player input controller to drive a car.
	///</summary>
	[RequireComponent(typeof(CarBehaviour))]
	public class PlayerController : MonoBehaviour {

		private CarBehaviour car;

		private void Start() {
			car = GetComponent<CarBehaviour>();
		}

		// axis input needs to go through every frame
		// it will be the case for AI and it is important for the current implentation of the car controller
		private void Update () {
			car.Drive(Input.GetAxis("Vertical"));
			car.Turn(Input.GetAxis("Horizontal"));
        }

	}
}