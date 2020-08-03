// Copyright (C) 2014 - 2016 Stephan Bouchard - All Rights Reserved
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms


using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Linq;

using TMPro;
using TMPro.EditorUtilities;

namespace BatchFontCreator
{
	public class UbiBCN_BatchFontAssetCreatorWindow : EditorWindow
    {
		[System.Serializable]
		private class FontObjectAndSettings {
			public Font fontTTF;
			public FontCreationSetting settings;
			public List<TextAsset> inputCharactersFiles;

			public Vector2 output_ScrollPosition;
			public string output_feedback;
			public int character_Count;
		}

        [MenuItem("Hungry Dragon/Fonts/Batch Font Asset Creator")]
        public static void ShowFontAtlasCreatorWindow()
        {
			var window = GetWindow<UbiBCN_BatchFontAssetCreatorWindow>();
            window.titleContent = new GUIContent("Batch Font Asset Creator");

            window.Focus();
        }

		private const string tool_folder_path = "/Tools/UITools/FontAssetCreator/";

        private string[] FontResolutionLabels = { "16","32", "64", "128", "256", "512", "1024", "2048", "4096", "8192" };
        private int[] FontAtlasResolutions = { 16, 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192 };
          
        
        private const string output_name_label = "Font: ";
		private const string output_size_label = "Pt. Size: ";
		private const string output_count_label = "Characters packed: ";        
        
        
        //private Thread MainThread;              
        private bool isRepaintNeeded = false;

        private Rect progressRect;
        public static float ProgressPercentage;
        private float m_renderingProgress;
        private bool isRenderingDone = false;
        private bool isProcessing = false;

		private List<FontObjectAndSettings> m_toDeleteSettings = new List<FontObjectAndSettings>();
		private List<FontObjectAndSettings> m_fontSettings = new List<FontObjectAndSettings>();

		private int m_currentRenderingSettingIndex = -1;
        
        private TMP_FontAsset m_fontAssetSelection;
        private TextAsset characterList;
		                
        private int font_scaledownFactor = 1;
        

		private FT_FaceInfo m_font_faceInfo;
		private FT_GlyphInfo[] m_font_glyphInfo;
        private byte[] m_texture_buffer;
        private Texture2D m_font_Atlas;
        
		        
        private bool includeKerningPairs = false;
        private int[] m_kerningSet;


        private EditorWindow m_editorWindow;
        private Vector2 m_previewWindow_Size = new Vector2(768, 768);
        private Rect m_UI_Panel_Size;


		private Queue<int> m_toGenerateQueue = new Queue<int>();
		private int m_toGenerateCount = 0;


        public void OnEnable() {			
            m_editorWindow = this;
            UpdateEditorWindowSize(768, 768);

            // Get the UI Skin and Styles for the various Editors
            TMP_UIStyleManager.GetUIStyles();

            // Initialize & Get shader property IDs.
            ShaderUtilities.GetShaderPropertyIDs();

			ReadFontSettings();
        }


        public void OnDisable() {
            // Destroy Engine only if it has been initialized already
            if (TMPro_FontPlugin.Initialize_FontEngine() == 99) {
                TMPro_FontPlugin.Destroy_FontEngine();
            }

			if (m_font_Atlas != null && EditorUtility.IsPersistent(m_font_Atlas) == false) {
                //Debug.Log("Destroying font_Atlas!");
                DestroyImmediate(m_font_Atlas);
            }

            Resources.UnloadUnusedAssets();
        }


        public void OnGUI() {
			GUILayout.BeginHorizontal(GUILayout.Width(310));
            
			DrawControls();

			if (Event.current.type == EventType.Repaint) {
				Rect r = GUILayoutUtility.GetLastRect();
				Vector2 windowSize = new Vector2();
				windowSize.x = r.xMax;
				windowSize.y = r.yMax;
				this.minSize = windowSize;
				this.maxSize = windowSize;
			}
            GUILayout.EndHorizontal();
        }


        public void Update() {            
            if (isRepaintNeeded) {
                isRepaintNeeded = false;
                Repaint();
            }

			// Update Progress bar is we are Rendering a Font.
			if (isProcessing) {
				m_renderingProgress = TMPro_FontPlugin.Check_RenderProgress();
				isRepaintNeeded = true;
			}

			if (m_toGenerateCount > 0) {
				if (m_currentRenderingSettingIndex < 0) {
					if (m_toGenerateQueue.Count == 0) {
						m_toGenerateCount = 0;
						m_currentRenderingSettingIndex = -1;

						Resources.UnloadUnusedAssets();
					} else {
						m_currentRenderingSettingIndex = m_toGenerateQueue.Dequeue();
						GenerateFontAtlas(m_fontSettings[m_currentRenderingSettingIndex]);
					}
				} else {
		            // Update Feedback Window & Create Font Texture once Rendering is done.
		            if (isRenderingDone) {
		                isProcessing = false;
		                isRenderingDone = false;
						UpdateRenderFeedbackWindow(m_fontSettings[m_currentRenderingSettingIndex]);
						CreateFontTexture(m_fontSettings[m_currentRenderingSettingIndex].settings);

						SaveFontAtlas(m_fontSettings[m_currentRenderingSettingIndex]);

						m_renderingProgress = 0f;
						m_currentRenderingSettingIndex = -1;
		            }
				}
			}
        }
			

