using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Wall : MonoBehaviour {

	public float hitRate;
	public float hitPoints = 20f;
	Color myColor;
	private float flashSpeed = 5f;
	private Color flashColour = new Color(1f, 0f, 0f, 0.1f);

	// Use this for initialization
	void Start () {
		myColor = GetComponent<Renderer> ().material.color;
	}
	
	// Update is called once per frame
	void Update () {
	}

	void OnTriggerEnter(Collider collider) {
		Move move = collider.GetComponent<Move>();
		Monster monster = collider.GetComponent<Monster>();
		if(move != null) {
			move.SetWallAhead(this.gameObject);
		}
		if(monster !=null) {
			monster.SendBack();
		}
	}

	public void HitWall() {
		Material mat = GetComponent<Renderer> ().material;
		mat.color = flashColour;
		hitPoints -= Time.deltaTime * hitRate;
		if (hitPoints <= 0) {
			Object.Destroy(this.gameObject);
		}
		mat.color = Color.Lerp (flashColour, myColor, flashSpeed * Time.deltaTime);
	}
}
