namespace Nez.AI.UtilityAI
{
	/// <summary>
	///     The Consideration with the highest score is selected
	/// </summary>
	public class HighestScoreReasoner<T> : Reasoner<T>
    {
        protected override IConsideration<T> SelectBestConsideration(T context)
        {
            var highestScore = DefaultConsideration.GetScore(context);
            IConsideration<T> consideration = null;
            for (var i = 0; i < Considerations.Count; i++)
            {
                var score = Considerations[i].GetScore(context);
                if (score > highestScore)
                {
                    highestScore = score;
                    consideration = Considerations[i];
                }
            }

            if (consideration == null)
                return DefaultConsideration;

            return consideration;
        }
    }
}