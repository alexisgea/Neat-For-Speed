using System.Collections.Generic;
using UnityEngine;

namespace nfs.car {

	/// <summary>
	/// Implementation of the evolution car to specifically train neural net controlling cars.
	/// </summary>
	public class CarLayeredNetTrainer : nets.layered.Trainer {


        // world and race tracks references
        //[SerializeField] private Transform world;
        [SerializeField] private GameObject[] tracks;
        private GameObject raceTrack;
        [SerializeField] private Vector3 startPosition = Vector3.zero;
        [SerializeField] private float startPositioSpread = 2f;

        protected override void InitializeWorld() {
            //create a new racetrack form the list
            raceTrack = GameObject.Instantiate(tracks[Random.Range(0, tracks.Length-1)]);
            raceTrack.transform.SetParent(worldGroup);
        }


        // protected override void InitializePopulation () {

        // }

        protected override void RefreshWorld() {
            GameObject.Destroy(raceTrack);
            InitializeWorld();
        }

		/// <summary>
		/// Calculates the start position. of each car.
		/// </summary>
        protected override Vector3 CalculateStartPosition(int i) {
            return startPosition + new Vector3( (i%2 -0.5f) * startPositioSpread, 0, - Mathf.Floor(i/2f) * startPositioSpread);
        }

        protected override Quaternion CalculateStartOrientation (int i) {
            return Quaternion.identity;
        }

	}

}
