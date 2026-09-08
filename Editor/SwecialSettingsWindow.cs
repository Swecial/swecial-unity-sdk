using UnityEngine;
using UnityEditor;
using com.swecial.unity;

namespace com.swecial.util {

    public class SwecialSettingsWindow : EditorWindow {

        string urlScheme = "";
        string cameraUsageDescription;
        bool groupEnabled;
        bool myBool = true;
        float myFloat = 1.23f;

        public static string URL_SCHEME_KEY = "com.swecial.unity.urlscheme";
        public static string CAMERA_USAGE_DESCRIPTION_KEY = "com.swecial.unity.camerausagedescription";
        public static SwecialSettings settings;

        [MenuItem("Swecial/Settings")]
        private static void NewMenuOption() {
            SwecialSettingsWindow settings = (SwecialSettingsWindow)EditorWindow.GetWindow(typeof(SwecialSettingsWindow));
            settings.init();
            settings.Show();
        }
        void init() {
            settings = SwecialSettings.settings;
            urlScheme = settings.urlScheme; 
            cameraUsageDescription = settings.cameraUsageDescription;
        }
        void OnDisable() {
            if (settings != null) {
                settings.save();
            }
        }
        void OnGUI() {
            if (settings == null) {
                init();
            }
            GUILayout.Label("Post Process Settings", EditorStyles.boldLabel);
            string newUrlScheme = EditorGUILayout.TextField("Url Scheme", urlScheme);
            string newCameraUsageDescription = EditorGUILayout.TextField("Camera Usage Description", cameraUsageDescription);
            if (newUrlScheme != urlScheme) {
                urlScheme = newUrlScheme;
                settings.urlScheme = urlScheme;
                //EditorPrefs.SetString(URL_SCHEME_KEY, urlScheme);
            }
            if (newCameraUsageDescription != cameraUsageDescription) {
                cameraUsageDescription = newCameraUsageDescription;
                settings.cameraUsageDescription = cameraUsageDescription;
                //EditorPrefs.SetString(CAMERA_USAGE_DESCRIPTION_KEY, cameraUsageDescription);
            }
            //groupEnabled = EditorGUILayout.BeginToggleGroup("Optional Settings", groupEnabled);
            //myBool = EditorGUILayout.Toggle("Toggle", myBool);
            //myFloat = EditorGUILayout.Slider("Slider", myFloat, -3, 3);
            //EditorGUILayout.EndToggleGroup();
        }
    }
}
