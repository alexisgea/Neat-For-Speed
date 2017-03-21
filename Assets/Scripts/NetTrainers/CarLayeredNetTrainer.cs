using System.Collections.Generic;
using UnityEngine;
using nfs.controllers;
using nfs.car;

namespace nfs.trainers {

	/// <summary>
	/// Implementation of the evolution car to specifically train neural net controlling cars.
	/// </summary>
	public class CarLayeredNetTrainer : MonoBehaviour {

		// THE genetic algorythm instance
        private layered.Evolution evolution;

		// variables necessary for creating an instance of the trainer
        [SerializeField] private int numberOfCar = 20;
        [SerializeField] private Transform populationGroup;
        [SerializeField] private GameObject carPrefab;
        [SerializeField] private float maxDistFitness = 1000;
        [SerializeField] private int[] networkLayerSizes = new int[] {4, 4, 5, 2};
        [SerializeField] private float survivorRate = 0.25f;
        [SerializeField] private float breedingRepartitionCoef = 0.6f;
        [SerializeField] private float freshBloodCoef = 0.1f;
        [SerializeField] private float synapsesMutationRate = 0.1f;
        [SerializeField] private float synapsesMutationRange = 0.1f;
        [SerializeField] private bool inputNbMutation = false;
        [SerializeField] private float inputNbMutationRate = 0.01f;
        [SerializeField] private bool hiddenNbMutation = true;
        [SerializeField] private float hiddenMbMutationRate = 0.01f;
        [SerializeField] private bool hiddenLayerNbMutation = true;
        [SerializeField] private float hiddenLayerNbMutationRate = 0.001f;

		// variables specific to this trainer

		// car related variable and reference
		public int CarAlive { private set; get; }
		private GameObject[] carPopulation;
		private Stack<CarBehaviour> deadCars = new Stack<CarBehaviour>();

		public int Generation { private set; get; }
        [SerializeField] private float maxGenerationTime = 90;
        private float generationStartTime;
	
        // world and race tracks references
        [SerializeField] private Transform world;
        [SerializeField] private GameObject[] tracks;
        private GameObject raceTrack;
        [SerializeField] private Vector3 startPosition = Vector3.zero;
        [SerializeField] private float startPositioSpread = 2f;

		 // Initialises the base popuplations
        private void Start() {
            
            evolution = new layered.Evolution(  numberOfCar, networkLayerSizes,
                                                survivorRate, breedingRepartitionCoef, freshBloodCoef,
                                                synapsesMutationRate, synapsesMutationRange,
                                                inputNbMutation, inputNbMutationRate,
                                                hiddenLayerNbMutation, hiddenLayerNbMutationRate,
                                                hiddenNbMutation, hiddenMbMutationRate);

            InitializePopulation();
        }

        // starts the next generation when the time has come
        private void Update() {
            if (CarAlive <= 0 || Time.unscaledTime - generationStartTime > maxGenerationTime) {
                NextGeneration();
            }
        }

        ///<summary>
        /// Instantiate the base popuplation and initialises their neural networks
        ///</summary>
        private void InitializePopulation () {

            // create a new racetrack form the list
            raceTrack = GameObject.Instantiate(tracks[Random.Range(0, tracks.Length-1)]);
            raceTrack.transform.SetParent(world);
            
            // creates the population
            carPopulation = new GameObject[numberOfCar];

            for (int i=0; i < numberOfCar; i++) {
                carPopulation[i] = GameObject.Instantiate(carPrefab, CalculateStartPosition(i), Quaternion.identity);
                carPopulation[i].transform.SetParent(populationGroup);
                carPopulation[i].AddComponent<CarLayeredNetController>();
                carPopulation[i].GetComponent<CarLayeredNetController>().NeuralNet = evolution.Population[i];
                carPopulation[i].GetComponent<CarLayeredNetController>().CarDeath += OnCarDeath; // we register to each car's signal for collision
            }

            // all is ready and set to start for the training
            Generation = 1;
            CarAlive = numberOfCar;
            generationStartTime = Time.unscaledTime;
        }

		///<summary>
        /// Initiate the new generation by breeding offspring and inserting them in the current popuplation.
        ///</summary>
        public void NextGeneration () {

            // we make sure everything is stopped
            for (int i=0; i < numberOfCar; i++) {
                carPopulation[i].GetComponent<CarLayeredNetController>().Stop();
            }

            evolution.PrepareNextGeneration();
            
            Debug.Log(  "Generation " + Generation +  " best fitness:" + evolution.GenerationFittestNets[0].FitnessScore +
                        " all time best fitness:" + evolution.AlltimeFittestNets[0].FitnessScore);

            // we make sure dead cars are cleared now that there is a new generation
            deadCars.Clear();

            // we create a new track to avoid overfitting
            Destroy(raceTrack);
            raceTrack = GameObject.Instantiate(tracks[Random.Range(0, tracks.Length-1)]);
            raceTrack.transform.SetParent(world);
            
            // we breed the next generation from the best cars

            // we reset the car position and insert new neural nets
            for (int i=0; i < numberOfCar; i++) {
                carPopulation[i].GetComponent<CarBehaviour>().Reset(CalculateStartPosition(i), Quaternion.identity);
                carPopulation[i].GetComponent<CarLayeredNetController>().NeuralNet = evolution.Population[i];
            }
            
            // all is ready and set to start for the training
            Generation += 1;
            CarAlive = numberOfCar;
            generationStartTime = Time.unscaledTime;
        }

		/// <summary>
		/// Calculates the start position. of each car.
		/// </summary>
        private Vector3 CalculateStartPosition(int i) {
            return startPosition + new Vector3( (i%2 -0.5f) * startPositioSpread, 0, - Mathf.Floor(i/2f) * startPositioSpread);
        }

		///<summary>
        /// Called by each car's colision.
        /// Updates the car alive counter and add the car to the deadCars array to not count them multiple tiem.
        ///</summary>
        private void OnCarDeath (CarBehaviour car) {
            // first we make sure the car hit a wall
            if(!deadCars.Contains(car)) {

                CarAlive -= 1;
                deadCars.Push(car);

                car.GetComponent<CarLayeredNetController>().NeuralNet.FitnessScore = CalculateFitness(car);
            }
        }

		/// <summary>
		/// Computes the fitness value of a network. 
		/// In this case from a mix distance and average speed where distance is more important than speed.
		/// </summary>
        private float CalculateFitness (CarBehaviour car) {
            float distanceFitness = car.DistanceDriven / maxDistFitness;

            float timeElapsed = (Time.unscaledDeltaTime - generationStartTime);
            float speedFitness = timeElapsed== 0? 0f : (car.DistanceDriven / timeElapsed) / car.MaxForwardSpeed;

            float fitness = distanceFitness + distanceFitness * speedFitness;

            return fitness;
        }

	}

}
