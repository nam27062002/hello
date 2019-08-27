using CodeStage.AntiCheat.ObscuredTypes;
using FGOL.Server;
using System;

public class UserPersistenceSystem : PersistenceSystem
{	
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
    
	public ObscuredBool isPirate = false;	// Not used at the momment
	// This flag is used when the players tries to modify the memory to change a variable. See AntiCheatsManager
	public ObscuredBool isHacker = false;	
	public ObscuredBool isCheater = false; // Not used at the momment

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
        //timePlayed = 0;

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
            PersistenceFacade.LogError("UserPersistenceSystem (Load) :: Exception - " + e);
            throw new CorruptedSaveException(e);
        }        
    }

    public override void Save()
    {
		SetInt("NumGameLoops", m_numGameLoops);		        		
        SetInt("TimePlayed", timePlayed);
        SetBool("HasBeenIngame", m_hasBeenIngame);
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
