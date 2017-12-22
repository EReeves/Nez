using System;
using System.Collections.Generic;

namespace Nez.AI.FSM
{
    public class StateMachine<T>
    {
        protected T Context;

        protected State<T> currentState;
        private readonly Dictionary<Type, State<T>> _states = new Dictionary<Type, State<T>>();
        public float ElapsedTimeInState;
        public State<T> PreviousState;


        public StateMachine(T context, State<T> initialState)
        {
            Context = context;

            // setup our initial state
            AddState(initialState);
            currentState = initialState;
            CurrentState.Begin();
        }

        public State<T> CurrentState => currentState;
        public event Action OnStateChanged;


	    /// <summary>
	    ///     adds the state to the machine
	    /// </summary>
	    public void AddState(State<T> state)
        {
            state.SetMachineAndContext(this, Context);
            _states[state.GetType()] = state;
        }


	    /// <summary>
	    ///     ticks the state machine with the provided delta time
	    /// </summary>
	    public virtual void Update(float deltaTime)
        {
            ElapsedTimeInState += deltaTime;
            CurrentState.Reason();
            CurrentState.Update(deltaTime);
        }


	    /// <summary>
	    ///     changes the current state
	    /// </summary>
	    public TR ChangeState<TR>() where TR : State<T>
        {
            // avoid changing to the same state
            var newType = typeof(TR);
            if (CurrentState.GetType() == newType)
                return CurrentState as TR;

            // only call end if we have a currentState
            if (CurrentState != null)
                CurrentState.End();

            Assert.IsTrue(_states.ContainsKey(newType),
                "{0}: state {1} does not exist. Did you forget to add it by calling addState?", GetType(), newType);

            // swap states and call begin
            ElapsedTimeInState = 0f;
            PreviousState = currentState;
            currentState = _states[newType];
            CurrentState.Begin();

            // fire the changed event if we have a listener
            if (OnStateChanged != null)
                OnStateChanged();

            return CurrentState as TR;
        }
    }
}