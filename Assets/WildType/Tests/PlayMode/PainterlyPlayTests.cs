using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace WildType.Tests
{
    public sealed class PainterlyPlayTests
    {
        StageSession session;PainterlyPreview preview;
        [UnitySetUp] public IEnumerator Setup(){
#if UNITY_EDITOR
            // Test the saved comparison scene without rewriting the user's local build list.
            yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/WildType/Scenes/CreatureStage_PainterlyPreview.unity",new LoadSceneParameters(LoadSceneMode.Single));
#else
            yield return SceneManager.LoadSceneAsync("CreatureStage_PainterlyPreview");
#endif
            session=Object.FindAnyObjectByType<StageSession>();session.autoPauseOnFocusLoss=false;session.Names.ShowPrompts=false;session.GetComponent<PlayerInputBridge>().enabled=false;yield return null;Freeze();session.SetPaused(true);
            preview=Object.FindAnyObjectByType<PainterlyPreview>();yield return null;yield return null;Assert.True(preview.Ready);}
        [UnityTearDown] public IEnumerator Cleanup(){if(session)session.SetPaused(false);yield return null;Time.timeScale=1;}
        void Freeze(){foreach(var a in session.Creatures){var b=a.GetComponent<HerbivoreBrain>();if(b)b.enabled=false;a.DesiredDirection=Vector3.zero;}}
        void Place(CreatureAgent a,float x,float z)=>a.Motor.Teleport(new Vector3(x,Ecosystem.Height(x,z)+.15f,z));
        [UnityTest] public IEnumerator ComparisonPreservesSimulationCollidersFoodAndRestoresVisuals()
        {
            var actors=session.Creatures.ToArray();var genomes=actors.Select(a=>JsonUtility.ToJson(a.Genome)).ToArray();var positions=actors.Select(a=>a.transform.position).ToArray();var energies=actors.Select(a=>a.Vitals.Energy).ToArray();
            var terrain=GameObject.Find("250m ecosystem terrain");var collider=terrain.GetComponent<MeshCollider>();var source=collider.sharedMesh;
            Assert.AreNotSame(source,terrain.GetComponent<MeshFilter>().sharedMesh);
            Assert.AreEqual(0,preview.GetComponentsInChildren<Collider>().Length);Assert.Greater(preview.Meadow.TreeCount,0);Assert.Less(preview.Meadow.GeneratedVertices,1000000);
            for(int i=0;i<4;i++){preview.SetPainted(i%2==0);yield return null;}
            Assert.AreSame(source,terrain.GetComponent<MeshFilter>().sharedMesh);Assert.AreSame(source,collider.sharedMesh);
            CollectionAssert.AreEqual(genomes,actors.Select(a=>JsonUtility.ToJson(a.Genome)).ToArray());CollectionAssert.AreEqual(positions,actors.Select(a=>a.transform.position).ToArray());CollectionAssert.AreEqual(energies,actors.Select(a=>a.Vitals.Energy).ToArray());
            Assert.AreEqual(72,session.World.Foods.Count);Assert.AreEqual(13,session.Population);
            preview.SetPainted(true);yield return null;session.SetPaused(false);var food=session.World.Foods[0];Assert.Greater(food.Consume(),0);Assert.False(food.fruit.gameObject.activeSelf);food.Tick(120);Assert.True(food.Available);Assert.True(food.fruit.gameObject.activeSelf);
        }
        [UnityTest] public IEnumerator PaintedChildGrowsKeepsGenomePulseAndControlThenReleasesMeshesOnRestart()
        {
            session.SetPaused(false);var parent=session.Player;var mate=session.Creatures[1];Place(parent,0,0);Place(mate,2,0);parent.Vitals.Eat(1000);mate.Vitals.Eat(1000);
            Assert.True(session.Generations.TryMate(parent,mate,out var reason),reason);yield return new WaitForSeconds(2.3f);Freeze();yield return null;
            var child=session.Creatures.Last();var art=child.Visual.GetComponent<PainterlyCreature>();Assert.NotNull(art);Assert.True(art.Painted);Assert.AreEqual(new CreatureAppearance(child.Genome),art.Appearance);Assert.Less(child.Visual.transform.localScale.x,.6f);
            var mesh=art.CoatMesh;var vertices=mesh.vertices;
            var coat=child.Visual.transform.Find("Painterly visual study/Continuous body and neck").GetComponent<Renderer>();var block=new MaterialPropertyBlock();coat.GetPropertyBlock(block);var originalTint=block.GetColor("_BaseColor");
            Assert.True(session.Locator.Select(child));yield return new WaitForSeconds(.35f);coat.GetPropertyBlock(block);Assert.AreNotEqual(originalTint,block.GetColor("_BaseColor"));
            session.Locator.Clear();yield return null;yield return null;coat.GetPropertyBlock(block);Assert.AreEqual(originalTint,block.GetColor("_BaseColor"));
            Assert.True(session.TakeControl(child));Freeze();Time.timeScale=15;yield return new WaitForSeconds(child.Stats.MaturityAge+1);Time.timeScale=1;
            Assert.True(child.Life.Adult);Assert.AreEqual(Vector3.one,child.Visual.transform.localScale);Assert.AreSame(mesh,art.CoatMesh);CollectionAssert.AreEqual(vertices,mesh.vertices);
            Place(child,0,0);child.Vitals.Eat(1000);child.DesiredDirection=Vector3.forward;yield return new WaitForSeconds(1);Assert.Greater(child.Motor.Speed,1);child.DesiredDirection=Vector3.zero;
            session.SetPaused(true);session.Restart(false);yield return null;yield return new WaitForSecondsRealtime(.3f);Freeze();Assert.False(mesh);Assert.AreEqual(13,session.Population);Assert.AreEqual(72,session.World.Foods.Count);Assert.False(session.Locator.Target);Assert.AreEqual(0,session.Generations.Births);
            session.SetPaused(true);session.Restart(true);yield return null;yield return new WaitForSecondsRealtime(.3f);Assert.LessOrEqual(session.Population,24);Assert.LessOrEqual(session.Creatures.Count,32);Assert.LessOrEqual(session.Generations.Archive.Count,512);foreach(var a in session.Creatures)Assert.True(a.Visual.GetComponent<PainterlyCreature>().Painted);
        }
        [UnityTest] public IEnumerator FullPopulationRetainsBoundedPresentationAndRejectsAnotherBirth()
        {
            session.SetPaused(false);
            for(int round=0;round<3&&session.Population<24;round++){
                // Capacity fixture, not a survival soak: maintain energy while real cooldowns expire.
                if(round>0){Time.timeScale=20;for(int step=0;step<5;step++){foreach(var a in session.Creatures)a.Vitals.Eat(1000);yield return new WaitForSeconds(21);}Time.timeScale=1;Freeze();}
                foreach(var a in session.Creatures)a.Vitals.Eat(1000);
                int pair=0;foreach(var a in session.Creatures.ToArray()){
                    if(session.Generations.IndividualReason(a).Length>0)continue;
                    var b=session.Generations.FindPartner(a,float.MaxValue,true);if(!b)continue;
                    Place(a,pair*5-12,0);Place(b,pair*5-10,0);
                    if(session.Generations.TryMate(a,b,out _))pair++;
                }
                Assert.Greater(pair,0,"Fixture must choose compatible eligible partners, not arbitrary unlike presets");
                yield return new WaitForSeconds(2.4f);Freeze();
            }
            yield return null;Assert.AreEqual(24,session.Population);Assert.LessOrEqual(session.Creatures.Count,32);Assert.AreEqual(72,session.World.Foods.Count);Assert.LessOrEqual(session.Generations.Archive.Count,512);
            Assert.AreEqual(24,Object.FindObjectsByType<PainterlyCreature>(FindObjectsSortMode.None).Length);
            foreach(var a in session.Creatures){var art=a.Visual.GetComponent<PainterlyCreature>();Assert.True(art.Painted);Assert.AreEqual(new CreatureAppearance(a.Genome),art.Appearance);Assert.True(CreatureMotor.Finite(a.transform.position));}
            Assert.False(session.Generations.TryMate(session.Creatures[0],session.Creatures[1],out _));
        }
    }
}
