//[DGR] ANTICHEAT No support added yet
//using CodeStage.AntiCheat.ObscuredTypes;
//[DGR] No support added yet
//using Definitions;
using FGOL.Save;
using FGOL.Server;
using System;
using System.Collections;
using System.Collections.Generic;

public class UserSaveSystem : SaveSystem
{
    private int m_gameDataID = -1;

    public int gameDataID
    {
        set
        {
            m_gameDataID = value;
        }
    }

    private bool m_hasBeenIngame = false;

    public bool hasBeenIngame
    {
        get { return m_hasBeenIngame; }
        set { m_hasBeenIngame = value; }
    }

    private int m_unreadMissions = 0;

    public int unreadMissions
    {
        get { return m_unreadMissions; }
        set { m_unreadMissions = value; }
    }

    private int m_numGameLoops = 0;

    public int numGameLoops
    {
        get { return m_numGameLoops; }
        set { m_numGameLoops = value; }
    }

	private bool m_preRegistrationGiftReceived;
	public bool preRegistrationGiftReceived
	{
		get { return m_preRegistrationGiftReceived; }
		set { m_preRegistrationGiftReceived = value; }
	}

    private Dictionary<string, int> m_promoPackPurchaseCount = new Dictionary<string, int>();

    //[DGR] COMMUNITY: No support added yet
    //public Dictionary<string, CommunityPopup.RewardStates> m_communityRewardData = new Dictionary<string, CommunityPopup.RewardStates>();

    public int timePlayed = 0;

    public bool sharkSelectOnboarding = false;
	public bool levelSelectOnboarding = false;

    public Dictionary<int, bool> tierInfoPopupShown = new Dictionary<int, bool>();

	public bool isPirate = false;
    //[DGR] ANTICHEAT: No support added yet
    //public ObscuredBool isHacker = false;
    //public ObscuredBool isCheater = false;
    public bool isHacker = false;
    public bool isCheater = false;

    public bool isBadUser
	{
		get { return (isHacker || isCheater || isPirate);  }
	}
	//reason why user was flagged as cheater/pirate/hacker
	public int flaggedReason = 0;

	public bool iapPurchaseMade = false;

    public bool facebookInviteSent = false;

	public bool childFriendlyMode = false;

	public bool bannedFromLiveEvents = false;

    //Needs to be updated if other onboarding occurs in future
    public bool onBoardingComplete
    {
        get { return numGameLoops >= 2 && levelSelectOnboarding && sharkSelectOnboarding; }
    }

	public UserSaveSystem()
    {
        m_systemName = "User";

        Reset();
    }

    public override void Reset()
    {
        m_gameDataID = -1;
        timePlayed = 0;

		m_hasBeenIngame = false;
        m_numGameLoops = 0;
		m_preRegistrationGiftReceived = false;
        sharkSelectOnboarding = false;
		levelSelectOnboarding = false;
		isPirate = false;
		isHacker = false;
		isCheater = false;
		flaggedReason = 0;
		bannedFromLiveEvents = false;
        iapPurchaseMade = false;
        tierInfoPopupShown.Clear();
        facebookInviteSent = false;
        // [DGR] COMMUNITY: No support added yet
        //m_communityRewardData.Clear();
        m_promoPackPurchaseCount.Clear();
    }

    public override void Load()
    {                
        try
        {
            m_gameDataID = GetInt("GameDataID", -1);
            timePlayed = GetInt("TimePlayed");
            m_hasBeenIngame = GetBool("HasBeenIngame", false);
            m_numGameLoops = GetInt("NumGameLoops", 0);
            m_preRegistrationGiftReceived = GetBool("PreRegistrationGiftReceived", false);
            sharkSelectOnboarding = GetBool("SharkSelectOnboarding");
            levelSelectOnboarding = GetBool("LevelSelectOnboarding");
			isPirate = GetBool("IsPirate", false);
			isHacker = GetBool("IsHacker", false);
			isCheater = GetBool("IsCheater", false);
			flaggedReason = GetInt("FlaggedReason", 0);
			bannedFromLiveEvents = GetBool("Banned");
            iapPurchaseMade = GetBool("IapPurchaseMade");
            facebookInviteSent = GetBool("FacebookInviteSent");

            /*
            //[DGR] RULES: No supported yet
            IEnumerator iterator = gameDB.GetEnumerator<SharkTierData>();

            PushKey("TierInfoPopupShown");
            {
                while (iterator.MoveNext())
                {
                    SharkTierData data = iterator.Current as SharkTierData;

                    tierInfoPopupShown.Add(data.tier, GetBool(data.tier.ToString(), false));
                }
            }
            PopKey();
            */

            childFriendlyMode = GetBool("ChildFriendlyMode", false);

            /*
            //[DGR] COMMUNITY: No supported yet
            List<string> communityRewardKeys = GetCommunityRewardKeys();

            PushKey("CommunityRewards");
            {
                for (int i = 0; i < communityRewardKeys.Count; i++)
                {
                    PushKey(communityRewardKeys[i]);
                    {
                        string str = GetString("CurrentState");

                        if (!string.IsNullOrEmpty(str))
                        {
                            CommunityPopup.RewardStates state = (CommunityPopup.RewardStates)Enum.Parse(typeof(CommunityPopup.RewardStates), str);

                            if (m_communityRewardData.ContainsKey(communityRewardKeys[i]))
                            {
                                m_communityRewardData[communityRewardKeys[i]] = state;
                            }
                            else
                            {
                                m_communityRewardData.Add(communityRewardKeys[i], state);
                            }
                        }
                    }
                    PopKey();
                }
            }
            PopKey();

            PushKey("PurchasedPromoPacks");
            {
                //check free give away promo packs
                IEnumerator promoPackIterator = gameDB.GetEnumerator<PromoPackData>();
                while (promoPackIterator.MoveNext())
                {
                    PromoPackData data = promoPackIterator.Current as PromoPackData;
                    int count = GetInt(data.key, 0);
                    if (count > 0)
                    {
                        m_promoPackPurchaseCount.Add(data.key, count);
                    }
                }
            }
            PopKey();
            */
        }
        catch (Exception e)
        {
            Debug.LogError("UserSaveSystem (Load) :: Exception - " + e);
            throw new CorruptedSaveException(e);
        }

        //[DGR] RULES: No support added yet
        //GameDataManager.Instance.gameDataID = m_gameDataID;
    }

