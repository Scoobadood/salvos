using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// The Cursor Manager is responsible for selecting and drawing the appropriate cursor icon as the player moves the
/// mouse over the game.
/// The CursorManager also detects boundaries of the window and is responsible for scrlling the Camera
/// The Basic Cursor Manager tracks the MapPosition of the cursor in the map.
/// </summary>
public class CursorManager : MonoBehaviour
{
	// The PathArrow prefab
	public GameObject pathArrowPrefab;

	// The current position of the cursor in map coords
	protected MapPosition mCursorPosition;

	// The map
	protected TileMap	mMap;

	// The cursor - created locally
	protected GameObject mCursor;

	// Direction highlight arrows
	private List<PathArrow> mPathArrowPool;

	// Size of arrows in pool
	private static int POOL_SIZE = 10;

	/// <summary>
	/// Position changed delegate.
	/// </summary>
	protected delegate void PositionChangedDelegate (MapPosition oldPosition, MapPosition newPosition);

	protected PositionChangedDelegate PositionChanged;


	/// <summary>
	/// Tile selected delegate.
	/// </summary>
	protected delegate void TileSelectedDelegate (MapPosition oldPosition, Tile tile);

	protected TileSelectedDelegate TileSelected;

	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start ()
	{
		mMap = FindObjectOfType<TileMap> ();

		CreateCursor ();

		CreateDirectionHighlights ();

		// Pick initial cursor mode
		SetHiddenMode ();
	}


	void Update ()
	{
		UpdateCursorPosition ();

		if (Input.GetMouseButtonDown (0) ) {
			TileClickedAt( mCursorPosition, mMap.TileAt( mCursorPosition.mapX, mCursorPosition.mapY) );
		}
	}


	private void UpdateCursorPosition( ) {
		// Fire a ray and see if it hits the player
		Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);

