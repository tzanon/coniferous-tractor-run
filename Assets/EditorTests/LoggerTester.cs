using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
	public class LoggerTester
	{
		/// <summary>
		/// Clears all flags on the logger for other tests
		/// </summary>
		[TearDown]
		public void ResetLogger()
		{
			MessageLogger.ClearAll();
		}

		/// <summary>
		/// Test if one of the logger's flags has been successfully set
		/// </summary>
		[Test]
		public void AddFlag()
		{
			// debug level for path is currently none; add error
			MessageLogger.EnableLevel(LogType.Path, LogLevel.Error);

			// logger's path level should be exactly just the error
			Assert.AreEqual(LogLevel.Error, MessageLogger.PathLevels);
		}

		/// <summary>
		/// Test if multiple flags of the logger have been successfully
		/// </summary>
		[Test]
		public void AddMultipleFlags()
		{
			// add error, debug, and warning levels to path level
			MessageLogger.EnableLevel(LogType.Path, LogLevel.Error);
			MessageLogger.EnableLevel(LogType.Path, LogLevel.Debug);
			MessageLogger.EnableLevel(LogType.Path, LogLevel.Warning);

			// logger's current path level should be exactly error, debug, and warning
			var enabledLevels = LogLevel.Error | LogLevel.Debug | LogLevel.Warning;
			Assert.AreEqual(enabledLevels, MessageLogger.PathLevels);
		}

		/// <summary>
		/// Checks if logger can correctly identify if a flag is enabled
		/// </summary>
		[Test]
		public void IsEnabled()
		{
			// set error level; it should be enabled
			MessageLogger.EnableLevel(LogType.Path, LogLevel.Error);
			Assert.IsTrue(MessageLogger.TypeHasLevelEnabled(LogType.Path, LogLevel.Error));
		}

		/// <summary>
		/// Checks if logger can correctly identify if multiple flags are enabled
		/// </summary>
		[Test]
		public void MultipleFlagsAreEnabled()
		{
			// set error, debug, and warning levels for path
			MessageLogger.EnableLevel(LogType.Path, LogLevel.Error);
			MessageLogger.EnableLevel(LogType.Path, LogLevel.Debug);
			MessageLogger.EnableLevel(LogType.Path, LogLevel.Warning);

			// the three set levels should be enabled
			Assert.IsTrue(MessageLogger.TypeHasLevelEnabled(LogType.Path, LogLevel.Error) &&
				MessageLogger.TypeHasLevelEnabled(LogType.Path, LogLevel.Debug) &&
				MessageLogger.TypeHasLevelEnabled(LogType.Path, LogLevel.Warning));
		}

		/// <summary>
		/// Checks if logger can correctly identify if a flag is disabled
		/// </summary>
		[Test]
		public void IsDisabled()
		{
			// set warning and debug levels for game
			MessageLogger.EnableLevel(LogType.Tile, LogLevel.Warning);
			MessageLogger.EnableLevel(LogType.Tile, LogLevel.Debug);

			// Verbose should be disabled
			Assert.IsTrue(MessageLogger.TypeHasLevelDisabled(LogType.Tile, LogLevel.Verbose));
		}

		/// <summary>
		/// Checks if logger can correctly identify if multiple flags are disabled
		/// </summary>
		[Test]
		public void MultipleFlagsAreDisabled()
		{
			// set warning and debug levels for game
			MessageLogger.EnableLevel(LogType.Game, LogLevel.Warning);
			MessageLogger.EnableLevel(LogType.Game, LogLevel.Debug);

			// verbose, error, and (especially!) none should be disabled
			Assert.IsTrue(MessageLogger.TypeHasLevelDisabled(LogType.Game, LogLevel.Verbose) &&
				MessageLogger.TypeHasLevelDisabled(LogType.Game, LogLevel.Error) &&
				MessageLogger.TypeHasLevelDisabled(LogType.Game, LogLevel.None));
		}

	}
}
