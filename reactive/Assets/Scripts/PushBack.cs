using UnityEngine;
using System.Collections;
using System;

public class PushBack : MonoBehaviour {

	void OnTriggerExit(Collider collider) {
		Move move = collider.GetComponent<Move> ();
		Monster monster = collider.GetComponent<Monster>();
		if (move != null) {
			move.SetEndOfWorld ();
		} 
		if (monster != null) {
			Debug.Log ("Monster");
			monster.SendBack();
		}
	}
}
