using UnityEngine;
using System.Collections;

public class MapObject : MonoBehaviour {
	/** The Map */
	private  TileMap mMap;

	public TileMap Map {
		get { return mMap; }
		set { mMap = value; }
	}

	/** Position on Map */
	public MapPosition Position {
		get { return mMap.PositionOf( this ); }
		set	{ 
			mMap.SetPositionOf (this, value);
			// Update the GameObject position
			Vector3 newWorldPosition = mMap.MapToWorld( value );
			newWorldPosition.y = gameObject.transform.position.y;
			gameObject.transform.position = newWorldPosition;
		}
	}

	public void setPosition( int mapX, int mapY ) {
		Position = new MapPosition (mapX, mapY);
	}
}
