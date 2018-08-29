using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetParcaeWeapon : PetMeleeWeapon {

    [Serializable]
    public class ModData {
        public float hpPercentage;
        public float xpPercentage;
        public float coinsPercentage;
    }

    [Serializable]
    public class ParcaeModifierDictionary : SerializableDictionary<string, ModData> { }

    [SerializeField] private ParcaeModifierDictionary m_modifiers;
}
