using UnityEngine;
using System.Collections;

public class PickUpable : MonoBehaviour {
	bool beingCarried;

	// Use this for initialization
	void Start () {
		beingCarried = false;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnTriggerEnter(Collider collider) {
		Move move = collider.GetComponent<Move>();
		if(move != null && !beingCarried) {
			beingCarried = true;
			move.SetFood(gameObject);
		}
	}
}
