using UnityEngine;
using System.Collections;

public class Monster : MonoBehaviour {

	private bool agentAhead;
	private GameObject agent;
	private float hitPoints = 12f;
	private float hitRate = 2f;
	Color flashColour = new Color(1f, 0f, 0f, 0.1f);

	// Use this for initialization
	void Start () {
		
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
		Material mat = GetComponent<Renderer> ().material;
		Color color = mat.color;
		mat.color = flashColour;
		transform.LookAt (ind.transform.position);
		hitPoints -= Time.deltaTime * hitRate;
		if (hitPoints <= 0) {
			Object.Destroy(this.gameObject);
		}
	}

	private bool AgentAhead() {
		return agentAhead;
	}

	public void SetAgentAhead(GameObject agent) {
		agentAhead = true;
		this.agent = agent;
	}	
}
