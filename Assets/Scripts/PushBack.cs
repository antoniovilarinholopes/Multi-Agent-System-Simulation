using UnityEngine;
using System.Collections;
using System;

public class PushBack : MonoBehaviour {

	void OnTriggerEnter(Collider collider) {
		Debug.Log ("here");
		System.Console.WriteLine ("Here");
		if (collider.tag == "Player") {
			System.Console.WriteLine("Here");
			Move ind = collider.GetComponent<Move>();
			ind.SendBack();
		}
	}

}
