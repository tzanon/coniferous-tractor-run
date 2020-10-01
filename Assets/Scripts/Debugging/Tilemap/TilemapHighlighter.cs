using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using UnityEngine.Events;

public class TilemapHighlighter : MonoBehaviour
{
	[SerializeField] private bool _shouldClearHighlight = true;
	[SerializeField] private bool _shouldAnimateHighlight = false;
	[SerializeField] private bool _shouldHoverHighlight = true;

	[SerializeField] private Toggle _refreshToggle, _animationToggle;
	private UnityAction<bool> _readRefreshToggle, _readAnimToggle;

	private bool _isAnimating = false;
	[SerializeField] [Range(0.1f, 1.0f)] private float _animationDelay = 0.2f;

	[SerializeField] private Color _hoverHighlight;
	[SerializeField] private Color _nodeHighlight;
	[SerializeField] private Color _neighbourHighlight;
	[SerializeField] private Color _searchHighlight;

	private readonly HashSet<Vector3Int> _highlightedCells = new HashSet<Vector3Int>();

	// components
	private Tilemap _map;
	private NavigationMap _navMap;

	/* Properties */

	public bool RefreshEnabled { get => _shouldClearHighlight; }
	public bool AnimationEnabled { get => _shouldAnimateHighlight; }

	/* Methods */

	private void Awake()
	{
		_map = GetComponent<Tilemap>();
		_navMap = GetComponent<NavigationMap>();

		InitToggleReaders();
		EnableToggleListeners();
		UpdateToggles();
	}

	private void InitToggleReaders()
	{
		_readRefreshToggle = delegate { ToggleHighlightRefresh(); };
		_readAnimToggle = delegate { ToggleAnimation(); };
	}

	private void EnableToggleListeners()
	{
		_refreshToggle.onValueChanged.AddListener(_readRefreshToggle);
		_animationToggle.onValueChanged.AddListener(_readAnimToggle);
	}

	private void DisableToggleListeners()
	{
		_refreshToggle.onValueChanged.RemoveListener(_readRefreshToggle);
		_animationToggle.onValueChanged.RemoveListener(_readAnimToggle);
	}

	private void UpdateToggles()
	{
		DisableToggleListeners();

		_refreshToggle.isOn = _shouldClearHighlight ? true : false;
		_animationToggle.isOn = _shouldAnimateHighlight ? true : false;

		EnableToggleListeners();
	}

	public void ToggleHighlightRefresh() => _shouldClearHighlight = !_shouldClearHighlight;

	public void ToggleAnimation()
	{
		_shouldAnimateHighlight = !_shouldAnimateHighlight;
		_animationDelay = !_shouldAnimateHighlight ? 0.0f : 0.2f;
	}

