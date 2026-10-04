using System;
using System.Reflection;
using CryptoMining;
using UnityEngine;
public static class EstateChecks {
 static int count;static void Check(bool value,string message){if(!value)throw new Exception(message);count++;}
 public static void Run(){
  GameplayChecks.Run();var host=new GameObject("Estate checks");var g=host.AddComponent<GameManager>();GameManager.I=g;g.SaveEnabled=false;
  g.S=(SaveData)typeof(GameManager).GetMethod("Fresh",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(g,null);
  Check(g.SlotCapacity()==8&&g.PropertyRackLimit()==1,"New game starts with a small room and one rack");
  Check(!g.BuyStudio()&&g.S.cash==0,"Cannot purchase room without money");g.S.cash=100000;
  Check(!g.BuyRack()&&g.S.cash==100000,"Cannot install second rack before expansion");g.Equip("gpu_1",8);Check(g.At(0)!=null,"Locked slots reject equip");
  var gpu=g.At(0);gpu.temp=42;g.S.coolingItems.Add(new CoolingItem{uid="cool_1",coolerId=1});g.EquipCoolerToRack("cool_1",0);
  Check(g.BuyStudio()&&g.S.cash==70000&&g.SlotCapacity()==8,"Room purchase opens space, not free slots");
  Check(g.At(0)==gpu&&gpu.temp==42&&g.RackCoolerLevel(0)==1,"Moving preserves equipment temperature and cooling");
  Check(!g.BuyStudio()&&g.S.cash==70000,"Duplicate property charge rejected");
  Check(g.BuyRack()&&g.S.cash==55000&&g.SlotCapacity()==16,"Rack purchase unlocks sixteen slots");
  Check(!g.BuyRack()&&g.S.cash==55000,"Rack limit enforced");
  Check(g.BuyGpu(0,8)&&g.At(8)!=null,"Buy directly into second rack");g.S.coolUidCounter=2;
  Check(g.BuyCooler(null,1,1),"Purchase separate cooler for second rack");Check(g.RackCoolerLevel(0)==1&&g.RackCoolerLevel(1)==1,"Both racks can own separate same-tier coolers");
  g.EquipCoolerToRack("cool_1",1);Check(g.RackCoolerLevel(0)==0&&g.RackCoolerLevel(1)==1,"Moving cooler removes it from old rack");Check(g.CoolerRack("cool_2")==-1,"Replaced cooler returns to storage");
  g.Equip("gpu_1",15);Check(g.At(15)==gpu&&gpu.coolerId==0,"Level one does not cool row four of second rack");g.Equip("gpu_1",9);Check(gpu.coolerId==1,"Second rack first row is cooled");
  g.S=JsonUtility.FromJson<SaveData>(JsonUtility.ToJson(g.S));g.MigrateSave();Check(g.S.propertyId==1&&g.S.racksInstalled==2&&g.At(9)!=null&&g.RackCoolerLevel(1)==1,"New save roundtrip preserves full expansion");
  g.S.version=2;g.S.activeCoolingUid="cool_1";g.S.items.RemoveAll(x=>x.uid!="gpu_1");g.S.items[0].slot=0;g.MigrateSave();Check(g.S.racksInstalled==1&&g.S.propertyId==0&&g.RackCoolerLevel(0)==1&&g.At(0)!=null,"Version two migrates unchanged into first rack");
  g.S.cash=100000;g.BuyStudio();g.BuyRack();g.S.runSeconds=20000;g.S.totalMined=10000000;g.S.items[0].modelId=8;g.Rebirth();Check(g.S.propertyId==1&&g.S.racksInstalled==2&&g.S.items.Count==1&&g.S.coolingItems.Count==0,"Rebirth keeps property and racks, resets equipment");
  g.S.items.Add(new GpuItem{uid="second",modelId=0,slot=8,temp=30});g.Tick();Check(Mathf.Abs(g.At(0).temp-g.At(8).temp)<.0001f,"Separate one-card racks have equal independent heat");
  Check(g.TotalPower()==360&&g.Equipped().Count==2,"Second rack contributes power and mining");
  Debug.Log("ESTATE_CHECKS_PASSED "+count+" expansion, slot, cooling, migration and rebirth checks");UnityEngine.Object.DestroyImmediate(host);
 }
}
