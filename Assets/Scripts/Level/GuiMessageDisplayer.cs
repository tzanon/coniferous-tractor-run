using System.Collections;
using UnityEngine;
using TMPro;

public class GuiMessageDisplayer : MonoBehaviour
{
	[SerializeField] TMP_Text _statusMessage;

	private const float _delayTime = 2.0f;
	private const float _displayTime = 5.0f;

	private void Start()
	{
		_statusMessage.gameObject.SetActive(false);
	}

	public void DisplayBeginMessage(int numCollectibles, string collectibleNameSingular)
	{
		ShowDelayedTimedMessage(string.Format("Collect all {0} {1}s and return here\nBeware the Tractor...", numCollectibles, collectibleNameSingular));
	}

	public void DisplayEarlyExitMessage(int numCollectibles, string collectibleNameSingular)
	{
		var collectibleNameDisplay = collectibleNameSingular;

		if (numCollectibles != 1)
			collectibleNameDisplay += "s";

		var message = string.Format("You must find {0} more {1} before leaving!", numCollectibles, collectibleNameDisplay);
		ShowTimedMessage(message);
	}

	// TODO: display level completion time?
	public void DisplayWonMessage() => ShowTimedMessage("You Won!");

	public void DisplayLostMessage() => ShowTimedMessage("GAME OVER\nYou Were Caught!");

	public void DisplayRemainingCollectibles(int numCollectibles, string collectibleNameSingular)
	{
		var message = FormatCollectibleMessage(numCollectibles, collectibleNameSingular);
		ShowTimedMessage(message);
	}

	private string FormatCollectibleMessage(int numCollectibles, string collectibleNameSingular)
	{
		var collectibleNameDisplay = collectibleNameSingular;

		if (numCollectibles != 1)
		{
			collectibleNameDisplay += "s";

			if (numCollectibles == 0)
				return string.Format("{0} {1} remain.\nGet to the exit!", numCollectibles, collectibleNameDisplay);
			else
				return string.Format("{0} {1} remain...", numCollectibles, collectibleNameDisplay);
		}
		else
			return string.Format("{0} {1} remains...", numCollectibles, collectibleNameDisplay);
	}

	private void ShowTextWithMessage(string message)
	{
		_statusMessage.text = message;
		_statusMessage.gameObject.SetActive(true);
	}

	private void HideText()
	{
		_statusMessage.gameObject.SetActive(false);
	}

	private void ShowTimedMessage(string message)
	{
		_statusMessage.text = message;
		StartCoroutine(DisplayForSeconds(_displayTime));
	}

	private void ShowDelayedTimedMessage(string message)
	{
		_statusMessage.text = message;
		StartCoroutine(TimedDisplayAfterDelay(_delayTime, _displayTime));
	}

	private IEnumerator DisplayForSeconds(float seconds)
	{
		_statusMessage.gameObject.SetActive(true);
		yield return new WaitForSeconds(seconds);
		_statusMessage.gameObject.SetActive(false);
	}

	private IEnumerator TimedDisplayAfterDelay(float delaySeconds, float displaySeconds)
	{
		yield return new WaitForSeconds(delaySeconds);
		StartCoroutine(DisplayForSeconds(displaySeconds));
	}
}
