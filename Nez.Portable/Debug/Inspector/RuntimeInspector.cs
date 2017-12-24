using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Nez.Console;
using Nez.UI;

#if DEBUG
namespace Nez
{
    public class RuntimeInspector : IDisposable
    {
        private ScreenSpaceCamera _camera;
        private Entity _entity;
        private readonly List<InspectorList> _inspectors = new List<InspectorList>();
        private ScrollPane _scrollPane;

        // ui fields
        private Skin _skin;

        private Table _table;
        private UiCanvas _ui;


	    /// <summary>
	    ///     creates a PostProcessor inspector
	    /// </summary>
	    public RuntimeInspector()
        {
            Initialize();
        }


	    /// <summary>
	    ///     creates an Entity inspector
	    /// </summary>
	    /// <param name="entity">Entity.</param>
	    public RuntimeInspector(Entity entity)
        {
            _entity = entity;
            Initialize();
            CacheTransformInspector();
        }


        private void Initialize()
        {
            PrepCanvas();
            _camera = new ScreenSpaceCamera();
            Core.Emitter.AddObserver(CoreEvents.GraphicsDeviceReset, OnGraphicsDeviceReset);
            Core.Emitter.AddObserver(CoreEvents.SceneChanged, OnSceneChanged);
        }


        private void OnGraphicsDeviceReset()
        {
            _scrollPane.SetHeight(Screen.Height);
        }


        private void OnSceneChanged()
        {
            DebugConsole.Instance.RuntimeInspector = null;
            Dispose();
        }


        public void Update()
        {
            // if we have an Entity this is an Entity inspector else it is a PostProcessor inspector
            if (_entity != null)
            {
                // update transform, which has a null Component
                GetOrCreateInspectorList(null).Update();

                for (var i = 0; i < _entity.Components.Count; i++)
                    GetOrCreateInspectorList(_entity.Components[i]).Update();
            }
            else
            {
                for (var i = 0; i < Core.Scene.PostProcessors.Length; i++)
                    GetOrCreateInspectorList(Core.Scene.PostProcessors.Buffer[i]).Update();
            }
        }


        public void Render()
        {
            // manually start a fresh batch and call the UICanvas Component lifecycle methods since it isnt attached to the Scene
            Graphics.Instance.Batcher.Begin();
            (_ui as IUpdatable).Update();
            _ui.Render(Graphics.Instance, _camera);
            Graphics.Instance.Batcher.End();
        }


	    /// <summary>
	    ///     attempts to find a cached version of the InspectorList and if it cant find one it will create a new one
	    /// </summary>
	    /// <returns>The or create inspector list.</returns>
	    /// <param name="comp">Comp.</param>
	    private InspectorList GetOrCreateInspectorList(object comp)
        {
            var inspector = _inspectors.Where(i => i.Target == comp).FirstOrDefault();
            if (inspector == null)
            {
                inspector = new InspectorList(comp);
                inspector.Initialize(_table, _skin);
                inspector.Target = comp;
                _inspectors.Add(inspector);
            }

            return inspector;
        }


        private void CacheTransformInspector()
        {
            // add Transform separately
            var transformInspector = new InspectorList(_entity.Transform);
            transformInspector.Initialize(_table, _skin);
            _inspectors.Add(transformInspector);
        }


        private void PrepCanvas()
        {
            _skin = Skin.CreateDefaultSkin();

            // modify some of the default styles to better suit our needs
            var tfs = _skin.Get<TextFieldStyle>();
            tfs.Background.LeftWidth = tfs.Background.RightWidth = 4;
            tfs.Background.BottomHeight = 0;
            tfs.Background.TopHeight = 3;

            var checkbox = _skin.Get<CheckBoxStyle>();
            checkbox.CheckboxOn.MinWidth = checkbox.CheckboxOn.MinHeight = 15;
            checkbox.CheckboxOff.MinWidth = checkbox.CheckboxOff.MinHeight = 15;
            checkbox.CheckboxOver.MinWidth = checkbox.CheckboxOver.MinHeight = 15;

            // since we arent using this as a Component on an Entity we'll fake it here
            _ui = new UiCanvas();
            _ui.OnAddedToEntity();
            _ui.Stage.IsFullScreen = true;

            _table = new Table();
            _table.Top().Left();
            _table.Defaults().SetPadTop(4).SetPadLeft(4).SetPadRight(0).SetAlign(Align.Left);
            _table.SetBackground(new PrimitiveDrawable(new Color(40, 40, 40)));

            // wrap up the table in a ScrollPane
            _scrollPane = _ui.Stage.AddElement(new ScrollPane(_table, _skin));
            // force a validate which will layout the ScrollPane and populate the proper scrollBarWidth
            _scrollPane.Validate();
            _scrollPane.SetSize(295 + _scrollPane.GetScrollBarWidth(), Screen.Height);
        }


        #region IDisposable Support

        private bool _disposedValue;

        private void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                Core.Emitter.RemoveObserver(CoreEvents.GraphicsDeviceReset, OnGraphicsDeviceReset);
                Core.Emitter.RemoveObserver(CoreEvents.SceneChanged, OnSceneChanged);
                _entity = null;
                _disposedValue = true;
            }
        }


        public void Dispose()
        {
            Dispose(true);
        }

        #endregion
    }
}
#endif