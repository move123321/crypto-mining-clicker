using System;
using System.Reflection;
using CryptoMining;
using UnityEngine;
public static class GameplayChecks {
 static int checks;static readonly BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
 static void Check(bool value,string message){if(!value)throw new Exception(message);checks++;}
 static SaveData Fresh(GameManager g){return (SaveData)typeof(GameManager).GetMethod("Fresh",Private).Invoke(g,null);}
 static void Call(GameManager g,string name,params object[] args){typeof(GameManager).GetMethod(name,Private).Invoke(g,args);}
 public static void Run(){
  UnityEngine.Random.InitState(42);
  var host=new GameObject("Isolated gameplay checks");var g=host.AddComponent<GameManager>();GameManager.I=g;g.SaveEnabled=false;g.S=Fresh(g);
  for(int i=0;i<600;i++)g.Tick();Check(Mathf.Abs(g.At(0).temp-70)<.02f,"Basic GPU must reach 70C after 600 seconds");
  Check(g.S.runSeconds==600,"600 active seconds");Check(g.ElectricityPerMinute()==12,"Basic electricity rate");
  float heat=g.At(0).temp;Call(g,"OnApplicationPause",true);g.Tick();Check(g.At(0).temp==heat&&g.S.runSeconds==600,"Pause must freeze heat and simulation");
  Call(g,"OnApplicationPause",false);Call(g,"OnApplicationFocus",false);g.Tick();Check(g.S.runSeconds==600,"Unfocused simulation must stop");Call(g,"OnApplicationFocus",true);
  g.IntroPaused=true;g.Tick();Check(g.S.runSeconds==600,"Tutorial must stop simulation");g.IntroPaused=false;
  g.S=Fresh(g);g.S.cash=1000000;g.S.runSeconds=10000;
  for(int i=1;i<8;i++)g.S.items.Add(new GpuItem{uid="test_"+i,modelId=i,slot=i});
  Check(g.BuyCooler(null,1),"Buy first row cooler");for(int i=0;i<8;i++)Check(g.At(i).coolerId==(i<2?1:0),"Level 1 coverage slot "+i);
  Check(g.BuyCooler(null,3),"Buy three row cooler");for(int i=0;i<8;i++)Check(g.At(i).coolerId==(i<6?3:0),"Level 3 coverage slot "+i);
  g.Unequip("gpu_1");Check(g.Find("gpu_1").coolerId==0,"Stored cards are not cooled by rack");g.Equip("gpu_1",7);Check(g.Find("gpu_1").coolerId==0,"Uncovered row remains uncooled");
  Check(g.S.coolingItems.Count==2,"Replacing cooler preserves previous equipment");
  g.S=Fresh(g);g.S.version=1;g.S.items[0].coolerId=2;g.MigrateSave();Check(g.S.coolingItems.Count==1&&g.RackCoolerLevel()==2,"Legacy per-card cooler migration");g.MigrateSave();Check(g.S.coolingItems.Count==1,"Migration is idempotent");
  g.S=Fresh(g);g.S.cash=1000;g.S.coins[0].amount=77;g.At(0).temp=100;g.Tick();Check(g.S.repairPending&&!g.S.running,"Overheat opens repair choice");
  Check(g.ResolveOverheat(true),"Paid repair works");Check(g.S.cash==500&&g.S.items.Count==1&&g.At(0).temp==30&&g.S.running,"Repair preserves equipment and charges exact amount");
  g.At(0).temp=100;g.Tick();g.S.cash=0;Check(!g.ResolveOverheat(true)&&g.S.repairPending,"Insufficient cash cannot bypass repair");
  double balance=g.Balance("btcx");Check(g.ResolveOverheat(false),"Abandon repair works");Check(g.S.items.Count==0&&g.Balance("btcx")==balance&&g.S.running,"Only final GPU is deleted, progress preserved");
  g.S=JsonUtility.FromJson<SaveData>(JsonUtility.ToJson(g.S));g.MigrateSave();Check(g.S.items.Count==0&&g.Balance("btcx")==balance,"Empty equipment save survives migration");g.EmergencyStarter();Check(g.S.items.Count==1&&g.At(0)!=null,"Final-card loss has recovery route");g.EmergencyStarter();Check(g.S.items.Count==1,"No duplicate starter grant");
  g.S=Fresh(g);g.S.items.Add(new GpuItem{uid="second",modelId=1,slot=1});g.S.items.Add(new GpuItem{uid="stored",modelId=2,slot=-1});g.S.repairPending=true;g.S.running=false;g.ResolveOverheat(false);Check(g.S.items.Count==2&&g.Find("stored")!=null&&g.Equipped().Count==1,"Failure removes exactly one installed card");
  g.S=Fresh(g);var market=host.AddComponent<MarketManager>();MarketManager.I=market;market.Init();
  for(int j=0;j<30;j++){double before=market.Price("meme404");market.MarketTick();Check(Math.Abs(market.Price("meme404")/before-1)<=.012,"Ordinary volatility is bounded");}
  int baseline=g.ElectricityPerMinute();market.PublishEvent(1);Check(g.ElectricityPerMinute()!=baseline,"Electricity news affects actual bills");g.S.runSeconds=g.S.electricityOfferUntil;Check(g.ElectricityPerMinute()==baseline,"Electricity offer expires");
  market.PublishEvent(2);Check(g.GpuPrice(0)!=Catalog.GPU(0).price,"GPU news affects actual prices");g.S.runSeconds=g.S.gpuOfferUntil;Check(g.GpuPrice(0)==Catalog.GPU(0).price,"GPU offer expires");
  double price=market.Price("btcx");market.PublishEvent(0);Check(Math.Abs(market.Price("btcx")/price-1)<=.081,"Coin news impact bounded");
  Check(!g.CanRebirth(),"Incomplete rebirth blocked");g.Rebirth();Check(g.S.rebirths==0,"Blocked rebirth preserves progress");g.S.items[0].modelId=8;g.S.runSeconds=g.RebirthSeconds();Check(g.CanRebirth(),"Valid rebirth available");g.Rebirth();Check(g.S.rebirths==1&&g.S.fork==1,"Successful rebirth grants point");
  var clip=MiningMusic.CreateLoop();var samples=new float[clip.samples];clip.GetData(samples,0);float peak=0;foreach(var v in samples){if(float.IsNaN(v))throw new Exception("Invalid audio sample");peak=Mathf.Max(peak,Mathf.Abs(v));}Check(peak>.05f&&peak<1,"Music is audible and does not clip");UnityEngine.Object.DestroyImmediate(clip);
  Debug.Log("GAMEPLAY_CHECKS_PASSED: "+checks+" assertions; 600s thermal balance, focus, cooling rows, legacy saves, repairs, markets, rebirth, music");
  UnityEngine.Object.DestroyImmediate(host);
 }
}
