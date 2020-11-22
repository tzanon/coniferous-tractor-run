using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAutoMovement : AutoControl
{
	public PlayerAutoMovement(Player player, TilemapManager tm, NavigationMap nm, LevelCompletionChecker lcc) : base(player, tm, nm)
	{
		
	}

	protected override void NoPathAction()
	{
		
	}

	protected override void PathEndAction() {}

	public override void OnEnter()
	{

		base.OnEnter();
	}

	public override void OnExit()
	{
		base.OnExit();
	}
}
