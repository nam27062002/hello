using UnityEngine;
using System;
using System.Collections;
using System.IO;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;

/*
 * @brief Contains some UI utils
 */
public partial class UIUtils {
    // ======================================================================================================================
    // ENUMS
    // ======================================================================================================================
    public enum eNumberFormatType {
        E_NONE = 0,
        E_NORMAL_LETTER_MULTIPLIER,
        E_NORMAL_LETTER_MULTIPLIER_NO_DECIMALS_UNDER_1K,
        E_1K_LETTER_MULTIPLIER,
    }

    public enum eScreenAspectRationType
    {
            E_2_1,
            E_4_3,
            E_16_9
    }

    // ======================================================================================================================
    // CONSTANTS
    // ======================================================================================================================
    public readonly static Color BUTTON_TEXT_DISABLED       = new Color (1f, 1f, 1f, 0.5f);
    public readonly static Color BUTTON_ICON_DISABLED       = new Color (1f, 1f, 1f, 0.5f);
    public readonly static Color BUTTON_BACKGROUND_DISABLED = new Color (1f, 1f, 1f, 0.5f);

	// ======================================================================================================================
	// STATIC VALUES
	// ======================================================================================================================
	public readonly static float		RESOLUTION_CHANGE_INCHES = 8.0f;
	public readonly static Vector2 		RESOLUTION_6S_REFERENCE = new Vector2(1334.0f, 750.0f);
	public readonly static Vector2 		RESOLUTION_6SPLUS_REFERENCE = new Vector2(1920.0f, 1080.0f);

    // ======================================================================================================================
    // HELPERS
    // ======================================================================================================================
    /*
     * Set the visibility of an element
     * @param _go           Element to change the visibility
     * @param _visible      True means it is visible
     * @param _interactable True means it can be clickable
     */
    public static void setGOVisible (ref GameObject _go, bool _visible, bool _interactable) {
        if (_go != null && _go.GetComponent<CanvasGroup>() != null) {
            _go.GetComponent<CanvasGroup>().interactable = _interactable;
            _go.GetComponent<CanvasGroup>().alpha = (_visible ? 1f : 0f);
            _go.GetComponent<CanvasGroup>().blocksRaycasts = _interactable;
        }
    }

    // ======================================================================================================================
    // SCREENS
    // ======================================================================================================================
    private static PointerEventData sPointer = null;
    private static List<RaycastResult> sRayCastResults = null;

    // ======================================================================================================================
    // RAYCAST
    // ======================================================================================================================
    /*
     * Return all the elements under the touch position
     */
    public static GameObject getTouchedUIGO () {
        Vector3 touch = getTouchPosition ();

        if (sPointer == null) {
            sPointer = new PointerEventData (EventSystem.current);
        }

        sPointer.position = touch;

        if (sRayCastResults == null) {
            sRayCastResults = new List<RaycastResult> ();
        } else {
            sRayCastResults.Clear ();
        }
            
            
        EventSystem.current.RaycastAll (sPointer, sRayCastResults);
        if (sRayCastResults.Count != 0) {
            return sRayCastResults [0].gameObject;
        }

        return null;
    }

    private static PointerEventData    sPointerEventData = null;
    private static List<RaycastResult> sRaycastResults   = null;

    public static GameObject getUnderMouseUIGO () {
        Vector3 touch = Input.mousePosition;

        if (sPointerEventData == null) {
            sPointerEventData = new PointerEventData (EventSystem.current);
        }
        sPointerEventData.position = touch;
        if (sRaycastResults == null) {
            sRaycastResults = new List <RaycastResult> ();
        }
        else {
            sRaycastResults.Clear ();
        }
        EventSystem.current.RaycastAll (sPointerEventData, sRaycastResults);
        if (sRaycastResults.Count != 0) {
            return sRaycastResults [0].gameObject;
        }
        
        return null;
    }

    public static List<GameObject> getUnderMouseUIGOs () {
        List<GameObject> retVal = new List <GameObject> ();
        Vector3 touch = Input.mousePosition;

        var pointer = new PointerEventData (EventSystem.current);
        pointer.position = touch;
        List<RaycastResult> raycastResults = new List<RaycastResult> ();
        EventSystem.current.RaycastAll (pointer, raycastResults);
        for (int i = 0; i < raycastResults.Count; ++ i) {
            retVal.Add (raycastResults [i].gameObject);
        }

        return retVal;
    }

