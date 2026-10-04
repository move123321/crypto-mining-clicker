using System;
using System.IO;
using CryptoMining;
using UnityEngine;
public static class ReleaseChecks {
 static int count;static void Check(bool pass,string message){if(!pass)throw new Exception(message);count++;}
 public static void Run(){
  EstateChecks.Run();var folder=Path.Combine(Path.GetTempPath(),"crypto-save-check-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(folder);var path=Path.Combine(folder,"save.json");
  var s=new SaveData{cash=123};s.items.Add(new GpuItem{uid="gpu_1",slot=0});s.coins.Add(new CoinBalance{id="btcx",amount=50});s.forkUnlocked.Add("root");
  string notice;bool blocked;Check(SaveStore.Load(path,out notice,out blocked)==null&&!blocked,"Missing file is a normal new game");
  SaveStore.Write(path,s);s.cash=456;SaveStore.Write(path,s);Check(SaveStore.Load(path,out notice,out blocked).cash==456,"Latest valid primary loads");
  File.WriteAllText(path,"{broken");var restored=SaveStore.Load(path,out notice,out blocked);Check(restored.cash==123&&!blocked&&notice!="","Corrupt primary recovers backup");SaveStore.Write(path,restored);Check(Directory.GetFiles(folder,"*.corrupt-*").Length==1,"Corrupt original is preserved");
  File.WriteAllText(path,"{}");File.WriteAllText(path+".bak","bad");Check(SaveStore.Load(path,out notice,out blocked)==null&&blocked,"Both invalid files block autosave");Check(File.ReadAllText(path)=="{}","Failed recovery does not overwrite original");
  File.WriteAllText(path+".tmp",JsonUtility.ToJson(s));Check(SaveStore.Load(path,out notice,out blocked).cash==456&&!blocked,"Interrupted first write can recover temporary save");
  s.items.Add(new GpuItem{uid="duplicate",slot=0});File.WriteAllText(path+".tmp",JsonUtility.ToJson(s));SaveData parsed;Check(!SaveStore.TryRead(path+".tmp",out parsed),"Duplicate slots rejected");s.items.Clear();SaveStore.Write(path,s);Check(SaveStore.Load(path,out notice,out blocked).items.Count==0,"Empty GPU inventory is valid");
  var host=new GameObject("Cooldown check");var g=host.AddComponent<GameManager>();GameManager.I=g;g.SaveEnabled=false;g.S=s;g.S.items.Add(new GpuItem{uid="cooling",slot=0,temp=80});g.EmergencyCool();Check(g.At(0).temp==60&&g.EmergencyCoolRemaining()==120,"Emergency cooling has fixed effect and timeout");g.EmergencyCool();Check(g.At(0).temp==60,"Repeated cooling cannot bypass timeout");
  var saved=JsonUtility.FromJson<SaveData>(JsonUtility.ToJson(g.S));g.S=saved;g.MigrateSave();Check(g.EmergencyCoolRemaining()==120,"Cooldown survives reload");g.IntroPaused=true;g.Tick();Check(g.EmergencyCoolRemaining()==120,"Paused time does not advance cooldown");g.IntroPaused=false;for(int i=0;i<120;i++)g.Tick();Check(g.EmergencyCoolRemaining()==0,"Cooling unlocks after active time");
  foreach(var safe in new[]{new Rect(0,0,1080,1920),new Rect(0,90,1080,2170),new Rect(30,50,660,1180)}){var scale=MobileSafeArea.Fit(safe,2);Check(480*2*scale.x<=safe.width+.1f&&854*2*scale.y<=safe.height+.1f,"Portrait UI fits safe area");}
  UnityEngine.Object.DestroyImmediate(host);foreach(var file in Directory.GetFiles(folder))File.Delete(file);Directory.Delete(folder);
  Debug.Log("RELEASE_CHECKS_PASSED "+count+" save corruption, interruption, cooldown and safe-area checks");
 }
}
