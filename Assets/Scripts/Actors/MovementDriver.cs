using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Directions;

public class MovementDriver : MonoBehaviour
{
	[SerializeField] private TilemapManager _tilemapManager;
	[SerializeField] private NavigationMap _navMap;

	private bool _movingPlayer;

	private void FixedUpdate()
	{
		


	}

	// TODO: refactor and get rid of this, maybe even whole class
	// time for state machines
	public void MovePlayerToNode(Player player, Vector3Int dest)
	{
		player.InputBlocked = true;

		Vector3Int playerCell = _tilemapManager.CellOfPosition(player.Position);
		Vector3Int closestNodeToPlayer = _navMap.ClosestNodeToCell(playerCell);

		MovementVector direction = _navMap.DirectionToNode(closestNodeToPlayer, dest);
		player.Position = _tilemapManager.CenterPositionOfCell(closestNodeToPlayer);
		//player.Position = _tilemapManager.CenterPositionOfCell(dest);

		player.SetMovementDirection(direction);

		player.InputBlocked = false;
	}

	public void MoveActorToNode(Actor actor, Vector3Int dest)
	{

	}

}
