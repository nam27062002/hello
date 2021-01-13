// EditorUtils.cs
// 
// Created by Alger Ortín Castellví on 17/09/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Reflection;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Collection of static utilities related to custom editors.
/// </summary>
public static class EditorUtils {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public enum ObjectIcon {
		NONE = -2,
		CUSTOM = -1,
		LABEL_ICONS_START = 0,
			LABEL_GRAY = LABEL_ICONS_START,
			LABEL_BLUE,
			LABEL_TEAL,
			LABEL_GREEN,
			LABEL_YELLOW,
			LABEL_ORANGE,
			LABEL_RED,
			LABEL_PURPLE,
		LABEL_ICONS_END = LABEL_PURPLE,

		SHAPE_ICONS_START,
			CIRCLE_GRAY = SHAPE_ICONS_START,
			CIRCLE_BLUE,
			CIRCLE_TEAL,
			CIRCLE_GREEN,
			CIRCLE_YELLOW,
			CIRCLE_ORANGE,
			CIRCLE_RED,
			CIRCLE_PURPLE,
			DIAMOND_GRAY,
			DIAMOND_BLUE,
			DIAMOND_TEAL,
			DIAMOND_GREEN,
			DIAMOND_YELLOW,
			DIAMOND_ORANGE,
			DIAMOND_RED,
			DIAMOND_PURPLE,
		SHAPE_ICONS_END = DIAMOND_PURPLE,

		COUNT
	}

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// GameObject Icons
	private static GUIContent[] s_objectIcons;

	//------------------------------------------------------------------//
	// GAME OBJECT UTILS												//
	//------------------------------------------------------------------//
	/// <summary>
	/// Given a menu command, get the game object where it was triggered or, if not valid,
	/// optionally the current selected object.
	/// </summary>
	/// <returns>The target game object.</returns>
	/// <param name="_command">The menu command to be checked.</param>
	/// <param name="_defaultToSelected">If the object couldn't be picked from the menu command, pick it from the current selection?</param>
	public static GameObject GetContextObject(MenuCommand _command, bool _defaultToSelected = true) {
		// Try command
		if(_command != null) return _command.context as GameObject;

		// Default to selected object
		if(_defaultToSelected) return Selection.activeGameObject;

		// No valid option found
		return null;
	}

	/// <summary>
	/// Create a new GameObject in the scene hierarchy.
	/// </summary>
	/// <returns>The newly created object.</returns>
	/// <param name="_name">The name to give to the new object.</param>
	/// <param name="_parent">The parent where the new object should be attached. If <c>null</c>, the object will be created at the hierarchy's root.</param>
	/// <param name="_focus">Whether to select and focus the newly created game object or not.</param>
	public static GameObject CreateGameObject(string _name, GameObject _parent, bool _focus) {
		// Create the object
		GameObject newObj = new GameObject(_name);

		// Put into hierarchy
		Undo.RegisterCreatedObjectUndo(newObj, "Create " + _name);
		if(_parent != null) {
			Undo.SetTransformParent(newObj.transform, _parent.transform, "Parent " + _name);
			GameObjectUtility.SetParentAndAlign(newObj, _parent);
		}

		// Focus new object and return
		if(_focus) FocusObject(newObj, true, true, false);
		return newObj;
	}

