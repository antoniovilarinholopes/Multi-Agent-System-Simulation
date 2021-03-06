﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public enum Desire {GET_FOOD, DEFEND_COL, HELP_OTHER, HELP_SELF, POPULATE}
public enum Intention {SEARCH_FOOD, GET_FOOD_AT, DESTROY_WALL_AT, GOTO_FOODSOURCE_AT, ATTACK_MONSTER_AT, GOTO_COL_AT, POPULATE_AT, HELP_OTHER_AT, EAT_FOOD}
public enum Action {MOVE_TO, EAT, FIGHT_MONSTER, POPULATE, DESTROY_WALL, PICK_FOOD, DROP_FOOD}
public enum SpeechAtc {INFORM_ADD, INFORM_REMOVE, REQUEST_ADD}


public class Move : MonoBehaviour
{	
	bool endOfWorld, hasFood, atBase;
	bool enemyAhead, foodAhead, obstacleAhead, agentAhead;
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
	private float health = 100f;
	private float hitRate = 2f;
	private const float SPEED = 10f;
	NavMeshAgent navMeshAgent;
	Dictionary<Desire, float> myDesires;
	public IntentionDetails myCurrentIntention;
	Dictionary<Vector3, string> myBeliefs;
	bool currentActionHasEnded;
	PlanAction currentAction;
	Queue<PlanAction> currentPlan;

	ComunicationModule commModule;

	Vector3 help_position;
	//bool help_request;

	void Awake () {
		navMeshAgent = this.GetComponent<NavMeshAgent> ();
		endOfWorld = false;
		foodAhead = false;
		hasFood = false;
		atBase = false;
		enemyAhead = false;
		agentAhead = false;
		isFoodOnSight = false;
		isEnemyOnSight = false;
		isObstacleOnSight = false;
		isSpecFoodOnSight = false;
		currentPlan = null;
		currentActionHasEnded = false;
		//help_request = false;
		currentAction = null;
		//myColor = transform.GetChild (0).GetChild (0).gameObject.GetComponent<Renderer> ().material.color;
		myDesires = new Dictionary<Desire,float> ();
		InitializeDesires ();

		commModule = GetComponent<ComunicationModule>();

		Vector3 moveRand = MoveRandomly ();
		myCurrentIntention = new IntentionDetails(Intention.SEARCH_FOOD, 1f, moveRand);
		myBeliefs = new Dictionary<Vector3,string> (new Vector3Comparer());
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
		//single commitment

		foreach (var belief in myBeliefs.Keys) {
			Debug.Log (belief + ":" + myBeliefs [belief]);
		}
		Debug.Log ("---------------");
		if (currentPlan == null) {
			Brf ();
			Options ();
			Filter ();
			Planner planner = CreateNewPlan ();
			currentPlan = planner.Plan ();
		} else {
			ChooseAction ();

			/*Debug.Log (myCurrentIntention.Intention ());
			if (currentAction != null) {
				Debug.Log (currentAction.Action ());
				Debug.Log (myCurrentIntention.Intention ());
			}*/
			bool impossible = Impossible ();
			if(PlanIsEmpty () || Succeeded () || impossible) {
				if(currentAction != null && impossible) {
					bool removeBelief = (myBeliefs.ContainsKey(myCurrentIntention.Position ()) && myBeliefs [myCurrentIntention.Position ()] != "MyCol");
					if (removeBelief) {
						myBeliefs.Remove(myCurrentIntention.Position ());
					}
				} 
				currentPlan = null;
				currentAction = null;
				currentActionHasEnded = false;
				return;
			}
			ExecuteAction ();
			Brf ();
			if (Reconsider ()) {
				Options ();
				Filter ();
			}
		}

	}

	/*
	 * BDI
	 */
	
	Planner CreateNewPlan () {
		//uses myBeliefs and myIntentions
		return new Planner (myColonyPosition, myCurrentIntention);
	}

