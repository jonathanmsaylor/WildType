using System;
using UnityEngine;
using UnityEngine.EventSystems;
namespace WildType
{
    public sealed class JournalHelpTarget : MonoBehaviour, IPointerEnterHandler, IPointerMoveHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
    {
        public string Explanation { get; set; } = "";
        public Action<JournalHelpTarget> Show, Hide;
        public void OnPointerEnter(PointerEventData e)
        {
            var selected=EventSystem.current?EventSystem.current.currentSelectedGameObject:null;
            // Auto-scrolling focus can move another row under a stationary pointer.
            // A deliberate pointer movement may take over; layout movement must not.
            if(e.delta.sqrMagnitude>0||!selected||selected==gameObject||!selected.GetComponent<JournalHelpTarget>())Reveal();
        }
        public void OnPointerMove(PointerEventData e){if(e.delta.sqrMagnitude>0)Reveal();}
        public void OnPointerExit(PointerEventData e) { if (!EventSystem.current || EventSystem.current.currentSelectedGameObject != gameObject) Hide?.Invoke(this); }
        public void OnSelect(BaseEventData e) => Reveal();
        public void OnDeselect(BaseEventData e) => Hide?.Invoke(this);
        public void Reveal() { if (isActiveAndEnabled) Show?.Invoke(this); }
        void OnDisable() => Hide?.Invoke(this);
    }
}
