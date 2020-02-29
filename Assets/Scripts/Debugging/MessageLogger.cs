using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Streamlined logger
/// TERMINOLOGY:
/// -Type: What category of code that is being logged (i.e. related to pathfinding logic, tilemap, etc.)
/// -Level: Severity of message e.g. 'debug' for info, 'error' for incorrect output/failure
/// </summary>
public static class MessageLogger
{
	[Flags]
	public enum Level { None = 0, Debug = 1, Verbose = 2, Warning = 4, Error = 8 }
	public enum Type { Tile, Actor, Path, Game }

	private static readonly Dictionary<Type, Level> _modeLevels = new Dictionary<Type, Level>
	{
		{ Type.Tile, Level.None },
		{ Type.Actor, Level.None },
		{ Type.Path, Level.None },
		{ Type.Game, Level.None }
	};

	/* Properties */

	public static Level TileLevels { get => _modeLevels[Type.Tile]; private set => _modeLevels[Type.Tile] = value; }
	public static Level ActorLevels { get => _modeLevels[Type.Actor]; private set => _modeLevels[Type.Actor] = value; }
	public static Level PathLevels { get => _modeLevels[Type.Path]; private set => _modeLevels[Type.Path] = value; }
	public static Level GameLevels { get => _modeLevels[Type.Game]; private set => _modeLevels[Type.Game] = value; }

	/* Methods */

	#region Debug flag enabling

	public static void ClearLevel(Type type) => _modeLevels[type] = Level.None;

	public static void ClearAll()
	{
		List<Type> types = new List<Type>(_modeLevels.Keys);

		foreach (Type type in types)
		{
			ClearLevel(type);
		}
	}

	/// <summary>
	/// Sets the given level flag on the given type
	/// </summary>
	/// <param name="type">Category that is having its level set</param>
	/// <param name="level">Level to be set</param>
	public static void EnableLevel(Type type, Level level) => _modeLevels[type] |= level;

	/// <summary>
	/// Clears the given level flag on the given type
	/// </summary>
	/// <param name="type">Category that is having its level cleared</param>
	/// <param name="level">Level to be cleared</param>
	public static void DisableLevel(Type type, Level level) => _modeLevels[type] &= ~level;

	/// <summary>
	/// Checks if the given type currently has the given level flag set
	/// </summary>
	/// <param name="type">Type being checked</param>
	/// <param name="level">Level flag being checked</param>
	/// <returns>True if the level is set, false if not</returns>
	public static bool TypeHasLevelEnabled(Type type, Level level)
	{
		if (_modeLevels[type] != Level.None && level == Level.None)
			return false;

		return (_modeLevels[type] & level) == level;
	}

	/// <summary>
	/// Checks if the given type currently has the given level flag cleared
	/// </summary>
	/// <param name="type">Type being checked</param>
	/// <param name="level">Level flag being checked</param>
	/// <returns>True if the level is clear, false if not</returns>
	public static bool TypeHasLevelDisabled(Type type, Level level) => !TypeHasLevelEnabled(type, level);

	#endregion

	#region Message logging

	public static void LogTileMessage(string msg, Level level, params object[] args) => LogMessage(msg, Type.Tile, level, args);

	public static void LogActorMessage(string msg, Level level, params object[] args) => LogMessage(msg, Type.Actor, level, args);

	public static void LogPathMessage(string msg, Level level, params object[] args) => LogMessage(msg, Type.Path, level, args);

	private static void LogMessage(string msg, Type type, Level level, params object[] args)
	{
		if (TypeHasLevelDisabled(type, level)) return;

		switch (level)
		{
			case Level.Debug | Level.Verbose:
				Debug.Log(string.Format(msg, args));
				break;
			case Level.Warning:
				Debug.LogWarning(string.Format(msg, args));
				break;
			case Level.Error:
				Debug.LogError(string.Format(msg, args));
				break;
			default:
				Debug.Log(string.Format(msg, args));
				break;
		}
	}

#endregion

}