    public static GameObject getUntouchedUIGO () {
        Vector3 touch = getUntouchPosition ();
        
        var pointer = new PointerEventData (EventSystem.current);
        pointer.position = touch;
        List<RaycastResult> raycastResults = new List<RaycastResult> ();
        EventSystem.current.RaycastAll (pointer, raycastResults);
        if (raycastResults.Count != 0) {
            return raycastResults [0].gameObject;
        }
        
        return null;
    }

    // ======================================================================================================================
    // LAYERS
    // ======================================================================================================================
    /*
     * Sets the layer for a PS and all its childs
     * @param _go       Parent PS
     * @param _layer    Layer name
     */
    public static void setLayer (GameObject _go, string _layer, Transform [] kTransforms = null, int _layerID = -1, bool bAvoidParticleSystems = false) {
        if (_go != null) {
			if (!bAvoidParticleSystems) {
	            if (_go.GetComponent<ParticleSystem> () != null) {
	                ParticleSystem ps = _go.GetComponent<ParticleSystem> ();
					if (_layerID != -1) {
						ps.GetComponent<Renderer>().sortingLayerID = _layerID;
					} else {
	                	ps.GetComponent<Renderer>().sortingLayerName = _layer;
					}

	                //ps.GetComponent<Renderer>().sortingOrder += 100;
	            }
			}

			if (_layerID != -1) {
				_go.layer = _layerID;
			} else {
            	_go.layer = LayerMask.NameToLayer (_layer);
			}

			Transform [] ts1 = null;
			if (kTransforms != null) {
				ts1 = kTransforms;
			} else {
				ts1 = _go.transform.GetComponentsInChildren<Transform> ();
			}

			for (int i = 0; i < ts1.Length; ++ i) {
				if (!bAvoidParticleSystems) {
					if (ts1[i].gameObject.GetComponent<ParticleSystem> () != null) {
						if (_layerID != -1) {
							ts1[i].gameObject.GetComponent<ParticleSystem> ().GetComponent<Renderer>().sortingLayerID = _layerID;
						} else {
							ts1[i].gameObject.GetComponent<ParticleSystem> ().GetComponent<Renderer>().sortingLayerName = _layer;
						}

						//ts1[i].gameObject.GetComponent<ParticleSystem> ().GetComponent<Renderer>().sortingOrder += 100;
	                }
				}

				if (_layerID != -1) {
					ts1[i].gameObject.layer = _layerID;
				} else {
					ts1[i].gameObject.layer = LayerMask.NameToLayer (_layer);
				}
            }
        }
    }

	public static void SetShadersForCanvas3D (GameObject _go) {
        Transform [] children = _go.GetComponentsInChildren<Transform>();
        foreach(Transform child in children){
            Renderer mr = child.GetComponent<Renderer>();
            if (mr && mr.material.shader.name == "Particles/Additive") {
                mr.material.shader = (Shader)Resources.Load ("Shaders/Particles/Particles Add");
            } else if (mr && mr.material.shader.name == "Particles/Additive (Soft)") {
                mr.material.shader = (Shader)Resources.Load ("Shaders/Particles/Particles AddSoft");
            }
        }
	}

    // ======================================================================================================================
    // TOUCH
    // ======================================================================================================================

	private static float m_fLastPressTimestamp = float.MinValue;
	private static float m_fLastReleaseTimestamp = float.MinValue;

    /*
     * Return true when the user has pressed the screen
     */
    public static bool hasPressed () {
        if (Application.platform != RuntimePlatform.IPhonePlayer) {
            if (Input.GetMouseButtonDown (0) == true) {
				m_fLastPressTimestamp = Time.time;
                return true;
            }
        } else {
			if (Input.touches.Length > 0 && Input.touches [0].phase == TouchPhase.Began) {
				m_fLastPressTimestamp = Time.time;
                return true;
            }
        }

        return false;
    }

