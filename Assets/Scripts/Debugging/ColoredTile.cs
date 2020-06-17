using UnityEngine;

public struct ColoredTile
{
	public Vector3Int Tile { get; set; }
	public Color Color { get; set; }

	public ColoredTile(Vector3Int tile, Color col)
	{
		Tile = tile;
		Color = col;
	}
}
