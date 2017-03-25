using System.Collections.Generic;
using UnityEngine;
using nfs.tools;

namespace nfs.nets.layered{

	public abstract class Trainer : MonoBehaviour {

		// variables necessary for creating an instance of the trainer
        [SerializeField] protected int population = 20;
        [SerializeField] protected Transform populationGroup;
        [SerializeField] protected GameObject networkHost;
        [SerializeField] protected int[] baseLayersSizes = new int[] {4, 4, 5, 2};
        [SerializeField] protected float survivorRate = 0.25f;
        [SerializeField] protected float breedingRepartitionCoef = 0.6f;
        [SerializeField] protected float freshBloodCoef = 0.1f;
        [SerializeField] protected float synapsesMutationRate = 0.1f;
        [SerializeField] protected float synapsesMutationRange = 0.1f;
        [SerializeField] protected bool inputNbMutation = false;
        [SerializeField] protected float inputNbMutationRate = 0.01f;
        [SerializeField] protected bool hiddenNbMutation = true;
        [SerializeField] protected float hiddenMbMutationRate = 0.01f;
        [SerializeField] protected bool hiddenLayerNbMutation = true;
        [SerializeField] protected float hiddenLayerNbMutationRate = 0.001f;

		// car related variable and reference
		public int hostAlive { private set; get; }
		protected GameObject[] hostPopulation;
		protected Stack<Controller> deadHosts = new Stack<Controller>();

		public int Generation { private set; get; }
        [SerializeField] protected float maxGenerationTime = 90;
        protected float generationStartTime;
	
        // world and race tracks references
        [SerializeField] protected Transform worldGroup;


        private int breedingSample;
        private  Network[] alltimeFittestNets;
        private Network[] generationFittestNets;



		 // Initialises the base popuplations
        private void Start() {

            InitializeWorld();
            InitializePopulation();
            
        }

        // starts the next generation when the time has come
        private void Update() {
            if (hostAlive <= 0 || Time.unscaledTime - generationStartTime > maxGenerationTime) {
                NextGeneration();
            }
        }

        protected abstract void InitializeWorld();

        ///<summary>
        /// Instantiate the base popuplation and initialises their neural networks
        ///</summary>
        protected virtual void InitializePopulation () {
            // creates the population
            hostPopulation = new GameObject[population];

            // compute the amount of network which will have a descendance in each generation
            breedingSample = (int)(population*survivorRate);
            breedingSample = breedingSample < 1 ? 1 : breedingSample;
            
            // create the best fitness reference
            alltimeFittestNets = new Network[breedingSample];
            generationFittestNets = new Network[breedingSample];

            for (int i=0; i < population; i++) {
                hostPopulation[i] = GameObject.Instantiate(networkHost, CalculateStartPosition(i), CalculateStartOrientation (i));
                hostPopulation[i].transform.SetParent(populationGroup);
                //hostPopulation[i].AddComponent<CarLayeredNetController>();
                hostPopulation[i].GetComponent<Controller>().NeuralNet = new Network(baseLayersSizes);
                
                hostPopulation[i].GetComponent<Controller>().Death += OnHostDeath; // we register to each car's signal for collision
            }

            // all is ready and set to start for the training
            Generation = 1;
            hostAlive = population;
            generationStartTime = Time.unscaledTime;

        }

		///<summary>
        /// Initiate the new generation by breeding offspring and inserting them in the current popuplation.
        ///</summary>
        public virtual void NextGeneration () {

            KillAnySurvivor();
            SortCurrentGeneration();
            BreedNextGeneration();
            
            Debug.Log(  "Generation " + Generation +  " best fitness:" + generationFittestNets[0].FitnessScore +
                        " all time best fitness:" + alltimeFittestNets[0].FitnessScore);

            RefreshWorld();
            ResetHostsPosition();

            // all is ready and set to start for the training
            Generation += 1;
            hostAlive = population;
            generationStartTime = Time.unscaledTime;

        }

        protected abstract void RefreshWorld();

		/// <summary>
		/// Calculates the start position. of each car.
		/// </summary>
        protected abstract Vector3 CalculateStartPosition(int i);

        protected abstract Quaternion CalculateStartOrientation (int i);

