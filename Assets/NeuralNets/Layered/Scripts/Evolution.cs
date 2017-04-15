using UnityEngine;
using System.Collections;
using nfs.tools;

namespace nfs.nets.layered {

	///<summary>
	/// All the different time of synapses mutation.
	///</summary>
	public enum MutationType { additive, multiply, reverse, replace, nullify }

	/// <summary>
	/// Evolution static class containing functions for mutating a Layered neural network.
	/// </summary>
	public static class Evolution {

		/// <summary>
        /// Creates the mutated offspring.
        /// </summary>
        /// <returns>The mutated offspring.</returns>
        /// <param name="neuralNet">Neural net.</param>
        /// <param name="mutateCoef">Mutate coef.</param>
		public static Network CreateMutatedOffspring(Network neuralNet, string newNetId, int mutateCoef,
													bool hiddenLayerNbMutation, float hiddenLayerNbMutationRate,
													bool hiddenNbMutation, float hiddenMbMutationRate,
													float synapsesMutationRate, float synapsesMutationRange) {

            int[] hiddenLayersSizes = neuralNet.HiddenLayersSizes;
            Matrix[] synapses = neuralNet.GetSynapsesClone();

			int synapsesChanged = 0;
			int originalSynapseNb = neuralNet.NumberOfSynapses;

            // TODO LATER
            // Implemenet here mutation for new input
            // have a fixe array of sensors and an array of int containing the sensor indx to read from
            // mutate this array of int

            // mutate number of hidden layers
            if(hiddenLayerNbMutation && Random.value < hiddenLayerNbMutationRate){
                hiddenLayersSizes = MutateNbOfHiddenLayer(neuralNet, hiddenLayersSizes, ref synapses);
			}

            // mutated number of neurons in hidden layers
            if(hiddenNbMutation && Random.value < hiddenMbMutationRate){
                hiddenLayersSizes = MutateNbOfHiddenLayerNeurons(neuralNet, hiddenLayersSizes, ref synapses);	
			}

            // mutate synapses values
            synapses = MutateSynapsesValues(neuralNet, synapses, synapsesMutationRate, synapsesMutationRange, ref synapsesChanged);

            int[] layerSizes = new int[hiddenLayersSizes.Length + 2];
            layerSizes[0] = neuralNet.InputSize;
            layerSizes[layerSizes.Length-1] = neuralNet.OutputSize;

            for(int i=1; i<layerSizes.Length-1; i++) {
                layerSizes[i] = hiddenLayersSizes[i-1];
            }

            Network mutatedOffspring = new Network(layerSizes, newNetId);
			mutatedOffspring.Ancestors = ExtendLineage (neuralNet.Ancestors, neuralNet.Id);
			mutatedOffspring.SpeciesLineage = neuralNet.SpeciesLineage;
            mutatedOffspring.InsertSynapses(synapses);

			synapsesChanged += Mathf.Abs(mutatedOffspring.NumberOfSynapses - originalSynapseNb);
			float synapsesChangedRatio = (float)(synapsesChanged / mutatedOffspring.NumberOfSynapses);

			if(neuralNet.Colorisation != null) { // useless says unity?
				Color newColor = MutateColor(neuralNet.Colorisation, synapsesChangedRatio);
				mutatedOffspring.Colorisation = newColor;
			}

            return mutatedOffspring;
        }


