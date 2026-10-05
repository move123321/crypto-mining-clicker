using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
namespace CryptoMining {
public static class SaveStore {
 public static bool TryRead(string path,out SaveData data){
  data=null;if(!File.Exists(path))return false;
  try {var json=File.ReadAllText(path);if(!json.Contains("\"items\"")||!json.Contains("\"version\""))return false;
   var s=JsonUtility.FromJson<SaveData>(json);if(s==null||s.version<1||s.version>3||s.items==null||s.coins==null||!Finite(s.cash)||s.cash<0||!Finite(s.totalMined))return false;
   // Unity serializes a null inline class as an empty object in older saves.
   if(s.contract!=null&&string.IsNullOrEmpty(s.contract.type)&&string.IsNullOrEmpty(s.contract.coinId)&&s.contract.target==0&&s.contract.progress==0&&s.contract.reward==0)s.contract=null;
   if(s.contract!=null){var c=s.contract;if((c.type!="mine"&&c.type!="sell")||Array.Find(Catalog.Coins,x=>x.id==c.coinId)==null||!Finite(c.target)||c.target<=0||!Finite(c.progress)||c.progress<0||c.progress>c.target||!Finite(c.reward)||c.reward<0)return false;}
   var ids=new HashSet<string>();var slots=new HashSet<int>();foreach(var gpu in s.items)if(gpu==null||string.IsNullOrEmpty(gpu.uid)||!ids.Add(gpu.uid)||gpu.modelId<0||gpu.modelId>8||!Finite(gpu.temp)||gpu.slot < -1||gpu.slot>15||(gpu.slot>=0&&!slots.Add(gpu.slot)))return false;
   foreach(var coin in s.coins)if(coin==null||!Finite(coin.amount)||coin.amount<0)return false;
   var coolerIds=new HashSet<string>();
   if(s.coolingItems!=null)foreach(var cooler in s.coolingItems)if(cooler==null||string.IsNullOrEmpty(cooler.uid)||!coolerIds.Add(cooler.uid)||cooler.coolerId<1||cooler.coolerId>5)return false;
   if(s.markets!=null)foreach(var market in s.markets){if(market==null||market.candles==null||market.candles.Count==0)return false;foreach(var candle in market.candles)if(candle==null||!Finite(candle.open)||!Finite(candle.close)||!Finite(candle.high)||!Finite(candle.low)||candle.close<=0)return false;}
   data=s;return true;
  }catch(Exception){return false;}
 }
 static bool Finite(double v){return !double.IsNaN(v)&&!double.IsInfinity(v);}
 public static SaveData Load(string path,out string notice,out bool blocked){
  notice="";blocked=false;SaveData data;
  if(TryRead(path,out data))return data;
  foreach(var candidate in new[]{path+".bak",path+".tmp"})if(TryRead(candidate,out data)){notice="저장 오류를 발견해 정상 백업에서 진행 상황을 복구했습니다.";return data;}
  blocked=File.Exists(path)||File.Exists(path+".bak")||File.Exists(path+".tmp");
  if(blocked)notice="저장 파일과 백업을 읽을 수 없습니다. 기존 파일을 보호하기 위해 자동 저장을 중지했습니다.";
  return null;
 }
 public static void Write(string path,SaveData data){
  Directory.CreateDirectory(Path.GetDirectoryName(path));string tmp=path+".tmp";SaveData previous;
  if(File.Exists(path)){if(TryRead(path,out previous))File.Copy(path,path+".bak",true);else File.Copy(path,path+".corrupt-"+Guid.NewGuid().ToString("N"));}
  File.WriteAllText(tmp,JsonUtility.ToJson(data));if(!TryRead(tmp,out previous))throw new InvalidDataException("Generated save is invalid; existing save preserved.");
  if(File.Exists(path)){
   try{File.Replace(tmp,path,null);}
   catch(PlatformNotSupportedException){File.Copy(tmp,path,true);File.Delete(tmp);}
  }else File.Move(tmp,path);
 }
}
}
