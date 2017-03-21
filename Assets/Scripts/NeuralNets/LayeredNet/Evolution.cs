using UnityEngine;
using nfs.tools;

namespace nfs.layered {

    ///<summary>
    /// All the different time of synapses mutation.
    ///</summary>
    public enum MutationType { additive, multiply, reverse, replace, nullify }

    ///<summary>
    /// Genetic algorythm managing all the neural network,
    /// checking their fitness and producing mutated offsprings.
    /// Also cycles through different tracks to avoid overfitting.
    ///</summary>
    public class Evolution {

        private float survivorRate; // how many of a generation will be considered for breeding
        private float breedingRepartitionCoef; // what proportion of a new generation will be alltime fittest
        private float freshBloodCoef; // what proportion of a new generation will be brand new nets
        private float synapsesMutationRate; // how much is a synapses likely to mutate
        private float synapsesMutationRange; // how much can be the mutation of a synapse at maximum
        private bool inputNbMutation; // is it possible to modify the number of input (Input discovery)
        private float inputNbMutationRate; // what is the probability of a new neuron in input layer
        private bool hiddenLayerNbMutation; // is it possible to modify the number of hidden layer
        private float hiddenLayerNbMutationRate; // what is the probability of a new hidden layer
        private bool hiddenNbMutation; // is it possible to modify the number of neurons in hidden layers
        private float hiddenMbMutationRate; // what is the probability of a new neuron in hidden layer

        private int breedingSample;

        public int PopulationSize {get {return Population.Length;}}
        public NeuralNet[] Population {private set; get;}
        public  NeuralNet[] AlltimeFittestNets {private set; get;}
        public NeuralNet[] GenerationFittestNets {private set; get;}

        private int[] baseNetLayersSizes;

		// Constructor
        public Evolution (int popSize, int[] networkLayersSizes,
                            float survivorRate = 0.25f, float breedingRepartitionCoef = 0.6f, float freshBloodCoef = 0.1f,
                            float synapsesMutationRate = 0.1f, float synapsesMutationRange = 0.1f,
                            bool inputNbMutation = false, float inputNbMutationRate = 0.01f,
                            bool hiddenLayerNbMutation = true, float hiddenLayerNbMutationRate = 0.001f,
                            bool hiddenNbMutation = true, float hiddenMbMutationRate = 0.01f) {

            this.baseNetLayersSizes = networkLayersSizes;
            this.survivorRate = survivorRate;
            this.breedingRepartitionCoef = breedingRepartitionCoef;
            this.freshBloodCoef = freshBloodCoef;
            this.synapsesMutationRate = synapsesMutationRate;
            this.synapsesMutationRange = synapsesMutationRange;
            this.inputNbMutation = inputNbMutation;
            this.inputNbMutationRate = inputNbMutationRate;
            this.hiddenLayerNbMutation = hiddenLayerNbMutation;
            this.hiddenLayerNbMutationRate = hiddenLayerNbMutationRate;
            this.hiddenNbMutation = hiddenNbMutation;
            this.hiddenMbMutationRate = hiddenMbMutationRate;

            InitializePopulation( popSize, baseNetLayersSizes);

        }

        ///<summary>
        /// Instantiate the base popuplation and initialises their neural networks
        ///</summary>
        private void InitializePopulation (int popSize, int[] networkLayersSizes) {

            // creates the population of networks
            Population = new NeuralNet[popSize];

            // compute the amount of network which will have a descendance in each generation
            breedingSample = (int)(PopulationSize*survivorRate);
            breedingSample = breedingSample < 1 ? 1 : breedingSample;
            
            // create the best fitness reference
            AlltimeFittestNets = new NeuralNet[breedingSample];
            GenerationFittestNets = new NeuralNet[breedingSample];

            for (int i=0; i < PopulationSize; i++) {
                Population[i] = new NeuralNet(networkLayersSizes);
            }
        }

		/// <summary>
		/// Prepares the next generation.
		/// It first sorts the networks by Fitness and them breeds a new generation from the best.
		/// </summary>
        public void PrepareNextGeneration() {
            SortCurrentGeneration();
            BreedNextGeneration();
        }

		/// <summary>
		/// Sorts the current generation of networks by fitness.
		/// </summary>
        private void SortCurrentGeneration() {

            if (GenerationFittestNets[GenerationFittestNets.Length-1] != null) {
                for (int i = 0; i < GenerationFittestNets.Length; i++) {
                    GenerationFittestNets[i].FitnessScore = 0;
                }
            }

            for (int i = 0; i< Population.Length; i++) {
                CompareFitness(AlltimeFittestNets, Population[i].GetClone());
                CompareFitness(GenerationFittestNets, Population[i].GetClone());
            }
        }