		private FontObjectAndSettings CreateDefaultSettings() {
			FontCreationSetting settings = new FontCreationSetting();
			settings.fontSourcePath = "";
			settings.fontSizingMode = 1;
			settings.fontSize = 24;
			settings.fontPadding = 3;
			settings.fontPackingMode = 4; //optimum
			settings.fontAtlasWidth = 512;
			settings.fontAtlasHeight = 512;
			settings.fontCharacterSet = 8;
			settings.fontStyle = (int)FaceStyles.Normal;
			settings.fontStlyeModifier = 2;
			settings.fontRenderMode = (int)RenderModes.DistanceField32;
			settings.fontKerning = false;

			FontObjectAndSettings container = new FontObjectAndSettings();
			container.settings = settings;
			container.fontTTF = null;
			container.inputCharactersFiles = new List<TextAsset>();

			return container;
		}

		private const int PANEL_WIDTH = 150;
		private const int PANEL_CONTENT_WIDTH = 140;

		private const int LABEL_WIDTH = 60;
		private const int FIELD_WIDTH = 80;

        void DrawControls() {
			GUILayout.BeginVertical();
			GUILayout.Label("<b>Font Asset Creator</b>", TMP_UIStyleManager.Section_Label, GUILayout.Width(300));

			GUI.enabled = (m_toGenerateCount == 0)? true : false;

			GUILayout.BeginHorizontal();
			for (int s = 0; s < m_fontSettings.Count; ++s) {
				FontObjectAndSettings container = m_fontSettings[s];
				FontCreationSetting settings = container.settings;

	            GUILayout.BeginVertical();
	            
				if (container.fontTTF != null) {
					GUILayout.Label(container.fontTTF.name, TMP_UIStyleManager.Section_Label, GUILayout.Width(PANEL_WIDTH));
				} else {
					GUILayout.Label("empty", TMP_UIStyleManager.Section_Label, GUILayout.Width(PANEL_WIDTH));
				}

				GUILayout.BeginVertical(TMP_UIStyleManager.TextureAreaBox, GUILayout.Width(PANEL_WIDTH));

				EditorGUIUtility.labelWidth = LABEL_WIDTH;
				EditorGUIUtility.fieldWidth = FIELD_WIDTH;

	            // FONT TTF SELECTION
	            EditorGUI.BeginChangeCheck();
				container.fontTTF = EditorGUILayout.ObjectField("Source", container.fontTTF, typeof(Font), false, GUILayout.Width(PANEL_CONTENT_WIDTH)) as Font;
	            if (EditorGUI.EndChangeCheck()) {
					settings.fontSourcePath = AssetDatabase.GetAssetPath(container.fontTTF);
	            }								                
				settings.fontSize = EditorGUILayout.IntField("Size", settings.fontSize, GUILayout.Width(PANEL_CONTENT_WIDTH));
                
	            
	            // FONT PADDING
				settings.fontPadding = EditorGUILayout.IntField("Padding", settings.fontPadding, GUILayout.Width(PANEL_CONTENT_WIDTH));
				settings.fontPadding = (int)Mathf.Clamp((float)settings.fontPadding, 0f, 64f);

	            
	            // FONT ATLAS RESOLUTION SELECTION
				GUILayout.BeginHorizontal(GUILayout.Width(PANEL_CONTENT_WIDTH));
	            GUI.changed = false;

				EditorGUIUtility.labelWidth = LABEL_WIDTH;
	            EditorGUIUtility.fieldWidth = 30f;

				GUILayout.Label("Atlas");
				settings.fontAtlasWidth = EditorGUILayout.IntPopup(settings.fontAtlasWidth, FontResolutionLabels, FontAtlasResolutions); //, GUILayout.Width(80));
				settings.fontAtlasHeight = EditorGUILayout.IntPopup(settings.fontAtlasHeight, FontResolutionLabels, FontAtlasResolutions); //, GUILayout.Width(80));

	            GUILayout.EndHorizontal();


	            // FONT CHARACTER SET SELECTION
	            // Character List from File
				// [AOC] Support multiple input files
				EditorGUILayout.BeginVertical(TMP_UIStyleManager.TextureAreaBox); {
					GUILayout.Label("Input Files:", TMP_UIStyleManager.Label);
					GUILayout.Space(10f);

					for (int i = 0; i < container.inputCharactersFiles.Count; ++i) {
						container.inputCharactersFiles[i] = EditorGUILayout.ObjectField(container.inputCharactersFiles[i], typeof(TextAsset), false, GUILayout.Width(PANEL_CONTENT_WIDTH-10)) as TextAsset;
					}

					GUILayout.BeginHorizontal();
					if (GUILayout.Button("Add", GUILayout.Width(50))) {
						container.inputCharactersFiles.Add(null);
					}
					if (container.inputCharactersFiles.Count > 0) {
						if (GUILayout.Button("Delete", GUILayout.Width(50))) {
							container.inputCharactersFiles.RemoveAt(container.inputCharactersFiles.Count - 1);
						}
					}
					GUILayout.EndHorizontal();
	            }
				EditorGUILayout.EndVertical();
					                        

				EditorGUIUtility.labelWidth = LABEL_WIDTH;
				EditorGUIUtility.fieldWidth = FIELD_WIDTH;

	            GUILayout.Space(20);

				if (GUILayout.Button("Generate Font Atlas", GUILayout.Width(PANEL_CONTENT_WIDTH)) && GUI.enabled) {
					m_toGenerateQueue.Enqueue(s);
					m_toGenerateCount = 1;
	            }

	            // FONT RENDERING PROGRESS BAR
				float renderProgress = 0f;
				if (m_currentRenderingSettingIndex == s) {
					renderProgress = m_renderingProgress;
				}

				GUILayout.Space(1);
				progressRect = GUILayoutUtility.GetRect(PANEL_CONTENT_WIDTH, 20, TMP_UIStyleManager.TextAreaBoxWindow, GUILayout.Width(PANEL_CONTENT_WIDTH), GUILayout.Height(20));

	            GUI.BeginGroup(progressRect);
				GUI.DrawTextureWithTexCoords(new Rect(2, 0, PANEL_CONTENT_WIDTH, 20), TMP_UIStyleManager.progressTexture, new Rect(1 - renderProgress, 0, 1, 1));
	            GUI.EndGroup();

	            // FONT STATUS & INFORMATION
	            GUISkin skin = GUI.skin;
	            GUI.skin = TMP_UIStyleManager.TMP_GUISkin;

	            GUILayout.Space(5);
	            GUILayout.BeginVertical(TMP_UIStyleManager.TextAreaBoxWindow);
				container.output_ScrollPosition = EditorGUILayout.BeginScrollView(container.output_ScrollPosition, GUILayout.Height(145));
				EditorGUILayout.LabelField(container.output_feedback, TMP_UIStyleManager.Label);
	            EditorGUILayout.EndScrollView();
	            GUILayout.EndVertical();

	            GUI.skin = skin;

	            GUILayout.Space(10);

	            // SAVE TEXTURE & CREATE and SAVE FONT XML FILE
				if (GUILayout.Button("Delete Font", GUILayout.Width(PANEL_CONTENT_WIDTH)) && GUI.enabled) {
					m_toDeleteSettings.Add(container);
	            }

	            GUILayout.Space(5);
	            GUILayout.EndVertical();
	            GUILayout.Space(25);
	             
	            // Figure out the size of the current UI Panel
	            Rect rect = EditorGUILayout.GetControlRect(false, 5);
	            if (Event.current.type == EventType.Repaint)
	                m_UI_Panel_Size = rect;

	            GUILayout.EndVertical();

				//Update reference
				container.settings = settings;
				m_fontSettings[s] = container;
			}

			for (int i = 0; i < m_toDeleteSettings.Count; ++i) {
				m_fontSettings.Remove(m_toDeleteSettings[i]);
			}

			GUILayout.EndHorizontal();

			if (GUILayout.Button("Add Font")) {
				m_fontSettings.Add(CreateDefaultSettings());
			}

			if (GUILayout.Button("Generate All Fonts")) {
				BatchGenerate();
			}

			if (GUILayout.Button("Save Settings")) {
				SaveFontSettings();
			}

			if (GUILayout.Button("Load Settings")) {
				ReadFontSettings();
			}

			GUILayout.EndVertical();
        }

