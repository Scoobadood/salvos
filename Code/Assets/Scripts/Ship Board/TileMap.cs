using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
[RequireComponent (typeof(MeshFilter))]
[RequireComponent (typeof(MeshCollider))]
[RequireComponent (typeof(MeshRenderer))]
public class TileMap : MonoBehaviour
{

	// THe number of tiles in each dimension
	public int tileMapWidth;
	public int tileMapHeight;

	// The size of a tile in real world metres
	public float tileWorldSize = 0.5f;


	// Terrain Tile
	public Texture2D	terrainTileTextures;

	// The size of a tile in ixels
	public int tileSizePixels;

	/** Is map 8-connected */
	public bool isEightConnected;

	/** Other Map Objects */
	private Dictionary<MapObject, MapPosition> mMapPositionForObject = new Dictionary<MapObject, MapPosition> ();
	private Dictionary<MapPosition, List<MapObject>>	mMapObjectsAtPosition = new Dictionary<MapPosition, List<MapObject>> ();


	/** Player spawn points */
	private List<MapPosition> mPlayerSpawnPoints = new List<MapPosition> ();


	// Tiles in the map
	private Tile[,] mTiles;


	/** Cached Mesh elements */
	private MeshFilter mMeshFilter;
	private MeshRenderer mMeshRenderer;
	private MeshCollider mMeshCollider;

	// Use this for initialization
	void Start ()
	{
		Debug.Log ("TileMap:Start() : enter");
		LoadMap ("Aeschylus");
		Debug.Log ("TileMap:Start() : exit");
	}


	// Check for intersects each tick
	void Update ()
	{
	}

	/// <summary>
	/// Reurns the Spawn points.
	/// </summary>
	/// <returns>The points.</returns>
	public List<MapPosition> SpawnPoints ()
	{
		return mPlayerSpawnPoints;
	}


	/// <summary>
	/// Gets the objects at position.
	/// </summary>
	/// <returns>The objects at position.</returns>
	/// <param name="position">Position.</param>
	public List<MapObject> GetObjectsAtPosition (MapPosition position)
	{
		List<MapObject> found;
		if (!mMapObjectsAtPosition.TryGetValue (position, out found)) {
			found = new List<MapObject> ();
		}
		return found;
	}


	/// <summary>
	/// Returns the position of the given MapObject in the Map or null if it's not there
	/// <param name="mapObject">The mapObject to locate</param>
	/// <returns>The MapPosition of the object</returns>
	/// </summary>
	public MapPosition PositionOf (MapObject mapObject)
	{
		MapPosition pos = null;

		bool found = mMapPositionForObject.TryGetValue (mapObject, out pos);

		if (!found) {
			Debug.LogError ("Request for position of unknown MapObject : " + mapObject);
		}
		return pos;
	}

	/// <summary>
	/// Set the position of the given object in the map.
	/// If the object already exists in the map then it's moved, otherwise it's placed
	/// Neither mapObject nor newPosition may be null
	/// <param name="mapObject">The MapObject to place</param>
	/// <param name = "newPosition">The position in the map at which to place it</param>
	/// </summary>
	public void SetPositionOf (MapObject mapObject, MapPosition newPosition)
	{
		if (mapObject != null) {
			if (newPosition != null) {

				// Check that th position is legal
				if (IsValidMapLocation (newPosition)) {

					// Get the current position for this object (may be null)
					MapPosition currentPosition = PositionOf (mapObject);

					// If I have an old position then remove it
					if (currentPosition != null) {
						InternalRemoveMapObject (mapObject, currentPosition);
					} 

					// Add at the new position
					InternalAddMapObject (mapObject, newPosition);
				} else {
					Debug.LogError ("Ignoring attempt to place MapObject at illegal location " + newPosition);
				}
			} else {
				Debug.LogError ("Attempt to reposition object to null MapPosition in Map.SetPositionOf");
			}
		} else {
			Debug.LogError ("Map Object is null in Map.SetPositionOf");
		}
	}


