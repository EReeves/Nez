﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Nez.Debug.Inspector;
using Nez.Graphics.Batcher;
using Nez.Input;
using Nez.Utils.Extensions;
using Nez.Utils.Fonts;

namespace Nez.Utils.DebugConsole
{
    public partial class DebugConsole
    {
        private const float UnderscoreTime = 0.5f;
        private const float RepeatDelay = 0.5f;
        private const float RepeatEvery = 1 / 30f;
        private const float Opacity = 0.65f;

        // render constants
        private const int LineHeight = 10;

        private const int TextPaddingX = 5;
        private const int TextPaddingY = 4;

	    /// <summary>
	    ///     separation of the command entry and history boxes
	    /// </summary>
	    private const int CommandHistoryPadding = 10;

	    /// <summary>
	    ///     global padding on the left/right of the console
	    /// </summary>
	    private const int HorizontalPadding = 10;

        public static DebugConsole Instance;

	    /// <summary>
	    ///     controls the scale of the console
	    /// </summary>
	    public static float RenderScale = 1f;

        private bool _canOpen;
        private readonly List<string> _commandHistory;
        private readonly Dictionary<string, CommandInfo> _commands;
        private KeyboardState _currentState;
        private string _currentText = "";
        private readonly List<string> _drawCommands;

	    /// <summary>
	    ///     bind any custom Actions you would like to function keys
	    /// </summary>
	    private readonly Action[] _functionKeyActions;

        private KeyboardState _oldState;
        private float _repeatCounter;
        private Keys? _repeatKey;
#if DEBUG
        internal RuntimeInspector RuntimeInspector;
#endif
        private int _seekIndex = -1;
        private readonly List<string> _sorted;
        private int _tabIndex = -1;
        private string _tabSearch;
        private bool _underscore;
        private float _underscoreCounter;

        private readonly bool _enabled = true;
        public bool IsOpen;


        static DebugConsole()
        {
            Instance = new DebugConsole();
        }


        public DebugConsole()
        {
            _commandHistory = new List<string>();
            _drawCommands = new List<string>();
            _commands = new Dictionary<string, CommandInfo>();
            _sorted = new List<string>();
            _functionKeyActions = new Action[12];

            BuildCommandsList();
        }


        public void Log(Exception e)
        {
            Log(e.Message);

            var str = e.StackTrace;
            var parts = str.Split(new[] {"\n"}, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in parts)
            {
                var lineWithoutPath = Regex.Replace(line, @"in\s\/.*?\/.*?(\w+\.cs)", "$1");
                Log(lineWithoutPath);
            }
        }


        public void Log(string format, params object[] args)
        {
            Log(string.Format(format, args));
        }


        public void Log(object obj)
        {
            Log(obj.ToString());
        }


        public void Log(string str)
        {
            // split up multi-line logs and log each line seperately
            var parts = str.Split(new[] {"\n"}, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 1)
            {
                foreach (var line in parts)
                    Log(line);
                return;
            }

            // Split the string if you overlow horizontally
            var maxWidth = Core.GraphicsDevice.PresentationParameters.BackBufferWidth - 40;
            var screenHeight = Core.GraphicsDevice.PresentationParameters.BackBufferHeight;

            while (Graphics.Graphics.Instance.BitmapFont.MeasureString(str).X * RenderScale > maxWidth)
            {
                var split = -1;
                for (var i = 0; i < str.Length; i++)
                    if (str[i] == ' ')
                        if (Graphics.Graphics.Instance.BitmapFont.MeasureString(str.Substring(0, i)).X * RenderScale <= maxWidth)
                            split = i;
                        else
                            break;

                if (split == -1)
                    break;

                _drawCommands.Insert(0, str.Substring(0, split));
                str = str.Substring(split + 1);
            }

            _drawCommands.Insert(0, str);

            // Don't overflow top of window
            var maxCommands = (screenHeight - 100) / 30;
            while (_drawCommands.Count > maxCommands)
                _drawCommands.RemoveAt(_drawCommands.Count - 1);
        }


        #region Updating and Rendering

        internal void Update()
        {
            if (IsOpen)
                UpdateOpen();
            else if (_enabled)
                UpdateClosed();
        }


        private void UpdateClosed()
        {
            if (!_canOpen)
            {
                _canOpen = true;
            }
            else if (Input.Input.IsKeyPressed(Keys.OemTilde, Keys.Oem8))
            {
                IsOpen = true;
                _currentState = Keyboard.GetState();
            }

            for (var i = 0; i < _functionKeyActions.Length; i++)
                if (Input.Input.IsKeyPressed(Keys.F1 + i))
                    ExecuteFunctionKeyAction(i);
        }