		void SaveFontSettings() {
			List<FontCreationSetting> settings = new List<FontCreationSetting>();
			List<List<string>> files = new List<List<string>>();
			for (int i = 0; i < m_fontSettings.Count; ++i) {
				FontObjectAndSettings container = m_fontSettings[i];
				settings.Add(container.settings);

				List<string> fileNames = new List<string>();
				for (int f = 0; f < container.inputCharactersFiles.Count; ++f) {
					fileNames.Add(AssetDatabase.GetAssetPath(container.inputCharactersFiles[f]));
				}
				files.Add(fileNames);
			}

			string path = Application.dataPath + tool_folder_path + "data01.dat";
			using (Stream file = File.Open(path, FileMode.Create)) {
				BinaryFormatter bf = new BinaryFormatter();
				bf.Serialize(file, settings);
			}

			path = Application.dataPath + tool_folder_path + "data02.dat";
			using (Stream file = File.Open(path, FileMode.Create)) {
				BinaryFormatter bf = new BinaryFormatter();
				bf.Serialize(file, files);
			}
		}

		void ReadFontSettings() {
			List<FontCreationSetting> settings = null;
			string path = Application.dataPath + tool_folder_path + "data01.dat";
			using (Stream stream = File.Open(path, FileMode.Open)) {
				var bf = new BinaryFormatter();
				object o = bf.Deserialize(stream);
				settings = (List<FontCreationSetting>)o;
			}

			List<List<string>> files = null;
			path = Application.dataPath + tool_folder_path + "data02.dat";
			using (Stream stream = File.Open(path, FileMode.Open)) {
				var bf = new BinaryFormatter();
				object o = bf.Deserialize(stream);
				files = (List<List<string>>)o;
			}

			if (settings != null) {
				m_fontSettings = new List<FontObjectAndSettings>();
				for (int i = 0; i < settings.Count; ++i) {
					FontObjectAndSettings container = new FontObjectAndSettings();
					container.settings = settings[i];
					container.fontTTF = AssetDatabase.LoadAssetAtPath(container.settings.fontSourcePath, typeof(Font)) as Font;
					container.inputCharactersFiles = new List<TextAsset>();

					container.inputCharactersFiles = new List<TextAsset>();
					for (int f = 0; f < files[i].Count; ++f) {
						container.inputCharactersFiles.Add(AssetDatabase.LoadAssetAtPath(files[i][f], typeof(TextAsset)) as TextAsset);
					}

					m_fontSettings.Add(container);
				}
			}
		}

