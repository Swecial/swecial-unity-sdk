using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace com.swecial.unity {
    public class SwecialSettings : ScriptableObject {
        private static SwecialSettings _settings;
        public static SwecialSettings settings {
            get {
                if (_settings != null) {
                    return _settings;
                } else {
                    _settings = AssetDatabase.LoadAssetAtPath<SwecialSettings>("Assets/SwecialSettings/SwecialSettings.asset");
                    if (_settings == null) {
                        _settings = ScriptableObject.CreateInstance<SwecialSettings>();
                        if (!AssetDatabase.IsValidFolder("Assets/SwecialSettings")) {
                            AssetDatabase.CreateFolder("Assets", "SwecialSettings");
                        }
                        AssetDatabase.CreateAsset(_settings, "Assets/SwecialSettings/SwecialSettings.asset");
                        AssetDatabase.SaveAssets();
                    }
                    return _settings;
                }
            }
        }
        public void save() {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
        [SerializeField]
        public string urlScheme;
        [SerializeField]
        public string cameraUsageDescription;
    }
}
