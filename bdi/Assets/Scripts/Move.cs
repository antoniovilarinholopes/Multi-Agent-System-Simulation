using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public enum Desire {GET_FOOD, DEFEND_COL, HELP_OTHER, HELP_SELF, POPULATE}
public enum Intention {SEARCH_FOOD, GET_FOOD_AT, DESTROY_WALL_AT, GOTO_FOODSOURCE_AT, ATTACK_MONSTER_AT, GOTO_COL_AT, POPULATE_AT, HELP_OTHER_AT, EAT_FOOD}
public enum Action {MOVE_TO, ROTATE_TO, EAT, FIGHT_MONSTER, POPULATE, DESTROY_WALL, PICK_FOOD, DROP_FOOD}

public class Move : MonoBehaviour
{	
	bool endOfWorld, hasFood, atBase;
	bool enemyInAhead, foodAhead, obstacleAhead, agentAhead;
	bool isFoodOnSight, isSpecFoodOnSight, isEnemyOnSight, isObstacleOnSight, isColonyOnSight;
	bool isFoodSourceOnSight;
	GameObject foodOnSight, specFoodOnSight, obstacleOnSight, enemyOnSight, colonyOnSight, foodSourceOnSight;
	GameObject food, enemy, wallObj, colony;
	GameObject myColony;
	Colony myColonyComp;
	Color flashColour = Color.red;
	Color myColor;
	float flashSpeed = 5f;
	Vector3 myColonyPosition;
	public float distance, smooth;
	private float health = 20f;
	private float hitRate = 2f;
	private const float SPEED = 10f;
	NavMeshAgent navMeshAgent;
	//IList<Desire> myDesires; 
	Dictionary<Desire, float> myDesires;
	//Dictionary<Intention, IntentionDetails> myIntentions;
	IList<IntentionDetails> myIntentions;
	IntentionDetails myCurrentIntention;
	Dictionary<Vector3, string> myBeliefs;
	Plan currentPlan;


	void Awake () {
		navMeshAgent = this.GetComponent<NavMeshAgent> ();
		endOfWorld = false;
		foodAhead = false;
		enemyInAhead = false;
		hasFood = false;
		atBase = false;
		enemyInAhead = false;
		agentAhead = false;
		isFoodOnSight = false;
		isEnemyOnSight = false;
		isObstacleOnSight = false;
		isSpecFoodOnSight = false;
		currentPlan = null;
		myColor = transform.GetChild (0).GetChild (0).gameObject.GetComponent<Renderer> ().material.color;

		myDesires = new Dictionary<Desire,float> ();
		//myDesires = new List<Desire> ();
		InitializeDesires ();

		//myIntentions = new Dictionary<Intention, IntentionDetails> ();
		//myIntentions = new List<IntentionDetails> ();
		//FIXME no initial intentions???
		//myIntentions [Intention.SEARCH_FOOD] = null;
		Vector3 moveRand = MoveRandomly ();
		myCurrentIntention = new IntentionDetails(Intention.SEARCH_FOOD, 1f, moveRand);
		myBeliefs = new Dictionary<Vector3,string> ();
	}

	/*
	 * Agent main loop
	 * BDI Agent
	 * The commitment is single-minded
	 */
	void Update() {
		// Decreases Agent life over time
		// Value can be a public variable
		DecreaseLife(0.5f);
		// just a test
		/*
		navMeshAgent.SetDestination (new Vector3 (5f, 0f, -7.44f));
		if (foodAhead) {
			Debug.Log("Food Ahead");
			PickFood ();
		}*/
		//p <- nextPercetp = what is seeing
		//single commitment

		if (currentPlan == null) {
			currentPlan = null;
			Brf ();
			Options ();
			Filter ();
			currentPlan = PlanNewPlan ();
			//currentPlan.Execute ();
		} else {
			if(currentPlan.IsEmpty () || Succeeded () || Impossible ()) {
				currentPlan = null;
				return;
			}
			currentPlan.Execute ();
			Brf ();
			if(!Sound ()) {
				currentPlan = PlanNewPlan ();
			}
		}

	}

	/*
	 * BDI
	 */

