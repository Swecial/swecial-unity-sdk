#if UNITY_EDITOR && !(UNITY_WINRT || UNITY_WEBPLAYER || UNITY_WEBGL)
using UnityEditor;
using UnityEditor.Callbacks;
using System.Collections.Generic;
using System.IO;
using com.swecial.util;
using UnityEngine;
using System.Collections;
using UnityEditor.iOS.Xcode;

namespace com.swecial.unity {

  public class SwecialPostProcessBuild {

    private const string kRelativePathInfoPlistFile = "Info.plist";
    private const string kRelativePathInfoPlistBackupFile = "Info.backup.plist";
    private static string nsCameraUsageDescription;
    private static string deepLinkUrlScheme;

    public static string URL_SCHEME_KEY = "com.swecial.unity.urlscheme";
    public static string CAMERA_USAGE_DESCRIPTION_KEY = "com.swecial.unity.camerausagedescription";

    [PostProcessBuild(0)]
    public static void OnPostProcessBuildActionStart(BuildTarget _target, string _buildPath) {
      string _targetStr = _target.ToString();
      nsCameraUsageDescription = SwecialSettings.settings.cameraUsageDescription;
      deepLinkUrlScheme = SwecialSettings.settings.urlScheme;
      Debug.Log("pp1: " + nsCameraUsageDescription);
      Debug.Log("pp: " + deepLinkUrlScheme);
      Debug.Log("pp: " + PlayerSettings.applicationIdentifier);
      Debug.Log($"pp: {_target.ToString()}");
      if (_targetStr.Equals("iOS") || _targetStr.Equals("iPhone")) {
        // camera plist stuff is no longer needed in new unity
        iOSPostProcessBuild(_target, _buildPath);
        return;
      }
    }

    private static void iOSPostProcessBuild(BuildTarget _target, string _buildPath) {
      string _infoPlistFilePath = GetInfoPlistFilePath(_buildPath);
      modifyInfoPlist(_buildPath);
      modifyUnityAppController(_buildPath);
      addFrameworks(_buildPath);
    }

    private static void addFrameworks(string path) {
      string projPath = path + "/Unity-iPhone.xcodeproj/project.pbxproj";
      PBXProject proj = new PBXProject();
      var file = File.ReadAllText(projPath);
      proj.ReadFromString(file);
      string target = proj.GetUnityMainTargetGuid();
      proj.AddFrameworkToProject(target, "AdSupport.framework", false);
      proj.AddFrameworkToProject(target, "CoreData.framework", false);
      proj.AddFrameworkToProject(target, "SystemConfiguration.framework", false);
      proj.AddFrameworkToProject(target, "libz.tbd", false);
      proj.AddFrameworkToProject(target, "libsqlite3.tbd", false);
      File.WriteAllText(projPath, proj.WriteToString());
    }

    private static void modifyUnityAppController(string _buildPath) {

      string openUrlInjectionBlock = @"
    // Code injected by Swecial
    NSLog(@""Swecial: openUrl"");
    NSString *swecialUrlString = [url absoluteString];
    [[NSUserDefaults standardUserDefaults] setObject:swecialUrlString forKey:@""com.swecial.unity.deeplink.url""];
    [[NSUserDefaults standardUserDefaults] synchronize];
    
    ";

      string didFinishLaunchingWithOptionsInjectionBlock = @"
    // Code injected by Swecial
    NSLog(@""Swecial: didFinishLaunchingWithOptions"");
    NSURL *swecialUrl = launchOptions[UIApplicationLaunchOptionsURLKey];
    if (swecialUrl) {
        NSString *swecialUrlString = [swecialUrl absoluteString];
        [[NSUserDefaults standardUserDefaults] setObject:swecialUrlString forKey:@""com.swecial.unity.deeplink.url""];
        [[NSUserDefaults standardUserDefaults] synchronize];
    }
    
    ";

      string unityAppController = GetUnityAppControllerFilePath(_buildPath);

      StringModifier sm = new StringModifier(unityAppController.readFile());
      sm.toNext("openURL:(NSURL*)url");
      sm.toNext("{");
      string block = sm.getCodeBlock();
      sm.toStartOfNextLine();
      sm.inject(openUrlInjectionBlock);
      Debug.Log("block: " + block);

      sm.toStart();
      sm.toNext("didFinishLaunchingWithOptions");
      sm.toNext("{");
      block = sm.getCodeBlock();
      sm.toStartOfNextLine();
      sm.inject(didFinishLaunchingWithOptionsInjectionBlock);
      Debug.Log("block:" + block);

      unityAppController.writeFile(sm.ToString());
    }

    private static void modifyInfoPlist(string _buildPath) {

      string infoPlistPath = GetInfoPlistFilePath(_buildPath);

      Plist infoPlist = Plist.LoadPlistAtPath(infoPlistPath);

      Dictionary<string, object> newEntries = new Dictionary<string, object>();

      // camera bullshit (may need to be set even if camera is not used)
      if (!nsCameraUsageDescription.isEmpty()) {
        newEntries["NSCameraUsageDescription"] = nsCameraUsageDescription;
      }
      // url schemes
      if (!deepLinkUrlScheme.isEmpty()) {
        IList cfBundleUrlTypes = (IList)infoPlist.GetKeyPathValue("CFBundleURLTypes");
        if (cfBundleUrlTypes == null) {
          cfBundleUrlTypes = new ArrayList();
        }
        Hashtable cfBundleUrlSettings = new Hashtable();
        ArrayList schemes = new ArrayList();
        schemes.Add(deepLinkUrlScheme);
        cfBundleUrlSettings.Add("CFBundleTypeRole", "Editor");
        cfBundleUrlSettings.Add("CFBundleURLName", PlayerSettings.applicationIdentifier);
        cfBundleUrlSettings.Add("CFBundleURLSchemes", schemes);
        cfBundleUrlTypes.Add(cfBundleUrlSettings);
        newEntries["CFBundleURLTypes"] = cfBundleUrlTypes;
      }

      if (newEntries.Count == 0)
        return;

      // First create a backup of old data
      string _infoPlistBackupSavePath = GetInfoPlistBackupFilePath(_buildPath);

      infoPlist.Save(_infoPlistBackupSavePath);

      // Now add new entries
      foreach (string _key in newEntries.Keys) {
        infoPlist.AddValue(_key, newEntries[_key]);
      }

      // Save these changes
      infoPlist.Save(infoPlistPath);
    }

    private static string GetInfoPlistFilePath(string _buildPath) {
      return Path.Combine(_buildPath, kRelativePathInfoPlistFile);
    }
    private static string GetUnityAppControllerFilePath(string _buildPath) {
      return Path.Combine(Path.Combine(_buildPath, "Classes"), "UnityAppController.mm");
    }

    private static string GetInfoPlistBackupFilePath(string _buildPath) {
      return Path.Combine(_buildPath, kRelativePathInfoPlistBackupFile);
    }

  }
}
#endif