using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrozenMaterialManager : Singleton<FrozenMaterialManager> {

	private Dictionary<Material, Material> m_frozenReferences;


	public static Material GetFrozenMaterialFor(Material _source) {
		return instance.__GetFrozenMaterialFor(_source);
	}

	public static void CleanFrozenMaterials(){
		if ( instance != null && instance.m_frozenReferences != null)
			instance.m_frozenReferences.Clear();
	}

	private Material __GetFrozenMaterialFor(Material _source) {
		if (m_frozenReferences == null) {
			m_frozenReferences = new Dictionary<Material, Material>();
		}

		Material frozenMat = null;
		if (m_frozenReferences.ContainsKey(_source)) {
			frozenMat = m_frozenReferences[_source];
		} else {
			frozenMat = CreateFrozenVersionOf(_source);
			m_frozenReferences[_source] = frozenMat;
		}

		return frozenMat;
	}

	private Material CreateFrozenVersionOf(Material _source) {
		Material frozenMat = new Material(_source);

		frozenMat.EnableKeyword(GameConstants.Materials.Keyword.FREEZE);
		frozenMat.SetColor(GameConstants.Materials.Property.FRESNEL_COLOR, new Color(114.0f / 255.0f, 248.0f / 255.0f, 255.0f / 255.0f, 255.0f / 255.0f));
		frozenMat.SetColor(GameConstants.Materials.Property.FRESNEL_COLOR_2, new Color(186.0f / 255.0f, 144.0f / 255.0f, 255.0f / 255.0f, 255.0f / 255.0f));
		frozenMat.SetFloat(GameConstants.Materials.Property.FRESNEL_POWER, 0.91f);
		frozenMat.SetColor(GameConstants.Materials.Property.GOLD_COLOR, new Color(179.0f / 255.0f, 250.0f / 255.0f, 254.0f / 255.0f, 64.0f / 255.0f));

		return frozenMat;
	}
}
