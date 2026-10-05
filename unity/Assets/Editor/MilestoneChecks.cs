using System;
using System.Reflection;
using CryptoMining;
using UnityEngine;
public static class MilestoneChecks {
 static void Check(bool value,string message){if(!value)throw new Exception(message);}
 public static void Run(){
  ReleaseChecks.Run();
  var host=new GameObject("Milestone checks");var g=host.AddComponent<GameManager>();GameManager.I=g;g.SaveEnabled=false;
  g.S=(SaveData)typeof(GameManager).GetMethod("Fresh",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(g,null);
  g.S.cash=100000;Check(g.BuyStudio(),"Studio purchase succeeds");g.S.seenMilestones=1;
  g.S.runSeconds=20000;g.S.items[0].modelId=8;g.Rebirth();
  Check(g.S.rebirths==1&&g.S.fork==1&&g.S.seenMilestones==1,"Rebirth retains first-view history and awards fork");
  g.S.seenMilestones|=2;g.S=JsonUtility.FromJson<SaveData>(JsonUtility.ToJson(g.S));g.MigrateSave();
  Check(g.S.seenMilestones==3,"Both first-view flags survive save reload");
  g.Restart();Check(g.S.seenMilestones==3,"Restart does not replay milestones");
  var seconds=g.S.runSeconds;var heat=g.At(0).temp;var coins=g.Balance("btcx");g.IntroPaused=true;
  for(int i=0;i<6;i++)g.Tick();Check(g.S.runSeconds==seconds&&g.At(0).temp==heat&&g.Balance("btcx")==coins,"Cinematic pauses time, heat and mining");
  g.IntroPaused=false;g.Tick();Check(g.S.runSeconds==seconds+1,"Simulation resumes after cinematic");
  UnityEngine.Object.DestroyImmediate(host);Debug.Log("MILESTONE_CHECKS_PASSED 6 persistence, rewards and pause checks");
 }
}