        ///<summary>
        /// Breed the next generation of networks from the current best and overall best.
        ///</summary>
        private void BreedNextGeneration() {

            int k = 0;
            int l = 0;
            for (int i=0; i < PopulationSize; i++) {
                if (i < PopulationSize * breedingRepartitionCoef) { // all time generation
                    Population[i] = CreateMutatedOffspring(AlltimeFittestNets[k], l+1); // l+1 as mutate coef to make each version more propable to mutate a lot
                    l += 1;
                    if (l > PopulationSize * breedingRepartitionCoef / breedingSample){
                        l = 0;
                        k += 1;
                    }

                } else if (i < PopulationSize * (1 - freshBloodCoef)) { // this generation
                    Population[i] = CreateMutatedOffspring(GenerationFittestNets[k], l+1);
                    l += 1;
                    if (l > PopulationSize * (1 - freshBloodCoef) / breedingSample){
                        l = 0;
                        k += 1;
                    }

                } else { // fresh blood (10%)
                    Population[i] = new NeuralNet(baseNetLayersSizes);   
                }

                if(k>=breedingSample)
                    k = 0;
            }
        } 

        /// <summary>
        /// Creates the mutated offspring.
        /// </summary>
        /// <returns>The mutated offspring.</returns>
        /// <param name="neuralNet">Neural net.</param>
        /// <param name="mutateCoef">Mutate coef.</param>
        private NeuralNet CreateMutatedOffspring(NeuralNet neuralNet, int mutateCoef) {
            
            int[] hiddenLayersSizes = neuralNet.HiddenLayersSizes;
            Matrix[] synapses = neuralNet.GetSynapsesClone();

            // TODO LATER
            // Implemenet here mutation for new input
            // have a fixe array of sensors and an array of int containing the sensor indx to read from
            // mutate this array of int

            // mutate number of hidden layers
            if(hiddenLayerNbMutation)
                MutateNbOfHiddenLayer(neuralNet, hiddenLayersSizes, synapses);

            // mutated number of neurons in hidden layers
            if(hiddenNbMutation)
                MutateNbOfHiddenLayerNeurons(neuralNet, hiddenLayersSizes, synapses);

            // mutate synapses values
            MutateSynapsesValues(neuralNet, synapses);

            int[] layerSizes = new int[hiddenLayersSizes.Length + 2];
            layerSizes[0] = neuralNet.InputSize;
            layerSizes[layerSizes.Length-1] = neuralNet.OutputSize;
            for(int i=1; i<layerSizes.Length-1; i++) {
                layerSizes[i] = hiddenLayersSizes[i-1];
            } 

            NeuralNet mutadedOffspring = new NeuralNet(layerSizes);

            mutadedOffspring.InsertSynapses(synapses);

            return mutadedOffspring;
        }

		/// <summary>
		/// Mutates the nb of hidden layer.
		/// </summary>
		/// <param name="neuralNet">Neural net.</param>
		/// <param name="hiddenLayersSizes">Hidden layers sizes.</param>
		/// <param name="synapses">Synapses.</param>
        private void MutateNbOfHiddenLayer(NeuralNet neuralNet, int[] hiddenLayersSizes, Matrix[] synapses) {
            
            if(Random.value < hiddenLayerNbMutationRate) {
                if (Random.value < 0.5f && hiddenLayersSizes.Length > 1) { // random to get positive vs negative value
                    hiddenLayersSizes = RedimentionLayersNb(hiddenLayersSizes, -1);

                    synapses = RedimentionLayersNb(synapses, -1);
                    synapses[synapses.Length - 1] = Matrix.Redimension(synapses[synapses.Length - 1], hiddenLayersSizes[hiddenLayersSizes.Length - 1], neuralNet.OutputSize);

                } else {
                    hiddenLayersSizes = RedimentionLayersNb(hiddenLayersSizes, +1);
                    hiddenLayersSizes[hiddenLayersSizes.Length - 1] = neuralNet.OutputSize;

                    synapses = RedimentionLayersNb(synapses, +1);
                    synapses[synapses.Length - 1] = new Matrix(hiddenLayersSizes[hiddenLayersSizes.Length - 1], neuralNet.OutputSize).SetAsSynapse();
                }
            }
        }

