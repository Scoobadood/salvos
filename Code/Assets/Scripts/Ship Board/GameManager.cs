using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {

	/** Prefab player */
	public GameObject playerPrefab;

	public GameObject actionButtonPrefab;

	public GameObject salvagePrefab;

	public GameObject moveButton;
	public GameObject salvageButton;

	/** Players in the game */
	private List<CrewmanToken>	mPlayers = new List<CrewmanToken> ();


	/** Current player index */
	private int mCurrentPlayerIndex;

	/** The Map */
	private  TileMap map;

	/** State */
	private enum TurnState
	{
		WaitingToSelectPlayer,
		WaitingToSelectAction,
		WaitingToSelectActionTarget,
		WaitingForActionToComplete}

	;

	private TurnState mCurrentState = TurnState.WaitingToSelectPlayer;


	/** Actions */
	private enum Action
	{
		None,
		Move,
		Salvaging
	};

	private Action mCurrentAction = Action.None;

	// Use this for initialization
	void Start ()
	{
		Debug.Log ("GameManager:Start() : enter");

		map = FindObjectOfType<TileMap> ();
		SpawnPlayers ();
		SpawnSalvage ();

		mCurrentState = TurnState.WaitingToSelectPlayer;
		mCurrentAction = Action.None;
		Debug.Log ("GameManager:Start() : exit");
	}
	
	// Update is called once per frame
	void Update ()
	{
		handleInput ();

		// Make camera look at current player
		Camera.main.transform.position = CurrentPlayer().transform.position + new Vector3( 0, 6, -3 );
	}

	/* ********************************************************************************
	 * *
	 * *  Handle input
	 * *
	 * ********************************************************************************/

	/**
	 * Check for user input
	 */
	private void handleInput ()
	{
		// Handle Mouse Down
		if (Input.GetMouseButtonDown (0)) {
			if (mCurrentState == TurnState.WaitingToSelectPlayer || mCurrentState == TurnState.WaitingToSelectAction) {
				SelectPlayerAtMouse ();
			}
		}
		if (Input.GetKeyDown (KeyCode.Tab)) {
			if (mCurrentState == TurnState.WaitingToSelectAction) {
				NextPlayer ();
			}
		}
		if (Input.GetKeyDown (KeyCode.Escape)) {
			DeactivateButtons ();
			mCurrentState = TurnState.WaitingToSelectAction;
			mCurrentAction = Action.None;
			GetComponent<CursorManager>( ).SetHiddenMode ();
		}
	}


	public void MovePlayer (List<MapPosition> movementPath)
	{
		// Validate state
		if ( (mCurrentState == TurnState.WaitingToSelectActionTarget) && (mCurrentAction == Action.Move) ){
			mCurrentState = TurnState.WaitingForActionToComplete;
			CurrentPlayer ().GetComponent<PathMover>().MoveAlongPath (movementPath);
		} else {
			Debug.LogError( "Ignored request to move player when not in correct state" );
		}
	}





	/* ********************************************************************************
	* *
	* *  Player Selection
	* *
	* ********************************************************************************/

	/**
	 * Select a specific player
	 * @param playerIndex The index of the player to selecy
	 */
	private void SelectPlayer (int playerIndex)
	{
		if (playerIndex >= 0 && playerIndex < mPlayers.Count) {
			CurrentPlayer ().Unhighlight ();

			mCurrentPlayerIndex = playerIndex;

			CurrentPlayer().Highlight ();

			ShowActionButtons ();

		} else {
			Debug.Log ("Player index is out of range (" + playerIndex + ")");
		}
	}

	/**
	 * Got to the next player in sequence
	 */
	public void NextPlayer ()
	{
		int nextPlayerIndex = ((mCurrentPlayerIndex + 1) % mPlayers.Count);
		SelectPlayer ( nextPlayerIndex );
	}

	/**
	 * Select player by click
	 */
	private void SelectPlayerAtMouse ()
	{ 
		// Fire a ray and see if it hits the player
		Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);

		// Find the player that got clicked
		RaycastHit hitInfo;
		int hitPlayerIndex = -1;
		float closestPlayerDistance = 1000.0f;
		for (int i = 0; i < mPlayers.Count; i++) {
			if (mPlayers [i].GetComponent<Collider> ().Raycast (ray, out hitInfo, closestPlayerDistance)) {
				// Check if it's the closest 
				if (hitInfo.distance < closestPlayerDistance) {
					closestPlayerDistance = hitInfo.distance;
					hitPlayerIndex = i;
				}
			}
		}

		// If we got a player and it's not the current one, switch
		if (hitPlayerIndex >= 0) {
			SelectPlayer (hitPlayerIndex);
		}
	}

	/// <summary>
	/// Spawns the players.
	/// </summary>
	private void SpawnPlayers ()
	{
		Debug.Log ("Spawning players...");
		List<MapPosition> spawnPoints = map.SpawnPoints ();
		Debug.Log ("Spawning " + spawnPoints.Count);
		spawnPoints.ForEach (p => {
			CrewmanToken player = ((GameObject)Instantiate (playerPrefab)).GetComponent<CrewmanToken> ();
			MapObject mo = player.GetComponent<MapObject>();
			mo.Map = map;
			mo.Position = p;
			mPlayers.Add (player);
		});

		SelectPlayer (0);
	}

	/**
	 * @return the current player
	 */
	public CrewmanToken CurrentPlayer ()
	{
		return mPlayers [mCurrentPlayerIndex];
	}



	/**
	 * Spawn some Salvage
	 */
	private void SpawnSalvage ()
	{
		Debug.Log ("Spawning salvage...");
		List<MapPosition> spawnPoints = map.GetSalvageSpawnPoints ();
		spawnPoints.ForEach (p => {
			Salveagable salvage = ((GameObject)Instantiate (salvagePrefab)).GetComponent<Salveagable> ();
			MapObject mo = salvage.GetComponent<MapObject>();
			mo.Map = map;
			mo.Position = p;
		});
	}


	private void ActionComplete( ) {
		mCurrentAction = Action.None;
		mCurrentState = TurnState.WaitingToSelectAction;
		ShowActionButtons ();
	}


	/* ********************************************************************************
	 * *
	 * *  Movement
	 * *
	 * ********************************************************************************/


	public void PlayerMovementComplete ()
	{
		FindObjectOfType<CursorManager>().SetHiddenMode();
		ActionComplete ();
	}

	/* ********************************************************************************
	 * *
	 * *  GUI Management
	 * *
	 * ********************************************************************************/
	void ShowActionButtons ()
	{
		// Update the state
		mCurrentState = TurnState.WaitingToSelectAction;
		mCurrentAction = Action.None;

		moveButton.GetComponent<Button> ().interactable = true;
		moveButton.GetComponent<Button> ().onClick.AddListener (() => { 
			mCurrentState = TurnState.WaitingToSelectActionTarget;
			mCurrentAction = Action.Move;
			DeactivateButtons ();

			FindObjectOfType<CursorManager>().SetMovementMode();
		});

		// If there's salvage, add a salvage item
		if ( map.HasSalvageNear ( CurrentPlayer ().GetComponent<MapObject>().Position ) ) {
			salvageButton.GetComponent<Button> ().interactable = true;
			salvageButton.GetComponent<Button> ().onClick.AddListener (() => { 
				mCurrentState = TurnState.WaitingToSelectActionTarget;
				mCurrentAction = Action.Salvaging;
				DeactivateButtons ();

				FindObjectOfType<CursorManager>().SetSalvageMode();
			});
		} else {
			salvageButton.GetComponent<Button> ().interactable = false;
		}
	}

	public void SalvageComplete ()
	{
		FindObjectOfType<CursorManager>().SetHiddenMode();
		ActionComplete ();
	}


	void DeactivateButtons ()
	{
		// There are twoseveral canvases now; we need to get the right one!
		Button[] buttons = FindObjectsOfType<Button> ();
		foreach ( Button button in buttons) {
			button.interactable = false;
		}
	}

}
