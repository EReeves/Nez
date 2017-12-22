namespace Nez.AI.UtilityAI
{
	/// <summary>
	///     The first Consideration to score above the score of the Default Consideration is selected
	/// </summary>
	public class FirstScoreReasoner<T> : Reasoner<T>
    {
        protected override IConsideration<T> SelectBestConsideration(T context)
        {
            var defaultScore = DefaultConsideration.GetScore(context);
            for (var i = 0; i < Considerations.Count; i++)
                if (Considerations[i].GetScore(context) >= defaultScore)
                    return Considerations[i];

            return DefaultConsideration;
        }
    }
}