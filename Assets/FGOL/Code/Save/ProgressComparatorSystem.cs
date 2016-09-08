using FGOL.Save;

public class ProgressComparatorSystem : SaveSystem
{
    public int dragonsOwned = 0;
    public int missionsCompleted = 0;
    public int timePlayed = 0;
    public bool iapPurchaseMade = false;
    public int lastModified = 0;
    public string lastDevice = "";    

    public ProgressComparatorSystem()
    {
    }

    public override void Reset()
    {
        dragonsOwned = 0;
        missionsCompleted = 0;
        timePlayed = 0;
        iapPurchaseMade = false;
    }

    public override void Load()
    {
        if (UserProfile == null)
        {
            UserProfile = new UserProfile();
        }
        UserProfile.data = m_saveData;
        UserProfile.Load();

        dragonsOwned = UserProfile.GetNumOwnedDragons();
        missionsCompleted = (UserProfile.userMissions == null) ? 0 : UserProfile.userMissions.missionsCompletedAmount;
        timePlayed = UserProfile.timePlayed;
        iapPurchaseMade = UserProfile.iapPurchaseMade;                
    }

    public override void Save()
    {
        throw new System.NotImplementedException();
    }

    public override bool Upgrade()
    {
        throw new System.NotImplementedException();
        //return false;
    }

    public override void Downgrade()
    {
        throw new System.NotImplementedException();
    }
    
    public UserProfile UserProfile { get; set; }  
}
