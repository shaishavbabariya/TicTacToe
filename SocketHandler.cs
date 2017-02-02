using System.Collections;
using UnityEngine;
using SocketIO;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;
using System.Runtime.Serialization;
using UnityEngine.UI;
using System.Security.Cryptography;
using System.Security.Authentication;
using System;
public class SocketHandler : MonoBehaviour
{

	public static SocketHandler Inst;

	public static readonly byte[] ENCKEY = Encoding.ASCII.GetBytes ("DFK8s58uWFCF4Vs8NCrgTxfMLwjL9WUy");
	public static readonly byte[] ENCIV = Encoding.ASCII.GetBytes ("Artoon#3Solution");

	public static event Action<JSONObject> OnSocketResponse;

	[SerializeField]SocketIOComponent socket;

	internal bool isCanSendRLR;
	public SocketState socketState;
	bool isPongReceived;
	int pingMissCounter;

	void Awake(){
		Inst = this;
		socketState = SocketState.None;
		isCanSendRLR = false;
	}

	internal void Create(){
		print("Socket Creating");
		Loading.Inst.ShowLodingIndicator();
		socketState = SocketState.None;
		socket.url = ArtoonConfig.Inst.ServerUrl;
		socket.SetUpWS();
		Init();
	}

	internal IEnumerator CheckAndCreateSocket(){
		float t = Time.time;
		WWW www = new WWW(ArtoonConfig.Inst.BaseURL+"check");
		yield return www;
		if(www.error != null){
			print("Internet Not Reachable ->Resp Time :"+(Time.time - t).ToString());
			
		}else{
			print("Internet Checked ->Resp Time :"+(Time.time - t).ToString());
			Create();
		}
	}

	void Init() 
	{
		socket.On ("open", TestOpen);
		socket.On ("error", TestError);
		socket.On ("close", TestClose);
		socket.On ("res", TestResponse);
		socket.On ("pong", OnReceivePong);
		socket.On ("chat", OnReceiveChat);
	}

	public void TestOpen(SocketIOEvent e)
	{
		Debug.Log("[SocketIO] Open received: " + e.name + " " + e.data);
		socketState = SocketState.Open;
	}

	public void TestError(SocketIOEvent e)
	{
		socketState = SocketState.Error;
	}
	
	public void TestClose(SocketIOEvent e)
	{	
		Debug.Log("[SocketIO] Close received: " + e.name + " " + e.data);
		socketState = SocketState.Close;
	}

	public void TestResponse(SocketIOEvent e){
		socketState = SocketState.Running;
		string recv = AESDecrypt (e.data.GetField ("data").ToString ().Trim (new char[] {'"'}), ENCKEY, ENCIV);
		JSONObject obj = new JSONObject (recv);
		print("<color=#F11043FF>Received:</color>"+obj.ToString());
		if(OnSocketResponse != null)OnSocketResponse(obj);
	}

	public void OnReceiveChat(SocketIOEvent e){
		//print("CHATR_->"+e.data.GetField("data").ToString());
		if(OnSocketResponse != null)OnSocketResponse(e.data.GetField("data"));
	}

	public void OnReceivePong(SocketIOEvent e){
		isPongReceived = true;
		socketState = SocketState.Running;
		if(NetworkPreLoader.Inst != null){
			NetworkPreLoader.Inst.OnNwConnected();
		}
	}

	public void SendData (JSONObject obj)
	{
		print("<color=#32D51CFF>SEND :</color> " + obj.Print());
		JSONObject newData = new JSONObject ();
		string tempstr = AESEncrypt (obj.Print (), ENCKEY, ENCIV);
		newData.AddField ("data", tempstr);
		socket.Emit ("req", newData);
	}

	public void SendChatData (JSONObject obj)
	{
		socket.Emit ("chat", obj);
	}


	IEnumerator CheckPingTimeOut(){
		yield return new WaitForSecondsRealtime(5);
	SB:
		if(IsConnected()){
			isPongReceived = false;
			socket.Emit("ping",new JSONObject());
			float wait = 3;
			int c = 3;
			yield return new WaitForSecondsRealtime(wait);
			if(isPongReceived){
				pingMissCounter = 0;
			}else{
				pingMissCounter++;
				if(pingMissCounter > c){
					pingMissCounter = 0;
					Close();
				}
			}
			goto SB;
		}else{
			yield break;
		}
	}

	/// <summary>
	/// Connect this instance.
	/// </summary>
	internal void Connect(){
		if(!IsConnected()){
			socket.Connect();
			StartCoroutine(CheckPingTimeOut());
		}
	}
	/// <summary>
	/// Close this instance.
	/// </summary>
	internal void Close(){
		if(IsConnected()){
			socket.sid = "";
			socket.Close();
		}
	}

