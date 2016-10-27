using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[RequireComponent ( typeof(MapObject) )]
public class PathMover : MonoBehaviour
{
	/** Speed of movement */
	public float 				speed;

		/** Path to follow */
	private List<MapPosition>	mPath;

	/** Flag indicating that the object is moving */
	private bool 				mIsMoving;

	/** My next target to move to */
	private Vector3				mTarget;

	/** Motionper second */
	private Vector3 mMovementPerSecond;


	/** Flag indicating that the thing is moving */
	public bool IsMoving {
		get { return mIsMoving; }
	}

	// Update is called once per frame
	void Update ( ) {
		if (mIsMoving) {
			Move ();
		}
	}


	/** 
	 * Move the associated GameObject along a path
	 * @param path The path to move along as a series of MapPositions
	 */
	public void MoveAlongPath( List<MapPosition> path ) {
		// If I'm already doing it, fail
		if (!mIsMoving) {

			// Otherwise, put the path in place
			if (path != null) {
				mPath = path;
				mIsMoving = true;

				// Start moving to first tile
				MoveToNextTile ();
			} else {
				Debug.LogError ("Requested movement path is null");
			}
		} else {
			Debug.LogError ("Instruction to move while already moving player is ignored");
		}

	}

	/**
	 * Start movement to next tile
	 */
	private void MoveToNextTile( ) {
		// Only if path is not null
		if (mPath != null) {

			// Assert that we have an entry in the oath
			if (mPath.Count > 0) {
				// Remove old first node (where I am) from path
				mPath.RemoveAt (0);

				// If there are any steps left
				if (mPath.Count > 0) {
					// Now grab new first node and move us to there
					MapPosition targetPosition = mPath [0];

					TileMap map = GetComponent<MapObject> ().Map;
					if (map != null) {

						mTarget = map.MapToWorld (targetPosition.mapX, targetPosition.mapY);
						mTarget.y = gameObject.transform.position.y;

						mMovementPerSecond = (mTarget - gameObject.transform.position) * speed;
					} else {
						Debug.Log ("Map is not set");
					}
				} else {
					// Must have arrived
					mIsMoving = false;
					mPath = null;

					// TODO: COnsider using Events to notify the listening party that I've come to a halt
					GameManager gm = FindObjectOfType<GameManager> ();
					gm.PlayerMovementComplete ();
				}
			} else {
				Debug.Log ("Path has no elements in MoveToNextTile");
			}
		} else {
			Debug.Log ("MoveNextTile when no movement path set");
		}
	}


	/**
	 * Update the position and check whether I've arrived at my target
	 */
	void Move( ) {
		// How far will I move
		Vector3 delta = mMovementPerSecond * Time.deltaTime;

		// Get my distance to target
		float oldDistanceToTarget = (gameObject.transform.position - mTarget).magnitude;

		// Update position
		gameObject.transform.position = gameObject.transform.position + delta;

		// Get my new distance to target. 
		float newDistanceToTarget = (gameObject.transform.position - mTarget).magnitude;

		// Stop when I start to get further away
		if ( newDistanceToTarget >= oldDistanceToTarget) {
			gameObject.transform.position = mTarget;

			GetComponent<MapObject>().Position = mPath [0];
			MoveToNextTile ();
		}
	}
}
