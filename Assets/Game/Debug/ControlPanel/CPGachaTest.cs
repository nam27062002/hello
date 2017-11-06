// CPGachaTest.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 19/12/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Text;
using System.Collections.Generic;
using TMPro;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Global class to control gacha testing features.
/// </summary>
public class CPGachaTest : MonoBehaviour {
	//------------------------------------------------------------------------//
	// REWARD CHANCE														  //
	//------------------------------------------------------------------------//
	public enum RewardChanceMode {
		DEFAULT = 0,
		SAME_PROBABILITY,
		COMMON_ONLY,
		RARE_ONLY,
		EPIC_ONLY,
		FORCED_PET_SKU
	};

	public enum DuplicateMode {
		DEFAULT = 0,
		ALWAYS,
		NEVER,
		RANDOM
	};

	public enum IncubationTime {
		DEFAULT = 0,
		SECONDS_10,
		SECONDS_30,
		SECONDS_60
	}

	public const string REWARD_CHANCE_MODE = "GACHA_REWARD_CHANCE_MODE";
	public static RewardChanceMode rewardChanceMode {
		get { return (RewardChanceMode)Prefs.GetIntPlayer(REWARD_CHANCE_MODE, (int)RewardChanceMode.DEFAULT); }
		set { Prefs.SetIntPlayer(REWARD_CHANCE_MODE, (int)value); }
	}

	public const string DUPLICATE_MODE = "GACHA_DUPLICATE_MODE";
	public static DuplicateMode duplicateMode {
		get { return (DuplicateMode)Prefs.GetIntPlayer(DUPLICATE_MODE, (int)DuplicateMode.DEFAULT); }
		set { Prefs.SetIntPlayer(DUPLICATE_MODE, (int)value); }
	}

	public const string FORCED_PET_SKU = "FORCED_PET_SKU";
	public static string forcedPetSku {
		get { return Prefs.GetStringPlayer(FORCED_PET_SKU, ""); }
		set { Prefs.SetStringPlayer(FORCED_PET_SKU, value); }
	}

	public const string INCUBATION_TIME = "GACHA_INCUBATION_TIME";
	public static IncubationTime incubationTime {
		get { return (IncubationTime)Prefs.GetIntPlayer(INCUBATION_TIME, (int)IncubationTime.DEFAULT); }
		set { Prefs.SetIntPlayer(INCUBATION_TIME, (int)value); }
	}

	//------------------------------------------------------------------------//
	// EXPOSED MEMBERS														  //
	//------------------------------------------------------------------------//
	// Reward Chance
	[Space]
	[SerializeField] private CPEnumPref m_rewardChanceDropdown = null;
	[SerializeField] private CPEnumPref m_duplicateDropdown = null;
	[SerializeField] private TMP_Dropdown m_petSkuDropdown = null;
	[SerializeField] private CPEnumPref m_incubationTimeDropdown = null;
	[Space]
	[SerializeField] private CanvasGroup m_gachaTesterGroup = null;
	[SerializeField] private TMP_InputField m_gachaTesterInput = null;
	[SerializeField] private TextMeshProUGUI m_gachaTesterOutputText = null;
	[SerializeField] private CanvasGroup m_gachaTesterResultsGroup = null;

	// Internal
	private List<DefinitionNode> m_petDefs = null;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Init pets dropdown
		m_petSkuDropdown.onValueChanged.AddListener(OnPetValueChanged);

		// Init enum dropdowns
		m_rewardChanceDropdown.InitFromEnum(REWARD_CHANCE_MODE, typeof(RewardChanceMode), 0);
		m_duplicateDropdown.InitFromEnum(DUPLICATE_MODE, typeof(DuplicateMode), 0);
		m_incubationTimeDropdown.InitFromEnum(INCUBATION_TIME, typeof(IncubationTime), 0);

