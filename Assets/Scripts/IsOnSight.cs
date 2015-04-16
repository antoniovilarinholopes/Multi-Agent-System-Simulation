using UnityEngine;
using System.Collections;

public class IsOnSight : MonoBehaviour {

	Move myRobot;

	// Use this for initialization
	void Start () {
		myRobot = this.transform.parent.GetComponent<Move>();
	}
/*
	void OnTriggerEnter (Collider collider) {
		
	}
*/

	bool IsObjectOnSight(Collider collider) {
		RaycastHit hit;
		Vector3 direction = collider.transform.position - transform.position;
		// ... and if a raycast towards the collider hits something...
		if (Physics.Raycast (transform.position, direction.normalized, out hit)) {
			// ... and if the raycast hits the player...
			return hit.collider.gameObject.tag == collider.tag;
		}
		return false;
	}

	void OnTriggerStay (Collider collider) {
		if (collider.tag == "Food") {
			//FIXME ugly as s**t
			if (IsObjectOnSight (collider)) {
				myRobot.SetIsFoodOnSight (true, collider.gameObject);
			} else {
				myRobot.SetIsFoodOnSight (false, null);
			}
		} else if (collider.tag == "Monster") {
			if (IsObjectOnSight (collider)) {
				myRobot.SetIsEnemyOnSight (true, collider.gameObject);
			} else {
				myRobot.SetIsEnemyOnSight (false, null);
			}
		} else if (collider.tag == "Wall") {
			//FIXME doesn't work properly
			if (IsObjectOnSight (collider)) {
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
/*
	void OnTriggerExit (Collider collider) {
	}

	// Update is called once per frame
	void Update () {
	
	}
	*/
}
