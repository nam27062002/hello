﻿#if (UNITY_EDITOR || DEVELOPMENT_BUILD)
#define ACTK_UNITY_DEBUG_ENABLED
#if ACTK_WALLHACK_DEBUG
#define WALLHACK_DEBUG
#endif
#endif

#define UNITY_5_4_PLUS
#if UNITY_4_6 || UNITY_4_7 || UNITY_4_8 || UNITY_4_9 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3
#undef UNITY_5_4_PLUS
#endif

using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using CodeStage.AntiCheat.Common;
using Random = UnityEngine.Random;

#if !UNITY_4_3 && !UNITY_4_5 && !UNITY_4_6 && !UNITY_4_7
using UnityEngine.Rendering;
#endif


#if UNITY_5_4_PLUS
using UnityEngine.SceneManagement;
#endif

#if ACTK_EXCLUDE_OBFUSCATION
using System.Reflection;
#endif

namespace CodeStage.AntiCheat.Detectors
{
	/// <summary>
	/// Detects common types of wall hack cheating: walking through the walls (Rigidbody and CharacterController modules),
	/// shooting through the walls (Raycast module), looking through the walls (Wireframe module).
	/// </summary>
	/// In order to work properly, this detector creates and uses some service objects right in the scene.<br/>
	/// It places all such objects within 3x3x3 sandbox area which is placed at the #spawnPosition and drawn as a red wire cube in 
	/// the scene when you select Game Object with this detector.<br/>
	/// Please, place this sandbox area at the empty unreachable space of your game to avoid any collisions and false positives.<br/>
	/// 
	/// To get started:<br/>
	/// - add detector to any GameObject as usual or through the "GameObject > Create Other > Code Stage > Anti-Cheat Toolkit" menu;<br/>
	/// - make sure 3x3x3 area at the #spawnPosition is unreachable for any objects of your game;
	/// 
	/// You can use detector completely from inspector without writing any code except the actual reaction on cheating.
	/// 
	/// Avoid using detectors from code at the Awake phase.
	/// 
	/// <strong>\htmlonly<font color="7030A0">Note #1:</font>\endhtmlonly Adds new objects to the scene and places them into the
	/// "[WH Detector Service]" container at the #spawnPosition.<br/>
	/// \htmlonly<font color="7030A0">Note #2:</font>\endhtmlonly May use physics and shaders. It may lead to the build size 
	/// increase and additional resources usage.</strong>
	[AddComponentMenu(MENU_PATH + COMPONENT_NAME)]
	public class WallHackDetector : ActDetectorBase
	{
		internal const string COMPONENT_NAME = "WallHack Detector";
		internal const string FINAL_LOG_PREFIX = Constants.LOG_PREFIX + COMPONENT_NAME + ": ";

		private const string SERVICE_CONTAINER_NAME = "[WH Detector Service]";
		private const string WIREFRAME_SHADER_NAME = "Hidden/ACTk/WallHackTexture";
		private const int SHADER_TEXTURE_SIZE = 4;
		private const int RENDER_TEXTURE_SIZE = 4;

		private readonly Vector3 rigidPlayerVelocity = new Vector3(0, 0, 1f);

		private static int instancesInScene;

		private readonly WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();

		#region public properties
		[SerializeField]
		[Tooltip("Check for the \"walk through the walls\" kind of cheats made via Rigidbody hacks?")]
		private bool checkRigidbody = true;

		/// <summary>
		/// Check for the "walk through the walls" kind of cheats made via Rigidbody hacks?
		/// </summary>
		/// Disable to save some resources if you're not using Rigidbody for characters.
		public bool CheckRigidbody
		{
			get { return checkRigidbody; }
			set
			{
				if (checkRigidbody == value || !Application.isPlaying || !enabled || !gameObject.activeSelf) return;
				checkRigidbody = value;

				if (!started) return;

				UpdateServiceContainer();
				if (checkRigidbody)
				{
					StartRigidModule();
				}
				else
				{
					StopRigidModule();
				}
			}
		}

		[SerializeField]
		[Tooltip("Check for the \"walk through the walls\" kind of cheats made via Character Controller hacks?")]
		private bool checkController = true;

		/// <summary>
		/// Check for the "walk through the walls" kind of cheats made via Character Controller hacks?
		/// </summary>
		/// Disable to save some resources if you're not using Character Controllers.
		public bool CheckController
		{
			get { return checkController; }
			set
			{
				if (checkController == value || !Application.isPlaying || !enabled || !gameObject.activeSelf) return;
				checkController = value;

				if (!started) return;

				UpdateServiceContainer();
				if (checkController)
				{
					StartControllerModule();
				}
				else
				{
					StopControllerModule();
				}
			}
		}

