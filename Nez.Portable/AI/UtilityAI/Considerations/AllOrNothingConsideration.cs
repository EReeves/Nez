using System.Collections.Generic;
using Nez.AI.UtilityAI.Actions;
using Nez.AI.UtilityAI.Considerations.Appraisals;

namespace Nez.AI.UtilityAI.Considerations
{
	/// <summary>
	///     Only scores if all child Appraisals score above the threshold
	/// </summary>
	public class AllOrNothingConsideration<T> : IConsideration<T>
    {
        private readonly List<IAppraisal<T>> _appraisals = new List<IAppraisal<T>>();
        public float Threshold;


        public AllOrNothingConsideration(float threshold = 0)
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
                    return 0;
                sum += score;
            }

            return sum;
        }


        public AllOrNothingConsideration<T> AddAppraisal(IAppraisal<T> appraisal)
        {
            _appraisals.Add(appraisal);
            return this;
        }
    }
}