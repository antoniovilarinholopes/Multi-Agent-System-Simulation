using UnityEngine;
using System.Collections;

public class ComunicationModule : MonoBehaviour {

	Colony colony;

	public void Broadcast(SpeechAtc speechAtc, string tag, Vector3 obj) {
		colony.Broadcast(speechAtc, tag, obj);
	}

	public void SetColonyComponent (Colony colony) {
		this.colony = colony;
	}

	void Update () {

	}
}
