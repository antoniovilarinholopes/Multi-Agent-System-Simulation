using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Wall : MonoBehaviour {

	public float hitRate;
	public float hitPoints = 20f;
	//public Image damageImage;
	//private float flashSpeed = 5f;
	private Color flashColour = new Color(1f, 0f, 0f, 0.1f);

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
		//damageImage.color = flashColour;
		Material mat = GetComponent<MeshRenderer>().material;
		Color color = mat.color;
		mat.color = flashColour;
		hitPoints -= Time.deltaTime * hitRate;
		if (hitPoints <= 0) {
			Object.Destroy(this.gameObject);
		}
		//damageImage.color = Color.Lerp (damageImage.color, Color.clear, flashSpeed * Time.deltaTime);
		//Debug.Log("Hitpoints: " + hitPoints);
		mat.color = color;
	}
}
