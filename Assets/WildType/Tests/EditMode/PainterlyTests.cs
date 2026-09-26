using NUnit.Framework;
using UnityEngine;
namespace WildType.Tests
{
    public sealed class PainterlyTests
    {
        [Test] public void OrganicBodyIsDeterministicFiniteAndUsesInheritedDimensions()
        {
            var genome=new Genome();genome.Validate();var a=new CreatureAppearance(genome);
            var first=PainterMesh.Body(a);var second=PainterMesh.Body(a);
            try {CollectionAssert.AreEqual(first.vertices,second.vertices);CollectionAssert.AreEqual(first.triangles,second.triangles);
                Assert.Less(first.vertexCount,2000);Assert.Greater(first.vertexCount,1000);
                foreach(var v in first.vertices)Assert.True(CreatureMotor.Finite(v));
                var longer=genome.Copy();longer.legLength=1.6f;var high=PainterMesh.Body(new CreatureAppearance(longer));
                try {Assert.Greater(high.bounds.center.y,first.bounds.center.y);Assert.Greater(high.bounds.size.z,first.bounds.size.z);}finally{Object.DestroyImmediate(high);}
            }finally{Object.DestroyImmediate(first);Object.DestroyImmediate(second);}
        }
        [Test] public void GeometryDoesNotConsumeUnityRandomOrModifyGenome()
        {
            var g=new Genome();g.Validate();string before=JsonUtility.ToJson(g);var random=Random.state;
            var mesh=PainterMesh.Body(new CreatureAppearance(g));try{Assert.AreEqual(before,JsonUtility.ToJson(g));Assert.AreEqual(random,Random.state);}finally{Object.DestroyImmediate(mesh);}
        }
        [TestCase(.65f,.6f)] [TestCase(1.7f,1.6f)] [TestCase(1.5f,.8f)]
        public void ExtremeInheritedBodiesHaveValidTopology(float size,float legs)
        {
            var g=new Genome{bodySize=size,legLength=legs};g.Validate();var mesh=PainterMesh.Body(new CreatureAppearance(g));
            try{foreach(var v in mesh.vertices)Assert.True(CreatureMotor.Finite(v));foreach(int i in mesh.triangles)Assert.That(i,Is.InRange(0,mesh.vertexCount-1));foreach(var n in mesh.normals)Assert.True(CreatureMotor.Finite(n));Assert.Greater(mesh.bounds.size.y,.1f);}finally{Object.DestroyImmediate(mesh);}
        }
    }
}