        private void UpdateOpen()
        {
            _oldState = _currentState;
            _currentState = Keyboard.GetState();

            _underscoreCounter += Time.DeltaTime;
            while (_underscoreCounter >= UnderscoreTime)
            {
                _underscoreCounter -= UnderscoreTime;
                _underscore = !_underscore;
            }

            if (_repeatKey.HasValue)
                if (_currentState[_repeatKey.Value] == KeyState.Down)
                {
                    _repeatCounter += Time.DeltaTime;

                    while (_repeatCounter >= RepeatDelay)
                    {
                        HandleKey(_repeatKey.Value);
                        _repeatCounter -= RepeatEvery;
                    }
                }
                else
                {
                    _repeatKey = null;
                }

            foreach (var key in _currentState.GetPressedKeys())
                if (_oldState[key] == KeyState.Up)
                {
                    HandleKey(key);
                    break;
                }
        }


        private void HandleKey(Keys key)
        {
            if (key != Keys.Tab && key != Keys.LeftShift && key != Keys.RightShift && key != Keys.RightAlt &&
                key != Keys.LeftAlt && key != Keys.RightControl && key != Keys.LeftControl)
                _tabIndex = -1;

            if (key != Keys.OemTilde && key != Keys.Oem8 && key != Keys.Enter && _repeatKey != key)
            {
                _repeatKey = key;
                _repeatCounter = 0;
            }

            switch (key)
            {
                default:
                    if (key.ToString().Length == 1)
                        if (InputUtils.IsShiftDown())
                            _currentText += key.ToString();
                        else
                            _currentText += key.ToString().ToLower();
                    break;

                case Keys.D1:
                    if (InputUtils.IsShiftDown())
                        _currentText += '!';
                    else
                        _currentText += '1';
                    break;
                case Keys.D2:
                    if (InputUtils.IsShiftDown())
                        _currentText += '@';
                    else
                        _currentText += '2';
                    break;
                case Keys.D3:
                    if (InputUtils.IsShiftDown())
                        _currentText += '#';
                    else
                        _currentText += '3';
                    break;
                case Keys.D4:
                    if (InputUtils.IsShiftDown())
                        _currentText += '$';
                    else
                        _currentText += '4';
                    break;
                case Keys.D5:
                    if (InputUtils.IsShiftDown())
                        _currentText += '%';
                    else
                        _currentText += '5';
                    break;
                case Keys.D6:
                    if (InputUtils.IsShiftDown())
                        _currentText += '^';
                    else
                        _currentText += '6';
                    break;
                case Keys.D7:
                    if (InputUtils.IsShiftDown())
                        _currentText += '&';
                    else
                        _currentText += '7';
                    break;
                case Keys.D8:
                    if (InputUtils.IsShiftDown())
                        _currentText += '*';
                    else
                        _currentText += '8';
                    break;
                case Keys.D9:
                    if (InputUtils.IsShiftDown())
                        _currentText += '(';
                    else
                        _currentText += '9';
                    break;
                case Keys.D0:
                    if (InputUtils.IsShiftDown())
                        _currentText += ')';
                    else
                        _currentText += '0';
                    break;
                case Keys.OemComma:
                    if (InputUtils.IsShiftDown())
                        _currentText += '<';
                    else
                        _currentText += ',';
                    break;
                case Keys.OemPeriod:
                    if (InputUtils.IsShiftDown())
                        _currentText += '>';
                    else
                        _currentText += '.';
                    break;
                case Keys.OemQuestion:
                    if (InputUtils.IsShiftDown())
                        _currentText += '?';
                    else
                        _currentText += '/';
                    break;
                case Keys.OemSemicolon:
                    if (InputUtils.IsShiftDown())
                        _currentText += ':';
                    else
                        _currentText += ';';
                    break;
                case Keys.OemQuotes:
                    if (InputUtils.IsShiftDown())
                        _currentText += '"';
                    else
                        _currentText += '\'';
                    break;
                case Keys.OemBackslash:
                    if (InputUtils.IsShiftDown())
                        _currentText += '|';
                    else
                        _currentText += '\\';
                    break;
                case Keys.OemOpenBrackets:
                    if (InputUtils.IsShiftDown())
                        _currentText += '{';
                    else
                        _currentText += '[';
                    break;
                case Keys.OemCloseBrackets:
                    if (InputUtils.IsShiftDown())
                        _currentText += '}';
                    else
                        _currentText += ']';
                    break;
                case Keys.OemMinus:
                    if (InputUtils.IsShiftDown())
                        _currentText += '_';
                    else
                        _currentText += '-';
                    break;
                case Keys.OemPlus:
                    if (InputUtils.IsShiftDown())
                        _currentText += '+';
                    else
                        _currentText += '=';
                    break;

                case Keys.Space:
                    _currentText += " ";
                    break;
                case Keys.Back:
                    if (_currentText.Length > 0)
                        _currentText = _currentText.Substring(0, _currentText.Length - 1);
                    break;
                case Keys.Delete:
                    _currentText = "";
                    break;

                case Keys.Up:
                    if (_seekIndex < _commandHistory.Count - 1)
                    {
                        _seekIndex++;
                        _currentText = string.Join(" ", _commandHistory[_seekIndex]);
                    }
                    break;
                case Keys.Down:
                    if (_seekIndex > -1)
                    {
                        _seekIndex--;
                        if (_seekIndex == -1)
                            _currentText = "";
                        else
                            _currentText = string.Join(" ", _commandHistory[_seekIndex]);
                    }
                    break;

                case Keys.Tab:
                    if (InputUtils.IsShiftDown())
                    {
                        if (_tabIndex == -1)
                        {
                            _tabSearch = _currentText;
                            FindLastTab();
                        }
                        else
                        {
                            _tabIndex--;
                            if (_tabIndex < 0 || _tabSearch != "" && _sorted[_tabIndex].IndexOf(_tabSearch) != 0)
                                FindLastTab();
                        }
                    }
                    else
                    {
                        if (_tabIndex == -1)
                        {
                            _tabSearch = _currentText;
                            FindFirstTab();
                        }
                        else
                        {
                            _tabIndex++;
                            if (_tabIndex >= _sorted.Count ||
                                _tabSearch != "" && _sorted[_tabIndex].IndexOf(_tabSearch) != 0)
                                FindFirstTab();
                        }
                    }
                    if (_tabIndex != -1)
                        _currentText = _sorted[_tabIndex];
                    break;

                case Keys.F1:
                case Keys.F2:
                case Keys.F3:
                case Keys.F4:
                case Keys.F5:
                case Keys.F6:
                case Keys.F7:
                case Keys.F8:
                case Keys.F9:
                case Keys.F10:
                case Keys.F11:
                case Keys.F12:
                    ExecuteFunctionKeyAction(key - Keys.F1);
                    break;

                case Keys.Enter:
                    if (_currentText.Length > 0)
                        EnterCommand();
                    break;

                case Keys.Oem8:
                case Keys.OemTilde:
                    IsOpen = _canOpen = false;
                    break;
            }
        }


