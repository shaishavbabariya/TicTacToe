using UnityEngine;
using System.Collections;
using Facebook.Unity;

public class SocketEventManager : MonoBehaviour {

	public static SocketEventManager Inst;

	void Awake(){
		Inst = this;
	}

	internal JSONObject RLR(){
		JSONObject obj = new JSONObject ();
		JSONObject data = new JSONObject ();

		obj.AddField("en","RLR");

		data.AddField ("UserId", GS.Inst.userInfo.ID);
		data.AddField ("det", ArtoonConfig.Inst.deviceTyp);
		data.AddField("tableId",GS.Inst.isCanRejoin);
		data.AddField ("speed",NetworkCheck.Speed);
		obj.AddField("data",data);
		return obj;
	}

	internal JSONObject FBSignup(){
		JSONObject obj = new JSONObject();
		JSONObject data = new JSONObject ();
		
		obj.AddField ("en", "FBSignup");
		
		data.AddField ("id", LoginProcess.Inst.ID);
		data.AddField ("un", Constant.FilterTxt(LoginProcess.Inst.Name));
        data.AddField ("email", LoginProcess.Inst.EmailID);
		data.AddField ("at", PlayerPrefs.GetString("AT"));
		data.AddField ("sno", ArtoonConfig.Inst.deviceUniqueIdentifier);
		data.AddField ("dids",PlayerPrefs.GetString("TOKEN"));
        data.AddField ("det", ArtoonConfig.Inst.deviceTyp);
		data.AddField ("os",Constant.OSInfo);
		data.AddField ("dc",Constant.DVCInfo);
		data.AddField ("ver",ArtoonConfig.Inst.Version);
		data.AddField ("speed",NetworkCheck.Speed);
		data.AddField ("ult","FBSignup");
		data.AddField ("prms", PlayerPrefs.GetString("PER"));

		obj.AddField ("data", data);
		return obj;
	}

	internal JSONObject GuestSignUp(){
		JSONObject obj = new JSONObject ();
		JSONObject data = new JSONObject ();

		obj.AddField ("en", "GuestSignup");

		data.AddField ("sno", ArtoonConfig.Inst.deviceUniqueIdentifier);
		data.AddField ("ult", "guest");
        data.AddField ("det", ArtoonConfig.Inst.deviceTyp);
		data.AddField ("dids", PlayerPrefs.GetString("TOKEN"));
		data.AddField ("os",Constant.OSInfo);
		data.AddField ("ver",ArtoonConfig.Inst.Version);
		data.AddField ("speed",NetworkCheck.Speed);
		data.AddField ("dc",Constant.DVCInfo);

		obj.AddField ("data", data);
		return obj;
	}

	internal void ReadyForNewBall(){
		JSONObject obj = new JSONObject();
		JSONObject data = new JSONObject();

		obj.AddField("en","RFNB");
		obj.AddField("data",data);
		SocketHandler.Inst.SendData(obj);
	}
}
