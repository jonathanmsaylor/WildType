using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace WildType
{
    // A shape, not just a color change, makes controller/keyboard focus visible on transparent cards too.
    public sealed class JournalFocusMarker : MonoBehaviour, ISelectHandler, IDeselectHandler
    {
        GameObject marker;
        public bool Marked => marker && marker.activeSelf;
        void Awake()
        {
            marker = new GameObject("Focus underline", typeof(RectTransform), typeof(Image));
            marker.transform.SetParent(transform, false);
            var rect = marker.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0); rect.anchorMax = new Vector2(1, 0);
            rect.pivot = new Vector2(.5f, 0); rect.offsetMin = new Vector2(8, 1); rect.offsetMax = new Vector2(-8, 4);
            var image = marker.GetComponent<Image>(); image.color = new Color(.96f, .88f, .63f); image.raycastTarget = false;
            marker.SetActive(false);
        }
        public void OnSelect(BaseEventData e) { if (marker) marker.SetActive(true); }
        public void OnDeselect(BaseEventData e) { if (marker) marker.SetActive(false); }
        void OnEnable() { if (marker) marker.SetActive(EventSystem.current && EventSystem.current.currentSelectedGameObject == gameObject); }
        void OnDisable() { if (marker) marker.SetActive(false); }
    }
}
