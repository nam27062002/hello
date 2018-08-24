using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/**
 * Use this Monobehaviour for all particles in game. It currently supports materials:
 *  - Shaders/Particles/Particles Additive
 *  - Shaders/Particles/Particles AddSoft
 *  - Shaders/Particles/Particle Alpha Blend
 * 
 * Prefabs or GameObjects using this materials and NOT using this Component are NOT guaranteed to work properly.
 * Supports both scaling (with the inspector slider "Scale") and Canvas 3D rendering.
 */
//[ExecuteInEditMode]
public class ParticleEffectComposer : MonoBehaviour {

	/** Game objects in the following list must be of one of this types
	 * - TrailRenderer
	 * - Runeguard2DSRParticle
	 * - Runeguard2DParticle
	 */

	[SerializeField][Range(0,10)]
	public double m_Scale = 1;
    public double m_ModelScale = 1;
    private double m_PreviousScale;

	public bool m_Permanent = false;

	private bool m_bFlipX;
	public bool FlipX{
		get {
			return m_bFlipX;
		}
		set {
			m_bFlipX = value;
			m_DirtyScale = true;
		}
	}

	private bool m_bFlipY;
	public bool FlipY{
		get {
			return m_bFlipY;
		}
		set {
			m_bFlipY = value;
			m_DirtyScale = true;
		}
	}

	private bool m_bIgnoreParentScale;
	public bool IgnoreParentScale{
		get {
			return m_bIgnoreParentScale;
		}
		set {
			m_bIgnoreParentScale = value;
			m_DirtyScale = true;
		}
	}

	private bool m_DirtyScale;

	[SerializeField]
	public bool m_IgnoreParentRotation;
	private Quaternion m_InitialLocalRotation;

	public bool m_CameraFacing; // Overrides any other rotation parameter

	public bool Overlay {
		set {
			m_bOverlay = value;
		}
	}
	public bool m_bOverlay;

	public float TimeScale {
		set {
			mTimeScale = value;
		}
	}
	[SerializeField]
	private float mTimeScale = 1;

	public bool UIParticle = false;

	public bool UIParticleKeepVisible = false;

	private struct TParticleScaleInfo{
		public ParticleSystem particleSystem;
		public Material mSharedMaterialCopy;

		public float startSize;
		public float startSpeed;
		public float startGravity;
		//public Vector3 startLocalScale;

		public float startRadius;
		public Vector3 startBox;
	}

	private List<TParticleScaleInfo> mParticleGOInfos = new List<TParticleScaleInfo>();
	private Animator [] mParticleAnimators;

    private Transform m_kCurrentParentTransform = null;

    private Vector3 m_kInitialParentLossyScale = Vector3.one;

    private bool m_bUsingInitialParentLossyScale = false;

    private bool m_bForceInitialParentScaleAlways = false;

    private Vector3 m_kTempVector = new Vector3();

    private ParticleScaler m_kParticleScaler;

    private Vector3 m_kDefaultTransformScale = Vector3.one;

    private bool m_bDecorationOneSaleForced = false;

    private bool m_bParentScalesChecked = false;