        private void EnterCommand()
        {
            var data = _currentText.Split(new[] {' ', ','}, StringSplitOptions.RemoveEmptyEntries);
            if (_commandHistory.Count == 0 || _commandHistory[0] != _currentText)
                _commandHistory.Insert(0, _currentText);
            _drawCommands.Insert(0, "> " + _currentText);
            _currentText = "";
            _seekIndex = -1;

            var args = new string[data.Length - 1];
            for (var i = 1; i < data.Length; i++)
                args[i - 1] = data[i];
            ExecuteCommand(data[0].ToLower(), args);
        }


        private void FindFirstTab()
        {
            for (var i = 0; i < _sorted.Count; i++)
                if (_tabSearch == "" || _sorted[i].IndexOf(_tabSearch) == 0)
                {
                    _tabIndex = i;
                    break;
                }
        }


        private void FindLastTab()
        {
            for (var i = 0; i < _sorted.Count; i++)
                if (_tabSearch == "" || _sorted[i].IndexOf(_tabSearch) == 0)
                    _tabIndex = i;
        }


        internal void Render()
        {
#if DEBUG
            if (RuntimeInspector != null)
            {
                RuntimeInspector.Update();
                RuntimeInspector.Render();
            }
#endif

            if (!IsOpen)
                return;

            var screenWidth = Screen.Width;
            var screenHeight = Screen.Height;
            var workingWidth = screenWidth - 2 * HorizontalPadding;

            Graphics.Graphics.Instance.Batcher.Begin();

            // setup the rect that encompases the command entry section
            var commandEntryRect = RectangleExt.FromFloats(HorizontalPadding, screenHeight - LineHeight * RenderScale,
                workingWidth, LineHeight * RenderScale);

            // take into account text padding. move our location up a bit and expand the Rect to accommodate
            commandEntryRect.Location -= new Point(0, TextPaddingY * 2);
            commandEntryRect.Height += TextPaddingY * 2;

            Graphics.Graphics.Instance.Batcher.DrawRect(commandEntryRect, Color.Black * Opacity);
            var commandLineString = "> " + _currentText;
            if (_underscore)
                commandLineString += "_";

            var commandTextPosition =
                commandEntryRect.Location.ToVector2() + new Vector2(TextPaddingX, TextPaddingY);
            Graphics.Graphics.Instance.Batcher.DrawString(Graphics.Graphics.Instance.BitmapFont, commandLineString, commandTextPosition,
                Color.White, 0, Vector2.Zero, new Vector2(RenderScale), SpriteEffects.None, 0);

            if (_drawCommands.Count > 0)
            {
                // start with the total height of the text then add in padding. We have an extra padding because we pad each line and the top/bottom
                var height = LineHeight * RenderScale * _drawCommands.Count;
                height += (_drawCommands.Count + 1) * TextPaddingY;

                var topOfHistoryRect = commandEntryRect.Y - height - CommandHistoryPadding;
                Graphics.Graphics.Instance.Batcher.DrawRect(HorizontalPadding, topOfHistoryRect, workingWidth, height,
                    Color.Black * Opacity);

                var yPosFirstLine = topOfHistoryRect + height - TextPaddingY - LineHeight * RenderScale;
                for (var i = 0; i < _drawCommands.Count; i++)
                {
                    var yPosCurrentLineAddition = i * LineHeight * RenderScale + i * TextPaddingY;
                    var position = new Vector2(HorizontalPadding + TextPaddingX,
                        yPosFirstLine - yPosCurrentLineAddition);
                    var color = _drawCommands[i].IndexOf(">") == 0 ? Color.Yellow : Color.White;
                    Graphics.Graphics.Instance.Batcher.DrawString(Graphics.Graphics.Instance.BitmapFont, _drawCommands[i], position,
                        color, 0, Vector2.Zero, new Vector2(RenderScale), SpriteEffects.None, 0);
                }
            }

            Graphics.Graphics.Instance.Batcher.End();
        }