    public override void Save()
    {                 
         SetInt("GameDataID", m_gameDataID);
         SetInt("TimePlayed", timePlayed);
         SetBool("HasBeenIngame", m_hasBeenIngame);
         SetInt("NumGameLoops", m_numGameLoops);
         SetBool("PreRegistrationGiftReceived", m_preRegistrationGiftReceived);
         SetBool("SharkSelectOnboarding", sharkSelectOnboarding);
         SetBool("LevelSelectOnboarding", levelSelectOnboarding);
         SetBool("IsPirate", isPirate);
         SetBool("IsHacker", isHacker);
         SetBool("IsCheater", isCheater);
         SetInt("FlaggedReason", flaggedReason);
         SetBool("Banned", bannedFromLiveEvents);
         SetBool("IapPurchaseMade", iapPurchaseMade);
         SetBool("FacebookInviteSent", facebookInviteSent);

        /*
        //[DGR] RULES: No support added yet
        PushKey("TierInfoPopupShown");
         {
             foreach (var pair in tierInfoPopupShown)
             {
                 SetBool(pair.Key.ToString(), pair.Value);
             }
         }
         PopKey();
         */

         SetBool("ChildFriendlyMode", childFriendlyMode);

        /*
        //[DGR] COMMUNITY: No supported yet            
        PushKey("CommunityRewards");
        {
            List<string> communityRewardKeys = new List<string>(GetCommunityRewardKeys());

            for (int i = 0; i < communityRewardKeys.Count; i++)
            {
                PushKey(communityRewardKeys[i]);
                {
                    if (m_communityRewardData.ContainsKey(communityRewardKeys[i]))
                    {
                        if (!string.IsNullOrEmpty(m_communityRewardData[communityRewardKeys[i]].ToString()))
                        {
                            SetString("CurrentState", m_communityRewardData[communityRewardKeys[i]].ToString());
                        }
                    }
                    else
                    {
                        m_communityRewardData.Add(communityRewardKeys[i], CommunityPopup.RewardStates.Unclicked);
                        SetString("CurrentState", m_communityRewardData[communityRewardKeys[i]].ToString());
                    }
                }
                PopKey();
            }
        }
        PopKey();

        PushKey("PurchasedPromoPacks");
        {
            foreach (var pair in m_promoPackPurchaseCount)
            {
                SetInt(pair.Key, pair.Value);
            }
        }
        PopKey();
        */
    }

    public void UpdatePromoPacksPurchased(string promoPackID)
    {
        if (m_promoPackPurchaseCount.ContainsKey(promoPackID))
        {
            //If we already have this promo pack increase the count by 1
            m_promoPackPurchaseCount[promoPackID] += 1;
        }
        else
        {
            //We have never bought this promo pack before so add it with a count of 1
            m_promoPackPurchaseCount.Add(promoPackID, 1);
        }

        Save();
    }

    public bool HasPromoPackBeenPurchased(string promoPackID)
    {
        bool purchased = false;
        if (m_promoPackPurchaseCount.ContainsKey(promoPackID))
        {
            purchased = true;
        }

        return purchased;
    }

    public int GetPurchasedPromoPackCount(string promoPackID)
    {
        int count = 0;
        if (m_promoPackPurchaseCount.ContainsKey(promoPackID))
        {
            count = m_promoPackPurchaseCount[promoPackID];
        }

        return count;
    }

    private List<string> GetCommunityRewardKeys()
    {
        List<string> keyList = new List<string>();

        /*
        //[DGR] COMMUNITY: No support added yet
        IEnumerator communityEntriesEnumerator = gameDB.GetEnumerator<Definitions.CommunityData>();

        while (communityEntriesEnumerator.MoveNext())
        {
            CommunityData data = communityEntriesEnumerator.Current as CommunityData;

            keyList.Add(data.key);
        }
        */

        return keyList;
    }

    public override bool Upgrade()
    {
		bool upgraded = false;

		//if version less than 0.1.1
		if (version.CompareTo("0.1.1") < 0)
		{
			SetBool("IsHacker", false);
			SetBool("IsCheater", false);
			upgraded = true;
		}
		return upgraded;
    }

    public override void Downgrade()
    {
    }

#if !PRODUCTION
    public void DisableOnboarding()
	{
		m_hasBeenIngame = true;
		m_numGameLoops = 2;
		sharkSelectOnboarding = true;
		levelSelectOnboarding = true;
        facebookInviteSent = true;
        SaveFacade.Instance.Save(name);
	}
#endif
}
