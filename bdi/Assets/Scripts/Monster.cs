﻿using UnityEngine;
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

	// Use this for initialization
	void Start () {
		myColor = this.gameObject.transform.GetChild(0).GetChild(1).GetComponent<Renderer> ().material.color;		
	}
	
	// Update is called once per frame
	void Update () {
		if (AgentAhead ()) {
			HitAgent();
		} else {
			MoveRandomly();
		}
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
			move.SetEnemyInFront(null);
			SetAgentAhead(null, false);
		}
	}

	void OnTriggerEnter(Collider collider) {
		Move move = collider.GetComponent<Move>();
		if(move != null) {
			move.SetEnemyInFront(gameObject);
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
		//Color color = mat.color;
		mat.color = flashColour;
		transform.LookAt (ind.transform.position);
		hitPoints -= Time.deltaTime * hitRate;
		if (hitPoints <= 0) {
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
		Instantiate(specFood, transform.position, Quaternion.identity);
	}
}
