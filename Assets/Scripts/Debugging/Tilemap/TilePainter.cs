using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

using Pathfinding;

/// <summary>
/// Provides various methods to highlight given tilemap with
/// </summary>
public class TilePainter
{
	/* vars and properties */

	protected Tilemap _map;
	protected TilemapManager _tileManager;
	protected NavigationMap _navMap;

	protected Color _paintColor, _secondaryColor;

	protected readonly HashSet<Vector3Int> _highlightedCells = new HashSet<Vector3Int>();

	public bool RefreshEnabled { get; set; } = true;

	/* constructors */

	public TilePainter(Tilemap m, TilemapManager tm, NavigationMap nm) => Init(m, tm, nm, Color.red);
	public TilePainter(Tilemap m, TilemapManager tm, NavigationMap nm, Color c) => Init(m, tm, nm, c);

	/* methods */

	private void Init(Tilemap m, TilemapManager tm, NavigationMap nm, Color c)
	{
		_map = m;
		_tileManager = tm;
		_navMap = nm;

		_paintColor = c;
		_secondaryColor = c;
		_secondaryColor.a = 0.5f;
	}

	/// <summary>
	/// Highlights all pathfinding node tiles
	/// </summary>
	public void PaintAllNodes()
	{
		PaintCells(_navMap.PathfindingNodes, _paintColor, RefreshEnabled);
	}

	/// <summary>
	/// Remove any highlighting
	/// </summary>
	public void RemovePaint()
	{
		foreach (Vector3Int cell in _highlightedCells)
			_map.SetColor(cell, Color.white);

		_highlightedCells.Clear();
	}

	/// <summary>
	/// Highlight a tile with default tint
	/// </summary>
	/// <param name="cell">Position of tile to highlight</param>
	public void PaintStandardCell(Vector3Int cell) => PaintCell(cell, _paintColor, RefreshEnabled);

	/// <summary>
	/// Highlights the given node and its neighbours
	/// </summary>
	/// <param name="node">Node to highlight neighbours of</param>
	public void PaintNodeNeighbours(Vector3Int node)
	{
		PaintStandardCell(node);

		if (!_navMap.IsPathfindingNode(node))
		{
			MessageLogger.LogDebugMessage(LogType.Highlight, "Cell {0} is not a node", node);
			return;
		}

		Vector3Int[] neighbours = _navMap.GetNeighboursOfNode(node);
		PaintCells(neighbours, _secondaryColor, false);
	}

	/// <summary>
	/// Highlights closest node of given cell
	/// </summary>
	/// <param name="cell">Cell to search from</param>
	public void PaintClosestNode(Vector3Int cell)
	{
		var closestNode = _navMap.ClosestNodeToCell(cell, out var examinedCells);

		if (RefreshEnabled)
			RemovePaint();

		PaintCells(examinedCells.ToArray(), _secondaryColor, false);
		PaintCell(closestNode, _paintColor, false);
	}

	/// <summary>
	/// Highlights the path between currently chosen nodes
	/// </summary>
	/// <param name="start">Start node of path</param>
	/// <param name="end">End node of path</param>
	public void PaintPath(Vector3Int start, Vector3Int end)
	{
		MessageLogger.LogVerboseMessage(LogType.Highlight, "Highlighting path between {0} and {1}...", start, end);
		var path = _navMap.FindPathBetweenNodes(start, end);
		PaintPath(path);
	}

	/// <summary>
	/// Highlight nodes of an existing path
	/// </summary>
	/// <param name="path">Path to highlight</param>
	public void PaintPath(Path path) => PaintCells(path.Points, _paintColor, RefreshEnabled);

	/// <summary>
	/// Highlight a single tile with the given colour
	/// </summary>
	/// <param name="cell">Coordinates of tile to highlight</param>
	/// <param name="col">Colour to highlight tile with</param>
	/// <param name="removeExistingHighlight">Whether to reset currently highlighted cells</param>
	protected void PaintCell(Vector3Int cell, Color col, bool removeExistingHighlight = true)
	{
		if (removeExistingHighlight)
			RemovePaint();

		if (!_map.HasTile(cell))
		{
			MessageLogger.LogDebugMessage(LogType.Highlight, "No tile here");
			return;
		}

		_map.SetColor(cell, col);
		_highlightedCells.Add(cell);
	}

	/// <summary>
	/// Highlight a set of tiles with the given colour
	/// </summary>
	/// <param name="cells">Cells to highlight</param>
	/// <param name="col">Colour to highlight tiles with</param>
	/// <param name="removeExistingHighlight">Whether to reset currently highlighted cells</param>
	protected void PaintCells(Vector3Int[] cells, Color col, bool removeExistingHighlight = true)
	{
		if (removeExistingHighlight)
			RemovePaint();

		foreach (Vector3Int cell in cells)
		{
			PaintCell(cell, col, false);
		}
	}
}
