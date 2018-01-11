﻿using System.Collections.Generic;
using Nez.AI.UtilityAI.Actions;
using Nez.AI.UtilityAI.Considerations.Appraisals;

namespace Nez.AI.UtilityAI.Considerations
{
	/// <summary>
	///     Scores by summing the score of all child Appraisals
	/// </summary>
	public class SumOfChildrenConsideration<T> : IConsideration<T>
    {
        private readonly List<IAppraisal<T>> _appraisals = new List<IAppraisal<T>>();
        public IAction<T> Action { get; set; }


        float IConsideration<T>.GetScore(T context)
        {
            var score = 0f;
            for (var i = 0; i < _appraisals.Count; i++)
                score += _appraisals[i].GetScore(context);

            return score;
        }
    }
}