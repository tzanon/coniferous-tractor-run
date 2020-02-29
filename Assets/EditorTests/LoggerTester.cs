using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
	public class LoggerTester
	{

		[TearDown]
		public void ResetLogger()
		{
			MessageLogger.ClearAll();
		}

		[Test]
		public void AddFlag()
		{
			// Debug level for Path is currently None; add Error
			MessageLogger.EnableLevel(MessageLogger.Type.Path, MessageLogger.Level.Error);
			Assert.AreEqual(MessageLogger.Level.Error, MessageLogger.PathLevels);
		}

		[Test]
		public void AddMultipleFlags()
		{
			MessageLogger.EnableLevel(MessageLogger.Type.Path, MessageLogger.Level.Error);
			MessageLogger.EnableLevel(MessageLogger.Type.Path, MessageLogger.Level.Debug);
			MessageLogger.EnableLevel(MessageLogger.Type.Path, MessageLogger.Level.Warning);

			var enabledLevels = MessageLogger.Level.Error | MessageLogger.Level.Debug | MessageLogger.Level.Warning;
			Assert.AreEqual(enabledLevels, MessageLogger.PathLevels);
		}

		[Test]
		public void IsEnabled()
		{
			MessageLogger.EnableLevel(MessageLogger.Type.Path, MessageLogger.Level.Error);
			Assert.IsTrue(MessageLogger.TypeHasLevelEnabled(MessageLogger.Type.Path, MessageLogger.Level.Error));
		}

		[Test]
		public void MultipleFlagsAreEnabled()
		{
			MessageLogger.EnableLevel(MessageLogger.Type.Path, MessageLogger.Level.Error);
			MessageLogger.EnableLevel(MessageLogger.Type.Path, MessageLogger.Level.Debug);
			MessageLogger.EnableLevel(MessageLogger.Type.Path, MessageLogger.Level.Warning);

			Assert.IsTrue(MessageLogger.TypeHasLevelEnabled(MessageLogger.Type.Path, MessageLogger.Level.Error) &&
				MessageLogger.TypeHasLevelEnabled(MessageLogger.Type.Path, MessageLogger.Level.Debug) &&
				MessageLogger.TypeHasLevelEnabled(MessageLogger.Type.Path, MessageLogger.Level.Warning));
		}

		[Test]
		public void IsDisabled()
		{
			MessageLogger.EnableLevel(MessageLogger.Type.Tile, MessageLogger.Level.Warning);
			MessageLogger.EnableLevel(MessageLogger.Type.Tile, MessageLogger.Level.Debug);

			Assert.IsTrue(MessageLogger.TypeHasLevelDisabled(MessageLogger.Type.Tile, MessageLogger.Level.Verbose));
		}

		[Test]
		public void MultipleFlagsAreDisabled()
		{
			MessageLogger.EnableLevel(MessageLogger.Type.Game, MessageLogger.Level.Warning);
			MessageLogger.EnableLevel(MessageLogger.Type.Game, MessageLogger.Level.Debug);

			Assert.IsTrue(MessageLogger.TypeHasLevelDisabled(MessageLogger.Type.Game, MessageLogger.Level.Verbose) &&
				MessageLogger.TypeHasLevelDisabled(MessageLogger.Type.Game, MessageLogger.Level.Error) &&
				MessageLogger.TypeHasLevelDisabled(MessageLogger.Type.Game, MessageLogger.Level.None));
		}

	}
}
