using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class QuitGame : MonoBehaviour
{
	public Button exitButton;

	void Start()
	{
		Button btn = exitButton.GetComponent<Button>();
		btn.onClick.AddListener(TaskOnClick);
	}

	void TaskOnClick()
	{
		Debug.Log("EXIT");
		Application.Quit();
	}

}
