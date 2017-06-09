using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SoftMasking {
    public class TextMeshProUGUISoftMask : TextMeshProUGUI {
        private static readonly List<Component> s_components = new List<Component>();

        public override Material materialForRendering {
            get {
                if (base.m_sharedMaterial == null)
                    return null;
                base.GetComponents(typeof(IMaterialModifier), s_components);
                Material baseMaterial = this.m_sharedMaterial;
                for (int i = 0; i < s_components.Count; i++) {
                    baseMaterial = (s_components[i] as IMaterialModifier).GetModifiedMaterial(baseMaterial);
                }
                return baseMaterial;
            }
        }
    }
}