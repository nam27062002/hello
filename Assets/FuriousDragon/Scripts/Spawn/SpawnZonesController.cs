using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// Restores entities that where destroyed, if the player returns to their area 
/// </summary>
public class SpawnZonesController: MonoBehaviour {


	public float respawnRange = 1500f; // distance the player has to be at minimum from a zone to consider respawning
	public float respawnTime = 30f;    // minimum time elapsed before respawn
	public float zoneSize = 1000f;     //  

	public enum State{
		OUT,
		IN
	};

    class SpawnElement{

		public Vector3 		originalPosition;
		public Vector3 		originalScale;
		public Quaternion 	originalRotation;

		public GameObject 	obj;
	};

	class SpawnZone{
	
		public Bounds bounds;
		public Bounds boundsOut;

		public List<SpawnElement>  elements;

		public float timer;
		public State state;
	};

	float sqrRange;
	Transform  player;

	List<SpawnZone> zones = new List<SpawnZone>();

	void Start () {

		player = GameObject.Find ("Player").transform;
		sqrRange = respawnRange*respawnRange;
	
	}
	
	void Update () {

		foreach(SpawnZone zone in zones){
		
			// Player is in the zone
			if (zone.state == State.OUT ){

				zone.timer += Time.deltaTime;
				if (zone.boundsOut.Contains(player.position)){
			
					//if(zone.timer > respawnTime){
			
						foreach(SpawnElement element in zone.elements){

							if (element.obj.GetComponent<SpawnableBehaviour>().state != SpawnableBehaviour.State.INVALID){

								element.obj.GetComponent<SpawnableBehaviour>().InRange( element.originalPosition, element.originalScale, element.originalRotation);
								element.obj.SetActive (true);
							}
						}
					//}
					zone.state = State.IN;
				}
			}else if (zone.state == State.IN && !zone.boundsOut.Contains(player.position)){
			
				foreach(SpawnElement element in zone.elements){

					if (element.obj.GetComponent<SpawnableBehaviour>().state != SpawnableBehaviour.State.INVALID){

						element.obj.GetComponent<SpawnableBehaviour>().OutOfRange();
						element.obj.SetActive (false);
					}
				}
				
				zone.state = State.OUT;
				zone.timer = 0f;
			}
		}
	}

	public void Add(GameObject spawnable){
	
		SpawnElement element = new SpawnElement();
		element.originalPosition = spawnable.transform.position;
		element.originalRotation = spawnable.transform.rotation;
		element.originalScale 	 = spawnable.transform.localScale;

		element.obj = spawnable;

	    // Check if there is a zone already we can use
		bool found = false;
		foreach(SpawnZone zone in zones){
			if (zone.bounds.Contains(element.originalPosition)){
				zone.elements.Add(element);
				found = true;
				break;
			}
		}

		// no zone exists that can fit this element, create a new one
		if (!found){
		
			SpawnZone zone = new SpawnZone();

			// find in which grid quad does this element belong to
			int quad_x = (int)(element.originalPosition.x / zoneSize);
			int quad_y = (int)(element.originalPosition.y / zoneSize);

			zone.bounds = new Bounds( new Vector3(quad_x*zoneSize+zoneSize*0.5f,quad_y*zoneSize+zoneSize*0.5f,0f),
			                          Vector3.one * zoneSize );
			zone.boundsOut = new Bounds( new Vector3(quad_x*zoneSize+zoneSize*0.5f,quad_y*zoneSize+zoneSize*0.5f,0f),
			                            Vector3.one * (zoneSize+respawnRange) );
			zone.elements = new List<SpawnElement>();
			zone.elements.Add(element);
			zone.timer = 0f;
			zone.state = State.OUT;

			zones.Add (zone);
		}
	}
}
