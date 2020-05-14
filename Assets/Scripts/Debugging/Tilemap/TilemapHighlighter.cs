using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using UnityEngine.Events;

public class TilemapHighlighter : MonoBehaviour
{
	private struct ColoredCells
	{
		public Vector3Int[] Cells { get; private set; }

		public Color Color { get; }

		public ColoredCells(Vector3Int[] cellList, Color col)
		{
			Cells = cellList;
			Color = col;
		}
	}

	[SerializeField] private bool _clearHighlight = true;
	[SerializeField] private bool _animateHighlight = false;

	[SerializeField] private Toggle _refreshToggle, _animationToggle;
	private UnityAction<bool> _readRefreshToggle, _readAnimToggle;

	private bool _isAnimating = false;
	[SerializeField] [Range(0.1f, 1.0f)] private float _animationDelay = 0.2f;

	[SerializeField] private Color _nodeHighlight;
	[SerializeField] private Color _neighbourHighlight;
	[SerializeField] private Color _searchHighlight;

	private readonly HashSet<Vector3Int> _highlightedCells = new HashSet<Vector3Int>();

	// components
	private Tilemap _map;
	private NavigationMap _navMap;

	/* Properties */

	public bool RefreshEnabled { get => _clearHighlight; }
	public bool AnimationEnabled { get => _animateHighlight; }

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

		_refreshToggle.isOn = _clearHighlight ? true : false;
		_animationToggle.isOn = _animateHighlight ? true : false;

		EnableToggleListeners();
	}

	public void ToggleHighlightRefresh() => _clearHighlight = !_clearHighlight;

	public void ToggleAnimation()
	{
		_animateHighlight = !_animateHighlight;
		_animationDelay = !_animateHighlight ? 0.0f : 0.2f;
	}

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

		if (_animateHighlight)
		{
			ColoredCells[] coloredCells =
			{
				new ColoredCells(examinedCells.ToArray(), _searchHighlight),
				new ColoredCells(new Vector3Int[] { closestNode }, _nodeHighlight)
			};

			StartCoroutine(HighlightCellsWithDelay(coloredCells, _clearHighlight));

			return;
		}

		if (_clearHighlight)
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
		MessageLogger.LogDebugMessage(LogType.Highlight, "Highlighting path between {0} and {1}...", start, end);

		if (!(_navMap.IsPathfindingNode(start) && _navMap.IsPathfindingNode(end)))
		{
			MessageLogger.LogErrorMessage(LogType.Highlight, "one or both of cells {0} and {1} are not nodes", start, end);
			return;
		}

		Vector3Int[] path = _navMap.FindPathBetweenNodes(start, end);
		HighlightCells(path, _nodeHighlight, _clearHighlight);

		MessageLogger.LogDebugMessage(LogType.Highlight, "path calculated successfully!");
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
	private IEnumerator HighlightCellsWithDelay(ColoredCells[] totalCells, bool removeExistingHighlight = true)
	{
		if (_isAnimating)
		{
			MessageLogger.LogErrorMessage(LogType.Highlight, "Error: cannot start a new animation until the current one is complete");
			yield break;
		}

		_isAnimating = true;

		if (removeExistingHighlight)
			RemoveHighlight();

		foreach(ColoredCells coloredCells in totalCells)
		{
			Vector3Int[] cells = coloredCells.Cells;
			Color currentColor = coloredCells.Color;

			foreach (Vector3Int cell in cells)
			{
				HighlightCell(cell, currentColor, false);
				yield return new WaitForSeconds(_animationDelay);
			}
		}

		_isAnimating = false;
	}

}