        #endregion


        #region Execute

        private void ExecuteCommand(string command, string[] args)
        {
            if (_commands.ContainsKey(command))
                _commands[command].Action(args);
            else
                Log("Command '" + command + "' not found! Type 'help' for list of commands");
        }


        private void ExecuteFunctionKeyAction(int num)
        {
            if (_functionKeyActions[num] != null)
                _functionKeyActions[num]();
        }


        public static void BindActionToFunctionKey(int functionKey, Action action)
        {
            Instance._functionKeyActions[functionKey - 1] = action;
        }

        #endregion


        #region Parse Commands

        private void BuildCommandsList()
        {
            // this will get us the Nez assembly
            ProcessAssembly(typeof(DebugConsole).GetTypeInfo().Assembly);

            // this will get us the current executables assembly in 99.9% of cases
            // for now we will let the next section handle loading this. If it doesnt work out we'll uncomment this
            //processAssembly( Core._instance.GetType().GetTypeInfo().Assembly );

            try
            {
                // this is a nasty hack that lets us get at all the assemblies. It is only allowed to exist because this will never get
                // hit in a release build.
                var appDomainType = typeof(string).GetTypeInfo().Assembly.GetType("System.AppDomain");
                var domain = appDomainType.GetRuntimeProperty("CurrentDomain").GetMethod.Invoke(null, new object[] { });
                var assembliesMethod = ReflectionUtils.GetMethodInfo(domain, "GetAssemblies");
                // not sure about arguments, detect in runtime
                var methodCallParams = assembliesMethod.GetParameters().Length == 0
                    ? new object[] { }
                    : new object[] {false};
                var assemblies = assembliesMethod.Invoke(domain, methodCallParams) as Assembly[];

                var ignoredAssemblies = new[]
                {
                    "mscorlib", "MonoMac", "MonoGame.Framework", "Mono.Security", "System", "OpenTK",
                    "ObjCImplementations", "Nez"
                };
                foreach (var assembly in assemblies)
                {
                    var name = assembly.GetName().Name;
                    if (name.StartsWith("System.") || ignoredAssemblies.Contains(name))
                        continue;

                    ProcessAssembly(assembly);
                }
            }
            catch (Exception e)
            {
                Debug.Debug.Log("DebugConsole pooped itself trying to get all the loaded assemblies. {0}", e);
            }


            // Maintain the sorted command list
            foreach (var command in _commands)
                _sorted.Add(command.Key);
            _sorted.Sort();
        }


