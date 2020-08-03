public class PersistenceFacadeConfig
{            
    public PersistenceFacadeConfig()
    {
        Setup();
    }

    private PersistenceLocalDriver mLocalDriver;
	public PersistenceLocalDriver LocalDriver
    {
        get { return mLocalDriver;  }

        set
        {
            if (mLocalDriver != null)
            {
                mLocalDriver.Destroy();
            }

            mLocalDriver = value;
        }
    }

    private PersistenceCloudDriver mCloudDriver;
    public PersistenceCloudDriver CloudDriver
    {
        get { return mCloudDriver; }

        set
        {
            if (mCloudDriver != null)
            {
                mLocalDriver.Destroy();
            }

            mCloudDriver = value;
        }
    }

    protected virtual void Setup()
	{
		LocalDriver = new PersistenceLocalDriver();
		CloudDriver = new PersistenceCloudDriver();
		CloudDriver.Setup(LocalDriver);
	}

    public void Destroy()
    {
        if (LocalDriver != null)
        {
            LocalDriver.Destroy();
        }

        if (CloudDriver != null)
        {
            CloudDriver.Destroy();
        }
    }
}
