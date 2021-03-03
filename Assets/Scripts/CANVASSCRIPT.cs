using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CANVASSCRIPT : MonoBehaviour
{

	[SerializeField] GameObject mInstructionsObject;
	[SerializeField] GameObject mModeSelectMenuObject;
	public void RESTART()
	{
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}
	public void Play()
	{
		if (mModeSelectMenuObject)
			mModeSelectMenuObject.SetActive(true);
	}
	public void QUIT()
	{
		Application.Quit();
	}
	public void callMainMenu()
	{
		SceneManager.LoadScene(0);
	}
	public void callSinglePlayer()
	{
		SceneManager.LoadScene(1);
	}
	public void callMultiPlayer()
	{
		SceneManager.LoadScene(2);
	}

	public void ShowInstructions(bool show)
	{
		if (mInstructionsObject)
			mInstructionsObject.SetActive(show);
	}
}
