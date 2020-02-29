using UnityEngine;

/// <summary>
/// Sorts sprites such that ones with higher y-coords are rendered before (i.e. are "behind") the lower ones
/// Credits to the Code Monkey YouTube channel for this simple, powerful method
/// </summary>

[RequireComponent(typeof(SpriteRenderer))]
public class SpritePositionRenderer : ScriptBase
{
	[SerializeField] private float _calculationRate = 0.1f;
	private float _nextCalculation = 0f;

	private static int _sortingOrderBase = 1000;
	[SerializeField] private int _offset = 0;
	[SerializeField] private bool _runOnlyOnce = false;

	private SpriteRenderer _sr;

	private void Awake()
	{
		_sr = GetComponent<SpriteRenderer>();
		_sr.sortingLayerName = "Objects";

		if (_runOnlyOnce)
		{
			CalculateOrder();
			Destroy(this);
		}
	}

	private void LateUpdate()
	{
		if (_runOnlyOnce) return;
		
		_nextCalculation -= Time.deltaTime;

		if (_nextCalculation <= 0f)
		{
			CalculateOrder();
			_nextCalculation = _calculationRate;
		}
	}

	private void CalculateOrder()
	{
		_sr.sortingOrder = (int)(_sortingOrderBase - transform.position.y - _offset);
	}
}