		void BatchGenerate() {
			m_toGenerateCount = m_fontSettings.Count;

			for (int i = 0; i < m_toGenerateCount; ++i) {
				m_toGenerateQueue.Enqueue(i);
			}
		}

		void GenerateFontAtlas(FontObjectAndSettings _container) {
			if (_container.fontTTF != null) {
				string characterSequence = string.Empty;
				for (int i = 0; i < _container.inputCharactersFiles.Count; ++i) {
					if (_container.inputCharactersFiles[i] != null) {
						characterSequence += _container.inputCharactersFiles[i].text;

						// Add the capitals as well!
						string isoCode = "";
						switch (_container.inputCharactersFiles[i].name) {
							case "english": 			isoCode = "en-US"; break;
							case "french": 				isoCode = "fr-FR"; break;
							case "italian": 			isoCode = "it-IT"; break;
							case "german": 				isoCode = "de-DE"; break;
							case "spanish": 			isoCode = "es-ES"; break;
							case "brazilian": 			isoCode = "pt-BR"; break;
							case "russian": 			isoCode = "ru-RU"; break;
							case "simplified_chinese": 	isoCode = "zh-CN"; break;
							case "japanese": 			isoCode = "ja-JP"; break;
							case "korean": 				isoCode = "ko-KR"; break;
							case "traditional_chinese": isoCode = "zh-TW"; break;
							case "turkish": 			isoCode = "tr-TR"; break;
						}

						if (!string.IsNullOrEmpty(isoCode)) {
							characterSequence += _container.inputCharactersFiles[i].text.ToUpper(CultureInfo.CreateSpecificCulture(isoCode));
						}
					}
				}

				int error_Code;

				error_Code = TMPro_FontPlugin.Initialize_FontEngine(); //Initialize Font Engine
				if (error_Code != 0) {
					if (error_Code == 99)
						error_Code = 0;	//Debug.Log("Font Library was already initialized!");
					else
						Debug.Log("Error Code: " + error_Code + "  occurred while Initializing the FreeType Library.");
				}

				string fontPath = AssetDatabase.GetAssetPath(_container.fontTTF); // Get file path of TTF Font.

				if (error_Code == 0) {
					error_Code = TMPro_FontPlugin.Load_TrueType_Font(fontPath); // Load the selected font.

					if (error_Code != 0) {
						if (error_Code == 99) { //Debug.Log("Font was already loaded!");
							error_Code = 0;
						} else
							Debug.Log("Error Code: " + error_Code + "  occurred while Loading the font.");
					}
				}

				if (error_Code == 0) {
					error_Code = TMPro_FontPlugin.FT_Size_Font(_container.settings.fontSize); // Load the selected font and size it accordingly.
					if (error_Code != 0)
						Debug.Log("Error Code: " + error_Code + "  occurred while Sizing the font.");
				}

				// Define an array containing the characters we will render.
				if (error_Code == 0) {
					int[] character_Set = null;
					List<int> char_List = new List<int>();

					for (int i = 0; i < characterSequence.Length; i++) {
						// Check to make sure we don't include duplicates
						if (char_List.FindIndex(item => item == characterSequence[i]) == -1)
							char_List.Add(characterSequence[i]);                            
					}

					character_Set = char_List.ToArray();

					_container.character_Count = character_Set.Length;

					m_texture_buffer = new byte[_container.settings.fontAtlasWidth * _container.settings.fontAtlasHeight];
					m_font_faceInfo = new FT_FaceInfo();
					m_font_glyphInfo = new FT_GlyphInfo[_container.character_Count];


					bool autoSizing = false;
					float strokeSize = _container.settings.fontStlyeModifier * 32;

					isProcessing = true;

					ThreadPool.QueueUserWorkItem(SomeTask => {
						isRenderingDone = false;
						error_Code = TMPro_FontPlugin.Render_Characters(
																		m_texture_buffer,
																		_container.settings.fontAtlasWidth,
																		_container.settings.fontAtlasHeight,
																		_container.settings.fontPadding,
																		character_Set,
																		_container.character_Count,
																		(FaceStyles)_container.settings.fontStyle,
																		strokeSize,
																		autoSizing,
																		(RenderModes)_container.settings.fontRenderMode,
																		4,
																		ref m_font_faceInfo,
																		m_font_glyphInfo);
						isRenderingDone = true;
					});
				}
			}
		}