	/*
     * Return true when the user has touched the screen (meaning pressed and unpressed in a short period of time)
     */
	public static bool hasTouched(float minTime = 0.5f) {

		return hasReleased() && m_fLastPressTimestamp > m_fLastReleaseTimestamp && m_fLastPressTimestamp - m_fLastReleaseTimestamp >= minTime;

	}
    
    /*
     * Return true when the user has untouched the screen
     */
    public static bool hasReleased () {
        if (Application.platform != RuntimePlatform.IPhonePlayer) {
			if (Input.GetMouseButtonUp (0) == true) {
				m_fLastReleaseTimestamp = Time.time;
                return true;
            }
        } else {
            if (Input.touches.Length > 0 && Input.touches [0].phase == TouchPhase.Ended) {
				m_fLastReleaseTimestamp = Time.time;
                return true;
            }
        }

        return false;
    }
    
    /*
     * Return the position of the touch or an invalid point
     */
    public static Vector3 getTouchPosition () {
        if (Application.platform != RuntimePlatform.IPhonePlayer) {
            if (Input.GetMouseButtonDown (0) == true) {
                Vector2 touchPos = Input.mousePosition;
                return new Vector3 (touchPos.x, touchPos.y, 10f);
            }
        } else {
            if (Input.touches.Length > 0) {
                Vector2 touchPos = Input.touches [0].position;
                return new Vector3 (touchPos.x, touchPos.y, 10f);
            }
        }

        return new Vector3 (-1000f, -1000f, 10f);
    }
    
    public static Vector3 getUntouchPosition () {
        if (Application.platform != RuntimePlatform.IPhonePlayer) {
            if (Input.GetMouseButtonUp (0) == true) {
                Vector2 touchPos = Input.mousePosition;
                return new Vector3 (touchPos.x, touchPos.y, 10f);
            }
        } else {
            if (Input.touches.Length > 0) {
                Vector2 touchPos = Input.touches [0].position;
                return new Vector3 (touchPos.x, touchPos.y, 10f);
            }
        }
        
        return new Vector3 (-1000f, -1000f, 10f);
    }

	private static bool m_sTouchesEnabledForCanvas3D = true;
	public static void setTouchesEnabledForCanvas3D(bool enabled) {
		m_sTouchesEnabledForCanvas3D = enabled;
	}

	public static bool getTouchesEnabledForCanvas3D() {
		return m_sTouchesEnabledForCanvas3D;
	}

    // ======================================================================================================================
    // ALPHA
    // ======================================================================================================================
    /*
     * Gets the alpha of a game object
     * @param _go   Game object to check the alpha
     */
    public static float getAlpha (GameObject _go) {
        if (_go != null) {
            if (_go.GetComponent<CanvasGroup> () != null) {
                return _go.GetComponent<CanvasGroup> ().alpha;
            } else if (_go.GetComponent<SpriteRenderer> () != null) {
                return _go.GetComponent<SpriteRenderer> ().color.a;
            } else if (_go.GetComponent<Renderer>() != null) {
                return _go.GetComponent<Renderer>().material.color.a;
            } else if (_go.GetComponent<MeshRenderer>() != null) {
                return _go.GetComponent<MeshRenderer> ().material.color.a;
            } else {
                //DebugConsole.DebugMsg ("This object doesn't have alpha");
                return 0f;
            }
        }

        return 0f;
    }

    /*
     * Sets the alpha of a game object
     * @param _go       Game object to check the alpha
     * @param _alpha    Alpha to be set
     */
    public static void setAlpha (ref GameObject _go, float _alpha) {
        if (_go != null) {
            if (_go.GetComponent<CanvasGroup> () != null) {
                _go.GetComponent<CanvasGroup> ().alpha = _alpha;
            } else if (_go.GetComponent<SpriteRenderer> () != null) {
                Color color = _go.GetComponent<SpriteRenderer> ().color;
                color.a = _alpha;
                _go.GetComponent<SpriteRenderer> ().color = color;
            } else if (_go.GetComponent<Renderer>() != null) {
                Color color = _go.GetComponent<Renderer>().material.color;
                color.a = _alpha;
                _go.GetComponent<Renderer>().material.color = color;
            } else if (_go.GetComponent<MeshRenderer>() != null) {
                foreach (Material m in _go.GetComponent<MeshRenderer> ().materials) {
                    Color color = m.color;
                    color.a = _alpha;
                    m.color = color;
                }
            } else {
                Debug.Log ("This object doesn't have alpha");
            }
        }
    }

