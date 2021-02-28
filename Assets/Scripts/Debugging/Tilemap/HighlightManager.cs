using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using UnityEngine.Events;

/// <summary>
/// Calls paint and animate methods for highlighting
/// </summary>
public class HighlightManager : MonoBehaviour
{
	/* variables */

	[SerializeField] private Toggle _refreshToggle, _animationToggle, _hoverToggle;
	private UnityAction<bool> _readRefreshToggle, _readAnimToggle, _readHoverToggle;

	private AnimatedTilePainter _painter;

	[SerializeField] private SpriteRenderer _hoverSprite;

	// components
	private Tilemap _map;
	private TilemapManager _tileManager;
	private NavigationMap _navMap;

	/* properties */

	public bool RefreshEnabled { get => _painter.RefreshEnabled; set => _painter.RefreshEnabled = value; }
	public bool AnimationEnabled { get; private set; }

	/// <summary>
	/// Tile currently being hovered over
	/// </summary>
	public Vector3Int HoveredTile
	{
		get => _tileManager.CellOfPosition(_hoverSprite.transform.position);
		set
		{
			if (_hoverSprite.enabled)
			{
				_hoverSprite.transform.position = _tileManager.CenterPositionOfCell(value);
			}
		}
	}

	/* methods */

	void Awake()
	{
		_map = GetComponent<Tilemap>();
		_navMap = GetComponent<NavigationMap>();
		_tileManager = GetComponent<TilemapManager>();

		_painter = CreateAnimatedPainter();

		RefreshEnabled = true;
		AnimationEnabled = false;
		_hoverSprite.enabled = false;

		InitToggleReaders();
		EnableToggleListeners();
		UpdateToggles();
	}

	// UI control methods

	/// <summary>
	/// Set up the toggle action wrappers
	/// </summary>
	private void InitToggleReaders()
	{
		_readRefreshToggle = delegate { ToggleHighlightRefresh(); };
		_readAnimToggle = delegate { ToggleAnimation(); };
		_readHoverToggle = delegate { ToggleHoverHighlight(); };
	}

	private void EnableToggleListeners()
	{
		_refreshToggle.onValueChanged.AddListener(_readRefreshToggle);
		_animationToggle.onValueChanged.AddListener(_readAnimToggle);
		_hoverToggle.onValueChanged.AddListener(_readHoverToggle);
	}

	private void DisableToggleListeners()
	{
		_refreshToggle.onValueChanged.RemoveListener(_readRefreshToggle);
		_animationToggle.onValueChanged.RemoveListener(_readAnimToggle);
		_hoverToggle.onValueChanged.RemoveListener(_readHoverToggle);
	}

	/// <summary>
	/// Updates the toggle UI elements based on the script variables
	/// </summary>
	private void UpdateToggles()
	{
		DisableToggleListeners();

		_refreshToggle.isOn = RefreshEnabled;
		_animationToggle.isOn = AnimationEnabled;
		_hoverToggle.isOn = _hoverSprite.enabled;

		EnableToggleListeners();
	}

	public void ToggleHighlightRefresh() => _painter.RefreshEnabled = !RefreshEnabled;

	public void ToggleAnimation()
	{
		AnimationEnabled = !AnimationEnabled;
	}

	public void ToggleHoverHighlight()
	{
		_hoverSprite.enabled = !_hoverSprite.enabled;
	}

	// highlight methods

	public void HighlightAllNodes() => _painter.PaintAllNodes();

	public void RemoveHighlight() => _painter.RemovePaint();

	public void HighlightStandardCell(Vector3Int cell) => _painter.PaintStandardCell(cell);

	public void HighlightNodeNeighbours(Vector3Int node) => _painter.PaintNodeNeighbours(node);

	public void HighlightClosestNode(Vector3Int cell)
	{
		if (AnimationEnabled)
			StartCoroutine(_painter.AnimateClosestNode(cell));
		else
			_painter.PaintClosestNode(cell);
	}

	public void HighlightPath(Vector3Int start, Vector3Int end)
	{
		if (AnimationEnabled)
			StartCoroutine(_painter.AnimatePath(start, end));
		else
			_painter.PaintPath(start, end);
	}

	public void HighlightPath(Pathfinding.Path path) => _painter.PaintPath(path);

	// painter factory methods

	/// <summary>
	/// Instantiates and returns a default colored TilePainter for use by another script
	/// </summary>
	/// <returns>A new TilePainter</returns>
	public TilePainter CreateTilePainter() => new TilePainter(_map, _tileManager, _navMap);

	/// <summary>
	/// Instantiates and returns a custom colored TilePainter for use by another script
	/// </summary>
	/// <param name="col">Color for tile painter</param>
	/// <returns>A new TilePainter</returns>
	public TilePainter CreateTilePainter(Color col) => new TilePainter(_map, _tileManager, _navMap, col);

	/// <summary>
	/// Instantiates and returns an AnimatedTilePainter for use by another script
	/// </summary>
	/// <param name="timeStep">Animation time step for painter to use</param>
	/// <returns>A new AnimatedTilePainter</returns>
	public AnimatedTilePainter CreateAnimatedPainter(float timeStep = 0.5f) => new AnimatedTilePainter(_map, _tileManager, _navMap, timeStep);
}
