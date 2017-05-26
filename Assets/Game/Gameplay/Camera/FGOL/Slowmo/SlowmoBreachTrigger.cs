// using FGOL;
using UnityEngine;

public class SlowmoBreachTrigger : SlowmoTrigger
{
	//------------------------------------------------------------
	// Inspector Variables:
	//------------------------------------------------------------

	[SerializeField]
	protected float m_minDuration = 3f;
	[SerializeField]
	protected float m_maxDuration = 5f;
	[SerializeField]
	protected float m_minAllowedDistanceBelowWaterLine = 0f;
	[SerializeField]
	protected float m_maxAllowedDistanceAboveWaterLine = 4f;
	[SerializeField]
	protected float m_absMinVelocityAllowed = 2f;
	[SerializeField]
	protected float m_absMinYVelocityAllowed = 4f;
	[SerializeField]
	protected bool m_onlyAscendingDirection = true;
	[SerializeField]
	protected bool m_triggerWhileInGR;

	//------------------------------------------------------------
	// Private Variables:
	//------------------------------------------------------------

	private bool m_externalStopRequested;

	//------------------------------------------------------------
	// Unity Lifecycle:
	//------------------------------------------------------------

	private new void Awake()
	{
		base.Awake();
		// get the square value so that when it's needed to be compared to the current velocity of the shark we can use sqrMagnitude of the Vector3 that is less expensive than magnitude (avoiding sqr root).
		m_absMinVelocityAllowed *= m_absMinVelocityAllowed;
	}

	private void Update()
	{
		if(m_inProgress && m_slowmo.inProgress)
		{
			float slowmoTime = m_slowmo.slowmoTime;

			// check if the slowmo needs to be stopped because called by an external entity.
			if(m_externalStopRequested && slowmoTime > m_minDuration)
			{
				m_externalStopRequested = false;
				// finalize the stop operation.
				StopSlowmo();
				return;
			}

			// check if the slowmo needs to be stopped because the max time has passed.
			if(slowmoTime > m_maxDuration)
			{
				// finalize the stop operation.
				StopSlowmo();
			}
		}
	}

	//------------------------------------------------------------
	// Private Methods:
	//------------------------------------------------------------

	private bool VerifyStartConditions()
	{
		// check if there is a valid player in the scene.
		DragonPlayer player = InstanceManager.player;
		DragonMotion playerMotion = player.GetComponent<DragonMotion>();
		DebugUtils.Assert(player != null, "No Virtual Player");

		ShowDebugMessage("<color=yellow><b>breach trigger</b></color> -> <color=green><b>start</b></color> -> evaluating conditions...");


		bool isGoldRushActive = player.IsFuryOn() || player.IsMegaFuryOn();

		// check if we can trigger the slowmo...
		// check if to trigger while in GR or MGR.
		// bool condition = m_triggerWhileInGR || (!m_triggerWhileInGR && !GoldRushController.Instance.IsGoldRushActive);
		bool condition = m_triggerWhileInGR || (!m_triggerWhileInGR && !isGoldRushActive);
		bool canTrigger = condition;
		ShowDebugMessage("<color=lightblue>trigger while in GR or MGR?</color> <color=white>-></color> " + m_triggerWhileInGR + " <color=lightblue>is in GR or MGR?</color> <color=white>-></color> " + isGoldRushActive + " <color=white>so</color> <color=" + (condition ? "green" : "red") + ">" + condition + "</color>");

		// the first condition is that the shark needs to have a certain absolute velocity.
		// condition = player.machine.velocity.sqrMagnitude > m_absMinVelocityAllowed;
		condition = playerMotion.velocity.sqrMagnitude > m_absMinVelocityAllowed;
		canTrigger = canTrigger && condition;
		ShowDebugMessage("<color=lightblue>has shark enough velocity?</color> <color=white>-></color> " + playerMotion.velocity.sqrMagnitude + " <color=white>of</color> " + m_absMinVelocityAllowed + " <color=white>so</color> <color=" + (condition ? "green" : "red") + ">" + condition + "</color> (please note that these values are squares)");

		// check the Y velocity of the shark.
		if(m_onlyAscendingDirection)
		{
			// it needs to be grater than the limit (ascending)
			condition = playerMotion.velocity.y > m_absMinYVelocityAllowed;
		}
		else
		{
			// it needs to be greater (ascending) or lower than the negative value.
			condition = playerMotion.velocity.y > m_absMinYVelocityAllowed || playerMotion.velocity.y < -m_absMinYVelocityAllowed;
		}
		canTrigger = canTrigger && condition;
		ShowDebugMessage("<color=lightblue>trigger only when ascending?</color> <color=white>-></color> " + m_onlyAscendingDirection + " <color=lightblue>is the Y velocity enough?</color> <color=white>-></color>" + playerMotion.velocity.y + " <color=white>so</color> <color=" + (condition ? "green" : "red") + ">" + condition + "</color>");


		// TODO (MALH) Recover this
		/*
		// check that the player position respect the water line it's ok.
		// it needs to be not less than the min allowed distance...
		condition = player.position.y >= GameInfo.WaterHeight - m_minAllowedDistanceBelowWaterLine;
		canTrigger = canTrigger && condition;
		ShowDebugMessage("<color=lightblue>is shark above the min allowed distance below the water?</color> <color=white>-></color> " + player.position.y + " <color=white>of</color> " + (GameInfo.WaterHeight - m_minAllowedDistanceBelowWaterLine) + " <color=white>so</color> <color=" + (condition ? "green" : "red") + ">" + condition + "</color>");
		
		// ...and not bigger than the max allowed distance.
		condition = player.position.y <= GameInfo.WaterHeight + m_maxAllowedDistanceAboveWaterLine;
		canTrigger = canTrigger && condition;
		ShowDebugMessage("<color=lightblue>is shark below the max allowed distance above the water?</color> <color=white>-></color> " + player.position.y + " <color=white>of</color> " + (GameInfo.WaterHeight + m_maxAllowedDistanceAboveWaterLine) + " <color=white>so</color> <color=" + (condition ? "green" : "red") + ">" + condition + "</color>");
		*/
		return canTrigger;
	}

