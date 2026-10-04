using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace CryptoMining {
// Prevent a drag release over a GPU from also being interpreted as a mining tap.
public class RoomPan:ScrollRect {
 bool panning;float blockUntil;
 public bool CanTap => !panning&&Time.unscaledTime>=blockUntil;
 public override void OnBeginDrag(PointerEventData e){if(e.button!=PointerEventData.InputButton.Left)return;panning=true;e.eligibleForClick=false;base.OnBeginDrag(e);}
 public override void OnEndDrag(PointerEventData e){base.OnEndDrag(e);panning=false;blockUntil=Time.unscaledTime+.12f;e.eligibleForClick=false;}
 protected override void OnDisable(){panning=false;blockUntil=0;base.OnDisable();}
}
}
