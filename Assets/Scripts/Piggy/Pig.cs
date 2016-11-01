﻿using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

[RequireComponent (typeof (LoadItemsOnPig))]
public class Pig : MonoBehaviour {

	//VARIABLLE DECLARATIONS
	public static GameObject player;

	//UI button objects
	public GameObject forwardButton;
	public GameObject backwardButton;
	public GameObject jumpButton;
	public GameObject kickButton;
	public AnimationClip kickAnimation; // Can be forward or backward since they should be the same time

	private bool jump;
	private bool kicked; // Make piggy only kick once per kick animation
	private AnimatorStateInfo currStateInfo;
	private AnimatorStateInfo newStateInfo;
	private float kickAnimationTime;
	private float jumpMovementScale; // Multiplier for moving while in the air. Should be < 1

	//Physical piggy paramters
	private Rigidbody2D rb;
	private Animator piggyAnimator;

	// The current surface piggy is on
	private GameObject standingOn;

	private bool gameStart;
	private bool bumped; // If piggy collides with a landable while walking
	private GameObject bumpedObject;

	private Vector2 lastVelocity = Vector2.zero;
	private Vector2 startingPosition;
//	private float startingTime;
	private float jumpTime;

	// Properties

	public bool GameStart {
		get { return gameStart; }
		set { gameStart = value; }
	}

	public bool Kicking {
		get { return piggyAnimator.GetBool(ConstantValues.piggyAnimatorParameterNames.kick); }
	}

	public bool Jumping {
		get { return piggyAnimator.GetBool(ConstantValues.piggyAnimatorParameterNames.jump); }
	}

	// Make piggy only kick once per kick animation
	public bool Kicked {
		get { return kicked; }
		set { kicked = value; }
	}

	public Animator PiggyAnimator {
		get { return piggyAnimator; }
	}

	public GameObject StandingOn {
		get { return standingOn; }
	}

	public void ModifySpeed(float modifier) {
		LevelManager.levelManager.speedModifier = modifier;
	}

	public void ModifyJump(float modifier) {
		LevelManager.levelManager.jumpModifier = (int)modifier;
	}

	void Start() {
		piggyAnimator = GetComponent<Animator> ();
		rb = GetComponent<Rigidbody2D>();
		player = gameObject;
		jump = false;
		kicked = false;
		gameStart = true;
		bumped = false;
		currStateInfo = piggyAnimator.GetCurrentAnimatorStateInfo(0);
		kickAnimationTime = kickAnimation.length;
		jumpMovementScale = 0.5f;

		// Deactivate all buttons until Piggy lands initially
		ChangeButtonStatusAll (false);
	}

	// Activate or deactivate buttons while an animation is happening
	public void ChangeButtonStatusAll(bool interactable) {
		ChangeButtonStatusIndividual (forwardButton, interactable);
		ChangeButtonStatusIndividual (backwardButton, interactable);
		ChangeButtonStatusIndividual (jumpButton, interactable);
		ChangeButtonStatusIndividual (kickButton, interactable);
	}

	void ChangeButtonStatusIndividual(GameObject button, bool interactable) {
		float alpha;
		if (!interactable) {
			alpha = 0.5f;
		} else {
			alpha = 1f;
		}
		Image buttonImage = button.GetComponent<Image> ();
		buttonImage.color = new Color(buttonImage.color.r, buttonImage.color.g, buttonImage.color.b, alpha);
		buttonImage.raycastTarget = interactable;
	}

	IEnumerator KickDuration() {
		yield return new WaitForSeconds(kickAnimationTime);
		piggyAnimator.SetBool(ConstantValues.piggyAnimatorParameterNames.kick, false);
		kicked = false;
		if (!CameraController.cameraController.Panning) {
			ChangeButtonStatusAll (true);
		}
	}

	public void MoveWithInput(string parameterName, bool active) {
		piggyAnimator.SetBool(parameterName, active);
	}

	public void MoveForward() {
		if (!piggyAnimator.GetBool (ConstantValues.piggyAnimatorParameterNames.backward)) {
			piggyAnimator.SetBool (ConstantValues.piggyAnimatorParameterNames.forward, true);
		}
	}

	public void MoveBackward() {
		if (!piggyAnimator.GetBool(ConstantValues.piggyAnimatorParameterNames.forward)) {
			piggyAnimator.SetBool(ConstantValues.piggyAnimatorParameterNames.backward, true);
		}
	}

	public void StopForward() {
		if (!piggyAnimator.GetBool (ConstantValues.piggyAnimatorParameterNames.backward)) {
			piggyAnimator.SetBool (ConstantValues.piggyAnimatorParameterNames.forward, false);
		}
	}

	public void StopBackward() {
		if (!piggyAnimator.GetBool (ConstantValues.piggyAnimatorParameterNames.forward)) {
			piggyAnimator.SetBool (ConstantValues.piggyAnimatorParameterNames.backward, false);
		}
	}

	public void Jump() {
		Debug.Log("Jump button");
		jump = true;
	}

