using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
// Passwords are supplied only through environment variables, never written into the project.
public static class StoreBundleBuild {
 [MenuItem("Crypto Mining/Build Store AAB")]
 public static void Build(){
  var path=Environment.GetEnvironmentVariable("CRYPTO_UPLOAD_KEYSTORE");var alias=Environment.GetEnvironmentVariable("CRYPTO_UPLOAD_ALIAS");var password=Environment.GetEnvironmentVariable("CRYPTO_UPLOAD_PASSWORD");var keyPassword=Environment.GetEnvironmentVariable("CRYPTO_UPLOAD_KEY_PASSWORD");
  if(string.IsNullOrEmpty(path)||!File.Exists(path)||string.IsNullOrEmpty(alias)||string.IsNullOrEmpty(password)||string.IsNullOrEmpty(keyPassword))throw new BuildFailedException("Release signing requires CRYPTO_UPLOAD_KEYSTORE, CRYPTO_UPLOAD_ALIAS, CRYPTO_UPLOAD_PASSWORD, CRYPTO_UPLOAD_KEY_PASSWORD. Do not commit keys or passwords.");
  var oldBundle=EditorUserBuildSettings.buildAppBundle;var oldCustom=PlayerSettings.Android.useCustomKeystore;var oldName=PlayerSettings.Android.keystoreName;var oldAlias=PlayerSettings.Android.keyaliasName;
  try{
   EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android,BuildTarget.Android);EditorUserBuildSettings.buildAppBundle=true;PlayerSettings.Android.useCustomKeystore=true;PlayerSettings.Android.keystoreName=path;PlayerSettings.Android.keyaliasName=alias;PlayerSettings.Android.keystorePass=password;PlayerSettings.Android.keyaliasPass=keyPassword;
   PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android,ScriptingImplementation.IL2CPP);PlayerSettings.Android.targetArchitectures=AndroidArchitecture.ARM64;PlayerSettings.defaultInterfaceOrientation=UIOrientation.Portrait;
   Directory.CreateDirectory("Builds/Android");var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/Scenes/Main.unity"},target=BuildTarget.Android,locationPathName="Builds/Android/CryptoMiningClicker.aab",options=BuildOptions.None});if(result.summary.result!=BuildResult.Succeeded)throw new BuildFailedException("Store bundle build failed.");
  }finally{PlayerSettings.Android.keystorePass="";PlayerSettings.Android.keyaliasPass="";PlayerSettings.Android.keystoreName=oldName;PlayerSettings.Android.keyaliasName=oldAlias;PlayerSettings.Android.useCustomKeystore=oldCustom;EditorUserBuildSettings.buildAppBundle=oldBundle;}
 }
}
