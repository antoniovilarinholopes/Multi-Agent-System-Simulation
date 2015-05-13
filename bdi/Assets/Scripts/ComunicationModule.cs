using UnityEngine;
using System.Collections;

public class ComunicationModule : MonoBehaviour {

	Colony colony;

	public void Broadcast(string messageType, string tag, Vector3 obj) {
		colony.Broadcast(messageType, tag, obj);
	}
}