		///<summary>
        /// Called by each car's colision.
        /// Updates the car alive counter and add the car to the deadCars array to not count them multiple tiem.
        ///</summary>
        private void OnHostDeath (Controller host) {
            // first we make sure the car hit a wall
            if(!deadHosts.Contains(host)) {

                hostAlive -= 1;
                deadHosts.Push(host);
            }
        }

        private void KillAnySurvivor() {
            for (int i=0; i < population; i++) {
                Controller host = hostPopulation[i].GetComponent<Controller>();
                if(!deadHosts.Contains(host)) {
                    host.Kill();
                }
            }

            // we make sure dead cars are cleared now that there is a new generation
            deadHosts.Clear();
        }


		/// <summary>
		/// Sorts the current generation of networks by fitness.
		/// </summary>
        private void SortCurrentGeneration() {

            if (generationFittestNets[generationFittestNets.Length-1] != null) {
                for (int i = 0; i < generationFittestNets.Length; i++) {
                    generationFittestNets[i].FitnessScore = 0;
                }
            }

            for (int i = 0; i< hostPopulation.Length; i++) {
                layered.Network fitnessContender = hostPopulation[i].GetComponent<Controller>().NeuralNet;
                
                CompareFitness(alltimeFittestNets, fitnessContender.GetClone());
                CompareFitness(generationFittestNets, fitnessContender.GetClone());
            }
        }

        ///<summary>
        /// Breed the next generation of networks from the current best and overall best.
        ///</summary>
        private void BreedNextGeneration() {

            int k = 0;
            int l = 0;
            for (int i=0; i < population; i++) {


                if (i < population * breedingRepartitionCoef) { // all time generation
                    hostPopulation[i].GetComponent<Controller>().NeuralNet = CreateMutatedOffspring(alltimeFittestNets[k], l+1); // l+1 as mutate coef to make each version more propable to mutate a lot
                    l += 1;
                    if (l > population * breedingRepartitionCoef / breedingSample){
                        l = 0;
                        k += 1;
                    }

                } else if (i < population * (1 - freshBloodCoef)) { // this generation
                    hostPopulation[i].GetComponent<Controller>().NeuralNet = CreateMutatedOffspring(generationFittestNets[k], l+1);
                    l += 1;
                    if (l > population * (1 - freshBloodCoef) / breedingSample){
                        l = 0;
                        k += 1;
                    }

                } else { // fresh blood (10%)
                    hostPopulation[i].GetComponent<Controller>().NeuralNet = new Network(baseLayersSizes);   
                }

                if(k>=breedingSample)
                    k = 0;
            }
        } 


        private void ResetHostsPosition() {
            // we reset the car position and insert new neural nets
            for (int i=0; i < population; i++) {
                Controller host = hostPopulation[i].GetComponent<Controller>();
                host.Reset(CalculateStartPosition(i), CalculateStartOrientation(i));
            }
        }

        /// <summary>
        /// Creates the mutated offspring.
        /// </summary>
        /// <returns>The mutated offspring.</returns>
        /// <param name="neuralNet">Neural net.</param>
        /// <param name="mutateCoef">Mutate coef.</param>
        private Network CreateMutatedOffspring(Network neuralNet, int mutateCoef) {

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

            Network mutadedOffspring = new Network(layerSizes);

            mutadedOffspring.InsertSynapses(synapses);

            return mutadedOffspring;
        }

		/// <summary>
		/// Mutates the nb of hidden layer.
		/// </summary>
		/// <param name="neuralNet">Neural net.</param>
		/// <param name="hiddenLayersSizes">Hidden layers sizes.</param>
		/// <param name="synapses">Synapses.</param>
        private void MutateNbOfHiddenLayer(Network neuralNet, int[] hiddenLayersSizes, Matrix[] synapses) {
            
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
        private void MutateNbOfHiddenLayerNeurons(Network neuralNet, int[] hiddenLayersSizes, Matrix[] synapses) {
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
        private void MutateSynapsesValues(Network neuralNet, Matrix[] synapses) {
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
        private void CompareFitness (Network[] fitnessRankings, Network fitnessContender) {
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
                        Network stepDown = fitnessRankings[i];
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
