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
			MessageLogger.EnableLevel(MessageLogger.Type.Path, MessageLogger.Level.Error);

			// logger's path level should be exactly just the error
			Assert.AreEqual(MessageLogger.Level.Error, MessageLogger.PathLevels);
		}

		/// <summary>
		/// Test if multiple flags of the logger have been successfully
		/// </summary>
		[Test]
		public void AddMultipleFlags()
		{
			// add error, debug, and warning levels to path level
			MessageLogger.EnableLevel(MessageLogger.Type.Path, MessageLogger.Level.Error);
			MessageLogger.EnableLevel(MessageLogger.Type.Path, MessageLogger.Level.Debug);
			MessageLogger.EnableLevel(MessageLogger.Type.Path, MessageLogger.Level.Warning);

			// logger's current path level should be exactly error, debug, and warning
			var enabledLevels = MessageLogger.Level.Error | MessageLogger.Level.Debug | MessageLogger.Level.Warning;
			Assert.AreEqual(enabledLevels, MessageLogger.PathLevels);
		}

		/// <summary>
		/// Checks if logger can correctly identify if a flag is enabled
		/// </summary>
		[Test]
		public void IsEnabled()
		{
			// set error level; it should be enabled
			MessageLogger.EnableLevel(MessageLogger.Type.Path, MessageLogger.Level.Error);
			Assert.IsTrue(MessageLogger.TypeHasLevelEnabled(MessageLogger.Type.Path, MessageLogger.Level.Error));
		}

		/// <summary>
		/// Checks if logger can correctly identify if multiple flags are enabled
		/// </summary>
		[Test]
		public void MultipleFlagsAreEnabled()
		{
			// set error, debug, and warning levels for path
			MessageLogger.EnableLevel(MessageLogger.Type.Path, MessageLogger.Level.Error);
			MessageLogger.EnableLevel(MessageLogger.Type.Path, MessageLogger.Level.Debug);
			MessageLogger.EnableLevel(MessageLogger.Type.Path, MessageLogger.Level.Warning);

			// the three set levels should be enabled
			Assert.IsTrue(MessageLogger.TypeHasLevelEnabled(MessageLogger.Type.Path, MessageLogger.Level.Error) &&
				MessageLogger.TypeHasLevelEnabled(MessageLogger.Type.Path, MessageLogger.Level.Debug) &&
				MessageLogger.TypeHasLevelEnabled(MessageLogger.Type.Path, MessageLogger.Level.Warning));
		}

		/// <summary>
		/// Checks if logger can correctly identify if a flag is disabled
		/// </summary>
		[Test]
		public void IsDisabled()
		{
			// set warning and debug levels for game
			MessageLogger.EnableLevel(MessageLogger.Type.Tile, MessageLogger.Level.Warning);
			MessageLogger.EnableLevel(MessageLogger.Type.Tile, MessageLogger.Level.Debug);

			// Verbose should be disabled
			Assert.IsTrue(MessageLogger.TypeHasLevelDisabled(MessageLogger.Type.Tile, MessageLogger.Level.Verbose));
		}

		/// <summary>
		/// Checks if logger can correctly identify if multiple flags are disabled
		/// </summary>
		[Test]
		public void MultipleFlagsAreDisabled()
		{
			// set warning and debug levels for game
			MessageLogger.EnableLevel(MessageLogger.Type.Game, MessageLogger.Level.Warning);
			MessageLogger.EnableLevel(MessageLogger.Type.Game, MessageLogger.Level.Debug);

			// verbose, error, and (especially!) none should be disabled
			Assert.IsTrue(MessageLogger.TypeHasLevelDisabled(MessageLogger.Type.Game, MessageLogger.Level.Verbose) &&
				MessageLogger.TypeHasLevelDisabled(MessageLogger.Type.Game, MessageLogger.Level.Error) &&
				MessageLogger.TypeHasLevelDisabled(MessageLogger.Type.Game, MessageLogger.Level.None));
		}

	}
}
