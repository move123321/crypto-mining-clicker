using UnityEngine;
namespace CryptoMining {
// Original synthesized background loop, with no external audio dependency.
[RequireComponent(typeof(AudioSource))]
public class MiningMusic:MonoBehaviour {
 AudioSource source;
 public bool Muted => PlayerPrefs.GetInt("mining_music_muted",0)==1;
 void Start(){
  source=GetComponent<AudioSource>();source.playOnAwake=false;source.loop=true;source.spatialBlend=0;source.volume=Muted?0:.18f;
  source.clip=CreateLoop();source.Play();
 }
 public static AudioClip CreateLoop(){
  const int rate=22050;const float beat=.6f;int count=Mathf.RoundToInt(rate*beat*64);var data=new float[count];
  int[] roots={45,41,48,43},melody={0,7,12,7,3,10,7,3,0,7,15,12,10,7,3,7},intervals={0,3,7};
  for(int i=0;i<count;i++){
   float t=i/(float)rate;int step=(int)(t/beat);float phase=t%beat;int root=roots[(step/16)%4];
   float note=440*Mathf.Pow(2,(root+12+melody[step%16]-69)/12f);
   float envelope=Mathf.Min(1,phase/.025f)*Mathf.Exp(-phase*6);
   float lead=(Mathf.Sin(2*Mathf.PI*note*t)+.15f*Mathf.Sin(4*Mathf.PI*note*t))*.16f*envelope;
   float bassFreq=440*Mathf.Pow(2,(root-69)/12f);
   float bass=Mathf.Sin(2*Mathf.PI*bassFreq*t)*.12f*Mathf.Min(1,phase/.03f)*Mathf.Exp(-phase*3);
   float pad=0;foreach(int interval in intervals)pad+=Mathf.Sin(2*Mathf.PI*bassFreq*Mathf.Pow(2,(interval+12)/12f)*t)*.025f;
   float barPhase=(t%(beat*16))/(beat*16);pad*=Mathf.Sin(barPhase*Mathf.PI);
   float kick=step%2==0?Mathf.Sin(2*Mathf.PI*(48*phase+9*(1-Mathf.Exp(-phase*25))))*.10f*Mathf.Exp(-phase*18):0;
   float edge=Mathf.Min(1,Mathf.Min(t,(count-1-i)/(float)rate)/.03f);
   data[i]=(lead+bass+pad+kick)*edge;
  }
  var clip=AudioClip.Create("채굴실의 밤",count,1,rate,false);clip.SetData(data,0);return clip;
 }
 public void Toggle(){PlayerPrefs.SetInt("mining_music_muted",Muted?0:1);PlayerPrefs.Save();if(source!=null)source.volume=Muted?0:.18f;}
 void OnDestroy(){if(source!=null&&source.clip!=null)Destroy(source.clip);}
}
}