	void Brf () {
		// The beliefs are constantly updated with the IsOnSight.
		// However it's necessary to update that in the position of the agent
		// there is nothing but him. Necessary in cases such as picking up food.
		Vector3 myPosition = this.transform.position;
		if (myBeliefs.ContainsKey (myPosition)) {
			myBeliefs.Remove(myPosition);
		}
		//FIXME may cause problems with food sources
		myPosition.y += 1.5f;
		if (myBeliefs.ContainsKey (myPosition)) {
			myBeliefs.Remove(myPosition);
		}
	}

	void Options () {
		//using myBeliefs and myIntentions update myDesires
		//FIXME currently myIntentions don't enter in the weight calculation

		if (KnowWhereFoodIs () || KnowWhereFoodSourceIs ()) {
			float get_food_multiplier = 0.9f;
			//lower weight to get food if already carrying
			if (HasFood ()) {
				get_food_multiplier = 0.0f;
			}
			myDesires [Desire.GET_FOOD] = 1f * get_food_multiplier;
		} else {
			myDesires [Desire.GET_FOOD] = 0.7f;
		}

		if (HasLowLife ()) {
			float help_self_multiplier = 0.9f;
			if (HasFood ()) {
				help_self_multiplier = 1f;
			}
			myDesires [Desire.HELP_SELF] = 1f * help_self_multiplier;
		} else if (EnemyAhead ()) {
			myDesires [Desire.HELP_SELF] = 1f;
		} else {
			myDesires [Desire.HELP_SELF] = 0.0f;
		}

		if (ColonyBeingAttacked ()) {
			if (AtBase ()) {
				myDesires [Desire.DEFEND_COL] = 1.0f;
			} else {
				float defend_col_multiplier = 1.0f;
				float number_of_ind_at_base = HowManyAtBase ();
				//The more there are at base the less I want to go there
				if (number_of_ind_at_base > 0) {
					defend_col_multiplier = defend_col_multiplier / number_of_ind_at_base;
				}
				myDesires [Desire.DEFEND_COL] = 1f * defend_col_multiplier;
			}
		} else {
			myDesires [Desire.DEFEND_COL] = 0.0f;
		}

		if (FoodToPopulate ()) {
			float populate_multiplier = 0.3f;
			if(AtBase ()) {
				populate_multiplier = 0.9f;
			} else {
				float number_of_ind_at_base = HowManyAtBase ();
				if (number_of_ind_at_base < 2) {
					populate_multiplier = 0.7f;
				} else {
					populate_multiplier = 0.2f;
				}
			}
			myDesires [Desire.POPULATE] = 1f * populate_multiplier;
		} else {
			myDesires [Desire.POPULATE] = 0.0f;
		}

		//FIXME 
		//TODO HELP_OTHER, communication needed
		myDesires [Desire.HELP_OTHER] = 0.0f;
	}

	void Filter () {
		//using my@(Beliefs,Intentions,Desires) update myIntentions
		//Get my biggest desires
		var biggestDesireValue = 0f;
		foreach (var desire in myDesires.Keys) {
			if(myDesires [desire] > biggestDesireValue) {
				biggestDesireValue = myDesires [desire];
			}
		}

		IList<Desire> myBiggestDesires = new List<Desire>() ;
		foreach (var desire in myDesires.Keys) {
			if(myDesires [desire] == biggestDesireValue) {
				myBiggestDesires.Add(desire);
			}
		}

		IList<IntentionDetails> myIntentions = RetrieveIntentionsFromDesires (myBiggestDesires);
		myCurrentIntention = null;
		float best_weight = 0.0f;
		foreach(var intentionDetail in myIntentions) {
			float weight = intentionDetail.Weight ();
			if(weight > best_weight) {
				best_weight = weight;
				myCurrentIntention = intentionDetail;
			}
		}
	}


	Plan PlanNewPlan () {
		//uses myBeliefs and myIntentions
		return new Plan (null, this.gameObject);
	}

	/*
	 *BDI aux
	 */