    // ==============================================================================================================
    // COLORS
    // ==============================================================================================================
    /*
     * Converts a char in its equivalent hex number
     * @param _char  Char to be converted
     */
    static int _hexToInt (char _char) {
        if      (_char == '0')                  return 0;
        else if (_char == '1')                  return 1;
        else if (_char == '2')                  return 2;
        else if (_char == '3')                  return 3;
        else if (_char == '4')                  return 4;
        else if (_char == '5')                  return 5;
        else if (_char == '6')                  return 6;
        else if (_char == '7')                  return 7;
        else if (_char == '8')                  return 8;
        else if (_char == '9')                  return 9;
        else if (_char == 'A' || _char == 'a')  return 10;
        else if (_char == 'B' || _char == 'b')  return 11;
        else if (_char == 'C' || _char == 'c')  return 12;
        else if (_char == 'D' || _char == 'd')  return 13;
        else if (_char == 'E' || _char == 'e')  return 14;
        else if (_char == 'F' || _char == 'f')  return 15;
        return 0;
    }
    
    static char __intToHex (int _char) {
        if      (_char == 0)  return '0';
        else if (_char == 1)  return '1';
        else if (_char == 2)  return '2';
        else if (_char == 3)  return '3';
        else if (_char == 4)  return '4';
        else if (_char == 5)  return '5';
        else if (_char == 6)  return '6';
        else if (_char == 7)  return '7';
        else if (_char == 8)  return '8';
        else if (_char == 9)  return '9';
        else if (_char == 10) return 'a';
        else if (_char == 11) return 'b';
        else if (_char == 12) return 'c';
        else if (_char == 13) return 'd';
        else if (_char == 14) return 'e';
        else if (_char == 15) return 'f';
        return '0';
    }

    public static Color hexStrtoColor (string _color) {
        string red   = _color.Substring (0, 2);
        string green = _color.Substring (2, 2);
        string blue  = _color.Substring (4, 2);
        
        return new Color
            (
                (_hexToInt (red   [0]) * 16 + _hexToInt (red   [1])) / 255f,
                (_hexToInt (green [0]) * 16 + _hexToInt (green [1])) / 255f,
                (_hexToInt (blue  [0]) * 16 + _hexToInt (blue  [1])) / 255f
                );
    }

    public static string colortoHexStr (Color _color) {
		return _color.ToHexString ().Remove (6);
    }

    public static Color hexStrstoColor (string [] _color) {
        if (_color.Length != 3) {
            return Color.white;
        }

        return new Color
            (
                (_hexToInt (_color [0][0]) * 16 + _hexToInt (_color [0][1])) / 255f,
                (_hexToInt (_color [1][0]) * 16 + _hexToInt (_color [1][1])) / 255f,
                (_hexToInt (_color [2][0]) * 16 + _hexToInt (_color [2][1])) / 255f
                );
    }

    public static Color intStrstoColor (string [] _color) {
        if (_color.Length != 3) {
            return Color.white;
        }

        return new Color
            (
                float.Parse (_color [0]) / 255f,
                float.Parse (_color [1]) / 255f,
                float.Parse (_color [2]) / 255f
                );
    }

    public static Vector3 floatStrstoVector3 (string [] _vct) {
        if (_vct.Length != 3) {
            return Vector3.zero;
        }

        float x = float.Parse (_vct [0]);
        float y = float.Parse (_vct [1]);
        float z = float.Parse (_vct [2]);

        return new Vector3 (x, y, z);
    }

    // ==============================================================================================================
    // TOGGLE
    // ==============================================================================================================
    public static void configToggle (GameObject _toggle) {
        if (_toggle != null) {
            Transform [] ts = _toggle.GetComponentsInChildren<Transform> ();
            foreach (Transform child in ts) {
                if (child.name == "Label") {
                    if (!_toggle.GetComponent<Toggle> ().interactable) {
                        child.gameObject.GetComponent<Text> ().color = Color.grey;
                    } else {
                        if (_toggle.GetComponent<Toggle> ().isOn) {
                            child.gameObject.GetComponent<Text> ().color = Color.green;
                        } else {
                            child.gameObject.GetComponent<Text> ().color = Color.red;
                        }
                    }
                }
            }
        }
    }

