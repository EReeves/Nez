using System.Collections.Generic;
using Nez.ECS.Systems;

namespace Nez.ECS.InternalUtils
{
    public class EntityProcessorList
    {
        protected List<EntitySystem> Processors = new List<EntitySystem>();


        public void Add(EntitySystem processor)
        {
            Processors.Add(processor);
        }


        public void Remove(EntitySystem processor)
        {
            Processors.Remove(processor);
        }


        public virtual void OnComponentAdded(Entity entity)
        {
            NotifyEntityChanged(entity);
        }


        public virtual void OnComponentRemoved(Entity entity)
        {
            NotifyEntityChanged(entity);
        }


        public virtual void OnEntityAdded(Entity entity)
        {
            NotifyEntityChanged(entity);
        }


        public virtual void OnEntityRemoved(Entity entity)
        {
            RemoveFromProcessors(entity);
        }


        protected virtual void NotifyEntityChanged(Entity entity)
        {
            for (var i = 0; i < Processors.Count; i++)
                Processors[i].OnChange(entity);
        }


        protected virtual void RemoveFromProcessors(Entity entity)
        {
            for (var i = 0; i < Processors.Count; i++)
                Processors[i].Remove(entity);
        }


        public void Begin()
        {
        }


        public void Update()
        {
            for (var i = 0; i < Processors.Count; i++)
                Processors[i].Update();
        }


        public void LateUpdate()
        {
            for (var i = 0; i < Processors.Count; i++)
                Processors[i].LateUpdate();
        }


        public void End()
        {
        }


        public T GetProcessor<T>() where T : EntitySystem
        {
            for (var i = 0; i < Processors.Count; i++)
            {
                var processor = Processors[i];
                if (processor is T)
                    return processor as T;
            }

            return null;
        }
    }
}