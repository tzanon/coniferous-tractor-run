using System.Collections;
using UnityEngine;
using TMPro;

public class GuiMessageDisplayer : MonoBehaviour
{
	[SerializeField] TMP_Text statusMessage;

	private const float _displayTime = 5.0f;

	private void Start()
	{
		statusMessage.gameObject.SetActive(false);
	}

	public void DisplayEarlyExitMessage(int numCollectibles, string collectibleNameSingular)
	{
		var collectibleNameDisplay = collectibleNameSingular;

		if (numCollectibles != 1)
			collectibleNameDisplay += "s";

		var message = string.Format("You must find {0} more {1} before leaving!", numCollectibles, collectibleNameDisplay);
		ShowMessage(message);
	}

	// TODO: display level completion time?
	public void DisplayWonMessage() => ShowMessage("You Won!");

	public void DisplayLostMessage() => ShowMessage("GAME OVER\nYou Were Caught!");

	public void DisplayRemainingCollectibles(int numCollectibles, string collectibleNameSingular)
	{
		var message = FormatCollectibleMessage(numCollectibles, collectibleNameSingular);
		ShowMessage(message);
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

	private void ShowMessage(string message)
	{
		statusMessage.text = message;
		StartCoroutine(DisplayForSeconds(statusMessage, _displayTime));
	}

	private IEnumerator DisplayForSeconds(TMP_Text text, float seconds)
	{
		text.gameObject.SetActive(true);
		yield return new WaitForSeconds(seconds);
		text.gameObject.SetActive(false);
	}
}