        [SerializeField]
		[Tooltip("Check for the \"see through the walls\" kind of cheats made via shader or driver hacks (wireframe, color alpha, etc.)?")]
		private bool checkWireframe = true;

		/// <summary>
		/// Check for the "see through the walls" kind of cheats made via shader or driver hacks (wireframe, color alpha, etc.)?
		/// </summary>
		/// Disable to save some resources in case you don't care about such cheats.
		/// 
		/// <strong>\htmlonly<font color="7030A0">NOTE:</font>\endhtmlonly Uses specific shader under the hood.
		/// Thus such shader should be included into the build to exist at runtime.<br/>
		/// You may easily add or remove shader at the ACTk Settings window (Window > Code Stage > Anti-Cheat Toolkit > Settings).<br/>
		/// You'll see error in logs at runtime if you'll have no needed shader included.</strong>
		public bool CheckWireframe
        {
            get { return checkWireframe; }
            set
            {
                if (checkWireframe == value || !Application.isPlaying || !enabled || !gameObject.activeSelf) return;
                checkWireframe = value;

				if (!started) return;

				UpdateServiceContainer();
                if (checkWireframe)
                {
                    StartWireframeModule();
                }
                else
                {
                    StopWireframeModule();
                }
            }
        }

        [SerializeField]
		[Tooltip("Check for the \"shoot through the walls\" kind of cheats made via Raycast hacks?")]
		private bool checkRaycast = true;

		/// <summary>
		/// Check for the "shoot through the walls" kind of cheats made via Raycast hacks?
		/// </summary>
		/// Disable to save some resources in case you don't care about such cheats.
		public bool CheckRaycast
		{
			get { return checkRaycast; }
			set
			{
				if (checkRaycast == value || !Application.isPlaying || !enabled || !gameObject.activeSelf) return;
				checkRaycast = value;

				if (!started) return;

				UpdateServiceContainer();
				if (checkRaycast)
				{
					StartRaycastModule();
				}
				else
				{
					StopRaycastModule();
				}
			}
		}
		#endregion

		#region public fields
		/// <summary>
		/// Delay between Wireframe module checks, from 1 up to 60 secs.
		/// </summary>
		[Tooltip("Delay between Wireframe module checks, from 1 up to 60 secs.")]
		[Range(1f, 60f)]
		public int wireframeDelay = 10;

		/// <summary>
		/// Delay between Raycast module checks, from 1 up to 60 secs.
		/// </summary>
		[Tooltip("Delay between Raycast module checks, from 1 up to 60 secs.")]
		[Range(1f, 60f)]
		public int raycastDelay = 10;

		/// <summary>
		/// World coordinates of the service container. 
		/// Please keep in mind it will have different active objects within 3x3x3 cube during gameplay.
		/// It should be unreachable for your game objects to avoid collisions and false positives.
		/// </summary>
		[Tooltip("World position of the container for service objects within 3x3x3 cube (drawn as red wire cube in scene).")]
		public Vector3 spawnPosition;

		/// <summary>
		/// Maximum false positives in a row for each detection module before registering a wall hack.
		/// </summary>
		[Tooltip("Maximum false positives in a row for each detection module before registering a wall hack.")]
		public byte maxFalsePositives = 3;
		#endregion

		#region private variables
		private GameObject serviceContainer;
		private GameObject solidWall;
		private GameObject thinWall;

		private Camera wfCamera;
		private MeshRenderer foregroundRenderer;
		private MeshRenderer backgroundRenderer;
		private Color wfColor1 = Color.black;
		private Color wfColor2 = Color.black;
		private Shader wfShader;
		private Material wfMaterial;
		private Texture2D shaderTexture;
		private Texture2D targetTexture;
		private RenderTexture renderTexture;

		private int whLayer = -1;
		private int raycastMask = -1;

		private Rigidbody rigidPlayer;
		private CharacterController charControllerPlayer;
		private float charControllerVelocity;

		private byte rigidbodyDetections;
		private byte controllerDetections;
		private byte wireframeDetections;
		private byte raycastDetections;

		private bool wireframeDetected;
		#endregion

		#region public static methods
		/// <summary>
		/// Starts detection.
		/// </summary>
		/// Make sure you have properly configured detector in scene with #autoStart disabled before using this method.
		public static void StartDetection()
		{
			if (Instance != null)
			{
				Instance.StartDetectionInternal(null, Instance.spawnPosition, Instance.maxFalsePositives);
			}
			else
			{
				Debug.LogError(FINAL_LOG_PREFIX + "can't be started since it doesn't exists in scene or not yet initialized!");
			}
		}

		/// <summary>
		/// Starts detection with specified callback.
		/// </summary>
		/// If you have detector in scene make sure it has empty Detection Event.<br/>
		/// Creates a new detector instance if it doesn't exists in scene.
		/// <param name="callback">Method to call after detection.</param>
		public static void StartDetection(UnityAction callback)
		{
			StartDetection(callback, GetOrCreateInstance.spawnPosition);
		}

