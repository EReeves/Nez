using Microsoft.Xna.Framework;
using Nez.UI;
using Nez.UI.Drawable;
using Nez.UI.Widgets;

namespace Nez.ECS.Components.Renderables
{
	/// <summary>
	///     simple component that houses a Stage and delegates update/render/debugRender calls
	/// </summary>
	public class UiCanvas : RenderableComponent, IUpdatable
    {
        public Stage Stage;


        public UiCanvas()
        {
            Stage = new Stage();
        }

        public override float Width => Stage.GetWidth();

        public override float Height => Stage.GetHeight();

	    /// <summary>
	    ///     if true, the rawMousePosition will be used else the scaledMousePosition will be used. If your UI is in screen space
	    ///     (using a
	    ///     ScreenSpaceRenderer for example) then set this to true so input is not scaled.
	    /// </summary>
	    public bool IsFullScreen
        {
            get => Stage.IsFullScreen;
            set => Stage.IsFullScreen = value;
        }


        void IUpdatable.Update()
        {
            Stage.Update();
        }


        public override void OnAddedToEntity()
        {
            Stage.Entity = Entity;

            foreach (var child in Stage.GetRoot().Children)
                if (child is Window)
                    (child as Window).KeepWithinStage();
        }


        public override void OnRemovedFromEntity()
        {
            Stage.Entity = null;
            Stage.Dispose();
        }


        public override void Render(Graphics.Graphics graphics, Camera camera)
        {
            Stage.Render(graphics, camera);
        }


        public override void DebugRender(Graphics.Graphics graphics)
        {
            Stage.GetRoot().DebugRender(graphics);
        }


	    /// <summary>
	    ///     displays a simple dialog with a button to close it
	    /// </summary>
	    /// <returns>The dialog.</returns>
	    /// <param name="title">Title.</param>
	    /// <param name="messageText">Message text.</param>
	    /// <param name="closeButtonText">Close button text.</param>
	    public Dialog ShowDialog(string title, string messageText, string closeButtonText)
        {
            var skin = Skin.CreateDefaultSkin();

            var style = new WindowStyle
            {
                Background = new PrimitiveDrawable(new Color(50, 50, 50)),
                StageBackground = new PrimitiveDrawable(new Color(0, 0, 0, 150))
            };

            var dialog = new Dialog(title, style);
            dialog.GetTitleLabel().GetStyle().Background = new PrimitiveDrawable(new Color(55, 100, 100));
            dialog.Pad(20, 5, 5, 5);
            dialog.AddText(messageText);
            dialog.AddButton(new TextButton(closeButtonText, skin)).OnClicked += butt => dialog.Hide();
            dialog.Show(Stage);

            return dialog;
        }
    }
}