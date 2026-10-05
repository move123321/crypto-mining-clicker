using System;
using System.IO;
using System.Reflection;
using CryptoMining;
using UnityEngine;
public static class ReleaseAuditChecks {
 static int count;static int failures;
 static void Check(bool pass,string message){count++;if(!pass){failures++;Debug.LogError("AUDIT_FAIL "+message);}}
 public static void Run(){
  count=failures=0;MilestoneChecks.Run();
  var host=new GameObject("Release audit");var g=host.AddComponent<GameManager>();GameManager.I=g;g.SaveEnabled=false;
  g.S=(SaveData)typeof(GameManager).GetMethod("Fresh",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(g,null);
  g.S.cash=100000;g.S.runSeconds=600;g.S.totalMined=20000;g.BuyGpu(1,1);g.Unequip("gpu_1");g.SetMiningCoin("ethx");g.S.coins.Find(c=>c.id=="ethx").amount=100;
  g.S.contract=new ContractState{type="mine",coinId="ethx",target=1000,progress=200,reward=3500};g.S.repairPending=true;g.S.running=false;g.ResolveOverheat(false);
  Check(g.CoinUnlocked(Catalog.Coin("ethx")),"GPU loss must not relock earned coin access or strand active contract");
  g.SetTradeCoin("ethx");Check(g.S.tradeCoinId=="ethx","Earned ETHER remains selectable for sale");
  g.S=JsonUtility.FromJson<SaveData>(JsonUtility.ToJson(g.S));g.MigrateSave();Check(g.CoinUnlocked(Catalog.Coin("ethx")),"Coin unlock persists after reload");
  var folder=Path.Combine(Path.GetTempPath(),"crypto-audit-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(folder);var path=Path.Combine(folder,"save.json");SaveStore.Write(path,g.S);
  g.S.contract.reward=-100;File.WriteAllText(path,JsonUtility.ToJson(g.S));SaveData parsed;Check(!SaveStore.TryRead(path,out parsed),"Negative contract rewards must be rejected");
  g.S.contract.reward=100;g.S.contract.coinId="missing";File.WriteAllText(path,JsonUtility.ToJson(g.S));Check(!SaveStore.TryRead(path,out parsed),"Unknown contract coin must be rejected");
  g.S.contract=null;g.S.coolingItems.Add(new CoolingItem{uid="cool_1",coolerId=1});g.S.coolingItems.Add(new CoolingItem{uid="cool_1",coolerId=2});File.WriteAllText(path,JsonUtility.ToJson(g.S));Check(!SaveStore.TryRead(path,out parsed),"Duplicate cooler identities must be rejected");
  foreach(var file in Directory.GetFiles(folder))File.Delete(file);Directory.Delete(folder);
  g.S=(SaveData)typeof(GameManager).GetMethod("Fresh",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(g,null);
  var market=host.AddComponent<MarketManager>();MarketManager.I=market;market.Init();g.S.cash=3000;g.BuyCooler(null,1);
  for(int second=0;second<14400;second++){
   g.Tick();if(second%5==0)market.MarketTick();if(second%60==0){g.Sell("btcx",g.Balance("btcx"));g.ClaimContract();}
   if(second%600==0){g.S=JsonUtility.FromJson<SaveData>(JsonUtility.ToJson(g.S));g.MigrateSave();}
  }
  Check(g.S.running&&g.S.runSeconds==14400&&g.Hottest()<100,"Four simulated hours with cooling and periodic reload remain playable");
  Check(!double.IsNaN(g.S.cash)&&g.S.cash>0&&g.S.contract!=null,"Long session retains valid economy and contracts");
  foreach(var series in g.S.markets)Check(series.candles.Count<=60,"Market history remains bounded");
  g.S.items[0].modelId=8;g.S.runSeconds=20000;g.Rebirth();Check(!g.CoinUnlocked(Catalog.Coin("ethx")),"Rebirth starts a fresh coin-unlock cycle");
  UnityEngine.Object.DestroyImmediate(host);
  Debug.Log("RELEASE_AUDIT "+count+" checks, "+failures+" failures");if(failures>0)throw new Exception("Release audit found "+failures+" problems");
 }
}
