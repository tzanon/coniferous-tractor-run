using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System;

using Pathfinding;

namespace Tests
{
	public class PathTests
	{
		private Vector3Int[] PrepareSimpleValidArray()
		{
			var points = new Vector3Int[] {
				new Vector3Int(0,0,0),
				new Vector3Int(1,0,0),
				new Vector3Int(2,0,0),
				new Vector3Int(2,-1,0)};

			return points;
		}

		private Vector3Int[] PrepareSimpleInvalidArray()
		{
			var points = new Vector3Int[] {
				new Vector3Int(0,0,0),
				new Vector3Int(0,1,0),
				new Vector3Int(2,1,0),
				new Vector3Int(1,1,0)};

			return points;
		}

		#region array validity tests

		[Test]
		public void SimpleValidArray_Test()
		{
			var points = PrepareSimpleValidArray();
			Assert.IsTrue(Path.ArrayIsValidPath(points));
		}

		[Test]
		public void SimpleInvalidArray_Test()
		{
			var points = PrepareSimpleInvalidArray();
			Assert.IsFalse(Path.ArrayIsValidPath(points));
		}

		#endregion

		#region path validity tests

		[Test]
		public void EmptyPath_Test()
		{
			var zeroPath = new Path(new Vector3Int[0]);
			Assert.IsTrue(zeroPath.Empty);
		}

		[Test]
		public void SimpleValidPath_Test()
		{
			var points = PrepareSimpleValidArray();
			Assert.DoesNotThrow(() => new Path(points));
		}

		[Test]
		public void SimpleInvalidPath_Test()
		{
			var points = PrepareSimpleInvalidArray();
			Assert.Throws(typeof(Exception), () => new Path(points));
		}

		#endregion

		#region path comparison tests

		[Test]
		public void PathsAreEqual_Test()
		{
			var p1 = new Path(PrepareSimpleValidArray());
			var p2 = new Path(PrepareSimpleValidArray());

			Assert.IsTrue(p1.Equals(p2));
		}

		[Test]
		public void PathsAreUnequal_Test1()
		{
			var p1 = new Path(PrepareSimpleValidArray());
			var p2 = new Path(new Vector3Int[] { new Vector3Int(0, 0, 0), new Vector3Int(0, 1, 0), new Vector3Int(1, 1, 0), new Vector3Int(2, 1, 0) });

			Assert.IsFalse(p1.Equals(p2));
		}

		[Test]
		public void PathsAreUnequal_Test2()
		{
			var p1 = new Path(PrepareSimpleValidArray());
			var p2 = new Path(new Vector3Int[] { new Vector3Int(0, 0, 0) });

			Assert.IsFalse(p1.Equals(p2));
		}

		#endregion

		#region path concatenation tests

		[Test]
		public void PathConcat_Test()
		{
			var p1 = new Path(new Vector3Int[] { new Vector3Int(0, 0, 0), new Vector3Int(0, 1, 0), new Vector3Int(1, 1, 0) });
			var p2 = new Path(new Vector3Int[] { new Vector3Int(1, 1, 0), new Vector3Int(2, 1, 0), new Vector3Int(2, 0, 0) });
			var expectedConcatenation = new Path(new Vector3Int[] { new Vector3Int(0, 0, 0), new Vector3Int(0, 1, 0),
				new Vector3Int(1, 1, 0), new Vector3Int(2, 1, 0), new Vector3Int(2, 0, 0) });
			var actualConcatenation = p1 + p2;

			Assert.AreEqual(expectedConcatenation, actualConcatenation);
		}

		[Test]
		public void PathInvalidConcat_Test()
		{
			var p1 = new Path(new Vector3Int[] { new Vector3Int(0, 0, 0), new Vector3Int(0, 1, 0), new Vector3Int(1, 1, 0) });
			var p2 = new Path(new Vector3Int[] { new Vector3Int(3, 1, 0), new Vector3Int(2, 1, 0), new Vector3Int(2, 0, 0) });
			var expectedConcatenation = Path.EmptyPath;
			var actualConcatenation = p1 + p2;

			LogAssert.Expect(UnityEngine.LogType.Error, "ERROR: attempting to concatenate two non-adjacent paths!");
			Assert.AreEqual(expectedConcatenation, actualConcatenation);
		}

		#endregion

		/*
		// A Test behaves as an ordinary method
		[Test]
		public void PathTestsSimplePasses()
		{
			// Use the Assert class to test conditions
		}

		// A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
		// `yield return null;` to skip a frame.
		[UnityTest]
		public IEnumerator PathTestsWithEnumeratorPasses()
		{
			// Use the Assert class to test conditions.
			// Use yield to skip a frame.
			yield return null;
		}
		/**/
	}
}
