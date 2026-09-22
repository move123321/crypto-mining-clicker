using System;
using System.Collections.Generic;
using UnityEngine;
namespace CryptoMining {
[Serializable] public class Candle { public double time; public float open,close,high,low; }
[Serializable] public class MarketSeries { public string id; public List<Candle> candles=new List<Candle>(); }
public class MarketManager:MonoBehaviour {
 public static MarketManager I; public string LastEvent="시장 대기 중",EventCoin="btcx"; public event Action Changed; public event Action<string> Event;
 public Func<bool> ModalOpen; float tick,eventTimer;
 void Awake(){I=this;Init();}
 public void Init(){foreach(var c in Catalog.Coins)if(!GameManager.I.S.markets.Exists(x=>x.id==c.id)){var m=new MarketSeries{id=c.id};float close=c.basePrice;for(int i=0;i<40;i++){float open=Clamp(c,close*(1+UnityEngine.Random.Range(-.5f,.5f)*c.vol));m.candles.Insert(0,new Candle{time=Now()-i*5000,open=open,close=close,high=Clamp(c,Mathf.Max(open,close)*1.01f),low=Clamp(c,Mathf.Min(open,close)*.99f)});close=open;}GameManager.I.S.markets.Add(m);}eventTimer=UnityEngine.Random.Range(45,91);}
 static double Now(){return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();}
 public MarketSeries Series(string id){return GameManager.I.S.markets.Find(x=>x.id==id);}
 public double Price(string id){var s=Series(id);return s==null?Catalog.Coin(id).basePrice:s.candles[s.candles.Count-1].close;}
 public float Countdown {get{return Mathf.Max(0,5-tick);}}
 float Clamp(CoinDef c,float p){return Mathf.Clamp(p,c.basePrice*.18f,c.basePrice*8);}
 void Push(MarketSeries s,Candle c){c.time=Math.Max(Now(),s.candles.Count==0?0:s.candles[s.candles.Count-1].time+1);s.candles.Add(c);if(s.candles.Count>60)s.candles.RemoveAt(0);}
 void Update(){tick+=Time.unscaledDeltaTime;if(tick>=5){tick-=5;MarketTick();}var g=GameManager.I;if(!g.S.running||g.IntroPaused)return;eventTimer-=Time.unscaledDeltaTime;if(eventTimer<=0&&(ModalOpen==null||!ModalOpen()))TriggerEvent();}
 public void MarketTick(){foreach(var coin in Catalog.Coins){float open=(float)Price(coin.id),close=Clamp(coin,open*(1+UnityEngine.Random.Range(-.5f,.5f)*coin.vol));Push(Series(coin.id),new Candle{open=open,close=close,high=Clamp(coin,Mathf.Max(open,close)*(1+UnityEngine.Random.value*coin.vol*.35f)),low=Clamp(coin,Mathf.Min(open,close)*(1-UnityEngine.Random.value*coin.vol*.35f))});}Changed?.Invoke();}
 public void TriggerEvent(){string[] title={"대형 거래소 상장","고래 지갑 대량 매수","채굴 최적화 패치 공개","커뮤니티 밈 폭발","대량 매도 물량 출현","네트워크 장애 루머"};float[] min={.45f,.25f,.20f,.40f,.20f,.12f},max={1.05f,.70f,.55f,1.20f,.45f,.35f};var pool=GameManager.I.UnlockedCoins();var coin=pool[UnityEngine.Random.Range(0,pool.Count)];int i=UnityEngine.Random.Range(0,6);float pct=UnityEngine.Random.Range(min[i],max[i])*coin.eventScale*(i<4?1:-1);float open=(float)Price(coin.id),close=Clamp(coin,open*(1+pct));Push(Series(coin.id),new Candle{open=open,close=close,high=Clamp(coin,Mathf.Max(open,close)*(i<4?1.015f:1.005f)),low=Clamp(coin,Mathf.Min(open,close)*(i<4?.995f:.985f))});EventCoin=coin.id;LastEvent=coin.name+" · "+title[i]+"\n"+(close>=open?"+":"")+((close/open-1)*100).ToString("0.0")+"%";GameManager.I.S.tradeCoinId=coin.id;eventTimer=UnityEngine.Random.Range(45,91);GameManager.I.Save();Event?.Invoke(LastEvent);Changed?.Invoke();}
}
}
