using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrozenMaterialManager : Singleton<FrozenMaterialManager> {

	private Dictionary<Material, Material> m_frozenReferences;


	public static Material GetFrozenMaterialFor(Material _source) {
		return instance.__GetFrozenMaterialFor(_source);
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

		frozenMat.EnableKeyword("FRESNEL");
		frozenMat.EnableKeyword("MATCAP");
		frozenMat.SetColor("_FresnelColor", new Color(114.0f / 255.0f, 248.0f / 255.0f, 255.0f / 255.0f, 255.0f / 255.0f));
		frozenMat.SetColor("_FresnelColor2", new Color(186.0f / 255.0f, 144.0f / 255.0f, 255.0f / 255.0f, 255.0f / 255.0f));
		frozenMat.SetFloat("_FresnelPower", 0.91f);
		frozenMat.SetColor("_GoldColor", new Color(179.0f / 255.0f, 250.0f / 255.0f, 254.0f / 255.0f, 64.0f / 255.0f));

		return frozenMat;
	}
}