		void SaveFontAtlas(FontObjectAndSettings _container) {
			string filePath = Application.dataPath + "/Resources/UI/Fonts/" + _container.fontTTF.name + "/" + _container.fontTTF.name + ".asset";

			Save_SDF_FontAsset(filePath, _container);
		}


		void UpdateRenderFeedbackWindow(FontObjectAndSettings _container) {
			_container.settings.fontSize = m_font_faceInfo.pointSize;

            string missingGlyphReport = string.Empty;
			string addedGlyphReport = string.Empty;

			string colorTag = m_font_faceInfo.characterCount == _container.character_Count ? "<color=#C0ffff>" : "<color=#ffff00>";
            string colorTag2 = "<color=#C0ffff>";

            missingGlyphReport = output_name_label + "<b>" + colorTag2 + m_font_faceInfo.name + "</color></b>";

            if (missingGlyphReport.Length > 60)
                missingGlyphReport += "\n" + output_size_label + "<b>" + colorTag2 + m_font_faceInfo.pointSize + "</color></b>";
            else
                missingGlyphReport += "  " + output_size_label + "<b>" + colorTag2 + m_font_faceInfo.pointSize + "</color></b>";

			missingGlyphReport += "\n" + output_count_label + "<b>" + colorTag + m_font_faceInfo.characterCount + "/" + _container.character_Count + "</color></b>";

            // Report added/missing requested glyph
            missingGlyphReport += "\n\n<color=#ffff00><b>Missing Characters</b></color>";
            missingGlyphReport += "\n----------------------------------------";

			addedGlyphReport += "\n\n<color=#00ff00><b>Added Characters</b></color>";
			addedGlyphReport += "\n----------------------------------------";

			_container.output_feedback = missingGlyphReport;

			for (int i = 0; i < _container.character_Count; i++) {
                if (m_font_glyphInfo[i].x == -1) {
                    missingGlyphReport += "\nID: <color=#C0ffff>" + m_font_glyphInfo[i].id + "\t</color>Hex: <color=#C0ffff>" + m_font_glyphInfo[i].id.ToString("X") + "\t</color>Char [<color=#C0ffff>" + (char)m_font_glyphInfo[i].id + "</color>]";

                    if (missingGlyphReport.Length < 16300)
						_container.output_feedback = missingGlyphReport;
				} else {
					// [AOC] Add character to the glyph report
					//addedGlyphReport += '\n' + (char)m_font_glyphInfo[i].id;
					addedGlyphReport += "\n" + (char)m_font_glyphInfo[i].id;
				}
            }

			string glyphReportPath = tool_folder_path + "GlyphReport" + _container.fontTTF.name + ".txt";

            if (missingGlyphReport.Length > 16300)
				_container.output_feedback += "\n\n<color=#ffff00>Report truncated.</color>\n<color=#c0ffff>See</color> \"" + glyphReportPath + "\"";

            // Save Missing Glyph Report file
			string path = Application.dataPath + glyphReportPath;
			missingGlyphReport = System.Text.RegularExpressions.Regex.Replace(missingGlyphReport, @"<[^>]*>", string.Empty);
			File.WriteAllText(path, missingGlyphReport);
			File.AppendAllText(path, addedGlyphReport);
            AssetDatabase.Refresh();
        }


