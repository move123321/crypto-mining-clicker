using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CryptoMining
{
    [Serializable]
    public class GPUDef
    {
        public int id; public string name; public float click; public float autoMine; public float hash; public int power;
        public float price; public float upgradeCost; public int unlockSeconds; public float unlockMined;
        public GPUDef(int id,string name,float click,float autoMine,float hash,int power,float price,float upgradeCost,int unlockSeconds,float unlockMined)
        { this.id=id; this.name=name; this.click=click; this.autoMine=autoMine; this.hash=hash; this.power=power; this.price=price; this.upgradeCost=upgradeCost; this.unlockSeconds=unlockSeconds; this.unlockMined=unlockMined; }
    }

    [Serializable]
    public class CoinDef
    {
        public string id,name,icon,desc; public float basePrice,vol,mineRate,eventScale; public int unlockGpu;
        public CoinDef(string id,string name,string icon,float basePrice,float vol,float mineRate,int unlockGpu,float eventScale,string desc)
        { this.id=id; this.name=name; this.icon=icon; this.basePrice=basePrice; this.vol=vol; this.mineRate=mineRate; this.unlockGpu=unlockGpu; this.eventScale=eventScale; this.desc=desc; }
    }

    [Serializable]
    public class CoolerDef
    {
        public int id; public string name,desc; public float rate,cost; public int unlockSeconds;
        public CoolerDef(int id,string name,float rate,float cost,int unlockSeconds,string desc)
        { this.id=id; this.name=name; this.rate=rate; this.cost=cost; this.unlockSeconds=unlockSeconds; this.desc=desc; }
    }

    [Serializable]
    public class CoolingItem { public string uid,gpuUid; public int coolerId; }
    [Serializable]
    public class GpuItem { public string uid; public int modelId; public float temp=30; public int coolerId; public int slot=-1; }
    [Serializable]
    public class CoinBalance { public string id; public double amount; }
    [Serializable]
    public class ContractState { public string type,coinId; public double target,progress,reward; }

    [Serializable]
    public class SaveData
    {
        public int version=1,rebirths,fork,runSeconds,electricityTimer,contractsCompleted,uidCounter=2;
        public long totalClicks;
        public double cash,totalMined;
        public bool running=true,overclock,fever;
        public int feverTime,feverCooldown=20;
        public string miningCoinId="btcx",tradeCoinId="btcx";
        public List<GpuItem> items=new List<GpuItem>();
        public List<CoinBalance> coins=new List<CoinBalance>();
        public List<string> forkUnlocked=new List<string>();
        public ContractState contract;
        public List<CoolingItem> coolingItems=new List<CoolingItem>();
        public List<MarketSeries> markets=new List<MarketSeries>();
        public int coolUidCounter=1;
    }

    public static class Catalog
    {
        public static readonly GPUDef[] GPUs = {
            new GPUDef(0,"RTX 1090",10,1,8,180,5000,15000,0,0),
            new GPUDef(1,"RTX 2090",18,2.5f,17,235,25000,50000,600,20000),
            new GPUDef(2,"RTX 3090",32,6,35,330,85000,130000,1800,80000),
            new GPUDef(3,"RTX 4090",58,14,78,480,220000,300000,3600,250000),
            new GPUDef(4,"RTX 5090",105,32,150,650,500000,600000,5400,600000),
            new GPUDef(5,"RTX 5090 Ti",160,55,225,760,900000,1000000,7200,1200000),
            new GPUDef(6,"RTX 5090 TITAN",240,90,310,840,1500000,1500000,8400,2200000),
            new GPUDef(7,"RTX 6090 PROTOTYPE",360,145,400,920,2500000,2000000,9600,3800000),
            new GPUDef(8,"RTX 6090 QUANTUM",520,230,540,1020,4000000,0,10500,6000000)
        };

        public static readonly CoinDef[] Coins = {
            new CoinDef("btcx","BTC-X","B",1f,.025f,1f,0,1f,"안정적인 기본 코인"),
            new CoinDef("ethx","ETHER-X","E",.82f,.04f,1.2f,1,1f,"RTX 2090 보유 시 해금"),
            new CoinDef("dogex","DOGE-X","D",.35f,.07f,2.7f,2,1.15f,"RTX 3090 보유 시 해금"),
            new CoinDef("nova","NOVA","N",1.35f,.05f,.75f,3,1.05f,"RTX 4090 보유 시 해금"),
            new CoinDef("meme404","MEME-404","404",.18f,.13f,5.2f,4,1.35f,"RTX 5090 보유 시 해금"),
            new CoinDef("qbit","QBIT","Q",2.4f,.035f,.42f,5,.85f,"RTX 5090 Ti 보유 시 해금")
        };

        public static readonly CoolerDef[] Coolers = {
            new CoolerDef(0,"기본 듀얼팬",0,0,0,"GPU 기본 쿨러"),
            new CoolerDef(1,"트리플팬 공랭",.35f,3000,0,"3팬 GPU 공랭"),
            new CoolerDef(2,"베이퍼 챔버",.75f,25000,1800,"고급 공랭"),
            new CoolerDef(3,"GPU HYBRID AIO",1.2f,180000,3600,"GPU 일체형 수냉"),
            new CoolerDef(4,"FULL COVER 수냉",1.75f,1200000,5400,"코어/VRAM/VRM 수냉"),
            new CoolerDef(5,"CUSTOM LOOP GPU",2.5f,6000000,7800,"최상급 GPU 커스텀 수냉")
        };

        public static GPUDef GPU(int id){ return id>=0&&id<GPUs.Length?GPUs[id]:GPUs[0]; }
        public static CoinDef Coin(string id){ for(int i=0;i<Coins.Length;i++) if(Coins[i].id==id) return Coins[i]; return Coins[0]; }
        public static CoolerDef Cooler(int id){ return id>=0&&id<Coolers.Length?Coolers[id]:Coolers[0]; }
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager I;
        public SaveData S;
        public bool IntroPaused;
        public event Action Changed;
        public event Action<string> Toast;
        public event Action Overheated;

        public bool SaveEnabled=true;
        float secAcc,saveAcc;
        string SavePath { get { return Path.Combine(Application.persistentDataPath,"crypto_mining_unity6_web_v1.json"); } }
        static readonly CultureInfo Inv=CultureInfo.InvariantCulture;

        void Awake(){ I=this; Load(); }
        void Update()
        {
            saveAcc+=Time.unscaledDeltaTime;
            if(saveAcc>=3){saveAcc=0;Save();}
            if(!S.running||IntroPaused)return;
            secAcc+=Time.unscaledDeltaTime;
            while(secAcc>=1){secAcc-=1;Tick();}
        }

        SaveData Fresh()
        {
            SaveData s=new SaveData();
            s.items.Add(new GpuItem{uid="gpu_1",modelId=0,temp=30,coolerId=0,slot=0});
            for(int i=0;i<Catalog.Coins.Length;i++) s.coins.Add(new CoinBalance{id=Catalog.Coins[i].id,amount=0});
            s.forkUnlocked.Add("root");
            return s;
        }

        public void Load()
        {
            try
            {
                if(File.Exists(SavePath)) S=JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath));
            }
            catch(Exception e)
            {
                Debug.LogWarning("Save load failed: "+e.Message);
                S=null;
            }

            if(S==null||S.items==null||S.items.Count==0)S=Fresh();
            if(S.coolingItems==null)S.coolingItems=new List<CoolingItem>();
            if(S.markets==null)S.markets=new List<MarketSeries>();
            if(S.coins==null)S.coins=new List<CoinBalance>();
            for(int i=0;i<Catalog.Coins.Length;i++) if(BalanceObj(Catalog.Coins[i].id)==null)S.coins.Add(new CoinBalance{id=Catalog.Coins[i].id});
            if(S.forkUnlocked==null)S.forkUnlocked=new List<string>();
            if(!S.forkUnlocked.Contains("root"))S.forkUnlocked.Add("root");
            if(!CoinUnlocked(Catalog.Coin(S.miningCoinId)))S.miningCoinId="btcx";
            if(!CoinUnlocked(Catalog.Coin(S.tradeCoinId)))S.tradeCoinId="btcx";
            EnsureContract();
        }

        public void Save()
        {
            if(S==null||!SaveEnabled)return;
            try{Directory.CreateDirectory(Application.persistentDataPath);string tmp=SavePath+".tmp";File.WriteAllText(tmp,JsonUtility.ToJson(S));if(File.Exists(SavePath))File.Copy(SavePath,SavePath+".bak",true);File.Copy(tmp,SavePath,true);File.Delete(tmp);}
            catch(Exception e){Debug.LogWarning("Save failed: "+e.Message);}
        }

        void OnApplicationQuit(){Save();}
        void OnApplicationPause(bool p){if(p)Save();}

        public void Tick()
        {
            S.runSeconds++;
            S.electricityTimer++;
            if(S.electricityTimer>=60){S.electricityTimer=0;S.cash=Math.Max(0,S.cash-ElectricityPerMinute());}

            CoinDef coin=MiningCoin();
            List<GpuItem> eq=Equipped();
            for(int i=0;i<eq.Count;i++)
            {
                GpuItem g=eq[i]; GPUDef m=Catalog.GPU(g.modelId);
                double baseGain=m.autoMine*AutoMul(),gain=baseGain*coin.mineRate;
                AddCoin(coin.id,gain); S.totalMined+=baseGain; ProgressContract("mine",coin.id,gain);
                float heat=(.32f+m.id*.08f)*HeatMul(); if(S.overclock)heat*=1.3f;
                g.temp=Mathf.Max(25,g.temp+heat-Cooling(g));
            }
            for(int i=0;i<S.items.Count;i++)if(S.items[i].slot<0)S.items[i].temp=Mathf.Max(25,S.items[i].temp-.35f);
            FeverTick(); CheckHeat(); Fire();
        }

        void FeverTick()
        {
            if(S.fever){S.feverTime--;if(S.feverTime<=0){S.fever=false;S.feverCooldown=35;Say("MINING FEVER 종료");}}
            else{S.feverCooldown--;if(S.feverCooldown<=0){S.fever=true;S.feverTime=HasFork("fever")?25:20;Say(HasFork("fever")?"MINING FEVER x3":"MINING FEVER x2");}}
        }

        public double ClickMine(string uid)
        {
            GpuItem clicked=Find(uid); if(clicked==null||clicked.slot<0||!S.running||IntroPaused)return 0;
            CoinDef coin=MiningCoin(); double total=0,work=0; List<GpuItem> eq=Equipped();
            for(int i=0;i<eq.Count;i++)
            {
                GpuItem g=eq[i];GPUDef m=Catalog.GPU(g.modelId);double b=m.click*ClickMul(),gain=Math.Floor(b*coin.mineRate);total+=gain;work+=b;
                float heat=(.85f+m.id*.11f)*HeatMul();if(S.overclock)heat*=1.3f;g.temp+=heat;
            }
            AddCoin(coin.id,total);S.totalMined+=work;S.totalClicks++;ProgressContract("mine",coin.id,total);CheckHeat();Fire();return total;
        }

        public bool BuyGpu(int id,int slot=-1)
        {
            GPUDef m=Catalog.GPU(id);if(!Certified(m)){Say("GPU 인증 조건 미달");return false;}if(S.cash<m.price){Say("현금 부족");return false;}
            S.cash-=m.price;GpuItem g=new GpuItem{uid="gpu_"+S.uidCounter++,modelId=id,temp=30,coolerId=0,slot=-1};S.items.Add(g);
            if(slot>=0&&slot<8&&At(slot)==null)g.slot=slot;Say(m.name+" 구매 완료");Save();Fire();return true;
        }

        public bool UpgradePrimary()
        {
            GpuItem g=At(0);if(g==null){Say("1번 슬롯에 GPU 필요");return false;}if(g.modelId>=Catalog.GPUs.Length-1){Say("최고 등급");return false;}
            GPUDef cur=Catalog.GPU(g.modelId),next=Catalog.GPU(g.modelId+1);if(!Certified(next)){Say("다음 GPU 인증 잠김");return false;}if(S.cash<cur.upgradeCost){Say("현금 부족");return false;}
            S.cash-=cur.upgradeCost;g.modelId++;g.temp=Mathf.Max(30,g.temp-8);Say(next.name+" 업그레이드");Save();Fire();return true;
        }

        public void Equip(string uid,int slot){GpuItem g=Find(uid);if(g==null||slot<0||slot>7)return;GpuItem old=At(slot);if(old!=null)old.slot=-1;g.slot=slot;Save();Fire();}
        public void Unequip(string uid){GpuItem g=Find(uid);if(g!=null){g.slot=-1;Save();Fire();}}

        public bool BuyCooler(string uid,int coolerId)
        {
            CoolerDef c=Catalog.Cooler(coolerId);if(!S.running||coolerId<=0||!CoolerUnlocked(c)||S.cash<c.cost){Say("현금 또는 냉각 인증 조건 부족");return false;}
            S.cash-=c.cost;var unit=new CoolingItem{uid="cool_"+S.coolUidCounter++,coolerId=coolerId};S.coolingItems.Add(unit);
            var g=Find(uid);if(g!=null&&g.slot>=0)EquipCooler(unit.uid,uid);Say(c.name+" 구매 완료");Save();Fire();return true;
        }
        public void EquipCooler(string coolingUid,string gpuUid)
        {
            var g=Find(gpuUid);var c=S.coolingItems.Find(x=>x.uid==coolingUid);if(g==null||g.slot<0||c==null)return;
            foreach(var x in S.coolingItems)if(x.gpuUid==gpuUid)x.gpuUid=null;
            c.gpuUid=gpuUid;SyncCoolers();Save();Fire();
        }
        public void StockCooler(string uid){foreach(var c in S.coolingItems)if(c.gpuUid==uid)c.gpuUid=null;SyncCoolers();Save();Fire();}
        void SyncCoolers(){foreach(var g in S.items){var c=S.coolingItems.Find(x=>x.gpuUid==g.uid);g.coolerId=c==null?0:c.coolerId;}}

        public void ToggleOC(){S.overclock=!S.overclock;if(S.overclock){List<GpuItem> eq=Equipped();for(int i=0;i<eq.Count;i++)eq[i].temp+=3*HeatMul();}Say(S.overclock?"OC ON":"OC OFF");CheckHeat();Fire();}
        public void EmergencyCool(){List<GpuItem> eq=Equipped();float a=UnityEngine.Random.Range(16,24)+(HasFork("cooling")?5:0);for(int i=0;i<eq.Count;i++)eq[i].temp=Mathf.Max(25,eq[i].temp-a);Say("긴급 냉각 -"+Mathf.RoundToInt(a)+"C");Fire();}

        public void SetMiningCoin(string id){CoinDef c=Catalog.Coin(id);if(!CoinUnlocked(c)){Say("상위 GPU 필요");return;}S.miningCoinId=id;Save();Fire();}
        public void SetTradeCoin(string id){CoinDef c=Catalog.Coin(id);if(CoinUnlocked(c)){S.tradeCoinId=id;Fire();}}

        public bool Sell(string id,double amount)
        {
            double bal=Balance(id);if(double.IsNaN(amount)||double.IsInfinity(amount)||amount<=0||amount>bal||MarketManager.I==null)return false;
            SetBalance(id,bal-amount);ProgressContract("sell",id,amount);S.cash+=amount*MarketManager.I.Price(id);Say(Catalog.Coin(id).name+" 판매");Save();Fire();return true;
        }

        public void EnsureContract()
        {
            if(S.contract!=null)return;List<CoinDef> pool=UnlockedCoins();CoinDef coin=pool[UnityEngine.Random.Range(0,pool.Count)];
            string type=UnityEngine.Random.value<.62f?"mine":"sell";int tier=Mathf.Max(0,RigTier());double scale=Math.Max(1,(tier+1)*(tier+1));
            S.contract=new ContractState{type=type,coinId=coin.id,target=Math.Round((type=="mine"?900:450)*scale*(1+Math.Min(S.contractsCompleted,12)*.08)),reward=Math.Round(3500*scale*(1+Math.Min(S.contractsCompleted,12)*.10)),progress=0};
        }
        void ProgressContract(string type,string id,double amount){EnsureContract();if(S.contract.type==type&&S.contract.coinId==id&&S.contract.progress<S.contract.target){S.contract.progress=Math.Min(S.contract.target,S.contract.progress+amount);if(S.contract.progress>=S.contract.target)Say("계약 완료");}}
        public void ClaimContract(){EnsureContract();if(S.contract.progress<S.contract.target)return;S.cash+=S.contract.reward;S.contractsCompleted++;S.contract=null;EnsureContract();Save();Fire();Say("계약 보상 수령");}

        public bool HasFork(string id){return S.forkUnlocked.Contains(id);}
        public bool BuyFork(string id,string req)
        {
            if(HasFork(id)||S.fork<1||(!string.IsNullOrEmpty(req)&&!HasFork(req)))return false;S.fork--;S.forkUnlocked.Add(id);Save();Fire();Say(id+" 해금");return true;
        }

        public int RebirthReq(){int[] a={1,2,3,4,6,8};return a[Mathf.Min(S.rebirths,a.Length-1)];}
        public int QuantumCount(){int n=0;for(int i=0;i<S.items.Count;i++)if(S.items[i].slot>=0&&S.items[i].modelId==8)n++;return n;}
        public int RebirthSeconds(){return RequiredSeconds(Catalog.GPU(8));}
        public bool CanRebirth(){return S.running&&QuantumCount()>=RebirthReq()&&S.runSeconds>=RebirthSeconds();}
        public void Rebirth()
        {
            if(!CanRebirth())return;int r=S.rebirths+1,f=S.fork+1;List<string> forks=new List<string>(S.forkUnlocked);var marketHistory=S.markets;S=Fresh();S.markets=marketHistory;S.rebirths=r;S.fork=f;S.forkUnlocked=forks;EnsureContract();Save();Fire();Say("환생 완료 FORK +1");
        }
        public void Restart(){int r=S.rebirths,f=S.fork;List<string> forks=new List<string>(S.forkUnlocked);var marketHistory=S.markets;S=Fresh();S.markets=marketHistory;S.rebirths=r;S.fork=f;S.forkUnlocked=forks;EnsureContract();Save();Fire();}

        public List<GpuItem> Equipped(){List<GpuItem> l=new List<GpuItem>();for(int s=0;s<8;s++){GpuItem g=At(s);if(g!=null)l.Add(g);}return l;}
        public GpuItem At(int slot){for(int i=0;i<S.items.Count;i++)if(S.items[i].slot==slot)return S.items[i];return null;}
        public GpuItem Find(string uid){for(int i=0;i<S.items.Count;i++)if(S.items[i].uid==uid)return S.items[i];return null;}

        CoinBalance BalanceObj(string id){for(int i=0;i<S.coins.Count;i++)if(S.coins[i].id==id)return S.coins[i];return null;}
        public double Balance(string id){CoinBalance b=BalanceObj(id);return b==null?0:Math.Max(0,b.amount);}
        void SetBalance(string id,double v){CoinBalance b=BalanceObj(id);if(b==null){b=new CoinBalance{id=id};S.coins.Add(b);}b.amount=Math.Max(0,v);}
        void AddCoin(string id,double v){SetBalance(id,Balance(id)+v);}

        public CoinDef MiningCoin(){return Catalog.Coin(S.miningCoinId);}
        public int HighestModel(){int h=0;for(int i=0;i<S.items.Count;i++)h=Mathf.Max(h,S.items[i].modelId);return h;}
        public bool CoinUnlocked(CoinDef c){return HighestModel()>=c.unlockGpu;}
        public List<CoinDef> UnlockedCoins(){List<CoinDef> l=new List<CoinDef>();for(int i=0;i<Catalog.Coins.Length;i++)if(CoinUnlocked(Catalog.Coins[i]))l.Add(Catalog.Coins[i]);return l;}

        public float CycleFactor(){return Mathf.Max(.55f,1-S.rebirths*.08f);}
        public int RequiredSeconds(GPUDef g){return Mathf.FloorToInt(g.unlockSeconds*CycleFactor());}
        public bool Certified(GPUDef g){return S.runSeconds>=RequiredSeconds(g)&&S.totalMined>=g.unlockMined;}
        public int RigTier(){int t=0;for(int i=1;i<Catalog.GPUs.Length;i++){if(Certified(Catalog.GPUs[i]))t=i;else break;}return t;}
        public bool CoolerUnlocked(CoolerDef c){return S.runSeconds>=Mathf.FloorToInt(c.unlockSeconds*CycleFactor());}

        public float ClickMul(){float m=HasFork("fork")?1.1f:1;if(HasFork("click"))m*=1.25f;if(S.overclock)m*=HasFork("oc")?1.5f:1.3f;if(S.fever)m*=HasFork("fever")?3:2;return m;}
        public float AutoMul(){float m=HasFork("fork")?1.1f:1;if(HasFork("auto"))m*=1.3f;if(S.overclock)m*=HasFork("oc")?1.5f:1.3f;if(S.fever)m*=HasFork("fever")?3:2;return m;}
        float HeatMul(){int e=Mathf.Max(0,Equipped().Count-1);return (1+e*.08f+e*e*.01f)*(HasFork("cooling")?.8f:1);}
        float Cooling(GpuItem g){return Catalog.Cooler(g.coolerId).rate*(HasFork("cooling")?1.15f:1);}

        public double TotalAuto(){double n=0;List<GpuItem> l=Equipped();for(int i=0;i<l.Count;i++)n+=Catalog.GPU(l[i].modelId).autoMine;return n*AutoMul()*MiningCoin().mineRate;}
        public double TotalHash(){double n=0;List<GpuItem> l=Equipped();for(int i=0;i<l.Count;i++)n+=Catalog.GPU(l[i].modelId).hash;return n*AutoMul();}
        public int TotalPower(){int n=0;List<GpuItem> l=Equipped();for(int i=0;i<l.Count;i++)n+=Catalog.GPU(l[i].modelId).power;return n;}
        public int ElectricityPerMinute(){return Mathf.Max(0,Mathf.RoundToInt(TotalPower()/42f));}
        public float Hottest(){float h=25;List<GpuItem> l=Equipped();for(int i=0;i<l.Count;i++)h=Mathf.Max(h,l[i].temp);return h;}
        public string Duration(int s){int h=s/3600,m=(s%3600)/60;return h>0?h+"시간 "+m+"분":m+"분";}

        void CheckHeat(){if(Hottest()>=100&&S.running){S.running=false;Save();if(Overheated!=null)Overheated();}}
        void Say(string m){if(Toast!=null)Toast(m);}
        void Fire(){if(Changed!=null)Changed();}
    }

}
