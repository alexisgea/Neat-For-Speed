using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using nfs.controllers;

namespace nfs.gui {

	/// <summary>
	/// Basic control for a spectator camera during training sessions.
	/// </summary>
	[RequireComponent(typeof(Camera))]
	public class SpectatorCam : MonoBehaviour {

		// camera specific variables
		[SerializeField] private const float rotationSpeed = 100;
		[SerializeField] private const float normalSpeed = 25;
		[SerializeField] private const float fastSpeed = 50;

		private float camSpeed = normalSpeed;
		private float sensitivityX = 15F;
		private float sensitivityY = 15F;
		private float minimumY = -60F;
		private float maximumY = 60F;
		private float rotationY = 0F;

		
		// Update is called once per frame
		private void Update () {
			CheckSpeedSwitch();
			CheckTranslateCam();
			CheckRotateCam();
			CheckFocusNetwork();
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

		/// <summary>
		/// Checks if the player clicked on a network to focus it.
		/// </summary>
		private void CheckFocusNetwork() { // add an overall "if not Input.GetMouseButton(1)"
			if (Input.GetMouseButtonDown(0)) {
				RaycastHit hitInfo = new RaycastHit();
				bool hit = Physics.Raycast(GetComponent<Camera>().ScreenPointToRay(Input.mousePosition), out hitInfo);

				if (hit && hitInfo.transform.tag == "car" && hitInfo.transform.GetComponent<CarLayeredNetController>() != null)	{
					layered.NeuralNet focusNet = hitInfo.transform.GetComponent<CarLayeredNetController>().NeuralNet;
					FindObjectOfType<layered.Visualiser>().AssignFocusNetwork(focusNet);
				}
			} else if (Input.GetMouseButtonDown(1)) { // change to mouse wheel click if to many errors
				FindObjectOfType<layered.Visualiser>().ClearCurrentVisualisation();
			}
		}

	}

}
