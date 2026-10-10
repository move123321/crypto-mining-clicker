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
 static void SetField(object obj,string name,object value){obj.GetType().GetField(name,BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).SetValue(obj,value);}
 static RectTransform CoolerCard(Canvas canvas){return canvas.transform.Find("Mobile 480 x 854/Modal/Popup/Scroll/Viewport/Content/베이퍼 챔버") as RectTransform;}
 static TMP_Text CoolerDescription(RectTransform card){foreach(var text in card.GetComponentsInChildren<TMP_Text>(true))if(text.text.Contains("°C/초")&&text.text.Contains("₩"))return text;return null;}
 static Button CoolerButton(RectTransform card){var buttons=card.GetComponentsInChildren<Button>(true);if(buttons.Length!=1)throw new Exception("Vapor chamber card must contain exactly one purchase button");return buttons[0];}
 static void CheckSettings(WebGameUI ui,Canvas canvas){
  string[] keys={"mining_music_volume","mining_effects_volume","mining_music_muted","mining_effects_muted","mining_fps","mining_fan_animation"};
  var had=new bool[keys.Length];var saved=new int[keys.Length];int originalFps=Application.targetFrameRate;
  for(int i=0;i<keys.Length;i++){had[i]=PlayerPrefs.HasKey(keys[i]);saved[i]=PlayerPrefs.GetInt(keys[i]);}
  try{
   foreach(var key in keys)PlayerPrefs.DeleteKey(key);
   Call(ui,"ApplyDisplaySettings");if(Application.targetFrameRate!=30)throw new Exception("Default frame cap must be 30");
   Call(ui,"Open","settings");
   var root=((RectTransform)Get(ui,"modal")).Find("Popup/Scroll/Viewport/Content");
   var music=root.Find("배경음악").GetComponentsInChildren<Button>();var musicSlider=root.Find("배경음악").GetComponentInChildren<Slider>();musicSlider.value=50;
   var audio=ui.GetComponent<MiningMusic>();if(audio.MusicVolume!=50)throw new Exception("Music button must persist 50 percent");
   music[0].onClick.Invoke();if(!audio.Muted)throw new Exception("Mute button must mute");music[0].onClick.Invoke();if(audio.Muted||audio.MusicVolume!=50)throw new Exception("Unmute must retain chosen volume");
   root.Find("효과음").GetComponentInChildren<Slider>().value=25;if(audio.EffectsVolume!=25)throw new Exception("Effects button must persist 25 percent");
   musicSlider.value=0;if(audio.MusicVolume!=0)throw new Exception("Slider minimum must be silent");musicSlider.value=100;if(audio.MusicVolume!=100)throw new Exception("Slider maximum must reach 100");musicSlider.value=37;if(audio.MusicVolume!=37)throw new Exception("Slider must support intermediate values");musicSlider.value=50;audio.FlushVolumeSettings();
   root.Find("화면 부드러움").GetComponentsInChildren<Button>()[1].onClick.Invoke();if(Application.targetFrameRate!=60)throw new Exception("60 FPS choice must apply immediately");
   root.Find("팬 애니메이션").GetComponentInChildren<Button>().onClick.Invoke();if(PlayerPrefs.GetInt("mining_fan_animation",1)!=0)throw new Exception("Animation choice must persist");
   ui.Close();Call(ui,"Open","settings");Call(ui,"ApplyDisplaySettings");if(Application.targetFrameRate!=60||audio.MusicVolume!=50||audio.EffectsVolume!=25)throw new Exception("Reopening must retain settings");
   var reload=new GameObject("Reloaded audio settings").AddComponent<MiningMusic>();if(reload.MusicVolume!=50||reload.EffectsVolume!=25)throw new Exception("New component must read saved settings");UnityEngine.Object.DestroyImmediate(reload.gameObject);
   Debug.Log("SETTINGS_CHECKS_PASSED slider 0/37/50/100, mute, FPS, animation, reopen, reload");
  }finally{ui.Close();for(int i=0;i<keys.Length;i++){if(had[i])PlayerPrefs.SetInt(keys[i],saved[i]);else PlayerPrefs.DeleteKey(keys[i]);}PlayerPrefs.Save();Application.targetFrameRate=originalFps;}
 }
 public static void Run(){
  var host=new GameObject("Release UI checks");var g=host.AddComponent<GameManager>();GameManager.I=g;g.SaveEnabled=false;g.S=new SaveData{cash=99999999,runSeconds=20000,totalMined=99999999,propertyId=1,racksInstalled=2};g.S.items.Add(new GpuItem{uid="gpu_1",slot=0,temp=55});g.S.forkUnlocked.Add("root");
  var market=host.AddComponent<MarketManager>();MarketManager.I=market;market.Init();g.EnsureContract();var ui=host.AddComponent<WebGameUI>();Set(ui,"g",g);Set(ui,"market",market);Set(ui,"font",typeof(WebGameUI).GetMethod("CreateUiFont",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,null));Call(ui,"Build");Call(ui,"Refresh");
  var canvas=(Canvas)Get(ui,"canvas");canvas.GetComponent<MobileSafeArea>().enabled=false;
  foreach(var image in canvas.GetComponentsInChildren<Image>(true))if(image.GetComponent<Button>()!=null&&!image.raycastTarget)throw new Exception("Button graphic must receive input: "+image.name);
  var grid=canvas.transform.Find("Mobile 480 x 854/Fixed HUD and room/Grid");if(grid==null||grid.GetComponent<Image>().raycastTarget)throw new Exception("Decorative grid must ignore raycasts");
  SetField(g.S,"cash",30000);SetField(g.S,"runSeconds",1799);Call(ui,"Open","cool");
  var cooler=CoolerCard(canvas);if(cooler==null)throw new Exception("Vapor chamber card must exist");
  var coolerDescription=CoolerDescription(cooler);var coolerButton=CoolerButton(cooler);
  Debug.Log("VAPOR_PRE seconds="+g.S.runSeconds+" description="+(coolerDescription==null?"NULL":coolerDescription.text)+" button="+coolerButton.GetComponentInChildren<TMP_Text>().text+" enabled="+coolerButton.interactable);if(coolerDescription==null||!coolerDescription.text.Contains("운영 30분 필요")||coolerButton.interactable||coolerButton.GetComponentInChildren<TMP_Text>().text!="인증 잠금")throw new Exception("Vapor chamber stays locked at 1,799 seconds");
  SetField(g.S,"runSeconds",1800);Call(ui,"Refresh");
  if(!coolerDescription.text.Contains("구매 가능 · 현금 충분")||!coolerButton.interactable||coolerButton.GetComponentInChildren<TMP_Text>().text!="구매하고 랙에 장착")throw new Exception("Vapor chamber unlocks at 1,800 seconds while menu stays open");
  SetField(g.S,"cash",24999);Call(ui,"Refresh");
  if(!coolerDescription.text.Contains("해금 완료 · 현금 부족")||coolerButton.interactable||coolerButton.GetComponentInChildren<TMP_Text>().text!="현금 부족")throw new Exception("Vapor chamber requires its full price");
  SetField(g.S,"cash",25000);Call(ui,"Refresh");coolerButton.onClick.Invoke();
  if(g.RackCoolerLevel(0)!=2||g.S.coolingItems.Find(x=>x.coolerId==2)==null)throw new Exception("Vapor chamber button purchases and equips cooler tier two");
  ui.Close();Call(ui,"Refresh");
  CheckSettings(ui,canvas);
  var camera=new GameObject("Camera",typeof(Camera)).GetComponent<Camera>();var render=new RenderTexture(1080,2400,24);camera.targetTexture=render;canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=10;var scaler=canvas.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ConstantPixelSize;scaler.scaleFactor=2;
  var screen=(RectTransform)Get(ui,"screen");var safe=new Rect(0,90,1080,2210);screen.localScale=MobileSafeArea.Fit(safe,2);screen.anchoredPosition=(safe.center-new Vector2(540,1200))/2;
  int overflows=0;
  foreach(var size in new[]{new Vector2Int(720,1280),new Vector2Int(1080,2400)}){
   camera.targetTexture=null;render.Release();UnityEngine.Object.DestroyImmediate(render);render=new RenderTexture(size.x,size.y,24);camera.targetTexture=render;
   screen.localScale=MobileSafeArea.Fit(new Rect(0,80,size.x,size.y-120),2);screen.anchoredPosition=new Vector2(0,20);
   foreach(var page in new[]{"settings","privacy","goals","gpu","inventory","cool","shop","fork","trade","estate"}){
    Call(ui,"Open",page);Canvas.ForceUpdateCanvases();
    if(page=="settings")foreach(var slider in ((RectTransform)Get(ui,"modal")).GetComponentsInChildren<Slider>()){
     float trackWidth=((RectTransform)slider.fillRect.parent).rect.width;
     if(slider.handleRect.rect.height>54||Mathf.Abs(slider.fillRect.rect.width-trackWidth*slider.normalizedValue)>.5f)throw new Exception("Volume slider geometry must fit its track and value");
    }
    foreach(var text in canvas.GetComponentsInChildren<TextMeshProUGUI>()){text.ForceMeshUpdate();if(text.isTextOverflowing){overflows++;Debug.Log("LAYOUT_OVERFLOW "+size+" / "+page+" / "+text.text);}}Canvas.ForceUpdateCanvases();camera.Render();camera.Render();
    if(size.x==720&&(page=="trade"||page=="goals"||page=="shop"||page=="settings")){RenderTexture.active=render;var shot=new Texture2D(size.x,size.y,TextureFormat.RGB24,false);shot.ReadPixels(new Rect(0,0,size.x,size.y),0,0);shot.Apply();File.WriteAllBytes(Path.GetFullPath("../audit-"+page+".png"),shot.EncodeToPNG());UnityEngine.Object.DestroyImmediate(shot);RenderTexture.active=null;}
    ui.HandleBack();if(Get(ui,"modal")!=null)throw new Exception("Back must close "+page);
   }
  }
  Debug.Log("LAYOUT_AUDIT_DONE 20 page/size checks; overflow labels="+overflows);
 }
}
