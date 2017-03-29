using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Predator : MonoBehaviour {

	public float angularSpeed = 10.0f;  //?
	public float startingSpeed = 1.0f;
	public float predatorMaxSpeed = 3.0f;
	private float speed  = 0.5f;  //meters per second
	private GameObject[] allFishHandle;

	private GameObject targetFish;
	public Vector3 returnPosition = new Vector3(0, 1,0);

	// Use this for initialization
	void Start () {
		//Apply a random speed to initialize the fish with, so that they all dont start with the same velocity
		speed = Random.Range (startingSpeed/2.0f, predatorMaxSpeed);	
		allFishHandle = GlobalFlock.allFish;
	}
	
	// Update is called once per frame
	void Update()
	{
		if ((this.transform.position.x <= -GlobalFlock.tankSize) || (this.transform.position.x >= GlobalFlock.tankSize) ||
		   (this.transform.position.z <= -GlobalFlock.tankSize) || (this.transform.position.z >= GlobalFlock.tankSize) ||
		   (this.transform.position.y <= 0.1f) || (this.transform.position.x >= GlobalFlock.tankHeight)) {			
			applyReturn ();
			//Debug.Log ("Return for Id: " + GetInstanceID());

		} else {

			if (Random.Range (0, 10) < 1)
				ApplyPredatoryBehavior ();
		}

		transform.Translate(0,0, Time.deltaTime * speed);

	}

	void applyReturn()
	{
		//Towards the center of the tank
		Vector3 direction = returnPosition - this.transform.position; 

		//Slowly turn in that direction
		transform.rotation = Quaternion.Slerp ( this.transform.rotation,
			Quaternion.LookRotation (direction),
			angularSpeed * Time.deltaTime);

		//reset speed:
		//speed = Random.Range (startingSpeed/2.0f, startingSpeed);	
		speed = Mathf.Lerp (speed, startingSpeed, Time.deltaTime);

	}



	void ApplyPredatoryBehavior () {

		float targetDistance = 1000.0f;
		Vector3 targetDirection;

		foreach (GameObject fish in allFishHandle) 
		{
			//Calc *maginitude* of the distance to this fish
			float distance = Vector3.Distance (fish.transform.position, this.transform.position);

			if (distance < targetDistance) { //the closest fish

				targetFish = fish;  //new target


			}
		}

		targetDirection = targetFish.transform.position - this.transform.position;
		//TODO : Scale the speed also

		//Slowly turn in that direction
		transform.rotation = Quaternion.Slerp ( this.transform.rotation,
			Quaternion.LookRotation (targetDirection) ,
			angularSpeed * Time.deltaTime);
		
	}
}