		/// <summary>
		/// Starts detection with specified callback using passed spawnPosition.
		/// </summary>
		/// If you have detector in scene make sure it has empty Detection Event.<br/>
		/// Creates a new detector instance if it doesn't exists in scene.
		/// <param name="callback">Method to call after detection.</param>
		/// <param name="spawnPosition">World position of the service 3x3x3 container. Overrides #spawnPosition property.</param>
		public static void StartDetection(UnityAction callback, Vector3 spawnPosition)
		{
			StartDetection(callback, spawnPosition, GetOrCreateInstance.maxFalsePositives);
		}

		/// <summary>
		/// Starts detection with specified callback using passed spawnPosition and maxFalsePositives.
		/// </summary>
		/// If you have detector in scene make sure it has empty Detection Event.<br/>
		/// Creates a new detector instance if it doesn't exists in scene.
		/// <param name="callback">Method to call after detection.</param>
		/// <param name="spawnPosition">World position of the service 3x3x3 container. Overrides #spawnPosition property.</param>
		/// <param name="maxFalsePositives">Amount of possible false positives in a row before registering detection. Overrides #maxFalsePositives property.</param>
		public static void StartDetection(UnityAction callback, Vector3 spawnPosition, byte maxFalsePositives)
		{
			GetOrCreateInstance.StartDetectionInternal(callback, spawnPosition, maxFalsePositives);
		}

		/// <summary>
		/// Stops detector. Detector's component remains in the scene. Use Dispose() to completely remove detector.
		/// </summary>
		public static void StopDetection()
		{
			if (Instance != null) Instance.StopDetectionInternal();
		}

		/// <summary>
		/// Stops and completely disposes detector component and destroys Service Container as well.
		/// </summary>
		/// On dispose Detector follows 2 rules:
		/// - if Game Object's name is "Anti-Cheat Toolkit Detectors": it will be automatically 
		/// destroyed if no other Detectors left attached regardless of any other components or children;<br/>
		/// - if Game Object's name is NOT "Anti-Cheat Toolkit Detectors": it will be automatically destroyed only
		/// if it has neither other components nor children attached;
		public static void Dispose()
		{
			if (Instance != null)
			{
				Instance.DisposeInternal();
			}
		}
		#endregion

		#region static instance
		/// <summary>
		/// Allows reaching public properties from code. Can be null.
		/// </summary>
		public static WallHackDetector Instance { get; private set; }

		private static WallHackDetector GetOrCreateInstance
		{
			get
			{
				if (Instance != null) return Instance;

				if (detectorsContainer == null)
				{
					detectorsContainer = new GameObject(CONTAINER_NAME);
				}
				Instance = detectorsContainer.AddComponent<WallHackDetector>();
				return Instance;
			}
		}
		#endregion

		private WallHackDetector() { } // prevents direct instantiation

		#region unity messages
		private void Awake()
		{
			instancesInScene++;
			if (Init(Instance, COMPONENT_NAME))
			{
				Instance = this;
			}

#if UNITY_5_4_PLUS
			SceneManager.sceneLoaded += OnLevelWasLoadedNew;
#endif
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();

			StopAllCoroutines();
			if (serviceContainer != null) Destroy(serviceContainer);

			if (wfMaterial != null)
			{
				wfMaterial.mainTexture = null;
				wfMaterial.shader = null;
				wfMaterial = null;

				wfShader = null;

				shaderTexture = null;
				targetTexture = null;

				renderTexture.DiscardContents();
				renderTexture.Release();
				renderTexture = null;
			}

			instancesInScene--;
		}

#if UNITY_5_4_PLUS
		private void OnLevelWasLoadedNew(Scene scene, LoadSceneMode mode)
		{
			OnLevelLoadedCallback();
		}
#else
		private void OnLevelWasLoaded()
		{
			OnLevelLoadedCallback();
		}
#endif

		private void OnLevelLoadedCallback()
		{
			if (instancesInScene < 2)
			{
				if (!keepAlive)
				{
					DisposeInternal();
				}
			}
			else
			{
				if (!keepAlive && Instance != this)
				{
					DisposeInternal();
				}
			}
		}

#if UNITY_EDITOR
		private void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.red;
			Gizmos.DrawWireCube(spawnPosition, new Vector3(3, 3, 3));
		}
#endif

		private void FixedUpdate()
		{
			if (!isRunning || !checkRigidbody || rigidPlayer == null)
				return;

			if (rigidPlayer.transform.localPosition.z > 1f)
			{
				rigidbodyDetections++;
				if (!Detect())
				{
					StopRigidModule();
					StartRigidModule();
				}
			}
		}