	public void Kick() {
		Debug.Log("Kick button");
		piggyAnimator.SetBool(ConstantValues.piggyAnimatorParameterNames.kick, true);
		ChangeButtonStatusAll(false);
		StartCoroutine(KickDuration());
	}

	void Update() {

//		if (!gameStart && piggyAnimator.GetBool(ConstantValues.piggyAnimatorParameterNames.jump)) {
//			using (System.IO.StreamWriter file = new System.IO.StreamWriter (@"C:\Users\SheeperT\Unity\Little Piggy 1.1\SpeedAndJumpData.txt", true)) {
//				file.WriteLine ("time: " + (Time.fixedTime - startingTime) + ", x: " + (transform.position.x - startingPosition.x) + ", y: " + (transform.position.y - startingPosition.y));
//			}
//		}

		// Highest point in jump
//		if ((rb.velocity.y < 0) && (lastVelocity.y > 0)) {
//			jumpTime = Time.fixedTime - jumpTime;
//			Debug.Log ("Force = " + LevelManager.piggyJump + " Height = " + (transform.position.y - startingPosition.y) + " Time = " + jumpTime);
//		}

//		Debug.Log ("startingHeight = " + startingPosition.y);

		newStateInfo = piggyAnimator.GetCurrentAnimatorStateInfo(0);

		if (ChangedDirection (currStateInfo, newStateInfo)) {
			for (int c = 0; c < transform.childCount; c++) {
				if (gameObject.transform.GetChild (c).gameObject.GetComponent<MoveWithPig> ()) {
					gameObject.transform.GetChild (c).gameObject.GetComponent<MoveWithPig> ().ChangeSides ();
				}
			}
		}
		currStateInfo = newStateInfo;

		#if UNITY_EDITOR // Same thing PigControlButtons.cs does
		if (Input.GetKeyDown(KeyCode.D)) {
//			Pig.player.GetComponent<Pig>().MoveForward ();
			Pig.player.GetComponent<Pig>().MoveWithInput(ConstantValues.piggyAnimatorParameterNames.forward, true);
		} 
		if (Input.GetKeyDown(KeyCode.A)) {
//			Pig.player.GetComponent<Pig>().MoveBackward ();
			Pig.player.GetComponent<Pig>().MoveWithInput(ConstantValues.piggyAnimatorParameterNames.backward, true);
		} 
		if (Input.GetKeyUp(KeyCode.D)) {
//			Pig.player.GetComponent<Pig>().StopForward ();
			Pig.player.GetComponent<Pig>().MoveWithInput(ConstantValues.piggyAnimatorParameterNames.forward, false);
		}
		if (Input.GetKeyUp(KeyCode.A)) {
//			Pig.player.GetComponent<Pig>().StopBackward ();
			Pig.player.GetComponent<Pig>().MoveWithInput(ConstantValues.piggyAnimatorParameterNames.backward, false);
		}
		if (!Jumping && Input.GetKeyUp(KeyCode.K)) {
			Pig.player.GetComponent<Pig>().Kick ();
		}
		if (!Jumping && Input.GetKeyDown(KeyCode.L)) {
			Pig.player.GetComponent<Pig>().Jump ();
		}
		#endif
	}
		
	void FixedUpdate() {
		// Physics end of piggy state machine
		if (jump) {
			// Start jump
//			Debug.Log("Start jump " + "mass = " + rb.mass + " time = " + Time.fixedDeltaTime + " jump = " + LevelManager.piggyJump);
			startingPosition = transform.position;
//			startingTime = Time.fixedTime;
//			Debug.Log ("Starting velocity = " + rb.velocity);
			rb.AddForce (Vector3.up * LevelManager.piggyJump, ForceMode2D.Impulse);
			StartJumpAnimation ();
			jumpTime = Time.fixedTime;
			jump = false;
		}

		// Move piggy on ground or in air if not over max horizontal speed
		if (Mathf.Abs(rb.velocity.x) <= LevelManager.piggySpeed) {
			if (standingOn) {
				// FIXME Piggy spazzes when forward and backward are pressed at the same time (queue it up?)
				// Walk forward
				if (piggyAnimator.GetBool (ConstantValues.piggyAnimatorParameterNames.forward)) {
					rb.AddForce (Vector3.right * LevelManager.piggySpeed, ForceMode2D.Force);
				}
				// Walk backward
				if (piggyAnimator.GetBool (ConstantValues.piggyAnimatorParameterNames.backward)) {
					rb.AddForce (Vector3.left * LevelManager.piggySpeed, ForceMode2D.Force);
				}
//				Debug.Log ("velocity  = " + rb.velocity);
			} else {
				// Move forward while in the air
				if (piggyAnimator.GetBool (ConstantValues.piggyAnimatorParameterNames.forward)) {
					rb.AddForce (new Vector2 (LevelManager.piggySpeed * jumpMovementScale, 0f), ForceMode2D.Force);
				}
				// Move backward while in the air
				if (piggyAnimator.GetBool (ConstantValues.piggyAnimatorParameterNames.backward)) {
					rb.AddForce (new Vector2 (LevelManager.piggySpeed * -jumpMovementScale, 0f), ForceMode2D.Force);
				}
			}
		}
		lastVelocity = rb.velocity;
	}

