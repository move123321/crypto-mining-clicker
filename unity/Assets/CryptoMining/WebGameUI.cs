using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Text = TMPro.TextMeshProUGUI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
namespace CryptoMining {
public class WebGameUI:MonoBehaviour {
 GameManager g; MarketManager market; TMP_FontAsset font; Canvas canvas; RectTransform screen,body,rack,modal,content,intro; Text money,coinIcon,coin,rate,cash,fork,temp,power,bill,fever,toast; float uiTick;
 readonly List<Action> live=new List<Action>(); readonly List<RectTransform> fans=new List<RectTransform>(); readonly List<Text> rackTemps=new List<Text>(); string rackKey="",page="",coolTarget=null,equipUid=null,selectedNode="root"; int pendingSlot=-1; Coroutine toastRoutine,typing; Text typed; string fullCaption; bool typingNow; int introIndex; bool replay;
 static readonly Color Bg=C("151a2c"),Panel=C("202a47"),Dark=C("0e1527"),Cyan=C("50dcff"),Green=C("79f17d"),Gold=C("ffd365"),Red=C("ff727d"),Muted=C("9aa7c5"),White=C("f2f5ff");
 static Color C(string s){Color c;ColorUtility.TryParseHtmlString("#"+s,out c);return c;}
 static Sprite fanSprite;
 static readonly Sprite[] navSprites=new Sprite[6];
 static Sprite NavSprite(int index){
  if(navSprites[index]!=null)return navSprites[index];
  var atlas=Resources.Load<Texture2D>("CryptoMining/NavIcons");
  if(atlas==null)return null;
  atlas.filterMode=FilterMode.Bilinear;
  float cellWidth=atlas.width/3f,cellHeight=atlas.height/2f;
  var area=new UnityEngine.Rect((index%3)*cellWidth,(1-index/3)*cellHeight,cellWidth,cellHeight);
  navSprites[index]=Sprite.Create(atlas,area,new Vector2(.5f,.5f),100);
  return navSprites[index];
 }
 static Sprite ArtSprite(string path,ref Sprite cache){if(cache!=null)return cache;var tex=Resources.Load<Texture2D>(path);if(tex==null)return null;tex.filterMode=FilterMode.Bilinear;cache=Sprite.Create(tex,new UnityEngine.Rect(0,0,tex.width,tex.height),new Vector2(.5f,.5f),100);return cache;}
 static Sprite FanSprite(){return ArtSprite("CryptoMining/GpuFan",ref fanSprite);}
 static TMP_FontAsset CreateUiFont(){
  var bundled=Resources.Load<Font>("CryptoMining/Fonts/NotoSansCJKkr-Regular");
  if(bundled!=null){
   var asset=TMP_FontAsset.CreateFontAsset(bundled);
   if(asset!=null){asset.isMultiAtlasTexturesEnabled=true;asset.fallbackFontAssetTable=new List<TMP_FontAsset>();if(TMP_Settings.defaultFontAsset!=null)asset.fallbackFontAssetTable.Add(TMP_Settings.defaultFontAsset);return asset;}
  }
  foreach(var family in new[]{"Malgun Gothic","Apple SD Gothic Neo","Noto Sans CJK KR","Arial"}){
   var asset=TMP_FontAsset.CreateFontAsset(family,"Regular",90);
   if(asset!=null){asset.isMultiAtlasTexturesEnabled=true;asset.fallbackFontAssetTable=new List<TMP_FontAsset>();if(TMP_Settings.defaultFontAsset!=null)asset.fallbackFontAssetTable.Add(TMP_Settings.defaultFontAsset);return asset;}
  }
  return TMP_Settings.defaultFontAsset;
 }
 static TextAlignmentOptions ToTmpAlignment(TextAnchor anchor){
  switch(anchor){
   case TextAnchor.UpperLeft:return TextAlignmentOptions.TopLeft;
   case TextAnchor.UpperCenter:return TextAlignmentOptions.Top;
   case TextAnchor.UpperRight:return TextAlignmentOptions.TopRight;
   case TextAnchor.MiddleCenter:return TextAlignmentOptions.Center;
   case TextAnchor.MiddleRight:return TextAlignmentOptions.MidlineRight;
   case TextAnchor.LowerLeft:return TextAlignmentOptions.BottomLeft;
   case TextAnchor.LowerCenter:return TextAlignmentOptions.Bottom;
   case TextAnchor.LowerRight:return TextAlignmentOptions.BottomRight;
   default:return TextAlignmentOptions.MidlineLeft;
  }
 }
 static string N(double n){return Math.Floor(n).ToString("N0",CultureInfo.InvariantCulture);}
 static string D(double n){return n.ToString("0.0",CultureInfo.InvariantCulture);}
 void Awake(){g=GetComponent<GameManager>();if(g==null)g=gameObject.AddComponent<GameManager>();market=GetComponent<MarketManager>();if(market==null)market=gameObject.AddComponent<MarketManager>();}
 void Start(){font=CreateUiFont();Build();g.Changed+=Refresh;g.Toast+=Toast;g.Overheated+=GameOver;market.Changed+=Refresh;market.Event+=News;market.ModalOpen=()=>modal!=null||intro!=null;Refresh();if(!g.S.running)GameOver();else if(PlayerPrefs.GetInt("webport_opening",0)==0)Opening(false);else if(PlayerPrefs.GetInt("webport_tutorial",0)==0)Tutorial();}
 void OnDestroy(){if(g!=null){g.Changed-=Refresh;g.Toast-=Toast;g.Overheated-=GameOver;}if(market!=null){market.Changed-=Refresh;market.Event-=News;}if(canvas!=null)Destroy(canvas.gameObject);}
 void Update(){foreach(var f in fans)if(f!=null)f.Rotate(0,0,-(g.Hottest()>=65?720:280)*Time.unscaledDeltaTime);uiTick+=Time.unscaledDeltaTime;if(uiTick>=.2f){uiTick=0;Refresh();}}
 RectTransform Rect(Transform p,string name,float x,float y,float w,float h){var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(p,false);r.anchorMin=r.anchorMax=new Vector2(0,1);r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);return r;}
 Image Paint(RectTransform r,Color c){var i=r.gameObject.AddComponent<Image>();i.color=c;return i;}
 RectTransform Box(Transform p,string name,float x,float y,float w,float h,Color c,Color? border=null,int bw=2){var r=Rect(p,name,x,y,w,h);Paint(r,border??c);if(border.HasValue){var fill=Rect(r,"Fill",bw,bw,w-bw*2,h-bw*2);Paint(fill,c).raycastTarget=false;}return r;}
 Text Text(Transform p,string value,float x,float y,float w,float h,int size=13,Color? color=null,TextAnchor align=TextAnchor.MiddleLeft){var r=Rect(p,value.Length>24?value.Substring(0,24):value,x,y,w,h);var t=r.gameObject.AddComponent<Text>();t.font=font;t.text=value;t.fontSize=size;t.color=color??White;t.alignment=ToTmpAlignment(align);t.raycastTarget=false;t.textWrappingMode=TextWrappingModes.Normal;t.overflowMode=TextOverflowModes.Ellipsis;t.enableAutoSizing=true;t.fontSizeMin=size*.85f;t.fontSizeMax=size;return t;}
 Button Button(Transform p,string label,float x,float y,float w,float h,Action action,Color? accent=null,int size=12){var c=accent??Green;var r=Box(p,label,x,y,w,h,C("1b3040"),c);var b=r.gameObject.AddComponent<Button>();b.targetGraphic=r.GetComponent<Image>();b.interactable=action!=null;if(action!=null)b.onClick.AddListener(()=>action());var colors=b.colors;colors.disabledColor=new Color(.35f,.35f,.35f,.6f);colors.pressedColor=new Color(.6f,.7f,.8f);b.colors=colors;Text(r,label,4,2,w-8,h-4,size,White,TextAnchor.MiddleCenter);return b;}
 void Fill(RectTransform r){r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;}
 RectTransform Scroll(Transform p,float x,float y,float w,float h,out ScrollRect sc){var outer=Rect(p,"Scroll",x,y,w,h);sc=outer.gameObject.AddComponent<ScrollRect>();var vp=Rect(outer,"Viewport",0,0,w,h);Paint(vp,new Color(0,0,0,0));vp.gameObject.AddComponent<RectMask2D>();var cr=Rect(vp,"Content",0,0,w,0);sc.viewport=vp;sc.content=cr;sc.horizontal=false;sc.movementType=ScrollRect.MovementType.Clamped;sc.scrollSensitivity=35;return cr;}
 void Build(){if(FindFirstObjectByType<EventSystem>()==null)new GameObject("EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule));var cv=new GameObject("Web Game Canvas",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));canvas=cv.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.pixelPerfect=false;var scaler=cv.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(480,854);scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.Expand;var backdrop=Rect(cv.transform,"Backdrop",0,0,480,854);Fill(backdrop);Paint(backdrop,C("0b101d"));screen=Box(cv.transform,"Mobile 480 x 854",0,0,480,854,Bg);screen.anchorMin=screen.anchorMax=new Vector2(.5f,.5f);screen.pivot=new Vector2(.5f,.5f);screen.anchoredPosition=Vector2.zero;
 ScrollRect scroll;body=Scroll(screen,0,0,480,774,out scroll);body.sizeDelta=new Vector2(480,817);Paint(body,C("303a54"));for(int i=0;i<35;i++){Box(body,"Grid",i*14,0,1,817,new Color(1,1,1,.025f));}for(int i=0;i<59;i++)Box(body,"Grid",0,i*14,480,1,new Color(1,1,1,.025f));
 var music=GetComponent<MiningMusic>();if(music==null)music=gameObject.AddComponent<MiningMusic>();var sound=Button(body,music.Muted?"음악 켜기":"음악 끄기",10,4,92,22,null,Cyan,10);sound.interactable=true;sound.onClick.AddListener(()=>{music.Toggle();sound.GetComponentInChildren<Text>().text=music.Muted?"음악 켜기":"음악 끄기";});Text(body,"『암호화폐 마이닝』",60,28,340,30,19,White,TextAnchor.MiddleCenter);Button(body,"▶",397,28,30,30,()=>Opening(true),Gold);Button(body,"?",436,28,30,30,Tutorial,Cyan,16);
 var hud=Box(body,"HUD",9,66,462,234,C("253150"),C("56658c"),3);var wallet=Box(hud,"Wallet",10,10,442,62,C("1b2440"),C("425078"));BuildWalletContents(wallet);
 var f=Box(hud,"Fever",10,80,442,34,C("684113"),C("f2b451"));fever=Text(f,"",5,2,432,30,12,Gold,TextAnchor.MiddleCenter);
 var cashBox=Box(hud,"Cash",10,122,218,34,Dark,C("465375"));cash=Text(cashBox,"",10,3,198,28,13);var forkBox=Box(hud,"Fork",234,122,218,34,Dark,C("465375"));ForkIcon(forkBox,10,8,17);fork=Text(forkBox,"",32,3,176,28,13,Green,TextAnchor.MiddleRight);
 var t=Box(hud,"Temp",10,164,143,60,Dark,C("465375"));Text(t,"온도",8,5,127,18,11,Muted);temp=Text(t,"",8,25,127,26,15);
 var p=Box(hud,"Power",160,164,142,60,Dark,C("465375"));Text(p,"전력",8,5,126,18,11,Muted);power=Text(p,"",8,25,126,26,15,Gold);
 var e=Box(hud,"Electricity",309,164,143,60,Dark,C("465375"));Text(e,"전기요금",8,5,127,18,10,Muted);bill=Text(e,"",8,25,127,26,13,Red);
 var room=Box(body,"Mining Room",9,309,462,500,C("cbb8a8"),C("88746f"),3);Box(room,"Floor",3,320,456,177,C("ded0c2"));Box(room,"Wood trim",3,3,456,18,C("654533"));rack=Box(room,"Rack",8,163,224,322,C("626775"),C("30343d"),4);
 var desk=Rect(room,"Trading Desk",252,266,192,220);Box(desk,"Desk top",0,85,192,32,C("71623d"));Box(desk,"Left leg",8,112,38,108,C("71623d"));Box(desk,"Right leg",146,112,38,108,C("71623d"));Box(desk,"Monitor stand",82,60,28,42,C("3b4155"));var monitor=Button(desk,"",49,0,94,69,()=>Open("trade"),C("737988"));var mr=monitor.GetComponent<RectTransform>();DecorateTradeMonitor(mr);
 var nav=Box(screen,"Bottom Navigation",0,774,480,80,C("090f1e"),C("56658c"));
 string[] names={"장착 관리","보관함","오버클럭","냉각","상점","포크"},pages={"gpu","inventory","oc","cool","shop","fork"};
 for(int i=0;i<6;i++){
  string key=pages[i];
  var button=Button(nav,"",5+i*79,7,74,65,()=>{pendingSlot=-1;Open(key);},C("56658c"),12);
  var tile=button.GetComponent<RectTransform>();
  var icon=Paint(Rect(tile,"Menu Icon",17,3,40,40),Color.white);
  icon.sprite=NavSprite(i);icon.preserveAspect=true;icon.raycastTarget=false;if(i==5){icon.enabled=false;ForkIcon(tile,25,6,24);}
  Text(tile,names[i],4,44,66,17,11,White,TextAnchor.MiddleCenter);
 }
 toast=Text(screen,"",20,714,440,48,13,White,TextAnchor.MiddleCenter);toast.outlineColor=Bg;toast.outlineWidth=.12f;toast.transform.SetAsLastSibling();toast.gameObject.SetActive(false);
 }
 void BuildWalletContents(RectTransform wallet){
  var badge=Rect(wallet,"Wallet coin",10,10,38,38);
  badge.gameObject.AddComponent<TradeCoinGraphic>().raycastTarget=false;
  coinIcon=Text(badge,"B",0,0,38,38,22,C("70400c"),TextAnchor.MiddleCenter);
  coinIcon.fontStyle=FontStyles.Bold;coinIcon.textWrappingMode=TextWrappingModes.NoWrap;
  coinIcon.enableAutoSizing=true;coinIcon.fontSizeMin=10;coinIcon.fontSizeMax=22;
  coinIcon.overflowMode=TextOverflowModes.Overflow;
  money=Text(wallet,"0",57,2,222,39,27,White,TextAnchor.MiddleLeft);
  money.textWrappingMode=TextWrappingModes.NoWrap;money.enableAutoSizing=true;
  money.fontSizeMin=12;money.fontSizeMax=27;money.overflowMode=TextOverflowModes.Ellipsis;
  coin=Text(wallet,"BTC-X 채굴 중",57,41,222,17,11,Cyan);
  rate=Text(wallet,"+1.0 / 초",287,18,142,28,14,Green,TextAnchor.MiddleRight);
 }
 void DecorateTradeMonitor(RectTransform monitor){
  var display=Box(monitor,"Trading display",6,6,82,57,C("0c202c"));
  display.GetComponent<Image>().raycastTarget=false;
  Box(display,"Header accent",5,4,14,1,Cyan).GetComponent<Image>().raycastTarget=false;
  Box(display,"Online light",72,4,4,2,Green).GetComponent<Image>().raycastTarget=false;
  for(int i=0;i<3;i++)Box(display,"Chart grid",42,16+i*9,33,1,C("193642")).GetComponent<Image>().raycastTarget=false;
  int[] heights={6,11,8,16,22};
  for(int i=0;i<heights.Length;i++)Box(display,"Chart bar",43+i*6,38-heights[i],4,heights[i],i==2?C("499cab"):Green).GetComponent<Image>().raycastTarget=false;
  var coin=Rect(display,"Gold coin",7,9,31,31);
  coin.gameObject.AddComponent<TradeCoinGraphic>().raycastTarget=false;
  var symbol=Text(coin,"B",0,0,31,31,20,C("70400c"),TextAnchor.MiddleCenter);
  symbol.fontStyle=FontStyles.Bold;symbol.overflowMode=TextOverflowModes.Overflow;
  Box(coin,"Coin upper stroke",14,4,2,4,C("70400c")).GetComponent<Image>().raycastTarget=false;
  Box(coin,"Coin lower stroke",14,23,2,4,C("70400c")).GetComponent<Image>().raycastTarget=false;
  Box(display,"Sell label background",4,42,74,12,C("183d38")).GetComponent<Image>().raycastTarget=false;
  var label=Text(display,"코인 판매",4,42,74,12,9,Green,TextAnchor.MiddleCenter);
  label.textWrappingMode=TextWrappingModes.NoWrap;label.overflowMode=TextOverflowModes.Overflow;
 }
 void Refresh(){if(money==null||g.S==null)return;var c=g.MiningCoin();coinIcon.text=new[]{"B","E","D","N","404","Q"}[Array.IndexOf(Catalog.Coins,c)];money.text=N(g.Balance(c.id));coin.text=c.name+" 채굴 중";rate.text="+"+D(g.TotalAuto())+" / 초";cash.text="₩ 현금 "+N(g.S.cash);fork.text="포크 "+g.S.fork;temp.text=Mathf.FloorToInt(g.Hottest())+"°C";temp.color=g.Hottest()>=85?Red:g.Hottest()>=65?Gold:White;power.text=g.TotalPower()+"W";bill.text="-₩"+g.ElectricityPerMinute()+"/분";fever.text=g.S.fever?"◆ 채굴 집중 | ×"+(g.HasFork("fever")?3:2)+" | "+g.S.feverTime+"초 ◆":"◆ 채굴 집중 | 준비 중 | "+g.S.feverCooldown+"초 ◆";
 string key="";for(int i=0;i<8;i++){var it=g.At(i);key+=it==null?"-":it.uid+":"+it.modelId+":"+it.coolerId;key+="|";}if(key!=rackKey){rackKey=key;BuildRack();}for(int i=0;i<rackTemps.Count;i++)if(rackTemps[i]!=null){var gpu=g.At(i);rackTemps[i].text=gpu==null?"":Mathf.FloorToInt(gpu.temp)+"°C";}foreach(var update in live.ToArray())update();}
void BuildRack(){foreach(Transform child in rack)Destroy(child.gameObject);fans.Clear();rackTemps.Clear();for(int i=0;i<8;i++){int slot=i;var gpu=g.At(i);float x=10+(i%2)*106,y=12+(i/2)*76;var b=Button(rack,gpu==null?"[추가하기]":"",x,y,98,68,gpu==null?(Action)(()=>{pendingSlot=slot;Open("shop");}):()=>{double gain=g.ClickMine(gpu.uid);if(gain>0)FloatGain(gain);},gpu==null?C("29412e"):C("414c61"),10);var r=b.GetComponent<RectTransform>();Box(r,"LED",9,63,80,2,Green).GetComponent<Image>().raycastTarget=false;if(gpu==null){rackTemps.Add(null);continue;}var model=Catalog.GPU(gpu.modelId);var body=Rect(r,"GPU body",4,14,90,37);GpuCardArt.Build(body,gpu.modelId,FanSprite(),fans,false);Text(r,model.name,5,2,89,13,8,White);rackTemps.Add(Text(r,"",61,51,31,11,8,Cyan,TextAnchor.MiddleRight));}}

 void FloatGain(double n){var t=Text(screen,"+"+N(n)+" "+g.MiningCoin().name,45,470,220,32,19,Green,TextAnchor.MiddleCenter);StartCoroutine(FloatRoutine(t));}
 IEnumerator FloatRoutine(Text t){float a=0;while(a<.9f){a+=Time.unscaledDeltaTime;t.rectTransform.anchoredPosition+=Vector2.up*35*Time.unscaledDeltaTime;t.color=new Color(Green.r,Green.g,Green.b,1-a/.9f);yield return null;}Destroy(t.gameObject);}
 void Toast(string s){if(toastRoutine!=null)StopCoroutine(toastRoutine);toastRoutine=StartCoroutine(ToastTime(s));}
 IEnumerator ToastTime(string s){toast.text=s;toast.transform.SetAsLastSibling();toast.gameObject.SetActive(true);yield return new WaitForSecondsRealtime(1.6f);toast.gameObject.SetActive(false);}
 float y; RectTransform Card(string title,string detail,float h=110,float detailHeight=-1){var r=Box(content,title,0,y,412,h,Dark,C("394765"));Text(r,title,10,7,392,26,14,Green);if(!string.IsNullOrEmpty(detail))Text(r,detail,10,37,392,detailHeight>=0?detailHeight:h-44,12,White,TextAnchor.UpperLeft);y+=h+9;return r;}
 void ActionCard(string title,string detail,string label,Action a,Color? accent=null,float h=158){var r=Card(title,detail,h,h-89);Button(r,label,10,h-44,392,34,a,accent);}
 void LiveText(Text t,Func<string> f){Action a=()=>{if(t!=null)t.text=f();};live.Add(a);a();}
 void Open(string type){if(!g.S.running&&type!="trade")return;Close(false);page=type;string title=type=="gpu"?"장착 관리":type=="inventory"?"장비 보관함":type=="oc"?"오버클럭":type=="cool"?"랙 냉각 연구소":type=="shop"?"장비 상점":type=="fork"?"포크 업그레이드":type=="news"?"채굴 경제신문":"코인 거래소";
 modal=Box(screen,"Modal",0,0,480,854,new Color(.01f,.02f,.05f,.94f));var box=Box(modal,"Popup",20,type=="news"?190:32,440,type=="news"?422:746,Panel,C("56658c"),3);Text(box,title,12,8,354,35,16,Cyan);var closeButton=Button(box,"X",387,8,40,35,()=>Close(),Red,18);var closeLabel=closeButton.GetComponentInChildren<Text>();closeLabel.fontStyle=FontStyles.Bold;closeLabel.textWrappingMode=TextWrappingModes.NoWrap;closeLabel.overflowMode=TextOverflowModes.Overflow;ScrollRect sc;content=Scroll(box,14,55,412,type=="news"?352:675,out sc);y=0;
 switch(type){case "gpu":GPU();break;case "inventory":Inventory();break;case "oc":OC();break;case "cool":Cool();break;case "shop":Shop();break;case "fork":Fork();break;case "trade":Trade();break;case "news":NewsContent();break;}content.sizeDelta=new Vector2(412,y+8);Refresh();}
 public void Close(bool reset=true){live.Clear();if(modal!=null){modal.gameObject.SetActive(false);Destroy(modal.gameObject);}modal=null;page="";if(reset){pendingSlot=-1;equipUid=null;coolTarget=null;}}
 string Certification(){int t=g.RigTier();if(t==8)return "QUANTUM 인증 완료 · 모든 GPU 등급 해금";var n=Catalog.GPUs[t+1];return "현재 "+Catalog.GPUs[t].name+" 인증 · 다음 "+n.name+"\n운영 "+g.Duration(g.S.runSeconds)+" / "+g.Duration(g.RequiredSeconds(n))+"\n누적 채굴 "+N(g.S.totalMined)+" / "+N(n.unlockMined);}
 void GpuTile(GpuItem item,string title,string label,Action action){
  var r=Card(title,"",166);var model=Catalog.GPU(item.modelId);
  var reading=Text(r,"",10,37,230,77,13);LiveText(reading,()=>model.name+"\n전력 "+model.power+"W\n온도 "+Mathf.FloorToInt(item.temp)+"°C");
  GpuCardArt.Build(Rect(r,"GPU Preview",250,38,152,76),item.modelId,FanSprite());
  Button(r,label,10,122,392,34,action);
 }
 void GPU(){
  Card("장착 현황",g.Equipped().Count+" / 8개 가동 · 자동 채굴 "+D(g.TotalAuto())+"/초\n총 전력 "+g.TotalPower()+"W · 장착 해제 및 업그레이드",92);
  Card("장비 인증",Certification(),123);
  var primary=g.At(0);var next=primary!=null&&primary.modelId<8?Catalog.GPUs[primary.modelId+1]:null;
  ActionCard("1번 슬롯 업그레이드",next==null?"1번 슬롯 GPU가 없거나 최종 등급입니다.":Catalog.GPU(primary.modelId).name+" → "+next.name+"\n₩"+N(Catalog.GPU(primary.modelId).upgradeCost),"업그레이드",next!=null?(Action)(()=>{g.UpgradePrimary();Open("gpu");}):null);
  for(int i=0;i<8;i++){int slot=i;var item=g.At(i);
   if(item==null)ActionCard("슬롯 "+(i+1),"비어 있음","GPU 구매",()=>{pendingSlot=slot;Open("shop");},Cyan,126);
   else GpuTile(item,"슬롯 "+(i+1)+" · "+Catalog.Cooler(item.coolerId).name,"보관함으로 이동",()=>{g.Unequip(item.uid);Open("gpu");});
  }
 }
 void Inventory(){
  Card("미장착 장비 보관함","보관 GPU "+(g.S.items.Count-g.Equipped().Count)+"개 · 쿨러 "+g.S.coolingItems.Count+"개\n장착된 GPU는 ‘장착 관리’에서 관리합니다.",96);
  if(g.S.items.Count==0)ActionCard("다시 시작할 장비가 없나요?","최종 장비를 잃었을 때 기본 GPU를 지원받을 수 있습니다.","기본 GPU 지원받기",()=>{g.EmergencyStarter();Open("inventory");});
  var selected=g.Find(equipUid);
  if(selected!=null){Card("장착 슬롯 선택",Catalog.GPU(selected.modelId).name+" · 기존 GPU는 보관함으로 이동합니다.",86);
   for(int i=0;i<8;i++){int slot=i;var old=g.At(i);Button(content,"슬롯 "+(i+1)+" · "+(old==null?"비어 있음":Catalog.GPU(old.modelId).name),(i%2)*210,y,202,50,()=>{g.Equip(selected.uid,slot);equipUid=null;Open("inventory");},old==null?Cyan:Gold,11);if(i%2==1)y+=58;}
   Button(content,"선택 취소",0,y,412,36,()=>{equipUid=null;Open("inventory");},Red);y+=45;
  }
  foreach(var item in g.S.items)if(item.slot<0)GpuTile(item,"보관 중","장착 위치 선택",()=>{equipUid=item.uid;Open("inventory");});
  if(g.S.items.Count==g.Equipped().Count)Card("보관 중인 GPU 없음","상점에서 추가 구매한 장비가 이곳에 표시됩니다.",83);
  foreach(var unit in g.S.coolingItems){bool active=g.S.activeCoolingUid==unit.uid;ActionCard(Catalog.Cooler(unit.coolerId).name,active?"현재 랙에 사용 중":"보관 중 · 장착하면 기존 랙 쿨러와 교체됩니다.",active?"사용 중":"랙에 장착",active?null:(Action)(()=>{g.EquipCooler(unit.uid,null);Open("inventory");}),Cyan,128);}
 }
 void OC(){ActionCard("오버클럭 상태 · "+(g.S.overclock?"활성":"꺼짐"),"현재 자동 채굴 "+D(g.TotalAuto())+"/초\n성능 증가 +"+(g.HasFork("oc")?50:30)+"%\n성능과 장착 GPU의 발열 속도가 함께 증가합니다.",g.S.overclock?"[ 오버클럭 끄기 ]":"[ 오버클럭 켜기 ]",()=>{g.ToggleOC();if(g.S.running)Open("oc");},Gold,180);}
 void CoolerArt(Transform parent,int level){
  var r=Box(parent,"Cooler illustration",270,41,125,70,C("172632"),Cyan);
  for(int i=0;i<15;i++)Box(r,"Radiator fin",7+i*7,7,2,56,C("607782")).GetComponent<Image>().raycastTarget=false;
  for(int i=0;i<2;i++){var fan=Paint(Rect(r,"Cooler fan",10+i*53,10,49,49),Color.white);fan.sprite=FanSprite();fan.raycastTarget=false;}
  Text(r,"냉각 "+Mathf.Min(4,level)+"행",3,48,119,19,10,Cyan,TextAnchor.MiddleCenter);
 }
 void Cool(){
  var status=Card("랙 냉각 시스템","",169);
  var reading=Text(status,"",10,39,392,73,13);LiveText(reading,()=>"쿨러 1개로 위쪽 행부터 함께 냉각합니다.\n1단계는 1행(2개), 최대 4행(8개) 적용.\n현재 "+g.CoolingRows()+"행 적용 · 최고 "+Mathf.FloorToInt(g.Hottest())+"°C");
  Button(status,"랙 전체 긴급 냉각",10,123,392,34,()=>g.EmergencyCool(),Cyan);
  for(int row=0;row<4;row++)Card((row+1)+"행 · 슬롯 "+(row*2+1)+"–"+(row*2+2),row<g.CoolingRows()?Catalog.Cooler(g.RackCoolerLevel()).name+" 적용 중":"기본 냉각 · 랙 쿨러 미적용",76);
  foreach(var cl in Catalog.Coolers){if(cl.id==0)continue;bool unlocked=g.CoolerUnlocked(cl);var owned=g.S.coolingItems.Find(x=>x.coolerId==cl.id);bool active=g.RackCoolerLevel()==cl.id;
   var card=Card(cl.name,"",185);CoolerArt(card,cl.id);
   Text(card,"최대 "+Mathf.Min(4,cl.id)+"행 · GPU "+(Mathf.Min(4,cl.id)*2)+"개\n냉각 -"+cl.rate.ToString("0.00")+"°C/초\n₩"+N(cl.cost)+"\n"+(unlocked?"구매 가능":"운영 "+g.Duration(Mathf.FloorToInt(cl.unlockSeconds*g.CycleFactor()))+" 필요"),10,38,250,100,12);
   Action action=active?null:owned!=null?(Action)(()=>{g.EquipCooler(owned.uid,null);Open("cool");}):unlocked&&g.S.cash>=cl.cost?(Action)(()=>{g.BuyCooler(null,cl.id);Open("cool");}):null;
   Button(card,active?"현재 사용 중":owned!=null?"보유 쿨러 장착":unlocked?"구매하고 랙에 장착":"인증 잠금",10,141,392,34,action,Cyan);
  }
 }
 void Shop(){
  Card("장비 인증",Certification()+"\n"+(pendingSlot>=0?"슬롯 "+(pendingSlot+1)+"에 바로 장착":"구매한 GPU는 보관함에 보관됩니다.")+"\n보유 ₩"+N(g.S.cash),158);
  foreach(var m in Catalog.GPUs){
   bool u=g.Certified(m);
   var card=Card(m.name,"",190);
   Func<string> detail=()=>"클릭 "+N(m.click)+" · 자동 "+D(m.autoMine)+"/초\n전력 "+m.power+"W\n가격 ₩"+N(g.GpuPrice(m.id))+"\n"+(g.Certified(m)?"해금됨":"운영 "+g.Duration(g.RequiredSeconds(m))+" + 누적 "+N(m.unlockMined));
   var details=Text(card,"",10,40,222,98,12,White,TextAnchor.UpperLeft);LiveText(details,detail);
   var preview=Rect(card,"GPU Preview",250,51,152,76);
   GpuCardArt.Build(preview,m.id,FanSprite());
   var purchase=Button(card,u?"구매":"잠금",10,146,392,34,()=>{if(g.BuyGpu(m.id,pendingSlot))pendingSlot=-1;Open("shop");});live.Add(()=>{bool certified=g.Certified(m);purchase.interactable=certified&&g.S.cash>=g.GpuPrice(m.id);purchase.GetComponentInChildren<Text>().text=certified?"구매":"잠금";});
  }
 }
 static readonly string[] ids={"root","cooling","click","auto","oc","fork","fever"},nodeNames={"채굴 코어","열 차단막","클릭 강화","자동 채굴","안정적 오버클럭","채굴 숙련","집중 채굴"},req={"","root","root","root","click","auto","cooling"},desc={"모든 포크 업그레이드의 시작점입니다.","모든 장착 GPU의 발열을 20% 감소시키고 냉각을 강화합니다.","GPU 클릭 채굴량이 25% 증가합니다.","랙 전체 자동 채굴량이 30% 증가합니다.","오버클럭 보너스를 +30%에서 +50%로 강화합니다.","클릭 및 자동 채굴 수익이 영구적으로 10% 증가합니다.","집중 채굴 배율을 ×2에서 ×3으로, 지속시간을 25초로 강화합니다."};
 void ForkIcon(Transform parent,float x,float y,float size){
  var r=Rect(parent,"Fork symbol",x,y,size,size);
  Line(r,new Vector2(size*.5f,size*.9f),new Vector2(size*.5f,size*.5f),Cyan,3);
  Line(r,new Vector2(size*.5f,size*.5f),new Vector2(size*.15f,size*.15f),Cyan,3);
  Line(r,new Vector2(size*.5f,size*.5f),new Vector2(size*.85f,size*.15f),Cyan,3);
  foreach(var pos in new[]{new Vector2(.15f,.15f),new Vector2(.85f,.15f),new Vector2(.5f,.9f)})Box(r,"Node",pos.x*size-3,pos.y*size-3,6,6,Gold).GetComponent<Image>().raycastTarget=false;
 }
 void Fork(){
  ActionCard("환생 · 완료 "+g.S.rebirths+"회","최종 GPU "+g.QuantumCount()+" / "+g.RebirthReq()+"개\n운영 "+g.Duration(g.S.runSeconds)+" / "+g.Duration(g.RebirthSeconds()),"환생 조건 확인",RebirthDialog,C("d383ff"),158);
  var heading=Card("영구 업그레이드","보유 포크 "+g.S.fork+" · 각 업그레이드 비용 1",90);ForkIcon(heading,361,17,29);
  for(int i=1;i<ids.Length;i++){int k=i;bool have=g.HasFork(ids[k]),ready=g.HasFork(req[k]);
   var r=Card(nodeNames[k],"",164);Box(r,"Accent",0,0,4,164,have?Green:ready?Cyan:Muted);ForkIcon(r,364,10,24);
   Text(r,desc[k]+"\n"+(have?"영구 적용 중":"선행: "+nodeNames[Array.IndexOf(ids,req[k])])+" · 비용 1 포크",12,40,385,71,13);
   Button(r,have?"해금 완료":!ready?"선행 업그레이드 필요":g.S.fork<1?"포크 부족":"업그레이드 해금",12,120,388,34,!have&&ready&&g.S.fork>=1?(Action)(()=>{g.BuyFork(ids[k],req[k]);Open("fork");}):null,Cyan);
  }
 }
 void RebirthDialog(){
  Close();modal=Box(screen,"Rebirth confirmation",0,0,480,854,new Color(.01f,.02f,.05f,.96f));var r=Box(modal,"Conditions",25,180,430,420,Panel,Cyan);
  bool ready=g.CanRebirth();Text(r,ready?"환생할 준비가 되었습니다":"조건을 달성하지 못했습니다",18,15,394,45,20,ready?Green:Gold);
  Text(r,"최종 GPU 장착: "+g.QuantumCount()+" / "+g.RebirthReq()+"개\n운영 시간: "+g.Duration(g.S.runSeconds)+" / "+g.Duration(g.RebirthSeconds())+"\n남은 시간: "+Mathf.CeilToInt(Mathf.Max(0,g.RebirthSeconds()-g.S.runSeconds)/60f)+"분\n\n초기화: 현금, 코인, GPU, 쿨러, 회차 기록\n유지: 환생 횟수, 포크, 영구 업그레이드",20,78,390,216,15);
  if(ready)Button(r,"환생하기 · 포크 +1",20,308,390,42,()=>{g.Rebirth();Open("fork");},Green);
  Button(r,ready?"취소":"확인",20,363,390,40,()=>Open("fork"),Cyan);
 }
 void Line(Transform p,Vector2 a,Vector2 b,Color c,float width){Vector2 delta=b-a;var r=Box(p,"Line",a.x,a.y,delta.magnitude,width,c);r.pivot=new Vector2(0,.5f);r.localRotation=Quaternion.Euler(0,0,-Mathf.Atan2(delta.y,delta.x)*Mathf.Rad2Deg);r.GetComponent<Image>().raycastTarget=false;}
 void Trade(){Card("코인 선택","상위 GPU 구매/업그레이드로 새 코인 해금\n현재 채굴: "+g.MiningCoin().name,86);for(int i=0;i<Catalog.Coins.Length;i++){var c=Catalog.Coins[i];bool u=g.CoinUnlocked(c);float x=(i%2)*210;var tile=Box(content,c.name,x,y,202,112,Dark,g.S.tradeCoinId==c.id?Cyan:C("405071"));Text(tile,c.name,8,6,186,23,13,Cyan);var quote=Text(tile,"",8,31,186,39,11);LiveText(quote,()=>u?"₩"+market.Price(c.id).ToString("0.000")+" · 보유 "+D(g.Balance(c.id)):Catalog.GPU(c.unlockGpu).name+" 보유 시 해금");Button(tile,"차트",8,75,88,29,u?(Action)(()=>{g.SetTradeCoin(c.id);Open("trade");}):null,Cyan,10);Button(tile,g.S.miningCoinId==c.id?"채굴중":"채굴",104,75,90,29,u?(Action)(()=>{g.SetMiningCoin(c.id);Open("trade");}):null,null,10);if(i%2==1)y+=120;}
 g.EnsureContract();var q=g.S.contract;var contract=Card("채굴 계약 · 완료 "+g.S.contractsCompleted,"",155);var qt=Text(contract,"",10,37,392,68,12);LiveText(qt,()=>(q.type=="mine"?"채굴":"판매")+" 계약 · "+Catalog.Coin(q.coinId).name+"\n"+N(q.progress)+" / "+N(q.target)+" · 보상 ₩"+N(q.reward));var claim=Button(contract,"[ 계약 보상 받기 ]",10,110,392,34,()=>{g.ClaimContract();Open("trade");},C("d383ff"));live.Add(()=>claim.interactable=q.progress>=q.target);
 string id=g.S.tradeCoinId;var chartBox=Card(Catalog.Coin(id).name+" / 원 · 5초봉","",275);var price=Text(chartBox,"",10,32,392,27,17,Gold);LiveText(price,()=>"₩"+market.Price(id).ToString("0.000"));var chart=Rect(chartBox,"Candlestick Chart",10,65,392,165).gameObject.AddComponent<CandleChart>();chart.market=market;chart.coinId=id;var stats=Text(chartBox,"",10,233,392,33,11,Muted);LiveText(stats,()=>"다음 시세 "+Mathf.CeilToInt(market.Countdown)+"초 · 보유 "+D(g.Balance(id))+"\n전량 판매 예상 ₩"+N(g.Balance(id)*market.Price(id)));live.Add(()=>chart.SetVerticesDirty());
 Text(content,"판매할 "+Catalog.Coin(id).name+" 수량",0,y,412,26,13);y+=30;var inputRect=Box(content,"판매 수량",0,y,412,45,Dark,Cyan);var input=inputRect.gameObject.AddComponent<TMP_InputField>();input.textComponent=Text(inputRect,"",10,4,392,37,17);input.placeholder=Text(inputRect,"판매 수량 입력",10,4,392,37,14,Muted);input.textViewport=inputRect;input.contentType=TMP_InputField.ContentType.DecimalNumber;y+=54;var estimate=Text(content,"",0,y,412,30,12,Gold);y+=38;LiveText(estimate,()=>{double amount;return double.TryParse(input.text,NumberStyles.Float,CultureInfo.InvariantCulture,out amount)&&!double.IsInfinity(amount)&&!double.IsNaN(amount)?"예상 수령 ₩"+N(amount*market.Price(id)):"수량을 입력하세요.";});Button(content,"[ 입력 수량 판매 ]",0,y,412,40,()=>{double a;if(double.TryParse(input.text,NumberStyles.Float,CultureInfo.InvariantCulture,out a)&&g.Sell(id,a))Open("trade");else Toast("판매 수량과 보유 코인을 확인하세요.");},Cyan);y+=49;var all=Button(content,"[ "+Catalog.Coin(id).name+" 전량 판매 ]",0,y,412,40,()=>{g.Sell(id,g.Balance(id));Open("trade");});live.Add(()=>all.interactable=g.Balance(id)>0);y+=49;}
 void News(string message){Open("news");}
 void NewsContent(){
  var paper=Box(content,"News newspaper",0,0,412,237,C("eee8d9"),C("9f947d"));
  Text(paper,"채굴 경제신문",14,10,384,33,24,C("263239"));Box(paper,"Rule",14,49,384,2,C("263239"));
  Text(paper,market.NewsCategory+" · 게임 속 가상 소식",14,58,384,24,12,C("53656a"));
  Text(paper,market.NewsTitle,14,90,384,54,20,C("203438"));
  Text(paper,market.NewsBody,14,151,384,76,14,C("34484a"),TextAnchor.UpperLeft);
  y=249;Button(content,market.NewsCategory=="장비 소식"?"상점 보기":market.NewsCategory=="전력 소식"?"장착 관리 보기":"거래소 보기",0,y,412,40,()=>Open(market.NewsCategory=="장비 소식"?"shop":market.NewsCategory=="전력 소식"?"gpu":"trade"),Gold);y+=48;Button(content,"확인",0,y,412,40,()=>Close(),Cyan);y+=44;
 }
 void GameOver(){
  Close();StopIntro();intro=Box(screen,"Repair choice",0,0,480,854,new Color(.03f,.01f,.02f,.98f));var b=Box(intro,"Overheat",25,175,430,460,Panel,Red,3);
  Text(b,"과열! 채굴을 일시 중지했어요",18,18,394,48,21,Red,TextAnchor.MiddleCenter);
  Text(b,"수리하면 모든 GPU를 보존하고 30°C로 냉각합니다.\n\n수리비 ₩"+N(g.RepairCost())+" · 보유 ₩"+N(g.S.cash)+"\n\n수리를 포기하면 장착 GPU 중 무작위 1개가 삭제됩니다. 현금·코인·다른 장비는 유지됩니다.",20,83,390,176,15);
  Button(b,"수리하고 계속하기",20,277,390,42,g.S.cash>=g.RepairCost()?(Action)(()=>{if(g.ResolveOverheat(true))StopIntro();}):null,Green);
  Button(b,"보유 코인 전량 판매",20,333,390,42,()=>{foreach(var c in Catalog.Coins)g.Sell(c.id,g.Balance(c.id));GameOver();},Gold);
  Button(b,"수리 포기 · 무작위 GPU 1개 삭제",20,389,390,42,()=>{g.ResolveOverheat(false);StopIntro();if(g.S.items.Count==0)Open("inventory");},Red,12);
 }
 // Original opening and tutorial copy, retained from index.html.
 static readonly string[] cutLabels={"시스템 시작","장비 가동","시장 연결","장비 확장","최종 목표"},cutTitles={"낡은 채굴실","첫 번째 GPU","가상 코인 시장 접속","더 강한 장비","목표: QUANTUM"},cutArt={"●\n전력 꺼짐\n...\n신호 감지","▣\nRTX 1090\n온도 30°C\n상태: 가동 중","₿ Ξ Ð ✦ Q\n시장 연결 : 연결됨\n시세 변동 : 활성","▣  ▣  ▣\nGPU / 냉각 / 전력\n나만의 채굴실","⚛\nRTX 6090 QUANTUM\n환생 시스템\n잠김"},cutText={"불 꺼진 작은 방.\n책상 위에는 오래된 채굴 장비 한 대만 남아 있다.","전원을 넣자 RTX 1090의 팬이 천천히 돌기 시작한다.\n이 한 장이 모든 것의 시작이다.","채굴한 코인은 시장에서 현금으로 바꿀 수 있다.\n하지만 시세는 계속 움직이고, 어떤 코인은 갑자기 폭등하거나 급락한다.","더 좋은 GPU를 사고, 냉각을 강화하고, 새로운 코인을 해금하라.\n작은 랙은 점점 거대한 채굴 시스템으로 변한다.","최종 목표는 RTX 6090 QUANTUM과 환생 프로토콜.\n지금부터 첫 번째 채굴을 시작한다."};
 static readonly string[] tutTitles={"1. GPU를 눌러 채굴","2. 코인을 현금으로 판매","3. GPU 업그레이드","4. 온도와 GPU 쿨링","5. 전력과 오버클럭","6. 계약과 시장 이벤트","7. 환생과 포크"},tutText={"랙에 장착된 GPU를 클릭하면 코인을 채굴합니다.\n\nGPU가 여러 장이면 클릭 한 번에 장착된 GPU가 모두 같이 채굴합니다.","방 오른쪽 컴퓨터를 누르면 거래소가 열립니다.\n\n채굴한 코인을 현재 게임 시세로 팔아 ₩ 현금을 만들고, 그 돈으로 장비를 구매합니다.","[상점]에서 GPU를 구매하거나 [장착 관리]에서 1번 슬롯 GPU를 업그레이드할 수 있습니다.\n\n상위 GPU를 보유하면 ETHER-X, DOGE-X 같은 새로운 코인도 순서대로 해금됩니다.","채굴과 오버클럭은 GPU 온도를 올립니다.\n\n[냉각]에서 랙 쿨러를 장착하세요. 1단계는 1행, 최대 4행을 냉각합니다. 100°C가 되면 수리비를 내거나 GPU 1개를 포기할 수 있습니다.","[오버클럭]는 채굴 성능을 높이지만 발열도 증가합니다.\n\nGPU 전력 사용량에 따라 실제 게임 현금에서 전기요금이 주기적으로 차감됩니다.","거래소에는 채굴/판매 계약이 있습니다. 목표를 완료하면 현금 보상을 받을 수 있습니다.\n\n랜덤 이벤트로 해금된 코인 시세, 전기요금, GPU 구매 가격이 일시적으로 변합니다.","최종 RTX 6090 QUANTUM을 준비하고 환생 조건을 달성하면 포크를 얻습니다.\n\n환생하면 이번 회차 장비는 초기화되지만 포크 업그레이드는 남아 다음 회차 성장을 빠르게 해줍니다."};
 void StopIntro(){if(typing!=null)StopCoroutine(typing);typingNow=false;if(intro!=null){intro.gameObject.SetActive(false);Destroy(intro.gameObject);}intro=null;g.IntroPaused=false;}
 void Opening(bool rp){if(!g.S.running)return;Close();replay=rp;introIndex=0;RenderOpening();}
 void RenderOpening(){StopIntro();g.IntroPaused=true;intro=Box(screen,"Opening",0,0,480,854,C("02040a"));var b=Box(intro,"Cutscene",25,148,430,535,Dark,C("303a50"),3);Text(b,cutLabels[introIndex],12,8,300,28,12,Cyan);Button(b,"건너뛰기",347,8,70,29,FinishOpening,Red,10);Box(b,"Visual",3,44,424,220,introIndex==2?C("241b39"):introIndex==4?C("123e3a"):C("14283e"));Text(b,cutArt[introIndex],18,68,394,176,22,Green,TextAnchor.MiddleCenter);Text(b,cutTitles[introIndex],16,280,398,33,18,Gold);typed=Text(b,"",16,321,398,125,16);fullCaption=cutText[introIndex];typing=StartCoroutine(TypeCaption());Text(b,new string('■',introIndex+1)+new string('□',4-introIndex),16,466,185,40,13,Cyan);Button(b,introIndex==4?"[ 채굴 시작 ]":"[ 계속 ]",260,470,151,42,()=>{if(typingNow){StopCoroutine(typing);typed.text=fullCaption;typingNow=false;}else if(introIndex<4){introIndex++;RenderOpening();}else FinishOpening();});}
 IEnumerator TypeCaption(){typingNow=true;for(int i=0;i<=fullCaption.Length;i++){typed.text=fullCaption.Substring(0,i);yield return new WaitForSecondsRealtime(.022f);}typingNow=false;}
 void FinishOpening(){PlayerPrefs.SetInt("webport_opening",1);PlayerPrefs.Save();StopIntro();if(!replay&&PlayerPrefs.GetInt("webport_tutorial",0)==0)Tutorial();}
 void Tutorial(){if(!g.S.running)return;Close();introIndex=0;RenderTutorial();}
 void RenderTutorial(){StopIntro();g.IntroPaused=true;intro=Box(screen,"Tutorial",0,0,480,854,new Color(.01f,.02f,.05f,.97f));var b=Box(intro,"Tutorial Card",30,155,420,534,Panel,Cyan,4);Text(b,"게임 안내 "+(introIndex+1)+" / 7",16,13,263,32,12,Cyan);Button(b,"건너뛰기",306,14,98,32,FinishTutorial,Red,11);Text(b,tutTitles[introIndex],20,85,380,55,21,Green,TextAnchor.MiddleCenter);Text(b,tutText[introIndex],26,158,368,231,17);Text(b,new string('■',introIndex+1)+new string('□',6-introIndex),20,407,380,28,14,Cyan,TextAnchor.MiddleCenter);Button(b,"[ 이전 ]",20,463,180,47,introIndex>0?(Action)(()=>{introIndex--;RenderTutorial();}):null,Cyan);Button(b,introIndex==6?"[ 시작 ]":"[ 다음 ]",220,463,180,47,()=>{if(introIndex<6){introIndex++;RenderTutorial();}else FinishTutorial();});}
 void FinishTutorial(){PlayerPrefs.SetInt("webport_tutorial",1);PlayerPrefs.Save();StopIntro();Toast("튜토리얼 완료! GPU를 클릭해 시작하세요.");}
}
[RequireComponent(typeof(CanvasRenderer))]
public class TradeCoinGraphic:MaskableGraphic {
 protected override void OnPopulateMesh(VertexHelper vh){
  vh.Clear();var bounds=rectTransform.rect;float radius=Mathf.Min(bounds.width,bounds.height)*.5f;
  Disc(vh,bounds.center,radius,new Color(.48f,.26f,.05f));
  Disc(vh,bounds.center+Vector2.up*.8f,radius*.92f,new Color(1f,.82f,.35f));
  Disc(vh,bounds.center,radius*.76f,new Color(.79f,.46f,.08f));
  Disc(vh,bounds.center,radius*.67f,new Color(1f,.71f,.19f));
 }
 static void Disc(VertexHelper vh,Vector2 center,float radius,Color color){
  int start=vh.currentVertCount;vh.AddVert(center,color,Vector2.zero);
  for(int i=0;i<=64;i++){float angle=i*Mathf.PI*2/64;vh.AddVert(center+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*radius,color,Vector2.zero);if(i>0)vh.AddTriangle(start,start+i,start+i+1);}
 }
}
[RequireComponent(typeof(CanvasRenderer))]
public class CandleChart:MaskableGraphic {
 public MarketManager market;public string coinId;
 protected override void OnPopulateMesh(VertexHelper vh){vh.Clear();if(market==null)return;var series=market.Series(coinId);if(series==null||series.candles.Count==0)return;var cs=series.candles;float lo=float.MaxValue,hi=float.MinValue;foreach(var c in cs){lo=Mathf.Min(lo,c.low);hi=Mathf.Max(hi,c.high);}float pad=Mathf.Max(Catalog.Coin(coinId).basePrice*.08f,(hi-lo)*.12f);lo-=pad;hi+=pad;float w=rectTransform.rect.width,h=rectTransform.rect.height;Func<float,float> y=p=>h*(p-lo)/(hi-lo);for(int i=0;i<5;i++)Quad(vh,0,h*i/4,w,1,new Color(.21f,.23f,.28f));float step=w/cs.Count;for(int i=0;i<cs.Count;i++){var c=cs[i];float x=(i+.5f)*step;Color color=c.close>=c.open?new Color(1,.38f,.36f):new Color(.31f,.60f,1);Quad(vh,x,y(c.low),1,Mathf.Max(1,y(c.high)-y(c.low)),color);Quad(vh,x-step*.3f,y(Mathf.Min(c.open,c.close)),Mathf.Max(1,step*.6f),Mathf.Max(1,Mathf.Abs(y(c.close)-y(c.open))),color);}}
 void Quad(VertexHelper vh,float x,float y,float w,float h,Color c){int n=vh.currentVertCount;Vector2 o=rectTransform.rect.min;vh.AddVert(new Vector3(x+o.x,y+o.y),c,Vector2.zero);vh.AddVert(new Vector3(x+w+o.x,y+o.y),c,Vector2.zero);vh.AddVert(new Vector3(x+w+o.x,y+h+o.y),c,Vector2.zero);vh.AddVert(new Vector3(x+o.x,y+h+o.y),c,Vector2.zero);vh.AddTriangle(n,n+1,n+2);vh.AddTriangle(n,n+2,n+3);}
}
}
