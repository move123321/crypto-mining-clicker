using System;
using System.IO;
using System.Reflection;
using CryptoMining;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public static class ReleaseLayoutAudit {
 static BindingFlags flags=BindingFlags.Instance|BindingFlags.NonPublic;
 static void Call(object obj,string name,params object[] args){obj.GetType().GetMethod(name,flags).Invoke(obj,args);}
 static object Get(object obj,string name){return obj.GetType().GetField(name,flags).GetValue(obj);}
 static void Set(object obj,string name,object value){obj.GetType().GetField(name,flags).SetValue(obj,value);}
 public static void Run(){
  var host=new GameObject("Release UI checks");var g=host.AddComponent<GameManager>();GameManager.I=g;g.SaveEnabled=false;g.S=new SaveData{cash=99999999,runSeconds=20000,totalMined=99999999,propertyId=1,racksInstalled=2};g.S.items.Add(new GpuItem{uid="gpu_1",slot=0,temp=55});g.S.forkUnlocked.Add("root");
  var market=host.AddComponent<MarketManager>();MarketManager.I=market;market.Init();g.EnsureContract();var ui=host.AddComponent<WebGameUI>();Set(ui,"g",g);Set(ui,"market",market);Set(ui,"font",typeof(WebGameUI).GetMethod("CreateUiFont",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,null));Call(ui,"Build");Call(ui,"Refresh");
  var canvas=(Canvas)Get(ui,"canvas");canvas.GetComponent<MobileSafeArea>().enabled=false;
  var camera=new GameObject("Camera",typeof(Camera)).GetComponent<Camera>();var render=new RenderTexture(1080,2400,24);camera.targetTexture=render;canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=10;var scaler=canvas.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ConstantPixelSize;scaler.scaleFactor=2;
  var screen=(RectTransform)Get(ui,"screen");var safe=new Rect(0,90,1080,2210);screen.localScale=MobileSafeArea.Fit(safe,2);screen.anchoredPosition=(safe.center-new Vector2(540,1200))/2;
  int overflows=0;
  foreach(var size in new[]{new Vector2Int(720,1280),new Vector2Int(1080,2400)}){
   camera.targetTexture=null;render.Release();UnityEngine.Object.DestroyImmediate(render);render=new RenderTexture(size.x,size.y,24);camera.targetTexture=render;
   screen.localScale=MobileSafeArea.Fit(new Rect(0,80,size.x,size.y-120),2);screen.anchoredPosition=new Vector2(0,20);
   foreach(var page in new[]{"settings","goals","gpu","inventory","cool","shop","fork","trade","estate"}){
    Call(ui,"Open",page);Canvas.ForceUpdateCanvases();foreach(var text in canvas.GetComponentsInChildren<TextMeshProUGUI>()){text.ForceMeshUpdate();if(text.isTextOverflowing){overflows++;Debug.Log("LAYOUT_OVERFLOW "+size+" / "+page+" / "+text.text);}}Canvas.ForceUpdateCanvases();camera.Render();camera.Render();
    if(size.x==720&&(page=="trade"||page=="goals"||page=="shop")){RenderTexture.active=render;var shot=new Texture2D(size.x,size.y,TextureFormat.RGB24,false);shot.ReadPixels(new Rect(0,0,size.x,size.y),0,0);shot.Apply();File.WriteAllBytes(Path.GetFullPath("../audit-"+page+".png"),shot.EncodeToPNG());UnityEngine.Object.DestroyImmediate(shot);RenderTexture.active=null;}
    ui.HandleBack();if(Get(ui,"modal")!=null)throw new Exception("Back must close "+page);
   }
  }
  Debug.Log("LAYOUT_AUDIT_DONE 18 page/size checks; overflow labels="+overflows);
 }
}
