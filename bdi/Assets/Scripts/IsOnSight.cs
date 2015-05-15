using UnityEngine;
using System.Collections;

public class IsOnSight : MonoBehaviour {

	Move myRobot;

	// Use this for initialization
	void Start () {
		myRobot = this.transform.parent.GetComponent<Move>();
	}


	void Update() {


	}

	bool IsObjectOnSight(Collider collider) {
		RaycastHit hit;
		//Vector3 direction = collider.transform.position - transform.parent.position;
		Vector3 direction = collider.transform.position - transform.position;
		// ... and if a raycast towards the collider hits something...
		//if (Physics.Raycast (transform.parent.position + transform.parent.up, direction.normalized, out hit, 15f)) {
		if (Physics.Raycast (transform.position + transform.up, direction.normalized, out hit, 15f)) {
		
			// ... and if the raycast hits the something..
			if(collider.tag == "FoodSource" && hit.collider.tag == "Food") {
				return true;
			}
			return collider.gameObject == hit.collider.gameObject;		
		}
		return false;
	}

	void OnTriggerStay (Collider collider) {
		if (collider.tag == "FoodSource") {
			if(IsObjectOnSight(collider)) {
				myRobot.SetIsFoodSourceOnSight(true, collider.gameObject);
			} else {
				myRobot.SetIsFoodSourceOnSight(false, null);
			}
		} else if (collider.tag == "Food") {
			if (IsObjectOnSight (collider) && !collider.gameObject.GetComponent<PickUpable> ().BeingCarried()) {
				myRobot.SetIsFoodOnSight (true, collider.gameObject);
			} else {
				myRobot.SetIsFoodOnSight (false, null);
			}
		} else if (collider.tag == "SpecFood") {
			if (IsObjectOnSight (collider) && !collider.gameObject.GetComponent<PickUpable> ().BeingCarried()) {
				myRobot.SetIsSpecFoodOnSight (true, collider.gameObject);
			} else {
				myRobot.SetIsSpecFoodOnSight (false, null);
			}
		} else if (collider.tag == "Monster") {
			if (IsObjectOnSight (collider)) {
				//Debug.Log("Monster on Sight");
				myRobot.SetIsEnemyOnSight (true, collider.gameObject);
			} else {
				myRobot.SetIsEnemyOnSight (false, null);
			}
		} else if (collider.tag == "Wall") {
			if (IsObjectOnSight (collider)) {
				//Debug.Log("Wall on Sight");
				myRobot.SetIsObstacleOnSight (true, collider.gameObject);
			} else {
				myRobot.SetIsObstacleOnSight (false, null);
			}
		} else if (collider.gameObject.tag.StartsWith ("Col")) {
			if (collider.gameObject.tag.Substring (3) == myRobot.tag.Substring (6)) {
				if (IsObjectOnSight (collider)) {
					myRobot.SetIsColonyOnSight (true, collider.gameObject);
				} else {
					myRobot.SetIsColonyOnSight (false, null);;
				}
			} else {
				myRobot.SetIsColonyOnSight (false, null);
			}
		} 
	}
}
