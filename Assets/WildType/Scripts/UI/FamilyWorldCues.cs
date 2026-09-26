using TMPro;
using UnityEngine;
namespace WildType
{
    // Presentation only, outside VisualRoot. Three reusable family cues and at most 12 hearts.
    public sealed class FamilyWorldCues : MonoBehaviour
    {
        public const int CueCap = 3;
        StageSession session;
        RectTransform canvas;
        Camera cameraView;
        readonly TMP_Text[] labels = new TMP_Text[CueCap];
        readonly CreatureAgent[] children = new CreatureAgent[CueCap];
        readonly CourtshipHeart[] hearts = new CourtshipHeart[GenerationLoop.PopulationCap / 2];
        TMP_Text foodPrompt, matePrompt, locatePrompt;
        CreatureAgent lastLocated;
        public bool LocateCueVisible => locatePrompt && locatePrompt.gameObject.activeSelf;
        public bool DirectionCueVisible { get; private set; }
        public bool FoodPromptVisible => foodPrompt && foodPrompt.gameObject.activeSelf;
        public bool MatePromptVisible => matePrompt && matePrompt.gameObject.activeSelf;
        float refresh;
        public int VisibleChildren { get; private set; }
        public int VisibleHearts { get; private set; }
        public void Configure(StageSession value, RectTransform root)
        {
            session = value; canvas = root; cameraView = value.orbit.GetComponent<Camera>();
            foodPrompt = TargetLabel(root, "Food interaction prompt", new Color(1, .88f, .43f));
            matePrompt = TargetLabel(root, "Mating prompt", new Color(1, .73f, .81f));
            locatePrompt = TargetLabel(root, "Descendant locate cue", new Color(1, .9f, .66f));
            locatePrompt.fontSize = 22; locatePrompt.rectTransform.sizeDelta = new Vector2(500, 66);
            root.gameObject.AddComponent<DescendantPulse>().Configure(session);
            for (int i = 0; i < labels.Length; i++)
            {
                var go = new GameObject("Child relationship cue", typeof(RectTransform), typeof(TextMeshProUGUI)); go.transform.SetParent(root, false);
                var t = go.GetComponent<TextMeshProUGUI>(); t.font = session.hudFont; t.fontSize = 22;
                t.alignment = TextAlignmentOptions.Center; t.color = new Color(.88f, 1, .74f); t.outlineWidth = .18f;
                t.raycastTarget = false; t.textWrappingMode = TextWrappingModes.NoWrap; t.rectTransform.sizeDelta = new Vector2(500, 66);
                labels[i] = t; go.SetActive(false);
            }
            for (int i = 0; i < hearts.Length; i++)
            {
                var go = new GameObject("Courtship heart", typeof(RectTransform), typeof(CourtshipHeart)); go.transform.SetParent(root, false);
                hearts[i] = go.GetComponent<CourtshipHeart>(); hearts[i].color = new Color(1, .28f, .49f, .95f);
                hearts[i].raycastTarget = false; hearts[i].rectTransform.sizeDelta = new Vector2(52, 52); go.SetActive(false);
            }
        }
        bool InView(CreatureAgent child)
        {
            Vector3 point = cameraView.WorldToViewportPoint(child.transform.position + Vector3.up * child.CurrentHeight);
            return point.z > 0 && point.x > .04f && point.x < .96f && point.y > .07f && point.y < .93f &&
                !Physics.Linecast(cameraView.transform.position, child.transform.position + Vector3.up * child.CurrentHeight * .8f,
                    Ecosystem.WorldMask, QueryTriggerInteraction.Ignore);
        }
        void ChooseChildren()
        {
            for (int i = 0; i < children.Length; i++) children[i] = null;
            var parent = session.Player; int start = 0;
            var selected = session.Locator.Target;
            if (selected && session.Care.IsChild(parent, selected) && InView(selected) && Vector3.Distance(parent.transform.position, selected.transform.position) <= 25)
            { children[0] = selected; start = 1; }
            foreach (var child in session.Creatures)
            {
                if (!session.Care.IsChild(parent, child) || child == children[0] || !InView(child)) continue;
                float d = Vector3.Distance(parent.transform.position, child.transform.position); if (d > 25) continue;
                for (int i = start; i < children.Length; i++)
                    if (!children[i] || d < Vector3.Distance(parent.transform.position, children[i].transform.position))
                    { for (int j = children.Length - 1; j > i; j--) children[j] = children[j - 1]; children[i] = child; break; }
            }
        }
        void LateUpdate()
        {
            if (!session || !canvas || !cameraView) return;
            VisibleChildren = VisibleHearts = 0;
            bool active = session.Ready && !session.Paused && session.Player && !session.Player.Vitals.Dead;
            var located = session.Locator.Target;
            if (located != lastLocated) { lastLocated = located; refresh = 0; }
            refresh -= Time.unscaledDeltaTime;
            bool updateText = refresh <= 0;
            if (active && updateText) { refresh = .1f; ChooseChildren(); }
            var target = active ? session.Care.NearbyChild(session.Player) : null;
            for (int i = 0; i < labels.Length; i++)
            {
                var child = children[i];
                bool shown = active && session.Care.IsChild(session.Player, child) &&
                    Vector3.Distance(session.Player.transform.position, child.transform.position) <= 25 && InView(child);
                labels[i].gameObject.SetActive(shown); if (!shown) continue;
                VisibleChildren++;
                Vector3 point = child.transform.position + Vector3.up * (child.CurrentHeight + .55f);
                Place(labels[i].rectTransform, point, false);
                // Spread projected neighbours just enough to keep their small labels distinct.
                for (int j = 0; j < i; j++) if (labels[j].gameObject.activeSelf &&
                    Mathf.Abs(labels[i].rectTransform.anchoredPosition.x - labels[j].rectTransform.anchoredPosition.x) < 500 &&
                    Mathf.Abs(labels[i].rectTransform.anchoredPosition.y - labels[j].rectTransform.anchoredPosition.y) < 68)
                    labels[i].rectTransform.anchoredPosition += Vector2.up * 70;
                if (!updateText) continue;
                float distance = Vector3.Distance(session.Player.transform.position, child.transform.position);
                labels[i].text = "Your child " + session.Names.Label(child.Life.Id) + $" · {distance:0} m\nGen {session.Generations.Archive.Get(child.Life.Id).Generation} · " +
                    (child.Life.Adult ? "Adult" : $"Energy {child.Vitals.Energy:0}/{child.Stats.MaxEnergy:0}" +
                    (child == target && session.Care.Reason(session.Player, child).Length == 0 ? " · " + session.GetComponent<PlayerInputBridge>().CareKey + " share" : " · juvenile"));
                labels[i].color = child == target ? new Color(1, .88f, .43f) : new Color(.88f, 1, .74f);
            }
            DirectionCueVisible = false;
            bool ordinaryChildCue = false;
            for (int i = 0; i < children.Length; i++) if (children[i] == located && labels[i].gameObject.activeSelf) ordinaryChildCue = true;
            locatePrompt.gameObject.SetActive(active && located && !ordinaryChildCue);
            if (locatePrompt.gameObject.activeSelf)
            {
                Vector3 point = located.transform.position + Vector3.up * (located.CurrentHeight + .55f);
                Vector3 vp = cameraView.WorldToViewportPoint(point);
                DirectionCueVisible = vp.z <= 0 || vp.x < .04f || vp.x > .96f || vp.y < .07f || vp.y > .93f;
                Place(locatePrompt.rectTransform, point, DirectionCueVisible);
                locatePrompt.color = new Color(1, .9f, .66f, .9f * Mathf.Clamp01(session.Locator.Remaining / .6f));
                if (updateText)
                {
                    string bearing = DirectionCueVisible ? Bearing(vp) + " · " : InView(located) ? "" : "Obscured · ";
                    locatePrompt.text = bearing + session.Names.PersonalName(located.Life.Id) + "\n" + GenerationLoop.ShortId(located.Life.Id) +
                        $" · Gen {session.Generations.Archive.Get(located.Life.Id).Generation} · {Vector3.Distance(session.Player.transform.position, located.transform.position):0} m";
                }
            }
            for (int i = 0; i < hearts.Length; i++)
            {
                bool shown = false;
                if (active && session.Generations.CourtshipAt(i, out var first, out var second))
                {
                    Vector3 point = (first.transform.position + second.transform.position) * .5f + Vector3.up *
                        (Mathf.Max(first.CurrentHeight, second.CurrentHeight) + .65f + Mathf.Sin((float)session.Generations.Clock * 4) * .15f);
                    Vector3 vp = cameraView.WorldToViewportPoint(point);
                    shown = Vector3.Distance(session.Player.transform.position, point) < 40 && vp.z > 0 && vp.x > 0 && vp.x < 1 && vp.y > 0 && vp.y < 1 &&
                        !Physics.Linecast(cameraView.transform.position, point, Ecosystem.WorldMask, QueryTriggerInteraction.Ignore);
                    if (shown)
                    {
                        Place(hearts[i].rectTransform, point, false);
                        hearts[i].transform.localScale = Vector3.one * (1 + Mathf.Sin((float)session.Generations.Clock * 8) * .12f);
                        VisibleHearts++;
                    }
                }
                hearts[i].gameObject.SetActive(shown);
            }
            var food = active ? session.Player.Interaction.Nearest() : null;
            bool canEat = food && session.Player.Vitals.Energy < session.Player.Stats.MaxEnergy - .5f;
            Vector3 foodPoint = food ? food.transform.position + Vector3.up * (food.Region == Habitat.Dry ? 2.8f : 1.8f) : Vector3.zero;
            foodPrompt.gameObject.SetActive(canEat && PointVisible(foodPoint));
            if (foodPrompt.gameObject.activeSelf)
            {
                Place(foodPrompt.rectTransform, foodPoint, false);
                if (updateText) foodPrompt.text = session.GetComponent<PlayerInputBridge>().EatKey + " eat " + food.DisplayName + $"\n+{Mathf.Min(session.Player.Stats.MaxEnergy-session.Player.Vitals.Energy,food.nutrition*session.Player.Stats.NutritionFactor):0.0} energy";
            }
            var partner = active ? session.Generations.ReadyPlayerPartner() : null;
            Vector3 matePoint = partner ? partner.transform.position + Vector3.up * (partner.CurrentHeight + .7f) : Vector3.zero;
            matePrompt.gameObject.SetActive(partner && PointVisible(matePoint));
            if (matePrompt.gameObject.activeSelf)
            {
                Place(matePrompt.rectTransform, matePoint, false);
                if (updateText) matePrompt.text = session.GetComponent<PlayerInputBridge>().MateKey + " mate · " + session.Names.PersonalName(partner.Life.Id) + $"\nYou spend {session.Player.Stats.MaxEnergy*GenerationLoop.ParentEnergyCost:0.0} energy at birth";
            }
        }
        public static string Bearing(Vector3 viewport) => viewport.z <= 0 ? "Behind" : viewport.x < .04f ? "Left" : viewport.x > .96f ? "Right" : viewport.y < .07f ? "Below" : "Above";
        TMP_Text TargetLabel(RectTransform root, string name, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI)); go.transform.SetParent(root, false);
            var text = go.GetComponent<TextMeshProUGUI>(); text.font = session.hudFont; text.fontSize = 24; text.color = color;
            text.outlineWidth = .18f; text.alignment = TextAlignmentOptions.Center; text.raycastTarget = false;
            text.textWrappingMode = TextWrappingModes.Normal; text.rectTransform.sizeDelta = new Vector2(480, 80);
            var shadow=go.AddComponent<UnityEngine.UI.Shadow>();shadow.effectColor=new Color(0,0,0,.95f);shadow.effectDistance=new Vector2(2,-2);
            go.SetActive(false); return text;
        }
        bool PointVisible(Vector3 point)
        {
            var p = cameraView.WorldToViewportPoint(point);
            return p.z > 0 && p.x > .03f && p.x < .97f && p.y > .05f && p.y < .95f &&
                !Physics.Linecast(cameraView.transform.position, point, Ecosystem.WorldMask, QueryTriggerInteraction.Ignore);
        }
        void Place(RectTransform rect, Vector3 point, bool clamp)
        {
            Vector3 screen = cameraView.WorldToScreenPoint(point);
            if (clamp)
            {
                if (screen.z < 0) { screen.x = Screen.width - screen.x; screen.y = Screen.height * .25f; }
                screen.x = Mathf.Clamp(screen.x, Screen.width * .27f, Screen.width * .83f);
                screen.y = Mathf.Clamp(screen.y, Screen.height * .28f, Screen.height * .83f);
            }
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas, screen, null, out var local);
            rect.anchoredPosition = local;
        }
    }
}