		private void Update()
		{
			if (!isRunning || !checkController || charControllerPlayer == null)
				return;

			if (charControllerVelocity > 0)
			{
				charControllerPlayer.Move(new Vector3(Random.Range(-0.002f, 0.002f), 0, charControllerVelocity));

				if (charControllerPlayer.transform.localPosition.z > 1f)
				{
					controllerDetections++;
					if (!Detect())
					{
						StopControllerModule();
						StartControllerModule();
					}
				}
			}
		}

#if WALLHACK_DEBUG
		private void OnGUI()
		{
			float x = 10;
			float y = 10;

			if (targetTexture != null)
			{
				//GUILayout.BeginArea(new Rect(10f, 10f, 100, 100), targetTexture, GUI.skin.box);
				//GUILayout.EndArea();
				GUI.DrawTexture(new Rect(x, y, 100, 100), targetTexture, ScaleMode.StretchToFill, false);
				//GUI.DrawTexture(new Rect(10, 10, RENDER_TEXTURE_SIZE, RENDER_TEXTURE_SIZE), targetTexture, ScaleMode.StretchToFill, false);
			}

			Color32 color1 = wfColor1;
			Color32 color2 = wfColor2;

			GUILayout.BeginArea(new Rect(x, y + 100, 200, 70), GUI.skin.box);
			GUI.color = wfColor1;
            GUILayout.Label("Color1: " + color1.r + ", " + color1.g + ", " + color1.b + ", " + color1.a);
			GUI.color = wfColor2;
			GUILayout.Label("Color2: " + color2.r + ", " + color2.g + ", " + color2.b + ", " + color2.a);
			GUILayout.EndArea();
		}
#endif
		#endregion

		private void StartDetectionInternal(UnityAction callback, Vector3 servicePosition, byte falsePositivesInRow)
		{
			if (isRunning)
			{
				Debug.LogWarning(FINAL_LOG_PREFIX + "already running!", this);
				return;
			}

			if (!enabled)
			{
				Debug.LogWarning(FINAL_LOG_PREFIX + "disabled but StartDetection still called from somewhere (see stack trace for this message)!", this);
				return;
			}

			if (callback != null && detectionEventHasListener)
			{
				Debug.LogWarning(FINAL_LOG_PREFIX + "has properly configured Detection Event in the inspector, but still get started with Action callback. Both Action and Detection Event will be called on detection. Are you sure you wish to do this?", this);
			}

			if (callback == null && !detectionEventHasListener)
			{
				Debug.LogWarning(FINAL_LOG_PREFIX + "was started without any callbacks. Please configure Detection Event in the inspector, or pass the callback Action to the StartDetection method.", this);
				enabled = false;
				return;
			}

#if UNITY_EDITOR
			if (checkRaycast || checkController || checkRigidbody)
			{
				int layerId = LayerMask.NameToLayer("Ignore Raycast");
                if (Physics.GetIgnoreLayerCollision(layerId, layerId))
				{
					Debug.LogError(FINAL_LOG_PREFIX + "IgnoreRaycast physics layer should collide with itself to avoid false positives! See readme's troubleshooting section for details.");
				}
			}
#endif

			detectionAction = callback;
			spawnPosition = servicePosition;
			maxFalsePositives = falsePositivesInRow;

			rigidbodyDetections = 0;
			controllerDetections = 0;
			wireframeDetections = 0;
			raycastDetections = 0;

			StartCoroutine(InitDetector());

			started = true;
			isRunning = true;
		}

		protected override void StartDetectionAutomatically()
		{
			StartDetectionInternal(null, spawnPosition, maxFalsePositives);
		}

		protected override void PauseDetector()
		{
			if (!isRunning) return;

			isRunning = false;
			StopRigidModule();
			StopControllerModule();
			StopWireframeModule();
			StopRaycastModule();
		}

		protected override void ResumeDetector()
		{
			if (detectionAction == null && !detectionEventHasListener) return;

			isRunning = true;

			if (checkRigidbody)
			{
				StartRigidModule();
			}

			if (checkController)
			{
				StartControllerModule();
			}

			if (checkWireframe)
			{
				StartWireframeModule();
			}

			if (checkRaycast)
			{
				StartRaycastModule();
			}
		}

		protected override void StopDetectionInternal()
		{
			if (!started)
				return;

			PauseDetector();
			detectionAction = null;
			isRunning = false;
		}

		protected override void DisposeInternal()
		{
			base.DisposeInternal();
			if (Instance == this)
				Instance = null;
		}

