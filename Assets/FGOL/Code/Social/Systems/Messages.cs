//[DGR] No support added yet
//using Definitions;
using FGOL.ThirdParty.MiniJSON;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Messages
{
	public List<Message> m_waitingMessages = new List<Message>();
	private enum MessageFlags
	{
		None = 0,
		Gift = 1,
		InviteReward = 2
	};

	public int GetMessageNotificationsCount
	{
		get
		{
			int numTypes = 0;
			if (Application.internetReachability != NetworkReachability.NotReachable)
			{
				int messageTypeFlags = 0;
				//use bitwise flags, should make an enum
				foreach (Message msg in m_waitingMessages)
				{
					if (msg.GetType() == typeof(GiftMessage)
						&& (messageTypeFlags & (int)MessageFlags.Gift) == 0)
					{
						numTypes++;
						messageTypeFlags &= (int)MessageFlags.Gift;
					}
					else if (msg.GetType() == typeof(InviteRewardMessage)
						&& (messageTypeFlags & (int)MessageFlags.InviteReward) == 0)
					{
						numTypes++;
						messageTypeFlags &= (int)MessageFlags.InviteReward;
					}
				}
			}
            return numTypes;
        }
	}


    public class Message
    {
        public string messageID;
    }

    public class InviteRewardMessage : Message
    {
        public string friendName;

        public InviteRewardMessage(string messageID, string name)
        {
            this.messageID = messageID;
            friendName = name;
        }
    }

	public class GiftMessage : Message
	{
		public string friendName;

		public GiftMessage(string messageID, string name)
		{
			this.messageID = messageID;
			friendName = name;
		}
	}

    public void CheckMessagesOnResume()
    {
        CheckMessages(delegate (Message[] messages) 
		{
            //[DGR] No support added yet
			// this popup could break the flow of the app. we show it not on top of other popups and not if the tweens for the transitions between main states is not in progress.
			/*if (messages != null && !(GameStateMachine.Instance.GetCurrentState() is PopupState) && UIPlayTween.playTweensActive == 0)
            {
                Debug.Log("Message (CheckMessagesOnResume) :: Received Messages - " + messages.Length);
				m_waitingMessages.Clear();
				m_waitingMessages.AddRange(messages);
            }*/
        });
    }

	public bool DisplayInviteRewardMessagePopup( Action onClosePopup = null )
	{
        //[DGR] No support added yet
		/*if (Application.internetReachability != NetworkReachability.NotReachable)
		{
			IncentivisedInviteData rewardData = GameDataManager.Instance.gameDB.GetItem<IncentivisedInviteData>(IncentivisedInviteData.KeyInviteAccepted);
			int rewards = 0;
			string firstUser = null;
			string secondUser = null;
			int numFriends = 0;
			List<string> messageIDs = new List<string>();
			foreach (Message message in m_waitingMessages)
			{
				if (message.GetType() == typeof(InviteRewardMessage))
				{
					InviteRewardMessage invmsg = message as InviteRewardMessage;
					messageIDs.Add(invmsg.messageID);
					if (firstUser == null)
					{
						firstUser = invmsg.friendName;
					}
					else if (secondUser == null)
					{
						secondUser = invmsg.friendName;
					}
					numFriends++;
					rewards += rewardData.value;
				}
			}

			if (numFriends > 0)
			{
				InviteRewardPopup.ShowInviteRewardPopup(firstUser, secondUser, numFriends, rewardData.type, rewards, delegate (Action<bool> onClear)
				{
					ClearMessages(messageIDs.ToArray(), delegate (bool success)
					{
						onClear(success);
						if (onClosePopup != null)
						{
							onClosePopup();
						}
					});
				});
			}
			return numFriends > 0;
		}*/
		return false;
    }

	public bool DisplayGiftMessagePopup()
	{
        //[DGR] No support added yet
        /*
		if (Application.internetReachability != NetworkReachability.NotReachable)
		{
			GiftingData rewardData = GameDataManager.Instance.gameDB.GetItem<GiftingData>(GiftingData.KeyGiftSent);
			int rewardValue = rewardData.amount;

			int rewards = 0;
			string firstUser = null;
			string secondUser = null;
			int numFriends = 0;
			List<string> messageIDs = new List<string>();
			foreach (Message message in m_waitingMessages)
			{
				if (message.GetType() == typeof(GiftMessage))
				{
					GiftMessage giftmsg = message as GiftMessage;
					messageIDs.Add(giftmsg.messageID);
					if (firstUser == null)
					{
						firstUser = giftmsg.friendName;
					}
					else if (secondUser == null)
					{
						secondUser = giftmsg.friendName;
					}
					numFriends++;
					rewards += rewardValue;
				}
			}

			if (numFriends > 0)
			{
				GiftRewardPopup.ShowGiftRewardPopup(firstUser, secondUser, numFriends, rewardData.currency, rewards, delegate (Action<bool> onClear)
				{
					ClearMessages(messageIDs.ToArray(), delegate (bool success)
					{
						onClear(success);
					});

				});
			}
			return numFriends > 0;
		}*/
		return false;
	}

	public void CheckMessages(Action<Message[]> onCheckMessage)
    {
        //[DGR] No support added yet
        /*
        string[] allowedStates = new string[] { "FrontEnd", "Shop", "SharkSelect", "SharkViewer", "LevelSelect" };

        if (Array.IndexOf<string>(allowedStates, GameStateMachine.Instance.GetCurrentState().name) >= 0)
        {
            SocialManagerUtilities.CheckConnectionAuth(delegate (SocialManagerUtilities.ConnectionState state)
            {
                if (state == SocialManagerUtilities.ConnectionState.OK)
                {
                    SocialFacade.Instance.GetFriends(SocialManager.GetSelectedSocialNetwork(), delegate (Dictionary<string, string> friends)
                    {
                        string friendIDs = null;

                        if (friends != null)
                        {
                            foreach (KeyValuePair<string, string> pair in friends)
                            {
                                if (friendIDs != null)
                                {
                                    friendIDs += ",";
                                }
                                string socialID = SocialManagerUtilities.GetPrefixedSocialID(pair.Key);

                                friendIDs += socialID;
                            }
                        }

                        if (!string.IsNullOrEmpty(friendIDs))
                        {
                            SocialFacade.Instance.GetProfileInfo(SocialManager.GetSelectedSocialNetwork(), delegate (Dictionary<string, string> profileInfo)
                            {
                                if (profileInfo != null && profileInfo.ContainsKey("name"))
                                {
                                    HSXServer.Instance.CheckMessages(profileInfo["name"], friendIDs, delegate (Dictionary<string, object> rawMessages)
                                    {
                                        if (rawMessages != null)
                                        {
                                            Debug.Log("Messages (CheckMessages) :: Received messages - " + rawMessages.Count);

                                            List<Message> messages = new List<Message>();

                                            foreach (KeyValuePair<string, object> pair in rawMessages)
                                            {
                                                Dictionary<string, object> message = Json.Deserialize(pair.Value as string) as Dictionary<string, object>;

                                                if (message != null)
                                                {
                                                    if (message.ContainsKey("type"))
                                                    {
                                                        string type = message["type"] as string;

                                                        switch (type)
                                                        {
                                                            case "InviteReward":
                                                                messages.Add(new InviteRewardMessage(pair.Key, message["name"] as string));
                                                                break;

															case "Gift":
																messages.Add(new GiftMessage(pair.Key, message["name"] as string));
																break;
														}
                                                    }
                                                }
                                            }

                                            onCheckMessage(messages.ToArray());
                                        }
                                        else
                                        {
                                            onCheckMessage(null);
                                        }
                                    });
                                }
                                else
                                {
                                    onCheckMessage(null);
                                }
                            });
                        }
                        else
                        {
                            onCheckMessage(null);
                        }
                    });
                }
                else
                {
                    onCheckMessage(null);
                }
            });
        }
        else*/
        {
            onCheckMessage(null);
        }
    }

	public void SendMessage(string socialID, string messageType, Action<bool> onSendMessage)
	{
        // [DGR] Not supported yet
		/*
        SocialFacade.Instance.GetProfileInfo(SocialManager.GetSelectedSocialNetwork(), delegate (Dictionary<string, string> profileInfo)
		{
			if (profileInfo != null && profileInfo.ContainsKey("name") && profileInfo.ContainsKey("id"))
			{
				HSXServer.Instance.SendMessages(profileInfo["name"], socialID, messageType, onSendMessage);
			}
			else
			{
				onSendMessage(false);
            }
		});
        */
	}
		

    public void ClearMessages(string[] messagesIDsToClear, Action<bool> onClear)
    {
        // [DGR] Not supported yet
        /*
        //previously we weren't clearing waiting messages when we cleared them from the server.
        //if the server command fails, we'll still get them again next time we pull them from the server,
        //but this simply stops the user collecting them several times.
        //we do this regardless of internet connection
        if (messagesIDsToClear.Length > 0)
		{
			for (int mtc = 0; mtc < messagesIDsToClear.Length; mtc++)
			{
				for (int wm = 0; wm < m_waitingMessages.Count; wm++)
				{
					if (m_waitingMessages[wm].messageID == messagesIDsToClear[mtc])
					{
						m_waitingMessages.RemoveAt(wm);
						break;
					}
				}
			}
		}

		SocialManagerUtilities.CheckConnectionAuth(delegate (SocialManagerUtilities.ConnectionState state){
            if (state == SocialManagerUtilities.ConnectionState.OK)
            {
                if (messagesIDsToClear.Length > 0)
                {
                    HSXServer.Instance.ClearMessages(messagesIDsToClear, onClear);
                }
                else
                {
                    onClear(true);
                }
            }
            else
            {
                onClear(false);
            }
        });
        */
    }
}
