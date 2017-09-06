using System.Collections.Generic;
public class PersistenceSyncOpDebug : PersistenceSyncOp
{
	public class DebugData
	{
		public float Delay { get; set; }		
        public PersistenceStates.ESyncResult Result { get; set; }
        public string PersistenceAsString { get; set; }        

        public DebugData(float delay, PersistenceStates.ESyncResult result, string persistenceAsString)
		{
			Delay = delay;			
            Result = result;
			PersistenceAsString = persistenceAsString;
		}
	}

	public PersistenceSyncOp NormalOp { get; set; }
	private Queue<DebugData> DebugDatas { get; set; }
	private DebugData CurrentDebugData { get; set; }

	private float TimeLeftToFinish { get; set;}

	public PersistenceSyncOpDebug(PersistenceSyncOp normalOp)
	{
		ExtendedReset();
		NormalOp = normalOp;
	}

	protected override void ExtendedReset() 
	{
		TimeLeftToFinish = -1;
	}

	public override void Setup(EType type, PersistenceData data, bool isSilent)
	{
		base.Setup(type, data, isSilent);
		if (NormalOp != null)
		{
			NormalOp.Setup(type, data, isSilent);
		}
	}

	public void Debug_RegisterData(float delay, PersistenceStates.ESyncResult result, string persistenceAsString)
	{
		if (DebugDatas == null)
		{
			DebugDatas = new Queue<DebugData>();
		}

		DebugData data = new DebugData(delay, result, persistenceAsString);
		DebugDatas.Enqueue(data);
	}

	protected override void ExtendedPerform ()
	{
		if (DebugDatas != null && DebugDatas.Count > 0)
		{
			CurrentDebugData = DebugDatas.Dequeue ();
		} else
		{
			CurrentDebugData = null;
		}	

		if (CurrentDebugData == null)
		{
			if (NormalOp != null)
			{
				NormalOp.Perform();
			}
			else
			{
				Finish();
			}
		}
		else
		{
			if (CurrentDebugData.Delay == 0)
			{
				Finish ();
			} else
			{
				TimeLeftToFinish = CurrentDebugData.Delay;
			}
		} 
	}

	protected override void ExtendedDiscard()
	{
		if (CurrentDebugData == null)
		{
			if (NormalOp != null)
			{
				NormalOp.Discard();
			}
		}
		else
		{
			CurrentDebugData = null;
		}

		ExtendedReset();
	}

    protected override PersistenceStates.ESyncResult ExtendedCalculateResult()
    {
        PersistenceStates.ESyncResult result = PersistenceStates.ESyncResult.Success;

        if (CurrentDebugData == null)
        {           
            if (NormalOp != null)
            {
                NormalOp.CalculateResult();
                result = NormalOp.Result;
            }
        }
        else 
        {
            // If a persistence was passed then we need to check that persistence, otherwise we use the result configured
            if (string.IsNullOrEmpty(CurrentDebugData.PersistenceAsString))
            {
                result = CurrentDebugData.Result;
            }
            else
            {
                result = base.ExtendedCalculateResult();
            }
        }

        return result;
    }

	private void Finish()
	{
		if (CurrentDebugData != null)
		{
			if (Data != null)
			{
				switch (Type)
				{
					case EType.Load:
					{
						if (!string.IsNullOrEmpty(CurrentDebugData.PersistenceAsString))
						{
							Data.LoadFromString(CurrentDebugData.PersistenceAsString);						
						}						
					}
					break;

					case EType.Save:
					{
						if (!string.IsNullOrEmpty(CurrentDebugData.PersistenceAsString))
						{
							Data.LoadFromString(CurrentDebugData.PersistenceAsString);
							Data.Save(false);						
						}						
					}
					break;
				}
			}			
		}

		OnPerformDone();	
		ExtendedReset();
	}

	public override void Update ()
	{
        if (State == EState.Running)
        {
            if (CurrentDebugData == null)
            {
                if (NormalOp != null)
                {
                    NormalOp.Update();
                    if (NormalOp.State == EState.Ready)
                    {
                        Finish();
                    }
                }
            }
            else if (TimeLeftToFinish > -1)
            {
                TimeLeftToFinish -= UnityEngine.Time.deltaTime;
                if (TimeLeftToFinish <= 0)
                {
                    Finish();
                }
            }
        }
	}
}
