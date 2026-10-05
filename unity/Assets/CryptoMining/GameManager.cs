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
        public int version=3,rebirths,fork,runSeconds,electricityTimer,contractsCompleted,uidCounter=2;
        public int seenMilestones;
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
        public string activeCoolingUid; // Version 2 migration field.
        public int propertyId, racksInstalled=1;
        public List<string> rackCoolingUids=new List<string>{null,null};
        public bool repairPending;
        public float electricityMultiplier=1, gpuPriceMultiplier=1;
        public int electricityOfferUntil, gpuOfferUntil;
        public int emergencyCoolUntil;
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
            new CoolerDef(1,"트리플팬 공랭",.35f,3000,0,"1행 · 최대 GPU 2개 냉각"),
            new CoolerDef(2,"베이퍼 챔버",.75f,25000,1800,"2행 · 최대 GPU 4개 냉각"),
            new CoolerDef(3,"일체형 수냉",1.2f,180000,3600,"3행 · 최대 GPU 6개 냉각"),
            new CoolerDef(4,"전면 수냉",1.75f,1200000,5400,"4행 · 최대 GPU 8개 냉각"),
            new CoolerDef(5,"맞춤형 수냉",2.5f,6000000,7800,"4행 · 최대 GPU 8개 강화 냉각")
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
        public bool SaveBlocked;public string SaveNotice="";
        float secAcc,saveAcc;
        bool appPaused,appFocused=true;
        public bool SimulationActive => S!=null && S.running && !SaveBlocked && !IntroPaused && !appPaused && appFocused;
        string SavePath { get { return Path.Combine(Application.persistentDataPath,"crypto_mining_unity6_web_v1.json"); } }
        static readonly CultureInfo Inv=CultureInfo.InvariantCulture;

        void Awake(){ I=this; Load(); }
        void Update()
        {
            saveAcc+=Time.unscaledDeltaTime;
            if(saveAcc>=3){saveAcc=0;Save();}
            if(!SimulationActive){secAcc=0;return;}
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
            S=SaveStore.Load(SavePath,out SaveNotice,out SaveBlocked);
            if(S==null||S.items==null)S=Fresh();
            if(S.coolingItems==null)S.coolingItems=new List<CoolingItem>();
            if(S.markets==null)S.markets=new List<MarketSeries>();
            if(S.coins==null)S.coins=new List<CoinBalance>();
            for(int i=0;i<Catalog.Coins.Length;i++) if(BalanceObj(Catalog.Coins[i].id)==null)S.coins.Add(new CoinBalance{id=Catalog.Coins[i].id});
            if(S.forkUnlocked==null)S.forkUnlocked=new List<string>();
            if(!S.forkUnlocked.Contains("root"))S.forkUnlocked.Add("root");
            if(!CoinUnlocked(Catalog.Coin(S.miningCoinId)))S.miningCoinId="btcx";
            if(!CoinUnlocked(Catalog.Coin(S.tradeCoinId)))S.tradeCoinId="btcx";
            MigrateSave();
            EnsureContract();
        }

        public void Save()
        {
            if(S==null||!SaveEnabled||SaveBlocked)return;
            try{SaveStore.Write(SavePath,S);}
            catch(Exception e){SaveNotice="저장에 실패했습니다. 저장 공간을 확인하세요.";Debug.LogWarning("Save failed: "+e.Message);}
        }

        void OnApplicationQuit(){Save();}
        void OnApplicationPause(bool p){appPaused=p;secAcc=0;if(p)Save();}
        void OnApplicationFocus(bool focus){appFocused=focus;secAcc=0;if(!focus)Save();}

        public void Tick()
        {
            if(!SimulationActive)return;
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
                float heat=(40f/600f)*(1+m.id*.12f)*HeatMul(g); if(S.overclock)heat*=1.3f;
                g.temp=Mathf.Max(25,g.temp+heat-Cooling(g));
            }
            for(int i=0;i<S.items.Count;i++)if(S.items[i].slot<0)S.items[i].temp=Mathf.Max(25,S.items[i].temp-.35f);
            FeverTick(); CheckHeat(); Fire();
        }

        void FeverTick()
        {
            if(S.fever){S.feverTime--;if(S.feverTime<=0){S.fever=false;S.feverCooldown=35;Say("채굴 집중 종료");}}
            else{S.feverCooldown--;if(S.feverCooldown<=0){S.fever=true;S.feverTime=HasFork("fever")?25:20;Say(HasFork("fever")?"채굴 집중 x3":"채굴 집중 x2");}}
        }

        public double ClickMine(string uid)
        {
            GpuItem clicked=Find(uid); if(clicked==null||clicked.slot<0||!SimulationActive)return 0;
            CoinDef coin=MiningCoin(); double total=0,work=0; List<GpuItem> eq=Equipped();
            for(int i=0;i<eq.Count;i++)
            {
                GpuItem g=eq[i];GPUDef m=Catalog.GPU(g.modelId);double b=m.click*ClickMul(),gain=Math.Floor(b*coin.mineRate);total+=gain;work+=b;
                float heat=(.12f+m.id*.025f)*HeatMul(g);if(S.overclock)heat*=1.3f;g.temp+=heat;
            }
            AddCoin(coin.id,total);S.totalMined+=work;S.totalClicks++;ProgressContract("mine",coin.id,total);CheckHeat();Fire();return total;
        }

        public bool BuyGpu(int id,int slot=-1)
        {
            GPUDef m=Catalog.GPU(id);if(!Certified(m)){Say("GPU 인증 조건 미달");return false;}if(S.cash<GpuPrice(id)){Say("현금 부족");return false;}
            S.cash-=GpuPrice(id);GpuItem g=new GpuItem{uid="gpu_"+S.uidCounter++,modelId=id,temp=30,coolerId=0,slot=-1};S.items.Add(g);
            if(slot>=0&&slot<SlotCapacity()&&At(slot)==null)g.slot=slot;SyncCoolers();Say(m.name+" 구매 완료");Save();Fire();return true;
        }

        public bool UpgradePrimary(int rack=0)
        {
            if(rack<0||rack>=S.racksInstalled)return false;GpuItem g=At(rack*8);if(g==null){Say("1번 슬롯에 GPU 필요");return false;}if(g.modelId>=Catalog.GPUs.Length-1){Say("최고 등급");return false;}
            GPUDef cur=Catalog.GPU(g.modelId),next=Catalog.GPU(g.modelId+1);if(!Certified(next)){Say("다음 GPU 인증 잠김");return false;}if(S.cash<cur.upgradeCost){Say("현금 부족");return false;}
            S.cash-=cur.upgradeCost;g.modelId++;g.temp=Mathf.Max(30,g.temp-8);Say(next.name+" 업그레이드");Save();Fire();return true;
        }

        public void Equip(string uid,int slot){GpuItem g=Find(uid);if(g==null||slot<0||slot>=SlotCapacity())return;GpuItem old=At(slot);if(old!=null)old.slot=-1;g.slot=slot;SyncCoolers();Save();Fire();}
        public void Unequip(string uid){GpuItem g=Find(uid);if(g!=null){g.slot=-1;SyncCoolers();Save();Fire();}}

        public bool BuyCooler(string uid,int coolerId,int rack=0)
        {
            CoolerDef c=Catalog.Cooler(coolerId);if(rack<0||rack>=S.racksInstalled||!S.running||coolerId<=0||!CoolerUnlocked(c)||S.cash<c.cost){Say("현금 또는 냉각 인증 조건 부족");return false;}
            S.cash-=c.cost;var unit=new CoolingItem{uid="cool_"+S.coolUidCounter++,coolerId=coolerId};S.coolingItems.Add(unit);
            EquipCoolerToRack(unit.uid,rack);Say(c.name+" 구매 및 랙 장착 완료");return true;
        }
        public int SlotCapacity(){return S.racksInstalled*8;}
        public int PropertyRackLimit(){return S.propertyId==0?1:2;}
        public string PropertyName(){return S.propertyId==0?"작은 방":"원룸 작업실";}
        public const int StudioCost=30000, ExtraRackCost=15000;
        public bool BuyStudio(){
            if(!S.running||S.propertyId!=0||S.cash<StudioCost){Say("확장 비용이 부족하거나 이미 원룸 작업실입니다.");return false;}
            S.cash-=StudioCost;S.propertyId=1;Save();Fire();Say("원룸 작업실로 이사했습니다. 오른쪽 공간에 랙을 설치하세요.");return true;
        }
        public bool BuyRack(){
            if(!S.running||S.racksInstalled>=PropertyRackLimit()||S.cash<ExtraRackCost){Say("설치 공간 또는 랙 구매 비용이 부족합니다.");return false;}
            S.cash-=ExtraRackCost;S.racksInstalled++;SyncCoolers();Save();Fire();Say("두 번째 랙 설치 완료 · GPU와 쿨러를 장착하세요.");return true;
        }
        public void EquipCooler(string coolingUid,string gpuUid){var gpu=Find(gpuUid);EquipCoolerToRack(coolingUid,gpu!=null&&gpu.slot>=0?gpu.slot/8:0);}
        public void EquipCoolerToRack(string coolingUid,int rack){
            if(!S.running||rack<0||rack>=S.racksInstalled||!S.coolingItems.Exists(x=>x.uid==coolingUid))return;
            // A physical cooling unit can belong to only one rack.
            for(int i=0;i<S.rackCoolingUids.Count;i++)if(S.rackCoolingUids[i]==coolingUid)S.rackCoolingUids[i]=null;
            S.rackCoolingUids[rack]=coolingUid;SyncCoolers();Save();Fire();
        }
        public int CoolerRack(string uid){return string.IsNullOrEmpty(uid)?-1:S.rackCoolingUids.IndexOf(uid);}
        public void StockCooler(string uid){var gpu=Find(uid);RemoveRackCooler(gpu!=null&&gpu.slot>=0?gpu.slot/8:0);}
        public void RemoveRackCooler(int rack){if(rack<0||rack>=S.racksInstalled)return;S.rackCoolingUids[rack]=null;SyncCoolers();Save();Fire();}
        public int RackCoolerLevel(int rack=0){if(rack<0||rack>=S.racksInstalled)return 0;var c=S.coolingItems.Find(x=>x.uid==S.rackCoolingUids[rack]);return c==null?0:c.coolerId;}
        public int CoolingRows(int rack=0){return Mathf.Min(4,RackCoolerLevel(rack));}
        void SyncCoolers(){foreach(var gpu in S.items){int rack=gpu.slot/8;gpu.coolerId=gpu.slot>=0&&(gpu.slot%8)/2<CoolingRows(rack)?RackCoolerLevel(rack):0;}S.activeCoolingUid=S.rackCoolingUids[0];}
        public void MigrateSave(){
            if(S.version<2){
                foreach(var g in S.items)if(g.coolerId>0&&!S.coolingItems.Exists(x=>x.gpuUid==g.uid))
                    S.coolingItems.Add(new CoolingItem{uid="cool_"+S.coolUidCounter++,coolerId=g.coolerId,gpuUid=g.uid});
                CoolingItem best=null;
                foreach(var c in S.coolingItems)if(!string.IsNullOrEmpty(c.gpuUid)&&(best==null||c.coolerId>best.coolerId))best=c;
                S.activeCoolingUid=best==null?null:best.uid;
                S.electricityMultiplier=S.gpuPriceMultiplier=1;
            }
            if(S.rackCoolingUids==null)S.rackCoolingUids=new List<string>();
            while(S.rackCoolingUids.Count<2)S.rackCoolingUids.Add(null);
            if(S.version<3){S.propertyId=0;S.racksInstalled=1;S.rackCoolingUids[0]=S.activeCoolingUid;S.rackCoolingUids[1]=null;}
            S.propertyId=Mathf.Clamp(S.propertyId,0,1);S.racksInstalled=Mathf.Clamp(S.racksInstalled,1,PropertyRackLimit());
            for(int i=0;i<2;i++)if(i>=S.racksInstalled||!S.coolingItems.Exists(x=>x.uid==S.rackCoolingUids[i])||(i>0&&S.rackCoolingUids[i]==S.rackCoolingUids[0]))S.rackCoolingUids[i]=null;
            foreach(var item in S.items)if(item.slot>=SlotCapacity())item.slot=-1;
            S.version=3;S.repairPending=!S.running;
            foreach(var c in S.coolingItems)c.gpuUid=null;
            SyncCoolers();
        }
        public double GpuPrice(int id){return Math.Round(Catalog.GPU(id).price*(S.runSeconds<S.gpuOfferUntil?Mathf.Max(.5f,S.gpuPriceMultiplier):1));}
        public double RepairCost(){double value=0;foreach(var gpu in Equipped())value+=Catalog.GPU(gpu.modelId).price;return Math.Max(500,Math.Round(value*.08));}
        public bool ResolveOverheat(bool repair){
            if(!S.repairPending)return false;
            if(repair){if(S.cash<RepairCost()){Say("수리비가 부족합니다. 코인을 판매하거나 장비 포기를 선택하세요.");return false;}S.cash-=RepairCost();}
            else {var cards=Equipped();if(cards.Count>0){var lost=cards[UnityEngine.Random.Range(0,cards.Count)];S.items.Remove(lost);Say(Catalog.GPU(lost.modelId).name+" 고장으로 폐기");}}
            foreach(var gpu in S.items)gpu.temp=30;
            S.repairPending=false;S.running=true;S.overclock=false;secAcc=0;SyncCoolers();Save();Fire();return true;
        }
        public void EmergencyStarter(){
            if(!S.running||S.items.Count!=0)return;
            // A lost final card must not trap an otherwise valid save in a dead end.
            S.items.Add(new GpuItem{uid="gpu_"+S.uidCounter++,modelId=0,slot=0,temp=30});
            SyncCoolers();Say("재시작 지원 GPU를 받았습니다.");Save();Fire();
        }

        public void ToggleOC(){if(!SimulationActive)return;S.overclock=!S.overclock;if(S.overclock){List<GpuItem> eq=Equipped();for(int i=0;i<eq.Count;i++)eq[i].temp+=3*HeatMul(eq[i]);}Say(S.overclock?"오버클럭 켜짐":"오버클럭 꺼짐");CheckHeat();Fire();}
        public int EmergencyCoolRemaining(){return Mathf.Max(0,S.emergencyCoolUntil-S.runSeconds);}
        public void EmergencyCool(){if(!SimulationActive||EmergencyCoolRemaining()>0)return;S.emergencyCoolUntil=S.runSeconds+120;List<GpuItem> eq=Equipped();float a=20+(HasFork("cooling")?5:0);for(int i=0;i<eq.Count;i++)eq[i].temp=Mathf.Max(25,eq[i].temp-a);Say("긴급 냉각 -"+a+"°C · 재사용 120초");Save();Fire();}


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
            if(HasFork(id)||S.fork<1||(!string.IsNullOrEmpty(req)&&!HasFork(req)))return false;S.fork--;S.forkUnlocked.Add(id);Save();Fire();Say("포크 업그레이드 해금");return true;
        }

        public int RebirthReq(){int[] a={1,2,3,4,6,8};return a[Mathf.Min(S.rebirths,a.Length-1)];}
        public int QuantumCount(){int n=0;for(int i=0;i<S.items.Count;i++)if(S.items[i].slot>=0&&S.items[i].modelId==8)n++;return n;}
        public int RebirthSeconds(){return RequiredSeconds(Catalog.GPU(8));}
        public bool CanRebirth(){return S.running&&QuantumCount()>=RebirthReq()&&S.runSeconds>=RebirthSeconds();}
        public void Rebirth()
        {
            if(!CanRebirth())return;int r=S.rebirths+1,f=S.fork+1;List<string> forks=new List<string>(S.forkUnlocked);var marketHistory=S.markets;int property=S.propertyId,racks=S.racksInstalled,seen=S.seenMilestones;S=Fresh();S.seenMilestones=seen;S.propertyId=property;S.racksInstalled=racks;S.markets=marketHistory;S.rebirths=r;S.fork=f;S.forkUnlocked=forks;EnsureContract();Save();Fire();Say("환생 완료 · 포크 +1");
        }
        public void Restart(){int r=S.rebirths,f=S.fork;List<string> forks=new List<string>(S.forkUnlocked);var marketHistory=S.markets;int property=S.propertyId,racks=S.racksInstalled,seen=S.seenMilestones;S=Fresh();S.seenMilestones=seen;S.propertyId=property;S.racksInstalled=racks;S.markets=marketHistory;S.rebirths=r;S.fork=f;S.forkUnlocked=forks;EnsureContract();Save();Fire();}

        public List<GpuItem> Equipped(){List<GpuItem> l=new List<GpuItem>();for(int s=0;s<SlotCapacity();s++){GpuItem g=At(s);if(g!=null)l.Add(g);}return l;}
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
        float HeatMul(GpuItem gpu){int count=0;foreach(var item in S.items)if(item.slot>=0&&item.slot/8==gpu.slot/8)count++;int e=Mathf.Max(0,count-1);return (1+e*.08f+e*e*.01f)*(HasFork("cooling")?.8f:1);}
        float Cooling(GpuItem g){return Catalog.Cooler(g.coolerId).rate*(HasFork("cooling")?1.15f:1);}

        public double TotalAuto(){double n=0;List<GpuItem> l=Equipped();for(int i=0;i<l.Count;i++)n+=Catalog.GPU(l[i].modelId).autoMine;return n*AutoMul()*MiningCoin().mineRate;}
        public double TotalHash(){double n=0;List<GpuItem> l=Equipped();for(int i=0;i<l.Count;i++)n+=Catalog.GPU(l[i].modelId).hash;return n*AutoMul();}
        public int TotalPower(){int n=0;List<GpuItem> l=Equipped();for(int i=0;i<l.Count;i++)n+=Catalog.GPU(l[i].modelId).power;return n;}
        public int ElectricityPerMinute(){return Mathf.Max(0,Mathf.RoundToInt(TotalPower()/15f*(S.runSeconds<S.electricityOfferUntil?Mathf.Max(.5f,S.electricityMultiplier):1)));}
        public float Hottest(){float h=25;List<GpuItem> l=Equipped();for(int i=0;i<l.Count;i++)h=Mathf.Max(h,l[i].temp);return h;}
        public string Duration(int s){int h=s/3600,m=(s%3600)/60;return h>0?h+"시간 "+m+"분":m+"분";}

        void CheckHeat(){if(Hottest()>=100&&S.running){S.running=false;S.repairPending=true;Save();if(Overheated!=null)Overheated();}}
        void Say(string m){if(Toast!=null)Toast(m);}
        void Fire(){if(Changed!=null)Changed();}
    }

}
