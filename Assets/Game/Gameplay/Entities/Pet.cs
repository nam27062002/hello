using UnityEngine;
using System.Collections;
using UnityEngine.Serialization;

public class Pet : IEntity {
	// Exposed to inspector
	[PetSkuList]
	[SerializeField] private string m_sku;
	public string sku { get { return m_sku; } }


	void Awake() {
		InitFromDef();
	}

	private void InitFromDef() {
		// Get the definition
		m_def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.PETS, sku);
		m_maxHealth = 1f;
	}
}