		private void UpdateServiceContainer()
		{
			if (enabled && gameObject.activeSelf)
			{
				#region common
				if (whLayer == -1)
					whLayer = LayerMask.NameToLayer("Ignore Raycast");
				if (raycastMask == -1)
					raycastMask = LayerMask.GetMask("Ignore Raycast");

				if (serviceContainer == null)
				{
					serviceContainer = new GameObject(SERVICE_CONTAINER_NAME);
					serviceContainer.layer = whLayer;
					serviceContainer.transform.position = spawnPosition;
					DontDestroyOnLoad(serviceContainer);
				}
				#endregion

				#region walk
				if ((checkRigidbody || checkController) && solidWall == null)
				{
#if WALLHACK_DEBUG

					solidWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
					solidWall.name = "SolidWall";
#else
					solidWall = new GameObject("SolidWall");
					solidWall.AddComponent<BoxCollider>();
#endif
					solidWall.layer = whLayer;
					solidWall.transform.parent = serviceContainer.transform;
					solidWall.transform.localScale = new Vector3(3, 3, 0.5f);
					solidWall.transform.localPosition = Vector3.zero;
				}
				else if ((!checkRigidbody && !checkController) && solidWall != null)
				{
					Destroy(solidWall);
				}
				#endregion

				#region wireframe
				if (checkWireframe && wfCamera == null)
				{
					if (wfShader == null)
						wfShader = Shader.Find(WIREFRAME_SHADER_NAME);

					if (wfShader == null)
					{
						Debug.LogError(FINAL_LOG_PREFIX + "can't find '" + WIREFRAME_SHADER_NAME + "' shader!\nPlease make sure you have it included at the Editor > Project Settings > Graphics.", this);
						checkWireframe = false;
					}
					else
					{
						if (!wfShader.isSupported)
						{
							Debug.LogError(FINAL_LOG_PREFIX + "can't detect wireframe cheats on this platform!", this);
							checkWireframe = false;
						}
						else
						{
							if (wfColor1 == Color.black)
							{
								wfColor1 = GenerateColor();
								do
									wfColor2 = GenerateColor();
								while (ColorsSimilar(wfColor1, wfColor2, 10));
							}

							if (shaderTexture == null)
							{
								shaderTexture = new Texture2D(SHADER_TEXTURE_SIZE, SHADER_TEXTURE_SIZE, TextureFormat.RGB24, false);
								shaderTexture.filterMode = FilterMode.Point;

								const int pixelsCount = SHADER_TEXTURE_SIZE * SHADER_TEXTURE_SIZE;
								const int halfPixelsCount = pixelsCount / 2;
								Color[] colors = new Color[pixelsCount];

								for (int i = 0; i < pixelsCount; i++)
								{
									if (i < halfPixelsCount)
									{
										colors[i] = wfColor1;
									}
									else
									{
										colors[i] = wfColor2;
									}
								}

								shaderTexture.SetPixels(colors, 0);
								shaderTexture.Apply();
							}

							if (renderTexture == null)
							{
								renderTexture = new RenderTexture(RENDER_TEXTURE_SIZE, RENDER_TEXTURE_SIZE, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
								renderTexture.autoGenerateMips = false;
								renderTexture.filterMode = FilterMode.Point;
								renderTexture.Create();
							}

							if (targetTexture == null)
							{
								targetTexture = new Texture2D(RENDER_TEXTURE_SIZE, RENDER_TEXTURE_SIZE, TextureFormat.RGB24, false);
								targetTexture.filterMode = FilterMode.Point;
							}

							if (wfMaterial == null)
							{
								wfMaterial = new Material(wfShader);
								wfMaterial.mainTexture = shaderTexture;
							}

							if (foregroundRenderer == null)
							{
								GameObject foregroundObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
								Destroy(foregroundObject.GetComponent<BoxCollider>());
								foregroundObject.name = "WireframeFore";
								foregroundObject.layer = whLayer;
								foregroundObject.transform.parent = serviceContainer.transform;
								foregroundObject.transform.localPosition = new Vector3(0, 0, 0f);

								foregroundRenderer = foregroundObject.GetComponent<MeshRenderer>();
								foregroundRenderer.sharedMaterial = wfMaterial;
#if UNITY_4_3 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7
								foregroundRenderer.castShadows = false;
#else
								foregroundRenderer.shadowCastingMode = ShadowCastingMode.Off;
#endif
								foregroundRenderer.receiveShadows = false;
								foregroundRenderer.enabled = false;
							}

							if (backgroundRenderer == null)
							{
								GameObject backgroundObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
								Destroy(backgroundObject.GetComponent<MeshCollider>());
								backgroundObject.name = "WireframeBack";
								backgroundObject.layer = whLayer;
								backgroundObject.transform.parent = serviceContainer.transform;
								backgroundObject.transform.localPosition = new Vector3(0, 0, 1f);
								backgroundObject.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);

								backgroundRenderer = backgroundObject.GetComponent<MeshRenderer>();
								backgroundRenderer.sharedMaterial = wfMaterial;
#if UNITY_4_3 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7
								backgroundRenderer.castShadows = false;
#else
								backgroundRenderer.shadowCastingMode = ShadowCastingMode.Off;
#endif
								backgroundRenderer.receiveShadows = false;
								backgroundRenderer.enabled = false;
							}

							if (wfCamera == null)
							{
								wfCamera = new GameObject("WireframeCamera").AddComponent<Camera>();
								wfCamera.gameObject.layer = whLayer;
								wfCamera.transform.parent = serviceContainer.transform;
								wfCamera.transform.localPosition = new Vector3(0, 0, -1f);
								wfCamera.clearFlags = CameraClearFlags.SolidColor;
								wfCamera.backgroundColor = Color.black;
								wfCamera.orthographic = true;
								wfCamera.orthographicSize = 0.5f;
								wfCamera.nearClipPlane = 0.01f;
								wfCamera.farClipPlane = 2.1f;
								wfCamera.depth = 0;
								wfCamera.renderingPath = RenderingPath.Forward;
								wfCamera.useOcclusionCulling = false;
								wfCamera.hdr = false;
								wfCamera.targetTexture = renderTexture;
								wfCamera.enabled = false;
							}
						}
					}
				}
				else if (!checkWireframe && wfCamera != null)
				{
					Destroy(foregroundRenderer.gameObject);
					Destroy(backgroundRenderer.gameObject);

					wfCamera.targetTexture = null;
					Destroy(wfCamera.gameObject);
				}
#endregion

				#region raycast
				if (checkRaycast && thinWall == null)
				{
					thinWall = GameObject.CreatePrimitive(PrimitiveType.Plane);
					thinWall.name = "ThinWall";
					thinWall.layer = whLayer;
					thinWall.transform.parent = serviceContainer.transform;

					// if we scale x down to 0.1, some raycast cheats wont work
					thinWall.transform.localScale = new Vector3(0.2f, 1f, 0.2f);
					thinWall.transform.localRotation = Quaternion.Euler(270, 0, 0);
					thinWall.transform.localPosition = new Vector3(0, 0, 1.4f);
					//thinWall.GetComponent<MeshCollider>().isTrigger = true;

#if !(WALLHACK_DEBUG)
					Destroy(thinWall.GetComponent<Renderer>());
					Destroy(thinWall.GetComponent<MeshFilter>());
#endif
				}
				else if (!checkRaycast && thinWall != null)
				{
					Destroy(thinWall);
				}
				#endregion
			}
			else if (serviceContainer != null)
			{
				Destroy(serviceContainer);
			}
		}