	void Awake() {
		if (UIParticle && !UIParticleKeepVisible)
		{
			//if (ParticlesHolder.SharedInstance != null)
			{
				//ParticlesHolder.SharedInstance.RegistryUIParticleToCurrentUIBase (this.transform.gameObject);
			}
		}

		m_PreviousScale = -1;

		mParticleAnimators = GetComponentsInChildren<Animator>();

        m_kParticleScaler = this.transform.gameObject.GetComponent<ParticleScaler> ();
        if (m_kParticleScaler == null)
        {
            m_kParticleScaler = this.transform.gameObject.AddComponent<ParticleScaler> ();
        }

		ParticleSystem[] pParticles = GetComponentsInChildren<ParticleSystem>();
		mParticleGOInfos.Clear();

		for(int i = 0; i < pParticles.Length; ++i){
			TParticleScaleInfo info = new TParticleScaleInfo();

			info.particleSystem = pParticles[i];

			ParticleSystemRenderer renderer = info.particleSystem.GetComponent<ParticleSystemRenderer>();

            // send custom data to the shader
            //renderer.EnableVertexStreams(ParticleSystemVertexStreams.Custom1);

			if(Application.isPlaying) {
				info.mSharedMaterialCopy = renderer.material;
			} else if(renderer.sharedMaterial != null){
				info.mSharedMaterialCopy = renderer.sharedMaterial;
				//renderer.sharedMaterial = info.mSharedMaterialCopy;
			}

            ParticleSystem.MainModule kParticleMainModule = info.particleSystem.main;

            info.startSize = kParticleMainModule.startSize.constant;
            info.startSpeed = kParticleMainModule.startSpeed.constant;
            info.startGravity = kParticleMainModule.gravityModifier.constant;

			if(info.particleSystem.shape.enabled) {
				info.startRadius = info.particleSystem.shape.radius;

			#if UNITY_2017_1_OR_NEWER
				info.startBox = info.particleSystem.shape.scale;
			#else
				info.startBox = Vector3.one * info.particleSystem.shape.meshScale;
			#endif
			}

			if(renderer != null && info.mSharedMaterialCopy != null) {
				string materialName = info.mSharedMaterialCopy.shader.name;

                if(!materialName.StartsWith("Runeguard/Particles/") && !materialName.StartsWith("Runeguard/Models/")) {
					Debug.LogWarning(string.Format("Particle {0} contains GameObject {1} with unsupported material shader:  {2}", this.transform.name, renderer.transform.name, materialName));
				}

				if(materialName == "Particles/Additive") {
					Shader sh = (Shader)Resources.Load ("Shaders/Particles/Particles Add");
					info.mSharedMaterialCopy.shader = sh;
				}
				else if(materialName == "Particles/Additive (Soft)") {
					Shader sh = (Shader)Resources.Load ("Shaders/Particles/Particles AddSoft");
					info.mSharedMaterialCopy.shader = sh;
				}
				else if(materialName == "Particles/Alpha Blended") {
					Shader sh = (Shader)Resources.Load ("Shaders/Particles/Particle Alpha Blend");
					info.mSharedMaterialCopy.shader = sh;
				}
			}

			mParticleGOInfos.Add(info);
		}
	}

	void OnDestroy ()
	{
		if (UIParticle && !UIParticleKeepVisible)
		{
			//if (ParticlesHolder.SharedInstance != null)
			{
				//ParticlesHolder.SharedInstance.UnRegistryUIParticleToCurrentUIBase (this.transform.gameObject);
			}
		}
	}

    public void SetInitialParent (Transform kInitialParent, bool bForceThisScaleAlways = false)
    {
        if (kInitialParent != null)
        {
            m_kInitialParentLossyScale.x = kInitialParent.lossyScale.x;
            m_kInitialParentLossyScale.y = kInitialParent.lossyScale.y;
            m_kInitialParentLossyScale.z = kInitialParent.lossyScale.z;

            m_bUsingInitialParentLossyScale = true;

            m_bForceInitialParentScaleAlways = bForceThisScaleAlways;
        }
    }

    public void SetDecorationOneScaleForced ()
    {
        m_bDecorationOneSaleForced = true;
    }
        
    public void CheckParentScales ()
    {
        if (!m_bParentScalesChecked)
        {
            if (m_bDecorationOneSaleForced)
            {
                m_ModelScale = 1.0f;
            }
            else
            {
                if (m_kCurrentParentTransform != null)
                {
                    if (m_bForceInitialParentScaleAlways)
                    {
                        m_ModelScale = Math.Round (m_kInitialParentLossyScale.x, 2);
                    }
                    else
                    {
                        m_kTempVector.x = 1.0f / m_kCurrentParentTransform.lossyScale.x;
                        m_kTempVector.y = 1.0f / m_kCurrentParentTransform.lossyScale.y;
                        m_kTempVector.z = 1.0f / m_kCurrentParentTransform.lossyScale.z;

                        m_kDefaultTransformScale = m_kTempVector;

                        m_ModelScale = Math.Round (m_kCurrentParentTransform.lossyScale.x, 2);
                    }
                }
                else
                {
                    if (m_bUsingInitialParentLossyScale)
                    {
                        m_ModelScale = Math.Round (m_kInitialParentLossyScale.x, 2);
                    }
                    else
                    {
                        m_ModelScale = 1.0f;
                    }
                }
            }

            m_bParentScalesChecked = true;
        }

		if (UIParticle)
		{
			m_kTempVector.x = 1.0f / this.transform.lossyScale.x;
			m_kTempVector.y = 1.0f / this.transform.lossyScale.y;
			m_kTempVector.z = 1.0f / this.transform.lossyScale.z;

			m_kDefaultTransformScale = m_kTempVector;

			m_ModelScale = Math.Round (this.transform.lossyScale.x, 2);
		}
			
		if (m_bIgnoreParentScale)
		{
			m_kDefaultTransformScale = Vector3.one;

			m_ModelScale = 1.0f;
		}
			
		if (m_kCurrentParentTransform != null)
		{
			for (int i = 0; i < this.transform.childCount; ++i)
			{
				Transform kChild = this.transform.GetChild (i);

				m_kTempVector.x = kChild.transform.localPosition.x * (1.0f / m_kCurrentParentTransform.lossyScale.x);
				m_kTempVector.y = kChild.transform.localPosition.y * (1.0f / m_kCurrentParentTransform.lossyScale.y);
				m_kTempVector.z = kChild.transform.localPosition.z * (1.0f / m_kCurrentParentTransform.lossyScale.z);

				kChild.transform.localPosition = m_kTempVector;
			}
		}
    }

