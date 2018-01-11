using System.Collections.Generic;
using Nez.ECS.InternalUtils;

namespace Nez.ECS.Systems
{
    public class EntitySystem
    {
        protected List<Entity> Entities = new List<Entity>();

        protected Matcher matcher;
        protected Scene scene;


        public EntitySystem()
        {
            matcher = Matcher.Empty();
        }


        public EntitySystem(Matcher matcher)
        {
            this.matcher = matcher;
        }

        public Matcher Matcher => matcher;

        public Scene Scene
        {
            get => scene;
            set
            {
                scene = value;
                Entities = new List<Entity>();
            }
        }


        public virtual void OnChange(Entity entity)
        {
            var contains = Entities.Contains(entity);
            var interest = Matcher.IsInterested(entity);

            if (interest && !contains)
                Add(entity);
            else if (!interest && contains)
                Remove(entity);
        }


        public virtual void Add(Entity entity)
        {
            Entities.Add(entity);
            OnAdded(entity);
        }


        public virtual void Remove(Entity entity)
        {
            Entities.Remove(entity);
            OnRemoved(entity);
        }


        public virtual void OnAdded(Entity entity)
        {
        }


        public virtual void OnRemoved(Entity entity)
        {
        }


        protected virtual void Process(List<Entity> entities)
        {
        }


        protected virtual void LateProcess(List<Entity> entities)
        {
        }


        protected virtual void Begin()
        {
        }


        public void Update()
        {
            Begin();
            Process(Entities);
        }


        public void LateUpdate()
        {
            LateProcess(Entities);
            End();
        }


        protected virtual void End()
        {
        }
    }
}