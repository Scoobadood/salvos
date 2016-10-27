using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Tile {
	/** Location of this tile */
	public MapPosition position;

	public int mCost;

	/**
	 * @return The cost to enter this tile
	 */
	public int GetCost( ) {
		return mCost;
	}


	/**
	 * @return true f the Tile is walkable
	 */
	public bool IsWalkable( ) {
		return (mCost >= 0);
	}
}