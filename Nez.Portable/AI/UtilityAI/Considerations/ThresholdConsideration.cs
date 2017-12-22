using System.Collections.Generic;

namespace Nez.AI.UtilityAI
{
	/// <summary>
	///     Scores by summing child Appraisals until a child scores below the threshold
	/// </summary>
	public class ThresholdConsideration<T> : IConsideration<T>
    {
        private readonly List<IAppraisal<T>> _appraisals = new List<IAppraisal<T>>();
        public float Threshold;


        public ThresholdConsideration(float threshold)
        {
            this.Threshold = threshold;
        }

        public IAction<T> Action { get; set; }


        float IConsideration<T>.GetScore(T context)
        {
            var sum = 0f;
            for (var i = 0; i < _appraisals.Count; i++)
            {
                var score = _appraisals[i].GetScore(context);
                if (score < Threshold)
                    return sum;
                sum += score;
            }

            return sum;
        }
    }
}