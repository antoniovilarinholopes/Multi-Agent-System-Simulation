using UnityEngine;
using System.Collections;

public class ComunicationModule : MonoBehaviour {

	Colony colony;
	Move move;

	public void Broadcast(SpeechAtc speechAtc, string tag, Vector3 obj) {
		colony.Broadcast(speechAtc, tag, obj);
	}

	public void SetColonyComponent (Colony colony) {
		this.colony = colony;
	}

	void Start () {
		move = this.GetComponent<Move>();
	}

	public float RequestBid (Vector3 position) {
		float bid;
		bid = 1/Mathf.Sqrt(move.DistanceBetweenMeAndPoint (position));
		if(move.myCurrentIntention.Intention () == Intention.SEARCH_FOOD) {
			bid *= 2f;
		}

		if(move.HasHighLife()) {
			bid *= 1.5f;
		}
		return bid;
	}

	public void GetSpecialFood(Vector3 position) {
		move.AddToBeliefs("SpecFood", position);
	}


	public float RequestHelpBid (Vector3 position) {
		float bid = 1.0f;
		bid = 1/Mathf.Sqrt(move.DistanceBetweenMeAndPoint (position));
		if(move.myCurrentIntention.Intention () == Intention.SEARCH_FOOD) {
			bid *= 2.0f;
		}
		
		if(move.HasHighLife()) {
			bid *= 1.5f;
		}

		if (move.HasFood () || move.HasLowLife ()) {
			bid = 0.01f;
		}
		return bid;
	}
	
	public void ReceiveHelpRequest(Vector3 position) {
		move.AddToBeliefs("HelpOther", position);
	}

}
