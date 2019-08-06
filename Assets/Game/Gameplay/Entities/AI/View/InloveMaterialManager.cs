using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InloveMaterialManager : Singleton<InloveMaterialManager> {

	private Dictionary<Material, Material> m_inloveReferences;


	public static Material GetInloveMaterialFor(Material _source) {
		return instance.__GetInloveMaterialFor(_source);
	}

	public static void CleanInloveMaterials(){
		if ( instance != null && instance.m_inloveReferences != null)
			instance.m_inloveReferences.Clear();
	}

	private Material __GetInloveMaterialFor(Material _source) {
		if (m_inloveReferences == null) {
			m_inloveReferences = new Dictionary<Material, Material>();
		}

		Material inloveMat = null;
		if (m_inloveReferences.ContainsKey(_source)) {
			inloveMat = m_inloveReferences[_source];
		} else {
			inloveMat = CreateInloveVersionOf(_source);
			m_inloveReferences[_source] = inloveMat;
		}

		return inloveMat;
	}

	private Material CreateInloveVersionOf(Material _source) {
		Material frozenMat = new Material(_source);

		frozenMat.EnableKeyword(GameConstants.Materials.Keyword.FREEZE);
		frozenMat.SetColor(GameConstants.Materials.Property.FRESNEL_COLOR, new Color(225.0f / 255.0f, 34.0f / 255.0f, 255.0f / 255.0f, 255.0f / 255.0f));
		frozenMat.SetColor(GameConstants.Materials.Property.FRESNEL_COLOR_2, new Color(225.0f / 255.0f, 34.0f / 255.0f, 255.0f / 255.0f, 255.0f / 255.0f));
		frozenMat.SetFloat(GameConstants.Materials.Property.FRESNEL_POWER, 1.44f);
		frozenMat.SetColor(GameConstants.Materials.Property.GOLD_COLOR, new Color(179.0f / 255.0f, 250.0f / 255.0f, 254.0f / 255.0f, 64.0f / 255.0f));

		return frozenMat;
	}
}