	public void ToggleActiveMovementButtons(bool isActive) {
		forwardButton.SetActive(isActive);
		backwardButton.SetActive(isActive);
		jumpButton.SetActive(isActive);
		kickButton.SetActive(isActive);
	}

	// TODO Make this not rely on state info
	bool ChangedDirection(AnimatorStateInfo currStateInfo, AnimatorStateInfo newStateInfo) {
		return ((currStateInfo.IsName("PiggyForward") && newStateInfo.IsName("PiggyBackward")) ||
			(currStateInfo.IsName("PiggyIdleForward") && newStateInfo.IsName("PiggyBackward")) ||
			(currStateInfo.IsName("PiggyBackward") && newStateInfo.IsName("PiggyIdleForward")) ||
			(currStateInfo.IsName("PiggyIdleBackward") && newStateInfo.IsName("PiggyIdleForward")) ||
			(currStateInfo.IsName("PiggyBackward") && newStateInfo.IsName("PiggyForward")) ||
			(currStateInfo.IsName("PiggyIdleBackward") && newStateInfo.IsName("PiggyForward")) ||
			(currStateInfo.IsName("PiggyJumpForward") && newStateInfo.IsName("PiggyJumpBackward")) ||
			(currStateInfo.IsName("PiggyJumpBackward") && newStateInfo.IsName("PiggyJumpForward")));
	}

	// Lands the piggy if he hits the floor
	void OnCollisionEnter2D(Collision2D other) {
		Debug.Log("Entering " + other.collider.gameObject.name);

		string collisionTag = other.collider.gameObject.tag;

		// Check if Piggy has fallen off the world
		if (collisionTag.Contains(ConstantValues.tags.catcher)) {
			LevelManager.piggyFallen = true;
		}

		// Cases for colliding with a platform or the ground
		if (collisionTag.Contains(ConstantValues.tags.landable)) {
			// Lands the piggy if jumping and hits the floor
			if (!standingOn && (rb.velocity.y == 0)) {
				LandPiggy (other.gameObject);
				// Moving platforms
				if (collisionTag.Contains(ConstantValues.tags.moving)) {
					// Make piggy move relative to platform
					player.transform.parent = other.gameObject.transform;
					Debug.Log("On moving platform.");
				} //else {
//					// Remove piggy from platform. Move normally
//					player.transform.parent = null;
				}
//			} else if (standingOn) {
//				// Piggy is already on the ground so no need to land. bumped is checked when exiting a landable
//				bumped = true;
//				bumpedObject = other.gameObject;
//				Debug.Log ("Bump start: " + bumpedObject.name);
//			}
		}
	 }
//	private bool notMoving;
//	private float startVel, endVel;
//	void OnCollisionStay2D(Collision2D other) {
//		Wait (0.1f);
//	}
//
//	IEnumerator Wait(float time) {
//		startVel = rb.velocity.y;
//		yield return new WaitForSeconds (time);
//		endVel = rb.velocity.y;
//		if (startVel == endVel) {
//			notMoving = true;
//		}
//	}

	void LandPiggy(GameObject landedOn) {
		standingOn = landedOn;
		if (standingOn.tag.Contains(ConstantValues.tags.heightChange)) {
			CameraController.cameraController.Ground = standingOn;
		}
		Debug.Log("Landed. Standing on " + standingOn);
//		Debug.Log ("Jump length = " + (transform.position.x - startingPosition.x));
		piggyAnimator.SetBool(ConstantValues.piggyAnimatorParameterNames.jump, false);
		if (gameStart) {
			gameStart = false;
		}
		ChangeButtonStatusAll(true);
	}

	void StartJumpAnimation() {
		piggyAnimator.SetBool (ConstantValues.piggyAnimatorParameterNames.jump, true);
		ChangeButtonStatusIndividual (kickButton, false);
		ChangeButtonStatusIndividual (jumpButton, false);
		standingOn = null;
		Debug.Log ("Start jump animation");
	}

	void OnCollisionExit2D(Collision2D other) {
		string collisionTag = other.collider.gameObject.tag;
		Debug.Log("Exiting " + other.collider.gameObject.name);

		// Covers cases for piggy leaving the ground or a platform, including bumping into a platform while walking
		if (other.gameObject.Equals(standingOn)) {
//			if (!bumped) {
				// Go into jump animation if leaving the ground by falling or jumping, but not by going through a one-way platform when piggy is already jumping
			if (!piggyAnimator.GetBool (ConstantValues.piggyAnimatorParameterNames.jump)) {
					StartJumpAnimation ();
			}
			// Moving platforms
			if (collisionTag.Contains(ConstantValues.tags.moving)) {
				// Remove piggy from platform after jumping. Move normally
				Debug.Log("Off moving platform.");
				player.transform.parent = null;
			}
//			} else if (bumped && (other.gameObject.Equals(bumpedObject))){
//				// Reset bumped for next time piggy walks into a platform
//				bumped = false;
//				bumpedObject = null;
//				Debug.Log ("Bump end.");
//			}
	 	}
	 }
}
