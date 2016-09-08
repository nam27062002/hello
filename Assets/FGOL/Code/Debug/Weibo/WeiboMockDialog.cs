
using System;
using System.Collections.Generic;
using UnityEngine;


namespace Weibo.Unity
{
#if UNITY_EDITOR
    public class WeiboMockDialog : MonoBehaviour
	{
		private Rect modalRect;
		private GUIStyle modalStyle = null;
		private string accessToken = string.Empty;
		private string m_tokenURL = null;
		private Action<bool, string> GetAccessTokenFunction = null;

		public WeiboMockDialog()
		{
			this.modalRect = new Rect(10, 10, Screen.width - 20, Screen.height - 20);
			//STUPIDLY LAZY CODE, ONLY IN EDITOR THOUGH, SO MAYBE WE DON'T KILL PETE JUST YET
        }

		public void SetURL(string tokenURL)
		{ 
			m_tokenURL = tokenURL;
		}

		public void SetAccessTokenFunction(Action<bool, string> method)
		{
			GetAccessTokenFunction = method;
		}

		public void OnGUI()
		{
			if (this.modalStyle == null)
			{
				this.modalStyle = new GUIStyle(GUI.skin.window);

				Texture2D texture = new Texture2D(1, 1);
				texture.SetPixel(0, 0, new Color(0.2f, 0.2f, 0.2f, 1.0f));
				texture.Apply();
				this.modalStyle.normal.background = texture;
			}

			GUI.ModalWindow(
				this.GetHashCode(),
				this.modalRect,
				this.OnGUIDialog,
				"WeiboToken",
				this.modalStyle);
		}

		private void SendSuccessResult()
		{
			if (string.IsNullOrEmpty(this.accessToken))
			{
				this.SendErrorResult("Empty Access token string");
				return;
			}
			GetAccessTokenFunction(true, this.accessToken);
		}

		private void SendCancelResult()
		{
			GetAccessTokenFunction(false, "Cancelled");
        }

		private void SendErrorResult(string errorMessage)
		{
			GetAccessTokenFunction(false, errorMessage);
		}

		private void OnGUIDialog(int windowId)
		{

			GUILayout.Space(10);
			GUILayout.BeginVertical();
			GUILayout.Label("Warning! Mock dialog responses will NOT match production dialogs");
			GUILayout.Label("Test your app on one of the supported platforms");

			GUILayout.BeginHorizontal();
			GUILayout.Label("User Access Token (Copy from URL):");
			this.accessToken = GUILayout.TextField(this.accessToken, GUI.skin.textArea, GUILayout.MinWidth(400));
			GUILayout.EndHorizontal();
			GUILayout.Space(10);
			if (GUILayout.Button("Find Access Token"))
			{
				Application.OpenURL(m_tokenURL);
			}

			GUILayout.Space(20);

			GUILayout.EndVertical();

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			var loginLabel = new GUIContent("Send Success");
			var buttonRect = GUILayoutUtility.GetRect(loginLabel, GUI.skin.button);
			if (GUI.Button(buttonRect, loginLabel))
			{
				this.SendSuccessResult();
				MonoBehaviour.Destroy(this);
			}

			var cancelLabel = new GUIContent("Send Cancel");
			var cancelButtonRect = GUILayoutUtility.GetRect(cancelLabel, GUI.skin.button);
			if (GUI.Button(cancelButtonRect, cancelLabel, GUI.skin.button))
			{
				this.SendCancelResult();
				MonoBehaviour.Destroy(this);
			}

			var errorLabel = new GUIContent("Send Error");
			var errorButtonRect = GUILayoutUtility.GetRect(cancelLabel, GUI.skin.button);
			if (GUI.Button(errorButtonRect, errorLabel, GUI.skin.button))
			{
				this.SendErrorResult("Error: Error button pressed");
				MonoBehaviour.Destroy(this);
			}

			GUILayout.EndHorizontal();
		}
	}
#endif
}