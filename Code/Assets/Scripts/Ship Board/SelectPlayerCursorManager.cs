using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// The Cursor Manager is responsible for selecting and drawing the appropriate cursor icon as the player moves the
/// mouse over the game.
/// The CursorManager also detects boundaries of the window and is responsible for scrlling the Camera
/// The Basic Cursor Manager tracks the MapPosition of the cursor in the map.
/// </summary>
public class SelectPlayerCursorManager : CursorManager
{
	/// <summary>
	/// Called when the position of the cursor has changed
	/// </summary>
	/// <param name="oldPosition">The old position of the cursor</param>
	/// <param name="newPosition">The new position of the cursor</param>
	protected void XXPositionChanged (MapPosition oldPosition, MapPosition newPosition)
	{}
}
