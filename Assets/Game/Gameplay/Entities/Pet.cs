using UnityEngine;
using System.Collections;
using UnityEngine.Serialization;

public class Pet : IEntity, IBroadcastListener {
	// Constants
	public const string GAME_PREFAB_PATH = "Game/Equipable/Pets/";
	public const string MENU_PREFAB_PATH = "UI/Menu/Pets/";

	// Exposed to inspector
	[PetSkuList]
	[SerializeField] private string m_sku;
	public override string sku { get { return m_sku; } }

	public bool CanExplodeMines
	{
		get;
		set;
	}

	public bool CanBreakCages
	{
		get;
		set;
	}

	public bool Charging
	{
		get;
		set;
	}

	protected override void Awake() {
		base.Awake();
		InitFromDef();
		Debug.Log(def.Get("powerup"));
		if ( def.Get("powerup").CompareTo("explode_mine") == 0 ){
			CanExplodeMines = true;
		}
		if ( def.Get("powerup").CompareTo("cage_breaker") == 0 ){
			CanBreakCages = true;
		}
		Charging = false;
	}

	void Start()
	{
        EntityManager.instance.RegisterPet(this);
	}

	void OnEnable()
	{
		Broadcaster.AddListener(BroadcastEventType.GAME_ENDED, this);
	}

	void OnDisable()
	{
		Broadcaster.RemoveListener(BroadcastEventType.GAME_ENDED, this);
	}

    private void OnDestroy() {
        if (EntityManager.instance != null) {
            EntityManager.instance.UnregisterPet(this);
        }
    }

    public void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo)
    {
        switch(eventType)
        {
            case BroadcastEventType.GAME_ENDED:
            {
                OnEnded();  
            }break;
        }
    }


	void OnEnded() {
        EntityManager.instance.UnregisterPet(this);
        gameObject.SetActive(false);
	}

	private void InitFromDef() {
		// Get the definition
		m_def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.PETS, sku);
		m_maxHealth = 1f;
	}
}
