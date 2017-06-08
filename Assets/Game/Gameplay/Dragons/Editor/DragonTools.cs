using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text;

public class DragonTools 
{

	[MenuItem("Hungry Dragon/Dragon Tools/Replace Dragon Preafab View")]
	static void ReplaceSelectedView()
	{
		if (Selection.activeObject != null)
		{
			GameObject go = Selection.activeObject as GameObject;
			DragonPlayer player = go.GetComponent<DragonPlayer>();
			if ( player != null )
			{
				Transform view = go.transform.Find("view");
				Vector3 localPos = view.localPosition;
				Quaternion localRot = view.localRotation;
				Vector3 localScale = view.localScale;

				StringBuilder modelName = new StringBuilder( player.sku + "_LOW.FBX" );
				modelName[0] = char.ToUpper( modelName[0] );	// D
				modelName[7] = char.ToUpper( modelName[7] );	//

				Object model = AssetDatabase.LoadAssetAtPath("Assets/Art/3D/Gameplay/Dragons/" + player.sku + "/Model/" + modelName, typeof(GameObject));
				GameObject clone = Object.Instantiate(model) as GameObject;
				// Modify the clone to your heart's content
				clone.transform.parent = go.transform;

				clone.name = "view";
				clone.transform.localPosition = localPos;
				clone.transform.localRotation = localRot;
				clone.transform.localScale = localScale;
				clone.SetLayerRecursively("Player");

				// Copy components to new view

				UnityEditorInternal.ComponentUtility.CopyComponent( view.GetComponent<DragonAnimationEvents>() );
				UnityEditorInternal.ComponentUtility.PasteComponentAsNew( clone );

				// UnityEditorInternal.ComponentUtility.CopyComponent( view.GetComponent< Animator>() );
				// UnityEditorInternal.ComponentUtility.PasteComponentValues( clone.GetComponent<Animator>() );
				// 
				/*
				DragonEquip equip = player.GetComponent<DragonEquip>();
				equip.Init();
				equip.SetSkin(player.sku + "_0");
				*/

				Object.DestroyImmediate(view.gameObject);
				AttachPoint[] attachs = player.GetComponentsInChildren<AttachPoint>(true);
				for( int i = 0; i<attachs.Length; ++i )
				{
					AutoParenter auto = attachs[i].GetComponent<AutoParenter>();
					if ( auto )
					{
						auto.CopyTargetPosAndRot();
					}
				}

			}
		}
	}


	[MenuItem("Hungry Dragon/Dragon Tools/Replace Dragon Menu Prefab View")]
	static void ReplaceSelectedMenuView()
	{
		if (Selection.activeObject != null)
		{
			GameObject go = Selection.activeObject as GameObject;
			MenuDragonPreview player = go.GetComponent<MenuDragonPreview>();
			if ( player != null )
			{
				Transform view = go.transform.Find("view");
				Vector3 localPos = view.localPosition;
				Quaternion localRot = view.localRotation;
				Vector3 localScale = view.localScale;

				StringBuilder modelName = new StringBuilder( player.sku + "_HI.FBX" );
				modelName[0] = char.ToUpper( modelName[0] );	// D
				modelName[7] = char.ToUpper( modelName[7] );	//

				Object model = AssetDatabase.LoadAssetAtPath("Assets/Art/3D/Gameplay/Dragons/" + player.sku + "/Model/" + modelName, typeof(GameObject));
				GameObject clone = Object.Instantiate(model) as GameObject;
				// Modify the clone to your heart's content
				clone.transform.parent = go.transform;

				clone.name = "view";
				clone.transform.localPosition = localPos;
				clone.transform.localRotation = localRot;
				clone.transform.localScale = localScale;
				clone.SetLayerRecursively("Player");

				// Copy components to new view

				UnityEditorInternal.ComponentUtility.CopyComponent( view.GetComponent<DragonAnimationEventsMenu>() );
				UnityEditorInternal.ComponentUtility.PasteComponentAsNew( clone );

				Object.DestroyImmediate(view.gameObject);
				AttachPoint[] attachs = player.GetComponentsInChildren<AttachPoint>(true);
				for( int i = 0; i<attachs.Length; ++i )
				{
					AutoParenter auto = attachs[i].GetComponent<AutoParenter>();
					if ( auto )
					{
						auto.CopyTargetPosAndRot();
					}
				}

			}
		}
	}


}