	void Update(){
		switch(socketState){
		case SocketState.Open:
			if(isCanSendRLR){
				isCanSendRLR = false;
				if(string.IsNullOrEmpty(GS.Inst.userInfo.ID)){
					if(LoginProcess.Inst.Session.Equals("FB")){
						SocketHandler.Inst.SendData(SocketEventManager.Inst.FBSignup());
					}
					if(LoginProcess.Inst.Session.Equals("GUEST")){
						SocketHandler.Inst.SendData(SocketEventManager.Inst.GuestSignUp());
					}
				}else{
					SendData(SocketEventManager.Inst.RLR());
				}
			}else{
				if(LoginProcess.Inst.LoginType.Equals("FB")){
					SocketHandler.Inst.SendData(SocketEventManager.Inst.FBSignup());
				}
				if(LoginProcess.Inst.LoginType.Equals("GUEST")){
					SocketHandler.Inst.SendData(SocketEventManager.Inst.GuestSignUp());
				}
			}
			socketState = SocketState.SignUp;
			break;
		case SocketState.Running:
			break;
		case SocketState.CallRLR:
			StartCoroutine(CheckAndReConnectSocket());
			break;
		case SocketState.Connecting:
			break;
		case SocketState.Error:
		case SocketState.Close:
			if(NetworkPreLoader.Inst != null){
				NetworkPreLoader.Inst.OnNwConnetionSlow();
			}
			socketState = SocketState.CallRLR;
			break;
		}
	}

	internal IEnumerator CheckAndReConnectSocket(){
		socketState = SocketState.Connecting;
		if(!string.IsNullOrEmpty(LoginProcess.Inst.Session) && !ArtoonConfig.Inst._serverConfig.m_Mode){
			yield return new WaitForSecondsRealtime(0.5f);
			isCanSendRLR = true;
			Connect();
		}
	}

	/// <summary>
	/// Determines whether this instance is connected.
	/// </summary>
	/// <returns><c>true</c> if this instance is connected; otherwise, <c>false</c>.</returns>
	internal bool IsConnected(){
		bool val = false;
		switch(socketState){
		case SocketState.Open:
		case SocketState.Running:
			val = true;
			break;
		}
		return val;
	}
	/// <summary>
	/// Sockets the I.
	/// </summary>
	/// <returns>The I.</returns>
	internal string GetSocketID(){
		return socket.sid;
	}

	/// <summary>
	/// /*Raises the application quit event.*/
	/// </summary>
	void OnApplicationQuit(){
		Close();
	}
	#region Encryption/Decryption Algorithm
	
	/// <summary>
	/// AES Encryption Algorithm 
	/// </summary>
	/// <returns>The encrypt.</returns>
	/// <param name="plainText">Plain text.</param>
	/// <param name="Key">Key.</param>
	/// <param name="IV">I.</param>
	public static string AESEncrypt (string plainText, byte[] Key, byte[] IV)
	{
		// Check arguments.
		if (plainText == null || plainText.Length <= 0)
			throw new System.ArgumentNullException ("plainText");
		if (Key == null || Key.Length <= 0)
			throw new System.ArgumentNullException ("Key");
		if (IV == null || IV.Length <= 0)
			throw new System.ArgumentNullException ("Key");
		byte[] encrypted;
		
		using (AesManaged aesAlg = new AesManaged()) {
			aesAlg.Key = Key;
			aesAlg.IV = IV;
			ICryptoTransform encryptor = aesAlg.CreateEncryptor (aesAlg.Key, aesAlg.IV);
			using (MemoryStream msEncrypt = new MemoryStream()) {
				using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write)) {
					using (StreamWriter swEncrypt = new StreamWriter(csEncrypt)) {
						swEncrypt.Write (plainText);
					}
					encrypted = msEncrypt.ToArray ();
				}
			}
		}
		return System.Convert.ToBase64String (encrypted);
	}


	/// <summary>
	/// AES Decryption Algorithm
	/// </summary>
	/// <returns>The decrypt.</returns>
	/// <param name="cipherText">Cipher text.</param>
	/// <param name="Key">Key.</param>
	/// <param name="IV">I.</param>
	public static string AESDecrypt (string cipherText, byte[] Key, byte[] IV)
	{
		byte[] cipherTextBytes;
		if (cipherText == null || cipherText.Length <= 0)
			throw new System.ArgumentNullException ("cipherText");
		else
			cipherTextBytes = System.Convert.FromBase64String (cipherText);
		
		if (Key == null || Key.Length <= 0)
			throw new System.ArgumentNullException ("Key");
		if (IV == null || IV.Length <= 0)
			throw new System.ArgumentNullException ("Key");
		
		string plaintext = null;
		using (AesManaged aesAlg = new AesManaged()) {
			aesAlg.Key = Key;
			aesAlg.IV = IV;
			ICryptoTransform decryptor = aesAlg.CreateDecryptor (aesAlg.Key, aesAlg.IV);
			using (MemoryStream msDecrypt = new MemoryStream(cipherTextBytes)) {
				using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read)) {
					using (StreamReader srDecrypt = new StreamReader(csDecrypt)) {
						plaintext = srDecrypt.ReadToEnd ();
					}
				}
			}
		}
		return plaintext;
	}
	#endregion

}

public enum SocketState{
	None,
	Close,
	Connecting,
	Open,
	Running,
	Error,
	CallRLR,
	SignUp,
	Logout
}