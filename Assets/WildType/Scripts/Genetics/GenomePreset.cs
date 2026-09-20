using UnityEngine;
namespace WildType
{
    [CreateAssetMenu(menuName = "WildType/Genome preset")]
    public sealed class GenomePreset : ScriptableObject
    {
        public string description;
        public Genome genome = new Genome();
        public Genome RuntimeCopy() { var copy = genome.Copy(); copy.Validate(); return copy; }
        void OnValidate() { if (genome == null) genome = new Genome(); genome.Validate(); }
    }
}
