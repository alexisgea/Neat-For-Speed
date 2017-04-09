using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace nfs {

    public enum Scenes {MainScreen, TrainForCar}
    public enum Panels {MainMenu, CreditsPanel}

    public class MainScreen : MonoBehaviour {

        public static Dictionary<string, string> ScenesPaths = new Dictionary<string, string>() {
            {"MainScreen", "MainScreen/Scenes/MainScreen"},
            {"TrainForCar", "CarRace/Scenes/TrainForCar"}
        };

        [SerializeField] private RectTransform[] panels;
        private Stack<Panels> panelsStack = new Stack<Panels>();
        private Panels currentPanel = Panels.MainMenu;

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

        public static void GoToScene(Scenes scene) {
            SceneManager.LoadScene(ScenesPaths[scene.ToString()]);
        }

        public void GoToCredits() {
            GoToNewPanel(Panels.CreditsPanel);
        }

        public void LoadTrain() {
            GoToScene(Scenes.TrainForCar);
        }

        
        public void Quit() {
            Application.Quit();
        }
    }
}

