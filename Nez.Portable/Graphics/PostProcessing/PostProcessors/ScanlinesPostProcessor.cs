using Nez.Graphics.Effects;

namespace Nez.Graphics.PostProcessing.PostProcessors
{
    public class ScanlinesPostProcessor : PostProcessor<ScanlinesEffect>
    {
        public ScanlinesPostProcessor(int executionOrder) : base(executionOrder)
        {
            Effect = new ScanlinesEffect();
        }
    }
}