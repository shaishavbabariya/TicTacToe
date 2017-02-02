using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Facebook.Unity;
public class LoginProcess : MonoBehaviour {

	public static LoginProcess Inst;

	internal string ID;
	internal string Name;
	internal string EmailID;
	internal string AT;
	internal string Permissions;
	internal string Session;
	internal bool LastCallFB;
	internal string LoginType;
	void Awake(){
		Inst = this;

		AT = PlayerPrefs.GetString("AT","");
		Permissions = PlayerPrefs.GetString("PER","");
		Session = PlayerPrefs.GetString("SSN","");

		PlayerPrefs.SetInt("FBL",0);
		if(FB.IsInitialized){
			FB.ActivateApp();
		}else{
			FB.Init(InitCallback,OnHideUnity);
		}
	}

	internal void CheckAndLogin(){
		if(string.IsNullOrEmpty(Session)){
			GS.Inst.LoadNextScene("Login");
		}else{
			if(Session.Equals("FB")){
				LoginFB();
			}else{
				LoginGuest();
			}
		}
//		if(SocketHandler.Inst.IsConnected()){
//			print("Call Login-SID:"+SocketHandler.Inst.GetSocketID());
//		}
	}

	public void OnButtonClick(Text lbl){
		if(lbl.text.Equals("Login")){
			LoginFB();
		}else{
			LogOut();
		}
	}
	/// <summary>
	/// Inits the callback.
	/// </summary>
	private void InitCallback ()
	{
		if (FB.IsInitialized) {
			// Signal an app activation App Event
			FB.ActivateApp();

			if(Session.Equals("FB") && AccessToken.CurrentAccessToken != null){
				StartCoroutine(ValidateAccessTocken(AccessToken.CurrentAccessToken.TokenString));
			}
			// Continue with Facebook SDK
//			print("InitCallback -"+FB.IsLoggedIn.ToString());
		} else {
			//Debug.Log("Failed to Initialize the Facebook SDK");
		}
	}

	IEnumerator ValidateAccessTocken(string at){
		string url = "https://graph.facebook.com/oauth/access_token_info?client_id="+FB.AppId+"&access_token="+at;
		WWW www = new WWW(url);
		yield return www;
		if(www.error != null){
			LogOut();
		}else{
			Permissions = AccessToken.CurrentAccessToken.Permissions.ToCommaSeparateList();
			PlayerPrefs.SetString("PER",Permissions);
			AT = AccessToken.CurrentAccessToken.TokenString;
			PlayerPrefs.SetString("AT",AT);
		}
	}
	private void OnHideUnity (bool isGameShown)
	{
		if (!isGameShown) {
			// Pause the game - we will need to hide
			Time.timeScale = 0;
		} else {
			// Resume the game - we're getting focus again
			Time.timeScale = 1;
		}
	}

	/// <summary>
	/// Logins the F.
	/// </summary>
	internal void LoginFB(){
		if(FB.IsLoggedIn){
			GetUserDetail();
		}else{
			FB.LogInWithReadPermissions(new List<string>() { "public_profile", "email", "user_friends" }, AuthCallback);
		}
	}

	/// <summary>
	/// Logins the guest.
	/// </summary>
	internal void LoginGuest(){
		LoginType = "GUEST";
		SocketHandler.Inst.Connect();
	}
	/// <summary>
	/// Auths the callback.
	/// </summary>
	/// <param name="result">Result.</param>
	void AuthCallback(ILoginResult result){
		if(!string.IsNullOrEmpty(result.Error)){
			if(LoginScreen.Instance != null){
				LoginScreen.Instance.ShowAlert("Login Error",result.Error);
			}
		}else{
			if(FB.IsLoggedIn){
				Permissions = result.AccessToken.Permissions.ToCommaSeparateList();
				PlayerPrefs.SetString("PER",Permissions);
				AT = AccessToken.CurrentAccessToken.TokenString;
				PlayerPrefs.SetString("AT",AT);
				GetUserDetail();
			}else{
				if(LoginScreen.Instance != null){
					LoginScreen.Instance.ShowAlert("Login Cancelled","You have cancelled Facebook login.");
					LoginScreen.Instance.OpenScreen();
				}
			}
		}
	}

	/// <summary>
	/// Gets the user detail.
	/// </summary>
	void GetUserDetail(){
		FB.API("me?fields=id,name,email",HttpMethod.GET,UserDetailCallback,new Dictionary<string, string>());
	}
	/// <summary>
	/// Users the detail callback.
	/// </summary>
	/// <param name="result">Result.</param>
	void UserDetailCallback(IGraphResult result){
		if(result != null){
			JSONObject data = new JSONObject(result.RawResult);
			ID = data.GetField("id").ToString().Trim(new char[]{'"'});
			Name = data.GetField("name").ToString().Trim(new char[]{'"'});
			if(data.HasField("email")){
				EmailID = data.GetField("email").ToString().Trim(new char[]{'"'}).Replace(@"\u0040","@");
				PlayerPrefs.SetString("Email",EmailID);
			}
			LoginType = "FB";
			SocketHandler.Inst.Connect();
		}else{
			FB.LogInWithReadPermissions(new List<string>() { "public_profile", "email", "user_friends" }, AuthCallback);
		}
	}
	/// <summary>
	/// Determines whether this instance is all permission granted the specified result.
	/// </summary>
	/// <returns><c>true</c> if this instance is all permission granted the specified result; otherwise, <c>false</c>.</returns>
	/// <param name="result">Result.</param>
	bool IsAllPermissionGranted(string result){
		return (result.Contains("public_profile"));
	}
	/// <summary>
	/// Logs the out.
	/// </summary>
	internal void LogOut(){
//		Loading.Inst.HideLodingIndicator();
		PlayerPrefs.SetString("SSN","");
		Session = "";
		LoginType = "";
		PlayerPrefs.SetInt("NewLogin",0);
		if(FB.IsLoggedIn){
			PlayerPrefs.SetString("Email","");
			FB.LogOut();
		}

		SocketHandler.Inst.Close();
	}
}
