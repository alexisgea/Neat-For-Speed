using UnityEngine;

namespace nfs.tools {

	/// <summary>
	/// Basic control for a spectator camera during training sessions.
	/// </summary>
	[RequireComponent(typeof(Camera))]
	public class SpectatorCam : MonoBehaviour {

		// camera constants
		[SerializeField] private const float rotationSpeed = 100;
		[SerializeField] private const float normalSpeed = 25;
		[SerializeField] private const float fastSpeed = 50;
		private const float sensitivityX = 15F;
		private const float sensitivityY = 15F;
		private const float minimumY = -60F;
		private const float maximumY = 60F;

		// cam motion variables
		private float camSpeed = normalSpeed;
		private float rotationY = 0F;


		/// <summary>
		/// Monobehaviour update.
		/// </summary>
		private void Update () {
			CheckSpeedSwitch();
			CheckTranslateCam();
			CheckRotateCam();
		}

		/// <summary>
		/// Checks if the player is pressing the sprint movement key for the cam.
		/// </summary>
		private void CheckSpeedSwitch() {
			if (Input.GetButtonDown("SpeedSwitch")) {
				camSpeed = fastSpeed;

			} else if (Input.GetButtonUp("SpeedSwitch")){
				camSpeed = normalSpeed;
			}
		}

		/// <summary>
		/// Checks the player input for moving the car around.
		/// </summary>
		private void CheckTranslateCam() {
			if(Input.GetAxis("Horizontal") != 0) {
				transform.position += transform.right * Input.GetAxis("Horizontal") * camSpeed * Time.deltaTime;
			}

			if(Input.GetAxis("Vertical") != 0) {
				transform.position += transform.forward * Input.GetAxis("Vertical") * camSpeed * Time.deltaTime;
			}

			if(Input.GetAxis("Yaxis") != 0) {
				transform.position += transform.up * Input.GetAxis("Yaxis") * camSpeed * Time.deltaTime;
			}
		}
			
		/// <summary>
		/// Checks the player input for rotating the cam.
		/// script inspired from here for simplicity sake
		/// https://forum.unity3d.com/threads/looking-with-the-mouse.109250/
		/// </summary>
		private void CheckRotateCam() {
			if (Input.GetMouseButton(2)) {

				float rotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * sensitivityX;
				
				rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
				rotationY = Mathf.Clamp (rotationY, minimumY, maximumY);
				
				transform.localEulerAngles = new Vector3(-rotationY, rotationX, 0);
			}
		}

	}

}
