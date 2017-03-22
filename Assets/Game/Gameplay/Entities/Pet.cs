using UnityEngine;
using System.Collections;
using UnityEngine.Serialization;

public class Pet : IEntity {
	// Constants
	public const string GAME_PREFAB_PATH = "Game/Equipable/Pets/";
	public const string MENU_PREFAB_PATH = "UI/Menu/Pets/";

	// Exposed to inspector
	[PetSkuList]
	[SerializeField] private string m_sku;
	public string sku { get { return m_sku; } }

	private MachineEatBehaviour m_eatBehaviour;

	protected override void Awake() {
		base.Awake();
		InitFromDef();
		m_eatBehaviour = GetComponent<MachineEatBehaviour>();
	}



	void OnEnable()
	{
		Messenger.AddListener(GameEvents.PLAYER_ENTERING_AREA, OnEnteringArea);
		Messenger.AddListener(GameEvents.PLAYER_LEAVING_AREA, OnLeavingArea);
	}

	void OnDisable()
	{
		Messenger.RemoveListener(GameEvents.PLAYER_ENTERING_AREA, OnEnteringArea);
		Messenger.RemoveListener(GameEvents.PLAYER_LEAVING_AREA, OnLeavingArea);
	}

	void OnEnteringArea()
	{
		if ( m_eatBehaviour != null )	
		{
			m_eatBehaviour.PauseEating();
		}
	}

	void OnLeavingArea()
	{
		if ( m_eatBehaviour != null )	
		{
			m_eatBehaviour.ResumeEating();
		}
	}

	private void InitFromDef() {
		// Get the definition
		m_def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.PETS, sku);
		m_maxHealth = 1f;
	}

	void Update()
	{
		base.CustomUpdate();
	}

	void FixedUpdate()
	{
		base.CustomFixedUpdate();
	}

	override public bool CanBeSmashed()
	{
		return false;
	}
}
