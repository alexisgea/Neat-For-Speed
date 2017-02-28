using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

// The correct NEAT implementation has not yet been done
namespace nfs.neat {
	public class Neuron {

		public uint InnovationMaker { private set; get; }
		public NeuronType Type { private set; get; }
		public NeuronActivation Activation { private set; get; }
		public float LastState { set; get; }
		public float DelayedInput { set; get; }

        public Neuron (uint innovationMaker, NeuronType type, NeuronActivation activation) {
            InnovationMaker = innovationMaker;
            Type = type;
            Activation = activation;
        }

		public Neuron (uint innovationMaker, NeuronType type) {
            InnovationMaker = innovationMaker;
            Type = type;
            Activation = (NeuronActivation)Random.Range(0, System.Enum.GetValues(typeof(NeuronActivation)).Length-1);
        }
    }
}
