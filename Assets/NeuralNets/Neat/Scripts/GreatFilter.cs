using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// The correct NEAT implementation has not yet been done
namespace nfs.nets.neat {

    public enum NeuronType { intput, output, bias, hidden }
    public enum NeuronActivation { linear, sigmoid, gaussian}

    public class GreatFilter {
		private uint innovationMarker = 0;
        public uint InnovationMarker { 
			get {
                innovationMarker += 1;
                return innovationMarker;
            } 
		}

		
    }

}
