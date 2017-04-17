using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using nfs.nets.layered;
using System.Linq;


namespace nfs {

	public class GetReady : MonoBehaviour {

		[SerializeField] private Visualiser visualiser;
		[SerializeField] private Transform networkListView;
		[SerializeField] private GameObject networkItemPrefab;
		[SerializeField] private int maxNbHiddenLayer = 7;
		[SerializeField] private int maxHiddenLayerSize = 7;

		private SrLife loadedData;		

		// Use this for initialization
		void Start () {
			LoadNetworksAndSetPanel();
		}
		
		// Update is called once per frame
		void Update () {
			
		}

		private void LoadNetworksAndSetPanel() {
			loadedData = Serializer.LoadAllSpecies(GetComponentInParent<MainScreen>().CurrentSim);

			for (int i = 0; i < loadedData.Species.Length; i++) {
				GameObject newNetItem = Instantiate(networkItemPrefab);
				newNetItem.transform.SetParent(networkListView);
				newNetItem.transform.localScale = new Vector3(1,1,1);
				newNetItem.transform.localPosition = new Vector3(0,0,0);
				SrSpecies sp = loadedData.Species[i];
				newNetItem.GetComponent<NetListItem>().SetFields(sp.Name, sp.Lineage[0].Id, sp.Lineage[0].FitnessScore.ToString("F2"), i);
				newNetItem.GetComponent<NetListItem>().Selected += OnNetworkSelected;
			}
			
		}

		private void OnNetworkSelected (int index) {

			nets.layered.Network selectedNetwork = Serializer.DeserializeNetwork(loadedData.Species[index].Lineage[0]);

			visualiser.SetFocusNetwork(selectedNetwork);
		}

		public void CreateRandomNetwork() {

			int[] layerSizes = new int[2 + Random.Range(0,maxNbHiddenLayer)];
			layerSizes[0] = 3;
			layerSizes[layerSizes.Length-1] = 2;

			Debug.Log("layer number: " + layerSizes.Length);

			for(int i = 1; i < layerSizes.Length - 1; i++) {
				layerSizes[i] = Random.Range(layerSizes[layerSizes.Length-1], maxHiddenLayerSize);
				Debug.Log(layerSizes[i]);
			}

			//String.Join(",", layerSizes.Select(p=>p.ToString()).ToArray())
			Debug.Log("layer sizes: " + string.Join(", ", layerSizes.Select(x=>x.ToString()).ToArray()));


			nets.layered.Network randomNetwork = new nets.layered.Network(layerSizes);
			randomNetwork.PingFwd(new float[3] {1, 1, 1});
			visualiser.SetFocusNetwork(randomNetwork);
		}
	}

}
