using UnityEngine;
using System.Collections.Generic;
using System;
using WebSocketSharp;
using SocketIO;

public class SocketHandler : MonoBehaviour 
{
	public static SocketHandler Inst;
	static private readonly char[] Delimiter = new char[] {','};

	public static event Action<JSONObject> OnReceiveMob;
	public static event Action<string> OnReceiveWeb;

	public SocketIOComponent SktIO;
	public SocketState state;

	void Awake()
	{
		Inst = this;

		state = SocketState.Close;

		switch(Application.platform){
		case RuntimePlatform.Android:
		case RuntimePlatform.IPhonePlayer:
			break;
		case RuntimePlatform.WebGLPlayer:
			Application.ExternalEval("socket.isReady = true;");
			break;
		}
	}

	#region iOS & Android Handlers
	internal void Create(){
//		SktIO.ws = new WebSocket("");
//		SktIO.ws.OnOpen 	+= OpenMob;
//		SktIO.ws.OnMessage 	+= ReceiveMob;
//		SktIO.ws.OnError 	+= ErrorMob;
//		SktIO.ws.OnClose 	+= CloseMob;

		Invoke ("SktInit", 1);
	}

	void SktInit(){
		SktIO.On("open", OpenMob);
		SktIO.On("req", ReceiveMob);
		SktIO.On("error", ErrorMob);
		SktIO.On("close", CloseMob);
	}

	internal void Connect(){
		if(!isConnected()){
			SktIO.Connect ();
		}
	}

	internal void Close(){
		SktIO.sid = "";
		SktIO.Close ();
	}

	bool isConnected(){
		return (!string.IsNullOrEmpty (SktIO.sid) && SktIO.IsConnected);
	}

	void OpenMob(SocketIOEvent e){
		print ("OnOpenMob " + e.data.ToString());
		state = SocketState.Open;
	}

	void ReceiveMob(SocketIOEvent e){
		state = SocketState.Running;
		if (OnReceiveMob != null)OnReceiveMob (e.data);
	}

	void ErrorMob(SocketIOEvent e){
		print ("ErrorMob " + e.data.ToString());
		state = SocketState.Error;
	}

	void CloseMob(SocketIOEvent e){
		print ("CloseMob " + e.data.ToString());
		state = SocketState.Close;
	}

	#endregion


	#region WebGL Handlers

	void OnOpenWebGL(){
		print ("OnOpenWebGL");
		state = SocketState.Open;
	}

	void OnReceiveWebGL(string msg){
		state = SocketState.Running;
		var args = msg.Split(Delimiter);
		if (OnReceiveWeb != null)OnReceiveWeb (args [0]);
	}

	void OnErrorWebGL(string msg){
		print ("OnErrorWebGL->" + msg);
		state = SocketState.Error;
	}

	void OnCloseWebGL(string msg){
		print ("OnCloseWebGL->" + msg);
		state = SocketState.Close;
	}



	#endregion

	internal void Send(JSONObject data){
		string msg = data.ToString ();
		print ("Send->" + msg);
		if (Application.platform == RuntimePlatform.WebGLPlayer) {
			Application.ExternalCall("socket.emit", "req", msg); 
		}else{
			SktIO.Emit ("req", data);
		}
	}
}

public enum SocketState{
	Close,
	Open,
	Running,
	Error
}