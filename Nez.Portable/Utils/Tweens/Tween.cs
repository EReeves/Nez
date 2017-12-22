using System;
using System.Collections;

namespace Nez.Tweens
{
    public enum LoopType
    {
        None,
        RestartFromBeginning,
        PingPong
    }


    public abstract class Tween<T> : ITweenable, ITween<T> where T : struct
    {
        protected Action<ITween<T>> CompletionHandler;
        protected float Delay;
        protected float DelayBetweenLoops;
        protected float Duration;
        protected EaseType EaseType;
        protected float ElapsedTime;
        protected T FromValue;
        protected bool IsFromValueOverridden;
        protected bool IsRelative;
        private bool _isRunningInReverse;
        private bool _isTimeScaleIndependent;
        protected Action<ITween<T>> LoopCompleteHandler;
        protected int Loops;

        // loop state
        protected LoopType LoopType;

        protected ITweenable NextTween;
        protected bool ShouldRecycleTween = true;


        protected ITweenTarget<T> Target;
        protected float TimeScale = 1f;
        protected T ToValue;

        // tween state
        protected TweenState State = TweenState.Complete;


        private void ResetState()
        {
            Context = null;
            CompletionHandler = LoopCompleteHandler = null;
            IsFromValueOverridden = false;
            _isTimeScaleIndependent = false;
            State = TweenState.Complete;
            // TODO: I don't think we should ever flip the flag from _shouldRecycleTween = false without the user's consent. Needs research and some thought
            //_shouldRecycleTween = true;
            IsRelative = false;
            EaseType = TweenManager.DefaultEaseType;

            if (NextTween != null)
            {
                NextTween.RecycleSelf();
                NextTween = null;
            }

            Delay = 0f;
            Duration = 0f;
            TimeScale = 1f;
            ElapsedTime = 0f;
            LoopType = LoopType.None;
            DelayBetweenLoops = 0f;
            Loops = 0;
            _isRunningInReverse = false;
        }


	    /// <summary>
	    ///     resets all state to defaults and sets the initial state based on the paramaters passed in. This method serves
	    ///     as an entry point so that Tween subclasses can call it and so that tweens can be recycled. When recycled,
	    ///     the constructor will not be called again so this method encapsulates what the constructor would be doing.
	    /// </summary>
	    /// <param name="target">Target.</param>
	    /// <param name="to">To.</param>
	    /// <param name="duration">Duration.</param>
	    public void Initialize(ITweenTarget<T> target, T to, float duration)
        {
            // reset state in case we were recycled
            ResetState();

            Target = target;
            ToValue = to;
            Duration = duration;
        }


	    /// <summary>
	    ///     handles loop logic
	    /// </summary>
	    private void HandleLooping(float elapsedTimeExcess)
        {
            Loops--;
            if (LoopType == LoopType.PingPong)
                ReverseTween();

            if (LoopType == LoopType.RestartFromBeginning || Loops % 2 == 0)
                if (LoopCompleteHandler != null)
                    LoopCompleteHandler(this);

            // if we have loops left to process reset our state back to Running so we can continue processing them
            if (Loops > 0)
            {
                State = TweenState.Running;

                // now we need to set our elapsed time and factor in our elapsedTimeExcess
                if (LoopType == LoopType.RestartFromBeginning)
                {
                    ElapsedTime = elapsedTimeExcess - DelayBetweenLoops;
                }
                else
                {
                    if (_isRunningInReverse)
                        ElapsedTime += DelayBetweenLoops - elapsedTimeExcess;
                    else
                        ElapsedTime = elapsedTimeExcess - DelayBetweenLoops;
                }

                // if we had an elapsedTimeExcess and no delayBetweenLoops update the value
                if (DelayBetweenLoops == 0f && elapsedTimeExcess > 0f)
                    UpdateValue();
            }
        }


        protected abstract void UpdateValue();

        protected enum TweenState
        {
            Running,
            Paused,
            Complete
        }


        #region ITweenT implementation

        public object Context { get; protected set; }


        public ITween<T> SetEaseType(EaseType easeType)
        {
            EaseType = easeType;
            return this;
        }


        public ITween<T> SetDelay(float delay)
        {
            Delay = delay;
            ElapsedTime = -Delay;
            return this;
        }


        public ITween<T> SetDuration(float duration)
        {
            Duration = duration;
            return this;
        }


        public ITween<T> SetTimeScale(float timeScale)
        {
            TimeScale = timeScale;
            return this;
        }


        public ITween<T> SetIsTimeScaleIndependent()
        {
            _isTimeScaleIndependent = true;
            return this;
        }


        public ITween<T> setCompletionHandler(Action<ITween<T>> completionHandler)
        {
            CompletionHandler = completionHandler;
            return this;
        }


