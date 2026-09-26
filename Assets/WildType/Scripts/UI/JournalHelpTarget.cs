using System;
using UnityEngine;
using UnityEngine.EventSystems;
namespace WildType
{
    public sealed class JournalHelpTarget : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
    {
        public string Explanation { get; set; } = "";
        public Action<JournalHelpTarget> Show, Hide;
        public void OnPointerEnter(PointerEventData e) => Reveal();
        public void OnPointerExit(PointerEventData e) { if (!EventSystem.current || EventSystem.current.currentSelectedGameObject != gameObject) Hide?.Invoke(this); }
        public void OnSelect(BaseEventData e) => Reveal();
        public void OnDeselect(BaseEventData e) => Hide?.Invoke(this);
        public void Reveal() { if (isActiveAndEnabled) Show?.Invoke(this); }
        void OnDisable() => Hide?.Invoke(this);
    }
}
