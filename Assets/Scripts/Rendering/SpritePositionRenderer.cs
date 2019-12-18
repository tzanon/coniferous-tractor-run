using UnityEngine;

/// <summary>
/// Sorts sprites such that ones with higher y-coords are rendered before (i.e. are "behind") the lower ones
/// Credits to the Code Monkey YouTube channel for this simple, powerful method
/// </summary>

[RequireComponent(typeof(SpriteRenderer))]
public class SpritePositionRenderer : MonoBehaviour
{
	[SerializeField] private float calculationRate = 0.1f;
	private float nextCalculation = 0f;

	private static int sortingOrderBase = 1000;
	[SerializeField] private int offset = 0;
	[SerializeField] private bool runOnlyOnce = false;

	private SpriteRenderer sr;

	private void Awake()
	{
		sr = GetComponent<SpriteRenderer>();
		sr.sortingLayerName = "Objects";

		if (runOnlyOnce)
		{
			CalculateOrder();
			Destroy(this);
		}
	}

	private void LateUpdate()
	{
		nextCalculation -= Time.deltaTime;

		if (!runOnlyOnce && nextCalculation <= 0f)
		{
			CalculateOrder();
			nextCalculation = calculationRate;
		}
	}

	private void CalculateOrder()
	{
		sr.sortingOrder = (int)(sortingOrderBase - transform.position.y - offset);
	}
}
