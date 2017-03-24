using System.Collections.Generic;
using UnityEngine;

namespace nfs.nets.layered{

	public abstract class Trainer : MonoBehaviour {

		// THE genetic algorythm instance
        protected Evolution evolution;

		// variables necessary for creating an instance of the trainer
        [SerializeField] protected int population = 20;
        [SerializeField] protected Transform populationGroup;
        [SerializeField] protected GameObject networkHost;
        [SerializeField] protected int[] networkLayersSizes = new int[] {4, 4, 5, 2};
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

		 // Initialises the base popuplations
        private void Start() {
            
            evolution = new nets.layered.Evolution(population, networkLayersSizes,
                                                survivorRate, breedingRepartitionCoef, freshBloodCoef,
                                                synapsesMutationRate, synapsesMutationRange,
                                                inputNbMutation, inputNbMutationRate,
                                                hiddenLayerNbMutation, hiddenLayerNbMutationRate,
                                                hiddenNbMutation, hiddenMbMutationRate);

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

            for (int i=0; i < population; i++) {
                hostPopulation[i] = GameObject.Instantiate(networkHost, CalculateStartPosition(i), CalculateStartOrientation (i));
                hostPopulation[i].transform.SetParent(populationGroup);
                //hostPopulation[i].AddComponent<CarLayeredNetController>();
                hostPopulation[i].GetComponent<Controller>().NeuralNet = evolution.Population[i];
                
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

            // we make sure everything is stopped
            for (int i=0; i < population; i++) {
                Controller host = hostPopulation[i].GetComponent<Controller>();
                if(!deadHosts.Contains(host)) {
                    host.Kill();
                }
            }

            evolution.PrepareNextGeneration();
            
            Debug.Log(  "Generation " + Generation +  " best fitness:" + evolution.GenerationFittestNets[0].FitnessScore +
                        " all time best fitness:" + evolution.AlltimeFittestNets[0].FitnessScore);

            // we make sure dead cars are cleared now that there is a new generation
            deadHosts.Clear();

            RefreshWorld();
            
            // we breed the next generation from the best cars

            // we reset the car position and insert new neural nets
            for (int i=0; i < population; i++) {
                Controller host = hostPopulation[i].GetComponent<Controller>();
                host.Reset(CalculateStartPosition(i), CalculateStartOrientation(i));
                host.NeuralNet = evolution.Population[i];
            }
            
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

	}

}
