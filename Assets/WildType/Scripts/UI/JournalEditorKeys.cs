using UnityEngine;
using UnityEngine.EventSystems;
namespace WildType
{
    // Unity's Game view can consume brief native arrow taps before the Input System sees them.
    // Wait two frames and route only if normal UI navigation has not already changed selection.
    public sealed class JournalEditorKeys : MonoBehaviour
    {
        StageSession session;GameObject pending;MoveDirection direction;int frame=-1;
        void Awake(){session=GetComponent<StageSession>();}
        public void Queue(MoveDirection move)
        {
            if(!session||!session.Paused||session.Names.HasPrompt||!EventSystem.current)return;
            pending=EventSystem.current.currentSelectedGameObject;direction=move;frame=Time.frameCount;
        }
        void LateUpdate()
        {
            if(frame<0||Time.frameCount-frame<2)return;int requested=frame;frame=-1;
            if(!pending||!session.Paused||session.Names.HasPrompt||!EventSystem.current||EventSystem.current.currentSelectedGameObject!=pending||JournalFocusMarker.LastSelectionFrame>=requested)return;
            var e=new AxisEventData(EventSystem.current){moveDir=direction,moveVector=direction==MoveDirection.Up?Vector2.up:direction==MoveDirection.Down?Vector2.down:direction==MoveDirection.Left?Vector2.left:Vector2.right};
            ExecuteEvents.Execute(pending,e,ExecuteEvents.moveHandler);pending=null;
        }
#if UNITY_EDITOR
        void OnGUI()
        {
            if(Event.current.type!=EventType.KeyDown)return;
            var key=Event.current.keyCode;if(key==KeyCode.UpArrow)Queue(MoveDirection.Up);else if(key==KeyCode.DownArrow)Queue(MoveDirection.Down);else if(key==KeyCode.LeftArrow)Queue(MoveDirection.Left);else if(key==KeyCode.RightArrow)Queue(MoveDirection.Right);
        }
#endif
    }
}
