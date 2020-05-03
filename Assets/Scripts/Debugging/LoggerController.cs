using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class LoggerController : MonoBehaviour
{
	// TODO: group level, toggle, actions/delegates in structs?

	// fields

	[SerializeField] private Toggle _debugToggle, _verboseToggle, _warningToggle, _errorToggle;
	[SerializeField] private TMP_Dropdown _typeSelector;

	private readonly Dictionary<Toggle, LogLevel> _toggleLevels = new Dictionary<Toggle, LogLevel>();

	private UnityAction<bool> _readDebugToggle;
	private UnityAction<bool> _readVerboseToggle;
	private UnityAction<bool> _readWarningToggle;
	private UnityAction<bool> _readErrorToggle;

	// properties

	public LogType SelectedType { get => (LogType)_typeSelector.value; }

	private void Start()
	{
		InitToggleDictionary();
		InitToggleReaders();

		InitListeners();
		UpdateToggles();
	}

	private void InitToggleDictionary()
	{
		_toggleLevels.Add(_debugToggle, LogLevel.Debug);
		_toggleLevels.Add(_verboseToggle, LogLevel.Verbose);
		_toggleLevels.Add(_warningToggle, LogLevel.Warning);
		_toggleLevels.Add(_errorToggle, LogLevel.Error);
	}

	/// <summary>
	/// Set up toggle read listeners
	/// </summary>
	private void InitToggleReaders()
	{
		_readDebugToggle = delegate { ReadToggleValue(_debugToggle); };
		_readVerboseToggle = delegate { ReadToggleValue(_verboseToggle); };
		_readWarningToggle = delegate { ReadToggleValue(_warningToggle); };
		_readErrorToggle = delegate { ReadToggleValue(_errorToggle); };
	}

	/// <summary>
	/// Add listeners for dropdown and toggles
	/// </summary>
	private void InitListeners()
	{
		foreach (string modeName in Enum.GetNames(typeof(LogType)))
			_typeSelector.options.Add(new TMP_Dropdown.OptionData(modeName));

		_typeSelector.onValueChanged.AddListener(delegate { UpdateToggles(); });

		EnableToggleListeners();
	}

	/// <summary>
	/// Add toggle listeners
	/// </summary>
	private void EnableToggleListeners()
	{
		_debugToggle.onValueChanged.AddListener(_readDebugToggle);
		_verboseToggle.onValueChanged.AddListener(_readVerboseToggle);
		_warningToggle.onValueChanged.AddListener(_readWarningToggle);
		_errorToggle.onValueChanged.AddListener(_readErrorToggle);
	}

	/// <summary>
	/// Remove toggle listeners
	/// </summary>
	private void DisableToggleListeners()
	{
		_debugToggle.onValueChanged.RemoveListener(_readDebugToggle);
		_verboseToggle.onValueChanged.RemoveListener(_readVerboseToggle);
		_warningToggle.onValueChanged.RemoveListener(_readWarningToggle);
		_errorToggle.onValueChanged.RemoveListener(_readErrorToggle);
	}

	/// <summary>
	/// Reads value of given toggle and sets/clears corresponding level flag
	/// </summary>
	/// <param name="toggle">Toggle to read</param>
	private void ReadToggleValue(Toggle toggle)
	{
		if (toggle.isOn)
		{
			MessageLogger.EnableLevel(SelectedType, _toggleLevels[toggle]);
			MessageLogger.LogGUIMessage("{0} enabled", LogLevel.Debug, toggle.name);
		}
		else
		{
			MessageLogger.LogGUIMessage("{0} disabled", LogLevel.Debug, toggle.name);
			MessageLogger.DisableLevel(SelectedType, _toggleLevels[toggle]);
		}

		MessageLogger.LogGUIMessage("Current {0} level is {1}", LogLevel.Verbose, SelectedType, MessageLogger.GetLevels(SelectedType));
	}

	/// <summary>
	/// Update toggles with current values from logger
	/// </summary>
	private void UpdateToggles()
	{
		DisableToggleListeners();

		foreach (Toggle toggle in _toggleLevels.Keys)
		{
			MessageLogger.LogGUIMessage("{0} updated", LogLevel.Verbose, toggle);

			if (MessageLogger.TypeHasLevelEnabled(SelectedType, _toggleLevels[toggle]))
			{
				toggle.isOn = true;
			}
			else
			{
				toggle.isOn = false;
			}
		}

		EnableToggleListeners();
		MessageLogger.LogGUIMessage("Selected {0}, currently has levels {1}", LogLevel.Debug, SelectedType, MessageLogger.GetLevels(SelectedType));
	}

}
