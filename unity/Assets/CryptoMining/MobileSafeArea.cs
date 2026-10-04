using UnityEngine;
namespace CryptoMining {
public class MobileSafeArea:MonoBehaviour {
 public RectTransform target;public Canvas canvas;
 public static Vector3 Fit(Rect safe,float scale){return Vector3.one*Mathf.Min(safe.width/(480*scale),safe.height/(854*scale));}
 void LateUpdate(){if(target==null||canvas==null||Screen.width<=0||Screen.height<=0)return;var safe=Screen.safeArea;float scale=Mathf.Max(.001f,canvas.scaleFactor);target.localScale=Fit(safe,scale);target.anchoredPosition=(safe.center-new Vector2(Screen.width,Screen.height)*.5f)/scale;}
}
}