		// Find the player that got clicked
		RaycastHit hitInfo;
		Collider c = mMap.gameObject.GetComponent<Collider> ();
		bool hit = c.Raycast (ray, out hitInfo, 10000.0f);
		if (hit) {
			Vector3 mapHit = hitInfo.point;
			MapPosition newPosition = mMap.WorldToMap (mapHit);
			if (mCursorPosition.mapX != newPosition.mapX || mCursorPosition.mapY != newPosition.mapY) {
				CursorEnteredTileAt (newPosition);
			}
		}
	}


	/// <summary>
	/// Creates the cursor.
	/// </summary>
	private void CreateCursor ()
	{
		mCursor = GameObject.CreatePrimitive (PrimitiveType.Plane);
		mCursor.name = "Cursor";
		mCursor.transform.position = new Vector3 (0f, 0.01f, 0f);
		mCursor.transform.localRotation = Quaternion.identity;
		mCursor.transform.localScale = new Vector3 (0.05f, 1.0f, 0.05f);
		mCursor.GetComponent<Renderer> ().material.color = Color.red;
		//		mCursor.layer = LayerMask.NameToLayer ("Cursor Layer");
		GameObject.Destroy (mCursor.GetComponent<Collider> ());


		mCursorPosition = new MapPosition (0, 0);
	}

	/// <summary>
	/// Creates the highlight layer; A pane, the same size as the map, above the map on which tile selections are drawn
	/// </summary>
	private void CreateDirectionHighlights ()
	{
		string[] directions = { "Up", "UpRight", "Right", "DownRight", "Down", "DownLeft", "Left", "UpLeft" };
		mPathArrowPool = new List<PathArrow> ();

		foreach (string directionName in directions) {
			for (int i = 1; i < POOL_SIZE; i++) {
				PathArrow pathArrow = ((GameObject) Instantiate<GameObject> (pathArrowPrefab )).GetComponent<PathArrow> ();
				pathArrow.gameObject.SetActive (false);
				mPathArrowPool.Add (pathArrow);
			}
		}
	}

	/// <summary>
	/// Called when the cursor enters a new tile. Update the local map position
	/// Also move the cursor in the 3D world and then call the current PositionChanged delegate
	/// which will do cursor mode specific behaviour
	/// </summary>
	/// <param name="position">Position.</param>
	public void CursorEnteredTileAt (MapPosition position)
	{
		MapPosition oldPosition = mCursorPosition;
		mCursorPosition = position;
		Vector3 newWorldPos = mMap.MapToWorld (position.mapX, position.mapY);
		newWorldPos.y = 0.01f;
		mCursor.transform.position = newWorldPos;

		if (PositionChanged != null) {
			PositionChanged (oldPosition, position);
		}
	}


	public void TileClickedAt (MapPosition position, Tile tile)
	{
		if (TileSelected != null) {
			TileSelected (position, tile);
		}
	}


	/// <summary>
	/// Sets the cursor material.
	/// </summary>
	/// <param name="texture">Texture.</param>
	private void SetCursorMaterial (Material material)
	{
		if (material == null) {
			mCursor.SetActive (false);
		} else {
			mCursor.GetComponent<Renderer> ().material = material;
			mCursor.SetActive (true);
		}
	}

	/// <summary>
	/// The default position changed method does nothing
	/// </summary>
	/// <param name="oldPosition">Old position.</param>
	/// <param name="newPosition">New position.</param>
	private void DefaultPositionChanged (MapPosition oldPosition, MapPosition newPosition)
	{
	}

	/* ********************************************************************************
	 * 
	 *  THe player selection cursor
	 * 
	 * ********************************************************************************/

	/// <summary>
	/// Turns th ecursor view off
	/// </summary>
	public void SetHiddenMode ()
	{
		PositionChanged = null;
		TileSelected = null;
		SetCursorMaterial (null);
	}

	/* ********************************************************************************
	 * 
	 *  The Movement Cursor
	 * 
	 * ********************************************************************************/

	/// <summary>
	/// Sets the movement mode.
	/// </summary>
	public void SetMovementMode ()
	{
		PositionChanged = MovementCursorPositionChanged;
		TileSelected = MovementCursorTileSelected;
	}


	// A list of positions defining the current movement path
	private List<MapPosition> mCurrentMovementPath = new List<MapPosition> ();

	// A list of rendered path cursor tiles
	private List<PathArrow> mRenderedPath = new List<PathArrow> ();



	/// <summary>
	/// The default position changed method does nothing
	/// </summary>
	/// <param name="oldPosition">Old position.</param>
	/// <param name="newPosition">New position.</param>
	private void MovementCursorPositionChanged (MapPosition oldPosition, MapPosition newPosition)
	{
		List<MapPosition> newPath = ComputeMovementPathForCurrentPlayer (newPosition);

		Material material = null;

		ClearMovementPath ();
		mCurrentMovementPath = newPath;
		if (mCurrentMovementPath != null) {
			RenderMovementPath ();
			material = Resources.Load ("Materials/WalkHere", typeof(Material)) as Material;
		} else {
			material = Resources.Load ("Materials/NoEntry", typeof(Material)) as Material;
		}
		SetCursorMaterial (material);
	}

	/**
	 * Clear the existing movement path from the map
	 */
	private void ClearMovementPath ()
	{
		if (mRenderedPath.Count > 0) {
			mRenderedPath.ForEach (c => c.gameObject.SetActive (false));
			mRenderedPath.Clear ();
		}
	}

	/**
	 * Plot the current movement path on the map
	 */
	private void RenderMovementPath ()
	{
		if (mCurrentMovementPath == null)
			return;

		// Ensure rendered path is clear
		mRenderedPath.ForEach( pathArrow => pathArrow.gameObject.SetActive( false ) );
		mRenderedPath.Clear ();

		// We don't plot anything at the first step or last step
		for (int i = 1; i < mCurrentMovementPath.Count - 1; i++) {

			// Work out where to put it
			MapPosition mp = mCurrentMovementPath [i];

			// And where next one is
			MapPosition nextMp = mCurrentMovementPath [i + 1];

			// Get the next thing
			PathArrow pa = mPathArrowPool [i - 1];

			Vector3 location = mMap.MapToWorld (mp.mapX, mp.mapY);
			location.y = 0.01f;
			pa.transform.position = location;

			if (nextMp.mapX > mp.mapX) {
				if (nextMp.mapY > mp.mapY) {
					pa.SetState (PathArrow.CursorState.DownLeftArrow);
				} else if (nextMp.mapY < mp.mapY) {
					pa.SetState (PathArrow.CursorState.UpLeftArrow);
				} else {
					pa.SetState (PathArrow.CursorState.LeftArrow);
				}
			} else if (nextMp.mapX < mp.mapX) {
				if (nextMp.mapY > mp.mapY) {
					pa.SetState (PathArrow.CursorState.DownRightArrow);
				} else if (nextMp.mapY < mp.mapY) {
					pa.SetState (PathArrow.CursorState.UpRightArrow);
				} else {
					pa.SetState (PathArrow.CursorState.RightArrow);
				}
			} else {
				if (nextMp.mapY > mp.mapY) {
					pa.SetState (PathArrow.CursorState.DownArrow);
				} else {
					pa.SetState (PathArrow.CursorState.UpArrow);
				}
			}

			pa.gameObject.SetActive (true);

			mRenderedPath.Add (pa);

		} 
	}

	/**
	 * Compute the movement path for the currently selected player to the given tile coordinate
	 * @param target The target map location
	 * @return A path between the player's current position and the target or null if one cannot be found
	 */


	/// <summary>
	/// Computes the movement path for current player given the cursor location
	/// </summary>
	/// <returns>The movement path for current player.</returns>
	/// <param name="target">Target.</param>
	private List<MapPosition> ComputeMovementPathForCurrentPlayer (MapPosition target)
	{

		CrewmanToken player = FindObjectOfType<GameManager> ().CurrentPlayer ();

		MapPosition playerPosition = player.GetComponent<MapObject> ().Position;

		AStarPathFinder pathFinder = new AStarPathFinder (mMap);

		// Constrain the search using player's action points
		Dictionary<string, string> options = new Dictionary<string,string> () { { "APs", "" + player.GetActionPoints () } };

		List<MapPosition> path = pathFinder.FindPath (playerPosition.mapX, playerPosition.mapY, target.mapX, target.mapY, options);

		return path;
	}


	private void MovementCursorTileSelected (MapPosition position, Tile tile)
	{
		List<MapPosition> newPath = ComputeMovementPathForCurrentPlayer (position);
		if (newPath != null) {
			// Turn off walk path
			ClearMovementPath( );


			FindObjectOfType<GameManager> ().MovePlayer (newPath);
		}
	}
		

	/* ********************************************************************************
	 * 
	 *  The Salvage Cursor
	 * 
	 * ********************************************************************************/

	/// <summary>
	/// Sets the movement mode.
	/// </summary>
	public void SetSalvageMode ()
	{
		PositionChanged = SalvageCursorPositionChanged;
		TileSelected = SalvageCursorTileSelected;
	}

	private void SalvageCursorPositionChanged (MapPosition oldPosition, MapPosition newPosition)
	{
		Material material = Resources.Load ("Materials/NoEntry", typeof(Material)) as Material;

		List<MapObject> items = mMap.GetObjectsAtPosition (newPosition);
		foreach (MapObject item in items) {
			if (item.GetComponent<Salveagable> () != null) {
				material = Resources.Load ("Materials/SalvageHere", typeof(Material)) as Material;
				break;
			}
		}

		SetCursorMaterial (material);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="CursorManager"/> class.
	/// </summary>
	/// <param name="position">Position.</param>
	/// <param name="tile">Tile.</param>
	private void SalvageCursorTileSelected (MapPosition position, Tile tile)
	{
		// TODO Add Salvage To Inventory
		// TODO Burn points in salvage activity
		// TODO Make a skill role on technical skills
		// TODO Could impact ship ?

		// For now, remove from map
		mMap.Remove<Salveagable> (position);
		FindObjectOfType<GameManager> ().SalvageComplete ();

	}
}
