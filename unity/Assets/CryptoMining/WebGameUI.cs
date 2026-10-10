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
using UnityEngine.InputSystem;
namespace CryptoMining {
public partial class WebGameUI:MonoBehaviour {
 GameManager g; MarketManager market; TMP_FontAsset font; Canvas canvas; RectTransform screen,body,modal,content,intro; Text money,coinIcon,coin,rate,cash,fork,temp,power,bill,fever,toast; float uiTick;
 readonly List<Action> live=new List<Action>(); readonly List<RectTransform> fans=new List<RectTransform>(); readonly List<Text> rackTemps=new List<Text>(); string rackKey="",page="",coolTarget=null,equipUid=null,selectedNode="root"; int selectedRack,pendingSlot=-1; Coroutine toastRoutine,typing; Text typed; string fullCaption; bool typingNow; int introIndex; bool replay;
 readonly List<RectTransform> roomRacks=new List<RectTransform>();
 RectTransform roomFrame,roomWorld;RoomPan roomPan;string roomKey="";
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
 void Start(){ApplyDisplaySettings();font=CreateUiFont();Build();g.Changed+=Refresh;g.Toast+=Toast;g.Overheated+=GameOver;market.Changed+=Refresh;market.Event+=News;market.ModalOpen=()=>modal!=null||intro!=null;Refresh();if(g.SaveBlocked)StorageIssue();else if(!g.S.running)GameOver();else if(PlayerPrefs.GetInt("webport_opening",0)==0)Opening(false);else if(PlayerPrefs.GetInt("webport_tutorial",0)==0)Tutorial();}
 void OnDestroy(){if(g!=null){g.Changed-=Refresh;g.Toast-=Toast;g.Overheated-=GameOver;}if(market!=null){market.Changed-=Refresh;market.Event-=News;}if(canvas!=null)Destroy(canvas.gameObject);}
 void Update(){if(Keyboard.current!=null&&Keyboard.current.escapeKey.wasPressedThisFrame)HandleBack();float hottest=g.Hottest();if(modal==null&&intro==null&&g.S.running&&PlayerPrefs.GetInt("mining_fan_animation",1)==1){float speed=hottest>=65?720:280;float step=-speed*Time.unscaledDeltaTime;foreach(var f in fans)if(f!=null)f.Rotate(0,0,step);}uiTick+=Time.unscaledDeltaTime;if(uiTick>=.2f){uiTick=0;Refresh();}}
 RectTransform Rect(Transform p,string name,float x,float y,float w,float h){var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(p,false);r.anchorMin=r.anchorMax=new Vector2(0,1);r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);return r;}
 Image Paint(RectTransform r,Color c){var i=r.gameObject.AddComponent<Image>();i.color=c;i.raycastTarget=false;return i;}
 RectTransform Box(Transform p,string name,float x,float y,float w,float h,Color c,Color? border=null,int bw=2){var r=Rect(p,name,x,y,w,h);Paint(r,border??c);if(border.HasValue){var fill=Rect(r,"Fill",bw,bw,w-bw*2,h-bw*2);Paint(fill,c).raycastTarget=false;}return r;}
 Text Text(Transform p,string value,float x,float y,float w,float h,int size=13,Color? color=null,TextAnchor align=TextAnchor.MiddleLeft){var r=Rect(p,value.Length>24?value.Substring(0,24):value,x,y,w,h);var t=r.gameObject.AddComponent<Text>();t.font=font;t.text=value;t.fontSize=size;t.color=color??White;t.alignment=ToTmpAlignment(align);t.raycastTarget=false;t.textWrappingMode=TextWrappingModes.Normal;t.overflowMode=TextOverflowModes.Ellipsis;t.enableAutoSizing=true;t.fontSizeMin=size*.85f;t.fontSizeMax=size;return t;}
 Button Button(Transform p,string label,float x,float y,float w,float h,Action action,Color? accent=null,int size=12){var c=accent??Green;var r=Box(p,label,x,y,w,h,C("1b3040"),c);var image=r.GetComponent<Image>();image.raycastTarget=true;var b=r.gameObject.AddComponent<Button>();b.targetGraphic=image;b.interactable=action!=null;if(action!=null)b.onClick.AddListener(()=>action());var colors=b.colors;colors.disabledColor=new Color(.35f,.35f,.35f,.6f);colors.pressedColor=new Color(.6f,.7f,.8f);b.colors=colors;Text(r,label,4,2,w-8,h-4,size,White,TextAnchor.MiddleCenter);return b;}
 void Fill(RectTransform r){r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;}
 RectTransform Scroll(Transform p,float x,float y,float w,float h,out ScrollRect sc){var outer=Rect(p,"Scroll",x,y,w,h);sc=outer.gameObject.AddComponent<ScrollRect>();var vp=Rect(outer,"Viewport",0,0,w,h);Paint(vp,new Color(0,0,0,0)).raycastTarget=true;vp.gameObject.AddComponent<RectMask2D>();var cr=Rect(vp,"Content",0,0,w,0);sc.viewport=vp;sc.content=cr;sc.horizontal=false;sc.movementType=ScrollRect.MovementType.Clamped;sc.scrollSensitivity=35;return cr;}
 void Build(){if(FindFirstObjectByType<EventSystem>()==null)new GameObject("EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule));var cv=new GameObject("Web Game Canvas",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));canvas=cv.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.pixelPerfect=false;var scaler=cv.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(480,854);scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.Expand;var backdrop=Rect(cv.transform,"Backdrop",0,0,480,854);Fill(backdrop);Paint(backdrop,C("0b101d")).raycastTarget=true;screen=Box(cv.transform,"Mobile 480 x 854",0,0,480,854,Bg);screen.anchorMin=screen.anchorMax=new Vector2(.5f,.5f);screen.pivot=new Vector2(.5f,.5f);screen.anchoredPosition=Vector2.zero;var safe=cv.AddComponent<MobileSafeArea>();safe.target=screen;safe.canvas=canvas;
 body=Box(screen,"Fixed HUD and room",0,0,480,774,C("303a54"));for(int i=0;i<35;i++){Box(body,"Grid",i*14,0,1,817,new Color(1,1,1,.025f));}for(int i=0;i<59;i++)Box(body,"Grid",0,i*14,480,1,new Color(1,1,1,.025f));
 var music=GetComponent<MiningMusic>();if(music==null)music=gameObject.AddComponent<MiningMusic>();Button(body,"설정",10,10,76,44,()=>Open("settings"),Cyan,14);Button(body,"다음 목표",94,10,90,44,()=>Open("goals"),Gold,12);Text(body,"암호화폐 마이닝",192,10,180,44,16,White,TextAnchor.MiddleCenter);Button(body,"▶",378,10,42,44,()=>Opening(true),Gold);Button(body,"?",428,10,42,44,Tutorial,Cyan,16);
 var hud=Box(body,"HUD",9,66,462,234,C("253150"),C("56658c"),3);var wallet=Box(hud,"Wallet",10,10,442,62,C("1b2440"),C("425078"));BuildWalletContents(wallet);
 var f=Box(hud,"Fever",10,80,442,34,C("684113"),C("f2b451"));fever=Text(f,"",5,2,432,30,12,Gold,TextAnchor.MiddleCenter);
 var cashBox=Box(hud,"Cash",10,122,218,34,Dark,C("465375"));cash=Text(cashBox,"",10,3,198,28,13);var forkBox=Box(hud,"Fork",234,122,218,34,Dark,C("465375"));ForkIcon(forkBox,10,8,17);fork=Text(forkBox,"",32,3,176,28,13,Green,TextAnchor.MiddleRight);
 var t=Box(hud,"Temp",10,164,143,60,Dark,C("465375"));Text(t,"온도",8,5,127,18,11,Muted);temp=Text(t,"",8,25,127,26,15);
 var p=Box(hud,"Power",160,164,142,60,Dark,C("465375"));Text(p,"전력",8,5,126,18,11,Muted);power=Text(p,"",8,25,126,26,15,Gold);
 var e=Box(hud,"Electricity",309,164,143,60,Dark,C("465375"));Text(e,"전기요금",8,5,127,18,10,Muted);bill=Text(e,"",8,25,127,26,13,Red);
 roomFrame=Rect(body,"Room frame",9,309,462,456);BuildRoom();
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
 public void HandleBack(){
  if(g.SaveBlocked)return;
  if(modal!=null){Close();return;}
  if(intro!=null){if(!g.S.running)return;StopIntro();return;}
  Open("settings");
 }
 void StorageIssue(){
  Close();StopIntro();g.IntroPaused=true;intro=Box(screen,"Storage issue",0,0,480,854,Bg);intro.GetComponent<Image>().raycastTarget=true;
  Text(intro,"진행 상황을 보호하고 있어요",30,200,420,50,23,Gold,TextAnchor.MiddleCenter);
  Text(intro,g.SaveNotice+"\n\n앱을 삭제하거나 데이터를 지우지 마세요. 정상 파일을 복원한 뒤 다시 시도할 수 있습니다.",40,275,400,185,17);
  Button(intro,"다시 읽기",40,500,400,45,()=>{g.Load();StopIntro();if(g.SaveBlocked)StorageIssue();else if(!g.S.running)GameOver();else Refresh();},Cyan);
 }
 void ApplyDisplaySettings(){Application.targetFrameRate=PlayerPrefs.GetInt("mining_fps",30)==60?60:30;}
 void VolumeCard(string title,Func<int> value,Action<int> set,Func<bool> muted,Action toggle){
  var card=Card(title,"",144);var status=Text(card,"",10,37,392,25,12,White);
  var mute=Button(card,"",302,7,100,28,()=>{toggle();Refresh();},Cyan,11);
  LiveText(status,()=>muted()?"음소거 · 저장된 음량 "+value()+"%":"음량 "+value()+"% · 선택 즉시 적용");
  LiveText(mute.GetComponentInChildren<Text>(),()=>muted()?"소리 켜기":"음소거");
  var area=Rect(card,"Volume slider",10,76,392,54);Paint(area,Color.clear).raycastTarget=true;
  Box(area,"Track",14,21,364,12,C("394765"));
  var fillArea=Rect(area,"Fill area",14,21,364,12);var fill=Rect(fillArea,"Fill",0,0,364,12);Paint(fill,Cyan);Fill(fill);
  var handleArea=Rect(area,"Handle area",14,6,364,42);var handle=Rect(handleArea,"Handle",0,0,28,42);
  handle.anchorMin=handle.anchorMax=new Vector2(0,.5f);handle.pivot=new Vector2(.5f,.5f);handle.anchoredPosition=Vector2.zero;handle.sizeDelta=new Vector2(28,0);
  var handleImage=Paint(handle,White);handleImage.raycastTarget=true;
  var slider=area.gameObject.AddComponent<Slider>();slider.minValue=0;slider.maxValue=100;slider.wholeNumbers=true;slider.direction=Slider.Direction.LeftToRight;
  slider.fillRect=fill;slider.handleRect=handle;slider.targetGraphic=handleImage;slider.SetValueWithoutNotify(value());
  slider.onValueChanged.AddListener(v=>{set(Mathf.RoundToInt(v));if(muted())toggle();Refresh();});
  live.Add(()=>{if(slider!=null)slider.SetValueWithoutNotify(value());});
 }
 void Settings(){
  var audio=GetComponent<MiningMusic>();
  VolumeCard("배경음악",()=>audio.MusicVolume,audio.SetMusicVolume,()=>audio.Muted,audio.Toggle);
  VolumeCard("효과음",()=>audio.EffectsVolume,audio.SetEffectsVolume,()=>audio.EffectsMuted,audio.ToggleEffects);
  var display=Card("화면 부드러움","30FPS: 배터리 절약 · 60FPS: 더 부드러운 움직임\n기기에 따라 발열과 배터리 사용량이 달라집니다.",154,49);
  foreach(int fps in new[]{30,60}){int choice=fps;var button=Button(display,"",fps==30?10:211,96,191,44,()=>{PlayerPrefs.SetInt("mining_fps",choice);PlayerPrefs.Save();ApplyDisplaySettings();Refresh();},Cyan);LiveText(button.GetComponentInChildren<Text>(),()=> (PlayerPrefs.GetInt("mining_fps",30)==choice?"선택됨 · ":"")+choice+"FPS");}
  var animation=Card("팬 애니메이션","팬 회전 표시만 조절합니다. 채굴량과 온도는 동일합니다.",132,35);
  var toggle=Button(animation,"",10,77,392,44,()=>{PlayerPrefs.SetInt("mining_fan_animation",PlayerPrefs.GetInt("mining_fan_animation",1)==1?0:1);PlayerPrefs.Save();Refresh();},Cyan);
  LiveText(toggle.GetComponentInChildren<Text>(),()=>PlayerPrefs.GetInt("mining_fan_animation",1)==1?"팬 회전 켜짐 · 눌러서 끄기":"팬 회전 꺼짐 · 눌러서 켜기");
  Card("저장 상태",string.IsNullOrEmpty(g.SaveNotice)?"진행 상황은 기기에 자동 저장됩니다.\n설정은 자동 저장되며 다음 실행에도 유지됩니다.\n앱 삭제 시 진행 상황과 설정이 사라질 수 있습니다.":g.SaveNotice,128);
  ActionCard("게임 안내","조작과 성장 방법을 다시 확인합니다.","튜토리얼 보기",Tutorial,Cyan,125);
  ActionCard("개인정보 및 문의","개발자 muibeu · seoghunjo08@gmail.com","개인정보 안내 보기",()=>Open("privacy"),Cyan,128);
  Card("게임 정보","암호화폐 마이닝 · "+Application.version+"\n가상 코인을 사용하는 채굴 경영 게임입니다.\n실제 암호화폐를 채굴하거나 현금으로 환전하지 않습니다.",120);
 }
 void Privacy(){
  Card("개발자 및 연락처","muibeu\n문의: seoghunjo08@gmail.com",100);
  Card("기기에 저장되는 정보","게임 진행 상황과 소리·화면 설정은 기기에 저장됩니다.\n현재 버전은 회원가입, 광고, 인앱결제, 클라우드 저장 기능을 제공하지 않습니다.",140);
  Card("데이터 삭제 및 기기 변경","앱 데이터 삭제 또는 앱 삭제 시 진행 상황과 설정이 삭제될 수 있습니다.\n다른 기기로 진행 상황이 자동 이전되지 않습니다.",140);
  Card("이메일 문의","문의 시 보내주신 이메일 주소와 문의 내용은 답변과 문제 해결에 사용합니다.\n비밀번호나 신분증 등 민감한 정보는 보내지 마세요.",140);
 }
 void Goals(){
  var contract=g.S.contract;g.EnsureContract();contract=g.S.contract;
  var c=Card("1. 계약으로 장비 자금 모으기","",136);var reading=Text(c,"",12,40,388,74,14);LiveText(reading,()=>"현재 계약 "+N(contract.progress)+" / "+N(contract.target)+"\n완료 보상 ₩"+N(contract.reward)+" · 거래소에서 수령");
  ActionCard("2. 다음 GPU 인증",Certification(),"장착 관리 열기",()=>Open("gpu"),Green,182);
  ActionCard("3. 채굴실 확장",g.S.propertyId==0?"원룸 작업실 ₩30,000\n현재 현금 ₩"+N(g.S.cash):g.S.racksInstalled<2?"두 번째 랙 ₩15,000\n현재 현금 ₩"+N(g.S.cash):"확장 완료 · 랙 2개와 GPU 최대 16개", "부동산 열기",()=>Open("estate"),Gold,155);
  ActionCard("4. 환생 준비","최종 GPU "+g.QuantumCount()+" / "+g.RebirthReq()+"개\n운영 "+g.Duration(g.S.runSeconds)+" / "+g.Duration(g.RebirthSeconds()),"포크 및 환생 보기",()=>Open("fork"),Cyan,155);
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
 string layout=g.S.propertyId+":"+g.S.racksInstalled;if(layout!=roomKey)BuildRoom();string key=layout;for(int i=0;i<g.SlotCapacity();i++){var it=g.At(i);key+=it==null?"-":it.uid+":"+it.modelId+":"+it.coolerId;key+="|";}if(key!=rackKey){rackKey=key;BuildRack();}for(int i=0;i<rackTemps.Count;i++)if(rackTemps[i]!=null){var gpu=g.At(i);rackTemps[i].text=gpu==null?"":Mathf.FloorToInt(gpu.temp)+"°C";}foreach(var update in live.ToArray())update();}
 void ClearChildren(Transform parent){foreach(Transform child in parent){child.gameObject.SetActive(false);Destroy(child.gameObject);}}
 void FocusRack(int index){Canvas.ForceUpdateCanvases();roomPan.StopMovement();roomPan.horizontalNormalizedPosition=index==0?0:1;}
 Button RoomButton(Transform parent,string label,float x,float y,float w,float h,Action action,Color? accent=null,int size=12){return Button(parent,label,x,y,w,h,()=>{if(roomPan.CanTap)action();},accent,size);}
 void BuildRoom(){
  float position=roomPan==null?0:roomPan.horizontalNormalizedPosition;
  ClearChildren(roomFrame);roomRacks.Clear();roomKey=g.S.propertyId+":"+g.S.racksInstalled;rackKey="";
  Box(roomFrame,"Header",0,0,462,51,C("243847"),C("71818b"));
  Text(roomFrame,g.PropertyName()+" · 랙 "+g.S.racksInstalled+" / "+g.PropertyRackLimit(),12,4,300,22,14,Gold);
  Text(roomFrame,g.S.propertyId==0?"부동산에서 더 넓은 방으로 이사하세요":"방을 좌우로 밀어 이동 · GPU는 가볍게 누르기",12,28,310,18,10,White);
  Button(roomFrame,"부동산 / 확장",324,7,128,36,()=>Open("estate"),Gold,12);
  var viewport=Rect(roomFrame,"Horizontal room viewport",0,52,462,400);Paint(viewport,C("cbb8a8")).raycastTarget=true;viewport.gameObject.AddComponent<RectMask2D>();roomPan=viewport.gameObject.AddComponent<RoomPan>();
  float width=g.PropertyRackLimit()*462;roomWorld=Box(viewport,"Room world",0,0,width,400,g.S.propertyId==0?C("cbb8a8"):C("c0d1d4"));
  roomPan.viewport=viewport;roomPan.content=roomWorld;roomPan.horizontal=true;roomPan.vertical=false;roomPan.movementType=ScrollRect.MovementType.Clamped;roomPan.scrollSensitivity=35;roomPan.decelerationRate=.12f;
  Box(roomWorld,"Floor",0,238,width,162,g.S.propertyId==0?C("ded0c2"):C("a4b3b4"));Box(roomWorld,"Skirting",0,234,width,5,C("637879"));
  for(int i=0;i<g.PropertyRackLimit();i++){
   float x=i*462;Box(roomWorld,"Ceiling trim",x,0,462,8,C("516875"));
   var window=Box(roomWorld,"Window",x+267,20,165,91,C("d5f2fa"),C("506d7c"),5);Box(window,"Window bar",80,3,5,85,C("506d7c"));Box(window,"Window bar",3,42,159,4,C("506d7c"));
   Text(roomWorld,"채굴 랙 "+(i+1),x+13,24,224,27,16,C("233f4b"));
   if(i<g.S.racksInstalled){var panel=Box(roomWorld,"Rack "+(i+1),x+12,58,224,322,C("626775"),C("30343d"),4);roomRacks.Add(panel);}
   else {
    var pad=Box(roomWorld,"Empty rack space",x+12,58,224,322,C("8fa7ad"),C("506875"),3);
    Text(pad,"추가 랙 설치 공간\n\nGPU 슬롯 8개\n랙 전용 쿨러 1개\n\n설치비 ₩"+N(GameManager.ExtraRackCost),12,34,200,208,16,C("203b46"),TextAnchor.MiddleCenter);
    RoomButton(pad,"랙 구매 및 설치",12,264,200,42,()=>{if(g.BuyRack()){Close();FocusRack(1);}},Gold);
   }
   if(i==0){var desk=Rect(roomWorld,"Trading Desk",252,162,192,220);Box(desk,"Desk top",0,85,192,32,C("71623d"));Box(desk,"Left leg",8,112,38,108,C("71623d"));Box(desk,"Right leg",146,112,38,108,C("71623d"));Box(desk,"Monitor stand",82,60,28,42,C("3b4155"));var monitor=RoomButton(desk,"",49,0,94,69,()=>Open("trade"),C("737988"));DecorateTradeMonitor(monitor.GetComponent<RectTransform>());}
   else {var bench=Box(roomWorld,"Workbench",x+270,228,158,24,C("647c86"));Box(roomWorld,"Workbench leg",x+281,252,15,116,C("435b66"));Box(roomWorld,"Workbench leg",x+403,252,15,116,C("435b66"));Text(roomWorld,"장비 확장 구역",x+253,169,190,35,16,C("294752"),TextAnchor.MiddleCenter);}
   if(g.S.propertyId>0){int destination=i==0?1:0;RoomButton(roomWorld,i==0?"다음 공간 →":"← 첫 공간",x+268,118,164,32,()=>FocusRack(destination),Cyan,12);}
  }
  Canvas.ForceUpdateCanvases();roomPan.horizontalNormalizedPosition=position;
 }
 void BuildRack(){
  fans.Clear();rackTemps.Clear();
  for(int rIndex=0;rIndex<roomRacks.Count;rIndex++){
   var rack=roomRacks[rIndex];ClearChildren(rack);
   for(int i=0;i<8;i++){
    int slot=rIndex*8+i;var gpu=g.At(slot);float x=10+(i%2)*106,y=12+(i/2)*76;
    var b=RoomButton(rack,gpu==null?"[추가하기]":"",x,y,98,68,gpu==null?(Action)(()=>{pendingSlot=slot;Open("shop");}):()=>{double gain=g.ClickMine(gpu.uid);if(gain>0)FloatGain(gain);},gpu==null?C("29412e"):C("414c61"),10);
    var frame=b.GetComponent<RectTransform>();Box(frame,"LED",9,63,80,2,Green).GetComponent<Image>().raycastTarget=false;
    if(gpu==null){rackTemps.Add(null);continue;}var model=Catalog.GPU(gpu.modelId);
    GpuCardArt.Build(Rect(frame,"GPU body",4,14,90,37),gpu.modelId,FanSprite(),fans,false);Text(frame,model.name,5,2,89,13,8,White);rackTemps.Add(Text(frame,"",61,51,31,11,8,Cyan,TextAnchor.MiddleRight));
   }
  }
 }
 void Estate(){
  Card("현재 공간 · "+g.PropertyName(),"설치된 랙 "+g.S.racksInstalled+"개 · 최대 "+g.PropertyRackLimit()+"개\nGPU 장착 공간 "+g.SlotCapacity()+"개 · 보유 현금 ₩"+N(g.S.cash),100);
  var art=Card("원룸 작업실","",190);Box(art,"Room preview",12,42,388,132,C("b7ced4"),Cyan);Box(art,"Preview floor",16,125,380,44,C("8c9fa4"));
  for(int i=0;i<2;i++){var rr=Box(art,"Rack preview",40+i*188,61,88,99,C("293f4e"),C("637c89"));for(int row=0;row<4;row++)Box(rr,"Slots",7,8+row*22,74,14,C("4c7d80"));}
  if(g.S.propertyId==0)ActionCard("공간 확장 · ₩"+N(GameManager.StudioCost),"작은 방 → 원룸 작업실\n기존 GPU·쿨러·랙을 그대로 이전합니다.\n추가 랙과 GPU는 별도 구매입니다.","원룸 작업실 구매",()=>{if(g.BuyStudio()){Close();FocusRack(1);PlayMilestone(false);}},Gold,180);
  else if(g.S.racksInstalled<2)ActionCard("두 번째 랙 · ₩"+N(GameManager.ExtraRackCost),"오른쪽 공간에 빈 랙을 설치합니다.\nGPU 8개 추가 장착 · 쿨러는 랙별로 장착합니다.","랙 구매 및 설치",()=>{if(g.BuyRack()){Close();FocusRack(1);}},Green,160);
  else ActionCard("원룸 작업실 확장 완료","랙 2개 · 최대 GPU 16개 장착 가능","두 번째 랙 보기",()=>{Close();FocusRack(1);},Cyan,140);
  Card("확장 안내","방 안을 좌우로 밀면 이동합니다.\nGPU를 짧게 누르면 채굴하고, 끌면 화면만 이동합니다.\n구매한 부동산과 랙은 환생 후에도 유지됩니다.",117);
 }
 void RackSelector(string target){selectedRack=Mathf.Clamp(selectedRack,0,g.S.racksInstalled-1);for(int i=0;i<g.S.racksInstalled;i++){int idx=i;Button(content,"랙 "+(i+1)+(i==selectedRack?" · 선택됨":""),i*210,y,202,36,()=>{selectedRack=idx;Open(target);},i==selectedRack?Gold:Cyan);}y+=46;}
 string SlotName(int slot){return "랙 "+(slot/8+1)+" · 슬롯 "+(slot%8+1);}
 void FloatGain(double n){var t=Text(screen,"+"+N(n)+" "+g.MiningCoin().name,45,470,220,32,19,Green,TextAnchor.MiddleCenter);StartCoroutine(FloatRoutine(t));}
 IEnumerator FloatRoutine(Text t){float a=0;while(a<.9f){a+=Time.unscaledDeltaTime;t.rectTransform.anchoredPosition+=Vector2.up*35*Time.unscaledDeltaTime;t.color=new Color(Green.r,Green.g,Green.b,1-a/.9f);yield return null;}Destroy(t.gameObject);}
 void Toast(string s){var audio=GetComponent<MiningMusic>();if(audio!=null)audio.Feedback(s);if(toastRoutine!=null)StopCoroutine(toastRoutine);toastRoutine=StartCoroutine(ToastTime(s));}
 IEnumerator ToastTime(string s){toast.text=s;toast.transform.SetAsLastSibling();toast.gameObject.SetActive(true);yield return new WaitForSecondsRealtime(1.6f);toast.gameObject.SetActive(false);}
 float y; RectTransform Card(string title,string detail,float h=110,float detailHeight=-1){var r=Box(content,title,0,y,412,h,Dark,C("394765"));Text(r,title,10,7,392,26,14,Green);if(!string.IsNullOrEmpty(detail))Text(r,detail,10,37,392,detailHeight>=0?detailHeight:h-44,12,White,TextAnchor.UpperLeft);y+=h+9;return r;}
 void ActionCard(string title,string detail,string label,Action a,Color? accent=null,float h=158){var r=Card(title,detail,h,h-89);Button(r,label,10,h-44,392,34,a,accent);}
 void LiveText(Text t,Func<string> f){Action a=()=>{if(t!=null)t.text=f();};live.Add(a);a();}
 void Open(string type){if(!g.S.running&&type!="trade")return;Close(false);page=type;string title=type=="gpu"?"장착 관리":type=="inventory"?"장비 보관함":type=="oc"?"오버클럭":type=="cool"?"랙 냉각 연구소":type=="shop"?"장비 상점":type=="fork"?"포크 업그레이드":type=="news"?"채굴 경제신문":type=="estate"?"부동산 · 공간 확장":type=="privacy"?"개인정보 및 문의":type=="settings"?"설정":type=="goals"?"다음 성장 목표":"코인 거래소";
 modal=Box(screen,"Modal",0,0,480,854,new Color(.01f,.02f,.05f,.94f));modal.GetComponent<Image>().raycastTarget=true;var box=Box(modal,"Popup",20,type=="news"?190:32,440,type=="news"?422:746,Panel,C("56658c"),3);Text(box,title,12,8,354,35,16,Cyan);var closeButton=Button(box,"X",387,8,40,35,()=>Close(),Red,18);var closeLabel=closeButton.GetComponentInChildren<Text>();closeLabel.fontStyle=FontStyles.Bold;closeLabel.textWrappingMode=TextWrappingModes.NoWrap;closeLabel.overflowMode=TextOverflowModes.Overflow;ScrollRect sc;content=Scroll(box,14,55,412,type=="news"?352:675,out sc);y=0;
 switch(type){case "privacy":Privacy();break;case "settings":Settings();break;case "goals":Goals();break;case "estate":Estate();break;case "gpu":GPU();break;case "inventory":Inventory();break;case "oc":OC();break;case "cool":Cool();break;case "shop":Shop();break;case "fork":Fork();break;case "trade":Trade();break;case "news":NewsContent();break;}content.sizeDelta=new Vector2(412,y+8);Refresh();}
 public void Close(bool reset=true){live.Clear();if(modal!=null){modal.gameObject.SetActive(false);Destroy(modal.gameObject);}modal=null;page="";if(reset){pendingSlot=-1;equipUid=null;coolTarget=null;}}
 string Certification(){int t=g.RigTier();if(t==8)return "QUANTUM 인증 완료 · 모든 GPU 등급 해금";var n=Catalog.GPUs[t+1];return "현재 "+Catalog.GPUs[t].name+" 인증 · 다음 "+n.name+"\n운영 "+g.Duration(g.S.runSeconds)+" / "+g.Duration(g.RequiredSeconds(n))+"\n누적 채굴 "+N(g.S.totalMined)+" / "+N(n.unlockMined);}
 void GpuTile(GpuItem item,string title,string label,Action action){
  var r=Card(title,"",166);var model=Catalog.GPU(item.modelId);
  var reading=Text(r,"",10,37,230,77,13);LiveText(reading,()=>model.name+"\n전력 "+model.power+"W\n온도 "+Mathf.FloorToInt(item.temp)+"°C");
  GpuCardArt.Build(Rect(r,"GPU Preview",250,38,152,76),item.modelId,FanSprite());
  Button(r,label,10,122,392,34,action);
 }
 void GPU(){RackSelector("gpu");
  Card("장착 현황",g.Equipped().Count+" / "+g.SlotCapacity()+"개 가동 · 자동 채굴 "+D(g.TotalAuto())+"/초\n총 전력 "+g.TotalPower()+"W · 장착 해제 및 업그레이드",92);
  Card("장비 인증",Certification(),123);
  var primary=g.At(selectedRack*8);var next=primary!=null&&primary.modelId<8?Catalog.GPUs[primary.modelId+1]:null;
  ActionCard("랙 "+(selectedRack+1)+" · 1번 슬롯 업그레이드",next==null?"1번 슬롯 GPU가 없거나 최종 등급입니다.":Catalog.GPU(primary.modelId).name+" → "+next.name+"\n₩"+N(Catalog.GPU(primary.modelId).upgradeCost),"업그레이드",next!=null?(Action)(()=>{g.UpgradePrimary(selectedRack);Open("gpu");}):null);
  for(int i=0;i<8;i++){int slot=selectedRack*8+i;var item=g.At(slot);
   if(item==null)ActionCard(SlotName(slot),"비어 있음","GPU 구매",()=>{pendingSlot=slot;Open("shop");},Cyan,126);
   else GpuTile(item,SlotName(slot)+" · "+Catalog.Cooler(item.coolerId).name,"보관함으로 이동",()=>{g.Unequip(item.uid);Open("gpu");});
  }
 }
 void Inventory(){
  Card("미장착 장비 보관함","보관 GPU "+(g.S.items.Count-g.Equipped().Count)+"개 · 쿨러 "+g.S.coolingItems.Count+"개\n장착된 GPU는 ‘장착 관리’에서 관리합니다.",96);
  if(g.S.items.Count==0)ActionCard("다시 시작할 장비가 없나요?","최종 장비를 잃었을 때 기본 GPU를 지원받을 수 있습니다.","기본 GPU 지원받기",()=>{g.EmergencyStarter();Open("inventory");});
  var selected=g.Find(equipUid);
  if(selected!=null){Card("장착 슬롯 선택",Catalog.GPU(selected.modelId).name+" · 기존 GPU는 보관함으로 이동합니다.",86);
   for(int i=0;i<g.SlotCapacity();i++){int slot=i;var old=g.At(i);Button(content,SlotName(i)+" · "+(old==null?"비어 있음":Catalog.GPU(old.modelId).name),(i%2)*210,y,202,50,()=>{g.Equip(selected.uid,slot);equipUid=null;Open("inventory");},old==null?Cyan:Gold,11);if(i%2==1)y+=58;}
   Button(content,"선택 취소",0,y,412,36,()=>{equipUid=null;Open("inventory");},Red);y+=45;
  }
  foreach(var item in g.S.items)if(item.slot<0)GpuTile(item,"보관 중","장착 위치 선택",()=>{equipUid=item.uid;Open("inventory");});
  if(g.S.items.Count==g.Equipped().Count)Card("보관 중인 GPU 없음","상점에서 추가 구매한 장비가 이곳에 표시됩니다.",83);
  foreach(var unit in g.S.coolingItems){int mounted=g.CoolerRack(unit.uid);ActionCard(Catalog.Cooler(unit.coolerId).name,mounted>=0?"랙 "+(mounted+1)+"에서 사용 중":"보관 중 · 냉각 메뉴에서 장착할 랙을 선택하세요.","랙 냉각 관리",()=>Open("cool"),Cyan,128);}

 }
 void OC(){ActionCard("오버클럭 상태 · "+(g.S.overclock?"활성":"꺼짐"),"현재 자동 채굴 "+D(g.TotalAuto())+"/초\n성능 증가 +"+(g.HasFork("oc")?50:30)+"%\n성능과 장착 GPU의 발열 속도가 함께 증가합니다.",g.S.overclock?"[ 오버클럭 끄기 ]":"[ 오버클럭 켜기 ]",()=>{g.ToggleOC();if(g.S.running)Open("oc");},Gold,180);}
 void CoolerArt(Transform parent,int level){
  var r=Box(parent,"Cooler illustration",270,41,125,70,C("172632"),Cyan);
  for(int i=0;i<15;i++)Box(r,"Radiator fin",7+i*7,7,2,56,C("607782")).GetComponent<Image>().raycastTarget=false;
  for(int i=0;i<2;i++){var fan=Paint(Rect(r,"Cooler fan",10+i*53,10,49,49),Color.white);fan.sprite=FanSprite();fan.raycastTarget=false;}
  Text(r,"냉각 "+Mathf.Min(4,level)+"행",3,48,119,19,10,Cyan,TextAnchor.MiddleCenter);
 }
 void Cool(){RackSelector("cool");
  var status=Card("랙 "+(selectedRack+1)+" 냉각 시스템","",169);
  var reading=Text(status,"",10,39,392,73,13);LiveText(reading,()=>"쿨러 1개로 위쪽 행부터 함께 냉각합니다.\n1단계는 1행(2개), 최대 4행(8개) 적용.\n현재 "+g.CoolingRows(selectedRack)+"행 적용 · 전체 최고 "+Mathf.FloorToInt(g.Hottest())+"°C");
  var emergency=Button(status,"",10,123,392,34,()=>g.EmergencyCool(),Cyan);LiveText(emergency.GetComponentInChildren<Text>(),()=>g.EmergencyCoolRemaining()>0?"긴급 냉각 대기 · "+g.EmergencyCoolRemaining()+"초":"모든 랙 긴급 냉각 · 20°C 감소");live.Add(()=>emergency.interactable=g.EmergencyCoolRemaining()==0);
  for(int row=0;row<4;row++)Card((row+1)+"행 · 슬롯 "+(row*2+1)+"–"+(row*2+2),row<g.CoolingRows(selectedRack)?Catalog.Cooler(g.RackCoolerLevel(selectedRack)).name+" 적용 중":"기본 냉각 · 랙 쿨러 미적용",76);
  foreach(var unit in g.S.coolingItems){int mounted=g.CoolerRack(unit.uid);if(mounted>=0&&mounted!=selectedRack)ActionCard(Catalog.Cooler(unit.coolerId).name,"현재 랙 "+(mounted+1)+"에서 사용 중입니다.\n이동하면 기존 랙의 냉각은 해제됩니다.","선택한 랙으로 이동",()=>{g.EquipCoolerToRack(unit.uid,selectedRack);Open("cool");},Gold,146);}
  foreach(var cl in Catalog.Coolers){if(cl.id==0)continue;bool unlocked=g.CoolerUnlocked(cl);var owned=g.S.coolingItems.Find(x=>x.coolerId==cl.id&&g.CoolerRack(x.uid)<0);bool active=g.RackCoolerLevel(selectedRack)==cl.id;
   var card=Card(cl.name,"",185);CoolerArt(card,cl.id);
   var description=Text(card,"",10,38,250,100,12);
   LiveText(description,()=>{
    string status=active?"현재 랙 사용 중":owned!=null?"보유 쿨러 · 장착 가능":!g.CoolerUnlocked(cl)?"운영 "+g.Duration(Mathf.FloorToInt(cl.unlockSeconds*g.CycleFactor()))+" 필요":g.S.cash<cl.cost?"해금 완료 · 현금 부족":"구매 가능 · 현금 충분";
    return "최대 "+Mathf.Min(4,cl.id)+"행 · GPU "+(Mathf.Min(4,cl.id)*2)+"개\n냉각 -"+cl.rate.ToString("0.00")+"°C/초\n₩"+N(cl.cost)+"\n"+status;
   });
   int coolerId=cl.id;string coolerUid=owned==null?null:owned.uid;
   var button=Button(card,"",10,141,392,34,()=>{if(g.RackCoolerLevel(selectedRack)==coolerId)return;var spare=string.IsNullOrEmpty(coolerUid)?null:g.S.coolingItems.Find(x=>x.uid==coolerUid);if(spare!=null){g.EquipCoolerToRack(coolerUid,selectedRack);Open("cool");}else if(g.CoolerUnlocked(cl)&&g.S.cash>=cl.cost&&g.BuyCooler(null,coolerId,selectedRack))Open("cool");},Cyan);
   var buttonLabel=button.GetComponentInChildren<Text>();live.Add(()=>{bool inUse=g.RackCoolerLevel(selectedRack)==coolerId;bool hasSpare=!string.IsNullOrEmpty(coolerUid)&&g.S.coolingItems.Exists(x=>x.uid==coolerUid);bool canBuy=g.CoolerUnlocked(cl)&&g.S.cash>=cl.cost;button.interactable=g.S.running&&!inUse&&(hasSpare||canBuy);buttonLabel.text=inUse?"현재 사용 중":hasSpare?"보유 쿨러 장착":!g.CoolerUnlocked(cl)?"인증 잠금":g.S.cash<cl.cost?"현금 부족":"구매하고 랙에 장착";});
  }
 }
 void Shop(){
  Card("장비 인증",Certification()+"\n"+(pendingSlot>=0?SlotName(pendingSlot)+"에 바로 장착":"구매한 GPU는 보관함에 보관됩니다.")+"\n보유 ₩"+N(g.S.cash),158);
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
  Close();modal=Box(screen,"Rebirth confirmation",0,0,480,854,new Color(.01f,.02f,.05f,.96f));modal.GetComponent<Image>().raycastTarget=true;var r=Box(modal,"Conditions",25,180,430,420,Panel,Cyan);
  bool ready=g.CanRebirth();Text(r,ready?"환생할 준비가 되었습니다":"조건을 달성하지 못했습니다",18,15,394,45,20,ready?Green:Gold);
  Text(r,"최종 GPU 장착: "+g.QuantumCount()+" / "+g.RebirthReq()+"개\n운영 시간: "+g.Duration(g.S.runSeconds)+" / "+g.Duration(g.RebirthSeconds())+"\n남은 시간: "+Mathf.CeilToInt(Mathf.Max(0,g.RebirthSeconds()-g.S.runSeconds)/60f)+"분\n\n초기화: 현금, 코인, GPU, 쿨러, 회차 기록\n유지: 부동산·랙, 환생 횟수, 포크, 영구 업그레이드",20,78,390,216,15);
  if(ready)Button(r,"환생하기 · 포크 +1",20,308,390,42,()=>{int before=g.S.rebirths;g.Rebirth();if(before==0&&g.S.rebirths==1)PlayMilestone(true);else Open("fork");},Green);
  Button(r,ready?"취소":"확인",20,363,390,40,()=>Open("fork"),Cyan);
 }
 void Line(Transform p,Vector2 a,Vector2 b,Color c,float width){Vector2 delta=b-a;var r=Box(p,"Line",a.x,a.y,delta.magnitude,width,c);r.pivot=new Vector2(0,.5f);r.localRotation=Quaternion.Euler(0,0,-Mathf.Atan2(delta.y,delta.x)*Mathf.Rad2Deg);r.GetComponent<Image>().raycastTarget=false;}
 void Trade(){Card("코인 선택","코인 해금은 환생 전까지 유지됩니다\n현재 채굴: "+g.MiningCoin().name,86);for(int i=0;i<Catalog.Coins.Length;i++){var c=Catalog.Coins[i];bool u=g.CoinUnlocked(c);float x=(i%2)*210;var tile=Box(content,c.name,x,y,202,112,Dark,g.S.tradeCoinId==c.id?Cyan:C("405071"));Text(tile,c.name,8,6,186,23,13,Cyan);var quote=Text(tile,"",8,31,186,39,11);LiveText(quote,()=>u?"₩"+market.Price(c.id).ToString("0.000")+" · 보유 "+D(g.Balance(c.id)):Catalog.GPU(c.unlockGpu).name+" 획득 시 해금");Button(tile,"차트",8,75,88,29,u?(Action)(()=>{g.SetTradeCoin(c.id);Open("trade");}):null,Cyan,10);Button(tile,g.S.miningCoinId==c.id?"채굴중":"채굴",104,75,90,29,u?(Action)(()=>{g.SetMiningCoin(c.id);Open("trade");}):null,null,10);if(i%2==1)y+=120;}
 g.EnsureContract();var q=g.S.contract;var contract=Card("채굴 계약 · 완료 "+g.S.contractsCompleted,"",155);var qt=Text(contract,"",10,37,392,68,12);LiveText(qt,()=>(q.type=="mine"?"채굴":"판매")+" 계약 · "+Catalog.Coin(q.coinId).name+"\n"+N(q.progress)+" / "+N(q.target)+" · 보상 ₩"+N(q.reward));var claim=Button(contract,"[ 계약 보상 받기 ]",10,110,392,34,()=>{g.ClaimContract();Open("trade");},C("d383ff"));live.Add(()=>claim.interactable=q.progress>=q.target);
 string id=g.S.tradeCoinId;var chartBox=Card(Catalog.Coin(id).name+" / 원 · 5초봉","",275);var price=Text(chartBox,"",10,32,392,27,17,Gold);LiveText(price,()=>"₩"+market.Price(id).ToString("0.000"));var chart=Rect(chartBox,"Candlestick Chart",10,65,392,165).gameObject.AddComponent<CandleChart>();chart.Bind(market,id);var stats=Text(chartBox,"",10,233,392,33,11,Muted);LiveText(stats,()=>"다음 시세 "+Mathf.CeilToInt(market.Countdown)+"초 · 보유 "+D(g.Balance(id))+"\n전량 판매 예상 ₩"+N(g.Balance(id)*market.Price(id)));
 Text(content,"판매할 "+Catalog.Coin(id).name+" 수량",0,y,412,26,13);y+=30;var inputRect=Box(content,"판매 수량",0,y,412,45,Dark,Cyan);var input=inputRect.gameObject.AddComponent<TMP_InputField>();var inputImage=inputRect.GetComponent<Image>();inputImage.raycastTarget=true;input.targetGraphic=inputImage;input.textComponent=Text(inputRect,"",10,4,392,37,17);input.placeholder=Text(inputRect,"판매 수량 입력",10,4,392,37,14,Muted);input.textViewport=inputRect;input.contentType=TMP_InputField.ContentType.DecimalNumber;y+=54;var estimate=Text(content,"",0,y,412,30,12,Gold);y+=38;LiveText(estimate,()=>{double amount;return double.TryParse(input.text,NumberStyles.Float,CultureInfo.InvariantCulture,out amount)&&!double.IsInfinity(amount)&&!double.IsNaN(amount)?"예상 수령 ₩"+N(amount*market.Price(id)):"수량을 입력하세요.";});Button(content,"[ 입력 수량 판매 ]",0,y,412,40,()=>{double a;if(double.TryParse(input.text,NumberStyles.Float,CultureInfo.InvariantCulture,out a)&&g.Sell(id,a))Open("trade");else Toast("판매 수량과 보유 코인을 확인하세요.");},Cyan);y+=49;var all=Button(content,"[ "+Catalog.Coin(id).name+" 전량 판매 ]",0,y,412,40,()=>{g.Sell(id,g.Balance(id));Open("trade");});live.Add(()=>all.interactable=g.Balance(id)>0);y+=49;}
 void News(string message){Open("news");}
 void NewsContent(){
  var paper=Box(content,"News newspaper",0,0,412,237,C("eee8d9"),C("9f947d"));
  Text(paper,"채굴 경제신문",14,10,384,33,24,C("263239"));Box(paper,"Rule",14,49,384,2,C("263239"));
  Text(paper,market.NewsCategory+" · 채굴 경제신문",14,58,384,24,12,C("53656a"));
  Text(paper,market.NewsTitle,14,90,384,54,20,C("203438"));
  Text(paper,market.NewsBody,14,151,384,76,14,C("34484a"),TextAnchor.UpperLeft);
  y=249;Button(content,market.NewsCategory=="장비 소식"?"상점 보기":market.NewsCategory=="전력 소식"?"장착 관리 보기":"거래소 보기",0,y,412,40,()=>Open(market.NewsCategory=="장비 소식"?"shop":market.NewsCategory=="전력 소식"?"gpu":"trade"),Gold);y+=48;Button(content,"확인",0,y,412,40,()=>Close(),Cyan);y+=44;
 }
 void GameOver(){
  Close();StopIntro();intro=Box(screen,"Repair choice",0,0,480,854,new Color(.03f,.01f,.02f,.98f));intro.GetComponent<Image>().raycastTarget=true;var b=Box(intro,"Overheat",25,175,430,460,Panel,Red,3);
  Text(b,"과열! 채굴을 일시 중지했어요",18,18,394,48,21,Red,TextAnchor.MiddleCenter);
  Text(b,"수리하면 모든 GPU를 보존하고 30°C로 냉각합니다.\n\n수리비 ₩"+N(g.RepairCost())+" · 보유 ₩"+N(g.S.cash)+"\n\n수리를 포기하면 장착 GPU 중 무작위 1개가 삭제됩니다. 현금·코인·다른 장비는 유지됩니다.",20,83,390,176,15);
  Button(b,"수리하고 계속하기",20,277,390,42,g.S.cash>=g.RepairCost()?(Action)(()=>{if(g.ResolveOverheat(true))StopIntro();}):null,Green);
  Button(b,"보유 코인 전량 판매",20,333,390,42,()=>{foreach(var c in Catalog.Coins)g.Sell(c.id,g.Balance(c.id));GameOver();},Gold);
  Button(b,"수리 포기 · 무작위 GPU 1개 삭제",20,389,390,42,()=>{g.ResolveOverheat(false);StopIntro();if(g.S.items.Count==0)Open("inventory");},Red,12);
 }
 // Original opening and tutorial copy, retained from index.html.
 static readonly string[] cutLabels={"시스템 시작","장비 가동","시장 연결","장비 확장","최종 목표"},cutTitles={"낡은 채굴실","첫 번째 GPU","가상 코인 시장 접속","더 강한 장비","목표: QUANTUM"},cutArt={"●\n전력 꺼짐\n...\n신호 감지","▣\nRTX 1090\n온도 30°C\n상태: 가동 중","₿ Ξ Ð ✦ Q\n시장 연결 : 연결됨\n시세 변동 : 활성","▣  ▣  ▣\nGPU / 냉각 / 전력\n나만의 채굴실","⚛\nRTX 6090 QUANTUM\n환생 시스템\n잠김"},cutText={"불 꺼진 작은 방.\n책상 위에는 오래된 채굴 장비 한 대만 남아 있다.","전원을 넣자 RTX 1090의 팬이 천천히 돌기 시작한다.\n이 한 장이 모든 것의 시작이다.","채굴한 코인은 시장에서 현금으로 바꿀 수 있다.\n하지만 시세는 계속 움직이고, 어떤 코인은 갑자기 폭등하거나 급락한다.","더 좋은 GPU를 사고, 냉각을 강화하고, 새로운 코인을 해금하라.\n작은 랙은 점점 거대한 채굴 시스템으로 변한다.","최종 목표는 RTX 6090 QUANTUM과 환생 프로토콜.\n지금부터 첫 번째 채굴을 시작한다."};
 static readonly string[] tutTitles={"1. GPU를 눌러 채굴","2. 코인을 현금으로 판매","3. GPU 업그레이드","4. 온도와 GPU 쿨링","5. 전력과 오버클럭","6. 계약과 시장 이벤트","7. 환생과 포크"},tutText={"랙에 장착된 GPU를 클릭하면 코인을 채굴합니다.\n\nGPU가 여러 장이면 클릭 한 번에 장착된 GPU가 모두 같이 채굴합니다.","방 오른쪽 컴퓨터를 누르면 거래소가 열립니다.\n\n채굴한 코인을 현재 게임 시세로 팔아 ₩ 현금을 만들고, 그 돈으로 장비를 구매합니다.","[상점]에서 GPU를 구매하거나 [장착 관리]에서 1번 슬롯 GPU를 업그레이드할 수 있습니다.\n\n상위 GPU를 보유하면 ETHER-X, DOGE-X 같은 새로운 코인도 순서대로 해금됩니다.","채굴과 오버클럭은 GPU 온도를 올립니다.\n\n[냉각]에서 랙 쿨러를 장착하세요. 1단계는 1행, 최대 4행을 냉각합니다. 100°C가 되면 수리비를 내거나 GPU 1개를 포기할 수 있습니다.","[오버클럭]는 채굴 성능을 높이지만 발열도 증가합니다.\n\nGPU 전력 사용량에 따라 실제 게임 현금에서 전기요금이 주기적으로 차감됩니다.","거래소에는 채굴/판매 계약이 있습니다. 목표를 완료하면 현금 보상을 받을 수 있습니다.\n\n랜덤 이벤트로 해금된 코인 시세, 전기요금, GPU 구매 가격이 일시적으로 변합니다.","최종 RTX 6090 QUANTUM을 준비하고 환생 조건을 달성하면 포크를 얻습니다.\n\n환생하면 이번 회차 장비는 초기화되지만 포크 업그레이드는 남아 다음 회차 성장을 빠르게 해줍니다."};
 void StopIntro(){if(milestoneRoutine!=null){StopCoroutine(milestoneRoutine);milestoneRoutine=null;}if(typing!=null)StopCoroutine(typing);typingNow=false;if(intro!=null){intro.gameObject.SetActive(false);Destroy(intro.gameObject);}intro=null;g.IntroPaused=false;}
 void Opening(bool rp){if(!g.S.running)return;Close();replay=rp;introIndex=0;RenderOpening();}
 void RenderOpening(){StopIntro();g.IntroPaused=true;intro=Box(screen,"Opening",0,0,480,854,C("02040a"));intro.GetComponent<Image>().raycastTarget=true;var b=Box(intro,"Cutscene",25,148,430,535,Dark,C("303a50"),3);Text(b,cutLabels[introIndex],12,8,300,28,12,Cyan);Button(b,"건너뛰기",347,8,70,29,FinishOpening,Red,10);Box(b,"Visual",3,44,424,220,introIndex==2?C("241b39"):introIndex==4?C("123e3a"):C("14283e"));Text(b,cutArt[introIndex],18,68,394,176,22,Green,TextAnchor.MiddleCenter);Text(b,cutTitles[introIndex],16,280,398,33,18,Gold);typed=Text(b,"",16,321,398,125,16);fullCaption=cutText[introIndex];typing=StartCoroutine(TypeCaption());Text(b,new string('■',introIndex+1)+new string('□',4-introIndex),16,466,185,40,13,Cyan);Button(b,introIndex==4?"[ 채굴 시작 ]":"[ 계속 ]",260,470,151,42,()=>{if(typingNow){StopCoroutine(typing);typed.text=fullCaption;typingNow=false;}else if(introIndex<4){introIndex++;RenderOpening();}else FinishOpening();});}
 IEnumerator TypeCaption(){typingNow=true;for(int i=0;i<=fullCaption.Length;i++){typed.text=fullCaption.Substring(0,i);yield return new WaitForSecondsRealtime(.022f);}typingNow=false;}
 void FinishOpening(){PlayerPrefs.SetInt("webport_opening",1);PlayerPrefs.Save();StopIntro();if(!replay&&PlayerPrefs.GetInt("webport_tutorial",0)==0)Tutorial();}
 void Tutorial(){if(!g.S.running)return;Close();introIndex=0;RenderTutorial();}
 void RenderTutorial(){StopIntro();g.IntroPaused=true;intro=Box(screen,"Tutorial",0,0,480,854,new Color(.01f,.02f,.05f,.97f));intro.GetComponent<Image>().raycastTarget=true;var b=Box(intro,"Tutorial Card",30,155,420,534,Panel,Cyan,4);Text(b,"게임 안내 "+(introIndex+1)+" / 7",16,13,263,32,12,Cyan);Button(b,"건너뛰기",306,14,98,32,FinishTutorial,Red,11);Text(b,tutTitles[introIndex],20,85,380,55,21,Green,TextAnchor.MiddleCenter);Text(b,tutText[introIndex],26,158,368,231,17);Text(b,new string('■',introIndex+1)+new string('□',6-introIndex),20,407,380,28,14,Cyan,TextAnchor.MiddleCenter);Button(b,"[ 이전 ]",20,463,180,47,introIndex>0?(Action)(()=>{introIndex--;RenderTutorial();}):null,Cyan);Button(b,introIndex==6?"[ 시작 ]":"[ 다음 ]",220,463,180,47,()=>{if(introIndex<6){introIndex++;RenderTutorial();}else FinishTutorial();});}
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
 MarketManager market;public string coinId;
 public void Bind(MarketManager source,string id){if(market!=null)market.Changed-=OnMarketChanged;market=source;coinId=id;if(isActiveAndEnabled&&market!=null)market.Changed+=OnMarketChanged;SetVerticesDirty();}
 protected override void OnEnable(){base.OnEnable();if(market!=null)market.Changed+=OnMarketChanged;}
 protected override void OnDisable(){if(market!=null)market.Changed-=OnMarketChanged;base.OnDisable();}
 void OnMarketChanged(){SetVerticesDirty();}
 protected override void OnPopulateMesh(VertexHelper vh){vh.Clear();if(market==null)return;var series=market.Series(coinId);if(series==null||series.candles.Count==0)return;var cs=series.candles;float lo=float.MaxValue,hi=float.MinValue;foreach(var c in cs){lo=Mathf.Min(lo,c.low);hi=Mathf.Max(hi,c.high);}float pad=Mathf.Max(Catalog.Coin(coinId).basePrice*.08f,(hi-lo)*.12f);lo-=pad;hi+=pad;float w=rectTransform.rect.width,h=rectTransform.rect.height;Func<float,float> y=p=>h*(p-lo)/(hi-lo);for(int i=0;i<5;i++)Quad(vh,0,h*i/4,w,1,new Color(.21f,.23f,.28f));float step=w/cs.Count;for(int i=0;i<cs.Count;i++){var c=cs[i];float x=(i+.5f)*step;Color color=c.close>=c.open?new Color(1,.38f,.36f):new Color(.31f,.60f,1);Quad(vh,x,y(c.low),1,Mathf.Max(1,y(c.high)-y(c.low)),color);Quad(vh,x-step*.3f,y(Mathf.Min(c.open,c.close)),Mathf.Max(1,step*.6f),Mathf.Max(1,Mathf.Abs(y(c.close)-y(c.open))),color);}}
 void Quad(VertexHelper vh,float x,float y,float w,float h,Color c){int n=vh.currentVertCount;Vector2 o=rectTransform.rect.min;vh.AddVert(new Vector3(x+o.x,y+o.y),c,Vector2.zero);vh.AddVert(new Vector3(x+w+o.x,y+o.y),c,Vector2.zero);vh.AddVert(new Vector3(x+w+o.x,y+h+o.y),c,Vector2.zero);vh.AddVert(new Vector3(x+o.x,y+h+o.y),c,Vector2.zero);vh.AddTriangle(n,n+1,n+2);vh.AddTriangle(n,n+2,n+3);}
}
}