		void CreateFontTexture(FontCreationSetting _settings) {
			m_font_Atlas = new Texture2D(_settings.fontAtlasWidth, _settings.fontAtlasHeight, TextureFormat.Alpha8, false, true);

			Color32[] colors = new Color32[_settings.fontAtlasWidth * _settings.fontAtlasHeight];

			for (int i = 0; i < (_settings.fontAtlasWidth * _settings.fontAtlasHeight); i++) {
                byte c = m_texture_buffer[i];
                colors[i] = new Color32(c, c, c, c);
            }

            m_font_Atlas.SetPixels32(colors, 0);
            m_font_Atlas.Apply(false, true);
			            
            UpdateEditorWindowSize(m_font_Atlas.width, m_font_Atlas.height);            
        }




		void Save_SDF_FontAsset(string filePath, FontObjectAndSettings _container)
        {
            filePath = filePath.Substring(0, filePath.Length - 6); // Trim file extension from filePath.

            string dataPath = Application.dataPath;

            if (filePath.IndexOf(dataPath, System.StringComparison.InvariantCultureIgnoreCase) == -1) {
                Debug.LogError("You're saving the font asset in a directory outside of this project folder. This is not supported. Please select a directory under \"" + dataPath + "\"");
                return;
            }

            string relativeAssetPath = filePath.Substring(dataPath.Length - 6);
            string tex_DirName = Path.GetDirectoryName(relativeAssetPath);
            string tex_FileName = Path.GetFileNameWithoutExtension(relativeAssetPath);
            string tex_Path_NoExt = tex_DirName + "/" + tex_FileName;



            // Check if TextMeshPro font asset already exists. If not, create a new one. Otherwise update the existing one.
            TMP_FontAsset font_asset = AssetDatabase.LoadAssetAtPath(tex_Path_NoExt + ".asset", typeof(TMP_FontAsset)) as TMP_FontAsset;
            if (font_asset == null) {
                //Debug.Log("Creating TextMeshPro font asset!");
                font_asset = ScriptableObject.CreateInstance<TMP_FontAsset>(); // Create new TextMeshPro Font Asset.
                AssetDatabase.CreateAsset(font_asset, tex_Path_NoExt + ".asset");

                // Reference to the source font file
                //font_asset.sourceFontFile = font_TTF as Font;

                //Set Font Asset Type
                font_asset.fontAssetType = TMP_FontAsset.FontAssetTypes.SDF;
				                
                // If using the C# SDF creation mode, we need the scale down factor.
                int scaleDownFactor = 1;

                // Add FaceInfo to Font Asset
                FaceInfo face = GetFaceInfo(m_font_faceInfo, scaleDownFactor);
                font_asset.AddFaceInfo(face);

                // Add GlyphInfo[] to Font Asset
                TMP_Glyph[] glyphs = GetGlyphInfo(m_font_glyphInfo, scaleDownFactor);
                font_asset.AddGlyphInfo(glyphs);

                // Get and Add Kerning Pairs to Font Asset
                if (includeKerningPairs)
                {
					string fontFilePath = AssetDatabase.GetAssetPath(_container.fontTTF);
                    KerningTable kerningTable = GetKerningTable(fontFilePath, (int)face.PointSize);
                    font_asset.AddKerningInfo(kerningTable);
                }

                // Add Line Breaking Rules
                //LineBreakingTable lineBreakingTable = new LineBreakingTable();
                //

                // Add Font Atlas as Sub-Asset
                font_asset.atlas = m_font_Atlas;
                m_font_Atlas.name = tex_FileName + " Atlas";

                // Special handling due to a bug in earlier versions of Unity.
                #if UNITY_5_3_OR_NEWER
                    // Nothing
                #else
                    m_font_Atlas.hideFlags = HideFlags.HideInHierarchy;
                #endif

                AssetDatabase.AddObjectToAsset(m_font_Atlas, font_asset);

                // Create new Material and Add it as Sub-Asset
                Shader default_Shader = Shader.Find("TextMeshPro/Distance Field"); //m_shaderSelection;
                Material tmp_material = new Material(default_Shader);

                tmp_material.name = tex_FileName + " Material";
                tmp_material.SetTexture(ShaderUtilities.ID_MainTex, m_font_Atlas);
                tmp_material.SetFloat(ShaderUtilities.ID_TextureWidth, m_font_Atlas.width);
                tmp_material.SetFloat(ShaderUtilities.ID_TextureHeight, m_font_Atlas.height);

				int spread = _container.settings.fontPadding + 1;
                tmp_material.SetFloat(ShaderUtilities.ID_GradientScale, spread); // Spread = Padding for Brute Force SDF.

                tmp_material.SetFloat(ShaderUtilities.ID_WeightNormal, font_asset.normalStyle);
                tmp_material.SetFloat(ShaderUtilities.ID_WeightBold, font_asset.boldStyle);

                font_asset.material = tmp_material;

                // Special handling due to a bug in earlier versions of Unity.
                #if UNITY_5_3_OR_NEWER
                    // Nothing
                #else
                    tmp_material.hideFlags = HideFlags.HideInHierarchy;
                #endif

                AssetDatabase.AddObjectToAsset(tmp_material, font_asset);
            } else {
				FaceInfo face = font_asset.fontInfo.Clone();

                // Find all Materials referencing this font atlas.
                Material[] material_references = TMP_EditorUtility.FindMaterialReferences(font_asset);

                // Destroy Assets that will be replaced.
                DestroyImmediate(font_asset.atlas, true);

                //Set Font Asset Type
                font_asset.fontAssetType = TMP_FontAsset.FontAssetTypes.SDF;

                int scaleDownFactor = 1;
                // Add FaceInfo to Font Asset  
                //FaceInfo face = GetFaceInfo(m_font_faceInfo, scaleDownFactor);
				//font_asset.AddFaceInfo(face);

				//Restore original face info
				font_asset.AddFaceInfo(face);


                // Add GlyphInfo[] to Font Asset
                TMP_Glyph[] glyphs = GetGlyphInfo(m_font_glyphInfo, scaleDownFactor);
                font_asset.AddGlyphInfo(glyphs);

                // Get and Add Kerning Pairs to Font Asset
				if (includeKerningPairs) {
					string fontFilePath = AssetDatabase.GetAssetPath(_container.fontTTF);
                    KerningTable kerningTable = GetKerningTable(fontFilePath, (int)face.PointSize);
                    font_asset.AddKerningInfo(kerningTable);
                }

                // Add Font Atlas as Sub-Asset
                font_asset.atlas = m_font_Atlas;
                m_font_Atlas.name = tex_FileName + " Atlas";

                // Special handling due to a bug in earlier versions of Unity.
                #if UNITY_5_3_OR_NEWER
                    m_font_Atlas.hideFlags = HideFlags.None;
                    font_asset.material.hideFlags = HideFlags.None;
                #else
                    m_font_Atlas.hideFlags = HideFlags.HideInHierarchy;
                #endif

                AssetDatabase.AddObjectToAsset(m_font_Atlas, font_asset);

                // Assign new font atlas texture to the existing material.
                font_asset.material.SetTexture(ShaderUtilities.ID_MainTex, font_asset.atlas);

                // Update the Texture reference on the Material
                for (int i = 0; i < material_references.Length; i++) {
                    material_references[i].SetTexture(ShaderUtilities.ID_MainTex, m_font_Atlas);
                    material_references[i].SetFloat(ShaderUtilities.ID_TextureWidth, m_font_Atlas.width);
                    material_references[i].SetFloat(ShaderUtilities.ID_TextureHeight, m_font_Atlas.height);

					int spread = _container.settings.fontPadding + 1;
                    material_references[i].SetFloat(ShaderUtilities.ID_GradientScale, spread); // Spread = Padding for Brute Force SDF.

                    material_references[i].SetFloat(ShaderUtilities.ID_WeightNormal, font_asset.normalStyle);
                    material_references[i].SetFloat(ShaderUtilities.ID_WeightBold, font_asset.boldStyle);
                }
            }

			string assetPath = AssetDatabase.GetAssetPath(font_asset);

            AssetDatabase.SaveAssets();
			AssetDatabase.ImportAsset(assetPath);  // Re-import font asset to get the new updated version.

			font_asset = AssetDatabase.LoadAssetAtPath(assetPath, typeof(TMP_FontAsset)) as TMP_FontAsset;
            font_asset.ReadFontDefinition();

            AssetDatabase.Refresh();

            m_font_Atlas = null;

            // NEED TO GENERATE AN EVENT TO FORCE A REDRAW OF ANY TEXTMESHPRO INSTANCES THAT MIGHT BE USING THIS FONT ASSET
            TMPro_EventManager.ON_FONT_PROPERTY_CHANGED(true, font_asset);
        }

