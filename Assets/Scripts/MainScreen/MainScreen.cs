using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum SceneName {MainScreen, TrainForCar}

public class MainScreen : MonoBehaviour {

    public static Dictionary<string, string> ScenePaths = new Dictionary<string, string>() {
        {"MainScreen", "Scenes/MainScreen"},
		{"TrainForCar", "Scenes/CarRace/ANNTraining"}
    };

    public static void GoToScene(SceneName scene) {
        SceneManager.LoadScene(ScenePaths[scene.ToString()]);
    }

	public void LoadTrain() {
        GoToScene(SceneName.TrainForCar);
    }
	
	public void Quit() {
        Application.Quit();
    }
}
