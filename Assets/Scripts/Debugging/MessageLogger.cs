using System;
using System.Collections.Generic;
using UnityEngine;

[Flags]
public enum LogLevel { None = 0, Debug = 1, Verbose = 2, Warning = 4, Error = 8 }
public enum LogType { Tile, Highlight, Graph, Path, FSM, Actor, Game, GUI }

/// <summary>
/// Streamlined console logger
/// TERMINOLOGY:
/// -Type: What category of code that is being logged (i.e. related to pathfinding logic, tilemap, etc.)
/// -Level: Severity of message e.g. 'debug' for info, 'error' for incorrect output/failure
/// </summary>
public static class MessageLogger
{
	/* Fields */

	private static readonly Dictionary<LogType, LogLevel> _modeLevels = new Dictionary<LogType, LogLevel>
	{
		{ LogType.Tile,			LogLevel.Warning | LogLevel.Error },
		{ LogType.Highlight,	LogLevel.Warning | LogLevel.Error },
		{ LogType.Actor,		LogLevel.Warning | LogLevel.Error },
		{ LogType.Graph,		LogLevel.Warning | LogLevel.Error },
		{ LogType.Path,			LogLevel.Warning | LogLevel.Error },
		{ LogType.FSM,			LogLevel.Warning | LogLevel.Error },
		{ LogType.Game,			LogLevel.Debug | LogLevel.Warning | LogLevel.Error },
		{ LogType.GUI,			LogLevel.Warning | LogLevel.Error }
	};

	/* Properties */

	public static LogLevel TileLevels { get => _modeLevels[LogType.Tile]; private set => _modeLevels[LogType.Tile] = value; }
	public static LogLevel HighlightLevels { get => _modeLevels[LogType.Highlight]; private set => _modeLevels[LogType.Highlight] = value; }
	public static LogLevel ActorLevels { get => _modeLevels[LogType.Actor]; private set => _modeLevels[LogType.Actor] = value; }
	public static LogLevel GraphLevels { get => _modeLevels[LogType.Graph]; private set => _modeLevels[LogType.Graph] = value; }
	public static LogLevel PathLevels { get => _modeLevels[LogType.Path]; private set => _modeLevels[LogType.Path] = value; }
	public static LogLevel FSMLevels { get => _modeLevels[LogType.FSM]; private set => _modeLevels[LogType.FSM] = value; }
	public static LogLevel GameLevels { get => _modeLevels[LogType.Game]; private set => _modeLevels[LogType.Game] = value; }
	public static LogLevel GUILevels { get => _modeLevels[LogType.GUI]; private set => _modeLevels[LogType.GUI] = value; }

	/* Methods */

	#region Debug flag enabling

	/// <summary>
	/// Gets levels of given type
	/// </summary>
	/// <param name="type">Log type</param>
	/// <returns>Levels of requested type</returns>
	public static LogLevel GetLevels(LogType type)
	{
		return _modeLevels[type];
	}

	/// <summary>
	/// Clear the debug levels of one of the logger's types
	/// </summary>
	/// <param name="type">Type to clear levels of</param>
	public static void ClearLevel(LogType type) => _modeLevels[type] = LogLevel.None;

	/// <summary>
	/// Clear all levels for all types
	/// </summary>
	public static void ClearAll()
	{
		List<LogType> types = new List<LogType>(_modeLevels.Keys);

		foreach (LogType type in types)
		{
			ClearLevel(type);
		}
	}

	/// <summary>
	/// Sets the given level flag on the given type
	/// </summary>
	/// <param name="type">Category that is having its level set</param>
	/// <param name="level">Level to be set</param>
	public static void EnableLevel(LogType type, LogLevel level) => _modeLevels[type] |= level;

	/// <summary>
	/// Clears the given level flag on the given type
	/// </summary>
	/// <param name="type">Category that is having its level cleared</param>
	/// <param name="level">Level to be cleared</param>
	public static void DisableLevel(LogType type, LogLevel level) => _modeLevels[type] &= ~level;

	/// <summary>
	/// Checks if the given type currently has the given level flag set
	/// </summary>
	/// <param name="type">Type being checked</param>
	/// <param name="level">Level flag being checked</param>
	/// <returns>True if the level is set, false if not</returns>
	public static bool TypeHasLevelEnabled(LogType type, LogLevel level)
	{
		if (_modeLevels[type] != LogLevel.None && level == LogLevel.None)
			return false;

		return (_modeLevels[type] & level) == level;
	}

	/// <summary>
	/// Checks if the given type currently has the given level flag cleared
	/// </summary>
	/// <param name="type">Type being checked</param>
	/// <param name="level">Level flag being checked</param>
	/// <returns>True if the level is clear, false if not</returns>
	public static bool TypeHasLevelDisabled(LogType type, LogLevel level) => !TypeHasLevelEnabled(type, level);

	#endregion

	#region Message logging

	/** DEPRECATED: logging by type
	public static void LogTileMessage(string msg, LogLevel level, params object[] args) => LogMessage(msg, LogType.Tile, level, args);
	public static void LogHighlightMessage(string msg, LogLevel level, params object[] args) => LogMessage(msg, LogType.Highlight, level, args);
	public static void LogActorMessage(string msg, LogLevel level, params object[] args) => LogMessage(msg, LogType.Actor, level, args);
	public static void LogGraphMessage(string msg, LogLevel level, params object[] args) => LogMessage(msg, LogType.Graph, level, args);
	public static void LogPathMessage(string msg, LogLevel level, params object[] args) => LogMessage(msg, LogType.Path, level, args);
	public static void LogFSMMessage(string msg, LogLevel level, params object[] args) => LogMessage(msg, LogType.FSM, level, args);
	public static void LogGameplayMessage(string msg, LogLevel level, params object[] args) => LogMessage(msg, LogType.Game, level, args);
	public static void LogGUIMessage(string msg, LogLevel level, params object[] args) => LogMessage(msg, LogType.GUI, level, args);
	*/

	// logging by error level 
	public static void LogDebugMessage(LogType type, string msg, params object[] args) => LogMessage(msg, type, LogLevel.Debug, args);
	public static void LogVerboseMessage(LogType type, string msg, params object[] args) => LogMessage(msg, type, LogLevel.Verbose, args);
	public static void LogWarningMessage(LogType type, string msg, params object[] args) => LogMessage(msg, type, LogLevel.Warning, args);
	public static void LogErrorMessage(LogType type, string msg, params object[] args) => LogMessage(msg, type, LogLevel.Error, args);

	private static void LogMessage(string msg, LogType type, LogLevel level, params object[] args)
	{
		if (TypeHasLevelDisabled(type, level)) return;

		switch (level)
		{
			case LogLevel.Debug | LogLevel.Verbose:
				Debug.Log(string.Format(msg, args));
				break;
			case LogLevel.Warning:
				Debug.LogWarning(string.Format(msg, args));
				break;
			case LogLevel.Error:
				Debug.LogError(string.Format(msg, args));
				break;
			default:
				Debug.Log(string.Format(msg, args));
				break;
		}
	}

#endregion

}