        void UpdateEditorWindowSize(float width, float height) {
            m_previewWindow_Size = new Vector2(768, 768);

            if (width > height) {
                m_previewWindow_Size = new Vector2(768, height / (width / 768));
            } else if (height > width) {
                m_previewWindow_Size = new Vector2(width / (height / 768), 768);
            }

            m_editorWindow.minSize = new Vector2(m_previewWindow_Size.x + 330, Mathf.Max(m_UI_Panel_Size.y + 20f, m_previewWindow_Size.y + 20f));
            m_editorWindow.maxSize = m_editorWindow.minSize + new Vector2(.25f, 0);
        }
				       
        // Convert from FT_FaceInfo to FaceInfo
        FaceInfo GetFaceInfo(FT_FaceInfo ft_face, int scaleFactor) {
            FaceInfo face = new FaceInfo();

            face.Name = ft_face.name;
            face.PointSize = (float)ft_face.pointSize / scaleFactor;
            face.Padding = ft_face.padding / scaleFactor;
            face.LineHeight = ft_face.lineHeight / scaleFactor;
            face.CapHeight = 0;
            face.Baseline = 0;
            face.Ascender = ft_face.ascender / scaleFactor;
            face.Descender = ft_face.descender / scaleFactor;
            face.CenterLine = ft_face.centerLine / scaleFactor;
            face.Underline = ft_face.underline / scaleFactor;
            face.UnderlineThickness = ft_face.underlineThickness == 0 ? 5 : ft_face.underlineThickness / scaleFactor; // Set Thickness to 5 if TTF value is Zero.
            face.strikethrough = (face.Ascender + face.Descender) / 2.75f;
            face.strikethroughThickness = face.UnderlineThickness;
            face.SuperscriptOffset = face.Ascender;
            face.SubscriptOffset = face.Underline;
            face.SubSize = 0.5f;
            //face.CharacterCount = ft_face.characterCount;
            face.AtlasWidth = ft_face.atlasWidth / scaleFactor;
            face.AtlasHeight = ft_face.atlasHeight / scaleFactor;

            return face;
        }