	public void Remove<T> (MapPosition position)  where T:MonoBehaviour
	{
		List<T> allItems = GetObjectsOfType<T> (position);
		if (allItems.Count > 0) {
			T item = allItems [0];
			List<MapObject> mapItems = GetObjectsAtPosition (position);
			mapItems.Remove (item.gameObject.GetComponent<MapObject> ());
			GameObject.Destroy (item.gameObject);
		}
	}

	private List<T> GetObjectsOfType<T> (MapPosition position)
	{
		List<T> items = new List<T> ();
		List<MapObject> allItems = GetObjectsAtPosition (position);
		allItems.ForEach (item => {
			T itemT = item.gameObject.GetComponent<T> ();
			if (itemT != null) {
				items.Add (itemT);
			}
		});

		return items;
	}

	/// <summary>
	/// Adds the given map object at the specified position. Should only be used internally to this class
	/// Adds the object to the list of objects at that locaton and sets the location for the object in the lookup dictionary
	/// </summary>
	/// <param name="mapObject">The MapObject to add</param>
	/// <param name="mapPosition">THe MapPosition to which to add it.</param>
	private void InternalAddMapObject (MapObject mapObject, MapPosition mapPosition)
	{
		List<MapObject> objectsAtPosition;

		bool ok = mMapObjectsAtPosition.TryGetValue (mapPosition, out objectsAtPosition);
		if (!ok) {
			objectsAtPosition = new List<MapObject> ();
			mMapObjectsAtPosition.Add (mapPosition, objectsAtPosition);
		}
		objectsAtPosition.Add (mapObject);
		mMapPositionForObject.Add (mapObject, mapPosition);
	}


	/// <summary>
	/// emove the given map object from the specified location. 
	/// </summary>
	/// <param name="mapObject">The MapObject to be removed</param>
	/// <param name="mapPosition">The MapPosition from which to remove it</param>
	private void InternalRemoveMapObject (MapObject mapObject, MapPosition mapPosition)
	{
		List<MapObject> objectsAtPosition;
		bool ok = mMapObjectsAtPosition.TryGetValue (mapPosition, out objectsAtPosition);
		if (ok) {
			ok = objectsAtPosition.Remove (mapObject);
			if (!ok) {
				Debug.LogError ("Ignoring attempt to remove specified object from position " + mapPosition + ". It's not there");
			}
		} else {
			Debug.LogError ("No objects at position " + mapPosition + " so can't remove one");
		}

		ok = mMapPositionForObject.Remove (mapObject);
		if (!ok) {
			Debug.LogError ("Couldn't remove poition " + mapObject + " for object as there is no known position for this object");
		}
	}


	/**
	 * @return A List of Slavage
	 */
	public List<MapPosition> GetSalvageSpawnPoints ()
	{
		return new List<MapPosition> () { new MapPosition (10, 11) };
	}


	/// <summary>
	/// Determines whether the specified mapX mapY is valid map location .
	/// </summary>
	/// <returns><c>true</c> if the specified mapX mapY is valid map location; otherwise, <c>false</c>.</returns>
	/// <param name="mapX">Map x.</param>
	/// <param name="mapY">Map y.</param>
	public bool IsValidMapLocation (int mapX, int mapY)
	{
		return ((mapX >= 0) && (mapX < tileMapWidth) &&
			(mapY >= 0) && (mapY < tileMapHeight) );
	}

	public bool IsValidMapLocation (MapPosition mapPosition)
	{
		return IsValidMapLocation (mapPosition.mapX, mapPosition.mapY);
	}


	/// <summary>
	/// Return the Tile at mapX and mapY.
	/// </summary>
	/// <returns>The <see cref="Tile"/> at the given location</returns>
	/// <param name="mapX">Map x.</param>
	/// <param name="mapY">Map y.</param>
	public  Tile TileAt (int mapX, int mapY)
	{
		Tile t = null;
		if (IsValidMapLocation (mapX, mapY)) {
			t = mTiles [mapX, mapY];
		} else {
			Debug.LogError ("Ignoring request for tile at invalid position (" + mapX + "," + mapY + ")");
		}
		return t;
	}

