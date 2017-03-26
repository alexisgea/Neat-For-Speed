using System.Collections.Generic;
using UnityEngine;

namespace nfs.nets.layered{

    /// <summary>
    /// Neural network trainer base class.
    /// </summary>
	public abstract class Trainer : MonoBehaviour {

		// neural netowrk popuplation variables and constants

		/// <summary>
		/// The amount of neural net and hosts
		/// </summary>
        [SerializeField] protected const int population = 20;
		/// <summary>
		/// The transform under which to parent the instanced population.
		/// </summary>
        [SerializeField] protected Transform populationGroup;
		/// <summary>
		/// The prefab containing a nets.layered.Controller implementation.
		/// </summary>
        [SerializeField] protected GameObject networkHost;

		// reference to the host population
		protected GameObject[] hostPopulation;
		protected int hostAlive { private set; get; }
		protected Stack<Controller> deadHosts = new Stack<Controller>();

		/// <summary>
		/// The base layers sizes of the neural networks.
		/// </summary>
        [SerializeField] protected int[] baseLayersSizes = new int[] {4, 4, 5, 2};


        // neural network mutation variables and constants

		/// <summary>
		/// The proportion of top performing network that will be selected for breeding.
		/// </summary>
        [SerializeField] protected float survivorRate = 0.25f;
		/// <summary>
		/// How much of the new generation will be from the all time best network,
		/// while the rest will be from this generation's best and some fresh blood.
		/// </summary>
        [SerializeField] protected float breedingRepartitionCoef = 0.6f;
		/// <summary>
		/// The fresh blood proportion in the new generation.
		/// </summary>
        [SerializeField] protected float freshBloodProportion = 0.1f;
		/// <summary>
		/// The probability a synapse will mutate.
		/// </summary>
        [SerializeField] protected float synapsesMutationRate = 0.1f;
		/// <summary>
		/// How much can a synapse change it's value at maximum (this depends on the type of mutation, check the code).
		/// </summary>
        [SerializeField] protected float synapsesMutationRange = 0.1f;
		/// <summary>
		/// Can the network mutate and discover it's number of inputs.
		/// </summary>
        [SerializeField] protected bool inputNbMutation = false;
		/// <summary>
		/// The probability the number of input will mutate.
		/// </summary>
        [SerializeField] protected float inputNbMutationRate = 0.01f;
		/// <summary>
		/// Can the network mutate the number of hidden neuron in a hidden layer.
		/// </summary>
        [SerializeField] protected bool hiddenNbMutation = true;
		/// <summary>
		/// The probability the number of hidden neuron in a layer will mutate.
		/// </summary>
        [SerializeField] protected float hiddenMbMutationRate = 0.01f;
		/// <summary>
		/// Can the network mutate the number of hidden layer.
		/// </summary>
        [SerializeField] protected bool hiddenLayerNbMutation = true;
		/// <summary>
		/// The probability the number of hidden layer will mutate.
		/// </summary>
        [SerializeField] protected float hiddenLayerNbMutationRate = 0.001f;

		// the number of network selected for breeding from the survivor rate.
        protected int breedingSampleNb;
        protected  Network[] alltimeFittestNets;
        protected Network[] generationFittestNets;

        // generation vaiables
		public int Generation { private set; get; }
		/// <summary>
		/// The algorythm will force the next generation after this time in seconds.
		/// </summary>
        [SerializeField] protected float maxGenerationTime = 90;
        protected float generationStartTime;
	
        /// <summary>
        /// The transform underwich to parent any instanctiated world element.
        /// </summary>
        [SerializeField] protected Transform worldGroup;


        // Abstract methods that HAVE TO be implemented

		/// <summary>
		/// Initialises the world.
		/// </summary>
        protected abstract void InitialiseWorld();
		/// <summary>
		/// After the population has been initialised, this function will be called to initialise other specific things.
		/// </summary>
		protected abstract void PostInitialisation ();
		/// <summary>
		/// Refreshs the world for a new generation.
		/// </summary>
        protected abstract void RefreshWorld();
		/// <summary>
		/// Refreshs the hosts and reset some variables for a new generation.
		/// </summary>
		protected abstract void RefreshHosts ();


		/// <summary>
		/// Monobehaviour start.
		/// </summary>
        protected virtual void Start() {
            InitialiseWorld();
            InitializePopulation();
			PostInitialisation ();
            
        }

        /// <summary>
        /// Monobehavirous update.
        /// </summary>
        protected virtual void Update() {
            if (hostAlive <= 0 || Time.unscaledTime - generationStartTime > maxGenerationTime) {
                NextGeneration();
            }
        }

