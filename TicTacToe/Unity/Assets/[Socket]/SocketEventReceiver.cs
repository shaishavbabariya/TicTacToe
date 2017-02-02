using UnityEngine;
using System.Collections;

public class SocketEventReceiver : MonoBehaviour {
	
	void OnEnable(){
		if(Application.platform == RuntimePlatform.WebGLPlayer){
			SocketHandler.OnReceiveWeb += ReceiveWebGl;
		}else{
			SocketHandler.OnReceiveMob += ReceiveMob;
		}
	}

	void OnDisable(){
		if(Application.platform == RuntimePlatform.WebGLPlayer){
			SocketHandler.OnReceiveWeb -= ReceiveWebGl;
		}else{
			SocketHandler.OnReceiveMob -= ReceiveMob;
		}
	}

	void ReceiveMob(JSONObject data){
		print ("Received ->" + data.ToString ());
	}

	void ReceiveWebGl(string data){
		print ("Received ->" + data);
	}
}
