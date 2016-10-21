//--------------------------------------------------------------------------------
// BossCameraAffector.cs
//--------------------------------------------------------------------------------
// Stick this on a prefab to make it invoke boss camera.  Disable the component
// to stop it from taking effect.
//
// This class will now manage just the onscreen detect type behaviour.
// For the radius type behaviour, please, check the BossCameraAffectorRadius
// script.
//--------------------------------------------------------------------------------
// using FGOL;
using UnityEngine;

public class BossCameraAffector : MonoBehaviour
{
	//--------------------------------------------------------
	// Enumerations:
	//--------------------------------------------------------

	public enum DetectType
	{
		radius,
		onscreen
	}

	//--------------------------------------------------------
	// Inspector Variables:
	//--------------------------------------------------------

	[Range(0.0f, 20.0f)]
	public float frameWidthIncrement;
	public bool frameMeAndPlayer;
	public float radius;
	public DetectType detectType;
	[Range(0f, 1f)]
	public float m_minPerformanceRatingToUse = 0.4f;

	//--------------------------------------------------------
	// Private Variables:
	//--------------------------------------------------------

	private Entity m_spawnedObject = null;
	private bool m_firstTime = true;
	private bool m_notified;

	//--------------------------------------------------------
	// Public Properties:
	//--------------------------------------------------------
	public bool permanentlyDisabled	{ get; private set; }

	//--------------------------------------------------------
	// Unity Lifecycle:
	//--------------------------------------------------------

	protected void Awake()
	{
		// initialize the property.
		permanentlyDisabled = false;
		// disable this if the performances of the current device are not enough.
		// TODO (MALH) : Recover this
		/*
		if(DeviceQualityManager.devicePerformanceRating < m_minPerformanceRatingToUse)	
		{
			enabled = false;
			permanentlyDisabled = true;
		}
		*/
	}

	protected void OnDisable()
	{
		RemoveBossCam();
	}

	protected void OnDestroy()
	{
		RemoveBossCam();
	}

	protected void Update()
	{
		// leaving this check here to be absolutely sure that nothing will go wrong...
		if(permanentlyDisabled)
		{
			return;
		}

		if(m_firstTime)
		{
			LateInit();
			m_firstTime = false;
		}

		if(InstanceManager.player == null)
			return;

		if(!InstanceManager.player.IsAlive())
			return;

		switch(detectType)
		{
			case DetectType.onscreen:
				if(m_spawnedObject != null)
				{
					if(m_spawnedObject.isOnScreen)
					{
						if(!m_notified)
						{
							InstanceManager.gameCamera.NotifyBoss(this);
							m_notified = true;
						}						
					}
					else
					{
						if(m_notified)
						{
							InstanceManager.gameCamera.RemoveBoss(this);
							m_notified = false;
						}
					}

				}
				break;
		}
	}

	//--------------------------------------------------------
	// Private Methods:
	//--------------------------------------------------------

	private void LateInit()
	{
		if(detectType == DetectType.onscreen)
		{
			// TODO (MALH) : Recover this
			// relying on existence of SpawnedObject for now, use it to grab bounds
			Entity sp = GetComponent<Entity>();
			if(sp == null)
			{
				enabled = false;
			}
			else
			{
				m_spawnedObject = sp;
			}
		}
	}

	private void RemoveBossCam()
	{
		// remove itself from the boss cam only if it is enabled and notified, if not it's a job for the BossCameraAffectorRadius script.
		if(enabled && m_notified)
		{
			// TODO (MALH) : Recover this
			// make this check because it could happen that some entities (like treasure chests) could be destroyed/disabled
			// before to enter in the level, so, before the camera to be instantiated.
			if(InstanceManager.gameCamera != null)
			{
				InstanceManager.gameCamera.RemoveBoss(this);
			}
			m_notified = false;
		}
	}
}