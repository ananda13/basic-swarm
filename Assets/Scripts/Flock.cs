using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flock : MonoBehaviour {


	public float angularSpeed = 1.0f;  //?
	//private Vector3 averageHeading;
	//private Vector3 averagePosition;
	public float neighborDistance = 2.0f;  // The max distance to its closest neighbor, within which it will display flocking behavior
	public float collisionDistance = 0.2f; // The minimum distance to its neighbors, at which it will avoid those fish
	public float startingSpeed = 0.5f;
	public float maxSpeed = 1.0f;			//clamp the speed to this value.
	public float predatorDistance = 2.0f;

	private float speed  = 0.5f;  //meters per second
	private GameObject[] allFishHandle, allPredatorHandle;
	private Vector3 goalPosHandle;

	//When you're at the edge of the space, turn around
	public bool turnAround = false;
	public Vector3 returnPosition = new Vector3(0, 1,0);

	public float sGoalSeeking = 1.0f;
	public float sPredatory = 1.0f;
	public float sCohesion = 1.0f;
	public float sAvoidance = 1.0f;

	// Use this for initialization
	void Start () 
	{
		//Apply a random speed to initialize the fish with, so that they all dont start with the same velocity
		speed = Random.Range (startingSpeed/2.0f, startingSpeed);	

		//Get the handle to all other fish & goal position from globalFlock
		allFishHandle = GlobalFlock.allFish;
		allPredatorHandle = GlobalFlock.allPredators;
				
	}
	
	// Update is called once per frame
	void Update () 
	{
		//float distanceFromCenter = Vector3.Distance (this.transform.position, Vector3.zero);

		//if ((distanceFromCenter >= (GlobalFlock.tankSize * 0.95)) || (this.transform.position.y <= 0.1f))
		if((this.transform.position.x <= -GlobalFlock.tankSize) || (this.transform.position.x >= GlobalFlock.tankSize) ||
			(this.transform.position.z <= -GlobalFlock.tankSize) || (this.transform.position.z >= GlobalFlock.tankSize) ||
			(this.transform.position.y <= 0.1f) || (this.transform.position.x >= GlobalFlock.tankHeight))
		{			
			applyReturn();
			//Debug.Log ("Return for Id: " + GetInstanceID());

		} 

		else 
		{
			//if (Random.Range (0, 5) < 1)
				ApplySwarm ();
		}

		//Move the fish now

		//Should be using Vector3.forward , but the Z axis (forward) of the fish, is actually towards the left.
		//transform.Translate (/*transform.localRotation * */Vector3.left * speed * Time.deltaTime);
		//transform.Translate(Time.deltaTime * speed,0,0);
		transform.Translate(0,0, Time.deltaTime * speed);
		//transform.Translate (Vector3.left*speed*Time.deltaTime);
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



	private void ApplySwarm()
	{
		//Group Speed
		float gSpeed = 0.1f;


		//Vectors that need to be calculated
		//Vector that points towards the center of the flock
		Vector3 vCohesion = Vector3.zero;

		//Vector that avoids the other fish
		Vector3 vAvoid = Vector3.zero;

		//Vector that heads towards goal
		Vector3 vGoal = Vector3.zero;

		//Vector that avoid predators
		Vector3 vPredatory = Vector3.zero;

		//local vars
		float dist = 0.0f;
		int groupSize = 0;
		float sPr = sPredatory;
		float sGo = sGoalSeeking;
		float sCo = sCohesion;
		float sAv = sAvoidance;

		goalPosHandle = GlobalFlock.goalPos;

		//Grouping and collision avoidance
		foreach (GameObject fish in allFishHandle) 
		{
			//If it is not *this* fish
			if (fish != this.gameObject) 
			{

				//Calc *maginitude* of the distance to this fish
				dist = Vector3.Distance(fish.transform.position, this.transform.position);

				//Check if this fish lies within my neighbor range
				if (dist <= neighborDistance) 
				{
					groupSize++;

					//New center
					vCohesion += fish.transform.position;

					if (dist <= collisionDistance)
						vAvoid -= this.transform.position - fish.transform.position;

					//Find the group speed
					float otherSpeed = fish.GetComponent<Flock>().speed;
					gSpeed += otherSpeed;
					//Debug.Log (otherSpeed);


				}


			}

		}


		//If this fish is within a group, change its position and velocity
		if (groupSize > 0) {

			vCohesion = vCohesion / groupSize;


			//Set this fish's speed
			speed = gSpeed / groupSize;
			//Clamp the top speed (safety)
			speed = Mathf.Clamp (speed, startingSpeed, maxSpeed);

			//Debug.Log ("Id: " + GetInstanceID() +" Group Size: " + groupSize + "  Speed: " + speed + " Goal: " + goalPosHandle);

			//Set it's orientation
			//Relative Position (or direction) = Target position (i.e. vCohesion - vAvoid) minus current position
			//example : https://docs.unity3d.com/ScriptReference/Quaternion.LookRotation.html
			//Vector3 direction = (vCohesion + vAvoid) - this.transform.position;

			//Randomly reverse the direction of flocking, away from center
			/*float reverse_dir = 1.0f;
			if (Random.Range (0, 100) < 1) {
				Debug.Log ("Reversed");
				reverse_dir = -1.0f;
			}

			Vector3 direction = ((vCohesion * reverse_dir) - vAvoid) - this.transform.position;*/
						



		} else
			sCo = 0f;
		

		//Goal Seeking Behavior
		vGoal = goalPosHandle - this.transform.position;

		//Avoid predators
		foreach (GameObject predator in allPredatorHandle) {

			//Find the predator within predatorDistance
			dist = Vector3.Distance(predator.transform.position, this.transform.position);

			if (dist <= predatorDistance) {
				vPredatory -= this.transform.position - predator.transform.position;

				Debug.Log ("Pred found " + dist + "m away");
			}
		}




		Vector3 direction = (vCohesion * sAv) + (vGoal * sGo) - (vAvoid * sAv) - (vPredatory * sPr) - this.transform.position;

		//Slowly turn in that direction
		transform.rotation = Quaternion.Slerp ( this.transform.rotation,
			Quaternion.LookRotation (direction) ,
			angularSpeed * Time.deltaTime);




	}

	public float GetSpeed()
	{
		return speed;
	}


}
