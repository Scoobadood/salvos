using System;
using UnityEngine;
using System.Collections;

public class MapPosition : IEquatable<MapPosition>	 {
	public int mapX;

	public int mapY;

	public MapPosition( int x, int y ) {
		mapX = x;
		mapY = y;
	}

	/// <summary>
	/// Returns a <see cref="System.String"/> that represents the current <see cref="MapPosition"/>.
	/// </summary>
	/// <returns>A <see cref="System.String"/> that represents the current <see cref="MapPosition"/>.</returns>
	public override String ToString( ) {
		return "(" + mapX + "," + mapY + ")";
	}

	public bool Equals( MapPosition otherPosition ) {
		if (otherPosition == null)
			return false;
		
		return (mapX == otherPosition.mapX) && (mapY == otherPosition.mapY);
	}

	public override bool Equals(System.Object obj)
	{
		if (obj == null)
			return false;

		MapPosition mapPositionObj = obj as MapPosition;
		return Equals(mapPositionObj);
	}

	public override int GetHashCode()
	{
		return (""+mapX+","+mapY).GetHashCode();
	}

	public static bool operator == (MapPosition position1, MapPosition position2)
	{
		if (((object)position1) == null || ((object)position2) == null)
			return System.Object.Equals(position1, position2);

		return position1.Equals(position2);
	}

	public static bool operator != (MapPosition position1, MapPosition position2)
	{
		if (((object)position1) == null || ((object)position2) == null)
			return ! System.Object.Equals(position1, position2);

		return ! (position1.Equals(position2));
	}
}
