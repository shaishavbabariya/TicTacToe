using UnityEngine;
using System.Collections;

public class SocketEventReceiver : MonoBehaviour {
	
	void OnEnable(){
		SocketHandler.OnConnect += OnConnect;
		SocketHandler.OnReceive += OnReceive;
		SocketHandler.OnError 	+= OnError;
	}

	void OnDisable(){
		SocketHandler.OnConnect -= OnConnect;
		SocketHandler.OnReceive -= OnReceive;
		SocketHandler.OnError 	-= OnError;
	}

	void OnConnect(){
		print ("Socket connected");
		SocketHandler.Inst.Send (new JSONObject ());
	}

	void OnReceive(string msg){
		print ("Received :" + msg);
	}

	void OnError(string msg){
		print ("Error:" + msg);
	}

}
