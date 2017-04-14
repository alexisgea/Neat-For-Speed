using System.Collections.Generic;
using UnityEngine;
using System;

namespace nfs.nets.layered{

    /// <summary>
    /// Neural network trainer base class.
    /// </summary>
	public abstract class Trainer : MonoBehaviour {

		// neural netowrk popuplation variables and constants

		/// <summary>
		/// The amount of neural net and hosts
		/// </summary>
        [SerializeField] protected int population = 20;
		/// <summary>
		/// The transform under which to parent the instanced population.
		/// </summary>
        [SerializeField] protected Transform populationGroup;
		/// <summary>
		/// The prefab containing a nets.layered.Controller implementation.
		/// </summary>
        [SerializeField] protected GameObject networkHost;

		public int TotalNetworkGenerated { private set; get;}
		public int TotalNetworkSelectedForBreeding { set; get;}
		public int CurrentLiveNetworks {get {return population - deadHosts.Count;}}
		public int CurrentDeadNetworks {get {return deadHosts.Count;}}


		// reference to the host population
		public GameObject[] HostPopulation {private set; get;}
		//protected int hostAlive { private set; get; }
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
        public Network[] AlltimeFittestNets { private set; get; }
        public Network[] GenerationFittestNets { private set; get; }
		public Dictionary<string, Network> NetworkGenealogy { private set; get;}

        // generation vaiables
        public int GenerationNb { private set; get; }

        /// <summary>
        /// Event when the next generation starts training.
        /// </summary>
        public event Action NextGenerationTraining;

        /// <summary>
        /// The algorythm will force the next generation after this time in seconds.
        /// </summary>
        [SerializeField] protected float maxGenerationTime = 90;
		public float GenerationStartTime { private set; get; }
	
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
            if (CurrentLiveNetworks == 0 || Time.unscaledTime - GenerationStartTime > maxGenerationTime) {
                NextGeneration();
            }
        }

        ///<summary>
        /// Instantiate the base popuplation and initialises their neural networks
        ///</summary>
        protected virtual void InitializePopulation () {
            // creates the population
            HostPopulation = new GameObject[population];

            // compute the amount of network which will have a descendance in each generation
            breedingSampleNb = (int)(population*survivorRate);
            breedingSampleNb = breedingSampleNb < 1 ? 1 : breedingSampleNb;
            
            // create the best fitness reference
            AlltimeFittestNets = new Network[breedingSampleNb];
            GenerationFittestNets = new Network[breedingSampleNb];
			NetworkGenealogy = new Dictionary<string, Network> ();

			//GenerationNb = 1;
			ResetTrainerVariables ();

			// loops through the popuplation to initialise the neural networks
            for (int i=0; i < population; i++) {
                HostPopulation[i] = GameObject.Instantiate(networkHost, CalculateStartPosition(i), CalculateStartOrientation(i));
                HostPopulation[i].transform.SetParent(populationGroup);
                HostPopulation[i].GetComponent<Controller>().NeuralNet = new Network(baseLayersSizes, GenerateNeworktId(i));
                HostPopulation[i].GetComponent<Controller>().NeuralNet.Colorisation = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
                HostPopulation[i].GetComponent<Controller>().Death += OnHostDeath; // we register to each car's signal for collision

				TotalNetworkGenerated++;
            }

            RaiseNextGenerationTraining();
        }

		/// <summary>
		/// Initialises the trainer variables.
		/// </summary>
		protected virtual void ResetTrainerVariables() {
			// all is ready and set to start for the training
			//hostAlive = population;
			GenerationNb += 1;
			GenerationStartTime = Time.unscaledTime;
		}

		///<summary>
        /// Initiate the new generation by breeding offspring and inserting them in the current popuplation.
        ///</summary>
        public virtual void NextGeneration () {

            // end the current generation
            KillAnySurvivor();
            SortCurrentGeneration();
            
            Debug.Log(  "Generation " + GenerationNb +  " best fitness:" + GenerationFittestNets[0].FitnessScore +
                        " all time best fitness:" + AlltimeFittestNets[0].FitnessScore);

            // get the next generation ready            
			ResetTrainerVariables ();
			//GenerationNb += 1;
			BreedNextGeneration();

            RefreshWorld();
			RefreshHosts ();
            ResetHostsPositions();

            RaiseNextGenerationTraining();
        }