	/// <summary>
	/// Create a new GameObject in the scene hierarchy - special case for UI objects.
	/// </summary>
	/// <returns>The newly created object.</returns>
	/// <param name="_name">The name to give to the new object.</param>
	/// <param name="_parent">The parent where the new object should be attached. If <c>null</c>, the object will be created at the hierarchy's root.</param>
	public static GameObject CreateUIGameObject(string _name, GameObject _parent) {
		// If no parent is given or the parent hierarchy has no canvas attached, create a new canvas and use it as a parent instead
		Canvas canvas = (_parent != null) ? _parent.GetComponentInParent<Canvas>() : null;
		if(canvas == null) {
			// Try to use any canvas on the scene..
			canvas = UnityEngine.Object.FindObjectOfType<Canvas>();
			if(canvas != null && canvas.gameObject.activeInHierarchy) {
				_parent = canvas.gameObject;
			}

			// No canvas in the scene at all? Then create a new one.
			else {
				// Create a new canvas
				GameObject canvasObj = CreateGameObject("Canvas", _parent, false);
				canvasObj.layer = LayerMask.NameToLayer("UI");
				canvas = canvasObj.AddComponent<Canvas>();
				canvas.renderMode = RenderMode.ScreenSpaceOverlay;
				canvasObj.AddComponent<CanvasScaler>();
				canvasObj.AddComponent<GraphicRaycaster>();
				_parent = canvasObj;

				// If there is no event system, create one as well
				EventSystem esys = UnityEngine.Object.FindObjectOfType<EventSystem>();
				if(esys == null) {
					GameObject esysObj = CreateGameObject("EventSystem", null, false);
					esysObj.AddComponent<EventSystem>();
					esysObj.AddComponent<StandaloneInputModule>();
					esysObj.AddComponent<TouchInputModule>();
				}
			}
		}

		// Use standard GameObject creator
		GameObject newObj = CreateGameObject(_name, _parent, true);

		// Since it's a UI object, set layer and add the RectTransform component
		RectTransform rectTransform = newObj.AddComponent<RectTransform>();
		rectTransform.sizeDelta = Vector2.one * 100f;	// Default size

		// Done!
		return newObj;
	}

	//------------------------------------------------------------------//
	// OBJECT ICONS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Get the current icon of the given GameObject.
	/// </summary>
	/// <returns>The icon texture.</returns>
	/// <param name="_obj">The game object whose icon we want.</param>
	public static Texture2D GetObjectIcon(GameObject _obj) {
		// Reflection black magic since the GetIconForObject method is internal
		MethodInfo method = typeof(EditorGUIUtility).GetMethod("GetIconForObject", BindingFlags.NonPublic | BindingFlags.Static);
		return method.Invoke(null, new object[] { _obj}) as Texture2D;
	}

	/// <summary>
	/// Get the current icon of the given GameObject if it's one of the predefined.
	/// </summary>
	/// <returns>The enum value of the object icon, NONE if it's not defined or CUSTOM if it's not one of the predefined.</returns>
	/// <param name="_obj">The object whose icon we want.</param>
	public static ObjectIcon GetObjectIconEnum(GameObject _obj) {
		// Aux vars
		Texture2D currentTex = GetObjectIcon(_obj);

		// Is it null?
		if(currentTex == null) return ObjectIcon.NONE;

		// Let's try to figure out whether the current icon matches one of the default ones
		InitDefaultIconsCache();	// Make sure cached textures are initialized
		for(int i = 0; i < s_objectIcons.Length; i++) {
			Texture2D tex = s_objectIcons[i].image as Texture2D;
			if(tex == currentTex) {
				return (ObjectIcon)i;
			}
		}

		// Texture not found within the default ones, return custom
		return ObjectIcon.CUSTOM;
	}

	/// <summary>
	/// Manually defines the icon of the given game object.
	/// </summary>
	/// <param name="_obj">The object whose icon we want to change.</param>
	/// <param name="_text">The texture to be used as icon for the given object.</param>
	public static void SetObjectIcon(GameObject _obj, Texture2D _tex) {
		// Apply the icon - reflection black magic since the SetIconForObject method is internal
		MethodInfo method = typeof(EditorGUIUtility).GetMethod("SetIconForObject", BindingFlags.NonPublic | BindingFlags.Static);
		method.Invoke(null, new object[] { _obj, _tex});
	}