	/// <summary>
	/// Convert the given Map coorindate to World Coordinates
	/// </summary>
	/// <returns>A Vector3 with world coordinates. Note taht Y is set to 0 and will need to be adjusted based on the object being placed.</returns>
	/// <param name="mapPosition">Map position to convert to world cooridinates.</param>
	public Vector3 MapToWorld (MapPosition mapPosition)
	{
		return MapToWorld (mapPosition.mapX, mapPosition.mapY);
	}

	/// <summary>
	/// Utility method to convert map coordinates into world coordinate 3D vector
	/// </summary>
	/// <returns>The world coordinate.</returns>
	/// <param name="mapX">Map x.</param>
	/// <param name="mapY">Map y.</param>
	public Vector3 MapToWorld (int mapX, int mapY)
	{
		return  new Vector3 ((mapX + 0.5f) * tileWorldSize, 0.0f, (mapY + 0.5f) * tileWorldSize);
	}

	/// <summary>
	/// Utility method to convert world coordinates into map coordinate
	/// </summary>
	/// <returns>The to map.</returns>
	/// <param name="mapX">Map x.</param>
	/// <param name="mapY">Map y.</param>
	public MapPosition WorldToMap (Vector3  worldCoordinate)
	{
		Vector3 fractionalMap = worldCoordinate / tileWorldSize;

		return  new MapPosition (Mathf.FloorToInt (fractionalMap.x), Mathf.FloorToInt (fractionalMap.z));
			
	}


	/// <summary>
	/// Gets the valid neighbours of position. A Valid neighbour is a location which is within the map bounds and contains a (non-null) tile
	/// </summary>
	/// <returns>The valid neighbours of position.</returns>
	/// <param name="mapX">Map x.</param>
	/// <param name="mapY">Map y.</param>
	public List<MapPosition> GetValidNeighboursOfPosition (int mapX, int mapY)
	{
		List<MapPosition> neighbours = new List<MapPosition> ();

		// Offsets for X and Y. First four are for 4-connected, last four for 8-connected
		int[] dx = { 0,  0, -1, 1, -1,  1, -1, 1 };
		int[] dy = { -1,  1,  0, 0, -1, -1,  1, 1 };

		int upperLimit = isEightConnected ? 7 : 3;
		for (int i = 0; i <= upperLimit; i++) {
			int newX = mapX + dx [i];
			int newY = mapY + dy [i];

			if (IsValidMapLocation (newX, newY)) {
				if (mTiles [newX, newY] != null) {
					neighbours.Add (new MapPosition (newX, newY));
				}
			}
		}
		return neighbours;
	}

	/// <summary>
	/// Gets the valid neighbours of position.
	/// </summary>
	/// <returns>The valid neighbours of position.</returns>
	/// <param name="mapPosition">Map position.</param>
	public List<MapPosition> GetValidNeighboursOfPosition (MapPosition mapPosition)
	{
		return GetValidNeighboursOfPosition (mapPosition.mapX, mapPosition.mapY);
	}

	/// <summary>
	/// Determines whether this instance has salvage near the specified position.
	/// </summary>
	/// <returns><c>true</c> if this instance has salvage near the specified position; otherwise, <c>false</c>.</returns>
	/// <param name="position">Position.</param>
	public bool HasSalvageNear (MapPosition position)
	{
		bool hasSalvage = false;

		List<MapPosition> neighbours = GetValidNeighboursOfPosition (position);

		neighbours.ForEach (n => {
			List<MapObject> objectsAtPosition;
			if (mMapObjectsAtPosition.TryGetValue (n, out objectsAtPosition)) {
				objectsAtPosition.ForEach (o => {
					if (o.gameObject.GetComponent<Salveagable> () != null) {
						hasSalvage = true;
					}
				});
			}
		});


		return hasSalvage;
	}

	private System.Type TileTypeForInt (int i)
	{
		switch (i) {
		case 0:
			return null;
		case 1:
			return typeof(CorridorTile);
		default:
			return typeof(BulkheadTile);
		}
	}

