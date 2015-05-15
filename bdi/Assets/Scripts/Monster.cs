using UnityEngine;
using System.Collections;

public class Monster : MonoBehaviour {

	public const float SPEED = 2.0f;

	private bool agentAhead;
	private GameObject agent;
	private float hitPoints = 12f;
	private float hitRate = 2f;
	public GameObject specFood;
	float flashSpeed = 5f;
	Color flashColour = Color.red;
	Color myColor;
	Vector3 colonyPos;
	bool inColony = false;

	// Use this for initialization
	void Start () {
		myColor = this.gameObject.transform.GetChild(0).GetChild(1).GetComponent<Renderer> ().material.color;		
	}
	
	// Update is called once per frame
	void Update () {
		if (AgentAhead ()) {
			HitAgent();
		} else if(inColony) {
			MoveToColony();
		} else {
			MoveRandomly();
		}
	}

	void MoveToColony() {
		float step = Time.deltaTime * SPEED;
		transform.position = Vector3.MoveTowards(transform.position, colonyPos, step);
	}

	void MoveRandomly () {
		int rand = Random.Range(1,1000);
		if (rand <= 2) {
			transform.Rotate (0f,-90f,0f);
		} else if(rand <= 4) {
			transform.Rotate (0f,90f,0f);
		} else {
			transform.Translate (Vector3.forward * Time.deltaTime * SPEED, Space.Self);
		}
	}
	
	void OnTriggerExit(Collider collider) {
		Move move = collider.GetComponent<Move>();
		if(move != null) {
			move.SetEnemyInFront(false, null);
			SetAgentAhead(null, false);
		}
	}

	void OnTriggerEnter(Collider collider) {
		Move move = collider.GetComponent<Move>();
		if(move != null) {
			move.SetEnemyInFront(true, gameObject);
			SetAgentAhead(collider.gameObject, true);
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
		mat.color = flashColour;
		transform.LookAt (ind.transform.position);
		hitPoints -= Time.deltaTime * hitRate;
		if (hitPoints <= 0) {
			Debug.Log ("Destroyed");
			Move move = agent.GetComponent<Move>();
			move.SetEnemyInFront(false, null);
			DropFood();
			Object.Destroy(this.gameObject);
		}
		mat.color = Color.Lerp (flashColour, myColor, flashSpeed * Time.deltaTime);
	}

	private bool AgentAhead() {
		return agentAhead;
	}

	public void SetAgentAhead(GameObject agent, bool agentAhead) {
		this.agentAhead = agentAhead;
		this.agent = agent;
	}

	void DropFood() {
		Vector3 position = transform.position;
		position.y = 1.5f;
		Instantiate(specFood, position, Quaternion.identity);
	}

	public void SendBack() {
		transform.Rotate (0f, 180f, 0f);
		return;
	}

	public void SetInColony (bool inColony, GameObject colony) {
		this.inColony = inColony;
		this.colonyPos = colony.transform.position;
	}
}
