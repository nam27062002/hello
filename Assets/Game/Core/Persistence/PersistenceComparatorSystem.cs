public class PersistenceComparatorSystem : PersistenceSystem
{
    public int dragonsOwned = 0;
    public int eggsCollected = 0;
    public int timePlayed = 0;
    public bool iapPurchaseMade = false;
    public int lastModified = 0;
    public string lastDevice = "";

    public PersistenceComparatorSystem()
    {
    }

    public override void Reset()
    {
        dragonsOwned = 0;
        eggsCollected = 0;
        timePlayed = 0;
        iapPurchaseMade = false;

        UserProfile.Reset();
    }

    public override void Load()
    {
        if (UserProfile == null)
        {
            UserProfile = new UserProfile();
        }
        UserProfile.data = m_persistenceData;
        UserProfile.Load();

        dragonsOwned = UserProfile.GetNumOwnedDragons();
        eggsCollected = UserProfile.eggsCollected;

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

    public UserProfile UserProfile { get; set; }
}
