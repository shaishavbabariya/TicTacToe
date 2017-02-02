
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using Artoon.Cricket.Engine;

public class SocketEventReceiver : MonoBehaviour {
	void Awake(){
		SocketHandler.OnSocketResponse += ReceiveData;
	}
	void OnDisable(){
		//SocketHandler.OnSocketResponse -= ReceiveData;
	}

	internal void ReceiveData(JSONObject e){

		string en = GS.GetTrimmed(e.GetField("en"));
		switch (en) {
		case "user_data":
			SetUserData(e);
			break;
		case "updateCoins":
			if(SceneManager.GetActiveScene().name.Equals("Menu")){
				GS.Inst.userInfo.coins = GS.GetTrimmed(e.GetField("data").GetField("coins"));
				GS.Inst.CoinsCount = int.Parse(GS.Inst.userInfo.coins);
				UIHeader.Inst.UpdateCoins();
			}
			break;
		case "OFL": //my online friend list
			StartCoroutine(OnLineFBFrndz.Inst.SetOnlineFrndList (e));
			break;
		case "FRR": //friend request response
			OnLineFBFrndz.Inst.OnReceiveFriendRequestResponse();
			break;
		case "FPRR"://friend play request response
			FriendPlayReqResp(e);
			break;
		case "BFRR":
			if(SceneManager.GetActiveScene().name.Equals("Menu")){
				if(MainDashBoard.Inst != null){
					GS.Inst.BFRR_ID = e.GetField("data").GetField("ID").ToString().Trim(new char[]{'"'});
					if(!string.IsNullOrEmpty(GS.Inst.FPRR_ID)){
						if(!GS.Inst.BFRR_ID.Equals(GS.Inst.FPRR_ID)){
							MainDashBoard.Inst.ShowConformationMessage(AlertType.Busy);
						}
						GS.Inst.FPRR_ID = "";
					}else{
						MainDashBoard.Inst.ShowConformationMessage(AlertType.Busy);
					}

					switch(GS.GetTrimmed(e.GetField("data").GetField("type"))){
					case "1":
						Buddies.Inst.SetAway(false);
						break;
					}
					TossSelectionPool.Inst.ShowWaitingProcess(false);
					Loading.Inst.HideLodingIndicator();
					GS.Inst.SetPrevMatch(0);
				}
			}else{
				if(SceneManager.GetActiveScene().name.Equals("Cricket")){
//					if(HUDManager.Inst != null){
//						HUDManager.Inst.ShowBusyFrndAlert(e.GetField("data"));
//					}
				}
			}
			break;
		case "CFPRR"://Cancle frndrequest response
			if(SceneManager.GetActiveScene().name.Equals("Menu")){
				if(Boolean.Parse(e.GetField("data").GetField("cancel").ToString().Trim(new char[]{'"'}))){
					MainDashBoard.Inst.ShowConformationMessage(AlertType.CanclePlayReq);
					GS.Inst.SetPrevMatch(0);
				}

				switch(GS.GetTrimmed(e.GetField("data").GetField("type"))){
				case "1":
					if(Buddies.Inst != null){
						Buddies.Inst.SetAway(false);
					}
					break;
				}

				GS.Inst.BFRR_ID = "";
				GS.Inst.FPRR_ID = "";
				FrndReqAlertBox.Inst.HideAlert();
				Loading.Inst.HideLodingIndicator();
				TossSelectionPool.Inst.ShowWaitingProcess(false);
			}
			break;
		case "RE":
			//			if(SceneManager.GetActiveScene().name.Equals("Menu")){
			//				FrndReqAlertBox.Inst.ShowAlert(e.GetField("data"));
			//			}
			break;
		case "BOLR":
			if(SceneManager.GetActiveScene().name.Equals("Menu")){
				Buddies.Inst.ReceivedBlockedBuddiesDataFromServer(e);
			}
			break;
		case "BLR"://Buddies list response
			if(SceneManager.GetActiveScene().name.Equals("Menu")){
				Buddies.Inst.ReceivedGlobBuddiesDataFromServer(e);
			}
			break;
		case "FFLR"://FB friend list response
			if(SceneManager.GetActiveScene().name.Equals("Menu")){
				Buddies.Inst.ReceivedFBBuddiesDataFromServer(e);
			}
			break;
		case "COINS_STORER":
			if(SceneManager.GetActiveScene().name.Equals("Menu")){
				Store.Inst.ReceivedStoreDataFromServer(e);
			}
			break;
		case "PRR":
			if(SceneManager.GetActiveScene().name.Equals("Menu")){
				//bat amount to play the match
				TossSelectionPool.Inst.OnReceivedPlayingRequestResponse(e);
			}
			break;
		case "PD":
			if(SceneManager.GetActiveScene().name.Equals("Menu")){
				TossSelectionPool.Inst.OnReceivedPlayersData(e);
			}
			break;
		case "TSR"://Tos Selection Response
			if(SceneManager.GetActiveScene().name.Equals("Menu")){
				TossSelectionPool.Inst.OnReceivedTossSelection(e);
			}
			break;
		case "TSPR":
			if(SceneManager.GetActiveScene().name.Equals("Menu")){
				TossSelectionPool.Inst.OnReceivedTossSelectionProcessResponse(e);
			}
			break;
		case "FINALTABLER":
			if(SceneManager.GetActiveScene().name.Equals("Menu")){
				TossSelectionPool.Inst.OnReceiveFinalTableResponse(e.GetField("data"));
			}
			break;
		case "GCR": //retrive from server On choose batting or bawling
			if(SceneManager.GetActiveScene().name.Equals("Menu")){
				TossSelectionPool.Inst.OnReceiveGCR(e);
			}
			break;
		case "SGR":
			//gifr received alert
			break;
		case "BRR":
			if(SceneManager.GetActiveScene().name.Equals("Menu")){
				if(NotificationScreen.Inst.transform.localScale.x > 0){
					NotificationScreen.Inst.RequestDataToServer();
				}else{
					int buzz = int.Parse(GS.Inst.userInfo.Notific);
					buzz += 1;
					GS.Inst.userInfo.Notific = buzz.ToString();
					UIHeader.Inst.SetNotificationBuzz();
				}
			}
			break;
		case "BRJR":
		case "BRBR":
		case "IDR":
		case "UBR":
		case "BAR":
			if(GS.Inst.CellActionHandler != null)GS.Inst.CellActionHandler.OnSuccess(en);
			break;
		case "CACR":
		case "PCCNFMR":
			if(GS.Inst.CellActionHandler != null)GS.Inst.CellActionHandler.OnSuccess(e.ToString());
			break;
		case "HTR":
			if(e.GetField("data").HasField("bat")){
				SettingScreen.Inst.OnChangeHandType();
			}else{
				if(BowlingController.Inst != null){
					//BowlingController.Inst.OnReceiveHTR(e.GetField("data"));
				}
			}
			break;
		case "INFOR":
			if(SceneManager.GetActiveScene().name.Equals("Menu")){
				ProfileScreen.Inst.ReceivedProfileInfoFromServer (e);
			}
			break;
		case "AVATARR":
			if(SceneManager.GetActiveScene().name.Equals("Menu")){
				AvatarScreen.Inst.ReceivedAvatarDataFromServer(e);
			}
			break;
		case "UPDATEAPROFILER":
			if(SceneManager.GetActiveScene().name.Equals("Menu")){
				AvatarScreen.Inst.ReceivedProfileUpdateFromServer();
			}
			break;
		case "CLR":
			if(SceneManager.GetActiveScene().name.Equals("Cricket")){
				LiveEventHandler.Inst.Received (e.GetField ("data"));
			}
			break;
		case "HTHR":
			if(Boolean.Parse(GS.GetTrimmed(e.GetField("data").GetField("dpf")))){
				if(SceneManager.GetActiveScene().name.Equals("Cricket")){
					GS.Inst.isResultDecrared = false;
					GameResult.Inst.OnCheckFinalStatus(e.GetField("data"));
				}else{
					GS.Inst.isResultDecrared = true;
					GS.Inst.ResData = new JSONObject();
					GS.Inst.ResData = e.GetField("data");
					GS.Inst.LoadNextScene("Cricket");
				}
			}
			break;
		case "LBF":
			if(SceneManager.GetActiveScene().name.Equals("Menu")){
				string str = "";
				if (e.GetField ("type") != null) {
					str = GS.GetTrimmed(e.GetField ("type"));
				}
				if (str == "wic") {
					Leaderboard.Inst.ReceivedLBFriendzWicketDataFromServer(e);
				} else {
					Leaderboard.Inst.ReceivedLBFriendzRunDataFromServer(e);
				}
			}
			break;
		case "FCR":
			if(SceneManager.GetActiveScene().name.Equals("Menu")){
				GS.Inst.userInfo.Frndz = GS.GetTrimmed(e.GetField("data").GetField("friend"));
				UIHeader.Inst.UpdateFriendCount();
			}
			break;
		case "LBG":
			if(SceneManager.GetActiveScene().name.Equals("Menu")){
				string str1 = "";
				if (e.GetField ("type") != null) {
					str1 = GS.GetTrimmed(e.GetField ("type"));
				}
				if (str1 == "wic") {
					Leaderboard.Inst.ReceivedLBGlobWicketDataFromServer(e);
				} else {
					Leaderboard.Inst.ReceivedLBGlobRunDataFromServer(e);
				}
			}
			break;
		case "NEC":
			//if not enough bats
			if(SceneManager.GetActiveScene().name.Equals("Cricket")){
				GS.Inst.isReceivedNotEnoughBat = true;
				GS.Inst.LoadNextScene("Menu");
			}else{
				GS.Inst.PrevScreen = UIScreen.Dashboard;
				MainDashBoard.Inst.ShowNotEnoughCoins();
			}
			break;
		case "NLR":
			if(SceneManager.GetActiveScene().name.Equals("Menu")){
				NotificationScreen.Inst.ReceivedNotificationDataFromServer (e);
			}
			break;
		case "MESSAGER":
			if(SceneManager.GetActiveScene().name.Equals("Menu")){
				Chat.Inst.ReceivedChatDataFromServer(e);
			}
			break;
		case "SMR":
			if(SceneManager.GetActiveScene().name.Equals("Menu")){
				CheckOnReceiveNewInboxMsg(e);
			}
			break;
		case "IBC":
			if(SceneManager.GetActiveScene().name.Equals("Menu")){
				if(Inbox.Inst.transform.localScale.x > 0){
					Inbox.Inst.RequestDataToServer();
				}else{
					int c = int.Parse(GS.Inst.userInfo.Inbox);
					GS.Inst.userInfo.Inbox = (c+1).ToString();
					UIHeader.Inst.SetInboxBuzz();
				}
			}
			break;
		case "INBOXR":
			Inbox.Inst.ReceivedInboxDataFromServer (e);
			break;
		case "IBCU":
			GS.Inst.userInfo.Inbox = GS.GetTrimmed(e.GetField("data").GetField("inboxcount"));
			UIHeader.Inst.SetInboxBuzz();
			break;
		case "timer":
			if(SceneManager.GetActiveScene().name.Equals("Cricket")){
				LiveEventHandler.Inst.OnReceiveNewBall(e.GetField ("data"));
			}
			break;
		case "TUT":
			if(SceneManager.GetActiveScene().name.Equals("Cricket")){
				LiveEventHandler.Inst.OnTimeOut(e.GetField ("data"));
			}
			break;
		case "FS":
			if(SceneManager.GetActiveScene().name.Equals("Cricket")){
				//LiveEventHandler.Inst.OnReceivedLiveScore(e.GetField("data").GetField("liveScore"));
//				CricketEventManager.SetCamera(CameraTAG.NONE);
//				CricketEventManager.DisplayGroundView();
			}
			break;
		case "NOTIFICATIONR":
			if(SceneManager.GetActiveScene().name.Equals("Menu")){
				SettingScreen.Inst.OnSetPushNotification();
			}
			break;
		case "RLRR":
			OnRejoinPrevMatch(e.GetField("data"));
			break;
		case "REMATCHC":
			if(SceneManager.GetActiveScene().name.Equals("Cricket")){
				ReMatch.Inst.OnReplayByOpp(-1);
			}
			break;
		case "REMATCHR":
			if(SceneManager.GetActiveScene().name.Equals("Cricket")){
				ReMatch.Inst.OnReplayByOpp(1);
			}
			break;
		case "PRCR":
			//cancle current match
			GS.Inst.SetPrevMatch(0);
			if(SceneManager.GetActiveScene().name.Equals("Cricket")){
				GS.Inst.isCanRejoin = false;
				GS.Inst.isQuitManually = true;
				GS.Inst.LoadNextScene("Menu");
			}
			if(SceneManager.GetActiveScene().name.Equals("Menu")){
				TossSelectionPool.Inst.CloseScreen();
				MainDashBoard.Inst.OnOpenScreen();
				Loading.Inst.HideLodingIndicator();
			}
			break;
		case "AnotherLogin":
			CheckAnotherLogin();
			break;
		case "INC":
			if(SceneManager.GetActiveScene().name.Equals("Cricket")){
				HUDManager.Inst.LoadingInstruction.text = "";
				GameResult.Inst.OnSetFirstInningStatus(e.GetField("data"));
				GS.Inst.SetRobotProbInfo(e.GetField("data").GetField("ROBOT_LEVEL"));
			}
			break;
		case "CBR":
			if(SceneManager.GetActiveScene().name.Equals("Menu")){
				MainDashBoard.Inst.OnReceiveBatsInfoFromServer(e);
			}
			break;
		case "BCTR":
			TimerController.Inst.OnReceiveCollectableBatsFromServer(e);
			break;
		case "RC":
			Loading.Inst.ShowLodingIndicator();
			if(SceneManager.GetActiveScene().name.Equals("Menu")){
				LoginProcess.Inst.CheckAndLogin();
				UIHeader.Inst.SetHeader(1);
				UIHeader.Inst.ClearUIScreens();
				MainDashBoard.Inst.OnOpenScreen();
			}
			break;
		case "Mmr":
			//Show maintanence screen
			if(!TimerController.Inst.isRunningMMR){
				TimerController.Inst.OnReceiveMMR(e.GetField("data"));
			}
			break;
		case "Mmn":
			//Show maintanence alert
			if(!TimerController.Inst.isRunningMMN){
				TimerController.Inst.OnReceiveMMN(e.GetField("data"));
			}
			break;
		case "config":
			
			ArtoonConfig.Inst.robotDress = new List<List<string>>();
			JSONObject list = e.GetField("data").GetField("rdc");

			//**************** Opponent Dress config  **********************
			for(int i = 0;i<list.Count;i++){
				List<string> dresscode = new List<string>();
				for(int j = 0; j< list[i].Count;j++){
					dresscode.Add(list[i][j].ToString());
				}
				ArtoonConfig.Inst.robotDress.Add(dresscode);
			}
			//**************************************
			if(GS.GetTrimmed(e.GetField("data").GetField("MAINTAINANCE_MODE")).Equals("true")){
				if(!TimerController.Inst.isRunningMMN){
					TimerController.Inst.OnReceiveConfig(e.GetField("data"));
				}
			}
			ArtoonConfig.Inst.S3URL = GS.GetTrimmed(e.GetField("data").GetField("S3_URL"));
			ArtoonConfig.Inst.COIN_COLLECT_INTERVAL = int.Parse(GS.GetTrimmed(e.GetField("data").GetField("COINS_COLLECT_TIMER")));
			ArtoonConfig.Inst.COIN_INTERVAL_COUNT = int.Parse(GS.GetTrimmed(e.GetField("data").GetField("COINS_COLLECT_COUNT")));
			ArtoonConfig.Inst.COIN_PER_INTERVEL = int.Parse(GS.GetTrimmed(e.GetField("data").GetField("TIME_INTERVAL_COINS")));

			if(e.GetField("data").HasField("ADD_FLAG")){
				AdMobAdsManager.Inst.AdsProbability = int.Parse(GS.GetTrimmed(e.GetField("data").GetField("ADD_POSSIBILITY")));
				AdMobAdsManager.Inst.isCanDisaply = Boolean.Parse(GS.GetTrimmed(e.GetField("data").GetField ("ADD_FLAG")));
			}
			break;
		case "FUR":
			if(SceneManager.GetActiveScene().name.Equals("Cricket")){
				//BowlingController.Inst.OnReceivedFUR(e);
			}
			break;
		case "CO":
			if(Loading.Inst != null){
				Loading.Inst.HideLodingIndicator();
			}
			break;
		case "SLRR":
			if(SceneManager.GetActiveScene().name.Equals("Cricket")){
				HighLightEffect.Inst.onReceiveScoreFromServer(e);
			}
			break;
		case "FEEDBACKR":
			FeedBackScreen.Inst.OnReceiveSuccess(e);
			break;
		case "NWCR":
			GS.Inst.isReceivedNwError = true;
			GS.Inst.NwErrorInfo = new JSONObject();
			GS.Inst.NwErrorInfo = e.GetField("data");
			GS.Inst.LoadNextScene("Menu");
			break;
		case "orderResp":
			if(SceneManager.GetActiveScene().name.Equals("Menu")){
				MainDashBoard.Inst.ShowPurchaseSuccess(GS.GetTrimmed(e.GetField("data").GetField("msg")));
			}
			Loading.Inst.HideLodingIndicator();
			break;
		case "REFBATR":
			int rb  = int.Parse(GS.GetTrimmed(e.GetField("data").GetField("inf")));
			if(rb > 0){
				MainDashBoard.Inst.ReferalDownloadAlert(rb);
			}
			break;
		case "MESSAGE_ADMINR":
			if(SceneManager.GetActiveScene().name.Equals("Menu")){
				AdminChat.Inst.ReceivedChatDataFromServer(e);
			}
			break;
		case "SM_ADMINR":
			if(SceneManager.GetActiveScene().name.Equals("Menu")){
				CheckOnReceiveNewAdminMsg(e);
			}
			break;
		case "MTR":
			ChallengeController.Inst.OnReceivedMTData(e.GetField("data"));
			break;
		case "TTR":
			ChallengeController.Inst.OnReceivedTTData(e.GetField("data"));
			break;
		case "PLRR":
			ChallengeController.Inst.OnReceivedGOData(e.GetField("data"));
			break;
		case "AVDR":
			PlayerCustomization.Inst.OnReceivePlayerCustomizationData(e.GetField("data"));
			break;
		case "BETLISTR":
			BetCoinsController.Inst.OnReceiveDataFromServer(e.GetField("data"));
			break;
		case "PCINFOR":
			PlayerCustomization.Inst.OnUpdateSuccess();
			break;
		}
	}


