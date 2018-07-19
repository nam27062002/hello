using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleControl : MonoBehaviour {

	private ParticleScaler m_scaler;
	private List<ParticleSystem> m_subsystems = null;
    private List<CustomParticleSystem> m_customSubsystems = null;
    private bool m_initialized = false;


    private void Awake() {
		if (!m_initialized) {
			FindSystems();
		}
	}

	private void FindSystems() {
		m_scaler = GetComponent<ParticleScaler>();
		m_subsystems = transform.FindComponentsRecursive<ParticleSystem>();
        m_customSubsystems = transform.FindComponentsRecursive<CustomParticleSystem>();
        m_initialized = true;
	}

	public void Play(ParticleData _data = null, bool _prewarm = true) {
        if (!m_initialized) {
            FindSystems();
		}

		if (_data != null) {
			if (m_scaler != null) {
				if (m_scaler.m_scale != _data.scale) {
					m_scaler.m_scale = _data.scale;
					m_scaler.DoScale();
				}
			}
		}

		// lets iterate
		for (int i = 0; i < m_subsystems.Count; i++) {
			ParticleSystem system = m_subsystems[i];
        
            system.Clear(false);

			ParticleSystem.MainModule main = system.main;

			if (_data != null) {
				if (_data.changeStartColor) {					
					ParticleSystem.MinMaxGradient gradient = main.startColor;
					if ( gradient.mode == ParticleSystemGradientMode.TwoColors ){
						gradient.colorMin = _data.startColor;
						gradient.colorMax = _data.startColorTwo;
					}else{
						gradient.color = _data.startColor;
					}
					main.startColor = gradient;
				}

				if (_data.changeColorOvertime) {
					ParticleSystem.ColorOverLifetimeModule colorOverLifetime = system.colorOverLifetime;
					ParticleSystem.MinMaxGradient gradient = colorOverLifetime.color;
					gradient.gradient = _data.colorOvertime;
					colorOverLifetime.color = gradient;
				}
			}

			ParticleSystem.EmissionModule em = system.emission;
			em.enabled = true;
/*
			if (main.prewarm && _prewarm) {
				system.Simulate(1f, false);
			}
*/
			system.Play(false);
		}


        // lets iterate custom
        for (int i = 0; i < m_customSubsystems.Count; i++)
        {
            CustomParticleSystem system = m_customSubsystems[i];

            system.Clear();

            if (_data != null)
            {
                if (_data.changeColorOvertime)
                {
                    system.m_colorAnimation = _data.colorOvertime;
                }
            }
/*
            if (system.m_ preWarm)
            {
                system.Simulate(1.0f);
            }
*/
            system.Play();
        }
	}

	// returns if it is fully stopped or not
	public bool Stop() {
        if (!m_initialized) {
            FindSystems();
        }

        bool isAlive = false;
		for (int i = 0; i < m_subsystems.Count; i++) {
			ParticleSystem system = m_subsystems[i];            
			ParticleSystem.EmissionModule em = system.emission;

			if (em.enabled && system.main.loop) {
				em.enabled = false;
				system.Stop();
			}

			if (system.IsAlive(false)) {
				isAlive = true;
			}
		}

        for (int i = 0; i < m_customSubsystems.Count; i++)
        {
            CustomParticleSystem system = m_customSubsystems[i];

            if (system.m_loop)
            {
                system.Stop(true);
            }

            if (system.IsPlaying)
            {
                isAlive = true;
            }
        }
        return !isAlive;
	}
}
