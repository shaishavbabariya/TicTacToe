using UnityEngine;
using System.Collections;
using System;

public class SocketHandler : MonoBehaviour {

	public static SocketHandler Inst;

	public static event Action OnConnect;
	public static event Action<string> OnReceive;
	public static event Action<string> OnError;

	Uri uri;
	WebSocket socket;
	void Awake(){
		Inst = this; 
	}

	// Use this for initialization
	void Start () {
		uri = new Uri ("ws://echo.websocket.org");
		socket = new WebSocket(uri);

		StartCoroutine (Connect ());
	}
	
	IEnumerator Connect(){
		yield return StartCoroutine(socket.Connect());
		if (OnConnect != null)
			OnConnect ();

		StartCoroutine (RunSocketThread ());
	}

	// Use this for initialization
	IEnumerator RunSocketThread () {
		while (true)
		{
			string reply = socket.RecvString();
			if (reply != null)
			{
				if (OnReceive != null)
					OnReceive (reply);
			}
			if (socket.error != null)
			{
				if (OnError != null)
					OnError (socket.error);
				break;
			}
			yield return 0;
		}
		socket.Close();
	}

	internal void Send(JSONObject data){
		JSONObject obj = new JSONObject ();
		obj.AddField ("en", "Login");
		obj.AddField ("un", "SB");
		obj.AddField ("pswd", "123456");

		print ("Send :" + obj.ToString ());

		socket.SendString(obj.ToString());
	}
}
