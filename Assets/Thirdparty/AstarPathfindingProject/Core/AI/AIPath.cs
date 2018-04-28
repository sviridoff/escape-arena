using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Pathfinding {
	using Pathfinding.RVO;
	using Pathfinding.Util;

	/** AI for following paths.
	 * This AI is the default movement script which comes with the A* Pathfinding Project.
	 * It is in no way required by the rest of the system, so feel free to write your own. But I hope this script will make it easier
	 * to set up movement for the characters in your game.
	 * This script works well for many types of units, but if you need the highest performance (for example if you are moving hundreds of characters) you
	 * may want to customize this script or write a custom movement script to be able to optimize it specifically for your game.
	 * \n
	 * \n
	 * This script will try to follow a target transform. At regular intervals, the path to that target will be recalculated.
	 * It will in the #Update method try to move towards the next point in the path.
	 * However it will only move in roughly forward direction (Z+ axis) of the character, but it will rotate around it's Y-axis
	 * to make it possible to reach the destination.
	 *
	 * \section variables Quick overview of the variables
	 * In the inspector in Unity, you will see a bunch of variables. You can view detailed information further down, but here's a quick overview.\n
	 * The #repathRate determines how often it will search for new paths, if you have fast moving targets, you might want to set it to a lower value.\n
	 * The #target variable is where the AI will try to move, it can be a point on the ground where the player has clicked in an RTS for example.
	 * Or it can be the player object in a zombie game.\n
	 * The speed is self-explanatory, so is #rotationSpeed, however #slowdownDistance might require some explanation.
	 * It is the approximate distance from the target where the AI will start to slow down.\n
	 * #pickNextWaypointDist is simply determines within what range it will switch to target the next waypoint in the path.\n
	 *
	 * Below is an image illustrating several variables as well as some internal ones, but which are relevant for understanding how it works.
	 * \note The image is slightly outdated, replace forwardLook with pickNextWaypointDist in the image and ignore the circle for pickNextWaypointDist.
	 *
	 * \shadowimage{aipath_variables.png}
	 * This script has many movement fallbacks.
	 * If it finds an RVOController attached to the same GameObject as this component, it will use that. If it fins a character controller it will also use that.
	 * Lastly if will fall back to simply modifying Transform.position which is guaranteed to always work and is also the most performant option.
	 */
	[AddComponentMenu("Pathfinding/AI/AIPath (2D,3D)")]
	public partial class AIPath : AIBase, IAstarAI {
		/** Rotation speed.
		 * Rotation is calculated using Quaternion.RotateTowards. This variable represents the rotation speed in degrees per second.
		 * The higher it is, the faster the character will be able to rotate.
		 */
		[UnityEngine.Serialization.FormerlySerializedAs("turningSpeed")]
		public float rotationSpeed = 360;

		/** Distance from the target point where the AI will start to slow down.
		 * Note that this doesn't only affect the end point of the path
		 * but also any intermediate points, so be sure to set #pickNextWaypointDist to a higher value than this
		 */
		public float slowdownDistance = 0.6F;

		/** Determines within what range it will switch to target the next waypoint in the path */
		public float pickNextWaypointDist = 2;

		/** Distance to the end point to consider the end of path to be reached.
		 * When the end is within this distance then #OnTargetReached will be called and #TargetReached will return true.
		 */
		public float endReachedDistance = 0.2F;

		/** Draws detailed gizmos constantly in the scene view instead of only when the agent is selected and settings are being modified */
		public bool alwaysDrawGizmos;

		/** Slow down when not facing the target direction.
		 * Incurs at a small performance overhead.
		 */
		public bool slowWhenNotFacingTarget = true;

		/** What to do when within #endReachedDistance units from the destination.
		 * The character can either stop immediately when it comes within that distance, which is useful for e.g archers
		 * or other ranged units that want to fire on a target. Or the character can continue to try to reach the exact
		 * destination point and come to a full stop there. This is useful if you want the character to reach the exact
		 * point that you specified.
		 *
		 * \note #targetReached will become true when the character is within #endReachedDistance units from the destination
		 * regardless of what this field is set to.
		 */
		public CloseToDestinationMode whenCloseToDestination = CloseToDestinationMode.Stop;

		/** Current path which is followed */
		protected Path path;

		/** Helper which calculates points along the current path */
		protected PathInterpolator interpolator = new PathInterpolator();

		#region IAstarAI implementation

		/** \copydoc Pathfinding::IAstarAI::Teleport */
		public override void Teleport (Vector3 newPosition, bool clearPath = true) {
			if (clearPath) interpolator.SetPath(null);
			targetReached = false;
			base.Teleport(newPosition, clearPath);
		}

		/** \copydoc Pathfinding::IAstarAI::remainingDistance */
		public float remainingDistance {
			get {
				return interpolator.valid ? interpolator.remainingDistance + (interpolator.position - position).magnitude : float.PositiveInfinity;
			}
		}

		/** \copydoc Pathfinding::IAstarAI::targetReached */
		public bool targetReached { get; protected set; }

		/** \copydoc Pathfinding::IAstarAI::hasPath */
		public bool hasPath {
			get {
				return interpolator.valid;
			}
		}

		/** \copydoc Pathfinding::IAstarAI::pathPending */
		public bool pathPending {
			get {
				return waitingForPathCalculation;
			}
		}

		/** \copydoc Pathfinding::IAstarAI::steeringTarget */
		public Vector3 steeringTarget {
			get {
				return interpolator.valid ? interpolator.position : position;
			}
		}

		/** \copydoc Pathfinding::IAstarAI::maxSpeed */
		float IAstarAI.maxSpeed { get { return maxSpeed; } set { maxSpeed = value; } }

		/** \copydoc Pathfinding::IAstarAI::canSearch */
		bool IAstarAI.canSearch { get { return canSearch; } set { canSearch = value; } }

		/** \copydoc Pathfinding::IAstarAI::canMove */
		bool IAstarAI.canMove { get { return canMove; } set { canMove = value; } }

		#endregion

		protected override void OnDisable () {
			base.OnDisable();

			// Release current path so that it can be pooled
			if (path != null) path.Release(this);
			path = null;
			interpolator.SetPath(null);
		}

		/** The end of the path has been reached.
		 * If you want custom logic for when the AI has reached it's destination add it here. You can
		 * also create a new script which inherits from this one and override the function in that script.
		 *
		 * This method will be called again if a new path is calculated as the destination may have changed.
		 * So when the agent is close to the destination this method will typically be called every #repathRate seconds.
		 */
		public virtual void OnTargetReached () {
		}

		/** Called when a requested path has been calculated.
		 * A path is first requested by #UpdatePath, it is then calculated, probably in the same or the next frame.
		 * Finally it is returned to the seeker which forwards it to this function.
		 */
		public override void OnPathComplete (Path newPath) {
			ABPath p = newPath as ABPath;

			if (p == null) throw new System.Exception("This function only handles ABPaths, do not use special path types");

			waitingForPathCalculation = false;

			// Increase the reference count on the new path.
			// This is used for object pooling to reduce allocations.
			p.Claim(this);

			// Path couldn't be calculated of some reason.
			// More info in p.errorLog (debug string)
			if (p.error) {
				p.Release(this);
				return;
			}

			// Release the previous path.
			if (path != null) path.Release(this);

			// Replace the old path
			path = p;

			// Make sure the path contains at least 2 points
			if (path.vectorPath.Count == 1) path.vectorPath.Add(path.vectorPath[0]);
			interpolator.SetPath(path.vectorPath);

			var graph = AstarData.GetGraph(path.path[0]) as ITransformedGraph;
			movementPlane = graph != null ? graph.transform : GraphTransform.identityTransform;

			// Reset some variables
			targetReached = false;

			// Simulate movement from the point where the path was requested
			// to where we are right now. This reduces the risk that the agent
			// gets confused because the first point in the path is far away
			// from the current position (possibly behind it which could cause
			// the agent to turn around, and that looks pretty bad).
			interpolator.MoveToLocallyClosestPoint((GetFeetPosition() + p.originalStartPoint) * 0.5f);
			interpolator.MoveToLocallyClosestPoint(GetFeetPosition());

			var distanceToEnd = movementPlane.ToPlane(steeringTarget - position).magnitude + interpolator.remainingDistance;
			if (distanceToEnd <= endReachedDistance) {
				targetReached = true;
				OnTargetReached();
			}
		}

		/** Called during either Update or FixedUpdate depending on if rigidbodies are used for movement or not */
		protected override void MovementUpdateInternal (float deltaTime, out Vector3 nextPosition, out Quaternion nextRotation) {
			// a = v/t, should probably expose as a variable
			float acceleration = maxSpeed / 0.4f;

			if (updatePosition) {
				// Get our current position. We read from transform.position as few times as possible as it is relatively slow
				// (at least compared to a local variable)
				simulatedPosition = tr.position;
			}
			if (updateRotation) simulatedRotation = tr.rotation;

			var currentPosition = simulatedPosition;

			// Update which point we are moving towards
			interpolator.MoveToCircleIntersection2D(currentPosition, pickNextWaypointDist, movementPlane);
			var dir = movementPlane.ToPlane(steeringTarget - currentPosition);

			// Calculate the distance to the end of the path
			float distanceToEnd = dir.magnitude + Mathf.Max(0, interpolator.remainingDistance);

			// Check if we have reached the target
			var prevTargetReached = targetReached;
			targetReached = distanceToEnd <= endReachedDistance && interpolator.valid;
			if (!prevTargetReached && targetReached) OnTargetReached();
			float slowdown;

			// Check if we have a valid path to follow and some other script has not stopped the character
			if (interpolator.valid && !isStopped) {
				// How fast to move depending on the distance to the destination.
				// Move slower as the character gets closer to the destination.
				// This is always a value between 0 and 1.
				slowdown = distanceToEnd < slowdownDistance ? Mathf.Sqrt(distanceToEnd / slowdownDistance) : 1;

				if (targetReached && whenCloseToDestination == CloseToDestinationMode.Stop) {
					// Slow down as quickly as possible
					velocity2D -= Vector2.ClampMagnitude(velocity2D, acceleration * deltaTime);
				} else {
					velocity2D += MovementUtilities.CalculateAccelerationToReachPoint(dir, dir.normalized*maxSpeed, velocity2D, acceleration, maxSpeed) * deltaTime;
				}
			} else {
				slowdown = 1;
				// Slow down as quickly as possible
				velocity2D -= Vector2.ClampMagnitude(velocity2D, acceleration * deltaTime);
			}

			velocity2D = MovementUtilities.ClampVelocity(velocity2D, maxSpeed, slowdown, slowWhenNotFacingTarget, movementPlane.ToPlane(rotationIn2D ? tr.up : tr.forward));

			ApplyGravity(deltaTime);


			// Set how much the agent wants to move during this frame
			var delta2D = lastDeltaPosition = CalculateDeltaToMoveThisFrame(movementPlane.ToPlane(currentPosition), distanceToEnd, deltaTime);
			nextPosition = currentPosition + movementPlane.ToWorld(delta2D, verticalVelocity * lastDeltaTime);
			CalculateNextRotation(slowdown, out nextRotation);
		}

		protected virtual void CalculateNextRotation (float slowdown, out Quaternion nextRotation) {
			if (lastDeltaTime > 0.00001f) {
				Vector2 desiredRotationDirection;
				desiredRotationDirection = velocity2D;

				// Rotate towards the direction we are moving in.
				// Don't rotate when we are very close to the target.
				var currentRotationSpeed = rotationSpeed * Mathf.Max(0, (slowdown - 0.3f) / 0.7f);
				nextRotation = SimulateRotationTowards(desiredRotationDirection, currentRotationSpeed * lastDeltaTime);
			} else {
				// TODO: simulatedRotation
				nextRotation = rotation;
			}
		}

	#if UNITY_EDITOR
		[System.NonSerialized]
		int gizmoHash = 0;

		[System.NonSerialized]
		float lastChangedTime = float.NegativeInfinity;

		protected static readonly Color GizmoColor = new Color(46.0f/255, 104.0f/255, 201.0f/255);

		protected override void OnDrawGizmos () {
			base.OnDrawGizmos();
			if (alwaysDrawGizmos) OnDrawGizmosInternal();
		}

		protected override void OnDrawGizmosSelected () {
			base.OnDrawGizmosSelected();
			if (!alwaysDrawGizmos) OnDrawGizmosInternal();
		}

		void OnDrawGizmosInternal () {
			var newGizmoHash = pickNextWaypointDist.GetHashCode() ^ slowdownDistance.GetHashCode() ^ endReachedDistance.GetHashCode();

			if (newGizmoHash != gizmoHash && gizmoHash != 0) lastChangedTime = Time.realtimeSinceStartup;
			gizmoHash = newGizmoHash;
			float alpha = alwaysDrawGizmos ? 1 : Mathf.SmoothStep(1, 0, (Time.realtimeSinceStartup - lastChangedTime - 5f)/0.5f) * (UnityEditor.Selection.gameObjects.Length == 1 ? 1 : 0);

			if (alpha > 0) {
				// Make sure the scene view is repainted while the gizmos are visible
				if (!alwaysDrawGizmos) UnityEditor.SceneView.RepaintAll();
				Draw.Gizmos.Line(position, steeringTarget, GizmoColor * new Color(1, 1, 1, alpha));
				Gizmos.matrix = Matrix4x4.TRS(position, transform.rotation * (rotationIn2D ? Quaternion.Euler(-90, 0, 0) : Quaternion.identity), Vector3.one);
				Draw.Gizmos.CircleXZ(Vector3.zero, pickNextWaypointDist, GizmoColor * new Color(1, 1, 1, alpha));
				Draw.Gizmos.CircleXZ(Vector3.zero, slowdownDistance, Color.Lerp(GizmoColor, Color.red, 0.5f) * new Color(1, 1, 1, alpha));
				Draw.Gizmos.CircleXZ(Vector3.zero, endReachedDistance, Color.Lerp(GizmoColor, Color.red, 0.8f) * new Color(1, 1, 1, alpha));
			}
		}
	#endif

		protected override int OnUpgradeSerializedData (int version, bool unityThread) {
			base.OnUpgradeSerializedData(version, unityThread);
			// Approximately convert from a damping value to a degrees per second value.
			if (version < 1) rotationSpeed *= 90;
			return 2;
		}
	}
}