		private IEnumerator InitDetector()
		{
			// allows to properly kill existing service objects before creating new ones
			yield return waitForEndOfFrame;

			UpdateServiceContainer();

			if (checkRigidbody)
			{
				StartRigidModule();
			}

			if (checkController)
			{
				StartControllerModule();
			}

			if (checkWireframe)
			{
				StartWireframeModule();
			}

			if (checkRaycast)
			{
				StartRaycastModule();
			}
		}

#if ACTK_EXCLUDE_OBFUSCATION
		[Obfuscation(Exclude = true)]
#endif
		private void StartRigidModule()
		{
			if (!checkRigidbody)
			{
				StopRigidModule();
				UninitRigidModule();
				UpdateServiceContainer();
				return;
			}

			if (!rigidPlayer) InitRigidModule();
			if (rigidPlayer.transform.localPosition.z <= 1f && rigidbodyDetections > 0)
			{
#if ACTK_UNITY_DEBUG_ENABLED
				Debug.Log(FINAL_LOG_PREFIX + "rigidbody success shot! False positives counter reset.", this);
#endif
				rigidbodyDetections = 0;
			}

			rigidPlayer.rotation = Quaternion.identity;
			rigidPlayer.angularVelocity = Vector3.zero;
			rigidPlayer.transform.localPosition = new Vector3(0.75f, 0, -1f);
			rigidPlayer.velocity = rigidPlayerVelocity;
			Invoke("StartRigidModule", 4);
		}

#if ACTK_EXCLUDE_OBFUSCATION
		[Obfuscation(Exclude = true)]
#endif
		private void StartControllerModule()
		{
			if (!checkController)
			{
				StopControllerModule();
				UninitControllerModule();
				UpdateServiceContainer();
				return;
			}

			if (!charControllerPlayer) InitControllerModule();
			if (charControllerPlayer.transform.localPosition.z <= 1f && controllerDetections > 0)
			{
#if ACTK_UNITY_DEBUG_ENABLED
				Debug.Log(FINAL_LOG_PREFIX + "controller success shot! False positives counter reset.", this);
#endif
				controllerDetections = 0;
			}

			charControllerPlayer.transform.localPosition = new Vector3(-0.75f, 0, -1f);
			charControllerVelocity = 0.01f;
			Invoke("StartControllerModule", 4);
		}

#if ACTK_EXCLUDE_OBFUSCATION
		[Obfuscation(Exclude = true)]
#endif
		private void StartWireframeModule()
		{
			if (!checkWireframe)
			{
				StopWireframeModule();
				UpdateServiceContainer();
				return;
			}

			if (!wireframeDetected)
			{
				Invoke("ShootWireframeModule", wireframeDelay);
			}
		}

#if ACTK_EXCLUDE_OBFUSCATION
		[Obfuscation(Exclude = true)]
#endif
		private void ShootWireframeModule()
		{
			StartCoroutine(CaptureFrame());
			Invoke("ShootWireframeModule", wireframeDelay);
		}