	/// <summary>
	/// Defines the icon of the given game object (from Unity's editor default icons).
	/// </summary>
	/// <param name="_obj">The object whose icon we want to change.</param>
	/// <param name="_icon">The icon to be applied.</param>
	public static void SetObjectIcon(GameObject _obj, ObjectIcon _icon) {
		// Special case if NONE or CUSTOM
		if(_icon == ObjectIcon.NONE || _icon == ObjectIcon.CUSTOM) {
			SetObjectIcon(_obj, null);
			return;
		}

		// Apply the icon using texture method
		InitDefaultIconsCache();	// Make sure cache is initialized
		Texture2D tex = s_objectIcons[(int)_icon].image as Texture2D;
		SetObjectIcon(_obj, tex);
	}

	/// <summary>
	/// Changes the icon of the given object to match its prefab preview.
	/// </summary>
	/// <param name="_obj">The object whose icon we want to change.</param>
	public static void SetObjectIcon(GameObject _obj) {
		// Get the preview texture and set it
		Texture2D iconTex = AssetPreview.GetAssetPreview(_obj);
		SetObjectIcon(_obj, iconTex);
	}

	/// <summary>
	/// Initialize the default object icons. Safe to spam it, will only happen if the cache was not initialized.
	/// </summary>
	private static void InitDefaultIconsCache() {
		// From http://sassybot.com/blog/snippet-automatically-add-scene-labels/
		// If textures haven't yet been cached, do it now
		if(s_objectIcons == null) {
			s_objectIcons = new GUIContent[(int)ObjectIcon.COUNT];
			
			for(int i = 0; i < (int)ObjectIcon.COUNT; i++) {
				// Label icons go apart
				if(i <= (int)ObjectIcon.LABEL_ICONS_END) {
					s_objectIcons[i] = EditorGUIUtility.IconContent("sv_label_" + i);
				} else {
					s_objectIcons[i] = EditorGUIUtility.IconContent("sv_icon_dot" + (i - (int)ObjectIcon.SHAPE_ICONS_START) + "_pix16_gizmo");
				}
			}
		}
	}

	//------------------------------------------------------------------//
	// SCENE UTILS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Focuses the given object.
	/// </summary>
	/// <param name="_obj">The object to be focused.</param>
	/// <param name="_select">Make it selected object?</param>
	/// <param name="_focusScene">Focus scene camera on it? Overrides _select parameter if true.</param>
	/// <param name="_ping">Ping effect. Overrides _select parameter if true.</param>
	public static void FocusObject(UnityEngine.Object _obj, bool _select = true, bool _focusScene = true, bool _ping = true) {
		if(_select || _focusScene || _ping) Selection.activeObject = _obj;	// In order to ping/frame the object, it must be selected first
		if(_ping) EditorGUIUtility.PingObject(_obj);
		if(_focusScene) FocusWorldPosition(Selection.activeGameObject.transform.position, false);
	}

	/// <summary>
	/// Focus current scene to the target world position.
	/// </summary>
	/// <param name="_worldPos">World position to be focused.</param>
	/// <param name="_zoom">Whether to try to adjust zoom as well or not.</param> 
	public static void FocusWorldPosition(Vector3 _worldPos, bool _zoom) {
		// Find the best scene view
		SceneView sceneView = SceneView.lastActiveSceneView;
		if(sceneView == null && SceneView.sceneViews.Count > 0) {
			sceneView = SceneView.sceneViews[0] as SceneView;
		}

		// If a SceneView couldn't be found, skip focusing
		if(sceneView != null && sceneView.camera != null) {
			// [AOC] The framing changes the camera zooming, which can result quite annoying - use LookAt method instead ^______^
			if(_zoom) {
				sceneView.FrameSelected();
			} else {
				sceneView.LookAt(_worldPos);
			}
		}
	}

