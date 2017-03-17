using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace nfs.trainers{
	public abstract class NetTrainer : MonoBehaviour {

		protected abstract float CalculateFitness ();

	}

}
