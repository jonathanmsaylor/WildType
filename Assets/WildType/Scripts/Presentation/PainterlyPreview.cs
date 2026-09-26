using UnityEngine;
using UnityEngine.InputSystem;
namespace WildType
{
    // Only present in the separate preview scene. No singleton and no changes to ecosystem state.
    [DefaultExecutionOrder(100)]
    public sealed class PainterlyPreview:MonoBehaviour
    {
        public Material paintedMaterial;
        public bool painted=true;
        StageSession session;PainterlyMeadow meadow;bool ready,last;int toggleFrame=-1;
        Color fog,sky,equator,ground;float density;Light sun;Color sunColor;float sunIntensity;
        public bool Ready=>ready;
        public PainterlyMeadow Meadow=>meadow;
        void Update()
        {
            if(!session)session=FindAnyObjectByType<StageSession>();if(!session||!session.Ready||!paintedMaterial)return;
            if(!ready){fog=RenderSettings.fogColor;density=RenderSettings.fogDensity;sky=RenderSettings.ambientSkyColor;equator=RenderSettings.ambientEquatorColor;ground=RenderSettings.ambientGroundColor;sun=RenderSettings.sun;if(sun){sunColor=sun.color;sunIntensity=sun.intensity;}
                meadow=gameObject.AddComponent<PainterlyMeadow>();meadow.Build(session,paintedMaterial);ready=true;last=!painted;}
            foreach(var actor in session.Creatures){if(!actor||!actor.Visual)continue;var study=actor.Visual.GetComponent<PainterlyCreature>();if(!study){study=actor.Visual.gameObject.AddComponent<PainterlyCreature>();study.Build(actor,paintedMaterial);study.SetPainted(painted);}}
            if(last!=painted)Apply();
            if(Keyboard.current!=null&&Keyboard.current.f6Key.wasPressedThisFrame)Toggle();
        }
        public void SetPainted(bool value){painted=value;if(ready&&last!=painted)Apply();}
        void Toggle(){if(toggleFrame==Time.frameCount||!ready)return;toggleFrame=Time.frameCount;SetPainted(!painted);}
        void Apply(){session.Locator.Clear();last=painted;meadow.SetPainted(painted);foreach(var actor in session.Creatures)if(actor&&actor.Visual){var s=actor.Visual.GetComponent<PainterlyCreature>();if(s)s.SetPainted(painted);}
            RenderSettings.fogColor=painted?new Color(.63f,.73f,.79f):fog;RenderSettings.fogDensity=painted?.0035f:density;
            RenderSettings.ambientSkyColor=painted?new Color(.63f,.73f,.82f):sky;RenderSettings.ambientEquatorColor=painted?new Color(.48f,.50f,.34f):equator;RenderSettings.ambientGroundColor=painted?new Color(.24f,.29f,.25f):ground;
            if(sun){sun.color=painted?new Color(1,.94f,.79f):sunColor;sun.intensity=painted?1.55f:sunIntensity;}}
        void OnGUI(){if(!ready)return;var e=Event.current;if(e.type==EventType.KeyDown&&e.keyCode==KeyCode.F6){Toggle();e.Use();}GUI.Label(new Rect(Screen.width/2-175,8,350,25),painted?"PAINTERLY STUDY · F6 compare original":"ORIGINAL LOOK · F6 painterly study");}
    }
}