	//------------------------------------------------------------------//
	// SCRIPTABLE OBJECTS MANAGEMENT									//
	//------------------------------------------------------------------//
	/// <summary>
	///	This makes it easy to create, name and place unique new ScriptableObject asset files.
	/// Based in http://wiki.unity3d.com/index.php?title=CreateScriptableObjectAsset
	/// </summary>
	/// <returns>The newly created asset.</returns>
	/// <param name="_name">The name for the new asset.</param>
	/// <param name="_path">The path where to store the new asset, typically "Assets/MyFolder". Leave empty to try to fetch currentl path in the project window.</param>
	public static T CreateScriptableObjectAsset<T>(string _name, string _path = "") where T : ScriptableObject {
		// Create a new instance of the given scriptable object
		T asset = ScriptableObject.CreateInstance<T>();

		// Is there a path already defined?
		if(_path == "") {
			// Try to put it in the currently selected folder
			_path = AssetDatabase.GetAssetPath(Selection.activeObject);
			if(_path == "") {
				_path = "Assets";
			} else if(Path.GetExtension(_path) != "") {
				_path = _path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
			}
		}

		// Compose full path and create asset
		if(_name == "") _name = "New " + typeof(T).ToString();
		_path = AssetDatabase.GenerateUniqueAssetPath(_path + "/" + _name + ".asset");
		AssetDatabase.CreateAsset(asset, _path);

		// Save asset database and select newly created object
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
		EditorUtility.FocusProjectWindow();
		Selection.activeObject = asset;

		return asset;
	}

