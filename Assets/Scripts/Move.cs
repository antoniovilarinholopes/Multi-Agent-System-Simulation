using UnityEngine;
using System.Collections;

public class Move : MonoBehaviour
{
	private bool canMove = false;
	
	void FixedUpdate() {
		//if (canMove) {
		int rand = Random.Range(1,1000);
		if (rand <= 2) {
			transform.Rotate (0f,-90f,0f);
			//transform.rotation = Quaternion.AngleAxis(-90,Vector3.up);
		} else if(rand <= 4) {
			transform.Rotate (0f,90f,0f);
			//transform.rotation = Quaternion.AngleAxis(90,Vector3.up);
		} else {
			transform.Translate (Vector3.forward * Time.deltaTime, Space.Self);
		}//}
	}

	public void SendBack() {
		transform.Rotate (0f, 180f, 0f);
		return;
	}

	void MakeItMove() {
		canMove = true;
	}

	void MakeItStop() {
		canMove = false;
	}

	//sensor objecto a frente
	/*
	void OnTriggerEnter(Collider collider) {
		//change direction
		if (collider.tag == "Wall") {
			transform.Rotate (0f,180f,0f);
		} 
	}*/
	/*
	void OnTriggerEnter (Collider collider) {
		if (collider.gameObject.tag == "Food") {
			return;
		} else if (collider.tag == "Obsticule") {
		}
	}*/

}

