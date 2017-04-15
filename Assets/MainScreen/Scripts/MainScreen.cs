using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace nfs {

    public enum Scenes {MainScreen, TrainForCar, Cells}
    public enum Simulations {CarRace, Cells}
    public enum Panels {MainMenu, SimulationList, SimulationModes, BrowsePanel, GetReadyPanel, CreditsPanel}

    public class MainScreen : MonoBehaviour {

        public static Dictionary<Scenes, string> ScenesPaths = new Dictionary<Scenes, string>() {
            {Scenes.MainScreen, "MainScreen/Scenes/MainScreen"},
            {Scenes.TrainForCar, "CarRace/Scenes/TrainForCar"},
            {Scenes.Cells, "Cells/Scenes/Cells"}
        };

        [SerializeField] private RectTransform[] panels;
        private Stack<Panels> panelsStack = new Stack<Panels>();
        private Panels currentPanel = Panels.MainMenu;
        private Simulations currentSim;

        private void Update() {

            if(Input.GetButtonDown("Cancel")) {
                if(panelsStack.Count==0) {
                    Quit();
                }
                else {
                    GoToPreviousPanel();
                }
            }
        }

        private void GoToPanel(Panels newPanel) {
            foreach(RectTransform panel in panels) {
                if (newPanel.ToString() == panel.name) {
                    panel.gameObject.SetActive(true);
                }
                else {
                    panel.gameObject.SetActive(false);
                }
            }

            currentPanel = newPanel;
        }

        private void GoToNewPanel(Panels newPanel) {
            panelsStack.Push(currentPanel);
            GoToPanel(newPanel);

        }

        private void GoToPreviousPanel() {
            GoToPanel(panelsStack.Pop());
        } 

        private string GetSimulationTrainScenePath() {
            switch(currentSim) {
                case Simulations.CarRace:
                    return ScenesPaths[Scenes.TrainForCar];
                case Simulations.Cells:
                    return ScenesPaths[Scenes.Cells];
                default:
                    Debug.LogError("No current simumlation designated for loading.");
                    return "";
            }
        }

        public static void LoadMainScreen() {
            SceneManager.LoadScene(ScenesPaths[Scenes.MainScreen]);
        }

        public void GoToCredits() {
            GoToNewPanel(Panels.CreditsPanel);
        }

        public void GoToSimulationList() {
            GoToNewPanel(Panels.SimulationList);
        }

        public void GotToCarRaceModes() {
            currentSim = Simulations.CarRace;
            GoToNewPanel(Panels.SimulationModes);
        }
         public void GotToCellsModes() {
            currentSim = Simulations.Cells;
            GoToNewPanel(Panels.SimulationModes);
        }

        public void GoToGetReadyPanel() {
            GoToNewPanel(Panels.GetReadyPanel);
        }

        public void LoadTrain() {
            SceneManager.LoadScene(GetSimulationTrainScenePath());
        }

        public void Quit() {
            Application.Quit();
        }



    }
}

