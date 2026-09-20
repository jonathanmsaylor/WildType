using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using TMPro;
namespace WildType
{
    public sealed class StageHud : MonoBehaviour
    {
        StageSession session;
        TMP_Text readout, prompt, notice, panelTitle, region;
        Image energy, stamina, health;
        GameObject pausePanel;
        Button resume;
        FoodPlant highlighted;
        float refresh;
        public void Configure(StageSession value)
        {
            session = value;
            var root = new GameObject("WildType HUD", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            root.transform.SetParent(transform);
            var canvas = root.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = root.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080); scaler.matchWidthOrHeight = .5f;
            var info = Box(root.transform, "Survival", new Vector2(28, -28), new Vector2(340, 352), new Vector2(0, 1));
            Text(info, "W I L D T Y P E", new Vector2(20, -15), new Vector2(300, 34), 28, new Color(.88f, .95f, .73f));
            energy = Bar(info, "Energy", 62, new Color(.66f, .84f, .32f));
            stamina = Bar(info, "Stamina", 108, new Color(.26f, .73f, .82f));
            health = Bar(info, "Health", 154, new Color(.93f, .51f, .36f));
            readout = Text(info, "", new Vector2(20, -207), new Vector2(305, 136), 19, Color.white);
            region = Text(root.transform, "", new Vector2(-28, -26), new Vector2(390, 44), 22, Color.white, new Vector2(1, 1));
            region.alignment = TextAlignmentOptions.Right;
            var help = Box(root.transform, "Controls", new Vector2(28, 25), new Vector2(720, 60), Vector2.zero);
            Text(help, "WASD / left stick  Move    Shift / L3  Sprint    E / A  Eat\nMouse / right stick  Orbit    Wheel  Zoom    Esc / Start  Pause", new Vector2(16, -9), new Vector2(690, 50), 18, new Color(.82f, .88f, .81f));
            prompt = Text(root.transform, "", new Vector2(0, 110), new Vector2(900, 42), 27, new Color(1, .85f, .4f), new Vector2(.5f, 0));
            prompt.alignment = TextAlignmentOptions.Center;
            notice = Text(root.transform, "", new Vector2(0, -44), new Vector2(850, 50), 27, new Color(.87f, .98f, .72f), new Vector2(.5f, 1));
            notice.alignment = TextAlignmentOptions.Center;
            var pause = Box(root.transform, "Pause", Vector2.zero, new Vector2(520, 450), new Vector2(.5f, .5f));
            pausePanel = pause.gameObject;
            panelTitle = Text(pause, "FIELD JOURNAL", new Vector2(35, -30), new Vector2(450, 70), 32, Color.white);
            resume = ButtonAt(pause, "Resume", 125, () => session.SetPaused(false));
            ButtonAt(pause, "Restart Prototype", 193, () => session.Restart(false));
            ButtonAt(pause, "Reseed Ecosystem", 261, () => session.Restart(true));
            ButtonAt(pause, "Quit", 329, session.Quit);
            pausePanel.SetActive(false);
            var events = new GameObject("UI input", typeof(EventSystem), typeof(InputSystemUIInputModule));
            events.transform.SetParent(root.transform);
            events.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
        }
        void Update()
        {
            if (!session || !session.Ready || !session.Player) return;
            bool opening = session.Paused && !pausePanel.activeSelf;
            pausePanel.SetActive(session.Paused);
            if (opening && EventSystem.current) EventSystem.current.SetSelectedGameObject(resume.gameObject);
            resume.interactable = !session.GameOver;
            panelTitle.text = session.GameOver ? "LIFE ENDED\nStart a new field test" : "FIELD JOURNAL\nSimulation paused";
            refresh -= Time.unscaledDeltaTime; if (refresh > 0) return; refresh = .1f;
            var a = session.Player; var v = a.Vitals;
            energy.fillAmount = v.Energy / a.Stats.MaxEnergy; stamina.fillAmount = v.Stamina / a.Stats.MaxStamina; health.fillAmount = v.Health / 100;
            energy.rectTransform.localScale = new Vector3(energy.fillAmount, 1, 1);
            stamina.rectTransform.localScale = new Vector3(stamina.fillAmount, 1, 1);
            health.rectTransform.localScale = new Vector3(health.fillAmount, 1, 1);
            stamina.color = v.SprintLocked ? new Color(1, .6f, .22f) : new Color(.26f, .73f, .82f);
            readout.text = $"Energy {v.Energy:0}/{a.Stats.MaxEnergy:0}   Health {v.Health:0}\nStamina {v.Stamina:0}/{a.Stats.MaxStamina:0}   Speed {a.Motor.Speed:0.0} m/s\nSize {a.Genome.bodySize:0.00}   Metabolism {a.Genome.metabolism:0.00}\nVision {a.Stats.Vision:0} m  •  {a.State}";
            region.text = session.World.Zone(a.transform.position) + $"\nPopulation {session.Population}  /  Food {session.World.AvailableFood}/{Ecosystem.FoodCap}";
            if (highlighted) highlighted.Highlight(false);
            highlighted = a.Interaction.Nearest();
            if (highlighted && !session.Paused) highlighted.Highlight(true);
            prompt.text = session.Paused ? "" : highlighted ? (v.Energy >= a.Stats.MaxEnergy - .5f ? "Brightfruit  /  Energy full" : "E / A  •  Eat brightfruit") : v.Energy <= 0 ? "STARVING — find brightfruit" : v.SprintLocked ? "Stamina depleted — recovering" : "";
            notice.text = session.Paused ? "" : session.CurrentNotice;
        }
        RectTransform Box(Transform parent, string label, Vector2 position, Vector2 size, Vector2 anchor)
        {
            var obj = new GameObject(label, typeof(RectTransform), typeof(Image)); obj.transform.SetParent(parent, false);
            var rect = obj.GetComponent<RectTransform>(); rect.anchorMin = rect.anchorMax = anchor;
            rect.pivot = anchor; rect.sizeDelta = size; rect.anchoredPosition = position;
            obj.GetComponent<Image>().color = new Color(.035f, .075f, .075f, .9f); return rect;
        }
        TMP_Text Text(Transform parent, string content, Vector2 position, Vector2 size, int fontSize, Color color, Vector2? anchorOverride = null)
        {
            var obj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI)); obj.transform.SetParent(parent, false);
            var rect = obj.GetComponent<RectTransform>(); Vector2 anchor = anchorOverride ?? new Vector2(0, 1);
            rect.anchorMin = rect.anchorMax = anchor; rect.pivot = anchor; rect.sizeDelta = size; rect.anchoredPosition = position;
            var text = obj.GetComponent<TextMeshProUGUI>(); text.font = session.hudFont; text.fontSize = fontSize; text.color = color; text.text = content;
            text.raycastTarget = false; text.textWrappingMode = TextWrappingModes.NoWrap; return text;
        }
        Image Bar(Transform parent, string label, float y, Color color)
        {
            Text(parent, label.ToUpperInvariant(), new Vector2(20, -y), new Vector2(300, 23), 15, color);
            var track = Box(parent, label + " track", new Vector2(20, -y - 24), new Vector2(300, 10), new Vector2(0, 1));
            track.GetComponent<Image>().color = new Color(.12f, .2f, .2f);
            var fill = Box(track, label + " fill", Vector2.zero, new Vector2(300, 10), new Vector2(0, 1)).GetComponent<Image>();
            fill.color = color; fill.type = Image.Type.Filled; fill.fillMethod = Image.FillMethod.Horizontal; return fill;
        }
        Button ButtonAt(Transform parent, string title, float y, UnityEngine.Events.UnityAction callback)
        {
            var rect = Box(parent, title, new Vector2(35, -y), new Vector2(450, 55), new Vector2(0, 1));
            var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = rect.GetComponent<Image>();
            rect.GetComponent<Image>().color = new Color(.17f, .3f, .26f);
            var label = Text(rect, title, new Vector2(0, -8), new Vector2(450, 42), 24, Color.white); label.alignment = TextAlignmentOptions.Center;
            button.onClick.AddListener(callback); return button;
        }
        void OnDestroy() { if (highlighted) highlighted.Highlight(false); }
    }
}
