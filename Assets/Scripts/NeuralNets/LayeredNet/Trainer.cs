using System.Collections.Generic;
using UnityEngine;

namespace nfs.nets.layered{

    ///<summary>
    /// All the different time of synapses mutation.
    ///</summary>
    public enum MutationType { additive, multiply, reverse, replace, nullify }

	public abstract class Trainer : MonoBehaviour {

		// neural netowrk popuplation variables
        [SerializeField] protected int population = 20;
        [SerializeField] protected Transform populationGroup;
        [SerializeField] protected GameObject networkHost;
		protected GameObject[] hostPopulation;
		protected int hostAlive { private set; get; }
		protected Stack<Controller> deadHosts = new Stack<Controller>();
        [SerializeField] protected int[] baseLayersSizes = new int[] {4, 4, 5, 2};


        // neural network mutation variables
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
        protected int breedingSampleNb;
        protected  Network[] alltimeFittestNets;
        protected Network[] generationFittestNets;

        // generation vaiables
		public int Generation { private set; get; }
        [SerializeField] protected float maxGenerationTime = 90;
        protected float generationStartTime;
	
        // world and race tracks references
        [SerializeField] protected Transform worldGroup;


        // Abstract methods that HAVE TO be implemented
        protected abstract void InitializeWorld();
        protected abstract void RefreshWorld();



		 // Initialises the base popuplations
        protected virtual void Start() {
            InitializeWorld();
            InitializePopulation();
            
        }

        // starts the next generation when the time has come
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

            for (int i=0; i < population; i++) {
                hostPopulation[i] = GameObject.Instantiate(networkHost, CalculateStartPosition(i), CalculateStartOrientation (i));
                hostPopulation[i].transform.SetParent(populationGroup);
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
            ResetHostsPositions();

            // all is ready and set to start for the training
            Generation += 1;
            hostAlive = population;
            generationStartTime = Time.unscaledTime;

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

                } else if (i < population * (1 - freshBloodCoef)) { // this generation
                    hostPopulation[i].GetComponent<Controller>().NeuralNet = Evolution.CreateMutatedOffspring(
                                    generationFittestNets[k], l+1,
                                    hiddenLayerNbMutation, hiddenLayerNbMutationRate,
                                    hiddenNbMutation, hiddenLayerNbMutationRate,
                                    synapsesMutationRate, synapsesMutationRange);

                    l += 1;
                    if (l > population * (1 - freshBloodCoef) / breedingSampleNb){
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
        protected virtual Quaternion CalculateStartOrientation (int i) {
            return Quaternion.identity;
        }
	}
}
