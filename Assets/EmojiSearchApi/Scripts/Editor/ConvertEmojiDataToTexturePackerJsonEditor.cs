using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using TMPro.EditorUtilities;
using TMPro.SpriteAssetUtilities;

namespace TMPro
{
    public class ConvertEmojiDataToTexturePackerJsonEditor : EditorWindow
    {
        #region Private Variables

        [SerializeField]
        TextAsset m_textAsset = null;
        [SerializeField]
        Vector2Int m_gridSize = new Vector2Int(32, 32);
        [SerializeField]
        Vector2Int m_padding = new Vector2Int(1, 1);
        [SerializeField]
        Vector2Int m_spacing = new Vector2Int(2, 2);

        #endregion

        #region Static Init

        [MenuItem("Window/TextMeshPro/Convert EmojiData to TexturePacker JSON")]
        static void Init()
        {
            // Get existing open window or if none, make a new one:
            ConvertEmojiDataToTexturePackerJsonEditor window = (ConvertEmojiDataToTexturePackerJsonEditor)EditorWindow.GetWindow(typeof(ConvertEmojiDataToTexturePackerJsonEditor));
            window.titleContent = new GUIContent("Convert To EmojiData to TexturePacker JSON");
            window.ShowUtility();
        }

        #endregion

        #region Unity Functions

        protected virtual void OnGUI()
        {
            EditorGUILayout.HelpBox("Use this tool to convert EmojiData Format to TexturePacket Json format (used in Sprite Import window).\n" +
                "A file with name texturepacker_<OriginalFileName> will be generated in same folder of the json provided\n\n" +
                "Check for the last EmojiData JSON and Spritesheets in:\nhttps://github.com/iamcal/emoji-data", MessageType.Info);

            EditorGUILayout.Space();

            m_textAsset = EditorGUILayout.ObjectField("EmojiData To Convert", m_textAsset, typeof(TextAsset), false) as TextAsset;
            EditorGUILayout.Space();
            m_gridSize = EditorGUILayout.Vector2IntField("Grid Size", m_gridSize);
            m_padding = EditorGUILayout.Vector2IntField("Padding", m_padding);
            m_spacing = EditorGUILayout.Vector2IntField("Spacing", m_spacing);
            EditorGUILayout.Space();

            var v_assetPath = m_textAsset != null? AssetDatabase.GetAssetPath(m_textAsset) : "";
            GUI.enabled = m_textAsset != null && !string.IsNullOrEmpty(m_textAsset.text) && !string.IsNullOrEmpty(v_assetPath);
            if (GUILayout.Button("Convert To TexturePacker JSON"))
            {
                var v_json = ConvertToEmojiOne(m_textAsset.text);

                if (!string.IsNullOrEmpty(v_json))
                {
                    var v_fileName = System.IO.Path.GetFileName(v_assetPath);
                    var v_newPath = v_assetPath.Replace(v_fileName, "texturepacker_" + v_fileName);

                    System.IO.File.WriteAllText(Application.dataPath.Replace("Assets", "") + v_newPath, v_json);
                    AssetDatabase.Refresh();
                }
                else
                    Debug.Log("Failed to convert to TexturePacker Json");
            }
        }

        #endregion

        #region Helper Functions

        protected virtual string ConvertToEmojiOne(string p_json)
        {
            try
            {
                //Unity cannot deserialize Dictionary, so we converted the dictionary to List using MiniJson
                p_json = ConvertToUnityJsonFormat(p_json);
                PreConvertedSpritesheetData v_preData = JsonUtility.FromJson<PreConvertedSpritesheetData>(p_json);
                TexturePacker.SpriteDataObject v_postData = v_preData.ToTexturePacketDataObject(m_gridSize, m_padding, m_spacing);

                return JsonUtility.ToJson(v_postData);
            }
            catch (System.Exception p_exception)
            {
                Debug.Log("Failed to convert to EmojiOne\n: " + p_exception);
            }

            return "";
        }

