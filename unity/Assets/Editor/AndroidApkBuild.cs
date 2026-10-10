using System;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class AndroidApkBuild
{
    static bool oneStoreRelease;
    [MenuItem("Crypto Mining/Build ONE store APK")]
    public static void BuildOneStore()
    {
        var oldCustom=PlayerSettings.Android.useCustomKeystore;
        var oldName=PlayerSettings.Android.keystoreName;
        var oldAlias=PlayerSettings.Android.keyaliasName;
        try { oneStoreRelease=true; Build(); }
        finally {
            oneStoreRelease=false;
            PlayerSettings.Android.keystorePass=""; PlayerSettings.Android.keyaliasPass="";
            PlayerSettings.Android.keystoreName=oldName; PlayerSettings.Android.keyaliasName=oldAlias;
            PlayerSettings.Android.useCustomKeystore=oldCustom;
            AssetDatabase.SaveAssets();
        }
    }
    [MenuItem("Crypto Mining/Build Galaxy APK")]
    public static void Build()
    {
        if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android, BuildTarget.Android))
            throw new BuildFailedException("Install Android Build Support, SDK, NDK and OpenJDK for this Unity version.");
        if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android))
            throw new BuildFailedException("Could not activate Android build target.");

        var source = Resources.Load<Font>("CryptoMining/Fonts/NotoSansCJKkr-Regular");
        if (source == null) throw new BuildFailedException("Bundled Korean font is missing.");
        var font = TMP_FontAsset.CreateFontAsset(source);
        if (font == null || !font.TryAddCharacters("암호화폐 채굴 판매 수량 갤럭시"))
            throw new BuildFailedException("Bundled Korean font cannot render Korean.");

        PlayerSettings.productName = "Crypto Mining Clicker";
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.move123321.cryptominingclicker");
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel23;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
        PlayerSettings.Android.useCustomKeystore = oneStoreRelease;
        if (oneStoreRelease) {
            var path=Environment.GetEnvironmentVariable("CRYPTO_UPLOAD_KEYSTORE");
            var alias=Environment.GetEnvironmentVariable("CRYPTO_UPLOAD_ALIAS");
            var password=Environment.GetEnvironmentVariable("CRYPTO_UPLOAD_PASSWORD");
            var keyPassword=Environment.GetEnvironmentVariable("CRYPTO_UPLOAD_KEY_PASSWORD");
            if(string.IsNullOrEmpty(path)||!File.Exists(path)||string.IsNullOrEmpty(alias)||string.IsNullOrEmpty(password)||string.IsNullOrEmpty(keyPassword))
                throw new BuildFailedException("ONE store release requires a private signing key and CRYPTO_UPLOAD_* environment variables.");
            PlayerSettings.Android.keystoreName=path; PlayerSettings.Android.keyaliasName=alias;
            PlayerSettings.Android.keystorePass=password; PlayerSettings.Android.keyaliasPass=keyPassword;
        }
        EditorUserBuildSettings.buildAppBundle = false;
        EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
        EditorUserBuildSettings.development = false;
        var output = Path.GetFullPath("Builds/Android/CryptoMiningClicker.apk");
        var args = Environment.GetCommandLineArgs();
        for (int i = 0; i + 1 < args.Length; i++)
            if (args[i] == "-apkOutputPath") output = Path.GetFullPath(args[i + 1]);
        Directory.CreateDirectory(Path.GetDirectoryName(output));
        AssetDatabase.SaveAssets();
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Scenes/Main.unity" },
            target = BuildTarget.Android,
            locationPathName = output,
            options = BuildOptions.None
        });
        if (report.summary.result != BuildResult.Succeeded)
            throw new BuildFailedException("Android build failed: " + report.summary.result);
        Debug.Log("GALAXY_APK_SUCCESS " + output + " bytes=" + report.summary.totalSize);
    }
}