	/// <summary>
	/// Extracts the tile pixels from the tile sprite map
	/// </summary>
	/// <returns>The tile pixels.</returns>
	/// <param name="numTilesWide">Number tiles wide.</param>
	/// <param name="numTilesHigh">Number tiles high.</param>
	private Color[][] ExtractTilePixels (int numTilesWide, int numTilesHigh)
	{
		Color[][] tilePixels = new Color[numTilesHigh * numTilesWide][];

		int tileIndex = 0;
		for (int ty = 0; ty < numTilesHigh; ty++) {
			for (int tx = 0; tx < numTilesWide; tx++) {
				tilePixels [tileIndex++] = terrainTileTextures.GetPixels (tx * tileSizePixels, ty * tileSizePixels, tileSizePixels, tileSizePixels);
			}
		}
		return tilePixels;
	}

			

	/// <summary>
	/// Builds the texture for the map
	/// </summary>
	void BuildTexture ()
	{
		int numTilesHigh = terrainTileTextures.height / tileSizePixels;
		int numTilesWide = terrainTileTextures.width / tileSizePixels;

		Texture2D texture = new Texture2D (tileSizePixels * tileMapWidth, tileSizePixels * tileMapHeight);

		Color[][] tilePixels = ExtractTilePixels (numTilesWide, numTilesHigh);

		for (int y = 0; y < tileMapHeight; y++) {
			for (int x = 0; x < tileMapWidth; x++) {
				int tileIndex = iMap [y, x];
				Color[] pixels = tilePixels [tileIndex];
				texture.SetPixels (x * tileSizePixels, y * tileSizePixels, tileSizePixels, tileSizePixels, pixels);
			}
		}

		texture.filterMode = FilterMode.Point;
		texture.Apply ();

		mMeshRenderer.sharedMaterials [0].mainTexture = texture;
		Debug.Log ("Built Texture!");
	}

	/**
	 * Construct the mesh for this tile map
	 */
	void BuildMesh ()
	{

		// Create mesh data
		int numVertices = (tileMapWidth + 1) * (tileMapHeight + 1);
		Vector3[]	vertices = new Vector3[ numVertices ];

		// One normal per vertex
		Vector3[]	normals = new Vector3[ numVertices ];

		// 3 ints per triangle
		int numTriangles = (tileMapWidth * tileMapHeight * 2);
		int[] triangles = new int[ numTriangles * 3];

		// Create vertices
		int vertexIndex = 0;
		for (int z = 0; z <= tileMapHeight; z++) {
			for (int x = 0; x <= tileMapWidth; x++) {
				vertices [vertexIndex++] = new Vector3 (x * tileWorldSize, 0.0f, z * tileWorldSize);
			}
		}

		// Create Triangles
		int triangleIndex = 0;
		for (int z = 0; z < tileMapHeight; z++) {
			for (int x = 0; x < tileMapWidth; x++) {
				int a = z * (tileMapWidth + 1) + x;
				int b = a + 1;
				int c = a + tileMapWidth + 1;
				int d = c + 1;

				triangles [triangleIndex++] = a;
				triangles [triangleIndex++] = c;
				triangles [triangleIndex++] = d;

				triangles [triangleIndex++] = a;
				triangles [triangleIndex++] = d;
				triangles [triangleIndex++] = b;
			}
		}

		// All normals (one per vertex) point up
		for (int i = 0; i < numVertices; i++) {
			normals [i] = Vector3.up;
		}

		// Create UV mapping
		Vector2[] uvCoords = new Vector2[ numVertices];
		int uvIndex = 0;
		float deltaU = 1.0f / tileMapWidth;
		float deltaV = 1.0f / tileMapHeight;
		for (int z = 0; z <= tileMapHeight; z++) {
			for (int x = 0; x <= tileMapWidth; x++) {

				// Bog Standard
				uvCoords [uvIndex++] = new Vector2 (x * deltaU, z * deltaV);
			}
		}


		// Create Mesh object to hold mesh
		Mesh mesh = new Mesh ();

		// Assign these to mesh
		mesh.vertices = vertices;
		mesh.normals = normals;
		mesh.triangles = triangles;
		mesh.uv = uvCoords;

		// Assign mesh to filter, renderer and collider
		mMeshFilter.mesh = mesh;

		mMeshCollider.sharedMesh = mesh;
	}



