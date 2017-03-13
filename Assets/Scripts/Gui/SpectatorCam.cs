using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using nfs.layered;
using nfs.controllers;

namespace nfs.gui {

	[RequireComponent(typeof(Camera))]
	public class SpectatorCam : MonoBehaviour {

		[SerializeField] const float camNormalSpeed = 25;
		[SerializeField] const float camFastSpeed = 50;
		float camSpeed = camNormalSpeed;

		// Use this for initialization
		void Start () {
			
		}
		
		// Update is called once per frame
		void Update () {
			CheckSpeedSwitch();
			CheckTranslateCam();
			CheckFocusNetwork();
		}

		void CheckSpeedSwitch() {
			if (Input.GetButtonDown("SpeedSwitch")) {
				camSpeed = camFastSpeed;
			} else if (Input.GetButtonUp("SpeedSwitch")){
				camSpeed = camNormalSpeed;
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

		void CheckFocusNetwork() {
			if (Input.GetMouseButtonDown(0)) {
				RaycastHit hitInfo = new RaycastHit();
				bool hit = Physics.Raycast(GetComponent<Camera>().ScreenPointToRay(Input.mousePosition), out hitInfo);

				if (hit && hitInfo.transform.tag == "car" && hitInfo.transform.GetComponent<LayeredNetController>() != null)	{
					LayeredNetwork focusNet = hitInfo.transform.GetComponent<LayeredNetController>().GetLayeredNet();
					FindObjectOfType<NetworkVisualiser>().AssignFocusNetwork(focusNet);
				}
			} else if (Input.GetMouseButtonDown(1)) {
				FindObjectOfType<NetworkVisualiser>().ClearCurrentVisualisation();
			}
		}

	}

}