	public void Initialize()
    {
		m_InitialLocalRotation = transform.localRotation;
	}

	void Start()
    {
        LateUpdate(); // force an update to set material values before the first render
	}

	void LateUpdate()
    {
        if (m_kCurrentParentTransform == null)
        {
            m_kCurrentParentTransform = this.transform.parent;

            CheckParentScales ();
        }
        else
        {
            if (m_kCurrentParentTransform != this.transform.parent)
            {
                m_kCurrentParentTransform = this.transform.parent;

                CheckParentScales ();
            }
        }

		if(m_CameraFacing)
        {
			//transform.forward = (transform.position - CameraManager.SharedInstance.SceneCam.transform.position).normalized;
		}
        else
        {
			if(m_IgnoreParentRotation)
            {
				this.transform.rotation = m_InitialLocalRotation;
			}
            else if(transform.parent != null)
            {
				this.transform.rotation = transform.parent.rotation * m_InitialLocalRotation;
			}
		}
            
		for (int i = 0; i < mParticleAnimators.Length; ++i)
        {
			mParticleAnimators[i].speed = mTimeScale;
		}
            
		for (int i = 0; i < mParticleGOInfos.Count; ++i)
        {
			TParticleScaleInfo info = mParticleGOInfos[i];

            ParticleSystem.MainModule kParticleMainModule = info.particleSystem.main;

            kParticleMainModule.simulationSpeed = mTimeScale;

			ParticleSystemRenderer renderer = info.particleSystem.GetComponent<ParticleSystemRenderer>();

			if (info.mSharedMaterialCopy != null)
            {
                if (renderer.renderMode == ParticleSystemRenderMode.Mesh)
                {
                    info.mSharedMaterialCopy.SetVector("_Center", Vector3.zero);
                }
                else
                {
                    info.mSharedMaterialCopy.SetVector("_Center", transform.position);
                }

                if (m_PreviousScale != (m_Scale * m_ModelScale) || m_DirtyScale)
                {
                    float fFinalScale = 1.0f;

					info.mSharedMaterialCopy.SetFloat("_Overlay", m_bOverlay? 6.0f:2.0f);

                    info.mSharedMaterialCopy.SetVector("_Scaling", new Vector3(fFinalScale * (FlipX ? -1 : 1), fFinalScale * (FlipY ? -1 : 1), fFinalScale));
				}

                if (info.mSharedMaterialCopy.HasProperty ("_ShineScale"))
                {
                    if (info.mSharedMaterialCopy.HasProperty ("_ShineRandomMax"))
                    {
                        float fRandomMax = info.mSharedMaterialCopy.GetFloat ("_ShineRandomMax");

                        if (fRandomMax > 0.0f)
                        {
                            float fNegative = UnityEngine.Random.value;
                            if (fNegative > 0.5)
                            {
                                fNegative = -1.0f;
                            }
                            else
                            {
                                fNegative = 1.0f;
                            }

                            info.mSharedMaterialCopy.SetFloat ("_ShineRandomScale", fRandomMax * UnityEngine.Random.value * fNegative);
                        }
                        else
                        {
                            info.mSharedMaterialCopy.SetFloat ("_ShineRandomScale", 0.0f);
                        }
                    }
                }
			}
		}    
            
        if (m_PreviousScale != m_Scale * m_ModelScale || m_DirtyScale)
        {
            float fFinalScale = (float)(m_Scale * m_ModelScale);

            if (m_bDecorationOneSaleForced)
            {
                this.transform.localScale = m_kDefaultTransformScale;
            }
            else
            {
                this.transform.localScale = m_kDefaultTransformScale * fFinalScale;
            }

            if (m_kParticleScaler != null)
            {
                m_kParticleScaler.m_scale = fFinalScale;

                m_kParticleScaler.DoScale ();
            }
        }

        m_PreviousScale = m_Scale * m_ModelScale;

		m_DirtyScale = false;
	}
}
