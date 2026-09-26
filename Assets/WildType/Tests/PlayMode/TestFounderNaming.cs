using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace WildType.Tests
{
    // Tests explicitly accept the ordinary founder prompt with Eddy, including restart/reseed.
    // This lives only in the test assembly. New naming tests opt out to assert the paused modal itself.
    [SetUpFixture]
    public sealed class TestFounderNaming
    {
        public static bool Automatic = true;
        [OneTimeSetUp] public void Begin(){Automatic=true;SceneManager.sceneLoaded+=Loaded;}
        [OneTimeTearDown] public void End(){SceneManager.sceneLoaded-=Loaded;Automatic=true;}
        static void Loaded(Scene scene,LoadSceneMode mode)
        {
            foreach(var root in scene.GetRootGameObjects())
            foreach(var session in root.GetComponentsInChildren<StageSession>())session.gameObject.AddComponent<AcceptTestFounder>();
        }
    }
    public sealed class AcceptTestFounder : MonoBehaviour
    {
        void LateUpdate()
        {
            var session=GetComponent<StageSession>();
            if(TestFounderNaming.Automatic&&session.Ready&&session.Names.IsFounderPrompt)session.Names.Submit("Eddy");
        }
    }
}
