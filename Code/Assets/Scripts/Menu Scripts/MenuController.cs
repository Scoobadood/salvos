using UnityEngine;
using System.Collections;


/// <summary>
/// This is the Game Main Menu
/// Options available here are to
/// <list type="unordered">
/// <item>Launch a new game</item>
/// <item>Help</item>
/// <item>Audio</item>
/// <item>Quit</item>
/// </summary>
public class MenuController : MonoBehaviour {

	private static MenuController menuController;

	/// <summary>
	/// Create singleton instance
	/// </summary>
	protected void Awake( ) {
		menuController = this;
	}

	/// <summary>
	/// Tidy up on destroy
	/// </summary>
	protected void OnDestroy( ) {
		if (menuController != null) {
			menuController = null;
		} 
	}

	/// <summary>
	/// Launch game on a mouse click
	/// </summary>
	void Update () {
	}




	/// ------------------------------------------------------------------------------------------
	///     Instance methods
	/// ------------------------------------------------------------------------------------------


	/// <summary>
	/// Launch into the next screen
	/// </summary>
	public void OnNewGameButton( ) {
		MainController.SwitchScenes ("Crew Scene");
	}

	/// <summary>
	/// Quit
	/// </summary>
	public void OnQuitButton( ) {
		#if UNITY_EDITOR
			// If we're in the editor, just exit play mode
			UnityEditor.EditorApplication.isPlaying = false;
		#else
			Application.Quit();
		#endif
	}
}
