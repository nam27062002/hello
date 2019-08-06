using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DragonTint : MonoBehaviour {
    DragonBreathBehaviour m_breath;

    DragonPlayer m_player;
    DragonHealthBehaviour m_health;
    DragonMotion m_motion;

    Renderer[] m_renderers = null;
    List<Renderer> m_dragonRenderers = new List<Renderer>();

    int m_materialsCount = 0;
    List<Material> m_materials = new List<Material>();
    List<Color> m_materialsMultiplyColors = new List<Color>();
    List<Color> m_materialsAddColors = new List<Color>();
    List<Material> m_originalMaterial = new List<Material>();
    List<Color> m_fresnelColors = new List<Color>();
    List<float> m_innerLightAddValue = new List<float>();
    List<Color> m_innerLightColors = new List<Color>();
    float m_innerLightColorValue = 1;

    float m_otherColorTimer = 0;

    // Cursed
    public Color m_curseColor = Color.green;


    // Damage
    public Color m_damageColor = Color.red;
    float m_damageTimer = 0;
    float m_damageTotalTime = 0.8f;

    // Fury Timer
    float m_furyTimer = 0;

    // Shield
    // float m_shieldValue = 0;

    float m_deathAlpha = 1;

    private bool m_starvingOn = false;
    private bool m_criticalOn = false;

    public bool m_reduceInnerColorInsideWater = false;

    // Use this for initialization
    IEnumerator Start() {
        m_breath = GetComponent<DragonBreathBehaviour>();
        m_player = GetComponent<DragonPlayer>();
        m_health = GetComponent<DragonHealthBehaviour>();
        m_motion = m_player.dragonMotion;
        yield return null;
        Transform t = transform.Find("view");
        if (t != null) {
            m_renderers = t.GetComponentsInChildren<Renderer>();
            GetMaterials();
        }
    }

    void GetMaterials() {
        m_materials.Clear();
        m_materialsMultiplyColors.Clear();
        m_materialsAddColors.Clear();
        m_innerLightAddValue.Clear();
        m_innerLightColors.Clear();


        Material bodyMaterial = null;
        List<Renderer> bodyRenderers = new List<Renderer>();

        Material wingsMaterial = null;
        List<Renderer> wingsRenderers = new List<Renderer>();

        Dictionary<string, Material> joinMaterial = new Dictionary<string, Material>();
        Dictionary<string, List<Renderer>> disguiseRenderers = new Dictionary<string, List<Renderer>>();


        if (m_renderers != null)
            for (int i = 0; i < m_renderers.Length; i++) {

                Renderer r = m_renderers[i];
                Material mat = r.material;
                string shaderName = mat.shader.name;
                if (shaderName.Contains("Dragon standard")) {
                    if (r.tag.Equals("DragonBody")) {
                        if (bodyMaterial == null)
                            bodyMaterial = mat;
                        bodyRenderers.Add(r);
                    } else if (r.tag.Equals("DragonWings")) {
                        if (wingsMaterial == null)
                            wingsMaterial = mat;
                        wingsRenderers.Add(r);
                    } else {
                        if (!joinMaterial.ContainsKey(mat.name)) {
                            joinMaterial.Add(mat.name, mat);
                        }
                        if (disguiseRenderers.ContainsKey(mat.name)) {
                            disguiseRenderers[mat.name].Add(r);
                        } else {
                            // List<Renderer> rends = new List<Renderer>();
                            // rends.Add( r );
                            disguiseRenderers.Add(mat.name, new List<Renderer> { r });
                        }

                    }

                    m_dragonRenderers.Add(m_renderers[i]);
                }
            }

        int max = 0;

        if (bodyMaterial) {
            max = bodyRenderers.Count;
            for (int i = 0; i < max; i++) {
                bodyRenderers[i].material = bodyMaterial;
            }
            AddMaterialInfo(bodyMaterial);
        }

        if (wingsMaterial) {
            max = wingsRenderers.Count;
            for (int i = 0; i < max; i++) {
                wingsRenderers[i].material = wingsMaterial;
            }
            AddMaterialInfo(wingsMaterial);
        }

        foreach (KeyValuePair<string, Material> pair1 in joinMaterial) {
            List<Renderer> rends = disguiseRenderers[pair1.Key];
            int num = rends.Count;
            for (int i = 0; i < num; i++) {
                rends[i].material = pair1.Value;
            }
            AddMaterialInfo(pair1.Value);
        }



        m_materialsCount = m_materials.Count;
    }

    void AddMaterialInfo(Material mat) {
        m_materials.Add(mat);
        m_materialsMultiplyColors.Add(mat.GetColor(GameConstants.Materials.Property.TINT));
        m_materialsAddColors.Add(mat.GetColor(GameConstants.Materials.Property.COLOR_ADD));
        m_fresnelColors.Add(mat.GetColor(GameConstants.Materials.Property.FRESNEL_COLOR));
        m_innerLightAddValue.Add(mat.GetFloat(GameConstants.Materials.Property.INNER_LIGHT_ADD));
        m_innerLightColors.Add(mat.GetColor(GameConstants.Materials.Property.INNER_LIGHT_COLOR));

        Material original = new Material(mat);
        m_originalMaterial.Add(original);
    }

    void OnEnable() {
        Messenger.AddListener<float, DamageType, Transform>(MessengerEvents.PLAYER_DAMAGE_RECEIVED, OnDamageReceived);
        Messenger.AddListener<DamageType, Transform>(MessengerEvents.PLAYER_KO, OnPlayerKo);
        Messenger.AddListener<DragonPlayer.ReviveReason>(MessengerEvents.PLAYER_REVIVE, OnPlayerRevive);
        Messenger.AddListener<DragonHealthModifier, DragonHealthModifier>(MessengerEvents.PLAYER_HEALTH_MODIFIER_CHANGED, OnHealthModifierChanged);
    }

    void OnDisable() {
        // Unsubscribe from external events
        Messenger.RemoveListener<float, DamageType, Transform>(MessengerEvents.PLAYER_DAMAGE_RECEIVED, OnDamageReceived);
        Messenger.RemoveListener<DamageType, Transform>(MessengerEvents.PLAYER_KO, OnPlayerKo);
        Messenger.RemoveListener<DragonPlayer.ReviveReason>(MessengerEvents.PLAYER_REVIVE, OnPlayerRevive);
        Messenger.RemoveListener<DragonHealthModifier, DragonHealthModifier>(MessengerEvents.PLAYER_HEALTH_MODIFIER_CHANGED, OnHealthModifierChanged);
    }

    private void OnDamageReceived(float _amount, DamageType _type, Transform _source) {
        if (_type != DamageType.LATCH && _type != DamageType.POISON)
            m_damageTimer = m_damageTotalTime;
    }

    // Update is called once per frame
    void LateUpdate() {
        // Color multiply
        if (!m_health.IsAlive()) {
            // To alpha
            m_deathAlpha -= Time.deltaTime * 1.0f / Time.timeScale * 0.5f;
        } else {
            // To opaque
            m_deathAlpha += Time.deltaTime;
        }
        m_deathAlpha = Mathf.Clamp01(m_deathAlpha);

        SetColorMultiplyAlpha(m_deathAlpha);
        //		SetFresnelAlpha( m_deathAlpha );

        // Color add
        m_damageTimer -= Time.deltaTime;
        if (m_damageTimer < 0)
            m_damageTimer = 0;
        Color damageColor = m_damageColor * (m_damageTimer / m_damageTotalTime);


        // Other color
        Color otherColor = Color.black;
        if (m_health.HasDOT(DamageType.POISON)) {
            m_otherColorTimer += Time.deltaTime * 5;
            otherColor = m_curseColor * (Mathf.Sin(m_otherColorTimer) + 1) * 0.5f;
        } else if (m_starvingOn || m_criticalOn || m_player.BeingLatchedOn()) {
            m_otherColorTimer += Time.deltaTime * 5;
            otherColor = m_damageColor * (Mathf.Sin(m_otherColorTimer) + 1) * 0.5f;
        } else {
            m_otherColorTimer = 0;
        }
        SetColorAdd(damageColor + otherColor);


        // Inner light
        float innerValue = 0;
        if (m_breath.IsFuryOn()) {
            // animate fury color and inner light
            m_furyTimer += Time.deltaTime;

            innerValue = (Mathf.Sin(m_furyTimer * 2) * 0.5f) + 0.5f;
            innerValue *= 4;
        } else {
            m_furyTimer = 0;
        }
        SetInnerLightAdd(innerValue);

        // Shield
        /*
		if (m_player.HasMineShield())
		{
			m_shieldValue = Mathf.Lerp( m_shieldValue, 1, Time.deltaTime);
		}
		else
		{
			m_shieldValue = Mathf.Lerp( m_shieldValue, 0, Time.deltaTime);
		}
		m_dragonRenderer.material.SetFloat("_NoiseValue", m_shieldValue );
		*/

        if (m_reduceInnerColorInsideWater) {
            float value = m_innerLightColorValue;
            if (m_motion.insideWater) {
                value -= Time.deltaTime;
                if (value < 0.25f)
                    value = 0.25f;
            } else {
                value += Time.deltaTime;
                if (value > 1)
                    value = 1;
            }
            if (value != m_innerLightColorValue) {
                m_innerLightColorValue = value;
                SetInnerLightColorValue(m_innerLightColorValue);
            }
        }

    }

    void SetColorMultiplyAlpha(float a) {
        for (int i = 0; i < m_materialsCount; ++i) {
            Color c = m_materialsMultiplyColors[i];
            c.a = a;
            m_materials[i].SetColor(GameConstants.Materials.Property.TINT, c);
        }
    }

    void SetFresnelAlpha(float alpha) {
        for (int i = 0; i < m_materialsCount; ++i) {
            Color c = m_fresnelColors[i];
            c.a = alpha;
            m_fresnelColors[i] = c;
            m_materials[i].SetColor(GameConstants.Materials.Property.FRESNEL_COLOR, c);
        }
    }

    void SetColorAdd(Color c) {
        c.a = 0;
        for (int i = 0; i < m_materialsCount; ++i) {
            Color res = m_materialsAddColors[i] + c;
            m_materials[i].SetColor(GameConstants.Materials.Property.COLOR_ADD, res);
        }
    }

    void SetInnerLightAdd(float innerValue) {
        for (int i = 0; i < m_materialsCount; ++i) {
            m_materials[i].SetFloat(GameConstants.Materials.Property.INNER_LIGHT_ADD, m_innerLightAddValue[i] + innerValue);
        }
    }

    void SetInnerLightColorValue(float value) {
        for (int i = 0; i < m_materialsCount; ++i) {
            Color c = m_innerLightColors[i] * value;
            m_materials[i].SetColor(GameConstants.Materials.Property.INNER_LIGHT_COLOR, c);
        }
    }

    private void OnPlayerKo(DamageType _type, Transform _source) {
        // Switch body material to wings
        for (int i = 0; i < m_materialsCount; ++i)
            DragonCorpse.setDeathMode(m_materials[i]);
        //            m_materials[i].shader = Shader.Find("Hungry Dragon/Dragon/Death");

        if (_type == DamageType.MINE || _type == DamageType.BIG_DAMAGE || InstanceManager.player.m_alwaysSpawnCorpse) {
            // Shows corpse
            m_deathAlpha = 0;
            for (int i = 0; i < m_dragonRenderers.Count; i++) {
                m_dragonRenderers[i].enabled = false;
            }
        }
    }

    private void OnPlayerRevive(DragonPlayer.ReviveReason reason) {
        // Switch back body materials
        for (int i = 0; i < m_materialsCount; ++i)
            m_materials[i].CopyPropertiesFromMaterial(m_originalMaterial[i]);

        m_deathAlpha = 1;
        for (int i = 0; i < m_dragonRenderers.Count; i++) {
            m_dragonRenderers[i].enabled = true;
        }
    }

    private void OnHealthModifierChanged(DragonHealthModifier _oldModifier, DragonHealthModifier _newModifier) {
        m_starvingOn = (_newModifier != null && _newModifier.IsStarving());
        m_criticalOn = (_newModifier != null && _newModifier.IsCritical());
    }
}
