using System.Collections.Generic;
using UnityEngine;

namespace nfs.car {

	/// <summary>
	/// Implementation of the evolution car to specifically train a layered neural net controlling cars.
	/// </summary>
	public class CarLayeredNetTrainer : nets.layered.Trainer {

        // world and race tracks references and variables
        [SerializeField] private GameObject[] tracks;
        private GameObject raceTrack;
        [SerializeField] private Vector3 startPosition = Vector3.zero;
        [SerializeField] private float startPositioSpread = 2f;
		[SerializeField] private float maxDistanceFitness = 500f;

		// we create a track for the cars
        protected override void InitialiseWorld() {
            //create a new racetrack form the list
            raceTrack = GameObject.Instantiate(tracks[Random.Range(0, tracks.Length-1)]);
            raceTrack.transform.SetParent(worldGroup);
        }

		// we set some values of the specific implementation of the controller
		protected override void PostInitialisation() {
			for (int i = 0; i < HostPopulation.Length; i++) {
				HostPopulation [i].GetComponent<CarLayeredNetController> ().MaxDistFitness = maxDistanceFitness;
				HostPopulation [i].GetComponent<CarLayeredNetController> ().StartTime = GenerationStartTime;
			}
		}

		// we destroy the track and reuse the world initialisation to get a new one
        protected override void RefreshWorld() {
            GameObject.Destroy(raceTrack);
            InitialiseWorld();
        }

		// we set the same start time to every one.
		protected override void RefreshHosts() {
			for (int i = 0; i < HostPopulation.Length; i++) {
				HostPopulation [i].GetComponent<CarLayeredNetController> ().StartTime = GenerationStartTime;
			}
		}

		// we calculate a position to put cars one behind another like in a race start
        protected override Vector3 CalculateStartPosition(int i) {
            return startPosition + new Vector3( (i%2 -0.5f) * startPositioSpread, 0, - Mathf.Floor(i/2f) * startPositioSpread);
        }

	}

}
