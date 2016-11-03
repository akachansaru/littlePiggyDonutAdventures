﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LevelManager : MonoBehaviour {

	public static LevelManager levelManager;
	public static int levelPayment;
	public static bool piggyFallen;
	public static bool levelComplete;
	public static bool paused;
	public static float piggySpeed;
	public static float piggyJump;
	public static float piggyDamage;

//	public float height1;
//	public float height2;
//	public float height3;
//	public float height4;
//	public float height5;
//	public float height6;

	public GameObject player;
	public int levelNumber;
	public GameObject goal;
	public float speedScale;
	public float damageScale;
	public float baseSpeed;
	public float baseDamage;
	public float speedModifier;
	public int jumpModifier;
	public float damageModifier;
	//UI stuff
	public GameObject failScreen;
	public GameObject pauseScreen;
	public Button settingsButton;
	public Button pauseButton;
	public Button unpauseButton;
	public Button itemButton;
	public GameObject levelCompleteScreen;
	public GameObject finalDonutCount;
	// Donuts
	public GameObject cinnamonHolePrefab;
	public GameObject chocolateHolePrefab;
	public GameObject sprinklesHolePrefab;
	public GameObject chocolatePrefab;
	public GameObject strawberryPrefab;

	public LevelSaveValues levelInstance = new LevelSaveValues();
//	public LevelSaveValues savedLevelInstance = new LevelSaveValues();

	private Text finalDonutCountText;
	private GameObject donutPrefab;
	private int modifiedDonutCount;

	void Start() {
		levelManager = this;
		GlobalControl.Instance.Load();
		speedModifier = 1f;
		jumpModifier = 0;
		damageModifier = 1f;
//		LoadPigItems ();
		levelInstance.donutsCollected = 0;
		levelInstance.clothCollected = 0;
		levelInstance.levelDonutCount = levelPayment;
		piggyFallen = false;
		levelComplete = false;
		paused = false;
		ScalePiggyStats(); // Initially will be scaled on levelPayment + wearable item modifiers
	}

	void Update() {
		ScalePiggyStats();
		if (piggyFallen) {
			LoadLastLevelInstance();
		}
		if (levelComplete) {
			LevelComplete();
		}
	}

	public GameObject GetDonutPrefab(string donutName) {
		if (donutName == ConstantValues.donutNames.cinnamonHole) {
			donutPrefab = cinnamonHolePrefab;
		} else if (donutName == ConstantValues.donutNames.chocolateHole) {
			donutPrefab = chocolateHolePrefab;
		} else if (donutName == ConstantValues.donutNames.sprinklesHole) {
			donutPrefab = sprinklesHolePrefab;
		} else if (donutName == ConstantValues.donutNames.chocolate) {
			donutPrefab = chocolatePrefab;
		} else if (donutName == ConstantValues.donutNames.strawberry) {
			donutPrefab = strawberryPrefab;
		} else {
			Debug.Log ("Incorrect donut name");
			donutPrefab = cinnamonHolePrefab;
		}
		return donutPrefab;
	}

	IEnumerator InstantiateDonut(GameObject donutPrefab, Vector2 position) {
		yield return new WaitForSeconds(2f);
		Instantiate(donutPrefab, position, Quaternion.identity);
	}

	// Returns the maximum height piggy can jump to with the current amount of donuts
	int DonutToHeight(int donutCount) {
		if (modifiedDonutCount < 5) {
			return 1;
		} else if (modifiedDonutCount < 50) {
			return 2;
		} else if (modifiedDonutCount < 150) {
			return 3;
		} else if (modifiedDonutCount < 325) {
			return 4;
		} else if (modifiedDonutCount < 700) {
			return 5;
		} else {
			return 6;
		}
	}

	// Returns the jump input for the donut range
	float HeightToJump(int height) {
		// See JumpData.xlsx for equation details. (These #'s from using the formula h = 5.098j^2 - 0.5028j + 0.0134)
		if (height == 1) {
			return 0.48f;
//			return height1;
		} else if (height == 2) {
			return 0.67f;
//			return height2;
		} else if (height == 3) {
			return 0.80f;
//			return height3;
		} else if (height == 4) {
			return 0.91f;
//			return height4;
		} else if (height == 5) {
			return 1.02f;
//			return height5;
		} else {
			return 1.11f;
//			return height6;
		}
	}
	
	void ScalePiggyStats() {
		modifiedDonutCount = levelInstance.levelDonutCount + jumpModifier;
		int height = DonutToHeight (modifiedDonutCount);
		piggyJump = HeightToJump(height);
		piggySpeed = (speedScale * levelInstance.levelDonutCount + baseSpeed) * speedModifier;
		piggyDamage = (damageScale * levelInstance.levelDonutCount + baseDamage) * damageModifier;
	}

	public void TogglePauseGame() {
		if (!paused) {
			PauseGame();
		} else {
			UnpauseGame();
		}
	}

	public void PauseGame() {
		paused = true;
		if (pauseScreen) {
			pauseScreen.SetActive (true);
		}
		if (pauseButton) {
			pauseButton.gameObject.SetActive (false);
			unpauseButton.gameObject.SetActive (true);
		}
		Time.timeScale = 0f;
		player.GetComponent<Pig>().ToggleActiveMovementButtons(false);
	}

	public void UnpauseGame() {
		paused = false;
		if (pauseScreen) {
			pauseScreen.SetActive (false);
		}
		if (pauseButton) {
			unpauseButton.gameObject.SetActive (false);
			pauseButton.gameObject.SetActive (true);
		}
		Time.timeScale = 1f;
		player.GetComponent<Pig>().ToggleActiveMovementButtons(true);
	}

	void LevelComplete() {
		player.GetComponent<Pig>().ToggleActiveMovementButtons(false);
		player.GetComponent<Pig> ().StopForward ();
		player.GetComponent<Pig> ().StopBackward();
		levelComplete = false;
		levelCompleteScreen.SetActive(true);
		finalDonutCountText = finalDonutCount.GetComponent<Text>();
		finalDonutCountText.text = finalDonutCountText.text + " " + levelInstance.donutsCollected.ToString();
		// UNDONE Add cloth count to end screen
		GlobalControl.Instance.savedData.SafeDonutCount += levelInstance.donutsCollected;
		GlobalControl.Instance.savedData.SafeClothCount += levelInstance.clothCollected;
		if (GlobalControl.Instance.savedData.CompletedLevels.Length > (levelNumber + 1)) {
			GlobalControl.Instance.savedData.CompletedLevels[levelNumber + 1] = true;
			Debug.Log("Level " + (levelNumber + 1) + " unlocked");
		}
		
		DeactivateButtons();
		GlobalControl.Instance.Save();
		Debug.Log("Level complete.");
	}

	public void DeactivateButtons() {
		if (settingsButton) {
			settingsButton.gameObject.SetActive (false);
		}
		if (itemButton) {
			itemButton.gameObject.SetActive (false);
		}
		if (pauseButton) {
			pauseButton.gameObject.SetActive (false);
			unpauseButton.gameObject.SetActive (false);
		}
	}

	public void ActivateButtons() {
		if (settingsButton) {
			settingsButton.gameObject.SetActive (true);
		}
		if (itemButton) {
			itemButton.gameObject.SetActive (true);
		}
		if (unpauseButton) {
			unpauseButton.gameObject.SetActive (true);
		}
	}

	// Called when a checkpoint is reached
	// This is only needed if the progress is reset after player falls off
//	public void SaveLevelInstance() {
//		savedLevelInstance.Clone(levelInstance);
//		Debug.Log("Saved level instance.");
//	}

	void LoadLastLevelInstance() {
		int fallPenalty = 10; // UNDONE Need to scale this on level somehow
		piggyFallen = false;
//		levelInstance.Clone(savedLevelInstance);
		levelInstance.donutsCollected = (LevelManager.levelManager.levelInstance.levelDonutCount - fallPenalty >= 0) ? 
			(LevelManager.levelManager.levelInstance.donutsCollected - fallPenalty) : -LevelManager.levelPayment;
		player.GetComponent<Pig>().ChangeButtonStatusAll (false);
		player.transform.position = levelInstance.lastCheckpoint.transform.position;
		Debug.Log("Loaded level instance");
	}

	// void LevelFailed() {
	// 	player.GetComponent<Pig>().ToggleActiveMovementButtons(false);
	// 	piggyFallen = false;
	// 	failScreen.SetActive(true);
	// 	exitButton.SetActive(false);
	// 	StartCoroutine(EndGame(3F));
	// }

	IEnumerator EndGame(float timeDelay) {
		yield return new WaitForSeconds(timeDelay);
		Application.Quit();
	}
}
