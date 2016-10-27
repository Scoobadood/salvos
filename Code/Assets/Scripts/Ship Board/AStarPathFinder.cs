using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AStarPathFinder
{
	/** The Map */
	private TileMap mMap;

	private static float DIAGONAL_COST = 1.0f;
	private static float ORTHOGNAL_COST = 1.4f;

	/**
	 * Construct with a map
	 * @param map The map to work on
	 */
	public AStarPathFinder (TileMap map)
	{
		mMap = map;
	}

	/**
	 * Find a path from point to point on the map using A Star
	 * @param fromPosition The from position
	 * @param toPosition The to position
	 * @param options Options which may affect the path finding. Ignored for now but may later include options including attributes enabling the mover to mitogate some cost of movement
	 * @return The path from start to finish or null of no path can be found
	 */
	public List<MapPosition> FindPath (MapPosition fromPosition, MapPosition toPosition, Dictionary<string,string> options)
	{
		return FindPath (fromPosition.mapX, fromPosition.mapY, toPosition.mapX, toPosition.mapY, options);
	}

	/**
	 * Find a path from point to point on the map using A Star
	 * @param fromX The form X Coordinate
	 * @param fromY The from Y Coordinate
	 * @param toX The to X coordinate
	 * @param toY The to Y coordinate
	 * @param options Options which may affect the path finding. Ignored for now but may later include options including attributes enabling the mover to mitogate some cost of movement. The first option should be APs the number of action points available. This will caus ethe path to fail if no route can be found at the appropriate cost
	 * Should also abandon cost when APs is exceeded or would be.
	 * 
	 * @return The path from start to finish or null of no path can be found
	 */
	public List<MapPosition> FindPath (int fromX, int fromY, int toX, int toY, Dictionary<string,string> options)
	{
		List<MapPosition> path = null;

		// Extract APs from options
		int actionPointsRemaining = 0;
		if (options != null) {
			string stringValue = null;
			if (options.TryGetValue ("APs", out stringValue)) {
				actionPointsRemaining = int.Parse (stringValue);
			}
		}

		// Construct open an closed node set
		List<AStarNode> openNodes = new List<AStarNode> ();
		List<AStarNode> closedNodes = new List<AStarNode> ();

		// Place the from node into the open node set
		int cost = EstimateHeuristic (fromX, fromY, toX, toY);
		AddNodeToOpenList (openNodes, new AStarNode (fromX, fromY, 0, cost, null));

		// Repeat until no nodes left
		while (openNodes.Count > 0) {
			
			// Pull lowest cost node from open nodes; totalCost first then estimated cost
			AStarNode currentNode = GetBestNode (openNodes);

			// Add this one to the closed list
			closedNodes.Add (currentNode);

			// So long as this isn't the goal...
			if (currentNode.nodeX != toX || currentNode.nodeY != toY) {

				// Get the neighbours
				List<AStarNode> neighbours = GetValidNeighboursOfNode (currentNode, mMap.isEightConnected, actionPointsRemaining);

				// For each neighbour
				foreach (AStarNode neighbour in neighbours) {

					// If it is not walkable or if it is on the closed list, ignore it. Or it would cost too much Otherwise do the following
					if ((closedNodes.Find (foundNode => ((foundNode.nodeX == neighbour.nodeX) && (foundNode.nodeY == neighbour.nodeY))) == null)) {

						// Update the H score for the node
						neighbour.h = EstimateHeuristic (fromX, fromY, toX, toY);

						// If it isn’t on the open list, add it to the open list. 
						AStarNode foundOpenNode = openNodes.Find (foundNode => ((foundNode.nodeX == neighbour.nodeX) && (foundNode.nodeY == neighbour.nodeY)));
						if (foundOpenNode == null) {
							openNodes.Add (neighbour);

						} else {
							// Otherwise, This node is in the open list but our version may be better...
							if (foundOpenNode.g > neighbour.g) {
								// Replace it
								openNodes.Remove (foundOpenNode);
								openNodes.Add (neighbour);
							}
						}
					}
				}
			} else {
				// Reached destination. Construct the path
				path = new List<MapPosition> ();
				do {
					path.Add (new MapPosition (currentNode.nodeX, currentNode.nodeY));
					currentNode = currentNode.previousNode;
				} while(currentNode != null);
				path.Reverse();
			}
		}

		// Dump path
		return path;
	}


	/**
	 * @param eightConnected If truem consider diagnal neighbours, otherwise just NSEW
	 * @return the neighbours of the given node
	 */
	private List<AStarNode> GetValidNeighboursOfNode (AStarNode node, bool eightConnected, int actionPointsRemaining)
	{
		List<AStarNode> neighbours = new List<AStarNode> ();


		List<MapPosition> mapNeighbours = mMap.GetValidNeighboursOfPosition (node.nodeX, node.nodeY);

		mapNeighbours.ForEach (neighbour => {
			Tile tile = mMap.TileAt (neighbour.mapX, neighbour.mapY);

			int tileCost = tile.GetCost();
			if(  ( neighbour.mapX - node.nodeX == 0 ) || ( neighbour.mapY - node.nodeY == 0 ) ) {
				tileCost = (int) ( tileCost * ORTHOGNAL_COST );
			} else {
				tileCost = (int) (tileCost * DIAGONAL_COST);
			}

			if ( tile.IsWalkable () && // Must be walkable
				(actionPointsRemaining != 0) &&
				(node.g + tileCost <= actionPointsRemaining)) {
				// H Cost is estimated on return
				neighbours.Add (new AStarNode (neighbour.mapX, neighbour.mapY, node.g + tileCost, 0, node));
			}
		});

		return neighbours;
	}

	/**
	 * Add a node to the list but only iff:
	 * it isn't already in the open node list OR
	 * if it is, this version has a lower cost
	 */
	private void AddNodeToOpenList (List<AStarNode> openNodes, AStarNode node)
	{
		// Find any matching node where matching means same X,Y
		AStarNode existingNode = openNodes.Find (found => ((found.nodeX == node.nodeX) && (found.nodeY == node.nodeY)));

		// Add this node if there's not one or if this is better
		if (existingNode == null) {
			openNodes.Add (node);
		} else {
			if (existingNode.FScore () < node.FScore ()) {
				openNodes.Remove (existingNode);
				openNodes.Add (node);
			}
		}
	}

	/**
	 * Find the best node in the list based on the total of fCost and gCost
	 * Use gCost as the tie breaker. Remove it from the list and return it
	 * @param openNodes The list of nodes to search
	 * @return An AStarNode being the best node
	 */
	private AStarNode GetBestNode (List<AStarNode> openNodes)
	{
		int bestNodeIndex = 0;
		int bestCost = openNodes [0].FScore ();
		for (int i = 1; i < openNodes.Count; i++) {
			if ((openNodes [i].FScore () < bestCost) ||
			    ((openNodes [i].FScore () == bestCost) && (openNodes [i].g < openNodes [bestNodeIndex].g))) {
				bestNodeIndex = i;
				bestCost = openNodes [i].FScore ();
			} 
		}
		AStarNode bestNode = openNodes [bestNodeIndex];
		openNodes.RemoveAt (bestNodeIndex);
		return bestNode;
	}

	/**
	 * Estimation heuristic from position to position
	 * @return The estimate
	 */
	private int EstimateHeuristic (int fromX, int fromY, int toX, int toY)
	{
		int h = (Mathf.Abs (fromX - toX) + Mathf.Abs (fromY - toY)) * 10;
		return h;
	}


	/* ********************************************************************************
	 * *
	 * *  Private inner class
	 * *
	 * ******************************************************************************* */
	private class AStarNode
	{
		//Location
		public int	nodeX;
		public int	nodeY;

		public int	h;
		// heuristic cost to target
		public int	g;
		// cost from start

		public AStarNode previousNode;

		/**
		 * Make me one
		 * @param x The x coordinate
		 * @param y The y coordinate
		 * @pram fromCost the cost to get here
		 * @param toCost The cost to get to goal
		 * @param previousNode The node from which I'm coming
		 */
		public AStarNode (int x, int y, int costToDate, int estimatedCostToGoal, AStarNode previousNode)
		{
			nodeX = x;
			nodeY = y;

			g = costToDate;
			h = estimatedCostToGoal;

			this.previousNode = previousNode;
		}

		public int FScore ()
		{
			return g + h;
		}
	};
}



