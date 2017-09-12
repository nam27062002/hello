public class PersistenceFacadeConfig
{            
    public PersistenceFacadeConfig()
    {
        Setup();
    }      

	public PersistenceLocalDriver LocalDriver { get; set; }
	public PersistenceCloudDriver CloudDriver { get; set; }

	protected virtual void Setup()
	{
		LocalDriver = new PersistenceLocalDriver();
		CloudDriver = new PersistenceCloudDriver();
		CloudDriver.Setup(LocalDriver);
	}
}