	void SetUserData(JSONObject e){

		JSONObject data = e.GetField ("data");
		if(GS.GetTrimmed(data.GetField("ult")).Equals("guest")){
			PlayerPrefs.SetString("SSN","GUEST");
		}else{
			PlayerPrefs.SetString("SSN","FB");
		}
		LoginProcess.Inst.Session = PlayerPrefs.GetString("SSN");

		GS.Inst.userInfo.ID 		= GS.GetTrimmed(data.GetField ("id"));
		GS.Inst.userInfo.name 		= GS.GetTrimmed(data.GetField ("un"));
		GS.Inst.userInfo.PicUrl 	= GS.GetTrimmed(data.GetField ("pp"));
		GS.Inst.userInfo.coins 		= GS.GetTrimmed(data.GetField ("coins"));
		GS.Inst.userInfo.Run 		= GS.GetTrimmed(data.GetField ("run"));
		GS.Inst.userInfo.Notific 	= GS.GetTrimmed(data.GetField ("noticount"));
		GS.Inst.userInfo.Inbox 		= GS.GetTrimmed(data.GetField ("inboxcount"));
		GS.Inst.userInfo.handTyp 	= (HandTyp)int.Parse(GS.GetTrimmed(data.GetField ("ht")));
		GS.Inst.ShareURL 			= GS.GetTrimmed(data.GetField ("rfl"));
		GS.Inst.userInfo.Frndz 		= GS.GetTrimmed(data.GetField ("friend"));
		GS.Inst.userInfo.RateUs 	= int.Parse(GS.GetTrimmed(data.GetField ("rateus")));
		GS.Inst.userInfo.AdminCount = int.Parse(GS.GetTrimmed(data.GetField ("admincount")));
		GS.Inst.userInfo.isFeedBackSent = Boolean.Parse(GS.GetTrimmed(data.GetField ("feedback")));
		GS.Inst.isCanRejoin 			= Boolean.Parse(GS.GetTrimmed(data.GetField ("rejoin")));

		//**************** User Dress config  **********************
		JSONObject obj1 = new JSONObject(JSONObject.Type.ARRAY);
		JSONObject pdc = data.GetField("pdc");
		for(int m = 0;m<pdc.Count;m++){
			obj1.Add(pdc[m]);
		}

		GS.Inst.P_rend_dress[1].SetColor("_Color",GS.Inst.HascodeToColor(GS.GetTrimmed(obj1[0])));
		GS.Inst.P_rend_dress[0].SetColor("_Color",GS.Inst.HascodeToColor(GS.GetTrimmed(obj1[2])));
		GS.Inst.P_rend_accessory[0].SetColor("_Color",GS.Inst.HascodeToColor(GS.GetTrimmed(obj1[3])));
		GS.Inst.P_rend_skin[0].SetColor("_Color",GS.Inst.HascodeToColor(GS.GetTrimmed(obj1[4])));

		//**************** Opponent Dress config  **********************
		int c = UnityEngine.Random.Range(0,ArtoonConfig.Inst.robotDress.Count);
		GS.Inst.O_rend_dress[1].SetColor("_Color",GS.Inst.HascodeToColor(ArtoonConfig.Inst.robotDress[c][0]));
		GS.Inst.O_rend_dress[0].SetColor("_Color",GS.Inst.HascodeToColor(ArtoonConfig.Inst.robotDress[c][2]));
		GS.Inst.O_rend_accessory[0].SetColor("_Color",GS.Inst.HascodeToColor(ArtoonConfig.Inst.robotDress[c][3]));
		GS.Inst.O_rend_skin[0].SetColor("_Color",GS.Inst.HascodeToColor(ArtoonConfig.Inst.robotDress[c][4]));
		//**************************************


		if(Boolean.Parse(GS.GetTrimmed(data.GetField("pushflag")))){
			PlayerPrefs.SetString("NOTI","ON");
		}else{
			PlayerPrefs.SetString("NOTI","OFF");
		}

		GS.Inst.CoinsCount = int.Parse(GS.Inst.userInfo.coins);

		if(GS.Inst.isCanRejoin)return;

		if(SceneManager.GetActiveScene().name.Equals("Cricket")){
			if(GS.Inst.gameMode != GameMode.Practice){
				GS.Inst.LoadNextScene("Menu");
			}else{
				return;
			}
		}else{
			if(SceneManager.GetActiveScene().name.Equals("Menu")){
				if(GS.Inst.gameMode == GameMode.Online){
					//GS.Inst.LoadNextScene("Menu");
					//GS.Inst.SetPrevMatch(0);
				}else{
				}
				Loading.Inst.HideLodingIndicator();
			}else{
				GS.Inst.LoadNextScene("Menu");
			}
		}
	}

