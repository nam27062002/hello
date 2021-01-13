using UnityEngine;

public class SlowmoDeathTrigger : SlowmoTrigger
{

	//------------------------------------------------------------
	// Inspector Variables:
	//------------------------------------------------------------

	[SerializeField]
	protected float m_duration = 3f;

	//------------------------------------------------------------
	// Unity Lifecycle:
	//------------------------------------------------------------

	private void Update()
	{
		if(m_inProgress && m_slowmo.inProgress)
		{
			// check if the slowmo needs to be stopped because the max time has passed.
			if(m_slowmo.slowmoTime > m_duration)
			{
				// trigger the stop operations.
				StopSlowmo();
			}
		}
	}

	//------------------------------------------------------------
	// Abstract Method Implementations:
	//------------------------------------------------------------

	protected override bool InitializeSlowmoStart()
	{
		// in this particular case if the shark is triggering its death we will always be able to start the slowmo in it's not in progress.
		return true;
	}

	protected override bool InitializeSlowmoStop()
	{
		// in this particular case if the shark has triggered its death nothing else can stop the slowmo apart the timeout.
		return false;
	}

	//------------------------------------------------------------
	// Virtual Methods Overriding:
	//------------------------------------------------------------

	protected override void EnablePostProcessEffects()
    {
        if (!this.enabled)
            return;

		// m_postProcessAnimators[0].StartAnimationLoop(); // TODO (MALH) Recover this
	}

	protected override void DisablePostProcessEffects()
    {
        if (!this.enabled)
            return;

		// m_postProcessAnimators[0].StopAnimationLoop(); // TODO (MALH) Recover this
	}
}