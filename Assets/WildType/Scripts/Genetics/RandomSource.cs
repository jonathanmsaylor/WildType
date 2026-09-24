using System;

namespace WildType
{
    public interface IRandomSource
    {
        double Next01();
    }

    public sealed class SeededRandomSource : IRandomSource
    {
        readonly Random random;
        public SeededRandomSource(int seed) { random = new Random(seed); }
        public double Next01() => random.NextDouble();
    }
}
