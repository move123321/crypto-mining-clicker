using System.Collections;
using UnityEngine;

namespace CryptoMining {
public partial class WebGameUI {
 Coroutine milestoneRoutine;

 void PlayMilestone(bool rebirth) {
  int bit=rebirth?2:1;
  if(g.SaveBlocked||!g.S.running||(g.S.seenMilestones&bit)!=0)return;
  // The purchase/rebirth is already committed. Skipping never changes rewards.
  g.S.seenMilestones|=bit;g.Save();
  Close();StopIntro();g.IntroPaused=true;
  intro=Box(screen,"Milestone",0,0,480,854,C("080d19"));intro.GetComponent<UnityEngine.UI.Image>().raycastTarget=true;
  Text(intro,"MILESTONE / "+(rebirth?"02":"01"),32,100,320,28,12,Cyan);
  Button(intro,"건너뛰기",340,96,108,36,StopIntro,Muted,12);
  Text(intro,rebirth?"새로운 시작":"더 넓은 작업실로",32,157,416,55,27,Gold);
  var visual=Box(intro,"Milestone stage",24,239,432,290,Dark,C("374965"),2);
  var scene=Rect(visual,"Animated scene",0,0,432,290);
  if(rebirth){
   for(int i=0;i<5;i++)Box(scene,"Circuit",40+i*76,40,2,204,C("213c50"));
   ForkIcon(scene,136,42,160);
   Text(scene,"FORK +1",66,218,300,44,25,Gold,TextAnchor.MiddleCenter);
  }else{
   Box(scene,"Floor",3,233,426,54,C("354453"));
   var window=Box(scene,"Window",274,28,126,118,C("286078"),Cyan,3);
   Box(window,"Cross",61,3,3,112,Cyan);Box(window,"Cross",3,56,120,3,Cyan);
   var rack=Box(scene,"Moved rack",38,35,144,206,C("14202d"),Muted,4);
   for(int i=0;i<3;i++){
    var card=Rect(rack,"GPU",12,15+i*58,120,52);
    GpuCardArt.Build(card,g.At(0)!=null?g.At(0).modelId:0,FanSprite());
   }
   Box(scene,"Desk",242,181,162,12,C("917763"));
   Box(scene,"Desk leg",250,193,9,50,Muted);Box(scene,"Desk leg",385,193,9,50,Muted);
   var monitor=Box(scene,"Monitor",280,133,70,43,C("122a34"),Cyan,2);
   Text(monitor,"ONLINE",5,7,60,28,10,Green,TextAnchor.MiddleCenter);
  }
  var caption=Text(intro,"",32,564,416,94,19,White,TextAnchor.MiddleCenter);
  Text(intro,"연출 중에는 채굴과 온도가 잠시 멈춥니다",32,704,416,30,12,Muted,TextAnchor.MiddleCenter);
  var progress=Box(intro,"Progress",32,762,0,3,Cyan);
  milestoneRoutine=StartCoroutine(AnimateMilestone(rebirth,scene,caption,progress));
 }

 IEnumerator AnimateMilestone(bool rebirth,RectTransform scene,TMPro.TextMeshProUGUI caption,RectTransform progress){
  var fade=scene.gameObject.AddComponent<CanvasGroup>();
  float elapsed=0;
  while(elapsed<4.8f){
   // Do not skip the reveal when returning from another app.
   if(Application.isFocused){elapsed+=Mathf.Min(Time.unscaledDeltaTime,.05f);}
   float reveal=Mathf.SmoothStep(0,1,Mathf.Clamp01(elapsed/1.6f));
   fade.alpha=reveal;scene.anchoredPosition=new Vector2((1-reveal)*36,0);
   progress.sizeDelta=new Vector2(416*Mathf.Clamp01(elapsed/4.8f),3);
   caption.text=rebirth?(elapsed<2.3f?"기계는 멈춰도, 경험은 남습니다.":"포크 하나를 얻었습니다.\n이번에는 더 멀리 나아가세요."):(elapsed<2.3f?"작은 방에서 시작한 채굴이\n새로운 공간으로 이어집니다.":"원룸 작업실에 도착했습니다.\n이제 두 번째 랙을 준비해 보세요.");
   yield return null;
  }
  milestoneRoutine=null;StopIntro();
 }
}
}
