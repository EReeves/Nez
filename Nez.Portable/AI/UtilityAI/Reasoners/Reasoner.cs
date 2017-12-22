using System.Collections.Generic;

namespace Nez.AI.UtilityAI
{
	/// <summary>
	///     the root of UtilityAI.
	/// </summary>
	public abstract class Reasoner<T>
    {
        protected List<IConsideration<T>> Considerations = new List<IConsideration<T>>();
        public IConsideration<T> DefaultConsideration = new FixedScoreConsideration<T>();


        public IAction<T> Select(T context)
        {
            var consideration = SelectBestConsideration(context);
            if (consideration != null)
                return consideration.Action;

            return null;
        }


        protected abstract IConsideration<T> SelectBestConsideration(T context);


        public Reasoner<T> AddConsideration(IConsideration<T> consideration)
        {
            Considerations.Add(consideration);
            return this;
        }


        public Reasoner<T> SetDefaultConsideration(IConsideration<T> defaultConsideration)
        {
            this.DefaultConsideration = defaultConsideration;
            return this;
        }
    }
}