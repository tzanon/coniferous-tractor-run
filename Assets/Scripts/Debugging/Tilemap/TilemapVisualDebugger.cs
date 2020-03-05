﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class TilemapVisualDebugger : MonoBehaviour
{
	/* fields */

	// visual debugging
	[SerializeField] private bool _UIDebugMode = false;

	/*
	[SerializeField] private bool _clearHighlight = true;
	[SerializeField] private Color _nodeHighlight;
	[SerializeField] private Color _neighbourHighlight;
	[SerializeField] private Color _searchHighlight;
	/**/

	[SerializeField] private float _hoverHighlightRefreshRate = 0.15f; // TODO: highlight tile borders on mouse hover

	[SerializeField] private Vector3Int _testCell = new Vector3Int(4, 6, 0);
	[SerializeField] private RectTransform _UIDebugMenu;

	private ChaserControls _controls;
	private InputAction _leftClick;
	private InputAction _mousePosAction;
	private Vector2 _mousePosition;

	//private List<Vector3Int> _highlightedCells;

	private enum VisualDebugType { None, Cell, Neighbours, Path, Closest }
	private VisualDebugType _visualDebugType = VisualDebugType.None;

	private readonly Vector3Int[] _visualPathPoints = new Vector3Int[2];
	private int _visualPathIdx;

	// components
	private TilemapManager _tileManager;
	private NavigationMap _navMap;
	private TilemapHighlighter _highlighter;

	private void Awake()
	{
		// get components
		_tileManager = GetComponent<TilemapManager>();
		_navMap = GetComponent<NavigationMap>();
		_highlighter = GetComponent<TilemapHighlighter>();

		// track all cells currently highlighted
		// TODO: change to a set?
		//_highlightedCells = new List<Vector3Int>();

		// init visual path debugging
		InitPathPoints();

		// init mouse interaction
		_controls = new ChaserControls();
		_leftClick = _controls.Debug.LeftClick;
		_mousePosAction = _controls.Debug.Position;

		_leftClick.performed += HandleMouseClick;
		_mousePosAction.started += ctx => _mousePosition = ctx.ReadValue<Vector2>();
		_mousePosAction.performed += ctx => _mousePosition = ctx.ReadValue<Vector2>();
	}

	private void Start()
	{
		_UIDebugMenu.gameObject.SetActive(_UIDebugMode);
	}

	private void OnEnable()
	{
		_controls.Debug.Enable();
	}

	private void OnDisable()
	{
		_controls.Debug.Disable();
	}

	// TODO: split debug selecting and highlighting into separate classes (Controller and Highlighter)

	#region Visual debug selecting

	//public void ToggleHighlightRefresh() => _clearHighlight = !_clearHighlight;

	private void SetVisualDebugType(VisualDebugType type) => _visualDebugType = type;

	// selectors

	public void NoVisualDebug()
	{
		_highlighter.RemoveHighlight();
		SetVisualDebugType(VisualDebugType.None);
	}

	public void CellVisualDebug() => SetVisualDebugType(VisualDebugType.Cell);
	public void NeighbourVisualDebug() => SetVisualDebugType(VisualDebugType.Neighbours);
	public void ClosestNodeVisualDebug() => SetVisualDebugType(VisualDebugType.Closest);

	public void PathVisualDebug()
	{
		SetVisualDebugType(VisualDebugType.Path);

		// reset path
		InitPathPoints();
	}

	#endregion


	#region Input reading

	/// <summary>
	/// Handle left mouse click for visual debugging
	/// </summary>
	/// <param name="ctx">Unused callback context</param>
	private void HandleMouseClick(InputAction.CallbackContext ctx)
	{
		if (EventSystem.current.IsPointerOverGameObject() || _visualDebugType == VisualDebugType.None)
			return;

		Vector3 screenMousePos = _mousePosition;
		Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(_mousePosition);

		Vector3Int mouseCell = _tileManager.CellOfPosition(worldMousePos);

		MessageLogger.LogHighlightMessage("Screen mouse position is {0}", LogLevel.Debug, screenMousePos);

		switch (_visualDebugType)
		{
			case VisualDebugType.Cell:
				_highlighter.HighlightStandardCell(mouseCell);
				break;
			case VisualDebugType.Neighbours:
				_highlighter.HighlightNodeNeighbours(mouseCell);
				break;
			case VisualDebugType.Path:
				AddVisualPathPoint(mouseCell);
				break;
			case VisualDebugType.Closest:
				_highlighter.HighlightClosestNode(mouseCell);
				break;
			default:
				break;
		}
	}

	private void InitPathPoints()
	{
		_visualPathIdx = 0;
		_visualPathPoints[0] = _visualPathPoints[1] = Vector3Int.zero;
	}

	/**
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
	private void HighlightStandardCell(Vector3Int cell)
	{
		HighlightCell(cell, _nodeHighlight, _clearHighlight);
	}

	/// <summary>
	/// Highlights the given node and its neighbours
	/// </summary>
	/// <param name="node">Node to highlight neighbours of</param>
	private void HighlightNodeNeighbours(Vector3Int node)
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
	private void HighlightClosestNode(Vector3Int cell)
	{
		HighlightStandardCell(_navMap.ClosestNodeToCell(cell));
	}
	/**/

	/// <summary>
	/// Adds node to use for highlighting a path
	/// </summary>
	/// <param name="node">Node to add</param>
	private void AddVisualPathPoint(Vector3Int node)
	{
		if (!_navMap.IsPathfindingNode(node))
		{
			Debug.Log("Cell is not a node");
			return;
		}

		MessageLogger.LogHighlightMessage("Adding node {0} to path", LogLevel.Debug, node);

		_visualPathPoints[_visualPathIdx++] = node;
		_highlighter.HighlightStandardCell(node);
		//_highlighter.HighlightCells(new Vector3Int[] { node }, _nodeHighlight, false);

		if (_visualPathIdx > 1)
		{
			_highlighter.HighlightPath(_visualPathPoints[0], _visualPathPoints[1]);
			InitPathPoints();
		}
	}


	/**
	/// <summary>
	/// Highlights the path between currently chosen nodes
	/// </summary>
	/// <param name="start">Start node of path</param>
	/// <param name="end">End node of path</param>
	private void HighlightPath(Vector3Int start, Vector3Int end)
	{
		//Vector3Int start = ;
		//Vector3Int end = _visualPathPoints[1];

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
	/**/

	#endregion

}