	void FriendPlayReqResp(JSONObject e){
		if(SceneManager.GetActiveScene().name.Equals("Menu")){
			GS.Inst.FPRR_ID = GS.GetTrimmed(e.GetField("data").GetField("comp").GetField("_id"));
			if(!string.IsNullOrEmpty(GS.Inst.BFRR_ID)){
				if(!GS.Inst.FPRR_ID.Equals(GS.Inst.BFRR_ID)){
					GS.Inst.gameMode = (GameMode)int.Parse(GS.GetTrimmed(e.GetField("data").GetField("gt")));
					FrndReqAlertBox.Inst.ShowAlert(e.GetField("data"));
					if(TossSelectionPool.Inst.transform.localScale.x > 0){
						TossSelectionPool.Inst.ShowWaitingProcessBlank();
					}
				}
				GS.Inst.BFRR_ID = "";
			}else{
				GS.Inst.gameMode = (GameMode)int.Parse(GS.GetTrimmed(e.GetField("data").GetField("gt")));
				FrndReqAlertBox.Inst.ShowAlert(e.GetField("data"));
				if(TossSelectionPool.Inst.transform.localScale.x > 0){
					TossSelectionPool.Inst.ShowWaitingProcessBlank();
				}
			}
		}else{
			NotificationAlert.Inst.GenerateAlert(e,AlertType.PlayReq);
		}
	}