    public static bool setToggleValue (GameObject _go, bool _value) {
        if (_go != null &&_go.GetComponent <Toggle> () != null) {
            _go.GetComponent <Toggle> ().isOn = _value;
            configToggle (_go);
            return true;
        }
        return false;
    }

    public static bool setToggleCallback (GameObject _go, UnityEngine.Events.UnityAction <bool> _callback) {
        if (_callback != null && _go != null && _go.GetComponent <Toggle> () != null) {
            _go.GetComponent <Toggle> ().onValueChanged.AddListener (_callback);
            return true;
        }
        return false;
    }

    public static bool setToggleEnabled (GameObject _go, bool _interactable) {
        if (_go != null && _go.GetComponent <Toggle> () != null) {
            _go.GetComponent <Toggle> ().interactable = _interactable;
            return true;
        }
        return false;
    }

    // ==============================================================================================================
    // INPUT FIELD
    // ==============================================================================================================
    public static void configInputField (GameObject _inputField, string _value) {
        if (_inputField != null) {
            Transform [] ts = _inputField.GetComponentsInChildren<Transform> ();
            foreach (Transform child in ts) {
                if (child.name == "Placeholder") {
                    _inputField.GetComponent<Text> ().text = _value;
                }
            }
        }
    }

    public static bool setInputFieldValue (GameObject _go, string _value) {
        if (_go != null && _go.GetComponent <InputField> () != null) {
            configInputField (_go, _value);
            return true;
        }
        return false;
    }

    public static bool setInputFieldCallback (GameObject _go, UnityEngine.Events.UnityAction <string> _callback) {
        if (_callback != null && _go != null && _go.GetComponent <InputField> () != null) {
            _go.GetComponent <InputField> ().onValueChanged.AddListener (_callback);
            return true;
        }
        return false;
    }

    // ==============================================================================================================
    // BUTTON
    // ==============================================================================================================
    public static bool setButtonCallback (GameObject _go, UnityEngine.Events.UnityAction _callback) {
        if (_callback != null && _go != null && _go.GetComponent <Button> () != null) {
            _go.GetComponent <Button> ().onClick.AddListener (_callback);
            return true;
        }
        return false;
    }

    public static bool setButtonEnabled (GameObject _go, bool _interactable) {
        if (_go != null && _go.GetComponent <Button> () != null) {
            _go.GetComponent <Button> ().interactable = _interactable;
            return true;
        }
        return false;
    }

    // ==============================================================================================================
    // SLIDER
    // ==============================================================================================================
    public static bool setSliderCallback (GameObject _go, UnityEngine.Events.UnityAction <float> _callback) {
        if (_callback != null && _go != null && _go.GetComponent <Slider> () != null) {
            _go.GetComponent <Slider> ().onValueChanged.AddListener (_callback);
            return true;
        }
        return false;
    }

    public static bool setSliderValue (GameObject _go, float _value) {
        if (_value >= 0 && _value <= 1 && _go != null && _go.GetComponent <Slider> () != null) {
            _go.GetComponent <Slider> ().value = _value;
            return true;
        }
        return false;
    }

    public static bool setSliderEnabled (GameObject _go, bool _interactable) {
        if (_go != null && _go.GetComponent <Slider> () != null) {
            _go.GetComponent <Slider> ().interactable = _interactable;
            return true;
        }
        return false;
    }

    // ==============================================================================================================
    // TEXT
    // ==============================================================================================================
    public static bool setTextValue (GameObject _go, string _value) {
        if (_go != null && _go.GetComponent <Text> () != null) {
            _go.GetComponent <Text> ().text = _value;
            return true;
        }
        return false;
    }

    // ==============================================================================================================
    // HIERARCHY
    // ==============================================================================================================
    public static bool is1stChildOf2nd (GameObject _1st, GameObject _2nd) {
        if (_1st != null && _2nd != null) {
            Transform t = _1st.transform;
            while (t != null) {
                if (t.gameObject == _2nd.gameObject) {
                    return true;
                }
                t = t.transform.parent;
            }
        }

        return false;
    }


