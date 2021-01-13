using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewFireColorConfig", menuName = "HungryDragon/Fire Color Config", order = 1)]
public class FireColorConfig : ScriptableObject {

    public Color m_fireStartColorA;
    public Color m_fireStartColorB;
    public Gradient m_fireGradient;
    public Material m_fireMaterial;
}