		private IEnumerator CaptureFrame()
		{
#if WALLHACK_DEBUG
			if (thinWall != null) thinWall.GetComponent<Renderer>().enabled = false;
			if (solidWall != null) solidWall.GetComponent<Renderer>().enabled = false;
			if (charControllerPlayer != null) charControllerPlayer.GetComponent<Renderer>().enabled = false;
			if (rigidPlayer != null) rigidPlayer.GetComponent<Renderer>().enabled = false;
#endif
			wfCamera.enabled = true;
			yield return waitForEndOfFrame;
			
			foregroundRenderer.enabled = true;
			backgroundRenderer.enabled = true;

			RenderTexture previousActive = RenderTexture.active;
            RenderTexture.active = renderTexture;

			wfCamera.Render();

			foregroundRenderer.enabled = false;
			backgroundRenderer.enabled = false;

			while (!renderTexture.IsCreated())
			{
				yield return waitForEndOfFrame;
			}
			targetTexture.ReadPixels(new Rect(0, 0, RENDER_TEXTURE_SIZE, RENDER_TEXTURE_SIZE), 0, 0, false);
			targetTexture.Apply();

			RenderTexture.active = previousActive;

#if WALLHACK_DEBUG
			if (thinWall != null) thinWall.GetComponent<Renderer>().enabled = true;
			if (solidWall != null) solidWall.GetComponent<Renderer>().enabled = true;
			if (charControllerPlayer != null) charControllerPlayer.GetComponent<Renderer>().enabled = true;
			if (rigidPlayer != null) rigidPlayer.GetComponent<Renderer>().enabled = true;
#endif

			// in case we've deactivated detector while waiting for a frame
			if (wfCamera == null) yield return null;

			wfCamera.enabled = false;

			//yield return waitForEndOfFrame;

			bool detected = (targetTexture.GetPixel(0, 3) != wfColor1 ||
						targetTexture.GetPixel(0, 1) != wfColor2 ||
						targetTexture.GetPixel(3, 3) != wfColor1 ||
						targetTexture.GetPixel(3, 1) != wfColor2 ||
						targetTexture.GetPixel(1, 3) != wfColor1 ||
						targetTexture.GetPixel(2, 3) != wfColor1 ||
						targetTexture.GetPixel(1, 1) != wfColor2 ||
						targetTexture.GetPixel(2, 1) != wfColor2);

			if (!detected)
			{
				if (wireframeDetections > 0)
				{
#if ACTK_UNITY_DEBUG_ENABLED
					Debug.Log(FINAL_LOG_PREFIX + "wireframe success shot! False positives counter reset.", this);
#endif
					wireframeDetections = 0;
				}
			}
			else
			{
				wireframeDetections++;
				wireframeDetected = Detect();

#if WALLHACK_DEBUG
				Debug.Log(FINAL_LOG_PREFIX + "wireframe wallhack detected. Details below:\n" +
						  "wfColor1: " + (Color32)wfColor1 + ", wfColor2: " + (Color32)wfColor2 + "\n" +
						  (Color32)targetTexture.GetPixel(0, 3) + " != wfColor1: " + (targetTexture.GetPixel(0, 3) != wfColor1) + "\n" +
                          (Color32)targetTexture.GetPixel(0, 1) + " != wfColor2: " + (targetTexture.GetPixel(0, 1) != wfColor2) + "\n" +
						  (Color32)targetTexture.GetPixel(3, 3) + " != wfColor1: " + (targetTexture.GetPixel(3, 3) != wfColor1) + "\n" +
						  (Color32)targetTexture.GetPixel(3, 1) + " != wfColor2: " + (targetTexture.GetPixel(3, 1) != wfColor2) + "\n" +
						  (Color32)targetTexture.GetPixel(1, 3) + " != wfColor1: " + (targetTexture.GetPixel(1, 3) != wfColor1) + "\n" +
						  (Color32)targetTexture.GetPixel(2, 3) + " != wfColor1: " + (targetTexture.GetPixel(2, 3) != wfColor1) + "\n" +
						  (Color32)targetTexture.GetPixel(1, 1) + " != wfColor2: " + (targetTexture.GetPixel(1, 1) != wfColor2) + "\n" +
						  (Color32)targetTexture.GetPixel(2, 1) + " != wfColor2: " + (targetTexture.GetPixel(2, 1) != wfColor2), this);
#endif
			}
			yield return null;
		}

#if ACTK_EXCLUDE_OBFUSCATION
		[Obfuscation(Exclude = true)]
#endif
		private void StartRaycastModule()
		{
			if (!checkRaycast)
			{
				StopRaycastModule();
				UpdateServiceContainer();
				return;
			}

			Invoke("ShootRaycastModule", raycastDelay);
		}

#if ACTK_EXCLUDE_OBFUSCATION
		[Obfuscation(Exclude = true)]
#endif
		private void ShootRaycastModule()
		{
			if (Physics.Raycast(serviceContainer.transform.position, serviceContainer.transform.TransformDirection(Vector3.forward), 1.5f, raycastMask))
			{
				if (raycastDetections > 0)
				{
#if ACTK_UNITY_DEBUG_ENABLED
					Debug.Log(FINAL_LOG_PREFIX + "raycast success shot! False positives counter reset.", this);
#endif
					raycastDetections = 0;
				}
			}
			else
			{
				raycastDetections++;
				if (Detect()) return;
			}

			Invoke("ShootRaycastModule", raycastDelay);
		}