	void CheckOnReceiveNewAdminMsg(JSONObject e){
		if(AdminChat.Inst != null){
			string id = e.GetField("sender").ToString().Trim(new char[]{'"'});
			string typ = e.GetField("type").ToString().Trim(new char[]{'"'});
			string msg = e.GetField("data").ToString().Trim(new char[]{'"'});
			if(typ.Equals("sender")){
				AdminChat.Inst.AddNewUserCell(msg);
			}else{
				if(!id.Equals(GS.Inst.userInfo.ID)){
					AdminChat.Inst.AddNewOppCell(msg);
				}
			}
		}
	}


	void CheckOnReceiveNewInboxMsg(JSONObject e){
		if(Chat.Inst.transform.localScale.x > 0){
			string id = e.GetField("sender").ToString().Trim(new char[]{'"'});
			string typ = e.GetField("type").ToString().Trim(new char[]{'"'});
			string msg = e.GetField("data").ToString().Trim(new char[]{'"'});
			if(typ.Equals("sender")){
				Chat.Inst.AddNewUserCell(msg);
			}else{
				if(id.Equals(Chat.Inst.OppInfo.ID)){
					Chat.Inst.AddNewOppCell(msg);
					//send conformation call that i'm chatting with this opponent for inbox counter management at server.
					JSONObject obj = new JSONObject();
					obj.AddField("en","RMC");
					JSONObject data = new JSONObject();
					data.AddField("rUserId",Chat.Inst.OppInfo.ID);
					obj.AddField("data",data);
					SocketHandler.Inst.SendData(obj);
				}
			}
		}
	}

