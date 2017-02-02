using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerController : MonoBehaviour 
{
	public Text textUi;



	void Talk(string message)
	{
		textUi.text = message;
		Application.ExternalCall("socket.emit", "talk", message); 
	}


}
