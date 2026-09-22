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

        float secAcc,saveAcc;
        string SavePath { get { return Path.Combine(Application.persistentDataPath,"crypto_mining_unity_save_v2.txt"); } }
        static readonly CultureInfo Inv=CultureInfo.InvariantCulture;

        void Awake(){ I=this; Load(); }
        void Update()
        {
            saveAcc+=Time.unscaledDeltaTime;
            if(saveAcc>=5){saveAcc=0;Save();}
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
                if(File.Exists(SavePath)) S=DeserializeState(File.ReadAllText(SavePath));
            }
            catch(Exception e)
            {
                Debug.LogWarning("Save load failed: "+e.Message);
                S=null;
            }

            if(S==null||S.items==null||S.items.Count==0)S=Fresh();
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
            if(S==null)return;
            try{File.WriteAllText(SavePath,SerializeState(S));}
            catch(Exception e){Debug.LogWarning("Save failed: "+e.Message);}
        }

        string SerializeState(SaveData s)
        {
            StringBuilder b=new StringBuilder();
            Action<string,string> line=(k,v)=>b.Append(k).Append('=').Append(v??"").Append('\n');
            line("version",s.version.ToString(Inv));
            line("rebirths",s.rebirths.ToString(Inv));
            line("fork",s.fork.ToString(Inv));
            line("runSeconds",s.runSeconds.ToString(Inv));
            line("electricityTimer",s.electricityTimer.ToString(Inv));
            line("contractsCompleted",s.contractsCompleted.ToString(Inv));
            line("uidCounter",s.uidCounter.ToString(Inv));
            line("totalClicks",s.totalClicks.ToString(Inv));
            line("cash",s.cash.ToString("R",Inv));
            line("totalMined",s.totalMined.ToString("R",Inv));
            line("running",s.running?"1":"0");
            line("overclock",s.overclock?"1":"0");
            line("fever",s.fever?"1":"0");
            line("feverTime",s.feverTime.ToString(Inv));
            line("feverCooldown",s.feverCooldown.ToString(Inv));
            line("miningCoinId",s.miningCoinId);
            line("tradeCoinId",s.tradeCoinId);

            List<string> items=new List<string>();
            for(int i=0;i<s.items.Count;i++)
            {
                GpuItem g=s.items[i];
                items.Add(g.uid+","+g.modelId.ToString(Inv)+","+g.temp.ToString("R",Inv)+","+g.coolerId.ToString(Inv)+","+g.slot.ToString(Inv));
            }
            line("items",string.Join(";",items.ToArray()));

            List<string> coins=new List<string>();
            for(int i=0;i<s.coins.Count;i++)
                coins.Add(s.coins[i].id+","+s.coins[i].amount.ToString("R",Inv));
            line("coins",string.Join(";",coins.ToArray()));
            line("forkUnlocked",string.Join(",",s.forkUnlocked.ToArray()));

            if(s.contract==null)line("contract","");
            else line("contract",s.contract.type+","+s.contract.coinId+","+s.contract.target.ToString("R",Inv)+","+s.contract.progress.ToString("R",Inv)+","+s.contract.reward.ToString("R",Inv));
            return b.ToString();
        }

        SaveData DeserializeState(string text)
        {
            Dictionary<string,string> m=new Dictionary<string,string>();
            string[] lines=text.Replace("\r","").Split('\n');
            for(int i=0;i<lines.Length;i++)
            {
                int p=lines[i].IndexOf('=');
                if(p<=0)continue;
                m[lines[i].Substring(0,p)]=lines[i].Substring(p+1);
            }

            SaveData s=new SaveData();
            s.version=GetInt(m,"version",1);
            s.rebirths=GetInt(m,"rebirths",0);
            s.fork=GetInt(m,"fork",0);
            s.runSeconds=GetInt(m,"runSeconds",0);
            s.electricityTimer=GetInt(m,"electricityTimer",0);
            s.contractsCompleted=GetInt(m,"contractsCompleted",0);
            s.uidCounter=GetInt(m,"uidCounter",2);
            s.totalClicks=GetLong(m,"totalClicks",0);
            s.cash=GetDouble(m,"cash",0);
            s.totalMined=GetDouble(m,"totalMined",0);
            s.running=GetBool(m,"running",true);
            s.overclock=GetBool(m,"overclock",false);
            s.fever=GetBool(m,"fever",false);
            s.feverTime=GetInt(m,"feverTime",0);
            s.feverCooldown=GetInt(m,"feverCooldown",20);
            s.miningCoinId=GetString(m,"miningCoinId","btcx");
            s.tradeCoinId=GetString(m,"tradeCoinId","btcx");

            s.items.Clear();
            string items=GetString(m,"items","");
            if(!string.IsNullOrEmpty(items))
            {
                string[] rows=items.Split(';');
                for(int i=0;i<rows.Length;i++)
                {
                    string[] x=rows[i].Split(',');
                    if(x.Length<5)continue;
                    s.items.Add(new GpuItem{uid=x[0],modelId=ParseInt(x[1],0),temp=ParseFloat(x[2],30),coolerId=ParseInt(x[3],0),slot=ParseInt(x[4],-1)});
                }
            }

            s.coins.Clear();
            string coins=GetString(m,"coins","");
            if(!string.IsNullOrEmpty(coins))
            {
                string[] rows=coins.Split(';');
                for(int i=0;i<rows.Length;i++)
                {
                    string[] x=rows[i].Split(',');
                    if(x.Length<2)continue;
                    s.coins.Add(new CoinBalance{id=x[0],amount=ParseDouble(x[1],0)});
                }
            }

            s.forkUnlocked.Clear();
            string forks=GetString(m,"forkUnlocked","root");
            string[] forkRows=forks.Split(',');
            for(int i=0;i<forkRows.Length;i++)if(!string.IsNullOrEmpty(forkRows[i]))s.forkUnlocked.Add(forkRows[i]);

            string contract=GetString(m,"contract","");
            if(!string.IsNullOrEmpty(contract))
            {
                string[] x=contract.Split(',');
                if(x.Length>=5)s.contract=new ContractState{type=x[0],coinId=x[1],target=ParseDouble(x[2],0),progress=ParseDouble(x[3],0),reward=ParseDouble(x[4],0)};
            }
            return s;
        }

        string GetString(Dictionary<string,string> m,string k,string d){string v;return m.TryGetValue(k,out v)?v:d;}
        int GetInt(Dictionary<string,string> m,string k,int d){return ParseInt(GetString(m,k,""),d);}
        long GetLong(Dictionary<string,string> m,string k,long d){long v;return long.TryParse(GetString(m,k,""),NumberStyles.Integer,Inv,out v)?v:d;}
        double GetDouble(Dictionary<string,string> m,string k,double d){return ParseDouble(GetString(m,k,""),d);}
        bool GetBool(Dictionary<string,string> m,string k,bool d){string v=GetString(m,k,"");return v=="1"?true:v=="0"?false:d;}
        int ParseInt(string s,int d){int v;return int.TryParse(s,NumberStyles.Integer,Inv,out v)?v:d;}
        float ParseFloat(string s,float d){float v;return float.TryParse(s,NumberStyles.Float,Inv,out v)?v:d;}
        double ParseDouble(string s,double d){double v;return double.TryParse(s,NumberStyles.Float,Inv,out v)?v:d;}

        void OnApplicationQuit(){Save();}
        void OnApplicationPause(bool p){if(p)Save();}

        void Tick()
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
            GpuItem g=Find(uid);CoolerDef c=Catalog.Cooler(coolerId);if(g==null||g.slot<0)return false;if(!CoolerUnlocked(c)){Say("냉각 인증 잠김");return false;}if(S.cash<c.cost){Say("현금 부족");return false;}
            S.cash-=c.cost;g.coolerId=c.id;Say(c.name+" 장착");Save();Fire();return true;
        }

        public void ToggleOC(){S.overclock=!S.overclock;if(S.overclock){List<GpuItem> eq=Equipped();for(int i=0;i<eq.Count;i++)eq[i].temp+=3*HeatMul();}Say(S.overclock?"OC ON":"OC OFF");CheckHeat();Fire();}
        public void EmergencyCool(){List<GpuItem> eq=Equipped();float a=UnityEngine.Random.Range(16,24)+(HasFork("cooling")?5:0);for(int i=0;i<eq.Count;i++)eq[i].temp=Mathf.Max(25,eq[i].temp-a);Say("긴급 냉각 -"+Mathf.RoundToInt(a)+"C");Fire();}

        public void SetMiningCoin(string id){CoinDef c=Catalog.Coin(id);if(!CoinUnlocked(c)){Say("상위 GPU 필요");return;}S.miningCoinId=id;Save();Fire();}
        public void SetTradeCoin(string id){CoinDef c=Catalog.Coin(id);if(CoinUnlocked(c)){S.tradeCoinId=id;Fire();}}

        public bool Sell(string id,double amount)
        {
            double bal=Balance(id);if(amount<=0||amount>bal||MarketManager.I==null)return false;
            SetBalance(id,bal-amount);ProgressContract("sell",id,amount);S.cash+=amount*MarketManager.I.Price(id);Say(Catalog.Coin(id).name+" 판매");Save();Fire();return true;
        }

        public void EnsureContract()
        {
            if(S.contract!=null)return;List<CoinDef> pool=UnlockedCoins();CoinDef coin=pool[UnityEngine.Random.Range(0,pool.Count)];
            string type=UnityEngine.Random.value<.62f?"mine":"sell";int tier=Mathf.Max(0,RigTier());double scale=Math.Max(1,(tier+1)*(tier+1));
            S.contract=new ContractState{type=type,coinId=coin.id,target=Math.Round((type=="mine"?900:450)*scale),reward=Math.Round(3500*scale),progress=0};
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
            if(!CanRebirth())return;int r=S.rebirths+1,f=S.fork+1;List<string> forks=new List<string>(S.forkUnlocked);S=Fresh();S.rebirths=r;S.fork=f;S.forkUnlocked=forks;Save();Fire();Say("환생 완료 FORK +1");
        }
        public void Restart(){int r=S.rebirths,f=S.fork;List<string> forks=new List<string>(S.forkUnlocked);S=Fresh();S.rebirths=r;S.fork=f;S.forkUnlocked=forks;Save();Fire();}

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

    public class MarketManager : MonoBehaviour
    {
        public static MarketManager I;
        class Series{public float price;public List<float> h=new List<float>();}
        Dictionary<string,Series> data=new Dictionary<string,Series>();
        float tick,eventTimer;
        public string LastEvent="시장 대기 중";
        public event Action Changed;
        public event Action<string> Event;

        void Awake(){I=this;for(int i=0;i<Catalog.Coins.Length;i++){CoinDef c=Catalog.Coins[i];Series s=new Series{price=c.basePrice};for(int k=0;k<40;k++){s.price=Clamp(c,s.price*(1+UnityEngine.Random.Range(-c.vol*.5f,c.vol*.5f)));s.h.Add(s.price);}data[c.id]=s;}ResetEvent();}
        void Update()
        {
            if(GameManager.I==null||!GameManager.I.S.running||GameManager.I.IntroPaused)return;tick+=Time.unscaledDeltaTime;eventTimer-=Time.unscaledDeltaTime;
            if(tick>=5){tick-=5;for(int i=0;i<Catalog.Coins.Length;i++){CoinDef c=Catalog.Coins[i];Series s=data[c.id];s.price=Clamp(c,s.price*(1+UnityEngine.Random.Range(-c.vol*.5f,c.vol*.5f)));Push(s);}if(Changed!=null)Changed();}
            if(eventTimer<=0){RandomEvent();ResetEvent();}
        }
        void ResetEvent(){eventTimer=UnityEngine.Random.Range(45f,90f);}
        void RandomEvent()
        {
            List<CoinDef> p=GameManager.I.UnlockedCoins();CoinDef c=p[UnityEngine.Random.Range(0,p.Count)];bool up=UnityEngine.Random.value<.67f;
            float pct=(up?UnityEngine.Random.Range(.2f,c.id=="meme404"?1.2f:.85f):-UnityEngine.Random.Range(.12f,.45f))*c.eventScale;Series s=data[c.id];float before=s.price;s.price=Clamp(c,s.price*(1+pct));Push(s);
            float actual=(s.price/before-1)*100;LastEvent=c.name+" "+(up?"급등 이벤트 ":"급락 이벤트 ")+(actual>=0?"+":"")+actual.ToString("0.0")+"%";GameManager.I.S.tradeCoinId=c.id;GameManager.I.Save();if(Event!=null)Event(LastEvent);if(Changed!=null)Changed();
        }
        float Clamp(CoinDef c,float p){return Mathf.Clamp(p,c.basePrice*.18f,c.basePrice*8);}
        void Push(Series s){s.h.Add(s.price);while(s.h.Count>60)s.h.RemoveAt(0);}
        public double Price(string id){return data.ContainsKey(id)?data[id].price:Catalog.Coin(id).basePrice;}
    }

    public class FanSpinner : MonoBehaviour
    {
        public float speed=280;
        void Update(){transform.Rotate(0,0,-speed*Time.unscaledDeltaTime);}
    }

    public static class RuntimeBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if(GameManager.I!=null)return;

            // Keep the project fully 2D, but create a simple orthographic camera so
            // Unity's Game view does not show "No cameras rendering" behind the UI.
            if(Camera.main==null)
            {
                GameObject camGo=new GameObject("Main Camera");
                Camera cam=camGo.AddComponent<Camera>();
                camGo.tag="MainCamera";
                cam.orthographic=true;
                cam.orthographicSize=5f;
                cam.clearFlags=CameraClearFlags.SolidColor;
                cam.backgroundColor=new Color(.082f,.102f,.173f,1f);
                cam.transform.position=new Vector3(0,0,-10);
                UnityEngine.Object.DontDestroyOnLoad(camGo);
            }

            GameObject root=new GameObject("CryptoMiningRuntime");UnityEngine.Object.DontDestroyOnLoad(root);
            root.AddComponent<GameManager>();root.AddComponent<MarketManager>();root.AddComponent<UIController>();
        }
    }

    public class UIController : MonoBehaviour
    {
        GameManager gm;MarketManager market;Font font;Canvas canvas;RectTransform root,rack;GameObject modal,intro,gameover;
        Text bal,coinName,auto,cash,fork,temp,power,electric,fever,eventLabel,toast;
        Color bg=Hex("#151a2c"),panel=Hex("#202a47"),panel2=Hex("#0e1527"),cyan=Hex("#50dcff"),green=Hex("#79f17d"),gold=Hex("#ffd365"),red=Hex("#ff727d"),white=Hex("#f2f5ff"),muted=Hex("#9aa7c5");
        string coolTarget="";int shopSlot=-1;Coroutine toastCo;

        static Color Hex(string s){Color c;ColorUtility.TryParseHtmlString(s,out c);return c;}

        void Start()
        {
            gm=GameManager.I;market=MarketManager.I;font=ResolveFont();EnsureEventSystem();Build();
            gm.Changed+=Refresh;gm.Toast+=ShowToast;gm.Overheated+=GameOver;market.Changed+=Refresh;market.Event+=ShowToast;Refresh();
            if(PlayerPrefs.GetInt("unity_cutscene_seen_v1",0)==0)Cutscene(false);else if(PlayerPrefs.GetInt("unity_tutorial_seen_v1",0)==0)Tutorial();
        }

        Font ResolveFont()
        {
            string[] p={"Malgun Gothic","Apple SD Gothic Neo","Noto Sans CJK KR","Arial Unicode MS","Arial"};string[] all=Font.GetOSInstalledFontNames();
            for(int a=0;a<p.Length;a++)for(int i=0;i<all.Length;i++)if(string.Equals(p[a],all[i],StringComparison.OrdinalIgnoreCase))return Font.CreateDynamicFontFromOSFont(all[i],18);
            return Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        void EnsureEventSystem(){if(FindObjectOfType<EventSystem>()==null){GameObject g=new GameObject("EventSystem",typeof(EventSystem),typeof(StandaloneInputModule));DontDestroyOnLoad(g);}}

        void Build()
        {
            GameObject cg=new GameObject("Canvas",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));DontDestroyOnLoad(cg);canvas=cg.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;
            CanvasScaler sc=cg.GetComponent<CanvasScaler>();sc.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;sc.referenceResolution=new Vector2(480,800);sc.matchWidthOrHeight=.5f;
            root=Box(canvas.transform,"Main",bg,0,0,480,800);
            Label(root,"『암호화폐 마이닝 · UNITY』",14,Color.white,30,10,420,34,TextAnchor.MiddleCenter);
            Btn(root,"MOV",7,panel2,gold,378,12,42,28,delegate{Cutscene(true);});Btn(root,"?",11,panel2,cyan,425,12,42,28,Tutorial);
            RectTransform hud=Box(root,"HUD",panel,10,52,460,120);bal=Label(hud,"0",24,Color.white,10,8,220,34,TextAnchor.MiddleLeft);coinName=Label(hud,"BTC-X",8,cyan,10,42,220,18,TextAnchor.MiddleLeft);
            auto=Label(hud,"+0/s",9,green,260,10,180,24,TextAnchor.MiddleRight);fever=Label(hud,"FEVER",8,gold,10,64,430,18,TextAnchor.MiddleCenter);
            cash=Label(hud,"현금 0",8,white,10,88,100,18,TextAnchor.MiddleLeft);fork=Label(hud,"FORK 0",8,green,110,88,70,18,TextAnchor.MiddleLeft);temp=Label(hud,"TEMP",8,white,185,88,75,18,TextAnchor.MiddleCenter);power=Label(hud,"W",8,gold,265,88,70,18,TextAnchor.MiddleCenter);electric=Label(hud,"-0/min",8,red,340,88,105,18,TextAnchor.MiddleCenter);
            RectTransform room=Box(root,"Room",Hex("#cbb8a8"),10,182,460,490);rack=Box(room,"Rack",Hex("#555a66"),12,66,220,408);eventLabel=Label(room,"시장 대기 중",9,Hex("#1e2434"),240,20,205,60,TextAnchor.MiddleCenter);
            Btn(room,"COIN\nEXCHANGE",10,Hex("#32394b"),green,285,230,130,82,OpenTrade);
            BuildNav();toast=Label(root,"",9,Color.white,20,640,440,38,TextAnchor.MiddleCenter);toast.gameObject.SetActive(false);
        }

        void BuildNav()
        {
            RectTransform n=Box(root,"Nav",Hex("#090d17"),0,690,480,110);string[] names={"GPU","INV","OC","COOL","SHOP","FORK"};Action[] a={OpenGPU,OpenInv,OpenOC,OpenCool,delegate{OpenShop(-1);},OpenFork};
            for(int i=0;i<6;i++){int k=i;Btn(n,names[i],7,panel,white,5+i*79,10,74,72,delegate{a[k]();});}
        }

        void Refresh()
        {
            if(gm==null||gm.S==null)return;CoinDef c=gm.MiningCoin();bal.text=Fmt(gm.Balance(c.id));coinName.text=c.name+" 채굴 중";auto.text="+"+gm.TotalAuto().ToString("0.0")+"/s";cash.text="현금 "+Fmt(gm.S.cash);fork.text="FORK "+gm.S.fork;
            temp.text="TEMP "+Mathf.FloorToInt(gm.Hottest())+"C";temp.color=gm.Hottest()>=85?red:gm.Hottest()>=65?gold:white;power.text=gm.TotalPower()+"W";electric.text="-"+gm.ElectricityPerMinute()+"/min";
            fever.text=gm.S.fever?"MINING FEVER x"+(gm.HasFork("fever")?3:2)+" · "+gm.S.feverTime+"s":"FEVER CHARGING · "+gm.S.feverCooldown+"s";eventLabel.text=market.LastEvent;BuildRack();
        }

        void BuildRack()
        {
            for(int i=rack.childCount-1;i>=0;i--)Destroy(rack.GetChild(i).gameObject);
            for(int s=0;s<8;s++)
            {
                int slot=s,col=s%2,row=s/2;float x=8+col*102,y=8+row*98;GpuItem g=gm.At(slot);
                if(g==null){Btn(rack,"[추가하기]",7,Hex("#183521"),green,x,y,94,86,delegate{OpenShop(slot);});continue;}
                Button b=Btn(rack,"",7,Hex("#1a1f2b"),white,x,y,94,86,delegate{double gain=gm.ClickMine(g.uid);if(gain>0)ShowToast("+"+Fmt(gain)+" "+gm.MiningCoin().name);});RectTransform r=b.GetComponent<RectTransform>();GPUDef m=Catalog.GPU(g.modelId);CoolerDef cool=Catalog.Cooler(g.coolerId);
                Label(r,m.name,5,muted,3,2,68,14,TextAnchor.MiddleLeft);Label(r,Mathf.FloorToInt(g.temp)+"C",5,cyan,70,2,21,14,TextAnchor.MiddleRight);
                if(cool.id>=3)Label(r,"WATER\n"+cool.name,6,cyan,7,25,80,44,TextAnchor.MiddleCenter);
                else{Text f1=Label(r,"+",28,muted,12,28,30,30,TextAnchor.MiddleCenter);Text f2=Label(r,"+",28,muted,52,28,30,30,TextAnchor.MiddleCenter);float sp=g.temp>=65?720:280;f1.gameObject.AddComponent<FanSpinner>().speed=sp;f2.gameObject.AddComponent<FanSpinner>().speed=sp;}
            }
        }

        void OpenGPU()
        {
            RectTransform c=Modal("GPU MANAGER");AddCard(c,"RIG STATUS",gm.Equipped().Count+"/8 ONLINE\nHASH "+gm.TotalHash().ToString("0.0")+" H/s\nAUTO "+gm.TotalAuto().ToString("0.0")+"/s","",null);
            int tier=gm.RigTier();GPUDef next=tier<Catalog.GPUs.Length-1?Catalog.GPUs[tier+1]:null;AddCard(c,"CERTIFICATION",next==null?"모든 인증 완료":"다음 "+next.name+"\n운영 "+gm.Duration(gm.S.runSeconds)+" / "+gm.Duration(gm.RequiredSeconds(next))+"\n누적 "+Fmt(gm.S.totalMined)+" / "+Fmt(next.unlockMined),"",null);
            AddCard(c,"PRIMARY GPU",gm.At(0)==null?"EMPTY":Catalog.GPU(gm.At(0).modelId).name,"업그레이드",delegate{gm.UpgradePrimary();OpenGPU();});
            for(int i=0;i<8;i++){int slot=i;GpuItem g=gm.At(i);if(g==null)AddCard(c,"SLOT "+(i+1),"EMPTY","SHOP",delegate{OpenShop(slot);});else AddCard(c,"SLOT "+(i+1)+" · "+Catalog.GPU(g.modelId).name,"TEMP "+Mathf.FloorToInt(g.temp)+"C\n"+Catalog.Cooler(g.coolerId).name,"해제",delegate{gm.Unequip(g.uid);OpenGPU();});}
        }

        void OpenInv()
        {
            RectTransform c=Modal("GPU INVENTORY");int free=FreeSlot();for(int i=0;i<gm.S.items.Count;i++){GpuItem g=gm.S.items[i];GPUDef m=Catalog.GPU(g.modelId);if(g.slot>=0)AddCard(c,m.name+" SLOT "+(g.slot+1),m.hash+" H/s · "+m.power+"W","해제",delegate{gm.Unequip(g.uid);OpenInv();});else AddCard(c,m.name+" 보관",m.hash+" H/s · "+m.power+"W",free>=0?"장착":"랙 가득",free>=0?(Action)delegate{gm.Equip(g.uid,FreeSlot());OpenInv();}:null);}
        }

        void OpenOC(){RectTransform c=Modal("OVERCLOCK");AddCard(c,gm.S.overclock?"OC ACTIVE":"OC OFF","자동 채굴 "+gm.TotalAuto().ToString("0.0")+"/s\n보너스 +"+(gm.HasFork("oc")?50:30)+"%\n발열 증가",gm.S.overclock?"OFF":"ON",delegate{gm.ToggleOC();OpenOC();});}

        void OpenCool()
        {
            RectTransform c=Modal("GPU COOLING LAB");List<GpuItem> eq=gm.Equipped();if(string.IsNullOrEmpty(coolTarget)&&eq.Count>0)coolTarget=eq[0].uid;
            AddCard(c,"RACK TEMP","최고 "+Mathf.FloorToInt(gm.Hottest())+"C\n100C 도달 시 파산","긴급 냉각",delegate{gm.EmergencyCool();OpenCool();});
            for(int i=0;i<eq.Count;i++){GpuItem g=eq[i];AddCard(c,Catalog.GPU(g.modelId).name+(g.uid==coolTarget?" · 선택됨":""),"TEMP "+Mathf.FloorToInt(g.temp)+"C\n"+Catalog.Cooler(g.coolerId).name,"선택",delegate{coolTarget=g.uid;OpenCool();});}
            GpuItem target=gm.Find(coolTarget);if(target!=null)for(int i=0;i<Catalog.Coolers.Length;i++){CoolerDef cl=Catalog.Coolers[i];int id=cl.id;bool u=gm.CoolerUnlocked(cl);AddCard(c,cl.name,cl.desc+"\n냉각 -"+cl.rate.ToString("0.00")+"C/s · "+Fmt(cl.cost)+"원"+(u?"":"\n잠금 운영 "+gm.Duration(Mathf.FloorToInt(cl.unlockSeconds*gm.CycleFactor()))),target.coolerId==id?"장착중":u?"구매/장착":"잠금",u&&target.coolerId!=id?(Action)delegate{gm.BuyCooler(target.uid,id);OpenCool();}:null);}
        }

        void OpenShop(int slot)
        {
            shopSlot=slot;RectTransform c=Modal("HARDWARE SHOP");AddCard(c,"현금",Fmt(gm.S.cash)+"원"+(slot>=0?"\nSLOT "+(slot+1)+" 자동 장착":""),"",null);
            for(int i=0;i<Catalog.GPUs.Length;i++){GPUDef m=Catalog.GPUs[i];int id=m.id;bool u=gm.Certified(m);AddCard(c,m.name,"CLICK "+m.click+" · AUTO "+m.autoMine+"/s\n"+m.hash+" H/s · "+m.power+"W\n가격 "+Fmt(m.price)+"원"+(u?"":"\n잠금: "+gm.Duration(gm.RequiredSeconds(m))+" + 누적 "+Fmt(m.unlockMined)),u?"구매":"잠금",u?(Action)delegate{gm.BuyGpu(id,shopSlot);OpenShop(-1);}:null);}
        }

        void OpenFork()
        {
            RectTransform c=Modal("FORK / REBIRTH");AddCard(c,"REBIRTH","QUANTUM "+gm.QuantumCount()+" / "+gm.RebirthReq()+"\n운영 "+gm.Duration(gm.S.runSeconds)+" / "+gm.Duration(gm.RebirthSeconds())+"\nFORK "+gm.S.fork,gm.CanRebirth()?"환생":"조건 미달",gm.CanRebirth()?(Action)delegate{gm.Rebirth();OpenFork();}:null);
            string[,] nodes={{"cooling","THERMAL SHIELD","root","발열 -20%, 냉각 강화"},{"click","CLICK ENGINE","root","클릭 +25%"},{"auto","AUTO HASH","root","자동 +30%"},{"oc","STABLE OC","click","OC +50%"},{"fork","MINING MASTERY","auto","전체 +10%"},{"fever","FEVER CORE","cooling","FEVER x3"}};
            for(int i=0;i<nodes.GetLength(0);i++){string id=nodes[i,0],name=nodes[i,1],req=nodes[i,2],desc=nodes[i,3];bool have=gm.HasFork(id),ok=gm.HasFork(req);AddCard(c,name,desc+"\nCOST 1 FORK",have?"UNLOCKED":ok?"해금":"선행 필요",!have&&ok?(Action)delegate{gm.BuyFork(id,req);OpenFork();}:null);}
        }

        void OpenTrade()
        {
            RectTransform c=Modal("MULTI COIN EXCHANGE");gm.EnsureContract();
            for(int i=0;i<Catalog.Coins.Length;i++){CoinDef coin=Catalog.Coins[i];bool u=gm.CoinUnlocked(coin);string id=coin.id;AddCard(c,coin.name,"시세 "+market.Price(id).ToString("0.000")+"원 · 보유 "+Fmt(gm.Balance(id))+(u?"":"\n잠금 "+Catalog.GPU(coin.unlockGpu).name+" 보유 필요"),u?(gm.S.miningCoinId==id?"채굴중":"채굴"):"잠금",u?(Action)delegate{gm.SetMiningCoin(id);gm.SetTradeCoin(id);OpenTrade();}:null);}
            CoinDef t=Catalog.Coin(gm.S.tradeCoinId);double b=gm.Balance(t.id);AddCard(c,"SELL "+t.name,"현재가 "+market.Price(t.id).ToString("0.000")+"원\n보유 "+Fmt(b),"25% 판매",b>0?(Action)delegate{gm.Sell(t.id,gm.Balance(t.id)*.25);OpenTrade();}:null);AddCard(c,"전량 판매",Fmt(b)+" "+t.name,"전량 판매",b>0?(Action)delegate{gm.Sell(t.id,gm.Balance(t.id));OpenTrade();}:null);
            ContractState q=gm.S.contract;AddCard(c,"MINING CONTRACT",(q.type=="mine"?"채굴 ":"판매 ")+Catalog.Coin(q.coinId).name+"\n"+Fmt(q.progress)+" / "+Fmt(q.target)+"\n보상 "+Fmt(q.reward)+"원",q.progress>=q.target?"보상 받기":"진행 중",q.progress>=q.target?(Action)delegate{gm.ClaimContract();OpenTrade();}:null);
            AddCard(c,"MARKET EVENT",market.LastEvent,"",null);
        }

        RectTransform Modal(string title)
        {
            CloseModal();modal=new GameObject("Modal",typeof(RectTransform),typeof(Image));modal.transform.SetParent(canvas.transform,false);Fill(modal.GetComponent<RectTransform>());modal.GetComponent<Image>().color=new Color(.01f,.02f,.05f,.92f);
            RectTransform box=Box(modal.transform,"Box",panel,20,35,440,720);Label(box,title,12,cyan,12,8,350,40,TextAnchor.MiddleLeft);Btn(box,"X",10,Hex("#4b2330"),Color.white,382,8,44,36,CloseModal);
            GameObject sg=new GameObject("Scroll",typeof(RectTransform),typeof(ScrollRect));sg.transform.SetParent(box,false);RectTransform sr=sg.GetComponent<RectTransform>();TL(sr,12,54,416,650);
            GameObject vp=new GameObject("Viewport",typeof(RectTransform),typeof(Image),typeof(Mask));vp.transform.SetParent(sg.transform,false);Fill(vp.GetComponent<RectTransform>());vp.GetComponent<Image>().color=Color.clear;vp.GetComponent<Mask>().showMaskGraphic=false;
            GameObject cg=new GameObject("Content",typeof(RectTransform),typeof(VerticalLayoutGroup),typeof(ContentSizeFitter));cg.transform.SetParent(vp.transform,false);RectTransform cr=cg.GetComponent<RectTransform>();cr.anchorMin=new Vector2(0,1);cr.anchorMax=new Vector2(1,1);cr.pivot=new Vector2(.5f,1);cr.offsetMin=Vector2.zero;cr.offsetMax=Vector2.zero;
            VerticalLayoutGroup vg=cg.GetComponent<VerticalLayoutGroup>();vg.spacing=8;vg.padding=new RectOffset(6,6,6,12);vg.childControlWidth=true;vg.childForceExpandWidth=true;cg.GetComponent<ContentSizeFitter>().verticalFit=ContentSizeFitter.FitMode.PreferredSize;
            ScrollRect sc=sg.GetComponent<ScrollRect>();sc.viewport=vp.GetComponent<RectTransform>();sc.content=cr;sc.horizontal=false;return cr;
        }

        RectTransform AddCard(RectTransform p,string title,string body,string button,Action action)
        {
            float h=string.IsNullOrEmpty(button)?90:132;GameObject g=new GameObject("Card",typeof(RectTransform),typeof(Image),typeof(VerticalLayoutGroup),typeof(LayoutElement));g.transform.SetParent(p,false);g.GetComponent<Image>().color=panel2;g.GetComponent<LayoutElement>().preferredHeight=h;VerticalLayoutGroup v=g.GetComponent<VerticalLayoutGroup>();v.padding=new RectOffset(10,10,8,8);v.spacing=5;v.childControlWidth=true;v.childForceExpandWidth=true;
            LText(g.transform,title,9,green,24);LText(g.transform,body,11,white,50);if(!string.IsNullOrEmpty(button)){GameObject b=new GameObject("Button",typeof(RectTransform),typeof(Image),typeof(Button),typeof(LayoutElement));b.transform.SetParent(g.transform,false);b.GetComponent<Image>().color=action!=null?Hex("#285733"):Hex("#2a2f3a");b.GetComponent<LayoutElement>().preferredHeight=36;Button bt=b.GetComponent<Button>();bt.interactable=action!=null;if(action!=null)bt.onClick.AddListener(delegate{action();});LabelFill(b.transform,button,7,action!=null?Color.white:muted,TextAnchor.MiddleCenter);}return g.GetComponent<RectTransform>();
        }

        Text LText(Transform p,string s,int size,Color c,float h){GameObject g=new GameObject("Text",typeof(RectTransform),typeof(Text),typeof(LayoutElement));g.transform.SetParent(p,false);Text t=g.GetComponent<Text>();t.font=font;t.fontSize=size;t.color=c;t.text=s;t.alignment=TextAnchor.MiddleLeft;t.horizontalOverflow=HorizontalWrapMode.Wrap;t.verticalOverflow=VerticalWrapMode.Overflow;g.GetComponent<LayoutElement>().preferredHeight=h;return t;}
        RectTransform Box(Transform p,string n,Color c,float x,float y,float w,float h){GameObject g=new GameObject(n,typeof(RectTransform),typeof(Image));g.transform.SetParent(p,false);RectTransform r=g.GetComponent<RectTransform>();TL(r,x,y,w,h);g.GetComponent<Image>().color=c;return r;}
        Button Btn(Transform p,string s,int size,Color bgc,Color fg,float x,float y,float w,float h,Action a){GameObject g=new GameObject("Button",typeof(RectTransform),typeof(Image),typeof(Button));g.transform.SetParent(p,false);RectTransform r=g.GetComponent<RectTransform>();TL(r,x,y,w,h);g.GetComponent<Image>().color=bgc;Button b=g.GetComponent<Button>();if(a!=null)b.onClick.AddListener(delegate{a();});LabelFill(g.transform,s,size,fg,TextAnchor.MiddleCenter);return b;}
        Text Label(RectTransform p,string s,int size,Color c,float x,float y,float w,float h,TextAnchor a){GameObject g=new GameObject("Text",typeof(RectTransform),typeof(Text));g.transform.SetParent(p,false);RectTransform r=g.GetComponent<RectTransform>();TL(r,x,y,w,h);Text t=g.GetComponent<Text>();t.font=font;t.fontSize=size;t.color=c;t.text=s;t.alignment=a;t.horizontalOverflow=HorizontalWrapMode.Wrap;t.verticalOverflow=VerticalWrapMode.Overflow;return t;}
        Text LabelFill(Transform p,string s,int size,Color c,TextAnchor a){GameObject g=new GameObject("Text",typeof(RectTransform),typeof(Text));g.transform.SetParent(p,false);Fill(g.GetComponent<RectTransform>());Text t=g.GetComponent<Text>();t.font=font;t.fontSize=size;t.color=c;t.text=s;t.alignment=a;return t;}
        void TL(RectTransform r,float x,float y,float w,float h){r.anchorMin=r.anchorMax=new Vector2(0,1);r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);}
        void Fill(RectTransform r){r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;}
        int FreeSlot(){for(int i=0;i<8;i++)if(gm.At(i)==null)return i;return-1;}
        void CloseModal(){if(modal!=null)Destroy(modal);modal=null;}

        void ShowToast(string s){if(toastCo!=null)StopCoroutine(toastCo);toastCo=StartCoroutine(ToastRoutine(s));}
        IEnumerator ToastRoutine(string s){toast.text=s;toast.gameObject.SetActive(true);yield return new WaitForSecondsRealtime(2);toast.gameObject.SetActive(false);}

        void GameOver()
        {
            CloseModal();gameover=new GameObject("GameOver",typeof(RectTransform),typeof(Image));gameover.transform.SetParent(canvas.transform,false);Fill(gameover.GetComponent<RectTransform>());gameover.GetComponent<Image>().color=new Color(.08f,.01f,.02f,.96f);
            RectTransform b=Box(gameover.transform,"Box",Hex("#35121c"),45,220,390,300);Label(b,"SYSTEM FAILURE",18,red,20,25,350,45,TextAnchor.MiddleCenter);Label(b,"GPU 100C 도달\n장비/코인/현금 초기화\n환생/FORK 유지",12,Color.white,30,95,330,90,TextAnchor.MiddleCenter);
            Btn(b,"시스템 재부팅",9,Hex("#47232e"),Color.white,55,210,280,50,delegate{gm.Restart();Destroy(gameover);});
        }

        string[] ct={"낡은 채굴실","첫 번째 GPU","가상 코인 시장","더 강한 장비","목표: QUANTUM"};
        string[] cb={"불 꺼진 작은 방. 오래된 장비 한 대만 남아 있다.","RTX 1090의 팬이 돌기 시작한다. 이 한 장이 시작이다.","채굴한 코인을 가상 시장에서 현금으로 바꿀 수 있다.","GPU와 냉각을 강화하고 새로운 코인을 해금하라.","RTX 6090 QUANTUM과 환생 프로토콜에 도달하라."};
        int ci;bool replay;
        void Cutscene(bool rp){CloseModal();CloseIntro();replay=rp;ci=0;gm.IntroPaused=true;RenderCut();}
        void RenderCut(){if(intro!=null)Destroy(intro);intro=new GameObject("Cutscene",typeof(RectTransform),typeof(Image));intro.transform.SetParent(canvas.transform,false);Fill(intro.GetComponent<RectTransform>());intro.GetComponent<Image>().color=Hex("#02040a");RectTransform b=Box(intro.transform,"Box",panel2,25,100,430,600);Label(b,"OPENING "+(ci+1)+" / 5",8,cyan,15,10,250,30,TextAnchor.MiddleLeft);Btn(b,"SKIP",7,Hex("#3a2430"),Color.white,330,10,80,32,FinishCut);Label(b,ci==4?"QUANTUM":ci==2?"B E D N 404 Q":ci==1?"GPU":"...",ci==2?20:36,ci==4?green:cyan,20,85,390,170,TextAnchor.MiddleCenter);Label(b,ct[ci],13,gold,20,280,390,35,TextAnchor.MiddleCenter);Label(b,cb[ci],13,Color.white,30,335,370,120,TextAnchor.MiddleCenter);Btn(b,ci==4?"채굴 시작":"계속",9,Hex("#244b32"),Color.white,260,515,145,48,NextCut);}
        void NextCut(){if(ci<4){ci++;RenderCut();}else FinishCut();}
        void FinishCut(){PlayerPrefs.SetInt("unity_cutscene_seen_v1",1);PlayerPrefs.Save();bool r=replay;CloseIntro();if(!r&&PlayerPrefs.GetInt("unity_tutorial_seen_v1",0)==0)Tutorial();}

        string[] tt={"GPU 클릭 채굴","코인 판매","GPU 구매/업그레이드","쿨링","전력/OC","계약/시장 이벤트","환생/FORK"};
        string[] tb={"랙 GPU를 클릭하면 장착 GPU가 모두 같이 채굴합니다.","COIN EXCHANGE에서 코인을 팔아 현금을 만드세요.","상위 GPU 보유 시 새 코인이 해금됩니다.","COOL에서 GPU별 공랭/수냉을 장착하세요. 100C면 파산합니다.","OC는 채굴량과 발열을 함께 올립니다. 전기요금도 실제 게임 현금에서 빠집니다.","채굴/판매 계약과 랜덤 급등/급락 이벤트가 있습니다.","QUANTUM과 조건을 달성하면 환생하고 FORK를 얻습니다."};
        int ti;
        void Tutorial(){CloseModal();CloseIntro();ti=0;gm.IntroPaused=true;RenderTut();}
        void RenderTut(){if(intro!=null)Destroy(intro);intro=new GameObject("Tutorial",typeof(RectTransform),typeof(Image));intro.transform.SetParent(canvas.transform,false);Fill(intro.GetComponent<RectTransform>());intro.GetComponent<Image>().color=new Color(.01f,.02f,.05f,.95f);RectTransform b=Box(intro.transform,"Box",panel,30,150,420,500);Label(b,"TUTORIAL "+(ti+1)+" / 7",8,cyan,15,10,230,30,TextAnchor.MiddleLeft);Btn(b,"건너뛰기",7,Hex("#3a2430"),Color.white,300,10,100,34,FinishTut);Label(b,tt[ti],13,green,20,85,380,45,TextAnchor.MiddleCenter);Label(b,tb[ti],13,Color.white,30,150,360,170,TextAnchor.MiddleCenter);if(ti>0)Btn(b,"이전",8,Hex("#25465c"),Color.white,30,405,150,48,delegate{ti--;RenderTut();});Btn(b,ti==6?"게임 시작":"다음",8,Hex("#285733"),Color.white,240,405,150,48,delegate{if(ti<6){ti++;RenderTut();}else FinishTut();});}
        void FinishTut(){PlayerPrefs.SetInt("unity_tutorial_seen_v1",1);PlayerPrefs.Save();CloseIntro();ShowToast("튜토리얼 완료");}
        void CloseIntro(){if(intro!=null)Destroy(intro);intro=null;if(gm!=null)gm.IntroPaused=false;}

        string Fmt(double n){if(n>=1000000000)return(n/1000000000d).ToString("0.##")+"B";if(n>=1000000)return(n/1000000d).ToString("0.##")+"M";if(n>=1000)return n.ToString("N0");return n.ToString("0.##");}
    }
}
