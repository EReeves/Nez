﻿using System.Collections.Generic;

namespace Nez.AI.BehaviorTrees
{
	/// <summary>
	///     any Composite nodes must subclass this. Provides storage for children and helpers to deal with AbortTypes
	/// </summary>
	public abstract class Composite<T> : Behavior<T>
    {
        protected List<Behavior<T>> Children = new List<Behavior<T>>();
        protected int CurrentChildIndex;
        protected bool HasLowerPriorityConditionalAbort;
        public AbortTypes AbortType = AbortTypes.None;


        public override void Invalidate()
        {
            base.Invalidate();

            for (var i = 0; i < Children.Count; i++)
                Children[i].Invalidate();
        }


        public override void OnStart()
        {
            // LowerPriority aborts happen one level down so we check for any here
            HasLowerPriorityConditionalAbort = HasLowerPriorityConditionalAbortInChildren();
            CurrentChildIndex = 0;
        }


        public override void OnEnd()
        {
            // we are done so invalidate our children so they are ready for the next tick
            for (var i = 0; i < Children.Count; i++)
                Children[i].Invalidate();
        }


	    /// <summary>
	    ///     adds a child to this Composite
	    /// </summary>
	    /// <param name="child">Child.</param>
	    public void AddChild(Behavior<T> child)
        {
            Children.Add(child);
        }


	    /// <summary>
	    ///     returns true if the first child of a Composite is a Conditional. Usef for dealing with conditional aborts.
	    /// </summary>
	    /// <returns><c>true</c>, if first child conditional was ised, <c>false</c> otherwise.</returns>
	    public bool IsFirstChildConditional()
        {
            return Children[0] is IConditional<T>;
        }


	    /// <summary>
	    ///     checks the children of the Composite to see if any are a Composite with a LowerPriority AbortType
	    /// </summary>
	    /// <returns><c>true</c>, if lower priority conditional abort in children was hased, <c>false</c> otherwise.</returns>
	    private bool HasLowerPriorityConditionalAbortInChildren()
        {
            for (var i = 0; i < Children.Count; i++)
            {
                // check for a Composite with an abortType set
                var composite = Children[i] as Composite<T>;
                if (composite != null && composite.AbortType.Has(AbortTypes.LowerPriority))
                    if (composite.IsFirstChildConditional())
                        return true;
            }

            return false;
        }


	    /// <summary>
	    ///     checks any child Composites that have a LowerPriority AbortType and a Conditional as the first child. If it finds
	    ///     one it will tick
	    ///     the Conditional and if the status is not equal to statusCheck the _currentChildIndex will be updated, ie the
	    ///     currently running
	    ///     Action will be aborted.
	    /// </summary>
	    /// <param name="context">Context.</param>
	    /// <param name="statusCheck">Status check.</param>
	    protected void UpdateLowerPriorityAbortConditional(T context, TaskStatus statusCheck)
        {
            // check any lower priority tasks to see if they changed status
            for (var i = 0; i < CurrentChildIndex; i++)
            {
                var composite = Children[i] as Composite<T>;
                if (composite != null && composite.AbortType.Has(AbortTypes.LowerPriority))
                {
                    // now we get the status of only the Conditional (update instead of tick) to see if it changed taking care with ConditionalDecorators
                    var child = composite.Children[0];
                    var status = UpdateConditionalNode(context, child);
                    if (status != statusCheck)
                    {
                        CurrentChildIndex = i;

                        // we have an abort so we invalidate the children so they get reevaluated
                        for (var j = i; j < Children.Count; j++)
                            Children[j].Invalidate();
                        break;
                    }
                }
            }
        }


	    /// <summary>
	    ///     checks any IConditional children to see if they have changed state
	    /// </summary>
	    /// <param name="context">Context.</param>
	    /// <param name="statusCheck">Status check.</param>
	    protected void UpdateSelfAbortConditional(T context, TaskStatus statusCheck)
        {
            // check any IConditional child tasks to see if they changed status
            for (var i = 0; i < CurrentChildIndex; i++)
            {
                var child = Children[i];
                if (!(child is IConditional<T>))
                    continue;

                var status = UpdateConditionalNode(context, child);
                if (status != statusCheck)
                {
                    CurrentChildIndex = i;

                    // we have an abort so we invalidate the children so they get reevaluated
                    for (var j = i; j < Children.Count; j++)
                        Children[j].Invalidate();
                    break;
                }
            }
        }


	    /// <summary>
	    ///     helper that gets the TaskStatus of either a Conditional or a ConditionalDecorator
	    /// </summary>
	    /// <returns>The conditional node.</returns>
	    /// <param name="context">Context.</param>
	    /// <param name="node">Node.</param>
	    private TaskStatus UpdateConditionalNode(T context, Behavior<T> node)
        {
            if (node is ConditionalDecorator<T>)
                return (node as ConditionalDecorator<T>).ExecuteConditional(context, true);
            return node.Update(context);
        }
    }
}