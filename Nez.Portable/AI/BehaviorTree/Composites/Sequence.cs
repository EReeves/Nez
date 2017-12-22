namespace Nez.AI.BehaviorTrees
{
	/// <summary>
	///     The sequence task is similar to an "and" operation. It will return failure as soon as one of its child tasks return
	///     failure. If a
	///     child task returns success then it will sequentially run the next task. If all child tasks return success then it
	///     will return success.
	/// </summary>
	public class Sequence<T> : Composite<T>
    {
        public Sequence(AbortTypes abortType = AbortTypes.None)
        {
            this.AbortType = abortType;
        }


        public override TaskStatus Update(T context)
        {
            // first, we handle conditional aborts if we are not already on the first child
            if (CurrentChildIndex != 0)
                HandleConditionalAborts(context);

            var current = Children[CurrentChildIndex];
            var status = current.Tick(context);

            // if the child failed or is still running, early return
            if (status != TaskStatus.Success)
                return status;

            CurrentChildIndex++;

            // if the end of the children is hit the whole sequence suceeded
            if (CurrentChildIndex == Children.Count)
            {
                // reset index for next run
                CurrentChildIndex = 0;
                return TaskStatus.Success;
            }

            return TaskStatus.Running;
        }


        private void HandleConditionalAborts(T context)
        {
            if (HasLowerPriorityConditionalAbort)
                UpdateLowerPriorityAbortConditional(context, TaskStatus.Success);

            if (AbortType.Has(AbortTypes.Self))
                UpdateSelfAbortConditional(context, TaskStatus.Success);
        }
    }
}