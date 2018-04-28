using UnityEngine;
using System.Collections;

[RequireComponent (typeof(Rigidbody2D))]
public class EnableGhostingAtVelocityThreshold2D : MonoBehaviour
{
	public GhostEffect ghostEffect;

	public float velocityXThreshold;
	public float velocityYThreshold;

	private Rigidbody2D _rigidbody2D;

	void Awake ()
	{
		_rigidbody2D = GetComponent<Rigidbody2D> ();
	}
	
	void LateUpdate ()
	{
		if (_rigidbody2D.velocity.x > velocityXThreshold || _rigidbody2D.velocity.y > velocityYThreshold) {
			ghostEffect.ghostingEnabled = true;
		} else {
			ghostEffect.ghostingEnabled = false;
		}
	}
}
