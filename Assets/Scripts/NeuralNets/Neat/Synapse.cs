using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// The correct NEAT implementation has not yet been done
namespace nfs.neat {
	public class Synapse {
        
		public uint InnovationMaker { private set; get; }
		public uint SourceNeuronId { private set; get; }
		public uint TargetNeuronId { private set; get; }
		public float Weight { set; get; }
		


        public Synapse (uint innovationMaker, uint sourceNeuronId, uint targetNeuronId) {
            InnovationMaker = innovationMaker;
            SourceNeuronId = sourceNeuronId;
            TargetNeuronId = targetNeuronId;
            Weight = Random.Range(0f, 1f);

        }

		public Synapse (uint innovationMaker,uint sourceNeuronId, uint targetNeuronId, float weight) {
            InnovationMaker = innovationMaker;
            SourceNeuronId = sourceNeuronId;
            TargetNeuronId = targetNeuronId;
            Weight = weight;
        }

	}
}
