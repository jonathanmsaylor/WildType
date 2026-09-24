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
        TMP_Text readout, prompt, notice, panelTitle, region, family, mating, descendantTitle;
        Image energy, stamina, health;
        GameObject pausePanel;
        Button resume, nextPage;
        readonly Button[] descendantButtons = new Button[6];
        readonly CreatureAgent[] choices = new CreatureAgent[6];
        TMP_Text[] choiceLabels = new TMP_Text[6];
        int page;
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
            Text(help, "WASD / stick Move · Shift / L3 Sprint · E / A Eat · M / X Mate\nMouse / stick Orbit · Wheel Zoom · F / Y Family · Esc / Start Pause", new Vector2(16, -9), new Vector2(690, 50), 18, new Color(.82f, .88f, .81f));
            var familyBox = Box(root.transform, "Lineage", new Vector2(28, -395), new Vector2(410, 220), new Vector2(0, 1));
            family = Text(familyBox, "", new Vector2(16, -13), new Vector2(378, 190), 18, new Color(.87f, .93f, .81f));
            family.textWrappingMode = TextWrappingModes.Normal;
            mating = Text(root.transform, "", new Vector2(0, 165), new Vector2(1100, 40), 23, new Color(.82f, .97f, .86f), new Vector2(.5f, 0));
            mating.alignment = TextAlignmentOptions.Center;
            prompt = Text(root.transform, "", new Vector2(0, 110), new Vector2(900, 42), 27, new Color(1, .85f, .4f), new Vector2(.5f, 0));
            prompt.alignment = TextAlignmentOptions.Center;
            notice = Text(root.transform, "", new Vector2(0, -44), new Vector2(850, 50), 27, new Color(.87f, .98f, .72f), new Vector2(.5f, 1));
            notice.alignment = TextAlignmentOptions.Center;
            var pause = Box(root.transform, "Pause", Vector2.zero, new Vector2(1040, 570), new Vector2(.5f, .5f));
            pausePanel = pause.gameObject;
            panelTitle = Text(pause, "FIELD JOURNAL", new Vector2(35, -30), new Vector2(450, 70), 32, Color.white);
            resume = ButtonAt(pause, "Resume", 125, () => session.SetPaused(false));
            ButtonAt(pause, "Restart Prototype", 193, () => session.Restart(false));
            ButtonAt(pause, "Reseed Ecosystem", 261, () => session.Restart(true));
            ButtonAt(pause, "Quit", 329, session.Quit);
            Text(pause, "Continue as a living descendant\nNo healing or resource refill on transfer.", new Vector2(35, -410), new Vector2(450, 90), 21, Color.white);
            var selection = Box(pause, "Descendants", new Vector2(520, -20), new Vector2(490, 530), new Vector2(0, 1));
            descendantTitle = Text(selection, "", new Vector2(16, -12), new Vector2(460, 76), 20, Color.white);
            descendantTitle.textWrappingMode = TextWrappingModes.Normal;
            for (int i = 0; i < descendantButtons.Length; i++)
            {
                int slot = i;
                var button = ButtonAt(selection, "", 92 + i * 58, () => { if (session.TakeControl(choices[slot])) page = 0; });
                button.GetComponent<RectTransform>().anchoredPosition = new Vector2(15, -92 - i * 58);
                descendantButtons[i] = button; choiceLabels[i] = button.GetComponentInChildren<TMP_Text>(); choiceLabels[i].fontSize = 18;
            }
            nextPage = ButtonAt(selection, "Next page", 452, () => { page++; refresh = 0; });
            nextPage.GetComponent<RectTransform>().anchoredPosition = new Vector2(15, -452);
            pausePanel.SetActive(false);
            var events = new GameObject("UI input", typeof(EventSystem), typeof(InputSystemUIInputModule));
            events.transform.SetParent(root.transform);
            events.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
        }
        void Update()
        {
            if (!session || !session.Ready || !session.Player) return;
            bool opening = session.Paused && !pausePanel.activeSelf;
            if (opening) refresh = 0;
            pausePanel.SetActive(session.Paused);
            if (opening && EventSystem.current) EventSystem.current.SetSelectedGameObject(resume.gameObject);
            resume.interactable = !session.GameOver;
            panelTitle.text = session.GameOver ? "LIFE ENDED\nChoose a descendant or restart" : "FAMILY JOURNAL\nSimulation paused";
            refresh -= Time.unscaledDeltaTime; if (refresh > 0) return; refresh = .1f;
            var a = session.Player; var v = a.Vitals;
            var generations = session.Generations; var record = generations.Archive.Get(a.Life.Id);
            family.text = $"{GenerationLoop.ShortId(a.Life.Id)} · GENERATION {record.Generation} · {(a.Life.Adult ? "ADULT" : "JUVENILE")}\n" +
                $"Age {a.Life.Age:0}s / lifespan {a.Stats.LifespanSeconds:0}s\n" +
                $"Parents {GenerationLoop.ShortId(record.FirstParentId)} + {GenerationLoop.ShortId(record.SecondParentId)}\n" +
                $"Living children {generations.LivingChildren(a.Life.Id)} / born {record.OffspringCount}\n" + generations.Archive.Changes(a.Life.Id);
            var mutations = record.ImportantMutations;
            if (mutations.Length > 0) family.text += "\nMutation: " + mutations[0].TraitName;
            mating.text = session.Paused ? "" : generations.PlayerHint();
            if (session.Paused)
            {
                var living = generations.LivingDescendants(a.Life.Id);
                int pages = Mathf.Max(1, Mathf.CeilToInt(living.Count / 6f)); page %= pages;
                nextPage.gameObject.SetActive(pages > 1);
                descendantTitle.text = living.Count == 0 ? (session.GameOver ? "No living descendants.\nRestart or reseed to begin a new lineage." : "No living descendants yet.\nSurvive, build energy, and mate to grow your family.") : $"Living descendants: {living.Count} · page {page + 1}/{pages}\nSelect a creature to take control.";
                for (int i = 0; i < choices.Length; i++)
                {
                    int index = page * choices.Length + i;
                    choices[i] = index < living.Count ? living[index] : null;
                    descendantButtons[i].gameObject.SetActive(choices[i]);
                    if (!choices[i]) continue;
                    var child = choices[i]; var childRecord = generations.Archive.Get(child.Life.Id);
                    choiceLabels[i].text = $"{GenerationLoop.ShortId(child.Life.Id)} · Gen {childRecord.Generation} · {(child.Life.Adult ? "Adult" : "Juvenile")} · E {child.Vitals.Energy:0} / HP {child.Vitals.Health:0}";
                }
                if (opening && session.GameOver && living.Count > 0 && EventSystem.current)
                    EventSystem.current.SetSelectedGameObject(descendantButtons[0].gameObject);
            }
            energy.fillAmount = v.Energy / a.Stats.MaxEnergy; stamina.fillAmount = v.Stamina / a.Stats.MaxStamina; health.fillAmount = v.Health / 100;
            energy.rectTransform.localScale = new Vector3(energy.fillAmount, 1, 1);
            stamina.rectTransform.localScale = new Vector3(stamina.fillAmount, 1, 1);
            health.rectTransform.localScale = new Vector3(health.fillAmount, 1, 1);
            stamina.color = v.SprintLocked ? new Color(1, .6f, .22f) : new Color(.26f, .73f, .82f);
            readout.text = $"Energy {v.Energy:0}/{a.Stats.MaxEnergy:0}   Health {v.Health:0}\nStamina {v.Stamina:0}/{a.Stats.MaxStamina:0}   Speed {a.Motor.Speed:0.0} m/s\nSize {a.Genome.bodySize:0.00}   Metabolism {a.Genome.metabolism:0.00}\nVision {a.Stats.Vision:0} m  •  {a.State}";
            region.text = session.World.Zone(a.transform.position) + $"\nPopulation {session.Population}/{GenerationLoop.PopulationCap} · Births {generations.Births} · Food {session.World.AvailableFood}/{Ecosystem.FoodCap}";
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
