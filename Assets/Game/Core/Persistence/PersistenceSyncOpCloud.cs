public class PersistenceSyncOpCloud : PersistenceSyncOp
{
	protected override void ExtendedPerform()
	{
		OnPerformDone();
	}

	protected override void ExtendedDiscard() {}

    protected override PersistenceStates.ESyncResult ExtendedCalculateResult()
    {
        return PersistenceStates.ESyncResult.Success;
    }
}