		/// <summary>
		/// Mutates the nb of hidden layer neurons.
		/// </summary>
		/// <param name="neuralNet">Neural net.</param>
		/// <param name="hiddenLayersSizes">Hidden layers sizes.</param>
		/// <param name="synapses">Synapses.</param>
        private void MutateNbOfHiddenLayerNeurons(NeuralNet neuralNet, int[] hiddenLayersSizes, Matrix[] synapses) {
            if(Random.value < hiddenMbMutationRate) {
                int layerNb = Random.Range(0, hiddenLayersSizes.Length - 1);
                if (Random.value < 0.5f && hiddenLayersSizes[layerNb] > 1) { // random to get positive vs negative value
                    hiddenLayersSizes[layerNb] -= 1;
                } else {
                    hiddenLayersSizes[layerNb] += 1;
                }
                // need to use the previous synapses values here as we might be going from/to oustide of the hidden layers
                synapses[layerNb] = Matrix.Redimension(synapses[layerNb], synapses[layerNb].I, hiddenLayersSizes[layerNb]);
                synapses[layerNb+1] = Matrix.Redimension(synapses[layerNb+1], hiddenLayersSizes[layerNb], synapses[layerNb+1].J);
            }
        }

		/// <summary>
		/// Mutates the synapses values.
		/// </summary>
		/// <param name="neuralNet">Neural net.</param>
		/// <param name="synapses">Synapses.</param>
        private void MutateSynapsesValues(NeuralNet neuralNet, Matrix[] synapses) {
            for (int n=0; n<synapses.Length; n++) {

                for (int i = 0; i < synapses[n].I; i++) {
                    for (int j=0; j < synapses[n].J; j++) {
                        if (Random.value < synapsesMutationRate) {
                            MutationType type = (MutationType)Random.Range(0, System.Enum.GetValues(typeof(MutationType)).Length-1);
                            float mutatedValue = synapses[n].GetValue(i, j);;
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
                            synapses[n].SetValue(i, j, mutatedValue);  
                        }
                    }
                }
            }
        }

		/// <summary>
		/// Redimension an array of in for the hidden layers.
		/// </summary>
		/// <returns>The layers nb.</returns>
		/// <param name="currentLayers">Current layers.</param>
		/// <param name="sizeMod">Size mod.</param>
        private int[] RedimentionLayersNb (int[] currentLayers, int sizeMod) {
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
        private Matrix[] RedimentionLayersNb (Matrix[] currentLayers, int sizeMod) {
            Matrix[] newLayers = new Matrix[currentLayers.Length + sizeMod];
            for (int i = 0; i < Mathf.Min(currentLayers.Length, newLayers.Length); i++) {
                newLayers[i] = currentLayers[i];
            }

            return newLayers;
        }

		/// <summary>
		// Compares a given neural network to a list of other and if better stores it at the correct rank.
		// Compares the network to the current generation as well as overall best network in all generations.
		/// </summary>
		/// <param name="fitnessRankings">Fitness rankings.</param>
		/// <param name="fitnessContender">Fitness contender.</param>
        private void CompareFitness (NeuralNet[] fitnessRankings, NeuralNet fitnessContender) {
            int last = fitnessRankings.Length-1;

            // first we take care of the first case of an empty array (no other contender yet)
            if(fitnessRankings[last] == null){
                fitnessRankings[last] = fitnessContender;
            } else if(fitnessRankings[last] != null && fitnessRankings[last].FitnessScore < fitnessContender.FitnessScore) {
                fitnessRankings[last] = fitnessContender;
            }

            // then we go through the rest of the arrays
            if (breedingSample > 1) { // just making sure there is  more than one network to breed (there can't be less)

                // we go from last to first in the loop
                for (int i = breedingSample - 2; i >= 0; i--) {
                    if (fitnessRankings[i] == null) { // if the array is empty we fill it one step at a time
                        fitnessRankings[i] = fitnessContender;
                        fitnessRankings[i + 1] = null;
                    } else if(fitnessRankings[i].FitnessScore < fitnessContender.FitnessScore) {
                        NeuralNet stepDown = fitnessRankings[i];
                        fitnessRankings[i] = fitnessContender;
                        fitnessRankings[i + 1] = stepDown;

                    } else {
                        i = 0; // if the contender doesn't have a better score anymore we exit the loop
                    }
                }
            }
        }

    }
}
