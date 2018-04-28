using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GhostEffect : MonoBehaviour
{
	public bool ghostingEnabled;
	public float effectDuration = 1f;
	public float spawnRate;

	private float _nextSpawnTime;

	public int trailLength = 1; //effect duration

	public int sortingLayer;

	public int maxNumberOfGhosts = 5;
	
	[SerializeField]
	private float
		_desiredAlpha = 0.8f;

	private List<GhostSprite> _inactiveGhostingSpritesPool; //a list of sprites stored in memory

	private Queue<GhostSprite> _ghostingSpritesQueue; 

	private SpriteRenderer _spriteRenderer;
	//private float _triggerTimer = 0.2f; // how often to trigger a ghosting image?
	
	//public Shader GhostShader;
	[SerializeField]
	private Color
		_desiredColor;
	private GameObject _ghostSpritesParent;
	private bool useTint;
	/// <summary>
	/// Average ms per frame
	/// </summary>
	/// 

	void Awake ()
	{
		_spriteRenderer = GetComponent<SpriteRenderer> ();
	}
	
	public List<GhostSprite> InactiveGhostSpritePool {
		get {
			if (_inactiveGhostingSpritesPool == null) {
				_inactiveGhostingSpritesPool = new List<GhostSprite> (5);
			}
			return _inactiveGhostingSpritesPool;
		}
		set { _inactiveGhostingSpritesPool = value; }
	}
	
	public Queue<GhostSprite> GhostingSpritesQueue {
		get {
			if (_ghostingSpritesQueue == null) {
				_ghostingSpritesQueue = new Queue<GhostSprite> (trailLength);
			}
			return _ghostingSpritesQueue;
		} //idito
		set { _ghostingSpritesQueue = value; }
		
	}
	
	public GameObject GhostSpritesParent {
		get {
			if (_ghostSpritesParent == null) {
				_ghostSpritesParent = new GameObject ();
				_ghostSpritesParent.transform.position = Vector3.zero;
				_ghostSpritesParent.name = "GhostspriteParent";
			}
			return _ghostSpritesParent;
		}
		set { _ghostSpritesParent = value; }
	}
	
	
	
	/// <summary>
	/// Stop the ghosting effect
	/// </summary>
	public void StopEffect ()
	{
		ghostingEnabled = false;
	}
	
	void Update ()
	{
		if (ghostingEnabled) { 
			//check for spawn rate
			//check if we're ok to spawn a new ghost
			if (Time.time >= _nextSpawnTime) {  
				//is the queue count number equal than the trail length? 
				if (GhostingSpritesQueue.Count == trailLength) { 
					GhostSprite peekedGhostingSprite = GhostingSpritesQueue.Peek ();
					//is it ok to use? 
					bool canBeReused = peekedGhostingSprite.CanBeReused ();
					if (canBeReused) {
						
						//pop the queue
						GhostingSpritesQueue.Dequeue ();
						GhostingSpritesQueue.Enqueue (peekedGhostingSprite);
						

						
						//initialize the ghosting sprite
						if (!useTint) {
							peekedGhostingSprite.Init (effectDuration, _desiredAlpha, _spriteRenderer.sprite, sortingLayer, _spriteRenderer.sortingOrder, transform, Vector3.zero);
						} else {
							peekedGhostingSprite.Init (effectDuration, _desiredAlpha, _spriteRenderer.sprite, sortingLayer, _spriteRenderer.sortingOrder, transform, Vector3.zero, _desiredColor);
						}
						_nextSpawnTime += spawnRate; 
					} else { //not ok, wait until next frame to try again 
						//peekedGhostingSprite.KillAnimationAndSpeedUpDissapearing();
						return;
					}
				}
				//check if the count is less than the trail length, we need to create a new ghosting sprite
				if (GhostingSpritesQueue.Count < trailLength) { 
					GhostSprite newGhostingSprite = Get ();
					GhostingSpritesQueue.Enqueue (newGhostingSprite); //queue it up!
					//newGhostingSprite.Init(_effectDuration, _desiredAlpha, _refSpriteRenderer.sprite, _sortingLayer,_refSpriteRenderer.sortingOrder-1, transform, Vector3.zero );
					
					if (!useTint) {
						newGhostingSprite.Init (effectDuration, _desiredAlpha, _spriteRenderer.sprite, sortingLayer, _spriteRenderer.sortingOrder, transform, Vector3.zero);
					} else {
						newGhostingSprite.Init (effectDuration, _desiredAlpha, _spriteRenderer.sprite, sortingLayer, _spriteRenderer.sortingOrder, transform, Vector3.zero, _desiredColor);
					}
					_nextSpawnTime += spawnRate; 
					
				}
				//check if the queue count is greater than the trail length. Dequeue these items off the queue, as they are no longer needed
				if (GhostingSpritesQueue.Count > trailLength) { 
					int difference = GhostingSpritesQueue.Count - trailLength;
					for (int i = 1; i < difference; i++) {
						GhostSprite gs = GhostingSpritesQueue.Dequeue ();
						InactiveGhostSpritePool.Add (gs);
					}
					return;
				}
			}
			
		}
		
		
		
	}
	
	
	
	
	
	/// <summary>
	/// Returns a ghosting sprite 
	/// </summary>
	/// <returns></returns>
	private GhostSprite Get ()
	{
		
		for (int i = 0; i < InactiveGhostSpritePool.Count; i++) {
			if (InactiveGhostSpritePool [i].CanBeReused ()) {
				return InactiveGhostSpritePool [i];
			}
			
		}
		return BuildNewGhostingSprite ();
		
		
	}
	
	private GhostSprite BuildNewGhostingSprite ()
	{
		//create a gameobject and set the current transform as a parent
		GameObject go = new GameObject ();
		go.transform.position = transform.position;
		go.transform.parent = GhostSpritesParent.transform;
		
		GhostSprite gs = go.AddComponent<GhostSprite> ();
		
		return gs;
	}
}
