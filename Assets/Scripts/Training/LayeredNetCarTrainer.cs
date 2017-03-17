using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using nfs.car;

namespace nfs.trainers {
	public class LayeredNetCarTrainer : MonoBehaviour {

		[SerializeField] int numberOfGeneration = 100;
        [SerializeField] float maxGenerationTime = 90;
        public int Generation { private set; get; }

        [SerializeField] int numberOfCar = 20;
        public int CarAlive { private set; get; }

        [SerializeField] int[] baseHiddenLayersSizes;

        [SerializeField] float survivorRate = 0.25f;
        private int breedingSample;
        [SerializeField] float freshBloodRate = 0.1f;
        [SerializeField] float synapsesMutationRate = 0.1f;
        [SerializeField] float synapsesMutationRange = 0.1f;
        [SerializeField] bool inputNbMutation = false;
        [SerializeField] bool hiddenNbMutation = false;
        [SerializeField] float hiddenMbMutationRate = 0.01f;
        [SerializeField] bool hiddenLayerNbMutation = false;
        [SerializeField] float hiddenLayerNbMutationRate = 0.001f;

        private GameObject[] carPopulation;
        private Stack<CarBehaviour> deadCars = new Stack<CarBehaviour>();
        private LayeredNetwork[] alltimeFittestNets;
        private LayeredNetwork[] generationFittestNets;
        [SerializeField] Transform populationGroup;
        [SerializeField] Vector3 startPosition = Vector3.zero;
        [SerializeField] GameObject carPrefab;

        [SerializeField] float maxDistFitness = 1000;
        private float generationStartTime;

        // world and race tracks references
        [SerializeField] Transform world;
        [SerializeField] GameObject[] tracks;
        private GameObject raceTrack;

		 // Initialises the base popuplations
        private void Start() {
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
        public void InitializePopulation () {
            // create a new racetrack form the list
            raceTrack = GameObject.Instantiate(tracks[Random.Range(0, tracks.Length-1)]);
            raceTrack.transform.SetParent(world);

            // compute the amount of network which will have a descendance in each generation
            breedingSample = (int)(numberOfCar*survivorRate);
            breedingSample = breedingSample < 1 ? 1 : breedingSample;
            
            // creates the population
            carPopulation = new GameObject[numberOfCar];
            alltimeFittestNets = new LayeredNetwork[breedingSample];
            generationFittestNets = new LayeredNetwork[breedingSample];

            for (int i=0; i < numberOfCar; i++) {
                carPopulation[i] = GameObject.Instantiate(carPrefab, startPosition, Quaternion.identity);
                carPopulation[i].transform.SetParent(populationGroup);
                carPopulation[i].GetComponent<LayeredNetController>().InitializeNeuralNetwork(hiddenSizes:baseHiddenLayersSizes);
                carPopulation[i].GetComponent<CarBehaviour>().HitSomething += OnCarHit; // we register to each car's signal for collision
            }

            // all is ready and set to start for the training
            Generation = 1;
            CarAlive = numberOfCar;
            generationStartTime = Time.unscaledTime;
        }

		///<summary>
        /// Initiate the new generation by breeding offspring and inserting them in the current popuplation.
        ///</summary>
        private void NextGeneration () {
            
            Debug.Log("Generation " + Generation +  " best fitness:" + generationFittestNets[0].FitnessScore + " all time best fitness:" + alltimeFittestNets[0].FitnessScore);

            // we make sure everything is stopped and reset their state
            for (int i=0; i < numberOfCar; i++) {
                carPopulation[i].GetComponent<CarBehaviour>().Stop = true;
                carPopulation[i].GetComponent<CarBehaviour>().Reset(startPosition, Quaternion.identity);
            }

            // we breed the next generation from the best cars
            BreedNextGeneration();
            // we make sure dead cars are cleared now that there is a new generation
            deadCars.Clear();
            // we also make sure that the generation fittest array can be beaten by the next one (easier than nullifying)
            for (int i = 0; i < breedingSample; i++) {
                generationFittestNets[i].FitnessScore = 0;
            }

            // we create a new track to avoid overfitting
            Destroy(raceTrack);
            raceTrack = GameObject.Instantiate(tracks[Random.Range(0, tracks.Length-1)]);
            raceTrack.transform.SetParent(world);
            
            // all is ready and set to start for the training
            Generation += 1;
            CarAlive = numberOfCar;
            generationStartTime = Time.unscaledTime;
        }

		///<summary>
        /// Breed the next generation of networks from the current best and overall best.
        ///</summary>
        private void BreedNextGeneration() {

            int k = 0;
            int l = 0;
            for (int i=0; i < numberOfCar; i++) {
                if (i < numberOfCar*0.6) { // all time generation (60%)
                    carPopulation[i].GetComponent<LayeredNetController>().SetLayeredNework(CreateMutatedOffspring(alltimeFittestNets[k], l+1)); // l+1 as mutate coef to make each version more propable to mutate a lot
                    l += 1;
                    if (l> numberOfCar*0.6/breedingSample){
                        l = 0;
                        k += 1;
                    }

                } else if (i < numberOfCar*0.9) { // this generation (30% 0.6 + 0.3)
                    carPopulation[i].GetComponent<LayeredNetController>().SetLayeredNework(CreateMutatedOffspring(generationFittestNets[k], l+1));
                    l += 1;
                    if (l> numberOfCar*0.9/breedingSample){
                        l = 0;
                        k += 1;
                    }

                } else { // fresh blood (10%)
                    carPopulation[i].GetComponent<LayeredNetController>().InitializeNeuralNetwork(hiddenSizes:baseHiddenLayersSizes);   
                }

                if(k>=breedingSample)
                    k = 0;
            }
        } 

		///<summary>
        /// Called by each car's colision.
        /// Updates the car alive counter and add the car to the deadCars array to not count them multiple tiem.
        ///</summary>
        private void OnCarHit (CarBehaviour who, string what) {
            // first we make sure the car hit a wall
            if(what == "wall" && !deadCars.Contains(who)) {

                CarAlive -= 1;
                deadCars.Push(who);
                who.Stop = true;

                // we retrive the neural network from the car and check it's fitness
                LayeredNetwork fitnessContender = who.GetComponent<LayeredNetController>().GetLayeredNetClone();
                fitnessContender.FitnessScore = CalculateFitness(who);

                // we then compare the fitness to the current best and store/sort it if good
                CompareFitness(alltimeFittestNets, fitnessContender);
                CompareFitness(generationFittestNets, fitnessContender);

            }
        }

		// compute the fitness value of a network from a mix distance and average speed
        // distance is more important than speed
        private float CalculateFitness (CarBehaviour who) {
            float distanceFitness = who.DistanceDriven / maxDistFitness;

            float timeElapsed = (Time.unscaledDeltaTime - generationStartTime);
            float speedFitness = timeElapsed== 0? 0f : (who.DistanceDriven / timeElapsed) / who.MaxForwardSpeed;

            float fitness = distanceFitness + distanceFitness * speedFitness;

            return fitness;
        }

	}

}