        protected virtual string ConvertToUnityJsonFormat(string p_json)
        {
            p_json = "{\"frames\":" + p_json + "}";

            var v_changed = false;
            var v_jObject = MiniJsonEditor.Deserialize(p_json) as Dictionary<string, object>;
            if (v_jObject != null)
            {
                var v_array = v_jObject.ContainsKey("frames") ? v_jObject["frames"] as IList : null;
                if (v_array != null)
                {
                    foreach (var v_jPreDataNonCasted in v_array)
                    {
                        var v_jPredataObject = v_jPreDataNonCasted as Dictionary<string, object>;
                        if (v_jPredataObject != null)
                        {
                            var v_skin_variation_dict = v_jPredataObject.ContainsKey("skin_variations") ? v_jPredataObject["skin_variations"] as Dictionary<string, object> : null;

                            if (v_skin_variation_dict != null)
                            {
                                v_changed = true;
                                List<object> v_skin_variation_array = new List<object>();

                                foreach (var v_skinVariationObject in v_skin_variation_dict.Values)
                                {
                                    
                                    v_skin_variation_array.Add(v_skinVariationObject);
                                }
                                v_jPredataObject["skin_variations"] = v_skin_variation_array;
                            }
                        }
                    }
                }
            }
            return v_jObject != null && v_changed ? MiniJsonEditor.Serialize(v_jObject) : p_json;
        }

        #endregion

        #region Helper Classes

        [System.Serializable]
        public class PreConvertedSpritesheetData
        {
            public List<PreConvertedImgDataWithVariants> frames = new List<PreConvertedImgDataWithVariants>();

            public virtual TexturePacker.SpriteDataObject ToTexturePacketDataObject(Vector2Int p_gridSize, Vector2 p_padding, Vector2 p_spacing)
            {
                TexturePacker.SpriteDataObject v_postData = new TexturePacker.SpriteDataObject();
                v_postData.frames = new List<TexturePacker.SpriteData>();

                if (frames != null)
                {
                    var v_framesToCheck = new List<PreConvertedImgData>();
                    if (frames != null)
                    {
                        foreach (var v_frameToCheck in frames)
                        {
                            v_framesToCheck.Add(v_frameToCheck);
                        }
                    }

                    for(int i=0; i< v_framesToCheck.Count; i++)
                    {
                        var v_preFrame = v_framesToCheck[i];

                        //Add all variations in list to check (after the current PreFrame)
                        var v_preFrameWithVariants = v_framesToCheck[i] as PreConvertedImgDataWithVariants;
                        if (v_preFrameWithVariants != null && v_preFrameWithVariants.skin_variations != null && v_preFrameWithVariants.skin_variations.Count > 0)
                        {
                            for (int j = v_preFrameWithVariants.skin_variations.Count-1; j >=0; j--)
                            {
                                var v_skinVariantFrame = v_preFrameWithVariants.skin_variations[j];
                                if (v_skinVariantFrame != null)
                                    v_framesToCheck.Insert(i+1, v_skinVariantFrame);
                            }
                        }

                        //Create TexturePacker SpriteData
                        var v_postFrame = new TexturePacker.SpriteData();

                        v_postFrame.filename = v_preFrame.image;
                        v_postFrame.rotated = false;
                        v_postFrame.trimmed = false;
                        v_postFrame.sourceSize = new TexturePacker.SpriteSize() { w = p_gridSize.x, h = p_gridSize.y };
                        v_postFrame.spriteSourceSize = new TexturePacker.SpriteFrame() { x = 0, y = 0, w = p_gridSize.x, h = p_gridSize.y };
                        v_postFrame.frame = new TexturePacker.SpriteFrame()
                        {
                            x = (v_preFrame.sheet_x * (p_gridSize.x + p_spacing.x)) + p_padding.x,
                            y = (v_preFrame.sheet_y * (p_gridSize.y + p_spacing.y)) + p_padding.y,
                            w = p_gridSize.x,
                            h = p_gridSize.y
                        };
                        v_postFrame.pivot = new Vector2(0f, 0f);

                        v_postData.frames.Add(v_postFrame);
                    }
                }

                return v_postData;
            }
        }

        [System.Serializable]
        public class PreConvertedImgData
        {
            public string name;
            public string unified;
            public string non_qualified;
            public string docomo;
            public string au;
            public string softbank;
            public string google;
            public string image;
            public int sheet_x;
            public int sheet_y;
            public string short_name;
            public string[] short_names;
            public object text;
            public object texts;
            public string category;
            public int sort_order;
            public string added_in;
            public bool has_img_apple;
            public bool has_img_google;
            public bool has_img_twitter;
            public bool has_img_facebook;
            public bool has_img_messenger;
        }

        [System.Serializable]
        public class PreConvertedImgDataWithVariants : PreConvertedImgData
        {
            public List<PreConvertedImgData> skin_variations = new List<PreConvertedImgData>();
        }

        #endregion
    }
}