	/// <summary>
	/// Highlights all pathfinding node tiles
	/// </summary>
	public void HighlightAllNodes()
	{
		HighlightCells(_navMap.PathfindingNodes, _nodeHighlight, _shouldClearHighlight);
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
	/// Highlight a tile the cursor hovers over
	/// </summary>
	/// <param name="cell">Position of tile to highlight</param>
	public void HighlightHover(Vector3Int cell)
	{
		if (_shouldHoverHighlight && _map.HasTile(cell) && !_highlightedCells.Contains(cell))
		{
			_map.SetColor(cell, _hoverHighlight);
		}
	}

	/// <summary>
	/// Clear any hover highlighting on the cell
	/// </summary>
	/// <param name="cell">Position of tile to clear</param>
	public void ClearHover(Vector3Int cell)
	{
		if (_shouldHoverHighlight && _map.HasTile(cell) && !_highlightedCells.Contains(cell))
		{
			_map.SetColor(cell, Color.white);
		}
	}

	/// <summary>
	/// Highlight a tile with default tint
	/// </summary>
	/// <param name="cell">Position of tile to highlight</param>
	public void HighlightStandardCell(Vector3Int cell)
	{
		HighlightCell(cell, _nodeHighlight, _shouldClearHighlight);
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
			MessageLogger.LogDebugMessage(LogType.Highlight, "Cell {0} is not a node", node);
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
		Queue<Vector3Int> examinedCells;

		Vector3Int closestNode = _navMap.ClosestNodeToCell(cell, out examinedCells);

		if (_shouldAnimateHighlight)
		{
			ColoredTile[] tileColors = new ColoredTile[examinedCells.Count + 1];

			int i = 0;
			while (examinedCells.Count > 0)
			{
				tileColors[i++] = new ColoredTile(examinedCells.Dequeue(), _searchHighlight);
			}

			tileColors[tileColors.Length - 1] = new ColoredTile(closestNode, _nodeHighlight);

			StartCoroutine(AnimateCellHighlight(tileColors, _shouldClearHighlight));

			return;
		}

		if (_shouldClearHighlight)
			RemoveHighlight();

		HighlightCells(examinedCells.ToArray(), _searchHighlight, false);
		HighlightCell(closestNode, _nodeHighlight, false);
	}

	/// <summary>
	/// Highlights the path between currently chosen nodes
	/// </summary>
	/// <param name="start">Start node of path</param>
	/// <param name="end">End node of path</param>
	public void HighlightPath(Vector3Int start, Vector3Int end)
	{
		MessageLogger.LogVerboseMessage(LogType.Highlight, "Highlighting path between {0} and {1}...", start, end);

		if (!(_navMap.IsPathfindingNode(start) && _navMap.IsPathfindingNode(end)))
		{
			MessageLogger.LogErrorMessage(LogType.Highlight, "one or both of cells {0} and {1} are not nodes", start, end);
			return;
		}

		Vector3Int[] path;

		// TODO: animate option
		if (_shouldAnimateHighlight)
		{
			ColoredTile[] tileColors;
			Vector3Int[] nonPathCells;
			// TODO: make struct for search highlight info...
			path = _navMap.FindPathBetweenNodes(start, end, out tileColors, out nonPathCells);
			StartCoroutine(AnimateCellHighlight(tileColors, _shouldClearHighlight, nonPathCells));
		}
		else
		{
			path = _navMap.FindPathBetweenNodes(start, end);
			HighlightCells(path, _nodeHighlight, _shouldClearHighlight);
		}

		MessageLogger.LogVerboseMessage(LogType.Highlight, "path calculated successfully!");
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
	private void HighlightCells(Vector3Int[] cells, Color col, bool removeExistingHighlight = true)
	{
		if (removeExistingHighlight)
			RemoveHighlight();

		foreach (Vector3Int cell in cells)
		{
			HighlightCell(cell, col, false);
		}
	}

	/// <summary>
	/// Animate the highlighting by having delays between each tile
	/// </summary>
	/// <param name="coloredCells">List of queues of cells with their corresponding colours</param>
	/// <param name="col">Colour to highlight tiles with</param>
	/// <param name="removeExistingHighlight">Whether to reset currently highlighted cells</param>
	private IEnumerator AnimateCellHighlight(ColoredTile[] tileColors, bool removeExistingHighlight = true, Vector3Int[] nonPathTiles = null)
	{
		if (_isAnimating)
		{
			MessageLogger.LogErrorMessage(LogType.Highlight, "Error: cannot start a new animation until the current one is complete");
			yield break;
		}

		_isAnimating = true;

		if (removeExistingHighlight)
			RemoveHighlight();

		foreach (ColoredTile coloredTile in tileColors)
		{
			HighlightCell(coloredTile.Tile, coloredTile.Color, false);
			yield return new WaitForSeconds(_animationDelay);
		}

		// clear remaining cells not in the path
		if (nonPathTiles != null)
		{
			HighlightCells(nonPathTiles, Color.white, false);
		}

		_isAnimating = false;
	}

}