        // Convert from FT_GlyphInfo[] to GlyphInfo[]
        TMP_Glyph[] GetGlyphInfo(FT_GlyphInfo[] ft_glyphs, int scaleFactor)
        {
            List<TMP_Glyph> glyphs = new List<TMP_Glyph>();
            List<int> kerningSet = new List<int>();

            for (int i = 0; i < ft_glyphs.Length; i++)
            {
                TMP_Glyph g = new TMP_Glyph();

                g.id = ft_glyphs[i].id;
                g.x = ft_glyphs[i].x / scaleFactor;
                g.y = ft_glyphs[i].y / scaleFactor;
                g.width = ft_glyphs[i].width / scaleFactor;
                g.height = ft_glyphs[i].height / scaleFactor;
                g.xOffset = ft_glyphs[i].xOffset / scaleFactor;
                g.yOffset = ft_glyphs[i].yOffset / scaleFactor;
                g.xAdvance = ft_glyphs[i].xAdvance / scaleFactor;

                // Filter out characters with missing glyphs.
                if (g.x == -1)
                    continue;

                glyphs.Add(g);
                kerningSet.Add(g.id);
            }

            m_kerningSet = kerningSet.ToArray();

            return glyphs.ToArray();
        }


        // Get Kerning Pairs
        public KerningTable GetKerningTable(string fontFilePath, int pointSize)
        {
            KerningTable kerningInfo = new KerningTable();
            kerningInfo.kerningPairs = new List<KerningPair>();

            // Temporary Array to hold the kerning pairs from the Native Plug-in.
            FT_KerningPair[] kerningPairs = new FT_KerningPair[7500];

            int kpCount = TMPro_FontPlugin.FT_GetKerningPairs(fontFilePath, m_kerningSet, m_kerningSet.Length, kerningPairs);

            for (int i = 0; i < kpCount; i++)
            {
                // Proceed to add each kerning pairs.
                KerningPair kp = new KerningPair(kerningPairs[i].ascII_Left, kerningPairs[i].ascII_Right, kerningPairs[i].xAdvanceOffset * pointSize);

                // Filter kerning pairs to avoid duplicates
                int index = kerningInfo.kerningPairs.FindIndex(item => item.AscII_Left == kp.AscII_Left && item.AscII_Right == kp.AscII_Right);

                if (index == -1)
                    kerningInfo.kerningPairs.Add(kp);
                else
                    if (!TMP_Settings.warningsDisabled) Debug.LogWarning("Kerning Key for [" + kp.AscII_Left + "] and [" + kp.AscII_Right + "] is a duplicate.");

            }

            return kerningInfo;
        }


        private string[] UpdateShaderList(RenderModes mode, out Shader[] shaders)
        {
            // Get shaders for the given RenderModes.
            string searchPattern = "t:Shader" + " TMP_"; // + fontAsset.name.Split(new char[] { ' ' })[0];

            if (mode == RenderModes.DistanceField16 || mode == RenderModes.DistanceField32)
                searchPattern += " SDF";
            else
                searchPattern += " Bitmap";

            // Get materials matching the search pattern.
            string[] shaderGUIDs = AssetDatabase.FindAssets(searchPattern);

            string[] shaderList = new string[shaderGUIDs.Length];
            shaders = new Shader[shaderGUIDs.Length];

            for (int i = 0; i < shaderGUIDs.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(shaderGUIDs[i]);
                Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(path);
                shaders[i] = shader;

                string name = shader.name.Replace("TextMeshPro/", "");
                name = name.Replace("Mobile/", "Mobile - ");
                shaderList[i] = name;
            }

            return shaderList;
        }

    }
}