	//------------------------------------------------------------
	// Abstract Method Implementations:
	//------------------------------------------------------------

	protected override bool InitializeSlowmoStart()
	{
		// reinitialise the external stop request flag.
		m_externalStopRequested = false;

		if(!VerifyStartConditions())
		{
			return false;
		}

		// play roar sfx anly if there is a valid player (if not something terribly wrong happened...).
		DragonPlayer player = InstanceManager.player;
		DebugUtils.Assert(player != null, "No Player");


		string roarType = "hsx_breach_roar_big";
		// set the type based on the dimensions of the player's shark.
		/* switch parameters:
		 *		hsx_breach_roar_small	-> from  0 to  5
		 *		hsx_breach_roar_med		-> from  5 to 18
		 *		hsx_breach_roar_big		-> from 18 on
		 */

		// TODO (MALH) Recover this
		/*
		float scale = player.cachedTransform.localScale.x;
		if(scale < 5f)
		{
			roarType = "hsx_breach_roar_small";
		}
		else if(scale < 18f)
		{
			roarType = "hsx_breach_roar_med";
		}
		// to be sure that we are getting the player gameobjet I'll take the one associated to the cached transform.
		GameObject m_gameObject = player.cachedTransform.gameObject;
		AudioManager.PostEvent(AudioManager.SharkEmote.Breach, Fabric.EventAction.SetSwitch, roarType, m_gameObject);
		AudioManager.PlaySfx(AudioManager.SharkEmote.Breach, m_gameObject);
		*/
		return true;
	}

	protected override bool InitializeSlowmoStop()
	{
		// in this case the trigger child will take care of the external request when the conditions will be verified.
		m_externalStopRequested = true;
		// stop the roar sfx.
		// TODO (MALH) Recover this
		// AudioManager.StopSfx(AudioManager.SharkEmote.Breach, GameInfo.activePlayer.cachedTransform.gameObject);
		return false;
	}

	//------------------------------------------------------------
	// Virtual Methods Overriding:
	//------------------------------------------------------------

	protected override void EnablePostProcessEffects()
    {
        if (!this.enabled)
            return;
		// TODO (MALH) Recover this
        // GameInfo.gameCamera.postProcessEffectsManager.StartVignetingEffect();
	}

	protected override void DisablePostProcessEffects()
    {
        if (!this.enabled)
            return;
		// TODO (MALH) Recover this
        // GameInfo.gameCamera.postProcessEffectsManager.StopVignetingEffect();
	}
}