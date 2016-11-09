using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

/// <summary>
/// This script accepts an EventSystem and a GameObject. It waits until it detects up/down
/// motion from a controller and then selects the default object.  This enables switching between
/// menu items etc. wth a controller.
/// Once an item is selecetd, the EventSystem handles naviagtion between items
/// </summary>
public class SelectOnInput : MonoBehaviour {

	/// <summary>
	/// Reference to the Event System fro the global canvas
	/// </summary>
	public EventSystem eventSystem;

	/// <summary>
	/// First item to select
	/// </summary>
	public GameObject selectedGameObject;

	private bool buttonSelected;

	// Use this for initialization
	void Start () {
	}
	
	/// <summary>
	/// Check for joystick movement to select default item
	/// </summary>
	void Update () {
		if (Input.GetAxisRaw ("Vertical") != 0 && !buttonSelected) {
			eventSystem.SetSelectedGameObject (selectedGameObject);
			buttonSelected = true;
		}
	
	}

	/// <summary>
	/// Ensure we deselect the button when we hde the panel
	/// </summary>
	private void OnDisable( ) {
		buttonSelected = false;
	}

}
