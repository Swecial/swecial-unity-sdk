using UnityEngine;
using UnityEditor;
using com.swecial.unity;

public class MenuItems {
 
    [MenuItem("Tools/Clear PlayerPrefs")]
    private static void NewMenuOption() {
        if (EditorUtility.DisplayDialog("Reset All?", "Are you sure you want to clear all local data?", "Yes", "No")) {
            PlayerPrefs.DeleteAll();
            Prefs.delete();
        }
    }
}
