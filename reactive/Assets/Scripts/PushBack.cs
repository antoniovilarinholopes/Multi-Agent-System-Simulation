using UnityEngine;
using System.Collections;
using System;

public class PushBack : MonoBehaviour {

	void OnTriggerExit(Collider collider) {
		Move move = collider.GetComponent<Move> ();
		if (move != null) {
			move.SetEndOfWorld ();
		}
	}
}
