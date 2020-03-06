using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class TilemapVisualDebugger : MonoBehaviour
{
	/* fields */

	// visual debugging
	[SerializeField] private bool _UIDebugMode = false;

	// TODO: highlight tile borders on mouse hover
	[SerializeField] private float _hoverHighlightRefreshRate = 0.15f;

	[SerializeField] private RectTransform _UIDebugMenu;

	private ChaserControls _controls;
	private InputAction _leftClick;
	private InputAction _mousePosAction;
	private Vector2 _mousePosition;

	private enum VisualDebugType { None, Cell, Neighbours, Path, Closest }
	private VisualDebugType _visualDebugType = VisualDebugType.None;

	private readonly Vector3Int[] _visualPathPoints = new Vector3Int[2];
	private int _visualPathIdx;

	// components
	private TilemapManager _tileManager;
	private NavigationMap _navMap;
	private TilemapHighlighter _highlighter;

	/* Unity methods */

	private void Awake()
	{
		ObtainComponents();
		SetupMouseInput();
		// init visual path debugging
		InitPathPoints();
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

	/* Setup methods */

	private void InitPathPoints()
	{
		_visualPathIdx = 0;
		_visualPathPoints[0] = _visualPathPoints[1] = Vector3Int.zero;
	}

	private void ObtainComponents()
	{
		_tileManager = GetComponent<TilemapManager>();
		_navMap = GetComponent<NavigationMap>();
		_highlighter = GetComponent<TilemapHighlighter>();
	}

	private void SetupMouseInput()
	{
		_controls = new ChaserControls();
		_leftClick = _controls.Debug.LeftClick;
		_mousePosAction = _controls.Debug.Position;

		_leftClick.performed += HandleMouseClick;
		_mousePosAction.started += ctx => _mousePosition = ctx.ReadValue<Vector2>();
		_mousePosAction.performed += ctx => _mousePosition = ctx.ReadValue<Vector2>();
	}

	/* Selectors */

	private void SetVisualDebugType(VisualDebugType type) => _visualDebugType = type;

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

		if (_visualPathIdx > 1)
		{
			_highlighter.HighlightPath(_visualPathPoints[0], _visualPathPoints[1]);
			InitPathPoints();
		}
	}

}
