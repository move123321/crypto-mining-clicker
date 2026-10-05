using System;
using System.IO;
using System.Reflection;
using CryptoMining;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public static class VerifyMilestoneUI {
 static BindingFlags flags=BindingFlags.Instance|BindingFlags.NonPublic;
 static void Call(object obj,string name,params object[] args){obj.GetType().GetMethod(name,flags).Invoke(obj,args);}
 static object Get(object obj,string name){return obj.GetType().GetField(name,flags).GetValue(obj);}
 static void Set(object obj,string name,object value){obj.GetType().GetField(name,flags).SetValue(obj,value);}
 public static void Run(){
  MilestoneChecks.Run();var host=new GameObject("Release UI checks");var g=host.AddComponent<GameManager>();GameManager.I=g;g.SaveEnabled=false;g.S=new SaveData{cash=4000};g.S.items.Add(new GpuItem{uid="gpu_1",slot=0,temp=55});g.S.forkUnlocked.Add("root");
  var market=host.AddComponent<MarketManager>();MarketManager.I=market;market.Init();g.EnsureContract();var ui=host.AddComponent<WebGameUI>();Set(ui,"g",g);Set(ui,"market",market);Set(ui,"font",typeof(WebGameUI).GetMethod("CreateUiFont",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,null));Call(ui,"Build");Call(ui,"Refresh");
  var canvas=(Canvas)Get(ui,"canvas");canvas.GetComponent<MobileSafeArea>().enabled=false;
  var camera=new GameObject("Camera",typeof(Camera)).GetComponent<Camera>();var render=new RenderTexture(1080,2400,24);camera.targetTexture=render;canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=10;var scaler=canvas.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ConstantPixelSize;scaler.scaleFactor=2;
  var screen=(RectTransform)Get(ui,"screen");var safe=new Rect(0,90,1080,2210);screen.localScale=MobileSafeArea.Fit(safe,2);screen.anchoredPosition=(safe.center-new Vector2(540,1200))/2;
  foreach(bool rebirth in new[]{false,true}){
   Call(ui,"PlayMilestone",rebirth);
   if(!g.IntroPaused||Get(ui,"intro")==null)throw new Exception("Milestone must pause simulation");
   if(!((RectTransform)Get(ui,"intro")).GetComponent<UnityEngine.UI.Image>().raycastTarget)throw new Exception("Milestone overlay must block taps behind it");
   var intro=(RectTransform)Get(ui,"intro");
   var scene=(RectTransform)intro.Find("Milestone stage/Animated scene");scene.anchoredPosition=Vector2.zero;scene.GetComponent<CanvasGroup>().alpha=1;
   Canvas.ForceUpdateCanvases();foreach(var text in canvas.GetComponentsInChildren<TextMeshProUGUI>())text.ForceMeshUpdate();Canvas.ForceUpdateCanvases();camera.Render();camera.Render();RenderTexture.active=render;var shot=new Texture2D(1080,2400,TextureFormat.RGB24,false);shot.ReadPixels(new Rect(0,0,1080,2400),0,0);shot.Apply();File.WriteAllBytes(Path.GetFullPath("../milestone-"+(rebirth?"rebirth":"studio")+".png"),shot.EncodeToPNG());UnityEngine.Object.DestroyImmediate(shot);
   if(rebirth)ui.HandleBack();else foreach(var button in intro.GetComponentsInChildren<Button>())if(button.name=="건너뛰기")button.onClick.Invoke();
   if(g.IntroPaused||Get(ui,"intro")!=null||Get(ui,"milestoneRoutine")!=null)throw new Exception("Skip/back must remove overlay, stop animation and resume simulation");
   Call(ui,"PlayMilestone",rebirth);if(Get(ui,"intro")!=null||g.IntroPaused)throw new Exception("Seen milestone must not replay");
  }
  Debug.Log("MILESTONE_UI_PASSED studio, rebirth, skip, Android back, repeat guard");
 }
}
