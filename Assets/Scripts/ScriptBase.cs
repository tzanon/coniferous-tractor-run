using UnityEngine;

/// <summary>
/// Base script for everything. Contains convenience methods for debug/error logging.
/// </summary>
public class ScriptBase : MonoBehaviour
{
	[SerializeField] private bool _debugMode = false;
	[SerializeField] private bool _warningMode = true;
	[SerializeField] private bool _errorMode = true;

	public bool DebugMode { get => _debugMode; set => _debugMode = value; }
	public bool WarningMode { get => _warningMode; set => _warningMode = value; }
	public bool ErrorMode { get => _errorMode; set => _errorMode = value; }

	/// <summary>
	/// Logs a debug message if debugging is enabled.
	/// </summary>
	/// <param name="message">Message to log</param>
	protected void LogDebugMessage(string message)
	{
		if (_debugMode)
			Debug.Log(message);
	}

	/// <summary>
	/// Logs a warning message if warnings are enabled.
	/// </summary>
	/// <param name="message">Message to log</param>
	protected void LogWarningMessage(string message)
	{
		if (_warningMode)
			Debug.LogWarning(message);
	}

	/// <summary>
	/// Logs an error message if errors are enabled.
	/// </summary>
	/// <param name="message">Message to log</param>
	protected void LogErrorMessage(string message)
	{
		if (_errorMode)
			Debug.LogError(message);
	}

}