	/// <summary>
	/// Build a map of the Aeschylus
	/// </summary>
	private void LoadMap (string shipName)
	{
		Debug.Log ("TileMap:LoadMap() : enter");
		mMeshFilter = GetComponent<MeshFilter> ();
		mMeshRenderer = GetComponent<MeshRenderer> ();
		mMeshCollider = GetComponent<MeshCollider> ();
		if (mMeshFilter != null && mMeshCollider != null && mMeshRenderer != null) {
			// Build the world mesh
			BuildMesh ();

			// Build the texture fo rthe world
			BuildTexture ();

			// Set up the tiles
			mTiles = new Tile[tileMapWidth, tileMapHeight];
			for (int y = 0; y < tileMapHeight; y++) {
				for (int x = 0; x < tileMapWidth; x++) {

					System.Type tileType = TileTypeForInt (iMap [y, x]);
					if (tileType != null) {
						Tile tile = (Tile)System.Activator.CreateInstance (tileType);
						tile.position = new MapPosition (x, y);
						mTiles [x, y] = tile;
					} else {
						mTiles [x, y] = null;
					}
				}
			}


			// Create spawn points
			mPlayerSpawnPoints.Add (new MapPosition (2, 10));
			mPlayerSpawnPoints.Add (new MapPosition (2, 11));
			mPlayerSpawnPoints.Add (new MapPosition (2, 12));


		} else {
			print ("MeshRenderer, MeshCollider or MeshFilter was null");
		}
		Debug.Log ("TileMap:LoadMap() : exit");

	}


	// Aeschylus Tile Data
	private int[,] iMap = {
		{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
		{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 1, 1, 1, 1, 1, 1, 1, 1, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
		{ 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 1, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
		{ 0, 0, 0, 0, 0, 0, 2, 2, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
		{ 0, 0, 0, 2, 2, 2, 2, 1, 2, 2, 2, 2, 2, 1, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
		{ 0, 0, 2, 2, 1, 1, 1, 1, 2, 1, 1, 1, 2, 1, 2, 1, 1, 1, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
		{ 0, 2, 2, 1, 1, 1, 1, 1, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },

		{ 0, 2, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 1, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
		{ 2, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 },
		{ 2, 1, 1, 1, 1, 1, 1, 1, 2, 1, 1, 1, 2, 1, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2 },
		{ 2, 1, 1, 1, 1, 2, 2, 2, 2, 2, 1, 2, 2, 1, 2, 2, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 1, 2, 2, 2, 2, 2, 2, 2, 2, 1, 2 },
		{ 2, 1, 1, 1, 1, 2, 2, 2, 2, 2, 1, 2, 1, 1, 1, 2, 1, 2, 2, 2, 2, 2, 2, 2, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 2, 1, 2 },
		{ 2, 1, 1, 1, 1, 2, 2, 2, 2, 2, 1, 2, 2, 1, 2, 2, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 1, 2, 2, 2, 2, 2, 2, 2, 2, 1, 2 },
		{ 2, 1, 1, 1, 1, 1, 1, 1, 2, 1, 1, 1, 2, 1, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2 },
		{ 2, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 },
		{ 0, 2, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 1, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },

		{ 0, 2, 2, 1, 1, 1, 1, 1, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
		{ 0, 0, 2, 2, 1, 1, 1, 1, 2, 1, 1, 1, 2, 1, 2, 1, 1, 1, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
		{ 0, 0, 0, 2, 2, 2, 2, 1, 2, 2, 2, 2, 2, 1, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
		{ 0, 0, 0, 0, 0, 0, 2, 2, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
		{ 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 1, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
		{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 1, 1, 1, 1, 1, 1, 1, 1, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
		{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
	};

}
