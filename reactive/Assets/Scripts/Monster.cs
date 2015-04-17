using UnityEngine;
using System.Collections;

public class Monster : MonoBehaviour {

	private bool agentAhead;
	private GameObject agent;
	private float hitPoints = 12f;
	private float hitRate = 2f;
	float flashSpeed = 5f;
	Color flashColour = Color.red;
	Color myColor;

	// Use this for initialization
	void Start () {
		myColor = this.gameObject.transform.GetChild(0).GetChild(1).GetComponent<Renderer> ().material.color;		
	}
	
	// Update is called once per frame
	void Update () {
		if (AgentAhead ()) {
			HitAgent();
		}
	}
	
	void OnTriggerEnter(Collider collider) {
		Move move = collider.GetComponent<Move>();
		if(move != null) {
			move.SetEnemyInFront(gameObject);
			SetAgentAhead(collider.gameObject);
		}
	}

	private void HitAgent() {
		if (agent != null) {
			Move move = agent.GetComponent<Move>();
			move.TakeDamage();
		} else {
			agentAhead = false;
		}
	}

	public void TakeDamage(GameObject ind) {
		Material mat = this.gameObject.transform.GetChild(0).GetChild(1).GetComponent<Renderer> ().material;
		Color color = mat.color;
		mat.color = flashColour;
		transform.LookAt (ind.transform.position);
		hitPoints -= Time.deltaTime * hitRate;
		if (hitPoints <= 0) {
			Object.Destroy(this.gameObject);
		}
		mat.color = Color.Lerp (flashColour, myColor, flashSpeed * Time.deltaTime);
	}

	private bool AgentAhead() {
		return agentAhead;
	}

	public void SetAgentAhead(GameObject agent) {
		agentAhead = true;
		this.agent = agent;
	}	
}
