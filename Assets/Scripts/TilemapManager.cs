using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapManager : MonoBehaviour
{
	/* fields */
	[SerializeField] private Player _player;

	// components
	private Tilemap _map;
	private NavigationMap _navMap;

	/* properties */
	public Vector3 PlayerPosition { get => _player.transform.position; }
	public Vector3Int PlayerCell { get => CellOfPosition(_player.transform.position); }

	public static Vector3Int UndefinedCell { get => new Vector3Int(0, 0, -1); }

	private void Awake()
	{
		// get ref to tilemap and lock tiles in place (?)
		_map = GetComponent<Tilemap>();
		_navMap = GetComponent<NavigationMap>();

		if (!ReferencesAreValid())
		{
			MessageLogger.LogErrorMessage(LogType.Tile, "Exiting Awake() in {0}", this.name);
			return;
		}

		foreach (Vector3Int pos in _map.cellBounds.allPositionsWithin)
		{
			_map.SetTileFlags(pos, (TileFlags.LockTransform));
		}
	}

	/// <summary>
	/// Checks if all references are defined
	/// </summary>
	/// <param name="refs">References to check</param>
	/// <returns>True if all defined, false if at least one isn't</returns>
	private bool ReferencesAreValid(params Object[] refs)
	{
		bool refsOK = true;

		foreach (Object reference in refs)
		{
			if (reference == null)
			{
				refsOK = false;
				MessageLogger.LogErrorMessage(LogType.Tile, "Error: Reference is null");
			}
		}

		return refsOK;
	}

	#region Tilemap functions

	/// <summary>
	/// Gets the tilemap cell that contains the given position
	/// </summary>
	/// <param name="pos">Position in world space</param>
	/// <returns>Coordinates of corresponding cell</returns>
	public Vector3Int CellOfPosition(Vector3 pos)
	{
		return _map.WorldToCell(pos);
	}

	/// <summary>
	/// Gets the position of the middle of the given cell
	/// </summary>
	/// <param name="cell">Cell to get center of</param>
	/// <returns>World space position at center of cell</returns>
	public Vector3 CenterPositionOfCell(Vector3Int cell)
	{
		return _map.CellToWorld(cell) + new Vector3(0.5f, 0.5f, 0f);
	}

	/// <summary>
	/// Checks if cell is within bounds of tilemap
	/// </summary>
	/// <param name="cell">Cell to check</param>
	/// <returns>True if the cell is in bounds, false if not</returns>
	public bool IsCellInBounds(Vector3Int cell)
	{
		return _map.cellBounds.Contains(cell);
	}

	#endregion

	#region Text debugging

	/// <summary>
	/// Prints info about the given cell
	/// </summary>
	/// <param name="cell">Cell to describe</param>
	public void PrintCellInfo(Vector3Int cell)
	{
		if (!IsCellInBounds(cell))
		{
			MessageLogger.LogDebugMessage(LogType.Graph, "Cell {0} is not within bounds of this tilemap", cell);
			return;
		}

		if (_navMap.IsPathfindingNode(cell))
		{
			string neighbourStr = _navMap.GetNeighboursOfNode(cell).ToString();

			MessageLogger.LogDebugMessage(LogType.Graph, "Cell {0} is in bounds and is a node", cell);
			MessageLogger.LogVerboseMessage(LogType.Graph, "Neighbours: {0}", neighbourStr);
		}
		else
		{
			MessageLogger.LogDebugMessage(LogType.Graph, "Cell {0} is in bounds", cell);
		}
	}

	/// <summary>
	/// Prints boundary limits of the tilemap
	/// </summary>
	public void PrintMapInfo()
	{
		BoundsInt mapBounds = _map.cellBounds;

		MessageLogger.LogDebugMessage(LogType.Tile, "bounds and size of tilemap: {0}", mapBounds);
		MessageLogger.LogVerboseMessage(LogType.Tile, "bound min is {0}, bound max is {1}", mapBounds.min, mapBounds.max);
	}

	/// <summary>
	/// Prints the number of nodes in the pathfinding graph
	/// </summary>
	public void PrintGraphInfo()
	{
		int nodeCount = _navMap.PathfindingNodeCount;
		MessageLogger.LogDebugMessage(LogType.Graph, "Total pathfinding nodes: {0}", nodeCount);
	}

	/// <summary>
	/// Prints the player's position and current tilemap cell
	/// </summary>
	public void PrintPlayerPosition()
	{
		Vector3 cellCenter = CenterPositionOfCell(PlayerCell);

		MessageLogger.LogDebugMessage(LogType.Tile, "Player has position {0}; is in cell {1}, position {2}",
			PlayerPosition, PlayerCell, cellCenter);
	}

	/// <summary>
	/// Prints name of the sprite at the given cell
	/// </summary>
	/// <param name="cell">Cell to check sprite of</param>
	public void PrintSpriteAtCell(Vector3Int cell)
	{
		Sprite spr = _map.GetSprite(cell);
		string msg;
		object[] args;

		if (spr == null)
		{
			msg = "No sprite at {0}";
			args = new object[] { cell };
		}
		else
		{
			msg = "Sprite at {0} is {1}";
			args = new object[] { cell, spr.ToString()};
		}

		MessageLogger.LogDebugMessage(LogType.Tile, msg, args);
	}

	#endregion

}