	bool ColonyBeingAttacked () {

		//Colony myColComponent = myColony.gameObject.GetComponent<Colony> ();
		//return myColComponent.IsUnderAttack ();
		return myColonyComp.IsUnderAttack ();
	}

	bool FoodToPopulate () {
		/*Colony myColComponent = myColony.GetComponent<Colony> ();
		return myColComponent.HasFoodToPopulate ();*/
		return myColonyComp.HasFoodToPopulate ();
	}

	Vector3 ClosestFood () {
		Vector3 closestFoodPosition = null;
		float minDist = float.MaxValue;
		foreach (Vector3 belief in myBeliefs) {
			if(myBeliefs [belief] == "Food" || myBeliefs [belief] == "SpecFood") {
				float distance_x = belief.x - this.transform.position.x;
				float distance_z = belief.z - this.transform.position.z;
				float distance_to_food = distance_x*distance_x + distance_z*distance_z;
				if(distance_to_food < minDist) {
					minDist = distance_to_food;
					closestFoodPosition = belief;
				}
			}
		}
		return closestFoodPosition;

	}

	Vector3 ClosestFoodSource () {
		Vector3 closestFoodSourcePosition = null;
		float minDist = float.MaxValue;
		foreach (Vector3 belief in myBeliefs) {
			if(myBeliefs [belief] == "FoodSource") {
				float distance_x = belief.x - this.transform.position.x;
				float distance_z = belief.z - this.transform.position.z;
				float distance_to_foodsource = distance_x*distance_x + distance_z*distance_z;
				if(distance_to_foodsource < minDist) {
					minDist = distance_to_foodsource;
					closestFoodSourcePosition = belief;
				}
			}
		}
		return closestFoodSourcePosition;
	}

	Vector3 ClosestSpecialFood () {
		Vector3 closestFoodSourcePosition = null;
		float minDist = float.MaxValue;
		foreach (Vector3 belief in myBeliefs) {
			if(myBeliefs [belief] == "SpecFood") {
				float distance_x = belief.x - this.transform.position.x;
				float distance_z = belief.z - this.transform.position.z;
				float distance_to_foodsource = distance_x*distance_x + distance_z*distance_z;
				if(distance_to_foodsource < minDist) {
					minDist = distance_to_foodsource;
					closestFoodSourcePosition = belief;
				}
			}
		}
		return closestFoodSourcePosition;
	}

	float HowManyAtBase () {
		/*Colony myColComponent = myColony.GetComponent<Colony> ();
		return myColComponent.HowManyAtBase ();*/
		return myColonyComp.HowManyAtBase ();
	}

	bool Impossible () {
		//is it possible that with my beliefs i complete my intention(s)?
		return false;
	}

	bool KnowWhereFoodIs () {
		return myBeliefs.ContainsValue ("Food") || myBeliefs.ContainsValue ("SpecFood");
	}

	bool KnowWhereSpecialFoodIs () {
		return myBeliefs.ContainsValue ("SpecFood");
	}

	bool KnowWhereFoodSourceIs () {
		return myBeliefs.ContainsValue ("FoodSource");
	}

	bool KnowWhereObstacleIs () {
		return myBeliefs.ContainsValue ("Wall");
	}