		///<summary>
        /// Called by each car's colision.
        /// Updates the car alive counter and add the car to the deadCars array to not count them multiple tiem.
        ///</summary>
        protected virtual void OnHostDeath (Controller host) {
            // first we make sure the car hit a wall
            if(!deadHosts.Contains(host)) {

                //hostAlive -= 1;
                deadHosts.Push(host);
            }
        }

		/// <summary>
		/// Kills any survivor.
		/// </summary>
        protected virtual void KillAnySurvivor() {
            for (int i=0; i < population; i++) {
                Controller host = HostPopulation[i].GetComponent<Controller>();
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

            if (GenerationFittestNets[GenerationFittestNets.Length-1] != null) {
                for (int i = 0; i < GenerationFittestNets.Length; i++) {
                    GenerationFittestNets[i].FitnessScore = 0;
                }
            }

            for (int i = 0; i< HostPopulation.Length; i++) {
                layered.Network fitnessContender = HostPopulation[i].GetComponent<Controller>().NeuralNet;
                
                Evolution.RankFitnessContender(AlltimeFittestNets, fitnessContender.GetClone());
                Evolution.RankFitnessContender(GenerationFittestNets, fitnessContender.GetClone());
            }

			for (int i = 0; i < breedingSampleNb; i++) {
				if(!NetworkGenealogy.ContainsKey (AlltimeFittestNets[i].Id)){
					NetworkGenealogy.Add (AlltimeFittestNets[i].Id, AlltimeFittestNets[i]);
				}
				if(!NetworkGenealogy.ContainsKey (GenerationFittestNets[i].Id)){
					NetworkGenealogy.Add (GenerationFittestNets[i].Id, GenerationFittestNets[i]);
				}
			}
        }

        protected virtual void RaiseNextGenerationTraining() {
            if (NextGenerationTraining != null) {
                NextGenerationTraining.Invoke();
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
                    HostPopulation[i].GetComponent<Controller>().NeuralNet = Evolution.CreateMutatedOffspring(
									AlltimeFittestNets[k], GenerateNeworktId (i), l+1,
                                    hiddenLayerNbMutation, hiddenLayerNbMutationRate,
                                    hiddenNbMutation, hiddenLayerNbMutationRate,
                                    synapsesMutationRate, synapsesMutationRange); // l+1 as mutate coef to make each version more propable to mutate a lot
                    
                    l += 1;
                    if (l > population * breedingRepartitionCoef / breedingSampleNb){
                        l = 0;
                        k += 1;
                    }

                } else if (i < population * (1 - freshBloodProportion)) { // this generation
                    HostPopulation[i].GetComponent<Controller>().NeuralNet = Evolution.CreateMutatedOffspring(
									GenerationFittestNets[k], GenerateNeworktId (i), l+1,
                                    hiddenLayerNbMutation, hiddenLayerNbMutationRate,
                                    hiddenNbMutation, hiddenLayerNbMutationRate,
                                    synapsesMutationRate, synapsesMutationRange);



                    l += 1;
                    if (l > population * (1 - freshBloodProportion) / breedingSampleNb){
                        l = 0;
                        k += 1;
                    }

                } else { // fresh blood (10%)
                    HostPopulation[i].GetComponent<Controller>().NeuralNet = new Network(baseLayersSizes, GenerateNeworktId (i));   
                }

				if(k>=breedingSampleNb) {
					k = 0;
				}

				TotalNetworkGenerated++;
            }
        }

		/// <summary>
		/// Generates the neworkt identifier.
		/// </summary>
		/// <returns>The neworkt identifier.</returns>
		/// <param name="i">The index.</param>
		protected virtual string GenerateNeworktId(int i) {
			return (string)(GenerationNb + "-" + i);
		}

		/// <summary>
		/// Resets the hosts positions.
		/// </summary>
        protected virtual void ResetHostsPositions() {
            // we reset the car position and insert new neural nets
            for (int i=0; i < population; i++) {
                Controller host = HostPopulation[i].GetComponent<Controller>();
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

        public abstract void SaveBestNetwork();

	}
}
