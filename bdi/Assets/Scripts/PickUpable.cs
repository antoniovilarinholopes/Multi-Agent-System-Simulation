using UnityEngine;
using System.Collections;

public class PickUpable : MonoBehaviour {
	bool beingCarried;
	Move carrying;
	// Use this for initialization
	void Start () {
		beingCarried = false;
	}

	void Update () {
		if (beingCarried) {
			Vector3 forward = new Vector3(0,1,1);
			this.transform.position = carrying.transform.position + forward;
		}
	}

	void OnTriggerEnter(Collider collider) {
		Move move = collider.GetComponent<Move>();
		if(move != null && !beingCarried) {
			beingCarried = true;
			carrying = move;
			move.SetFood(gameObject);
		}
	}

	public bool BeingCarried () {
		return beingCarried;
	}

	public void SetBeingCarried(bool carried) {
		this.beingCarried = carried;
	}
}