	/// <summary>
	/// Checks another login.
	/// </summary>
	void CheckAnotherLogin(){
		if(SceneManager.GetActiveScene().name.Equals("Cricket")){
			HUDManager.Inst.ShowAnotherLoginAlert();
		}else{
			MainDashBoard.Inst.ShowAnotherLoginAlert();
		}
	}

	/// <summary>
	/// Raises the rejoin previous match event.
	/// </summary>
	/// <param name="data">Data.</param>
	void OnRejoinPrevMatch(JSONObject data){

		Loading.Inst.HideLodingIndicator();

		int over = int.Parse(data.GetField("type").ToString().Trim(new char[]{'"'}));
		JSONObject uinfo = data.GetField("userInfo");
		GS.Inst.userInfo.name = GS.GetTrimmed(uinfo.GetField("un"));
		GS.Inst.userInfo.PicUrl = GS.GetTrimmed(uinfo.GetField("pp"));
		
		uinfo = data.GetField("compInfo");
		GS.Inst.OppInfo.Name = GS.GetTrimmed(uinfo.GetField("un"));
		GS.Inst.OppInfo.Url = GS.GetTrimmed(uinfo.GetField("pp"));
		
		GS.Inst.isTossSelPending = Boolean.Parse(GS.GetTrimmed(data.GetField ("tsp")));
		bool isRematch = Boolean.Parse(GS.GetTrimmed(data.GetField ("rmf")));

		GameMode mode = (GameMode)int.Parse(GS.GetTrimmed(data.GetField("gt")));

		GS.Inst.StartNewMatch(mode,over);
		GS.Inst.SetPrevMatch(1);

		//=================ReMatch=====
		if(isRematch && GS.Inst.isTossSelPending){
			GS.Inst.curInningInfo.isgotRematch = true;
			GS.Inst.LoadNextScene("Menu");
			return;
		}

		//======================
		if(GS.Inst.isTossSelPending){
			if(!SceneManager.GetActiveScene().name.Equals("Menu")){
				GS.Inst.LoadNextScene("Menu");
			}
			return;
		}
		
		GS.Inst.isCanRejoin = Boolean.Parse(GS.GetTrimmed(data.GetField ("rejoin")));
		
		if(!GS.Inst.isCanRejoin){
			GS.Inst.SetPrevMatch(0);
			return;
		}

		// Setting previous inning data
		if(data.HasField("fi_score")){
			GS.Inst.curInningInfo.isSecondInning = true;
			GS.Inst.prevInningInfo = new MatchData();
			GS.Inst.prevInningInfo.score = int.Parse(GS.GetTrimmed(data.GetField("fi_score")));
			GS.Inst.prevInningInfo.wicket = int.Parse(data.GetField("fi_wicket").ToString().Trim(new char[]{'"'}));
			GS.Inst.prevInningInfo.overs = int.Parse(data.GetField("fi_over").ToString().Trim(new char[]{'"'}));
			GS.Inst.prevInningInfo.isPlayingRobot = Boolean.Parse(data.GetField ("is_robot").ToString ().Trim (new char[] {'"'}));
		}
		//setting current inning data
		string recvIDBat = data.GetField ("batting").ToString ().Trim (new char[] {'"'});
		string recvIDBow = data.GetField ("bowling").ToString ().Trim (new char[] {'"'});
		
		
		GS.Inst.receivedFieldID  = int.Parse(data.GetField ("field").ToString ().Trim (new char[] {'"'}));
		if(data.HasField("nbt")){
			GS.Inst.receivedBallTimer = int.Parse(data.GetField ("nbt").ToString ().Trim (new char[] {'"'}));
		}

		if(GS.Inst.userInfo.ID != recvIDBat && GS.Inst.userInfo.ID != recvIDBow){
		}else{
			if(recvIDBat.Equals(GS.Inst.userInfo.ID)){
				GS.Inst.curInningInfo.isMyBatting = true;
				GS.Inst.userInfo.handTyp = (HandTyp)int.Parse(data.GetField("userInfo").GetField("htba").ToString().Trim(new char[]{'"'}));
				GS.Inst.curInningInfo.OppHandTyp = (HandTyp)int.Parse(data.GetField("compInfo").GetField("htba").ToString().Trim(new char[]{'"'}));
				GS.Inst.receivedBowlingSide = int.Parse(data.GetField("compInfo").GetField("htbo").ToString().Trim(new char[]{'"'}));
				GS.Inst.curInningInfo.OppID = recvIDBow;
				
			}else{
				GS.Inst.curInningInfo.isMyBatting = false;
				GS.Inst.curInningInfo.OppHandTyp = (HandTyp)int.Parse(data.GetField("compInfo").GetField("htba").ToString().Trim(new char[]{'"'}));
				GS.Inst.userInfo.handTyp = (HandTyp)int.Parse(data.GetField("userInfo").GetField("htba").ToString().Trim(new char[]{'"'}));
				GS.Inst.receivedBowlingSide = int.Parse(data.GetField("userInfo").GetField("htbo").ToString().Trim(new char[]{'"'}));
				GS.Inst.curInningInfo.OppID = recvIDBat;
			}
		}

		GS.Inst.curInningInfo.isPlayingRobot = Boolean.Parse(data.GetField ("is_robot").ToString ().Trim (new char[] {'"'}));

		if(GS.Inst.curInningInfo.isPlayingRobot){
			GS.Inst.SetRobotProbInfo(data.GetField("ROBOT_LEVEL"));
		}
		
		GS.Inst.curInningInfo.score = int.Parse(data.GetField("live_score").ToString().Trim(new char[]{'"'}));
		GS.Inst.curInningInfo.wicket = int.Parse(data.GetField("live_wicket").ToString().Trim(new char[]{'"'}));
		GS.Inst.curInningInfo.overs = int.Parse(data.GetField("live_over").ToString().Trim(new char[]{'"'}));
		GS.Inst.curInningInfo.overBalls = int.Parse(data.GetField("live_overball").ToString().Trim(new char[]{'"'}));
		GS.Inst.curInningInfo.isFreeHit = Boolean.Parse(data.GetField("fh").ToString().Trim(new char[]{'"'}));
		GS.Inst.curInningInfo.isRejoined = true;
		
		JSONObject Data = data.GetField ("ball");
		JSONObject balls = new JSONObject (JSONObject.Type.ARRAY);
		
		for (int i=0; i<Data.Count; i++) {
			balls.Add (Data [i]);
		}
		
		for(int i = 0;i<balls.Count;i++){
			GS.Inst.curInningInfo.balls.Add(balls[i].ToString().Trim(new char[]{'"'}));
		}

		GS.Inst.LoadNextScene("Cricket");
	}

	#region Application Pause
	int pc = 0;
	int fc = 0;
	float pt = 0;
	void OnApplicationPause(bool isPause){
		if(string.IsNullOrEmpty(PlayerPrefs.GetString("SSN")))return;
		if(isPause){
			//app goes on pause
			pt = Time.realtimeSinceStartup;
			pc++;
		}else{
			fc++;
			HandleOnAppFocus();
		}
	}
	void HandleOnAppFocus(){
		if(fc > 1){
			if(string.IsNullOrEmpty(PlayerPrefs.GetString("SSN")))return;
			GS.Inst.PauseTime = Time.realtimeSinceStartup - pt;
			if(SceneManager.GetActiveScene().name.Equals("Cricket")){
				if(GS.Inst.gameMode == GameMode.Online){
					GS.Inst.LoadNextScene("Wait");
					SocketHandler.Inst.Close();
				}else{
//					if(!CameraMovements.Inst.isStartupAnim){
//					HUDManager.Inst.OnOpenPauseScreen();
//					}
				}
			}
			if(SceneManager.GetActiveScene().name.Equals("Menu")){
				TimerController.Inst.GetCollectableBatsFromServer();
			}
		}
	}
	#endregion
}
