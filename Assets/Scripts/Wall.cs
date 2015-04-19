using UnityEngine;
using System.Collections;

public class Wall : MonoBehaviour {

	public float hitRate;
	public float hitPoints = 20f;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnTriggerEnter(Collider collider) {
		Move move = collider.GetComponent<Move>();
		if(move != null) {
			move.SetWallAhead(this.gameObject);
		}
	}

	public void HitWall() {
		hitPoints -= Time.deltaTime * hitRate;
		if (hitPoints <= 0) {
			Object.Destroy(this.gameObject);
		}
		//Debug.Log("Hitpoints: " + hitPoints);
	}
}
