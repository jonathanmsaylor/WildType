using System.Text;
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
        TMP_Text readout, prompt, notice, panelTitle, region, family, mating, descendantTitle, turnover;
        RectTransform familyBox;
        PlayerInputBridge input;
        readonly StringBuilder turnoverText = new StringBuilder(640);
        Image energy, stamina, health;
        GameObject pausePanel;
        GameObject namePanel;
        TMP_InputField nameInput;
        TMP_Text namePreview;
        CreatureId namingId;
        Button resume, nextPage, chronicleToggle;
        bool chronicle;
        LineageArchive journalArchive;
        CreatureId journalFocus;
        int journalRevision = -1;
        readonly System.Collections.Generic.List<CreatureLineageRecord> chronicleRecords = new System.Collections.Generic.List<CreatureLineageRecord>(LineageArchive.Capacity);
        readonly Button[] descendantButtons = new Button[4];
        readonly Button[] findButtons = new Button[4];
        readonly CreatureAgent[] choices = new CreatureAgent[4];
        TMP_Text[] choiceLabels = new TMP_Text[4];
        int page;
        FoodPlant highlighted;
        float refresh;
        public void Configure(StageSession value)
        {
            session = value;
            input = GetComponent<PlayerInputBridge>();
            var root = new GameObject("WildType HUD", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            root.transform.SetParent(transform);
            var canvas = root.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = root.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080); scaler.matchWidthOrHeight = .5f;
            root.AddComponent<FamilyWorldCues>().Configure(session, root.GetComponent<RectTransform>());
            var info = Box(root.transform, "Survival", new Vector2(24, -24), new Vector2(300, 270), new Vector2(0, 1));
            Text(info, "W I L D T Y P E", new Vector2(20, -13), new Vector2(260, 32), 25, new Color(.88f, .95f, .73f));
            energy = Bar(info, "Energy", 52, new Color(.66f, .84f, .32f));
            stamina = Bar(info, "Stamina", 90, new Color(.26f, .73f, .82f));
            health = Bar(info, "Health", 128, new Color(.93f, .51f, .36f));
            readout = Text(info, "", new Vector2(20, -175), new Vector2(260, 86), 18, Color.white);
            region = Text(root.transform, "", new Vector2(-28, -26), new Vector2(560, 64), 20, Color.white, new Vector2(1, 1));
            region.alignment = TextAlignmentOptions.Right;
            var turnoverBox = Box(root.transform, "Recent turnover", new Vector2(-24, 24), new Vector2(500, 190), new Vector2(1, 0));
            Text(turnoverBox, "RECENT BIRTHS / DEATHS · last 4", new Vector2(14, -10), new Vector2(472, 24), 17, new Color(.87f, .93f, .81f));
            turnover = Text(turnoverBox, "", new Vector2(14, -35), new Vector2(472, 147), 16, Color.white);
            turnover.gameObject.name = "Turnover events";
            familyBox = Box(root.transform, "Lineage", new Vector2(24, -314), new Vector2(340, 255), new Vector2(0, 1));
            family = Text(familyBox, "", new Vector2(20, -18), new Vector2(300, 219), 20, new Color(.92f, .98f, .88f));
            family.lineSpacing = 6;
            family.gameObject.name = "Inherited traits";
            family.textWrappingMode = TextWrappingModes.Normal;
            mating = Text(root.transform, "", new Vector2(0, 165), new Vector2(1100, 40), 23, new Color(.82f, .97f, .86f), new Vector2(.5f, 0));
            mating.alignment = TextAlignmentOptions.Center;
            prompt = Text(root.transform, "", new Vector2(0, 110), new Vector2(900, 42), 27, new Color(1, .85f, .4f), new Vector2(.5f, 0));
            prompt.alignment = TextAlignmentOptions.Center;
            notice = Text(root.transform, "", new Vector2(0, -44), new Vector2(850, 50), 27, new Color(.87f, .98f, .72f), new Vector2(.5f, 1));
            notice.alignment = TextAlignmentOptions.Center;
            var pause = Box(root.transform, "Pause", Vector2.zero, new Vector2(1000, 550), new Vector2(.5f, .5f));
            pause.GetComponent<Image>().color = new Color(.035f, .075f, .075f, .55f);
            pausePanel = pause.gameObject;
            panelTitle = Text(pause, "FIELD JOURNAL", new Vector2(35, -25), new Vector2(300, 90), 23, Color.white);
            panelTitle.textWrappingMode = TextWrappingModes.Normal;
            panelTitle.gameObject.name = "Journal title";
            resume = ButtonAt(pause, "Resume", 125, () => session.SetPaused(false));
            ButtonAt(pause, "Restart Prototype", 193, () => session.Restart(false));
            ButtonAt(pause, "Reseed Ecosystem", 261, () => session.Restart(true));
            ButtonAt(pause, "Quit", 329, session.Quit);
            Text(pause, "Locate: briefly reveal a relative.\nCreature card: take control.\nNo healing or energy refill.", new Vector2(35, -410), new Vector2(300, 110), 18, Color.white);
            var selection = Box(pause, "Descendants", new Vector2(360, -15), new Vector2(620, 520), new Vector2(0, 1));
            selection.GetComponent<Image>().color = new Color(.035f, .075f, .075f, .18f);
            descendantTitle = Text(selection, "", new Vector2(16, -12), new Vector2(580, 76), 22, Color.white);
            descendantTitle.textWrappingMode = TextWrappingModes.Normal;
            for (int i = 0; i < descendantButtons.Length; i++)
            {
                int slot = i;
                var button = ButtonAt(selection, "", 92 + i * 84, () => { if (session.TakeControl(choices[slot])) { page = 0; refresh = 0; } });
                button.GetComponent<RectTransform>().anchoredPosition = new Vector2(15, -92 - i * 84);
                button.GetComponent<RectTransform>().sizeDelta = new Vector2(495, 78);
                descendantButtons[i] = button; choiceLabels[i] = button.GetComponentInChildren<TMP_Text>(); choiceLabels[i].fontSize = 18;
                choiceLabels[i].alignment = TextAlignmentOptions.Left;
                choiceLabels[i].rectTransform.sizeDelta = new Vector2(475, 72);
                choiceLabels[i].rectTransform.anchoredPosition = new Vector2(10, -4);
                var find = ButtonAt(selection, "Locate", 92 + i * 84, () => session.Locator.Select(choices[slot]));
                find.GetComponent<RectTransform>().anchoredPosition = new Vector2(522, -92 - i * 84);
                find.GetComponent<RectTransform>().sizeDelta = new Vector2(78, 78);
                find.GetComponentInChildren<TMP_Text>().rectTransform.sizeDelta = new Vector2(78, 40);
                find.GetComponentInChildren<TMP_Text>().fontSize = 18;
                findButtons[i] = find;
            }
            nextPage = ButtonAt(selection, "Next page", 452, () => { page++; refresh = 0; });
            nextPage.GetComponent<RectTransform>().anchoredPosition = new Vector2(15, -452);
            chronicleToggle = ButtonAt(selection, "Chronicle", 452, () => { chronicle = !chronicle; page = 0; refresh = 0; });
            chronicleToggle.GetComponent<RectTransform>().anchoredPosition = new Vector2(335, -452);
            chronicleToggle.GetComponent<RectTransform>().sizeDelta = new Vector2(265, 55);
            chronicleToggle.GetComponentInChildren<TMP_Text>().rectTransform.sizeDelta = new Vector2(265, 42);
            pausePanel.SetActive(false);
            BuildNaming(root.transform);
            var events = new GameObject("UI input", typeof(EventSystem), typeof(InputSystemUIInputModule));
            events.transform.SetParent(root.transform);
            events.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
        }
        void Update()
        {
            if (!session || !session.Ready || !session.Player) return;
            session.Names.ValidatePrompt();
            bool naming = session.Names.HasPrompt;
            namePanel.SetActive(naming);
            if (naming && namingId != session.Names.Pending)
            {
                namingId = session.Names.Pending; nameInput.text = "";
                namePreview.text = "Default: " + session.Names.PersonalName(namingId);
                nameInput.Select(); nameInput.ActivateInputField();
            }
            if (!naming) namingId = default;
            bool opening = session.Paused && !naming && !pausePanel.activeSelf;
            if (opening) refresh = 0;
            pausePanel.SetActive(session.Paused && !naming);
            if (opening && EventSystem.current) EventSystem.current.SetSelectedGameObject(resume.gameObject);
            resume.interactable = !session.GameOver;
            refresh -= Time.unscaledDeltaTime; if (refresh > 0) return; refresh = .1f;
            panelTitle.text = "FAMILY JOURNAL\nSimulation paused";
            var a = session.Player; var v = a.Vitals;
            var generations = session.Generations; var record = generations.Archive.Get(a.Life.Id);
            if (journalArchive != generations.Archive || journalFocus != a.Life.Id)
            {
                journalArchive = generations.Archive; journalFocus = a.Life.Id;
                chronicle = false; page = 0; journalRevision = -1; chronicleRecords.Clear();
            }
            familyBox.sizeDelta = new Vector2(session.Paused ? 410 : 340, session.Paused ? 650 : 255);
            family.rectTransform.sizeDelta = new Vector2(session.Paused ? 370 : 300, session.Paused ? 614 : 219);
            family.fontSize = session.Paused ? 17 : 20; family.lineSpacing = session.Paused ? 4 : 6;
            string personalName = session.Names.PersonalName(a.Life.Id);
            family.text = (personalName.Length > 0 ? personalName + "\n" : "") + $"{GenerationLoop.ShortId(a.Life.Id)} · GENERATION {record.Generation}\n" +
                $"{(a.Life.Adult ? "Adult" : "Juvenile")} · Age {a.Life.Age:0} / {a.Stats.LifespanSeconds:0}s\n\n" +
                $"Parents {GenerationLoop.ShortId(record.FirstParentId)} + {GenerationLoop.ShortId(record.SecondParentId)}\n" +
                $"Living children {generations.LivingChildren(a.Life.Id)} / born {record.OffspringCount}\n\n";
            family.text += session.Paused ? generations.Archive.Changes(a.Life.Id) + InheritanceSummary.NotableMutation(record.ImportantMutations) :
                $"{CreatureAppearance.CoatName(a.Genome.camouflage)} · size {a.Genome.bodySize:0.00} · legs {a.Genome.legLength:0.00}\n\n{input.JournalKey} · Family journal";
            if (session.Paused) familyBox.sizeDelta = new Vector2(410, Mathf.Clamp(family.preferredHeight + 36, 255, 650));
            else if (personalName.Length > 0) { family.rectTransform.sizeDelta = new Vector2(300, 275); familyBox.sizeDelta = new Vector2(340, Mathf.Clamp(family.preferredHeight + 36, 255, 311)); }
            mating.text = "";
            var careTarget = session.Care.NearbyChild(a);
            if (!session.Paused && careTarget && !generations.Reserved(a))
            {
                string reason = session.Care.Reason(a, careTarget);
                if (reason.Length == 0 && FamilyCareRules.Transfer(v.Energy, a.Stats.MaxEnergy, careTarget.Vitals.Energy, careTarget.Stats.MaxEnergy, out float cost, out float gain))
                    mating.text = $"{input.CareKey}: share with {GenerationLoop.ShortId(careTarget.Life.Id)} · you −{cost:0.0} → child +{gain:0.0} energy";
            }
            if (session.Paused)
            {
                var living = generations.LivingDescendants(a.Life.Id);
                if (session.GameOver) panelTitle.text = living.Count == 0 ? "LIFE ENDED\nNo living descendants\nRestart or reseed" : "LIFE ENDED\nChoose a descendant or restart";
                if (chronicle && journalRevision != journalArchive.Revision)
                { journalArchive.CollectFamily(journalFocus, chronicleRecords); journalRevision = journalArchive.Revision; }
                int count = chronicle ? chronicleRecords.Count : living.Count;
                int pageSize = chronicle ? 3 : choices.Length;
                int pages = Mathf.Max(1, Mathf.CeilToInt(count / (float)pageSize)); page %= pages;
                nextPage.gameObject.SetActive(pages > 1);
                chronicleToggle.GetComponentInChildren<TMP_Text>().text = chronicle ? "Living descendants" : "Chronicle";
                descendantTitle.text = living.Count == 0 ? (session.GameOver ? "No living descendants.\nRestart or reseed to begin a new lineage." : "No living descendants yet.\nSurvive, build energy, and mate to grow your family.") : $"Living descendants: {living.Count} · page {page + 1}/{pages}\nSelect a creature to take control.";
                if (chronicle) descendantTitle.text = $"Family chronicle · page {page + 1}/{pages}\nRecorded lives: ancestors, you & descendants";
                for (int i = 0; i < choices.Length; i++)
                {
                    int index = page * pageSize + i;
                    float rowY = 92 + i * (chronicle ? 112 : 84), rowHeight = chronicle ? 106 : 78;
                    var cardRect = descendantButtons[i].GetComponent<RectTransform>();
                    cardRect.anchoredPosition = new Vector2(15, -rowY); cardRect.sizeDelta = new Vector2(495, rowHeight);
                    choiceLabels[i].rectTransform.sizeDelta = new Vector2(475, rowHeight - 6);
                    var locateRect = findButtons[i].GetComponent<RectTransform>();
                    locateRect.anchoredPosition = new Vector2(522, -rowY); locateRect.sizeDelta = new Vector2(78, rowHeight);
                    if (chronicle)
                    {
                        var entry = i < pageSize && index < count ? chronicleRecords[index] : null;
                        var actor = entry == null ? null : LineageChronicle.LivingActor(session, entry);
                        choices[i] = actor && generations.IsLivingDescendant(actor, a.Life.Id) ? actor : null;
                        descendantButtons[i].gameObject.SetActive(entry != null);
                        descendantButtons[i].interactable = choices[i];
                        findButtons[i].gameObject.SetActive(choices[i]); findButtons[i].interactable = !session.GameOver;
                        choiceLabels[i].fontSize = 18;
                        // Keep archived records readable even though they cannot be clicked.
                        var colors = descendantButtons[i].colors; colors.disabledColor = Color.white; descendantButtons[i].colors = colors;
                        if (entry != null) choiceLabels[i].text = LineageChronicle.Card(session, entry, actor, choices[i]);
                        continue;
                    }
                    choices[i] = index < living.Count ? living[index] : null;
                    descendantButtons[i].interactable = true; choiceLabels[i].fontSize = 18;
                    descendantButtons[i].gameObject.SetActive(choices[i]);
                    findButtons[i].gameObject.SetActive(choices[i]);
                    findButtons[i].interactable = !session.GameOver;
                    if (!choices[i]) continue;
                    var child = choices[i]; var childRecord = generations.Archive.Get(child.Life.Id);
                    choiceLabels[i].text = $"<size=21>{session.Names.Label(child.Life.Id)} · Gen {childRecord.Generation}</size>\n" +
                        $"{(child.Life.Adult ? "Adult" : "Juvenile")} · Energy {child.Vitals.Energy:0}/{child.Stats.MaxEnergy:0} · HP {child.Vitals.Health:0} · {Vector3.Distance(a.transform.position, child.transform.position):0} m\n" +
                        $"{session.World.Zone(child.transform.position)} · {CreatureAppearance.CoatName(child.Genome.camouflage)} · size {child.Genome.bodySize:0.00} / legs {child.Genome.legLength:0.00}";
                }
                if (opening && !chronicle && session.GameOver && living.Count > 0 && EventSystem.current)
                    EventSystem.current.SetSelectedGameObject(descendantButtons[0].gameObject);
            }
            energy.fillAmount = v.Energy / a.Stats.MaxEnergy; stamina.fillAmount = v.Stamina / a.Stats.MaxStamina; health.fillAmount = v.Health / 100;
            energy.rectTransform.localScale = new Vector3(energy.fillAmount, 1, 1);
            stamina.rectTransform.localScale = new Vector3(stamina.fillAmount, 1, 1);
            health.rectTransform.localScale = new Vector3(health.fillAmount, 1, 1);
            stamina.color = v.SprintLocked ? new Color(1, .6f, .22f) : new Color(.26f, .73f, .82f);
            readout.text = $"Energy {v.Energy:0}/{a.Stats.MaxEnergy:0} · HP {v.Health:0}\nStamina {v.Stamina:0}/{a.Stats.MaxStamina:0}\n{a.Motor.Speed:0.0} m/s · {a.State}";
            region.text = session.World.Zone(a.transform.position) + $"\nPopulation {session.Population}/{GenerationLoop.PopulationCap} · Births {generations.Births} · Deaths {generations.Deaths}";
            RefreshTurnover(generations.Turnover);
            if (highlighted) highlighted.Highlight(false);
            highlighted = a.Interaction.Nearest();
            if (highlighted && !session.Paused && v.Energy < a.Stats.MaxEnergy - .5f) highlighted.Highlight(true);
            prompt.text = !session.Paused && v.Energy <= 0 ? "STARVING — find ripe forage" : "";
            notice.text = session.Paused ? "" : session.CurrentNotice;
        }
        void RefreshTurnover(TurnoverHistory history)
        {
            turnoverText.Clear();
            foreach (var entry in history.Recent)
            {
                if (turnoverText.Length > 0) turnoverText.Append('\n');
                turnoverText.Append(entry.Kind == TurnoverKind.Birth ? "<color=#C5E893>Born</color> " : "<color=#FFB99B>Died</color> ");
                turnoverText.Append(GenerationLoop.ShortId(entry.CreatureId)).Append(" · Gen ").Append(entry.Generation);
                if (entry.Kind == TurnoverKind.Death)
                {
                    if (entry.Cause == CreatureDeathCause.Starvation) turnoverText.Append(" · starvation");
                    else if (entry.Cause == CreatureDeathCause.OldAge) turnoverText.Append(" · old age");
                    else turnoverText.Append(" · cause unknown");
                }
                turnoverText.Append('\n');
                if (entry.FirstParentId.IsValid || entry.SecondParentId.IsValid)
                    turnoverText.Append("Parents ").Append(GenerationLoop.ShortId(entry.FirstParentId)).Append(" + ").Append(GenerationLoop.ShortId(entry.SecondParentId)).Append(" · ");
                else turnoverText.Append("Founder · ");
                turnoverText.Append("Lineage ").Append(GenerationLoop.ShortId(entry.FounderId));
            }
            turnover.text = turnoverText.Length == 0 ? "No births or deaths yet this run." : turnoverText.ToString();
        }
        RectTransform Box(Transform parent, string label, Vector2 position, Vector2 size, Vector2 anchor)
        {
            var obj = new GameObject(label, typeof(RectTransform), typeof(Image)); obj.transform.SetParent(parent, false);
            var rect = obj.GetComponent<RectTransform>(); rect.anchorMin = rect.anchorMax = anchor;
            rect.pivot = anchor; rect.sizeDelta = size; rect.anchoredPosition = position;
            obj.GetComponent<Image>().color = new Color(.035f, .075f, .075f, .36f); return rect;
        }
        TMP_Text Text(Transform parent, string content, Vector2 position, Vector2 size, int fontSize, Color color, Vector2? anchorOverride = null)
        {
            var obj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI)); obj.transform.SetParent(parent, false);
            var rect = obj.GetComponent<RectTransform>(); Vector2 anchor = anchorOverride ?? new Vector2(0, 1);
            rect.anchorMin = rect.anchorMax = anchor; rect.pivot = anchor; rect.sizeDelta = size; rect.anchoredPosition = position;
            var text = obj.GetComponent<TextMeshProUGUI>(); text.font = session.hudFont; text.fontSize = fontSize; text.color = color; text.text = content;
            text.outlineWidth = .12f;
            text.raycastTarget = false; text.textWrappingMode = TextWrappingModes.NoWrap; return text;
        }
        Image Bar(Transform parent, string label, float y, Color color)
        {
            Text(parent, label.ToUpperInvariant(), new Vector2(20, -y), new Vector2(260, 20), 14, color);
            var track = Box(parent, label + " track", new Vector2(20, -y - 21), new Vector2(260, 8), new Vector2(0, 1));
            track.GetComponent<Image>().color = new Color(.12f, .2f, .2f);
            var fill = Box(track, label + " fill", Vector2.zero, new Vector2(260, 8), new Vector2(0, 1)).GetComponent<Image>();
            fill.color = color; fill.type = Image.Type.Filled; fill.fillMethod = Image.FillMethod.Horizontal; return fill;
        }
        Button ButtonAt(Transform parent, string title, float y, UnityEngine.Events.UnityAction callback)
        {
            var rect = Box(parent, title, new Vector2(35, -y), new Vector2(300, 55), new Vector2(0, 1));
            var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = rect.GetComponent<Image>();
            rect.GetComponent<Image>().color = new Color(.17f, .3f, .26f, .48f);
            var label = Text(rect, title, new Vector2(0, -8), new Vector2(300, 42), 24, Color.white); label.alignment = TextAlignmentOptions.Center;
            button.onClick.AddListener(callback); return button;
        }
        void BuildNaming(Transform root)
        {
            var panel = Box(root, "Name your child", Vector2.zero, new Vector2(620, 290), new Vector2(.5f, .5f));
            panel.GetComponent<Image>().color = new Color(.035f, .075f, .075f, .72f);
            namePanel = panel.gameObject;
            Text(panel, "WELCOME TO THE FAMILY", new Vector2(30, -22), new Vector2(560, 38), 26, Color.white);
            Text(panel, "Name your child · birth order is added automatically", new Vector2(30, -67), new Vector2(560, 30), 19, Color.white);
            var field = Box(panel, "Child name", new Vector2(30, -108), new Vector2(560, 48), new Vector2(0, 1));
            nameInput = field.gameObject.AddComponent<TMP_InputField>();
            nameInput.targetGraphic = field.GetComponent<Image>(); nameInput.characterLimit = 16;
            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D)); viewport.transform.SetParent(field, false);
            var view = viewport.GetComponent<RectTransform>(); view.anchorMin = Vector2.zero; view.anchorMax = Vector2.one;
            view.offsetMin = new Vector2(12, 2); view.offsetMax = new Vector2(-12, -2);
            nameInput.textViewport = view;
            nameInput.textComponent = Text(view, "", Vector2.zero, new Vector2(536, 44), 24, Color.white);
            nameInput.placeholder = Text(view, "Type a name (optional)", Vector2.zero, new Vector2(536, 44), 24, new Color(1, 1, 1, .55f));
            namePreview = Text(panel, "", new Vector2(30, -168), new Vector2(560, 30), 20, new Color(.88f, .95f, .73f));
            nameInput.onValueChanged.AddListener(value => namePreview.text = FamilyNames.CleanName(value).Length == 0 ? "Default: " + session.Names.PersonalName(session.Names.Pending) : FamilyNames.Format(value, session.Names.BirthOrder(session.Names.Pending)));
            nameInput.onSubmit.AddListener(value => { if (session.Names.HasPrompt) session.Names.Submit(value); });
            var confirm = ButtonAt(panel, "Name child", 214, () => session.Names.Submit(nameInput.text));
            confirm.GetComponent<RectTransform>().sizeDelta = new Vector2(260, 55); confirm.GetComponentInChildren<TMP_Text>().rectTransform.sizeDelta = new Vector2(260, 42);
            var skip = ButtonAt(panel, "Skip", 214, () => session.Names.Submit(""));
            skip.GetComponent<RectTransform>().anchoredPosition = new Vector2(325, -214); skip.GetComponent<RectTransform>().sizeDelta = new Vector2(260, 55);
            skip.GetComponentInChildren<TMP_Text>().rectTransform.sizeDelta = new Vector2(260, 42);
            namePanel.SetActive(false);
        }
        void OnDestroy() { if (highlighted) highlighted.Highlight(false); }
    }
}
