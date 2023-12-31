﻿using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CPGameplayCheats : MonoBehaviour {
    [SerializeField] private TMP_InputField m_textInput;
    [SerializeField] private TMP_InputField m_numberInput;


    //--------------------------------------------------------------------------
    public void OnAddTime()     { InstanceManager.gameSceneControllerBase.elapsedSeconds += long.Parse(m_numberInput.text, NumberStyles.Any, CultureInfo.InvariantCulture); }
    public void OnRemoveTime()  { InstanceManager.gameSceneControllerBase.elapsedSeconds -= long.Parse(m_numberInput.text, NumberStyles.Any, CultureInfo.InvariantCulture); }
    public void OnSetTime()     { InstanceManager.gameSceneControllerBase.elapsedSeconds  = long.Parse(m_numberInput.text, NumberStyles.Any, CultureInfo.InvariantCulture); }
    //--------------------------------------------------------------------------

    //--------------------------------------------------------------------------
    public void OnAddXP() {
        Reward reward = new Reward();
        reward.xp = long.Parse(m_numberInput.text, NumberStyles.Any, CultureInfo.InvariantCulture);
        RewardManager.instance.DEBUG_ApplyReward(reward);
    }

    public void OnRemoveXP() {
        Reward reward = new Reward();
        reward.xp = -long.Parse(m_numberInput.text, NumberStyles.Any, CultureInfo.InvariantCulture);
        RewardManager.instance.DEBUG_ApplyReward(reward);
    }

    public void OnSetXP() {
        Reward reward = new Reward();
        reward.xp = long.Parse(m_numberInput.text, NumberStyles.Any, CultureInfo.InvariantCulture) - RewardManager.xp;
        RewardManager.instance.DEBUG_ApplyReward(reward);
    }
    //--------------------------------------------------------------------------

    //--------------------------------------------------------------------------
    public void OnSetCategoryKill() {
        RewardManager.instance.DEBUG_SetCategoryKill(m_textInput.text, int.Parse(m_numberInput.text, NumberStyles.Any, CultureInfo.InvariantCulture));
    }
    public void OnSetNPCKill() {
        RewardManager.instance.DEBUG_SetNPCKill(m_textInput.text, int.Parse(m_numberInput.text, NumberStyles.Any, CultureInfo.InvariantCulture));
    }
    //--------------------------------------------------------------------------
}
