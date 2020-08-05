using System;
using UnityEngine;

public class DragonVampireEffects : MonoBehaviour
{
    [SerializeField]
    GameObject onDieParticleEffect;

    void OnEnable()
	{
        Messenger.AddListener<DamageType, Transform>(MessengerEvents.PLAYER_KO, OnPlayerDeath);
    }

    void OnDisable()
    {
        Messenger.RemoveListener<DamageType, Transform>(MessengerEvents.PLAYER_KO, OnPlayerDeath);
    }

    void OnPlayerDeath(DamageType arg1, Transform arg2)
    {
        onDieParticleEffect.SetActive(true);
    }

    private void Update()
    {
       // if (Input.GetKeyDown(KeyCode.J))
        //    onDieParticleEffect.SetActive(true);
    }
}