        private void ProcessAssembly(Assembly assembly)
        {
            foreach (var type in assembly.DefinedTypes)
            foreach (var method in type.DeclaredMethods)
            {
                CommandAttribute attr = null;
                var attrs = method.GetCustomAttributes(typeof(CommandAttribute), false)
                    .Where(a => a is CommandAttribute);
                if (EnumerableExt.Count(attrs) > 0)
                    attr = attrs.First() as CommandAttribute;

                if (attr != null)
                    ProcessMethod(method, attr);
            }
        }


        private void ProcessMethod(MethodInfo method, CommandAttribute attr)
        {
            if (!method.IsStatic)
                throw new Exception(method.DeclaringType.Name + "." + method.Name +
                                    " is marked as a command, but is not static");
            var info = new CommandInfo();
            info.Help = attr.Help;

            var parameters = method.GetParameters();
            var defaults = new object[parameters.Length];
            var usage = new string[parameters.Length];

            for (var i = 0; i < parameters.Length; i++)
            {
                var p = parameters[i];
                usage[i] = p.Name + ":";

                if (p.ParameterType == typeof(string))
                    usage[i] += "string";
                else if (p.ParameterType == typeof(int))
                    usage[i] += "int";
                else if (p.ParameterType == typeof(float))
                    usage[i] += "float";
                else if (p.ParameterType == typeof(bool))
                    usage[i] += "bool";
                else
                    throw new Exception(method.DeclaringType.Name + "." + method.Name +
                                        " is marked as a command, but has an invalid parameter type. Allowed types are: string, int, float, and bool");

                // no System.DBNull in PCL so we fake it
                if (p.DefaultValue.GetType().FullName == "System.DBNull")
                {
                    defaults[i] = null;
                }
                else if (p.DefaultValue != null)
                {
                    defaults[i] = p.DefaultValue;
                    if (p.ParameterType == typeof(string))
                        usage[i] += "=\"" + p.DefaultValue + "\"";
                    else
                        usage[i] += "=" + p.DefaultValue;
                }
                else
                {
                    defaults[i] = null;
                }
            }

            if (usage.Length == 0)
                info.Usage = "";
            else
                info.Usage = "[" + string.Join(" ", usage) + "]";

            info.Action = args =>
            {
                if (parameters.Length == 0)
                {
                    method.Invoke(null, null);
                }
                else
                {
                    var param = (object[]) defaults.Clone();

                    for (var i = 0; i < param.Length && i < args.Length; i++)
                        if (parameters[i].ParameterType == typeof(string))
                            param[i] = ArgString(args[i]);
                        else if (parameters[i].ParameterType == typeof(int))
                            param[i] = ArgInt(args[i]);
                        else if (parameters[i].ParameterType == typeof(float))
                            param[i] = ArgFloat(args[i]);
                        else if (parameters[i].ParameterType == typeof(bool))
                            param[i] = ArgBool(args[i]);

                    if (StrictlyThrowExceptions)
                    {
                        method.Invoke(null, param);
                    }
                    else
                    {
                        try
                        {
                            method.Invoke(null, param);
                        }
                        catch (Exception e)
                        {
                            Log(e);
                        }                                      
                    }

                }
            };

            _commands[attr.Name] = info;
        }
        
        /// <summary>
        /// If true, debug console commands won't be surrounded by a Try/Catch
        /// May annoying as typo's in parameters might throw an exception.
        /// </summary>
        public bool StrictlyThrowExceptions { get; set; } = false;


        private struct CommandInfo
        {
            public Action<string[]> Action;
            public string Help;
            public string Usage;
        }


        #region Parsing Arguments

        private static string ArgString(string arg)
        {
            if (arg == null)
                return "";
            return arg;
        }


        private static bool ArgBool(string arg)
        {
            if (arg != null)
                return !(arg == "0" || arg.ToLower() == "false" || arg.ToLower() == "f");
            return false;
        }


        private static int ArgInt(string arg)
        {
            try
            {
                return Convert.ToInt32(arg);
            }
            catch
            {
                return 0;
            }
        }


        private static float ArgFloat(string arg)
        {
            try
            {
                return Convert.ToSingle(arg);
            }
            catch
            {
                return 0;
            }
        }

        #endregion

        #endregion
    }
}