	IList<IntentionDetails> RetrieveIntentionsFromDesires (IList<Desire> desires) {

		IList<IntentionDetails> myCurrentIntentions = new List<IntentionDetails> () ;
	
		foreach (var desire in desires) {
			if (desire == Desire.HELP_SELF) {
				float eat_food_weight = 1f;
				if (EnemyAhead ()) {
					IntentionDetails intention = new IntentionDetails(Intention.ATTACK_MONSTER_AT,1f,enemy.transform.position);
					myCurrentIntentions.Add (intention);
					eat_food_weight = 0.5f;
				}

				if (HasFood()) {
					IntentionDetails intention = new IntentionDetails(Intention.EAT_FOOD,1f*eat_food_weight,this.transform.position);
					myCurrentIntentions.Add (intention);
				}
				float colony_multiplier = 0.9f;
				Vector3 closestFood = ClosestFood ();
				if (closestFood != null) {
					float dist_food_x = closestFood.x - this.transform.position.x;
					float dist_food_z = closestFood.z - this.transform.position.z;
					float distance_to_food = dist_food_x*dist_food_x + dist_food_z*dist_food_z;
					float dist_col_x = myColonyPosition.x - this.transform.position.x;
					float dist_col_z = myColonyPosition.z - this.transform.position.z;
					float distance_to_col = dist_col_x*dist_col_x + dist_col_z*dist_col_z;

					float weight = 0.5f;
					if (distance_to_food < distance_to_col) {
						weight = 0.9;
						colony_multiplier = 0.5f;
					} 
					IntentionDetails intention = new IntentionDetails(Intention.GOTO_COL_AT,0.9f,myColonyPosition);
					myCurrentIntentions.Add (intention);
				}
				IntentionDetails intention_details = new IntentionDetails(Intention.GOTO_COL_AT,1f*closestFood,myColonyPosition);
				myCurrentIntentions.Add (intention_details);
			} else if (desire == Desire.POPULATE) {
				float weight = 0.6f;
				float dist_col_x = myColonyPosition.x - this.transform.position.x;
				float dist_col_z = myColonyPosition.z - this.transform.position.z;
				float distance_to_col = dist_col_x*dist_col_x + dist_col_z*dist_col_z;
				float delta = 2f;
				if (distance_to_col < delta || AtBase ()) {
					weight = 0.9f;
				}
				Vector3 myPositon = this.transform.position;
				IntentionDetails intention = new IntentionDetails(Intention.POPULATE_AT, weight, myColonyPosition);
				myCurrentIntentions.Add (intention);
			} else if (desire == Desire.DEFEND_COL) {
				//FIXME improve!!!
				float defend_col_multiplier = 1.0f;
				float number_of_ind_at_base = HowManyAtBase ();
				//The more there are at base the less I want to go there
				if (number_of_ind_at_base > 0) {
					defend_col_multiplier = defend_col_multiplier / number_of_ind_at_base;
				}
				IntentionDetails intention = new IntentionDetails(Intention.ATTACK_MONSTER_AT, 1f*defend_col_multiplier, myColonyPosition));
				myCurrentIntentions.Add (intention);
			} else if (desire == Desire.GET_FOOD) {
				bool knowWhereFoodIs = KnowWhereFoodIs ();
				bool knowWhereFoodSourceIs = KnowWhereFoodSourceIs ();
				bool knowWhereSpecialFoodIs = KnowWhereSpecialFoodIs ();
				bool knowWhereObsIs = KnowWhereObstacleIs ();

				if (knowWhereSpecialFoodIs) {
					float special_food_multiplier = 0.9f;
					Vector3 closestSpecialFood = ClosestSpecialFood ();
					bool canMakeToSpecFood = CanMakeItThere(closestSpecialFood);
					if(canMakeToSpecFood) {
						IntentionDetails intention = new IntentionDetails(Intention.GOTO_FOODSOURCE_AT, 1f*special_food_multiplier, closestSpecialFood);
						myCurrentIntentions.Add (intention);
						continue;
					}
				} 
				if (knowWhereFoodSourceIs) {
					float foodsource_mul = 0.9f;
					Vector3 closestFoodSource = ClosestFoodSource ();
					bool canMakeToFoodSource = CanMakeItThere(ClosestFoodSource);
					if(canMakeToFoodSource) {
						IntentionDetails intention = new IntentionDetails(Intention.GOTO_FOODSOURCE_AT, 1f*foodsource_mul, closestFoodSource);
						myCurrentIntentions.Add (intention);
						continue;
					} 

				} 
				if (knowWhereFoodIs) {
					float food_mul = 0.9f;
					Vector3 closestFood = ClosestFood ();
					bool canMakeToFood = CanMakeItThere(ClosestFood);
					if(canMakeToFood) {
						IntentionDetails intention = new IntentionDetails(Intention.GOTO_FOODSOURCE_AT, 1f*food_mul, closestFoodSource);
						myCurrentIntentions.Add (intention);
						continue;
					} 

				} 
				if (KnowWhereObstacleIs ()){

				} 
				IntentionDetails intention = new IntentionDetails(Intention.SEARCH_FOOD, 0.9f, MoveRandomly());
				myCurrentIntentions.Add (intention);

				//myCurrentIntentions.Add (Intention.GET_FOOD_AT);
			} else {
				//desire == Desire.HELP_OTHER

			}
		}
		return myCurrentIntentions;

	}

	bool Sound () {
		//has the plan gone wrong?
		return false;
	}

	bool Succeeded () {
		//have i done what i wanted to?
		return true;
	}


	public class IntentionDetails {
		Intention intention;
		float weight;
		Vector3 position;

		public IntentionDetails(Intention intention, float weight, Vector3 position) {
			this.weight = weight;
			this.position = position;
			this.intention = intention;
		}

		public IntentionDetails(float weight, Vector3 position) {
			this.weight = weight;
			this.position = position;
		}

		public float Weight () {
			return this.weight;
		}

		public Vector3 Position () {
			return this.position;
		}

		public Intention Intention () {
			return this.intention;
		}

	}

	//FIXME Template for now, must be reviewd!!!
	class Plan {
		IList<PlanAction> steps;
		GameObject ind;
		
		public Plan (IList<PlanAction> steps, GameObject ind) {
			this.steps = steps;
			this.ind = ind;
		}
		
		public bool IsEmpty () {
			if (steps == null) {
				return true;
			}
			return steps.Count == 0;
		}
		
		public PlanAction Head () {
			if (steps == null) {
				return null;
			}
			return steps[0];
		}
		
		public void Execute () {
			if (steps == null) {
				return;
			}
			var step = Head ();
			//FIXME make ind execute step
			steps.RemoveAt (0);
		}
		
	}




	/*
	 * Sensors
	 */
	
	public bool AtBase () {
		return atBase;
	}

	bool AgentAhead () {
		return agentAhead;
	}

	bool ColonyOnSight ()	{
		return isColonyOnSight;
	}

	bool EndOfWorld () {
		return endOfWorld;
	}

	bool EnemyAhead() {
		return enemyInAhead;
	}

	
	bool EnemyOnSight () {
		return isEnemyOnSight;
	}


	bool FoodAhead (){
		return foodAhead;
	}
	
	bool FoodOnSight () {
		return isFoodOnSight;
	}

	public bool HasLowLife () {
		return health <= 5;
	}
	
	bool HasFood ()	{
		return hasFood;
	}

	
	bool ObstacleOnSight ()	{
		return isObstacleOnSight;
	}

	bool SpecFoodOnSight () {
		return isSpecFoodOnSight;
	}

	bool ObstacleAhead() {
		return obstacleAhead;
	}

	/*
	 * Actuators
	 */

	void DropFood() {
		hasFood = false;
		string food_tag = food.tag;
		Destroy(food);
		food = null;
		Colony colonyComp = colony.GetComponent<Colony>();
		if(colonyComp != null) {
			colonyComp.IncreaseFood(food_tag);
		}
	}
	
	void EvadeMonster () {
		EvadeAgent();
	}

	void EvadeAgent () {
		int rand = Random.Range(1, 4);
		if (rand <= 2) {
			transform.Rotate (0f,-90f,0f);
		} else {
			transform.Rotate (0f,90f,0f);
		}
		agentAhead = false;
	}
	
	void GotoBase () {
		Pursue (this.colonyOnSight);
	}

	private void HitEnemy() {
		if (enemy != null) {
			Monster monster = enemy.GetComponent<Monster> ();
			monster.TakeDamage (gameObject);
		} else {
			enemyInAhead = false;

		}
	}

	private void HitWall() {
		if(wallObj != null) {
			Wall wall = wallObj.GetComponent<Wall>();
			wall.HitWall();
		} else {
			obstacleAhead = false;
		}
	}

	Vector3 MoveRandomly () {
		int rand = Random.Range(1,1000);
		if (rand <= 2) {
			//transform.Rotate (0f,-90f,0f);
			return this.transform.position + Vector3.left*2;
		} else if(rand <= 4) {
			return this.transform.position + Vector3.right*2;
				//transform.Rotate (0f,90f,0f);
		} else {
			return this.transform.position + Vector3.forward*2;
		}
	}

	void PickFood () {
		this.foodAhead = false;
		this.hasFood = true;
		Vector3 foodPosition = food.transform.position;
		// We do not need to know where the food we have is
		if(myBeliefs.ContainsKey(foodPosition)) {
			myBeliefs.Remove(foodPosition);
		}
	}
	
	void PursueFood (GameObject food) {
		Pursue(food);
	}

	void PursueMonster () {
		Pursue (this.enemyOnSight);
	}
	
	void PursueObstacle () {
		Pursue(this.obstacleOnSight);
	}

	void TryToDestroyEnemy () {
		if (health > 10f) {
			PursueMonster ();
		} else {
			EvadeMonster ();
		}
	}
	

	
	/////////////////////////////////////////////////////////////////////////

	/*
	 * Auxiliar
	 */

	void CleanSight () {
		SetIsFoodOnSight (false, null);
		SetIsEnemyOnSight (false, null);
		SetIsObstacleOnSight (false, null);
		SetIsColonyOnSight (false, null);
		SetIsSpecFoodOnSight (false, null);
	}

	void InitializeDesires () {
		/*foreach (Desire desire in System.Enum.GetValues(typeof(Desire))) {
			myDesires.Add(desire);
		}*/

		foreach (Desire desire in System.Enum.GetValues(typeof(Desire))) {
			myDesires[desire] = 1f;
		}
		/*
		myDesires.Add (Desire.GET_FOOD);
		myDesires.Add (Desire.DEFEND_COL);
		myDesires.Add (Desire.HELP_OTHER);
		myDesires.Add (Desire.HELP_SELF);
		myDesires.Add (Desire.POPULATE);*/
	}

	void MoveForward() {
		transform.Translate (Vector3.forward * Time.deltaTime * SPEED, Space.Self);
		// Move food along with him
		if(HasFood()) {
			Vector3 currentPosition = food.transform.position;
			Vector3 newPosition = transform.position +
				transform.TransformDirection(Vector3.forward) * distance;
			
			this.food.transform.position = 
				Vector3.Lerp (currentPosition, newPosition, Time.deltaTime*smooth);
		}
	}

	void OnTriggerEnter(Collider collider) {
		if(collider.gameObject.tag == "PlayerA" || collider.gameObject.tag == "PlayerB") {
			//Debug.Log ("Collision");
			agentAhead = true;
		}
	}

	void Pursue(GameObject target) {
		//pursuing = true;
		Vector3 targetDir = target.transform.position - transform.position;
		targetDir.y = 0;
		float step  = smooth * Time.deltaTime;
		Vector3 newDir = Vector3.RotateTowards(transform.forward, targetDir, step, 0.0f);
		transform.rotation = Quaternion.LookRotation(newDir);
		MoveForward();
	}

	void RunFromEnemy() {
		enemy = null;
		enemyInAhead = false;
		SendBack ();
	}

	////////////////////////////////////////////////////////////////////////


	/*
	 * Public (Being called outside Agent)
	 */

	
	public void DecreaseLife (float lifeDecreased) {
		health -= Time.deltaTime * lifeDecreased;
		if (health <= 0) {
			Debug.Log ("Agent Died");
			if(HasFood()) {
				PickUpable foodBeingCarried = this.food.GetComponent<PickUpable>();
				foodBeingCarried.SetBeingCarried(false);
				Vector3 myPosition = this.transform.position;
				myPosition.y = 1.5f;
				foodBeingCarried.transform.position = myPosition;
			}
			Object.Destroy(this.gameObject);
		}
	}


	
	public void EatFood (float healHealth) {
		this.health += healHealth;
	}

	// FIXME: Do we need the public here?
	public void SendBack () {
		transform.Rotate (0f, 180f, 0f);
		endOfWorld = false;
		return;
	}

	// Setters


	public void SetColonyPosition (Vector3 position) {
		myColonyPosition = position;
		myBeliefs [position] = "MyCol";
	}

	public void SetMyColony (GameObject myColony) {
		this.myColony = myColony;
		myColonyComp = myColony.gameObject.GetComponent<Colony> ();
		Debug.Log (myColonyComp.IsUnderAttack ());
	}

	public void SetIsFoodSourceOnSight (bool isFoodSourceOnSight, GameObject foodSourceOnSight){
		this.isFoodSourceOnSight = isFoodSourceOnSight;
		this.foodSourceOnSight = foodSourceOnSight;
		if(isFoodSourceOnSight) {
			Vector3 foodSourcePosition = new Vector3(foodSourceOnSight.transform.position.x, foodSourceOnSight.transform.position.y, foodSourceOnSight.transform.position.z);
			if(!myBeliefs.ContainsKey(foodSourcePosition)) {
				myBeliefs [foodSourcePosition] = "FoodSource";
			}
		}
	}

	public void SetIsFoodOnSight (bool isFoodOnSight, GameObject foodOnSight) {
		this.isFoodOnSight = isFoodOnSight;
		this.foodOnSight = foodOnSight;
		if(isFoodOnSight) {
			Vector3 foodPosition = new Vector3(foodOnSight.transform.position.x, foodOnSight.transform.position.y, foodOnSight.transform.position.z);
			if(!myBeliefs.ContainsKey(foodPosition)) {
				myBeliefs[foodPosition] = "Food";
			}
		}
	}

	public void SetIsSpecFoodOnSight (bool isSpecFoodOnSight, GameObject specFoodOnSight) {
		this.isSpecFoodOnSight = isSpecFoodOnSight;
		this.specFoodOnSight = specFoodOnSight;
		if(isSpecFoodOnSight) {
			Vector3 foodPosition = new Vector3(specFoodOnSight.transform.position.x, specFoodOnSight.transform.position.y, specFoodOnSight.transform.position.z);
			if(!myBeliefs.ContainsKey(foodPosition)) {
				myBeliefs[foodPosition] = "SpecFood";
			}
		}
	}

	public void SetIsEnemyOnSight (bool isEnemyOnSight, GameObject enemyOnSight) {
		this.isEnemyOnSight = isEnemyOnSight;
		this.enemyOnSight = enemyOnSight;
		// TODO: Do we need this?
		Vector3 enemyPosition = enemyOnSight.transform.position;
		/* FIXME verify that if has the key, it was not a monster
		if(!myBeliefs.ContainsKey(enemyPosition)) {
			myBeliefs[enemyPosition] = "Monster";
		}*/
	}

	public void SetIsObstacleOnSight (bool isObstacleOnSight, GameObject obstacleOnSight) {
		this.isObstacleOnSight = isObstacleOnSight;
		this.obstacleOnSight = obstacleOnSight;
		Vector3 obsPosition = obstacleOnSight.transform.position;
		// FIXME
		if(!myBeliefs.ContainsKey(obsPosition)) {
			myBeliefs [obsPosition] = "Wall";
		}
	}

	public void SetIsColonyOnSight (bool isColonyOnSight, GameObject colonyOnSight) {
		this.isColonyOnSight = isColonyOnSight;
		this.colonyOnSight = colonyOnSight;
	}

	public void SetEndOfWorld() {
		endOfWorld = true;
	}
	
	public void SetEnemyInFront(GameObject enemy) {
		enemyInAhead = true;
		this.enemy = enemy;
	}
	
	// Box collider (mundo) chama esta funcao quando o agente sai
	public void SetWallAhead(GameObject wall) {
		obstacleAhead = true;
		this.wallObj = wall;
	}

	public void SetFood(GameObject food) {
		this.food = food;
		this.foodAhead = true;
	}

	public void SetAtBase (bool atBase, GameObject colony)	{
		this.atBase = atBase;
		this.colony = colony;
	}

	// Called by monsters
	public void TakeDamage() {
		Material mat = this.transform.GetChild(0).GetChild(0).GetComponent<Renderer> ().material;
		//Color color = mat.color;
		mat.color = flashColour;
		DecreaseLife (hitRate);

		Debug.Log ("Agent Hitpoints: " + health);
		mat.color = Color.Lerp (flashColour, myColor, flashSpeed * Time.deltaTime);
	}
	

}

