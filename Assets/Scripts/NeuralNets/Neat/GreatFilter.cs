using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace nfs.neat {

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
