using Nez.Maths;
using Nez.UI.Containers;

namespace Nez.UI.Widgets
{
	/// <summary>
	///     Displays a dialog, which is a modal window containing a content table with a button table underneath it. Methods
	///     are provided
	///     to add a label to the content table and buttons to the button table, but any widgets can be added. When a button is
	///     clicked,
	///     {@link #result(Object)} is called and the dialog is removed from the stage.
	/// </summary>
	public class Dialog : Window
    {
        private Table _contentTable, _buttonTable;


        public Dialog(string title, WindowStyle windowStyle) : base(title, windowStyle)
        {
            Initialize();
        }


        public Dialog(string title, Skin skin, string styleName = null) : this(title, skin.Get<WindowStyle>(styleName))
        {
        }


        private void Initialize()
        {
            Defaults().Space(16);
            Add(_contentTable = new Table()).Expand().Fill();
            Row();
            Add(_buttonTable = new Table());

            _contentTable.Defaults().Space(16);
            _buttonTable.Defaults().Space(16);
        }


        public Table GetContentTable()
        {
            return _contentTable;
        }


        public Table GetButtonTable()
        {
            return _buttonTable;
        }


	    /// <summary>
	    ///     Adds a label to the content table
	    /// </summary>
	    /// <returns>The text.</returns>
	    /// <param name="text">Text.</param>
	    public Dialog AddText(string text)
        {
            return AddText(new Label(text));
        }


	    /// <summary>
	    ///     Adds the given Label to the content table
	    /// </summary>
	    /// <param name="label">Label.</param>
	    public Dialog AddText(Label label)
        {
            _contentTable.Add(label);
            return this;
        }


        /** Adds a text button to the button table.
     * @param object The object that will be passed to {@link #result(Object)} if this button is clicked. May be null. */
        public Button AddButton(string text, TextButtonStyle buttonStyle)
        {
            return AddButton(new TextButton(text, buttonStyle));
        }


        /** Adds the given button to the button table.
     * @param object The object that will be passed to {@link #result(Object)} if this button is clicked. May be null. */
        public Button AddButton(Button button)
        {
            _buttonTable.Add(button);
            return button;
        }


	    /// <summary>
	    ///     {@link #pack() Packs} the dialog and adds it to the stage
	    /// </summary>
	    /// <param name="stage">Stage.</param>
	    public Dialog Show(Stage stage)
        {
            stage.AddElement(this);
            SetPosition(Mathf.Round((stage.GetWidth() - GetWidth()) / 2),
                Mathf.Round((stage.GetHeight() - GetHeight()) / 2));

            Pack();

            return this;
        }


	    /// <summary>
	    ///     Hides the dialog
	    /// </summary>
	    public void Hide()
        {
            Remove();
        }
    }
}