		// Hide tester results
		m_gachaTesterResultsGroup.gameObject.SetActive(false);
	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Make sure all values are updated
		Refresh();
	}

	/// <summary>
	/// Called every frame.
	/// </summary>
	private void Update() {
		// Only enable forced pet sku dropdown if the right mode is chosen
		m_petSkuDropdown.interactable = (rewardChanceMode == RewardChanceMode.FORCED_PET_SKU);
	}

	/// <summary>
	/// Make sure all fields have the right values.
	/// </summary>
	private void Refresh() {
		m_rewardChanceDropdown.Refresh();
		m_duplicateDropdown.Refresh();
		m_incubationTimeDropdown.Refresh();

		// Initialize pets dropdown
		m_petSkuDropdown.ClearOptions();
		m_petDefs = DefinitionsManager.SharedInstance.GetDefinitionsList(DefinitionsCategory.PETS);
		if(m_petDefs.Count > 0) {
			// Content ready! Init dropdown
			int selectedIdx = -1;
			string currentValue = forcedPetSku;
			for(int i = 0; i < m_petDefs.Count; i++) {
				// Add poption
				m_petSkuDropdown.options.Add(
					new TMP_Dropdown.OptionData(
						m_petDefs[i].GetLocalized("tidName")
					)
				);

				// Is it the current one?
				if(m_petDefs[i].sku == currentValue) {
					selectedIdx = i;
				}
			}

			// If no pet was selected, use first one
			if(selectedIdx < 0) {
				selectedIdx = 0;
				forcedPetSku = m_petDefs[selectedIdx].sku;
			}

			// Set selection
			m_petSkuDropdown.value = selectedIdx;

			// [AOC] Dropdown seems to be bugged -_-
			if(m_petSkuDropdown.options.Count > 0) {
				m_petSkuDropdown.captionText.text = m_petSkuDropdown.options[m_petSkuDropdown.value].text;
			} else {
				m_petSkuDropdown.captionText.text = "";
			}
		} else {
			// Content not ready, try again on next "Refresh" call
			m_petDefs = null;
		}
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		m_petSkuDropdown.onValueChanged.RemoveListener(OnPetValueChanged);
	}

	/// <summary>
	/// Gacha tester async call to prevent game from freezing when num tries is too big.
	/// </summary>
	/// <returns>Async task.</returns>
	private System.Collections.IEnumerator GachaTester() {
		// Get number of tries from input textfield
		int numTries = int.Parse(m_gachaTesterInput.text);
		float seconds = 10;
		float triesPerSecond = Mathf.Ceil((float)numTries/seconds);
		int triesPerFrame = Mathf.CeilToInt(triesPerSecond/30f);	// Assuming we're at 30FPS

		// Create output string builder
		StringBuilder sb = new StringBuilder();

		// Some constants
		string[] colorTags = {
			"<color=#aaaaaa>",
			"<color=#00ffff>",
			"<color=#ffaa00>"
		};

		// Hide results group
		m_gachaTesterResultsGroup.gameObject.SetActive(false);

		// Disable tester group
		m_gachaTesterGroup.interactable = false;
		m_gachaTesterGroup.alpha = 0.5f;

		// Give time for UI refresh
		yield return new WaitForEndOfFrame();

		// Run gacha random as many times as needed and store results
		Metagame.RewardEgg reward = null;
		string color = "";
		int[] count = new int[3];
		int[] duplicates = new int[3];
		int totalDuplicates = 0;
		int rarity = 0;
		bool duplicate = false;
		List<Metagame.RewardEgg> rewards = new List<Metagame.RewardEgg>();
		HashSet<string> skus = new HashSet<string>();
		List<bool> isDuplicate = new List<bool>();
		for(int i = 0; i < numTries; i += triesPerFrame) {
			for(int j = 0; j < triesPerFrame; ++j) {
				// Create a new reward
				reward = Metagame.Reward.CreateTypeEgg("egg_premium", "cheats") as Metagame.RewardEgg;

				// Figure out rarity and duplicate
				rarity = (int)reward.reward.rarity;
				duplicate = (reward.reward.WillBeReplaced() || skus.Contains(reward.reward.sku));

				// Increase stats
				count[rarity]++;
				if(duplicate) {
					duplicates[rarity]++;
					totalDuplicates++;
				}

				// Update collections
				rewards.Add(reward);
				skus.Add(reward.reward.sku);
				isDuplicate.Add(duplicate);

				// Log
				Debug.Log(colorTags[rarity] + reward.reward.sku + (duplicate ? " (d)" : "") + "</color>");
			}
			yield return new WaitForEndOfFrame();
		}

		// Log summary
		Debug.Log("<color=green>DONE!</color>\n" 
			+ colorTags[0] + count[0] + " common</color> | " 
			+ colorTags[1] + count[1] + " rare</color> | "
			+ colorTags[2] + count[2] + " epic</color>"
		);

		// Reenable group
		m_gachaTesterGroup.interactable = true;
		m_gachaTesterGroup.alpha = 1f;

		// Show results
		float factor = 1f / Mathf.Max((float)numTries, 1f) * 100f;	// Prevent div0, apply * 100f directly
		sb.Append(colorTags[0]).AppendFormat("common: {0} ({1:0.##}%)</color>", count[0], (float)count[0] * factor).AppendLine();
		sb.Append(colorTags[1]).AppendFormat("rare: {0} ({1:0.##}%)</color>", count[1], (float)count[1] * factor).AppendLine();
		sb.Append(colorTags[2]).AppendFormat("epic: {0} ({1:0.##}%)</color>", count[2], (float)count[2] * factor).AppendLine();

		sb.AppendLine();
		sb.AppendFormat("Duplicates: {0} ({1:0.##}%) from which:", totalDuplicates, ((float)totalDuplicates/(float)numTries * 100f)).AppendLine();

		factor = 1f / Mathf.Max((float)totalDuplicates, 1f) * 100f;	// Prevent div0, apply * 100f directly
		sb.Append(colorTags[0]).AppendFormat("common: {0} ({1:0.##}%)</color>", duplicates[0], (float)duplicates[0] * factor).AppendLine();
		sb.Append(colorTags[1]).AppendFormat("rare: {0} ({1:0.##}%)</color>", duplicates[1], (float)duplicates[1] * factor).AppendLine();
		sb.Append(colorTags[2]).AppendFormat("epic: {0} ({1:0.##}%)</color>", duplicates[2], (float)duplicates[2] * factor).AppendLine();

		sb.AppendLine();
		sb.AppendLine("Rewards List:");
		for(int i = 0; i < rewards.Count; ++i) {
			reward = rewards[i];

			rarity = (int)reward.reward.rarity;
			sb.Append(colorTags[rarity]);
			sb.Append(reward.reward.sku);

			if(isDuplicate[i]) {
				sb.Append(" (d)");
			}

			sb.Append("</color>");
			sb.AppendLine();
		}

		m_gachaTesterResultsGroup.gameObject.SetActive(true);
		m_gachaTesterOutputText.text = sb.ToString();
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// A new option has been picked on the dropdown.
	/// </summary>
	public void OnPetValueChanged(int _newValueIdx) {
		if(m_petDefs != null) {
			forcedPetSku = m_petDefs[_newValueIdx].sku;
			Messenger.Broadcast<string, string>(GameEvents.CP_STRING_CHANGED, FORCED_PET_SKU, m_petDefs[_newValueIdx].sku);
			Messenger.Broadcast<string>(GameEvents.CP_PREF_CHANGED, FORCED_PET_SKU);
		}
	}

	/// <summary>
	/// The gacha tester has been triggered.
	/// </summary>
	public void OnGachaTester() {
		StartCoroutine(GachaTester());
	}
}