        ///<summary>
        /// Instantiate the base popuplation and initialises their neural networks
        ///</summary>
        protected virtual void InitializePopulation () {
            // creates the population
            hostPopulation = new GameObject[population];

            // compute the amount of network which will have a descendance in each generation
            breedingSampleNb = (int)(population*survivorRate);
            breedingSampleNb = breedingSampleNb < 1 ? 1 : breedingSampleNb;
            
            // create the best fitness reference
            alltimeFittestNets = new Network[breedingSampleNb];
            generationFittestNets = new Network[breedingSampleNb];

			// loops through the popuplation to initialise the neural networks
            for (int i=0; i < population; i++) {
                hostPopulation[i] = GameObject.Instantiate(networkHost, CalculateStartPosition(i), CalculateStartOrientation (i));
                hostPopulation[i].transform.SetParent(populationGroup);
                hostPopulation[i].GetComponent<Controller>().NeuralNet = new Network(baseLayersSizes);
                hostPopulation[i].GetComponent<Controller>().Death += OnHostDeath; // we register to each car's signal for collision
            }

			Generation = 1;
			ResetTrainerVariables ();
        }

		/// <summary>
		/// Initialises the trainer variables.
		/// </summary>
		protected virtual void ResetTrainerVariables() {
			// all is ready and set to start for the training
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
                        
			ResetTrainerVariables ();

            RefreshWorld();
			RefreshHosts ();
            ResetHostsPositions();

			Generation += 1;
        }

		///<summary>
        /// Called by each car's colision.
        /// Updates the car alive counter and add the car to the deadCars array to not count them multiple tiem.
        ///</summary>
        protected virtual void OnHostDeath (Controller host) {
            // first we make sure the car hit a wall
            if(!deadHosts.Contains(host)) {

                hostAlive -= 1;
                deadHosts.Push(host);
            }
        }

		/// <summary>
		/// Kills any survivor.
		/// </summary>
        protected virtual void KillAnySurvivor() {
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
        protected virtual void SortCurrentGeneration() {

            if (generationFittestNets[generationFittestNets.Length-1] != null) {
                for (int i = 0; i < generationFittestNets.Length; i++) {
                    generationFittestNets[i].FitnessScore = 0;
                }
            }

            for (int i = 0; i< hostPopulation.Length; i++) {
                layered.Network fitnessContender = hostPopulation[i].GetComponent<Controller>().NeuralNet;
                
                Evolution.RankFitnessContender(alltimeFittestNets, fitnessContender.GetClone());
                Evolution.RankFitnessContender(generationFittestNets, fitnessContender.GetClone());
            }
        }

        ///<summary>
        /// Breed the next generation of networks from the current best and overall best.
        ///</summary>
        protected virtual void BreedNextGeneration() {

            int k = 0;
            int l = 0;
            for (int i=0; i < population; i++) {

                if (i < population * breedingRepartitionCoef) { // all time generation
                    hostPopulation[i].GetComponent<Controller>().NeuralNet = Evolution.CreateMutatedOffspring(
                                    alltimeFittestNets[k], l+1,
                                    hiddenLayerNbMutation, hiddenLayerNbMutationRate,
                                    hiddenNbMutation, hiddenLayerNbMutationRate,
                                    synapsesMutationRate, synapsesMutationRange); // l+1 as mutate coef to make each version more propable to mutate a lot
                    
                    l += 1;
                    if (l > population * breedingRepartitionCoef / breedingSampleNb){
                        l = 0;
                        k += 1;
                    }

                } else if (i < population * (1 - freshBloodProportion)) { // this generation
                    hostPopulation[i].GetComponent<Controller>().NeuralNet = Evolution.CreateMutatedOffspring(
                                    generationFittestNets[k], l+1,
                                    hiddenLayerNbMutation, hiddenLayerNbMutationRate,
                                    hiddenNbMutation, hiddenLayerNbMutationRate,
                                    synapsesMutationRate, synapsesMutationRange);

                    l += 1;
                    if (l > population * (1 - freshBloodProportion) / breedingSampleNb){
                        l = 0;
                        k += 1;
                    }

                } else { // fresh blood (10%)
                    hostPopulation[i].GetComponent<Controller>().NeuralNet = new Network(baseLayersSizes);   
                }

                if(k>=breedingSampleNb)
                    k = 0;
            }
        } 

		/// <summary>
		/// Resets the hosts positions.
		/// </summary>
        protected virtual void ResetHostsPositions() {
            // we reset the car position and insert new neural nets
            for (int i=0; i < population; i++) {
                Controller host = hostPopulation[i].GetComponent<Controller>();
                host.Reset(CalculateStartPosition(i), CalculateStartOrientation(i));
            }
        }

		/// <summary>
		/// Calculates the start position. of each car.
		/// </summary>
        protected virtual Vector3 CalculateStartPosition(int i) {
            return Vector3.zero;
        }

		/// <summary>
		/// Calculates the start orientation.
		/// </summary>
		/// <returns>The start orientation.</returns>
		/// <param name="i">The index.</param>
        protected virtual Quaternion CalculateStartOrientation (int i) {
            return Quaternion.identity;
        }
	}
}
