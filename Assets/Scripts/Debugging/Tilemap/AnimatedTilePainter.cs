using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// TilePainter with animation coroutines to show algorithm process
/// </summary>
public class AnimatedTilePainter : TilePainter
{
	private WaitForSecondsRealtime _delayInterval;

	public bool CurrentlyAnimating { get; private set; }

	public AnimatedTilePainter(Tilemap m, TilemapManager tm, NavigationMap nm, float ad = 0.5f) : base(m, tm, nm) => InitAnim(ad);

	public AnimatedTilePainter(Tilemap m, TilemapManager tm, NavigationMap nm, Color c, float ad = 0.5f) : base(m, tm, nm, c) => InitAnim(ad);

	private void InitAnim(float ad)
	{
		_delayInterval = new WaitForSecondsRealtime(ad);
	}

	// necessary?
	public IEnumerator AnimateNodeNeighbours(Vector3Int node)
	{
		if (CurrentlyAnimating)
		{
			MessageLogger.LogErrorMessage(LogType.Highlight, "Error: cannot start a new animation until the current one is complete");
			yield break;
		}

		yield break;
	}

	/// <summary>
	/// Animates algorithm for finding closest node to given cell
	/// </summary>
	/// <param name="cell">Cell to search from</param>
	public IEnumerator AnimateClosestNode(Vector3Int cell)
	{
		if (CurrentlyAnimating)
		{
			MessageLogger.LogErrorMessage(LogType.Highlight, "Error: cannot start a new animation until the current one is complete");
			yield break;
		}

		// run algorithm and send evaluated tiles to animate methods

		var closestNode = _navMap.ClosestNodeToCell(cell, out var examinedCells);
		var tileColors = new ColoredTile[examinedCells.Count + 1];

		int i = 0;
		while (examinedCells.Count > 0)
		{
			tileColors[i++] = new ColoredTile(examinedCells.Dequeue(), _secondaryColor);
		}

		tileColors[tileColors.Length - 1] = new ColoredTile(closestNode, _paintColor);

		yield return AnimateHighlight(tileColors, RefreshEnabled);
	}

	/// <summary>
	/// Highlights the path between currently chosen nodes
	/// </summary>
	/// <param name="start">Start node of path</param>
	/// <param name="end">End node of path</param>
	public IEnumerator AnimatePath(Vector3Int start, Vector3Int end)
	{
		if (CurrentlyAnimating)
		{
			MessageLogger.LogErrorMessage(LogType.Highlight, "Error: cannot start a new animation until the current one is complete");
			yield break;
		}

		// run algorithm and send evaluated tiles to animate methods
		_navMap.FindPathBetweenNodes(start, end, out var tileColors, out var nonPathCells);
		yield return AnimateHighlight(tileColors, RefreshEnabled, nonPathCells);
	}

	/// <summary>
	/// Animate the highlighting by having delays between each tile
	/// </summary>
	/// <param name="tileColors">Tiles to highlight with associated color</param>
	/// <param name="removeExistingHighlight">Whether to reset currently highlighted cells</param>
	/// <param name="extraSearchTiles">Tiles used in algorithm but not relevant to final result</param>
	/// <returns>Animation delay</returns>
	private IEnumerator AnimateHighlight(ColoredTile[] tileColors, bool removeExistingHighlight = true, Vector3Int[] extraSearchTiles = null)
	{
		CurrentlyAnimating = true;

		if (removeExistingHighlight)
			RemoveHighlight();

		foreach (ColoredTile coloredTile in tileColors)
		{
			HighlightCell(coloredTile.Tile, coloredTile.Color, false);
			yield return _delayInterval;
		}

		// clear remaining cells not in the path
		if (extraSearchTiles != null)
			HighlightCells(extraSearchTiles, Color.white, false);

		CurrentlyAnimating = false;
	}
}
