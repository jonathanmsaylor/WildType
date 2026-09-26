using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
namespace WildType
{
    public sealed class FamilyTreeCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler, IMoveHandler
    {
        public CreatureId Id;
        public Action<FamilyTreeCard> Focused, Explain;
        public Action<CreatureId,int,int> Navigate;
        public Color BaseColor;
        bool hovered,focused;
        public bool Highlighted=>hovered||focused;
        public void Paint(){GetComponent<Image>().color=Highlighted?new Color(.68f,.52f,.16f,1):BaseColor;}
        public void OnPointerEnter(PointerEventData e){hovered=true;Paint();Explain?.Invoke(this);}
        public void OnPointerExit(PointerEventData e){hovered=false;Paint();}
        public void OnSelect(BaseEventData e){focused=true;Paint();Focused?.Invoke(this);Explain?.Invoke(this);}
        public void OnDeselect(BaseEventData e){focused=false;Paint();}
        public void OnMove(AxisEventData e){int dx=e.moveDir==MoveDirection.Left?-1:e.moveDir==MoveDirection.Right?1:0;
            int dy=e.moveDir==MoveDirection.Up?-1:e.moveDir==MoveDirection.Down?1:0;if(dx!=0||dy!=0){Navigate?.Invoke(Id,dx,dy);e.Use();}}
        public void ResetState(){hovered=focused=false;Paint();}
        void OnDisable(){ResetState();}
    }
}