		private void StopRigidModule()
		{
			if (rigidPlayer) rigidPlayer.velocity = Vector3.zero;
			CancelInvoke("StartRigidModule");
		}

		private void StopControllerModule()
		{
			if (charControllerPlayer) charControllerVelocity = 0;
			CancelInvoke("StartControllerModule");
		}

		private void StopWireframeModule()
		{
			CancelInvoke("ShootWireframeModule");
		}

		private void StopRaycastModule()
		{
			CancelInvoke("ShootRaycastModule");
		}

		private void InitRigidModule()
		{
#if WALLHACK_DEBUG
			GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
			player.name = "RigidPlayer";
			player.GetComponent<CapsuleCollider>().height = 2;
#else
			GameObject player = new GameObject("RigidPlayer");
			player.AddComponent<CapsuleCollider>().height = 2;
#endif
			player.layer = whLayer;
			player.transform.parent = serviceContainer.transform;
			player.transform.localPosition = new Vector3(0.75f, 0, -1f);
			rigidPlayer = player.AddComponent<Rigidbody>();
			rigidPlayer.useGravity = false;
		}

		private void InitControllerModule()
		{
#if WALLHACK_DEBUG
			GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
			player.name = "ControlledPlayer";
			player.GetComponent<CapsuleCollider>().height = 2;
#else
			GameObject player = new GameObject("ControlledPlayer");
			player.AddComponent<CapsuleCollider>().height = 2;
#endif
			player.layer = whLayer;
			player.transform.parent = serviceContainer.transform;
			player.transform.localPosition = new Vector3(-0.75f, 0, -1f);
			charControllerPlayer = player.AddComponent<CharacterController>();
		}

		private void UninitRigidModule()
		{
			if (!rigidPlayer) return;

			Destroy(rigidPlayer.gameObject);
			rigidPlayer = null;
		}

		private void UninitControllerModule()
		{
			if (!charControllerPlayer) return;

			Destroy(charControllerPlayer.gameObject);
			charControllerPlayer = null;
		}

		private bool Detect()
		{
			bool result = false;

			if (controllerDetections > maxFalsePositives ||
				rigidbodyDetections > maxFalsePositives ||
				wireframeDetections > maxFalsePositives ||
                raycastDetections > maxFalsePositives)
			{
#if ACTK_UNITY_DEBUG_ENABLED
				Debug.LogWarning(FINAL_LOG_PREFIX + "final detection!", this);
#endif
				OnCheatingDetected();
				result = true;
			}
#if ACTK_UNITY_DEBUG_ENABLED
			else
			{
				Debug.LogWarning(FINAL_LOG_PREFIX + "detection!", this);
			}
#endif

			return result;
		}

		private static Color32 GenerateColor()
		{
			return new Color32((byte)Random.Range(0, 256), (byte)Random.Range(0, 256), (byte)Random.Range(0, 256), 255);
		}

		private static bool ColorsSimilar(Color32 c1, Color32 c2, int tolerance)
		{
			return System.Math.Abs(c1.r - c2.r) < tolerance &&
				   System.Math.Abs(c1.g - c2.g) < tolerance &&
				   System.Math.Abs(c1.b - c2.b) < tolerance;
		}
	}
}