		/// <summary>
		/// Mutates the nb of hidden layer.
		/// </summary>
		/// <param name="neuralNet">Neural net.</param>
		/// <param name="hiddenLayersSizes">Hidden layers sizes.</param>
		/// <param name="synapses">Synapses.</param>
        private static int[] MutateNbOfHiddenLayer(Network neuralNet, int[] hiddenLayersSizes, ref Matrix[] synapses) {
            
			if (Random.value < 0.5f && hiddenLayersSizes.Length > 1) { // random to get positive vs negative value
				hiddenLayersSizes = RedimentionLayersNb(hiddenLayersSizes, -1);

				synapses = RedimentionLayersNb(synapses, -1);
				synapses[synapses.Length - 1] = Matrix.Redimension(synapses[synapses.Length - 1],
												hiddenLayersSizes[hiddenLayersSizes.Length - 1], neuralNet.OutputSize);

			} else {
				hiddenLayersSizes = RedimentionLayersNb(hiddenLayersSizes, +1);
				hiddenLayersSizes[hiddenLayersSizes.Length - 1] = neuralNet.OutputSize;

				synapses = RedimentionLayersNb(synapses, +1);
				synapses[synapses.Length - 1] = new Matrix(hiddenLayersSizes[hiddenLayersSizes.Length - 1],
												neuralNet.OutputSize).SetAsSynapse();
            }

			return hiddenLayersSizes;
        }

		/// <summary>
		/// Mutates the nb of hidden layer neurons.
		/// </summary>
		/// <param name="neuralNet">Neural net.</param>
		/// <param name="hiddenLayersSizes">Hidden layers sizes.</param>
		/// <param name="synapses">Synapses.</param>
        private static int[] MutateNbOfHiddenLayerNeurons(Network neuralNet, int[] hiddenLayersSizes, ref Matrix[] synapses) {

			int layerNb = Random.Range(0, hiddenLayersSizes.Length - 1);
			if (Random.value < 0.5f && hiddenLayersSizes[layerNb] > 1) { // random to get positive vs negative value
				hiddenLayersSizes[layerNb] -= 1;
			} else {
				hiddenLayersSizes[layerNb] += 1;
			}
			// need to use the previous synapses values here as we might be going from/to oustide of the hidden layers
			synapses[layerNb] = Matrix.Redimension(synapses[layerNb], synapses[layerNb].I, hiddenLayersSizes[layerNb]);
			synapses[layerNb+1] = Matrix.Redimension(synapses[layerNb+1], hiddenLayersSizes[layerNb], synapses[layerNb+1].J);

			return hiddenLayersSizes;
        }

		/// <summary>
		/// Mutates the synapses values.
		/// </summary>
		/// <param name="neuralNet">Neural net.</param>
		/// <param name="synapses">Synapses.</param>
        private static Matrix[] MutateSynapsesValues(Network neuralNet, Matrix[] synapses,
													float synapsesMutationRate, float synapsesMutationRange,
													ref int synapsesChaned) {
            
			for (int n=0; n<synapses.Length; n++) {
                for (int i = 0; i < synapses[n].I; i++) {
                    for (int j=0; j < synapses[n].J; j++) {

                        if (Random.value < synapsesMutationRate) {
                            MutationType type = (MutationType)Random.Range(0, System.Enum.GetValues(typeof(MutationType)).Length-1);
                            float mutatedValue = synapses[n].GetValue(i, j);

                            switch(type) {
                                case MutationType.additive:
                                    mutatedValue += Random.Range(-synapsesMutationRange, synapsesMutationRange);
                                    break;

                                case MutationType.multiply:
                                    mutatedValue *= Random.Range(1f - 5f *synapsesMutationRange, 1f + 5f * synapsesMutationRange);
                                    break;

                                case MutationType.reverse:
                                    mutatedValue *= -1;
                                    break;

                                case MutationType.replace:
                                    float weightRange = Matrix.StandardSynapseRange(synapses[n].J);
                                    mutatedValue = Random.Range(-weightRange, weightRange);
                                    break;

                                case MutationType.nullify:
                                    mutatedValue = 0f;
                                    break;

                                default:
                                    Debug.LogWarning("Unknown weight mutation type. Doing nothing.");
                                    break;
                            }

							synapsesChaned += 1;

                            synapses[n].SetValue(i, j, mutatedValue);
							
                        }
                    }
                }
            }

			return synapses;
        }

