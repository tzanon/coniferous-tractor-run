using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapManager : ScriptBase
{
	/* fields */
	[SerializeField] private Player _player;

	// components
	private Tilemap _map;
	private NavigationMap _navMap;
	private TilemapVisualDebugger visualDebugger;

	/* properties */
	public Vector3 PlayerPosition { get => _player.transform.position; }
	public Vector3Int PlayerCell { get => CellOfPosition(_player.transform.position); }

	private void Awake()
	{
		// get ref to tilemap and lock tiles in place (?)
		_map = GetComponent<Tilemap>();
		_navMap = GetComponent<NavigationMap>();

		foreach (Vector3Int pos in _map.cellBounds.allPositionsWithin)
		{
			_map.SetTileFlags(pos, (TileFlags.LockTransform));
		}
	}

	#region Tilemap functions

	public Vector3Int CellOfPosition(Vector3 pos)
	{
		return _map.WorldToCell(pos);
	}

	public Vector3 CenterPositionOfCell(Vector3Int cell)
	{
		return _map.CellToWorld(cell) + new Vector3(0.5f, 0.5f, 0f);
	}

	public bool IsCellInBounds(Vector3Int cell)
	{
		return _map.cellBounds.Contains(cell);
	}

	#endregion

	#region Text debugging

	public void PrintCellInfo(Vector3Int cell)
	{
		if (!IsCellInBounds(cell))
		{
			Debug.Log("Cell " + cell + " is not within bounds of this tilemap");
			return;
		}

		if (_navMap.IsPathfindingNode(cell))
		{
			Debug.Log("Cell " + cell + " is in bounds and is a node");
			Debug.Log("Neighbours: " +  _navMap.GetNeighboursOfNode(cell).ToString());
		}
		else
		{
			Debug.Log("Cell " + cell + " is in bounds");
		}
	}

	public void PrintMapInfo()
	{
		BoundsInt mapBounds = _map.cellBounds;
		Debug.Log("bounds and size of tilemap are " + mapBounds);
		Debug.Log("bound min is " + mapBounds.min + ", bound max is " + mapBounds.max);
	}

	public void PrintGraphInfo()
	{
		Debug.Log("Total pathfinding nodes: " + _navMap.PathfindingNodeCount);
	}

	public void PrintPlayerPosition()
	{
		Vector3 playerPos = _player.transform.position;
		Debug.Log("Player at position " + playerPos + " is in cell " + PlayerCell + ", cell position is " + CenterPositionOfCell(PlayerCell));
	}

	public void PrintSpriteAtCell(Vector3Int cell)
	{
		Sprite spr = _map.GetSprite(cell);
		if (spr == null)
			Debug.Log("No sprite at " + cell);
		else
			Debug.Log("sprite at " + cell + " is: " + spr.ToString());
	}

	#endregion
	
}
