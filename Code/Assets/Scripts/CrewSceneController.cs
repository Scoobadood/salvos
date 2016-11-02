using UnityEngine;
using System.Collections;


/// <summary>
/// The Crew Scene Controller looks after the crew management panel. It allows 
/// <list type="unordered">
/// <item>you to equip crew members with objects</item>
/// <item>view details of crew</item>
/// </summary>
public class CrewSceneController : MonoBehaviour {
	/// <summary>
	/// Static instance
	/// </summary>
	protected static CrewSceneController crewSceneController;

	/// <summary>
	/// Instantiate singleton instance
	/// </summary>
	protected void Awake( ) {
		crewSceneController = this;
	}




	protected void Start( ) {
		CreateCrewCards ();	
	}

	/// <summary>
	/// Wait for a mouse click to return to the main menu
	/// </summary>
	protected void Update () {
		if( Input.GetMouseButtonDown(0) == true ) {
			MainController.SwitchScenes( "Menu Scene" );
		}
	}

	/// <summary>
	/// Tidy up crewController after use
	/// </summary>
	protected void OnDestroy( ) {
		if( crewSceneController != null ) {
			crewSceneController = null;
		}
	}


	/// ------------------------------------------------------------------------------------------
	/// Crew Card Management
	/// ------------------------------------------------------------------------------------------
	protected void CreateCrewCards( ) {
		// Get the Crew


	}
}