    // ==============================================================================================================
    // TRIGGERS
    // ==============================================================================================================
    public static void uiTrigger (string _trigger) {
    }

    public static void stopUITrigger (string _trigger) {
    }

    public static void uiTrigger (string _trigger, GameObject _base) {
    }

    public static void uiTrigger (string _trigger, GameObject _base, float _timeOverride) {
    }

    // ==============================================================================================================
    // RAWIMAGES
    // ==============================================================================================================
    public static void loadRawImageKeepingHeightAR (GameObject _container, string _path) {
        if (_container != null) {
            if (_container.GetComponent <RawImage> () != null) {
                Vector2 prev = new Vector2 (_container.GetComponent <RectTransform> ().rect.width, _container.GetComponent <RectTransform> ().rect.height);
                Texture t = (Texture)Resources.Load (_path);
                if (t != null) {
                    _container.GetComponent <RawImage> ().texture = t;
                    _container.GetComponent <RawImage> ().SetNativeSize ();
                    Vector2 current = new Vector2 (_container.GetComponent <RectTransform> ().rect.width, _container.GetComponent <RectTransform> ().rect.height);

                    if (prev.y != current.y) {
                        float factor = prev.y / current.y;
                        _container.GetComponent <RectTransform> ().sizeDelta = current * factor;
                    }
                }
            }
            else if (_container.GetComponent <Image> () != null) {
                Vector2 prev = new Vector2 (_container.GetComponent <RectTransform> ().rect.width, _container.GetComponent <RectTransform> ().rect.height);
                Sprite s = Resources.Load <Sprite> (_path);
                if (s != null) {
                    _container.GetComponent <Image> ().sprite = s;
                    _container.GetComponent <Image> ().SetNativeSize ();
                    Vector2 current = new Vector2 (_container.GetComponent <RectTransform> ().rect.width, _container.GetComponent <RectTransform> ().rect.height);

                    if (prev.y != current.y) {
                        float factor = prev.y / current.y;
                        _container.GetComponent <RectTransform> ().sizeDelta = current * factor;
                    }
                }
            }
        }
    }

    public static void loadRawImageKeepingHeightAR (GameObject _container, Texture _tex) {
        if (_container != null && _container.GetComponent <RawImage> () != null) {
            Vector2 prev = new Vector2 (_container.GetComponent <RectTransform> ().rect.width, _container.GetComponent <RectTransform> ().rect.height);
            if (_tex != null) {
                _container.GetComponent <RawImage> ().texture = _tex;
                _container.GetComponent <RawImage> ().SetNativeSize ();
                Vector2 current = new Vector2 (_container.GetComponent <RectTransform> ().rect.width, _container.GetComponent <RectTransform> ().rect.height);

                if (prev.y != current.y) {
                    float factor = prev.y / current.y;
                    _container.GetComponent <RectTransform> ().sizeDelta = current * factor;
                }
            }
        }
    }