		/// <summary>
		/// Extends the lineage.
		/// </summary>
		/// <returns>The lineage.</returns>
		/// <param name="currentLineage">Current lineage.</param>
		/// <param name="parentId">Parent identifier.</param>
		public static string[] ExtendLineage (string[] currentLineage, string parentId) {
			string[] extendedLineage;

			if(currentLineage == null) {
				extendedLineage = new string[1] { parentId };

			} else {
				extendedLineage = new string[currentLineage.Length + 1];
				for (int i = 0; i < currentLineage.Length; i++) {
					extendedLineage[i] = currentLineage[i];
				}
				
				extendedLineage [extendedLineage.Length - 1] = parentId;
				
			}

			return extendedLineage;
        }

		/// <summary>
		/// Redimension an array of in for the hidden layers.
		/// </summary>
		/// <returns>The layers nb.</returns>
		/// <param name="currentLayers">Current layers.</param>
		/// <param name="sizeMod">Size mod.</param>
		public static int[] RedimentionLayersNb (int[] currentLayers, int sizeMod) {

			int[] newLayers = new int[currentLayers.Length + sizeMod];
			for (int i = 0; i < Mathf.Min(currentLayers.Length, newLayers.Length); i++) {
				newLayers[i] = currentLayers[i];
			}

			return newLayers;
		}

		/// <summary>
		/// Redimension an array of matrix for the synapses.
		/// </summary>
		/// <returns>The layers nb.</returns>
		/// <param name="currentLayers">Current layers.</param>
		/// <param name="sizeMod">Size mod.</param>
        public static Matrix[] RedimentionLayersNb (Matrix[] currentLayers, int sizeMod) {

            Matrix[] newLayers = new Matrix[currentLayers.Length + sizeMod];
            for (int i = 0; i < Mathf.Min(currentLayers.Length, newLayers.Length); i++) {
                newLayers[i] = currentLayers[i];
            }

            return newLayers;
        }

		/// <summary>
		/// Compares a given neural network to a list of other and if better stores it at the correct rank.
		/// Compares the network to the current generation as well as overall best network in all generations.
		/// </summary>
		/// <param name="fitnessRankings">Fitness rankings.</param>
		/// <param name="fitnessContender">Fitness contender.</param>
        public static Network[] RankFitnessContender (Network[] fitnessRankings, Network fitnessContender) {
            int last = fitnessRankings.Length-1;

            // first we take care of the first case of an empty array (no other contender yet)
            if(fitnessRankings[last] == null) {
                fitnessRankings[last] = fitnessContender;

            } else if(fitnessRankings[last] != null && fitnessRankings[last].FitnessScore < fitnessContender.FitnessScore) {
                fitnessRankings[last] = fitnessContender;
            }

            // then we go through the rest of the arrays
            if (fitnessRankings.Length > 1) { // just making sure there is  more than one network to breed (there can't be less)

                // we go from last to first in the loop
                for (int i = fitnessRankings.Length - 2; i >= 0; i--) {
                    if (fitnessRankings[i] == null) { // if the array is empty we fill it one step at a time
                        fitnessRankings[i] = fitnessContender;
                        fitnessRankings[i + 1] = null;

                    } else if(fitnessRankings[i].FitnessScore < fitnessContender.FitnessScore) {
                        Network stepDown = fitnessRankings[i];
                        fitnessRankings[i] = fitnessContender;
                        fitnessRankings[i + 1] = stepDown;

                    } else {
                        i = 0; // if the contender doesn't have a better score anymore we exit the loop
                    }
                }
            }

			return fitnessRankings;
        }


		public static Color MutateColor(Color color, float synapsesChangedRatio) {

			// mutation value
			float mutationValue = Random.value > 0.5? synapsesChangedRatio : -synapsesChangedRatio;

			// select RGB
			float selector = Random.value;

			if(selector < 1f/3f) {
				color.r = Mathf.Clamp01(color.r + mutationValue);
			}
			else if (selector < 2f/3f){
				color.g = Mathf.Clamp01(color.g + mutationValue);
			}
			else {
				color.b = Mathf.Clamp01(color.b + mutationValue);
			}

			return color;
		}

	}
}
