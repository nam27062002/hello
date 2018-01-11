// MenuPetPreview.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 17/11/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;
using DG.Tweening;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Preview of a pet in the main menu.
/// </summary>
public class MenuPetPreview : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public enum Anim {
		IDLE,
		IN,
		OUT,

		COUNT
	};

	public static readonly string[] ANIM_TRIGGERS  = {
		"idle",
		"in",
		"out"
	};

	private const string RARITY_GLOW_PREFAB_PATH = "UI/Menu/Pets/PF_PetRarityGlow_";	// Attach rarity sku to it

//    private static readonly Color EPIC_COLOR = new Color(255.0f / 255.0f, 237.0f / 255.0f, 0.0f / 255.0f);
//    private static readonly Color RARE_COLOR = new Color(2.0f / 255.0f, 240.0f / 255.0f, 13.0f / 255.0f);
//    private static readonly Color SPECIAL_COLOR = new Color(255.0f / 255.0f, 153.0f / 255.0f, 0.0f / 255.0f);

    //------------------------------------------------------------------//
    // MEMBERS															//
    //------------------------------------------------------------------//
    // Exposed
    [SerializeField] private string m_sku;
	public string sku { 
		get { return m_sku; }
		set { m_sku = value; }
	}

	[Comment("Used to attach anything that should follow the pet's animation. Typically the \"Hip\" node.")]
	[SerializeField] private Transform m_rootNode = null;
	public Transform rootNode { get { return m_rootNode; }}

	// Internal
	private Animator m_animator = null;
	private GameObject m_rarityGlow = null;
	private Tween m_rarityGlowShowHideTween = null;
    private Renderer[] m_renderers;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		m_animator = GetComponentInChildren<Animator>();
        m_renderers = GetComponentsInChildren<Renderer>();

		Messenger.AddListener<MenuScreens, MenuScreens>(MessengerEvents.MENU_SCREEN_TRANSITION_START, OnMenuScreenTransitionStart);
	}

    void setFresnelColor(Color col)
    {
        for (int c = 0; c < m_renderers.Length; c++)
        {
            Material m = m_renderers[c].material;
            m.SetFloat("_Fresnel", 3.0f);
            m.SetColor("_FresnelColor", col);
        }
    }

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// Kill tween as well
		if(m_rarityGlowShowHideTween != null) {
			m_rarityGlowShowHideTween.Kill(false);
			m_rarityGlowShowHideTween = null;
		}

		Messenger.RemoveListener<MenuScreens, MenuScreens>(MessengerEvents.MENU_SCREEN_TRANSITION_START, OnMenuScreenTransitionStart);
	}

	/// <summary>
	/// Apply the given animation to the pet's animator.
	/// </summary>
	/// <param name="_anim">The animation to be launched.</param>
	public void SetAnim(Anim _anim) {
		if(m_animator != null) {
			m_animator.SetTrigger(ANIM_TRIGGERS[(int)_anim]);
		}
	}

	/// <summary>
	/// Show/Hide rarity glow.
	/// </summary>
	/// <param name="_show">Whether to show or hide the rarity glow around the pet.</param>
	public void ToggleRarityGlow(bool _show) {
		// Show?
		if(_show) {
            // If not yet loaded, do it
            DefinitionNode petDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.PETS, m_sku);
            if (petDef == null) return;
            string rarity = petDef.Get("rarity");

            if (m_rarityGlow == null) {
				// Get pet definition from sku

                // Load glow for the target rarity
                string prefabPath = RARITY_GLOW_PREFAB_PATH + rarity;
                GameObject glowPrefab = Resources.Load<GameObject>(prefabPath);
				if(glowPrefab == null) return;	// No glow for this rarity (i.e. common)

				// Create new instance - use root node if available so the glow follows the pet's animation
				m_rarityGlow = GameObject.Instantiate<GameObject>(glowPrefab);
				if(rootNode != null) {
					m_rarityGlow.transform.SetParent(rootNode, false);
				} else {
					m_rarityGlow.transform.SetParent(this.transform, false);
				}

				// Create show/hide animator
			/*	m_rarityGlowShowHideTween = m_rarityGlow.transform
					.DOScale(Vector3.zero, 0.15f)
					.From()
					.SetEase(Ease.OutBack)
					.SetAutoKill(false)
					.Pause();*/

			}

			// Launch animation forwards

			/*if(m_rarityGlowShowHideTween != null) {
				m_rarityGlowShowHideTween.PlayForward();
			} else if(m_rarityGlow != null) {
				m_rarityGlow.SetActive(true);
			}*/

            foreach (Renderer rend in m_renderers)
            {
                rend.material.renderQueue = 3060;   //Draw pet just after particle glow effect
            }

            if (rarity == "epic")
            {
//                setFresnelColor(EPIC_COLOR);
                setFresnelColor(UIConstants.RARITY_COLORS[(int)EggReward.Rarity.EPIC]);
            }
            else if (rarity == "rare")
            {
//                setFresnelColor(RARE_COLOR);
                setFresnelColor(UIConstants.RARITY_COLORS[(int)EggReward.Rarity.RARE]);
            }
            else if (rarity == "special")
            {
//                setFresnelColor(SPECIAL_COLOR);
                setFresnelColor(UIConstants.RARITY_COLORS[(int)EggReward.Rarity.SPECIAL]);
            }

        } else {

			// Hiding, ignore if not yet loaded
			if(m_rarityGlow == null) return;

			// Launch animation backwards
		/*	if(m_rarityGlowShowHideTween != null) {
				m_rarityGlowShowHideTween.PlayBackwards();
			} else if(m_rarityGlow != null) {
				m_rarityGlow.SetActive(false);
			}*/

            setFresnelColor(Color.black);
		}

		if (m_rarityGlow != null) {
			m_rarityGlow.SetActive(_show);
		}
	}

	private void OnMenuScreenTransitionStart(MenuScreens _from, MenuScreens _to) {		
		ToggleRarityGlow(_to == MenuScreens.PETS);
	}
}