    public static void loadRawImageKeepingWidthAR (GameObject _container, string _path) {
        if (_container != null) {
            if (_container.GetComponent <RawImage> () != null) {
                Vector2 prev = new Vector2 (_container.GetComponent <RectTransform> ().rect.width, _container.GetComponent <RectTransform> ().rect.height);
                Texture t = (Texture)Resources.Load (_path);
                if (t != null) {
                    _container.GetComponent <RawImage> ().texture = t;
                    _container.GetComponent <RawImage> ().SetNativeSize ();
                    Vector2 current = new Vector2 (_container.GetComponent <RectTransform> ().rect.width, _container.GetComponent <RectTransform> ().rect.height);

                    if (prev.x != current.x) {
                        float factor = prev.x / current.x;
                        _container.GetComponent <RectTransform> ().sizeDelta = current * factor;

                        current = new Vector2 (_container.GetComponent <RectTransform> ().rect.width, _container.GetComponent <RectTransform> ().rect.height);
                        if (current.y > prev.y) {
                            factor = prev.y / current.y;
                            _container.GetComponent <RectTransform> ().sizeDelta = current * factor;
                        }
                    }
                }
            }
            else if (_container.GetComponent <Image> () != null) {
                Vector2 prev = new Vector2 (_container.GetComponent <RectTransform> ().rect.width, _container.GetComponent <RectTransform> ().rect.height);
                Sprite s = Resources.Load <Sprite> (_path);
                if (s != null) {
                    _container.GetComponent <Image> ().sprite = s;
                    _container.GetComponent <Image> ().SetNativeSize ();
                    Vector2 current = new Vector2 (_container.GetComponent <RectTransform> ().rect.width, _container.GetComponent <RectTransform> ().rect.height);

                    if (prev.x != current.x) {
                        float factor = prev.x / current.x;
                        _container.GetComponent <RectTransform> ().sizeDelta = current * factor;

                        current = new Vector2 (_container.GetComponent <RectTransform> ().rect.width, _container.GetComponent <RectTransform> ().rect.height);
                        if (current.y > prev.y) {
                            factor = prev.y / current.y;
                            _container.GetComponent <RectTransform> ().sizeDelta = current * factor;
                        }
                    }
                }
            }
        }
    }

    public static void enableButton (GameObject _button, bool _enabled, string _path = "") {
        if (_button != null && _button.GetComponent <Image> () != null) {
            if (_enabled) {
                _button.GetComponent <Image> ().sprite = Resources.Load <Sprite> ("AR/UI/Assets/Buttons/Regular/btn-active");
                Text t = _button.GetComponentInChildren <Text> ();
                if (t != null) {
                    t.color = UIUtils.hexStrtoColor ("593902");
                    if (t.GetComponent <Shadow> () != null) {
                        t.GetComponent <Shadow> ().effectColor = UIUtils.hexStrtoColor ("d2c29f");
                        Vector2 v = t.GetComponent <Shadow> ().effectDistance;
                        v.y = -1;
                        t.GetComponent <Shadow> ().effectDistance = v;
                    }
                }
            } else {
                _button.GetComponent <Image> ().sprite = Resources.Load <Sprite> ("AR/UI/Assets/Buttons/Regular/brown-up");
                Text t = _button.GetComponentInChildren <Text> ();
                if (t != null) {
                    t.color = Color.white;
                    if (t.GetComponent <Shadow> () != null) {
                        t.GetComponent <Shadow> ().effectColor = Color.black;
                        Vector2 v = t.GetComponent <Shadow> ().effectDistance;
                        v.y = -4;
                        t.GetComponent <Shadow> ().effectDistance = v;
                    }
                }
            }

            if (_path != "") {
                Transform [] ts = _button.GetComponentsInChildren <Transform> ();
                foreach (Transform child in ts) {
                    if (child != _button.transform) {
                        if (child.GetComponent <Image> () != null) {
                            string name = child.GetComponent <Image> ().sprite.name;
                            name = name.Replace ("-active", "");
                            if (_enabled) {
                                child.GetComponent <Image> ().sprite = Resources.Load <Sprite> (_path + "/" + name + "-active");
                            }
                            else {
                                child.GetComponent <Image> ().sprite = Resources.Load <Sprite> (_path + "/" + name);
                            }
                        }
                        else if (child.GetComponent <RawImage> () != null) {
                            string name = child.GetComponent <RawImage> ().texture.name;
                            name = name.Replace ("-active", "");
                            if (_enabled) {
                                child.GetComponent <RawImage> ().texture = Resources.Load <Texture> (_path + "/" + name + "-active");
                            }
                            else {
                                child.GetComponent <RawImage> ().texture = Resources.Load <Texture> (_path + "/" + name);
                            }
                        }
                    }
                }
            }
        }
    }

    public static string removeUnnecessaryCommas (string _number) {
        int posOfComma = _number.IndexOf (".");
        if (posOfComma != -1) {
            for (int i = _number.Length - 1; i > posOfComma; --i) {
                if (_number [i] == '0') {
                    _number = _number.Remove (i);
                }
            }
        }

        if (posOfComma == _number.Length - 1) {
            _number = _number.Remove (posOfComma);
        }

        return _number;
    }

