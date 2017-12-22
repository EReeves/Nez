using System.Collections.Generic;

namespace Nez.AI.GOAP
{
	/// <summary>
	///     Agent provides a simple and concise way to use the planner. It is not necessary to use at all since it is just a
	///     convenince wrapper
	///     around the ActionPlanner making it easier to get plans and store the results.
	/// </summary>
	public abstract class Agent
    {
        protected ActionPlanner Planner;
        public Stack<Action> Actions;


        public Agent()
        {
            Planner = new ActionPlanner();
        }


        public bool Plan(bool debugPlan = false)
        {
            List<AStarNode> nodes = null;
            if (debugPlan)
                nodes = new List<AStarNode>();

            Actions = Planner.Plan(GetWorldState(), GetGoalState(), nodes);

            if (nodes != null && nodes.Count > 0)
            {
                Debug.Log("---- ActionPlanner plan ----");
                Debug.Log("plan cost = {0}\n", nodes[nodes.Count - 1].CostSoFar);
                Debug.Log("{0}\t{1}", "start".PadRight(15), GetWorldState().Describe(Planner));
                for (var i = 0; i < nodes.Count; i++)
                {
                    Debug.Log("{0}: {1}\t{2}", i, nodes[i].Action.GetType().Name.PadRight(15),
                        nodes[i].WorldState.Describe(Planner));
                    Pool<AStarNode>.Free(nodes[i]);
                }
            }

            return HasActionPlan();
        }


        public bool HasActionPlan()
        {
            return Actions != null && Actions.Count > 0;
        }


	    /// <summary>
	    ///     current WorldState
	    /// </summary>
	    /// <returns>The world state.</returns>
	    public abstract WorldState GetWorldState();


	    /// <summary>
	    ///     the goal state that the agent wants to achieve
	    /// </summary>
	    /// <returns>The goal state.</returns>
	    public abstract WorldState GetGoalState();
    }
}