//[DGR] ANTICHEAT No support added yet
//using CodeStage.AntiCheat.ObscuredTypes;
using FGOL.Server;
using System;
using System.Collections.Generic;

public class UserPersistenceSystem : PersistenceSystem
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

    private int m_numGameLoops = 0;

    public int numGameLoops
    {
        get { return m_numGameLoops; }
        set { m_numGameLoops = value; }
    }    
    
    public int timePlayed = 0;
    
    public bool isPirate = false;
    //[DGR] ANTICHEAT: No support added yet
    //public ObscuredBool isHacker = false;
    //public ObscuredBool isCheater = false;
    public bool isHacker = false;
    public bool isCheater = false;

    public bool isBadUser
    {
        get { return (isHacker || isCheater || isPirate); }
    }
    //reason why user was flagged as cheater/pirate/hacker
    public int flaggedReason = 0;

    public bool iapPurchaseMade = false;    

    public bool bannedFromLiveEvents = false;
    
    public UserPersistenceSystem()
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
        isPirate = false;
        isHacker = false;
        isCheater = false;
        flaggedReason = 0;
        bannedFromLiveEvents = false;
        iapPurchaseMade = false;                             
    }

    public override void Load()
    {
        try
        {
            m_gameDataID = GetInt("GameDataID", -1);
            timePlayed = GetInt("TimePlayed");
            m_hasBeenIngame = GetBool("HasBeenIngame", false);
            m_numGameLoops = GetInt("NumGameLoops", 0);            
            isPirate = GetBool("IsPirate", false);
            isHacker = GetBool("IsHacker", false);
            isCheater = GetBool("IsCheater", false);
            flaggedReason = GetInt("FlaggedReason", 0);
            bannedFromLiveEvents = GetBool("Banned");
            iapPurchaseMade = GetBool("IapPurchaseMade");            
        }
        catch (Exception e)
        {
            Debug.LogError("UserPersistenceSystem (Load) :: Exception - " + e);
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
        SetBool("IsPirate", isPirate);
        SetBool("IsHacker", isHacker);
        SetBool("IsCheater", isCheater);
        SetInt("FlaggedReason", flaggedReason);
        SetBool("Banned", bannedFromLiveEvents);
        SetBool("IapPurchaseMade", iapPurchaseMade);        
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
}
