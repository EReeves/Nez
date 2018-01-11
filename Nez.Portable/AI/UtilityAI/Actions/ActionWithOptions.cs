using System.Collections.Generic;

namespace Nez.AI.UtilityAI.Actions
{
	/// <summary>
	///     Action that encompasses a List of options. The options are passed to Appraisals which score and locate the best
	///     option.
	/// </summary>
	public abstract class ActionWithOptions<T, TU> : IAction<T>
    {
        protected List<IActionOptionAppraisal<T, TU>> Appraisals = new List<IActionOptionAppraisal<T, TU>>();

        public abstract void Execute(T context);


        public TU GetBestOption(T context, List<TU> options)
        {
            var result = default(TU);
            var bestScore = float.MinValue;

            for (var i = 0; i < options.Count; i++)
            {
                var option = options[i];
                var current = 0f;
                for (var j = 0; j < Appraisals.Count; j++)
                    current += Appraisals[j].GetScore(context, option);

                if (current > bestScore)
                {
                    bestScore = current;
                    result = option;
                }
            }

            return result;
        }


        public ActionWithOptions<T, TU> AddScorer(IActionOptionAppraisal<T, TU> scorer)
        {
            Appraisals.Add(scorer);
            return this;
        }
    }
}