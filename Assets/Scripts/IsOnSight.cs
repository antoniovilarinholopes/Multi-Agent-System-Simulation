using UnityEngine;
using System.Collections;

public class IsOnSight : MonoBehaviour {

	Move myRobot;

	// Use this for initialization
	void Start () {
		myRobot = this.transform.parent.GetComponent<Move>();
	}

	bool IsObjectOnSight(Collider collider) {
		RaycastHit hit;
		Vector3 direction = collider.transform.position - transform.parent.position;
		// ... and if a raycast towards the collider hits something...
		if (Physics.Raycast (transform.parent.position, direction.normalized, out hit, 15f)) {
			// ... and if the raycast hits the player...
			if(collider.tag == "Food") {
				Debug.Log ("Tag: " + hit.collider.gameObject.tag);
			}
			return hit.collider.gameObject.tag == collider.tag;
		}
		return false;
	}

	void OnTriggerStay (Collider collider) {
		if (collider.tag == "Food") {
			//FIXME ugly as s**t
			//Debug.Log("Is on Sight: " + IsObjectOnSight(collider));
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
}
