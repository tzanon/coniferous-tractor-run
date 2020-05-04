using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
	public class FSMTester
	{
		private FiniteStateMachine _fsm;
		private DummyState a = new DummyState("A");
		private DummyState b = new DummyState("B");
		private DummyState c = new DummyState("C");

		static int x = 0;

		private FSMTransition aToB;
		private FSMTransition toA;
		private FSMTransition bToC;

		Func<bool> xIsPos = () => x > 0;
		Func<bool> xIsNeg = () => x < 0;
		Func<bool> xIsZero = () => x == 0;

		[OneTimeSetUp]
		public void InitSFM()
		{
			/**
			a = new DummyState();
			b = new DummyState();
			c = new DummyState();
			/**/

			aToB = new FSMTransition(b, xIsPos);
			toA = new FSMTransition(a, xIsNeg);
			bToC = new FSMTransition(c, xIsPos);

			a.AddTransition(aToB);
			b.AddTransition(toA);
			b.AddTransition(bToC);
			c.AddTransition(toA);

			_fsm = new FiniteStateMachine(a);
		}

		[TearDown]
		public void ResetFSM()
		{
			_fsm = new FiniteStateMachine(a);
		}

		[Test]
		public void TestTransitAToB()
		{
			x = 3;
			_fsm.Run();

			Assert.AreEqual(b, _fsm.CurrentState);
		}

		[Test]
		public void TestTransitBToA()
		{
			x = 3;
			_fsm.Run();

			x = -7;
			_fsm.Run();

			Assert.AreEqual(a, _fsm.CurrentState);
		}

		[Test]
		public void TestTransitBToC()
		{
			x = 3;
			_fsm.Run();
			_fsm.Run();

			Assert.AreEqual(c, _fsm.CurrentState);
		}

		[Test]
		public void TestTransitCToA()
		{
			x = 3;
			_fsm.Run();
			_fsm.Run();

			x = -5;
			_fsm.Run();

			Assert.AreEqual(a, _fsm.CurrentState);
		}

		/*
		// A Test behaves as an ordinary method
		[Test]
		public void FSMTesterSimplePasses()
		{
			// Use the Assert class to test conditions
		}

		// A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
		// `yield return null;` to skip a frame.
		[UnityTest]
		public IEnumerator FSMTesterWithEnumeratorPasses()
		{
			// Use the Assert class to test conditions.
			// Use yield to skip a frame.
			yield return null;
		}
		/**/
	}
}