        public ITween<T> SetLoops(LoopType loopType, int loops = 1, float delayBetweenLoops = 0f)
        {
            LoopType = loopType;
            DelayBetweenLoops = delayBetweenLoops;

            // double the loop count for ping-pong
            if (loopType == LoopType.PingPong)
                loops = loops * 2;
            Loops = loops;

            return this;
        }


        public ITween<T> setLoopCompletionHandler(Action<ITween<T>> loopCompleteHandler)
        {
            LoopCompleteHandler = loopCompleteHandler;
            return this;
        }


        public ITween<T> SetFrom(T from)
        {
            IsFromValueOverridden = true;
            FromValue = from;
            return this;
        }


        public ITween<T> PrepareForReuse(T from, T to, float duration)
        {
            Initialize(Target, to, duration);
            return this;
        }


        public ITween<T> SetRecycleTween(bool shouldRecycleTween)
        {
            ShouldRecycleTween = shouldRecycleTween;
            return this;
        }


        public abstract ITween<T> SetIsRelative();


        public ITween<T> SetContext(object context)
        {
            this.Context = context;
            return this;
        }


        public ITween<T> SetNextTween(ITweenable nextTween)
        {
            NextTween = nextTween;
            return this;
        }

        #endregion


        #region ITweenable

        public bool Tick()
        {
            if (State == TweenState.Paused)
                return false;

            // when we loop we clamp values between 0 and duration. this will hold the excess that we clamped off so it can be reapplied
            var elapsedTimeExcess = 0f;
            if (!_isRunningInReverse && ElapsedTime >= Duration)
            {
                elapsedTimeExcess = ElapsedTime - Duration;
                ElapsedTime = Duration;
                State = TweenState.Complete;
            }
            else if (_isRunningInReverse && ElapsedTime <= 0)
            {
                elapsedTimeExcess = 0 - ElapsedTime;
                ElapsedTime = 0f;
                State = TweenState.Complete;
            }

            // elapsed time will be negative while we are delaying the start of the tween so dont update the value
            if (ElapsedTime >= 0 && ElapsedTime <= Duration)
                UpdateValue();

            // if we have a loopType and we are Complete (meaning we reached 0 or duration) handle the loop.
            // handleLooping will take any excess elapsedTime and factor it in and call udpateValue if necessary to keep
            // the tween perfectly accurate.
            if (LoopType != LoopType.None && State == TweenState.Complete && Loops > 0)
                HandleLooping(elapsedTimeExcess);

            var deltaTime = _isTimeScaleIndependent ? Time.UnscaledDeltaTime : Time.DeltaTime;
            deltaTime *= TimeScale;

            // running in reverse? then we need to subtract deltaTime
            if (_isRunningInReverse)
                ElapsedTime -= deltaTime;
            else
                ElapsedTime += deltaTime;

            if (State == TweenState.Complete)
            {
                if (CompletionHandler != null)
                    CompletionHandler(this);

                // if we have a nextTween add it to TweenManager so that it can start running
                if (NextTween != null)
                {
                    NextTween.Start();
                    NextTween = null;
                }

                return true;
            }

            return false;
        }


        public virtual void RecycleSelf()
        {
            if (ShouldRecycleTween)
            {
                Target = null;
                NextTween = null;
            }
        }


        public bool IsRunning()
        {
            return State == TweenState.Running;
        }


        public virtual void Start()
        {
            if (!IsFromValueOverridden)
                FromValue = Target.GetTweenedValue();

            if (State == TweenState.Complete)
            {
                State = TweenState.Running;
                TweenManager.AddTween(this);
            }
        }


        public void Pause()
        {
            State = TweenState.Paused;
        }


        public void Resume()
        {
            State = TweenState.Running;
        }


        public void Stop(bool bringToCompletion = false)
        {
            State = TweenState.Complete;

            if (bringToCompletion)
            {
                // if we are running in reverse we finish up at 0 else we go to duration
                ElapsedTime = _isRunningInReverse ? 0f : Duration;
                LoopType = LoopType.None;
                Loops = 0;

                // TweenManager will handle removal on the next tick
            }
            else
            {
                TweenManager.RemoveTween(this);
            }
        }

        #endregion


        #region ITweenControl

        public void JumpToElapsedTime(float elapsedTime)
        {
            ElapsedTime = Mathf.Clamp(elapsedTime, 0f, Duration);
            UpdateValue();
        }


	    /// <summary>
	    ///     reverses the current tween. if it was going forward it will be going backwards and vice versa.
	    /// </summary>
	    public void ReverseTween()
        {
            _isRunningInReverse = !_isRunningInReverse;
        }


	    /// <summary>
	    ///     when called via StartCoroutine this will continue until the tween completes
	    /// </summary>
	    /// <returns>The for completion.</returns>
	    public IEnumerator WaitForCompletion()
        {
            while (State != TweenState.Complete)
                yield return null;
        }


        public object GetTargetObject()
        {
            return Target.GetTargetObject();
        }

        #endregion
    }
}