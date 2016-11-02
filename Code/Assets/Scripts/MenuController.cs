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
		if (Input.GetMouseButtonDown (0) == true) {
			MainController.SwitchScenes ("Crew Scene");
		}
			
	}

	public void OnNewGameButton( Event evt ) {
	}

	public void OnHelpButton( ) {
	}

	public void OnAutiodButton( ) {
	}

	public void OnQuitButton( ) {
	}
}
