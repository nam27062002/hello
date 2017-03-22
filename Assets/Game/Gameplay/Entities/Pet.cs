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

	protected override void Awake() {
		base.Awake();
		InitFromDef();
	}



	void OnEnable()
	{
		Messenger.AddListener(GameEvents.GAME_ENDED, OnEnded);
	}

	void OnDisable()
	{
		Messenger.RemoveListener(GameEvents.GAME_ENDED, OnEnded);
	}



	void OnEnded()
	{
		gameObject.SetActive(false);
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
