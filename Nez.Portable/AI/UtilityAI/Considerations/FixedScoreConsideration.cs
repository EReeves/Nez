namespace Nez.AI.UtilityAI
{
	/// <summary>
	///     always returns a fixed score. Serves double duty as a default Consideration.
	/// </summary>
	public class FixedScoreConsideration<T> : IConsideration<T>
    {
        public float Score;


        public FixedScoreConsideration(float score = 1)
        {
            this.Score = score;
        }

        public IAction<T> Action { get; set; }


        float IConsideration<T>.GetScore(T context)
        {
            return Score;
        }
    }
}