    public static string formatNumber (double _number, eNumberFormatType _nft, bool _useDecimals) {
        switch (_nft) {
            case eNumberFormatType.E_NONE:
                {
                    if (_useDecimals) {
                        return _number.ToString ();
                    }
                    else {
                        return (Math.Round (_number)).ToString ();
                    }
                }

            case eNumberFormatType.E_NORMAL_LETTER_MULTIPLIER:
                {
                    if (_number < 1) {
                        if (_useDecimals) {
                            return _number.ToString ("F2");
                        }
                        else {
                            return (Math.Round (_number)).ToString ();
                        }
                    } else if (_number < 10) {
                        if (_useDecimals) {
                            return _number.ToString ("F1");
                        }
                        else {
                            return (Math.Round (_number)).ToString ();
                        }
                    } else if (_number < 1000) {
                        return (Math.Round (_number)).ToString ();
                    } else if (_number < 1000000) {
                        return (_number * 0.001).ToString ("F1") + "K"; 
                    } else {
                        return (_number * 0.000001).ToString ("F1") + "M"; 
                    }
                }

            case eNumberFormatType.E_NORMAL_LETTER_MULTIPLIER_NO_DECIMALS_UNDER_1K:
                {
                    if (_number < 1000) {
                        return (Math.Round (_number)).ToString ();
                    } else if (_number < 1000000) {
                        return (_number * 0.001).ToString ("F1") + "K"; 
                    } else {
                        return (_number * 0.000001).ToString ("F1") + "M"; 
                    }
                }

            case eNumberFormatType.E_1K_LETTER_MULTIPLIER:
                {
                    if (_number < 1) {
                        if (_useDecimals) {
                            return _number.ToString ("F2");
                        }
                        else {
                            return (Math.Round (_number)).ToString ();
                        }
                    } else if (_number < 10) {
                        if (_useDecimals) {
                            return _number.ToString ("F1");
                        }
                        else {
                            return (Math.Round (_number)).ToString ();
                        }
                    } else if (_number < 1000) {
                        return (Math.Round (_number)).ToString ();
                    } else if (_number < 1000000) {
                        return (Math.Round (_number)).ToString ();
                    } else if (_number < 1000000000) {
                        return (_number * 0.001).ToString ("F1") + "K"; 
                    } else {
                        return (_number * 0.000001).ToString ("F1") + "M"; 
                    }
                }
        }

        return _number.ToString ();
    }

    public static void tintEvolutionItemAmount (GameObject _go, int _needed, int _current) {
        if (_go != null && _go.GetComponent <Text> () != null) {
            if (_current >= _needed) {
                _go.GetComponent <Text> ().color = new Color (9f / 255f, 1f, 0f, 1f);
            } else {
                if (_current == 0) {
                    _go.GetComponent <Text> ().color = new Color (1f, 1f, 1f, 81f / 255f);
                } else {
                    _go.GetComponent <Text> ().color = Color.white;
                }
            }

            _go.GetComponent <Text> ().text = _current.ToString () + " / " + _needed.ToString ();
        }
    }

    public static void autoTranslate (GameObject _go) {
    }

	public static void showFeedbackText(string _tid)
	{
	}

	public static void setText(GameObject _go, string _text)
	{
		if (_go != null) {
			if (_go.GetComponent<Text> () != null) {
				_go.GetComponent<Text> ().text = _text;
			}
		}
	}

    public static void showNotBlockingLoader(GameObject _parent)
    {
        GameObject asset = Resources.Load<GameObject>("UI/Prefabs/Art and FX/NotBlockingLoader");

        if (asset != null)
        {
            GameObject go = GameObject.Instantiate(asset) as GameObject;

            if (go != null)
            {
                go.transform.SetParent(_parent.transform);
                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale = Vector3.one;
            }
        }       
    }

    public static eScreenAspectRationType screenAspectRationType()
    {
        float ar = (float)Screen.width / (float) Screen.height;
        if (ar >= 1.9f)
        {
            return eScreenAspectRationType.E_2_1;
        }
        else if (ar >= 1.5f)
        {
            return eScreenAspectRationType.E_16_9;
        }

        return eScreenAspectRationType.E_4_3;

    }
}
