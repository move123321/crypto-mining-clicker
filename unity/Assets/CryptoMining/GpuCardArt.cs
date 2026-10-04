using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CryptoMining {
// One normalized layout is used by both shop previews and installed cards.
[RequireComponent(typeof(CanvasRenderer))]
public class GpuCardArt : MaskableGraphic {
 public int model;
 public bool liquid;
 static readonly string[] accents={"70d478","53ddf4","ff6466","c5d2df","a783ff","ff96dc","efc96b","ffac44","55ffe2"};
 static readonly string[] shells={"2c3c38","dce6ef","343b47","778493","322849","e7ddeb","514733","454841","163e49"};
 public static Vector3[] FanLayout(int id){
  switch(id){
   case 0:return new[]{new Vector3(101,43,25)};
   case 1:return new[]{new Vector3(57,43,25),new Vector3(124,43,25)};
   case 2:return new[]{new Vector3(43,44,22),new Vector3(94,44,22),new Vector3(145,44,22)};
   case 3:return new[]{new Vector3(52,42,28),new Vector3(130,42,28)};
   case 4:return new[]{new Vector3(43,45,22),new Vector3(94,45,22),new Vector3(145,45,22)};
   case 5:return new[]{new Vector3(42,44,21),new Vector3(94,44,27),new Vector3(146,44,21)};
   case 6:return new[]{new Vector3(42,44,24),new Vector3(94,44,24),new Vector3(146,44,24)};
   case 7:return new[]{new Vector3(126,43,29)};
   default:return new[]{new Vector3(44,43,24),new Vector3(142,43,24)};
  }
 }
 public static void Build(RectTransform target,int modelId,Sprite rotor,List<RectTransform> animated=null,bool water=false){
  int id=Mathf.Clamp(modelId,0,8);float scale=Mathf.Min(target.rect.width/180f,target.rect.height/90f);
  var root=new GameObject("GPU design "+id,typeof(RectTransform)).GetComponent<RectTransform>();root.SetParent(target,false);
  root.anchorMin=root.anchorMax=new Vector2(.5f,.5f);root.pivot=new Vector2(.5f,.5f);root.sizeDelta=new Vector2(180,90);root.localScale=Vector3.one*scale;
  var art=root.gameObject.AddComponent<GpuCardArt>();art.model=id;art.liquid=water;art.raycastTarget=false;
  if(water)return;
  foreach(var layout in FanLayout(id)){
   var fan=new GameObject("Fan",typeof(RectTransform)).GetComponent<RectTransform>();fan.SetParent(root,false);
   fan.anchorMin=fan.anchorMax=new Vector2(0,1);fan.pivot=new Vector2(.5f,.5f);fan.anchoredPosition=new Vector2(layout.x,-layout.y);fan.sizeDelta=Vector2.one*(layout.z*1.72f);
   var image=fan.gameObject.AddComponent<Image>();image.sprite=rotor;image.preserveAspect=true;image.color=Color.white;image.raycastTarget=false;
   if(animated!=null)animated.Add(fan);
  }
 }
 static Color Hex(string value){ColorUtility.TryParseHtmlString("#"+value,out var c);return c;}
 protected override void OnPopulateMesh(VertexHelper vh){
  vh.Clear();int id=Mathf.Clamp(model,0,8);Color accent=Hex(accents[id]),shell=Hex(shells[id]);
  float left=id==0?36:12,right=id==0?143:174,top=id==6?7:12,bottom=76;
  // PCB, expansion-slot bracket and visible gold edge contacts.
  Quad(vh,left,18,right-left,63,Hex("173b32"));Quad(vh,left-7,11,5,73,Hex("85949e"));
  Quad(vh,left-10,9,11,3,Hex("c1cbd0"));Quad(vh,left-9,77,9,3,Hex("c1cbd0"));
  for(int i=0;i<14;i++)Quad(vh,54+i*4,80,3,6,Hex("d9ad50"));
  Plate(vh,left,top,right-left,bottom-top,id==3?15:id==8?12:6,Hex("0a101a"));
  Plate(vh,left+2,top+2,right-left-4,bottom-top-4,id==3?14:5,shell);
  // Different cooling and chassis construction for each generation.
  switch(id){
   case 0:
    for(int i=0;i<6;i++)Quad(vh,43,25+i*6,21,2,Hex("101d22"));
    Quad(vh,44,17,28,3,accent);break;
   case 1:
    Quad(vh,17,16,148,3,accent);Quad(vh,87,24,8,38,Hex("94a7b6"));
    Quad(vh,20,68,144,3,Hex("647c8f"));break;
   case 2:
    Strip(vh,new Vector2(17,17),new Vector2(72,24),4,accent);
    Strip(vh,new Vector2(111,65),new Vector2(169,71),4,accent);break;
   case 3:
    Strip(vh,new Vector2(73,16),new Vector2(112,70),7,Hex("161e28"));
    Strip(vh,new Vector2(72,68),new Vector2(108,16),7,Hex("d6e0e8"));break;
   case 4:
    for(int i=0;i<6;i++)Quad(vh,18+i*25,14,22,3,Color.Lerp(accent,Hex("56e4f2"),i/5f));
    Quad(vh,22,71,143,2,accent);break;
   case 5:
    for(int i=0;i<4;i++){Strip(vh,new Vector2(78+i*3,15),new Vector2(67+i*3,26),2,accent);Strip(vh,new Vector2(109+i*3,63),new Vector2(99+i*3,73),2,accent);}break;
   case 6:
    Quad(vh,18,11,150,4,accent);Quad(vh,18,71,150,3,accent);
    for(int i=0;i<5;i++)Quad(vh,23+i*30,5,17,4,Hex("b59a68"));break;
   case 7:
    for(int i=0;i<8;i++)Quad(vh,23+i*6,24,3,36,Hex("9eaaa1"));
    Quad(vh,22,19,55,4,accent);Quad(vh,22,64,55,4,accent);
    for(int i=0;i<4;i++)Strip(vh,new Vector2(22+i*10,67),new Vector2(27+i*10,72),3,Hex("17242a"));break;
   case 8:
    Strip(vh,new Vector2(18,13),new Vector2(81,18),3,accent);
    Strip(vh,new Vector2(106,68),new Vector2(168,73),3,accent);
    Poly(vh,new[]{new Vector2(93,20),new Vector2(108,43),new Vector2(93,66),new Vector2(78,43)},accent);
    Poly(vh,new[]{new Vector2(93,28),new Vector2(102,43),new Vector2(93,58),new Vector2(84,43)},Hex("20657a"));break;
  }
  foreach(var fan in FanLayout(id)){
   Disc(vh,new Vector2(fan.x,fan.y),fan.z+2,Hex("0c1520"));
   Disc(vh,new Vector2(fan.x,fan.y),fan.z,id==0?Hex("7a9185"):accent);
   Disc(vh,new Vector2(fan.x,fan.y),fan.z-1.5f,Hex("152331"));
  }
  foreach(var x in new[]{left+5,right-6})foreach(var y in new[]{top+6,bottom-6})Disc(vh,new Vector2(x,y),1.4f,Hex("c1cbd0"));
  if(liquid){
   Plate(vh,left+7,22,right-left-14,43,6,Hex("104053"));
   for(int i=0;i<3;i++)Quad(vh,left+13,30+i*10,right-left-26,2,Hex("53dced"));
   Disc(vh,new Vector2(94,43),12,accent);Disc(vh,new Vector2(94,43),8,Hex("153442"));
  }
 }
 void Quad(VertexHelper vh,float x,float y,float w,float h,Color c){Poly(vh,new[]{new Vector2(x,y),new Vector2(x+w,y),new Vector2(x+w,y+h),new Vector2(x,y+h)},c);}
 void Plate(VertexHelper vh,float x,float y,float w,float h,float cut,Color c){Poly(vh,new[]{new Vector2(x+cut,y),new Vector2(x+w-cut,y),new Vector2(x+w,y+cut),new Vector2(x+w,y+h-cut),new Vector2(x+w-cut,y+h),new Vector2(x+cut,y+h),new Vector2(x,y+h-cut),new Vector2(x,y+cut)},c);}
 void Strip(VertexHelper vh,Vector2 a,Vector2 b,float width,Color c){var d=(b-a).normalized;var n=new Vector2(-d.y,d.x)*width*.5f;Poly(vh,new[]{a+n,b+n,b-n,a-n},c);}
 void Disc(VertexHelper vh,Vector2 center,float radius,Color c){var points=new Vector2[64];for(int i=0;i<64;i++){float t=i*Mathf.PI*2/64;points[i]=center+new Vector2(Mathf.Cos(t),Mathf.Sin(t))*radius;}Poly(vh,points,c);}
 void Poly(VertexHelper vh,Vector2[] points,Color c){int first=vh.currentVertCount;var r=rectTransform.rect;foreach(var p in points)vh.AddVert(new Vector3(r.xMin+p.x,r.yMax-p.y),c,Vector2.zero);for(int i=1;i<points.Length-1;i++)vh.AddTriangle(first,first+i+1,first+i);}
}
}
