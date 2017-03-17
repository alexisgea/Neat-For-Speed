using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using nfs.controllers;

namespace nfs.gui {

	[RequireComponent(typeof(Camera))]
	public class SpectatorCam : MonoBehaviour {

		[SerializeField] const float rotationSpeed = 100;
		[SerializeField] const float normalSpeed = 25;
		[SerializeField] const float fastSpeed = 50;
		float camSpeed = normalSpeed;

		float sensitivityX = 15F;
		float sensitivityY = 15F;
		float minimumY = -60F;
		float maximumY = 60F;
		float rotationY = 0F;

		
		// Update is called once per frame
		void Update () {
			CheckSpeedSwitch();
			CheckTranslateCam();
			CheckRotateCam();
			CheckFocusNetwork();
		}

		void CheckSpeedSwitch() {
			if (Input.GetButtonDown("SpeedSwitch")) {
				camSpeed = fastSpeed;
			} else if (Input.GetButtonUp("SpeedSwitch")){
				camSpeed = normalSpeed;
			}
		}

		void CheckTranslateCam() {
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

		// script inspired from here for simplicity sake
		// https://forum.unity3d.com/threads/looking-with-the-mouse.109250/
		void CheckRotateCam() {
			if (Input.GetMouseButton(2)) {

				float rotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * sensitivityX;
				
				rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
				rotationY = Mathf.Clamp (rotationY, minimumY, maximumY);
				
				transform.localEulerAngles = new Vector3(-rotationY, rotationX, 0);
				
			}
		}

		void CheckFocusNetwork() { // add an overall "if not Input.GetMouseButton(1)"
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
