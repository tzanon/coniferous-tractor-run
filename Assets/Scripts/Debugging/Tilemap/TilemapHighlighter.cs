using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapHighlighter : MonoBehaviour
{

	[SerializeField] private bool _clearHighlight = true;
	[SerializeField] private Color _nodeHighlight;
	[SerializeField] private Color _neighbourHighlight;
	[SerializeField] private Color _searchHighlight;

	private readonly HashSet<Vector3Int> _highlightedCells = new HashSet<Vector3Int>();

	// components
	private Tilemap _map;
	private NavigationMap _navMap;

	private void Awake()
	{
		_map = GetComponent<Tilemap>();
		_navMap = GetComponent<NavigationMap>();
	}

	public void ToggleHighlightRefresh() => _clearHighlight = !_clearHighlight;

	/// <summary>
	/// Highlights all pathfinding node tiles
	/// </summary>
	public void HighlightAllNodes()
	{
		HighlightCells(_navMap.PathfindingNodes, _nodeHighlight, _clearHighlight);
	}

	/// <summary>
	/// Remove any highlighting
	/// </summary>
	public void RemoveHighlight()
	{
		foreach (Vector3Int cell in _highlightedCells)
		{
			_map.SetColor(cell, Color.white);
		}

		_highlightedCells.Clear();
	}

	/// <summary>
	/// Highlight a tile with default tint
	/// </summary>
	/// <param name="cell">Position of tile to highlight</param>
	public void HighlightStandardCell(Vector3Int cell)
	{
		HighlightCell(cell, _nodeHighlight, _clearHighlight);
	}

	/// <summary>
	/// Highlights the given node and its neighbours
	/// </summary>
	/// <param name="node">Node to highlight neighbours of</param>
	public void HighlightNodeNeighbours(Vector3Int node)
	{
		HighlightStandardCell(node);

		if (!_navMap.IsPathfindingNode(node))
		{
			MessageLogger.LogHighlightMessage("Cell {0} is not a node", LogLevel.Debug, node);
			return;
		}

		Vector3Int[] neighbours = _navMap.GetNeighboursOfNode(node);
		HighlightCells(neighbours, _neighbourHighlight, false);
	}

	/// <summary>
	/// Highlights closest node of given cell
	/// </summary>
	/// <param name="cell">Cell to search from</param>
	public void HighlightClosestNode(Vector3Int cell)
	{
		HighlightStandardCell(_navMap.ClosestNodeToCell(cell));
	}

	/// <summary>
	/// Highlights the path between currently chosen nodes
	/// </summary>
	/// <param name="start">Start node of path</param>
	/// <param name="end">End node of path</param>
	public void HighlightPath(Vector3Int start, Vector3Int end)
	{
		MessageLogger.LogHighlightMessage("Highlighting path between {0} and {1}...", LogLevel.Debug, start, end);

		if (!(_navMap.IsPathfindingNode(start) && _navMap.IsPathfindingNode(end)))
		{
			MessageLogger.LogHighlightMessage("one or both of cells {0} and {1} are not nodes", LogLevel.Error, start, end);
			return;
		}

		Vector3Int[] path = _navMap.FindPathBetweenNodes(start, end);
		HighlightCells(path, _nodeHighlight, _clearHighlight);

		MessageLogger.LogHighlightMessage("path calculated successfully!", LogLevel.Debug);
	}


	/// <summary>
	/// Highlight a single tile with the given colour
	/// </summary>
	/// <param name="cell">Coordinates of tile to highlight</param>
	/// <param name="col">Colour to highlight tile with</param>
	/// <param name="removeExistingHighlight">Whether to reset currently highlighted cells</param>
	private void HighlightCell(Vector3Int cell, Color col, bool removeExistingHighlight = true)
	{
		if (removeExistingHighlight)
			RemoveHighlight();

		if (!_map.HasTile(cell))
		{
			MessageLogger.LogHighlightMessage("No tile here", LogLevel.Debug);
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
	private void HighlightCells(Vector3Int[] cells, Color col, bool removeExistingHighlight = true)
	{
		if (removeExistingHighlight)
			RemoveHighlight();

		foreach (Vector3Int cell in cells)
		{
			HighlightCell(cell, col, false);
		}
	}


}
