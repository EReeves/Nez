using System.Collections.Generic;
using Nez.Debug;
using Nez.ECS.Components.Renderables;
using Nez.Utils.Collections;

namespace Nez.ECS.InternalUtils
{
    public class RenderableComponentList
    {
        // global updateOrder sort for the IUpdatable list
        private static readonly RenderableComparer CompareUpdatableOrder = new RenderableComparer();

	    /// <summary>
	    ///     list of components added to the entity
	    /// </summary>
	    private readonly FastList<IRenderable> _components = new FastList<IRenderable>();

	    /// <summary>
	    ///     tracks components by renderLayer for easy retrieval
	    /// </summary>
	    private readonly Dictionary<int, FastList<IRenderable>> _componentsByRenderLayer =
            new Dictionary<int, FastList<IRenderable>>();

        private bool _componentsNeedSort = true;
        private readonly List<int> _unsortedRenderLayers = new List<int>();


        public void Add(IRenderable component)
        {
            _components.Add(component);
            AddToRenderLayerList(component, component.RenderLayer);
        }


        public void Remove(IRenderable component)
        {
            _components.Remove(component);
            _componentsByRenderLayer[component.RenderLayer].Remove(component);
        }


        public void UpdateRenderableRenderLayer(IRenderable component, int oldRenderLayer, int newRenderLayer)
        {
            // a bit of care needs to be taken in case a renderLayer is changed before the component is "live". this can happen when a component
            // changes its renderLayer immediately after being created
            if (_componentsByRenderLayer.ContainsKey(oldRenderLayer) &&
                _componentsByRenderLayer[oldRenderLayer].Contains(component))
            {
                _componentsByRenderLayer[oldRenderLayer].Remove(component);
                AddToRenderLayerList(component, newRenderLayer);
            }
        }


        public void SetRenderLayerNeedsComponentSort(int renderLayer)
        {
            if (!_unsortedRenderLayers.Contains(renderLayer))
                _unsortedRenderLayers.Add(renderLayer);
            _componentsNeedSort = true;
        }


        internal void SetNeedsComponentSort()
        {
            _componentsNeedSort = true;
        }


        private void AddToRenderLayerList(IRenderable component, int renderLayer)
        {
            var list = ComponentsWithRenderLayer(renderLayer);
            Assert.IsFalse(list.Contains(component), "Component renderLayer list already contains this component");

            list.Add(component);
            if (!_unsortedRenderLayers.Contains(renderLayer))
                _unsortedRenderLayers.Add(renderLayer);
            _componentsNeedSort = true;
        }


        public FastList<IRenderable> ComponentsWithRenderLayer(int renderLayer)
        {
            FastList<IRenderable> list = null;
            if (!_componentsByRenderLayer.TryGetValue(renderLayer, out list))
            {
                list = new FastList<IRenderable>();
                _componentsByRenderLayer[renderLayer] = list;
            }

            return _componentsByRenderLayer[renderLayer];
        }


        public void UpdateLists()
        {
            if (_componentsNeedSort)
            {
                _components.Sort(CompareUpdatableOrder);
                _componentsNeedSort = false;
            }

            if (_unsortedRenderLayers.Count > 0)
            {
                for (int i = 0, count = _unsortedRenderLayers.Count; i < count; i++)
                {
                    FastList<IRenderable> renderLayerComponents;
                    if (_componentsByRenderLayer.TryGetValue(_unsortedRenderLayers[i], out renderLayerComponents))
                        renderLayerComponents.Sort(CompareUpdatableOrder);
                }

                _unsortedRenderLayers.Clear();
            }
        }


        #region array access

        public int Count => _components.Length;

        public IRenderable this[int index] => _components.Buffer[index];

        #endregion
    }
}