	void Brf () {
		// The beliefs are constantly updated with the IsOnSight.
		// However it's necessary to update that in the position of the agent
		// there is nothing but him. Necessary in cases such as picking up food.
		Vector3 myPosition = this.transform.position;
		if (myBeliefs.ContainsKey (myPosition) && myBeliefs [myPosition] != "MyCol") {
			myBeliefs.Remove(myPosition);
			commModule.Broadcast(SpeechAtc.INFORM_REMOVE, myBeliefs [myPosition], myPosition);
		}
	}

	void Options () {
		//using myBeliefs and myIntentions update myDesires

		if (KnowWhereFoodIs () || KnowWhereFoodSourceIs ()) {
			float get_food_multiplier = 0.9f;
			if (HasFood ()) {
				get_food_multiplier = 0.0f;
			}
			myDesires [Desire.GET_FOOD] = 1f * get_food_multiplier;
		} else {
			myDesires [Desire.GET_FOOD] = 0.7f;
		}

		if (EnemyAhead () && !AtBase ()) {
			myDesires [Desire.HELP_SELF] = 1f;
		} else if (HasLowLife ()) {
			float help_self_multiplier = 0.9f;
			if (HasFood ()) {
				help_self_multiplier = 1f;
			}
			myDesires [Desire.HELP_SELF] = 1f * help_self_multiplier;
		}  else {
			myDesires [Desire.HELP_SELF] = 0.0f;
		}

		if (ColonyBeingAttacked ()) {
			if (AtBase ()) {
				myDesires [Desire.DEFEND_COL] = 1.0f;
			} else {
				float defend_col_multiplier = 1.0f;
				float number_of_ind_at_base = HowManyAtBase ();
				//The more there are at base the less I want to go there
				if (number_of_ind_at_base > 0 && !HasFood ()) {
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

		if (HelpRequestReceived ()) {
			myDesires [Desire.HELP_OTHER] = 1.0f;
			RemoveRequestHelp ();
		} else {
			myDesires [Desire.HELP_OTHER] = 0.0f;
		}
	}

	void Filter () {
		//using my@(Beliefs,Intentions,Desires) update myIntentions
		//Get my biggest desires
		var biggestDesireValue = 0.0f;
		foreach (var desire in myDesires.Keys) {
			if(myDesires [desire] > biggestDesireValue) {
				biggestDesireValue = myDesires [desire];
			}
		}

		//All the biggest desire
		IList<Desire> myBiggestDesires = new List<Desire>() ;
		foreach (var desire in myDesires.Keys) {
			if(myDesires [desire] == biggestDesireValue) {
				myBiggestDesires.Add(desire);
			}
		}

		//Best intention
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
	

	/*
	 *BDI aux
	 */

	bool ColonyBeingAttacked () {
		return myColonyComp.IsUnderAttack ();
	}

	bool CanMakeItThere(Vector3 there) {
		float distance_to_object = Mathf.Sqrt(DistanceBetweenMeAndPoint (there));
		return (HasHighLife () && distance_to_object <= 120) || (distance_to_object <= 90 && !HasLowLife ()) || (distance_to_object <= 20 && HasLowLife ());
	}

	void ChooseAction () {	
		if (currentAction == null && currentPlan.Count > 0) {
			currentAction = currentPlan.Dequeue ();
		} else {
			if (CurrentActionHasEnded () && currentPlan.Count == 0) {
				//currentAction = null;
				return;
			} else if(CurrentActionHasEnded () && currentPlan.Count > 0) {
				currentAction = currentPlan.Dequeue ();
				currentActionHasEnded = false;
			}
		}
	}

	Vector3 ClosestFood () {
		Vector3 closestFoodPosition = new Vector3(0f,0f,0f);
		float minDist = float.MaxValue;
		foreach (Vector3 belief in myBeliefs.Keys) {
			if(myBeliefs [belief] == "Food" || myBeliefs [belief] == "SpecFood") {
				float distance_to_food = DistanceBetweenMeAndPoint(belief);
				if(distance_to_food < minDist) {
					minDist = distance_to_food;
					closestFoodPosition = belief;
				}
			}
		}
		return closestFoodPosition;

	}

	Vector3 ClosestFoodSource () {
		Vector3 closestFoodSourcePosition = new Vector3(0f,0f,0f);
		float minDist = float.MaxValue;
		foreach (Vector3 belief in myBeliefs.Keys) {
			if(myBeliefs [belief] == "FoodSource") {
				float distance_to_foodsource = DistanceBetweenMeAndPoint(belief);
				if(distance_to_foodsource < minDist) {
					minDist = distance_to_foodsource;
					closestFoodSourcePosition = belief;
				}
			}
		}
		return closestFoodSourcePosition;
	}

	Vector3 ClosestObstacle () {
		Vector3 closestFoodSourcePosition = new Vector3(0f,0f,0f);
		float minDist = float.MaxValue;
		foreach (Vector3 belief in myBeliefs.Keys) {
			if(myBeliefs [belief] == "SpecFood") {
				float distance_to_foodsource = DistanceBetweenMeAndPoint(belief);
				if(distance_to_foodsource < minDist) {
					minDist = distance_to_foodsource;
					closestFoodSourcePosition = belief;
				}
			}
		}
		return closestFoodSourcePosition;
	}

	Vector3 ClosestSpecialFood () {
		//Assume that belief exists
		Vector3 closestFoodSourcePosition = new Vector3(0f,0f,0f);
		float minDist = float.MaxValue;
		foreach (Vector3 belief in myBeliefs.Keys) {
			if(myBeliefs [belief] == "SpecFood") {
				float distance_to_foodsource = DistanceBetweenMeAndPoint(belief);
				if(distance_to_foodsource < minDist) {
					minDist = distance_to_foodsource;
					closestFoodSourcePosition = belief;
				}
			}
		}
		return closestFoodSourcePosition;
	}

	bool CurrentActionHasEnded () {
		return currentActionHasEnded;
	}

	public float DistanceBetweenMeAndPoint (Vector3 target) {
		float distance_x = target.x - this.transform.position.x;
		float distance_z = target.z - this.transform.position.z;
		return distance_x*distance_x + distance_z*distance_z;
	}


	void ExecuteAction () {

		Action action = currentAction.Action ();
		if (action == Action.MOVE_TO) {
			Vector3 targetPosition = currentAction.Position ();;
			bool equal_x = this.transform.position.x == targetPosition.x;
			bool equal_z = this.transform.position.z == targetPosition.z;
			float distance_to_target = Mathf.Sqrt(DistanceBetweenMeAndPoint(targetPosition));
			bool isSomethingAhead = IsSomethingAhead();
			bool populateEnded = myCurrentIntention.Intention () == Intention.POPULATE_AT && AtBase ();
			bool stopBeforeFoodSource = myCurrentIntention.Intention () == Intention.GOTO_FOODSOURCE_AT && distance_to_target <= 4.0f;
			bool stopAtBase = AtBase () && HasFood () && myCurrentIntention.Intention () == Intention.GET_FOOD_AT;
			if ((stopAtBase) || (stopBeforeFoodSource) || (populateEnded) || (equal_x && equal_z) || (isSomethingAhead && distance_to_target <= 1.5f)) {
				currentActionHasEnded = true;
				return;
			} 
			navMeshAgent.SetDestination (currentAction.Position ());
		} else if (action == Action.EAT) {
			if(hasFood) {
				hasFood = false;
				string food_tag = food.tag;
				Destroy (food);
				food = null;
				float eat_food_val = 10.0f;
				if (food_tag == "SpecFood") {
					eat_food_val = 15.0f;
				}
				EatFood (eat_food_val); 
			}
			currentActionHasEnded = true;
		} else if (action == Action.PICK_FOOD) {
			if (FoodAhead ()) {
				this.PickFood ();
			}
			currentActionHasEnded = true; 
		} else if (action == Action.DROP_FOOD) {
			if (HasFood () && AtBase ()) {
				this.DropFood ();
			}
			currentActionHasEnded = true;
		} else if (action == Action.POPULATE) {
			currentActionHasEnded = true;
		} else if (action == Action.DESTROY_WALL) {
			if (ObstacleAhead ()) {
				this.HitWall (); 
			} else {
				currentActionHasEnded = true;
			}
		} else if (action == Action.FIGHT_MONSTER) {
			if (AtBase () && HasFood ()) {
				DropFood ();
			}
			this.HitEnemy ();
			if(!EnemyAhead () || !ColonyBeingAttacked ()) {
				Material mat = this.transform.GetChild(0).GetChild(0).GetComponent<Renderer> ().material;
				mat.color = myColor;
				enemy = null;
				enemyAhead = false;
				myColonyComp.SetIsUnderAttack(false);
				currentActionHasEnded = true; 
			}
		}
	}

	bool FoodToPopulate () {
		return myColonyComp.HasFoodToPopulate ();
	}

	bool HelpRequestReceived () {
		return myBeliefs.ContainsValue ("HelpOther");
	}

	float HowManyAtBase () {
		return myColonyComp.HowManyAtBase ();
	}
	
	bool Impossible () {
		//is it possible that with my beliefs i complete my intention(s)?
		// if the target position is no longer in our beliefs the plan becomes impossible

		//If intention or action is a always possible one, return true
		//bool intentionsPossible = (myCurrentIntention.Intention () == Intention.EAT_FOOD) || (myCurrentIntention.Intention () == Intention.SEARCH_FOOD);
		bool intentionsPossible = (myCurrentIntention.Intention () == Intention.SEARCH_FOOD);
		if (intentionsPossible) { 
			return false; 
		}

		bool foodHasBeenPicked = myCurrentIntention.Intention () == Intention.GET_FOOD_AT && !myBeliefs.ContainsKey (myCurrentIntention.Position ());
		if (foodHasBeenPicked && !HasFood ()) {
			return true;
		}

		bool stopAttackInvisibleMonster = myCurrentIntention.Intention () == Intention.ATTACK_MONSTER_AT && AtBase () && !ColonyBeingAttacked (); 
		if (stopAttackInvisibleMonster) {
			return true;
		}

		bool actionNotNull = currentAction != null;
		if (actionNotNull) {
			bool pickFoodNotAhead = (currentAction.Action () == Action.PICK_FOOD && !FoodAhead ());
			bool eatFoodNot = (currentAction.Action () == Action.EAT && !HasFood ());
			bool fightMonsterNotAhead = (currentAction.Action () == Action.FIGHT_MONSTER && !EnemyAhead ());
			return pickFoodNotAhead || eatFoodNot || fightMonsterNotAhead; 
		}

		//bool notPossible = currentAction == null || !myBeliefs.ContainsKey(currentAction.Position ());
		//Debug.Log (notPossible);
		return true;
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

	bool PlanIsEmpty () {
		return currentPlan.Count == 0 && CurrentActionHasEnded ();
	}

	bool Reconsider () {

		if (currentAction.Action () == Action.FIGHT_MONSTER && EnemyAhead ()) {
			return false;
		}
		bool amIGoingToDie = HasFood () && HasLowLife () && !CanMakeItThere (myColonyPosition);
		bool haveIReceivedHelpReq = HelpRequestReceived ();
		bool helpingOther = myCurrentIntention.Intention () == Intention.HELP_OTHER_AT;
		if (helpingOther && !EnemyAhead () && !amIGoingToDie) {
			return false;
		}
		return haveIReceivedHelpReq || EnemyAhead () || EnemyOnSight () || amIGoingToDie || SpecFoodOnSight () || (!FoodToPopulate () && myCurrentIntention.Intention () == Intention.POPULATE_AT);
	}


	IList<IntentionDetails> RetrieveIntentionsFromDesires (IList<Desire> desires) {

		IList<IntentionDetails> myCurrentIntentions = new List<IntentionDetails> () ;
	
		foreach (var desire in desires) {
			if (desire == Desire.HELP_SELF) {
				float eat_food_weight = 1.0f;
				if (EnemyAhead () && enemy != null) {
					IntentionDetails intention = new IntentionDetails(Intention.ATTACK_MONSTER_AT,1.0f,enemy.transform.position);
					myCurrentIntentions.Add (intention);
					continue;
				}
				//FIXME
				/*else if (EnemyOnSight ()) {
					IntentionDetails intention = new IntentionDetails(Intention.ATTACK_MONSTER_AT,1f,enemyOnSight.transform.position);
					myCurrentIntentions.Add (intention);
					eat_food_weight = 0.5f;
				}*/

				if (HasFood() && HasLowLife ()) {
					IntentionDetails intention = new IntentionDetails(Intention.EAT_FOOD,1.0f*eat_food_weight,this.transform.position);
					myCurrentIntentions.Add (intention);
					continue;
				}

				float colony_multiplier = 0.9f;
				//Food source is always a better option
				if(KnowWhereFoodSourceIs ()) {
					Vector3 closestFoodSource = ClosestFoodSource ();
					float distance_to_foodsource = DistanceBetweenMeAndPoint(closestFoodSource);
					float distance_to_col = DistanceBetweenMeAndPoint(myColonyPosition);
					float weight = 0.0f;
					if (distance_to_foodsource < distance_to_col) {
						weight = 0.9f;
						IntentionDetails intention = new IntentionDetails(Intention.GET_FOOD_AT,1.0f*weight,myColonyPosition);
						myCurrentIntentions.Add (intention);
						continue;
					} 	
				} else if(KnowWhereSpecialFoodIs ()) {
					Vector3 closestFood = ClosestSpecialFood ();
					float distance_to_food = DistanceBetweenMeAndPoint(closestFood);
					float distance_to_col = DistanceBetweenMeAndPoint(myColonyPosition);
					float weight = 0.0f;
					if (distance_to_food < distance_to_col) {
						weight = 0.9f;
						//colony_multiplier = 0.5f;
						IntentionDetails intention = new IntentionDetails(Intention.GET_FOOD_AT,1.0f*weight,myColonyPosition);
						myCurrentIntentions.Add (intention);
						continue;
					} 
				} else if (KnowWhereFoodIs ()) {
					Vector3 closestFood = ClosestFood ();
					float distance_to_food = DistanceBetweenMeAndPoint(closestFood);
					float distance_to_col = DistanceBetweenMeAndPoint(myColonyPosition);
					float weight = 0.0f;
					if (distance_to_food < distance_to_col) {
						weight = 0.9f;
						//colony_multiplier = 0.5f;
						IntentionDetails intention = new IntentionDetails(Intention.GET_FOOD_AT,1.0f*weight,myColonyPosition);
						myCurrentIntentions.Add (intention);
						//continue;
					} 
				}

				IntentionDetails intention_details = new IntentionDetails(Intention.GOTO_COL_AT,1.0f*colony_multiplier, myColonyPosition);
				myCurrentIntentions.Add (intention_details);
			} else if (desire == Desire.POPULATE) {
				float weight = 0.8f;
				float distance_to_col = Mathf.Sqrt(DistanceBetweenMeAndPoint(myColonyPosition));
				float delta = 4.0f;
				if (distance_to_col < delta || AtBase ()) {
					weight = 0.9f;
				}
				IntentionDetails intention = new IntentionDetails(Intention.POPULATE_AT, weight, myColonyPosition);
				myCurrentIntentions.Add (intention);
			} else if (desire == Desire.DEFEND_COL) {
				float defend_col_multiplier = 1.0f;
				float number_of_ind_at_base = HowManyAtBase ();
				//The more there are at base the less I want to go there
				if (number_of_ind_at_base > 0 && !HasFood ()) {
					defend_col_multiplier = defend_col_multiplier / number_of_ind_at_base;
				}
				IntentionDetails intention = new IntentionDetails(Intention.ATTACK_MONSTER_AT, 1f*defend_col_multiplier, myColonyPosition);
				myCurrentIntentions.Add (intention);
			} else if (desire == Desire.GET_FOOD) {

				//FIXME
				bool knowWhereSpecialFoodIs = KnowWhereSpecialFoodIs ();
				if (knowWhereSpecialFoodIs) {
					float special_food_multiplier = 0.9f;
					Vector3 closestSpecialFood = ClosestSpecialFood ();
					bool canMakeToSpecFood = CanMakeItThere(closestSpecialFood);
					if(canMakeToSpecFood) {
						IntentionDetails intention = new IntentionDetails(Intention.GET_FOOD_AT, 1f*special_food_multiplier, closestSpecialFood);
						myCurrentIntentions.Add (intention);
						continue;
					}
				}

				//Only goes when there is not food on sight nor ahead
				bool knowWhereFoodSourceIs = KnowWhereFoodSourceIs ();
				if (knowWhereFoodSourceIs && !FoodOnSight () && !FoodAhead ()) {
					float foodsource_mul = 0.9f;
					Vector3 closestFoodSource = ClosestFoodSource ();
					bool canMakeToFoodSource = CanMakeItThere(closestFoodSource);
					if(canMakeToFoodSource) {
						IntentionDetails intention = new IntentionDetails(Intention.GOTO_FOODSOURCE_AT, 1f*foodsource_mul, closestFoodSource);
						myCurrentIntentions.Add (intention);
						continue;
					} 
				}

				bool knowWhereFoodIs = KnowWhereFoodIs ();
				bool knowWhereObsIs = KnowWhereObstacleIs ();
				if (knowWhereFoodIs) {
					float food_mul = 0.9f;
					Vector3 closestFood = ClosestFood ();
					bool canMakeToFood = CanMakeItThere(closestFood);
					if(canMakeToFood) {
						if ((!knowWhereObsIs) || (knowWhereObsIs && !HasHighLife ())) {
							IntentionDetails intention = new IntentionDetails(Intention.GET_FOOD_AT, 1f*food_mul, closestFood);
							myCurrentIntentions.Add (intention);
							continue;
						} 
					} 
				}

				if (knowWhereObsIs){
					Vector3 closestObstacle = ClosestObstacle ();
					bool canMakeToObs = CanMakeItThere(closestObstacle);
					if(canMakeToObs) {
						IntentionDetails intention = new IntentionDetails(Intention.DESTROY_WALL_AT, 0.9f, closestObstacle);
						myCurrentIntentions.Add (intention);
						continue;
					} 
				}
				//Search for food randomly
				IntentionDetails intention_det = new IntentionDetails(Intention.SEARCH_FOOD, 0.7f, MoveRandomly());
				myCurrentIntentions.Add (intention_det);

			} else {
				//desire == Desire.HELP_OTHER
				float help_multiplier = 1.0f;
				/*if(HasLowLife ()) {
					help_multiplier = 0.9f;
				}*/
				IntentionDetails intention = new IntentionDetails(Intention.HELP_OTHER_AT, 1f*help_multiplier, help_position);
				myCurrentIntentions.Add (intention);

			}
		}
		return myCurrentIntentions;
	}


	void RemoveRequestHelp () {
		var beliefs = myBeliefs.Keys;
		foreach(var belief in beliefs) {
			if(myBeliefs [belief] == "HelpOther") {
				help_position = belief;
				myBeliefs.Remove(belief);
				return;
			}
		}
	}

	//Not used
	bool Sound () {
		//always true due to our implementation using navmesh
		return true;
	}

	bool Succeeded () {
		//have i done what i wanted to?
		return PlanIsEmpty ();
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
		return enemyAhead;
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
		return health <= 20f;
	}
	
	public bool HasFood ()	{
		return hasFood;
	}

	public bool HasHighLife () {
		return health >= 60f;
	}

	bool IsSomethingAhead() {
		return ObstacleAhead () || EnemyAhead () || FoodAhead ();
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
		if (enemy != null && EnemyAhead ()) {
			Monster monster = enemy.GetComponent<Monster> ();
			monster.TakeDamage (gameObject);
		} else {
			enemyAhead = false;
			Material mat = this.transform.GetChild(0).GetChild(0).GetComponent<Renderer> ().material;
			mat.color = myColor;
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
		int rand = Random.Range(1,100);
		float multiplier = 2.0f;
		/*transform.Rotate (0f,-90f,0f);
		return this.transform.position + this.transform.position.left*multiplier;
		*/
		/*
		if (EndOfWorld ()) {
			endOfWorld = false;
			return this.transform.position - this.transform.forward*multiplier;
		}*/
		if (rand <= 2) {
			//transform.Rotate (0f,-90f,0f);
			return this.transform.position - this.transform.right*multiplier;
		} else if(rand <= 4) {
			return this.transform.position + this.transform.right*multiplier;
				//transform.Rotate (0f,90f,0f);
		} else {
			return this.transform.position + this.transform.forward*multiplier;
		}
	}

	void PickFood () {
		this.foodAhead = false;
		this.hasFood = true;
		PickUpable foodComp = food.GetComponent<PickUpable> ();
		foodComp.SetBeingCarried (true);
		foodComp.SetCarrying (this);
		Vector3 foodPosition = food.transform.position;
		foodPosition.y = 1.5f;
		// We do not need to know where the food we have is
		//if(myBeliefs.ContainsKey(foodPosition)) {
		string item;
		if(myBeliefs.TryGetValue(foodPosition, out item)) {
			commModule.Broadcast(SpeechAtc.INFORM_REMOVE, foodComp.gameObject.tag, foodPosition);
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



	void CallForHelpIfNeeded () {
		bool aloneAtBase = AtBase () && HowManyAtBase () == 1;
		if (aloneAtBase || HasLowLife () || HasFood ()) {
			this.commModule.Broadcast(SpeechAtc.REQUEST_ADD, "Monster", this.transform.position);
			RemoveRequestHelp ();
		}
	}

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
		enemyAhead = false;
		SendBack ();
	}

	////////////////////////////////////////////////////////////////////////


	/*
	 * Public (Being called outside Agent)
	 */


	public void AddToBeliefs(string name, Vector3 position) {
		bool hasBelief = myBeliefs.ContainsKey(position) && myBeliefs [position] == name;
		if(!hasBelief) {
			myBeliefs [position] = name;
		}
	}

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
			Object.Destroy(this);
		}
	}


	
	public void EatFood (float healHealth) {
		this.health += healHealth;
	}

	public void HelpRequest (string name, Vector3 position) {
		if (position != this.transform.position) {
			bool go_help_other = true;
		}
		
	}

	public void RemoveBelief(string name, Vector3 position) {
		bool hasBelief = myBeliefs.ContainsKey(position) && myBeliefs [position] == name;
		if(hasBelief) {
			myBeliefs.Remove(position);
		}
	}

	public void SendBack () {
		transform.Rotate (0f, 180f, 0f);
		endOfWorld = false;
		return;
	}

	// Setters


	public void SetColor(Color myColor) {
		this.myColor = myColor;
	}

	public void SetColonyPosition (Vector3 position) {
		myColonyPosition = position;
		myBeliefs [position] = "MyCol";
	}

	public void SetMyColony (GameObject myColony) {
		this.myColony = myColony;
		myColonyComp = myColony.gameObject.GetComponent<Colony> ();
		commModule.SetColonyComponent (myColonyComp);
	}

	public void SetIsFoodSourceOnSight (bool isFoodSourceOnSight, GameObject foodSourceOnSight){
		this.isFoodSourceOnSight = isFoodSourceOnSight;
		this.foodSourceOnSight = foodSourceOnSight;
		if(isFoodSourceOnSight) {
			Vector3 foodSourcePosition = foodSourceOnSight.transform.position;
			if (!myBeliefs.ContainsKey(foodSourcePosition)) {
				commModule.Broadcast(SpeechAtc.INFORM_ADD, "FoodSource", foodSourcePosition);
				AddToBeliefs("FoodSource", foodSourcePosition);
			}
		}
	}

	public void SetIsFoodOnSight (bool isFoodOnSight, GameObject foodOnSight) {
		this.isFoodOnSight = isFoodOnSight;
		this.foodOnSight = foodOnSight;
		if(isFoodOnSight) {
			Vector3 foodPosition = foodOnSight.transform.position;
			if(!myBeliefs.ContainsKey(foodPosition)) {
				commModule.Broadcast(SpeechAtc.INFORM_ADD, "Food", foodPosition);
				AddToBeliefs("Food", foodPosition);
			}
		}
	}

	public void SetIsSpecFoodOnSight (bool isSpecFoodOnSight, GameObject specFoodOnSight) {
		this.isSpecFoodOnSight = isSpecFoodOnSight;
		this.specFoodOnSight = specFoodOnSight;
		if(isSpecFoodOnSight) {
			Vector3 foodPosition = specFoodOnSight.transform.position;
			if(!myBeliefs.ContainsKey(foodPosition)) {
				commModule.Broadcast(SpeechAtc.INFORM_ADD, "SpecFood", foodPosition);
				AddToBeliefs("SpecFood", foodPosition);
			}
		}
	}

	public void SetIsEnemyOnSight (bool isEnemyOnSight, GameObject enemyOnSight) {
		this.isEnemyOnSight = isEnemyOnSight;
		this.enemyOnSight = enemyOnSight;
		//FIXME CallForHelpIfNeeded ();
	}

	public void SetIsObstacleOnSight (bool isObstacleOnSight, GameObject obstacleOnSight) {
		this.isObstacleOnSight = isObstacleOnSight;
		this.obstacleOnSight = obstacleOnSight;
		if(isObstacleOnSight) {
			Vector3 obsPosition = obstacleOnSight.transform.position;
			if(!myBeliefs.ContainsKey(obsPosition)) {
				commModule.Broadcast(SpeechAtc.INFORM_ADD, "Wall", obsPosition);
				AddToBeliefs("Wall", obsPosition);
			}
		}
	}

	public void SetIsColonyOnSight (bool isColonyOnSight, GameObject colonyOnSight) {
		this.isColonyOnSight = isColonyOnSight;
		this.colonyOnSight = colonyOnSight;
	}

	public void SetEndOfWorld() {
		endOfWorld = true;
	}
	
	public void SetEnemyInFront(bool enemyAhead, GameObject enemy) {
		if (enemyAhead) {
			CallForHelpIfNeeded ();
		}
		this.enemyAhead = enemyAhead;
		this.enemy = enemy;
	}

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
	
	public void TakeDamage() {
		Material mat = this.transform.GetChild(0).GetChild(0).GetComponent<Renderer> ().material;
		mat.color = flashColour;
		DecreaseLife (hitRate);

		//Debug.Log ("Agent Hitpoints: " + health);
		mat.color = Color.Lerp (flashColour, myColor, flashSpeed * Time.deltaTime);
	}

}