	//------------------------------------------------------------------//
	// ASSETS MANAGEMENT												//
	//------------------------------------------------------------------//
	/// <summary>
	/// Extension of the AssetDatabase.LoadAssetAtPath method to load ALL the assets in a given directory.
	/// </summary>
	/// <returns>An array with all the assets found under the given directory.</returns>
	/// <param name="_dirPath">The path of the directory to be checked, starting at the "Assets" folder. Example "MyAssetsDir/".</param>
	/// <param name="_fileExtension">File extension for the expected type T. Must match. Example "mat" for Materials, "asset" for ScriptableObjects or "prefab" for GameObjects.</param>
	/// <param name="_recursive">Whether to include nested directories.</param>
	/// <typeparam name="T">The type of asset we're loading.</typeparam>
	public static T[] LoadAllAssetsAtPath<T>(string _dirPath, string _fileExtension, bool _recursive) where T : UnityEngine.Object {
		string[] files = Directory.GetFiles(Application.dataPath + "/" + _dirPath, "*." + _fileExtension, _recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
		files.SortAlphanumeric();	// [AOC] Yeah, we don't want _0, _1, _10, _11..., we want it as it's displayed in the project window
		T[] assets = new T[files.Length];
		for(int i = 0; i < files.Length; i++) {
			string assetPath = "Assets" + files[i].Replace(Application.dataPath, "").Replace('\\', '/');
			assets[i] = AssetDatabase.LoadAssetAtPath<T>(assetPath);
		}
		return assets;
	}

	/// <summary>
	/// Find all prefabs containing a specific component (T).
	/// </summary>
	/// <param name="_path">Optionally select a single resources path to be explored.</param>
	/// <typeparam name="T">The type of component</typeparam>
	public static List<GameObject> LoadPrefabsContaining<T>(string _path = "") where T : UnityEngine.Component {
		// Aux vars
		List<GameObject> result = new List<GameObject>();

		// Load all prefabs in the target path
		GameObject[] allPrefabs = Resources.LoadAll<GameObject>(_path);

		// Find those containing the required component
		for(int i = 0; i < allPrefabs.Length; i++) {
			if(allPrefabs[i].GetComponent<T>() != null) {
				result.Add(allPrefabs[i]);
			}
		}
		return result;
	}

	/// <summary>
	/// Given a serialized property of type ObjectReference, return the name of the expected object type.
	/// </summary>
	/// <returns>The name of the property's reference object expected type.</returns>
	/// <param name="_property">The property to be analyzed.</param>
	public static string GetPropertyObjectReferenceTypeName(SerializedProperty _property) {
		// Ignore if not of objectReferenceType
		if(_property.propertyType != SerializedPropertyType.ObjectReference) return "";

		// Black magic using reflection
		// From http://answers.unity3d.com/questions/929293/get-field-type-of-serializedproperty.html
		// Get type name
		string typeName = _property.type;
		Match match = Regex.Match(typeName, @"PPtr<\$(.*?)>");
		if(match.Success) {
			typeName = match.Groups[1].Value;
		}
		return typeName;
	}

	/// <summary>
	/// Given a serialized property of type ObjectReference, return the possible expected object types.
	/// </summary>
	/// <returns>All the possible Types matching the property's reference object type name.</returns>
	/// <param name="_property">The property to be analyzed.</param>
	public static Type[] GetPropertyObjectReferenceTypes(SerializedProperty _property) {
		// Ignore if not of objectReferenceType
		if(_property.propertyType != SerializedPropertyType.ObjectReference) return null;

		// Find matching types!
		// TypeUtils help us with that
		string typeName = GetPropertyObjectReferenceTypeName(_property);
		Type[] types = TypeUtil.GetTypesByClassName(typeName);
		return types;
	}

	/// <summary>
	/// Create a texture with the given values.
	/// </summary>
	/// <returns>The newly created texture.</returns>
	/// <param name="_color">Color of the texture.</param>
	/// <param name="_width">Width in pixels of the new texture.</param>
	/// <param name="_height">Height in pixels of the new texture.</param>
	public static Texture2D CreateTexture(Color _color, int _width = 2, int _height = 2) {
		return Texture2DExt.Create(_color, _width, _height);
	}

	//------------------------------------------------------------------//
	// SERIALIZED PROPERTIES UTILS										//
	//------------------------------------------------------------------//
	/// <summary>
	/// Extension method to reset the value of a serialized property to a default.
	/// </summary>
	/// <param name="_prop">Property to be modified.</param>
	public static void ResetValue(this SerializedProperty _prop) {
		// Use an arbitrary default value for 
		switch(_prop.propertyType) {
			case SerializedPropertyType.AnimationCurve: _prop.animationCurveValue = default(AnimationCurve); break;
			case SerializedPropertyType.Boolean: _prop.boolValue = default(bool); break;
			case SerializedPropertyType.Bounds: _prop.boundsValue = default(Bounds); break;
			case SerializedPropertyType.Color: _prop.colorValue = default(Color); break;
			case SerializedPropertyType.Enum: _prop.enumValueIndex = 0; break;
			case SerializedPropertyType.Float: _prop.floatValue = default(float); break;
			case SerializedPropertyType.Integer: _prop.intValue = default(int); break;
			case SerializedPropertyType.LayerMask: _prop.intValue = 0; break;
			case SerializedPropertyType.ObjectReference: _prop.objectReferenceValue = null; break;
			case SerializedPropertyType.Quaternion: _prop.quaternionValue = default(Quaternion); break;
			case SerializedPropertyType.Rect: _prop.rectValue = default(Rect); break;
			case SerializedPropertyType.String: _prop.stringValue = default(string); break;
			case SerializedPropertyType.Vector2: _prop.vector2Value = default(Vector2); break;
			case SerializedPropertyType.Vector3: _prop.vector3Value = default(Vector3); break;
			case SerializedPropertyType.Vector4: _prop.vector4Value = default(Vector4); break;
			case SerializedPropertyType.ArraySize: _prop.arraySize = 0;	break;
		}
	}

	/// <summary>
	/// Get the current value of the property based on its type.
	/// </summary>
	/// <returns>The value.</returns>
	/// <param name="_prop">Property to be checked.</param>
	public static object GetValue(this SerializedProperty _prop) {
		// Get the value from the right field based on type
		switch(_prop.propertyType) {
			case SerializedPropertyType.AnimationCurve: return _prop.animationCurveValue;
			case SerializedPropertyType.Boolean: return _prop.boolValue;
			case SerializedPropertyType.Bounds: return _prop.boundsValue;
			case SerializedPropertyType.Color: return _prop.colorValue;
			case SerializedPropertyType.Enum: return _prop.enumValueIndex;
			case SerializedPropertyType.Float: return _prop.floatValue;
			case SerializedPropertyType.Integer: return _prop.intValue;
			case SerializedPropertyType.LayerMask: return _prop.intValue;
			case SerializedPropertyType.ObjectReference: return _prop.objectReferenceValue;
			case SerializedPropertyType.Quaternion: return _prop.quaternionValue;
			case SerializedPropertyType.Rect: return _prop.rectValue;
			case SerializedPropertyType.String: return _prop.stringValue;
			case SerializedPropertyType.Vector2: return _prop.vector2Value;
			case SerializedPropertyType.Vector3: return _prop.vector3Value;
			case SerializedPropertyType.Vector4: return _prop.vector4Value;
			case SerializedPropertyType.ArraySize: return _prop.arraySize;
		}
		return null;
	}

	/// <summary>
	/// Set the value of the property based on its type.
	/// </summary>
	/// <param name="_prop">Property to be modified.</param>
	/// <param name="_newValue">The value to be set.</param>
	public static void SetValue(this SerializedProperty _prop, object _newValue) {
		// Set the value from the right field based on type
		switch(_prop.propertyType) {
			case SerializedPropertyType.AnimationCurve: _prop.animationCurveValue = (AnimationCurve)_newValue; break;
			case SerializedPropertyType.Boolean: _prop.boolValue = (bool)_newValue; break;
			case SerializedPropertyType.Bounds: _prop.boundsValue = (Bounds)_newValue; break;
			case SerializedPropertyType.Color: _prop.colorValue = (Color)_newValue; break;
			case SerializedPropertyType.Enum: _prop.enumValueIndex = (int)_newValue; break;
			case SerializedPropertyType.Float: _prop.floatValue = (float)_newValue; break;
			case SerializedPropertyType.Integer: _prop.intValue = (int)_newValue; break;
			case SerializedPropertyType.LayerMask: _prop.intValue = (int)_newValue; break;
			case SerializedPropertyType.ObjectReference: _prop.objectReferenceValue = (UnityEngine.Object)_newValue; break;
			case SerializedPropertyType.Quaternion: _prop.quaternionValue = (Quaternion)_newValue; break;
			case SerializedPropertyType.Rect: _prop.rectValue = (Rect)_newValue; break;
			case SerializedPropertyType.String: _prop.stringValue = (string)_newValue; break;
			case SerializedPropertyType.Vector2: _prop.vector2Value = (Vector2)_newValue; break;
			case SerializedPropertyType.Vector3: _prop.vector3Value = (Vector3)_newValue; break;
			case SerializedPropertyType.Vector4: _prop.vector4Value = (Vector4)_newValue; break;
			case SerializedPropertyType.ArraySize: _prop.arraySize = (int)_newValue; break;
		}
	}

	/// <summary>
	/// Print a Serialized Property hierarchy in the screen.
	/// </summary>
	/// <param name="_p">The property to be printed.</param>
	/// <param name="_depth">Current recursive depth.</param>
	public static void PrintSerializedProperty(SerializedProperty _p, int _depth = 0) {
		// Control infinite recursivity
		if(_depth > 10) return;

		// Create prefix
		string prefix = "";
		for(int i = 0; i < _depth; ++i) prefix += "\t";

		// Recursively iterate through all the relative properties in the given one
		SerializedProperty p = _p.Copy();   // Avoid modifying given param
		p.Next(true);   // To get first element
		do {
			// Print!
			Debug.Log(Color.cyan.Tag(prefix + p.name + " (" + p.propertyType + ")"));

			// If generic type, do recursive call
			if(p.propertyType == SerializedPropertyType.Generic) {
				PrintSerializedProperty(p, _depth + 1);
			}
		} while(p.NextVisible(false));      // Only direct children, not grand-children
	}
}