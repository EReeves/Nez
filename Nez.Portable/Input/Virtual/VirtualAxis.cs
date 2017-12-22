using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

namespace Nez
{
	/// <summary>
	///     A virtual input represented as a float between -1 and 1
	/// </summary>
	public class VirtualAxis : VirtualInput
    {
        public List<Node> Nodes = new List<Node>();


        public VirtualAxis()
        {
        }


        public VirtualAxis(params Node[] nodes)
        {
            this.Nodes.AddRange(nodes);
        }

        public float Value
        {
            get
            {
                for (var i = 0; i < Nodes.Count; i++)
                {
                    var val = Nodes[i].Value;
                    if (val != 0)
                        return val;
                }

                return 0;
            }
        }


        public override void Update()
        {
            for (var i = 0; i < Nodes.Count; i++)
                Nodes[i].Update();
        }


        public static implicit operator float(VirtualAxis axis)
        {
            return axis.Value;
        }


        #region Node types

        public abstract class Node : VirtualInputNode
        {
            public abstract float Value { get; }
        }


        public class GamePadLeftStickX : Node
        {
            public float Deadzone;
            public int GamepadIndex;


            public GamePadLeftStickX(int gamepadIndex = 0, float deadzone = Input.DefaultDeadzone)
            {
                this.GamepadIndex = gamepadIndex;
                this.Deadzone = deadzone;
            }

            public override float Value =>
                Mathf.SignThreshold(Input.GamePads[GamepadIndex].GetLeftStick(Deadzone).X, Deadzone);
        }


        public class GamePadLeftStickY : Node
        {
            public float Deadzone;
            public int GamepadIndex;

	        /// <summary>
	        ///     if true, pressing up will return -1 and down will return 1 matching GamePadDpadUpDown
	        /// </summary>
	        public bool InvertResult = true;


            public GamePadLeftStickY(int gamepadIndex = 0, float deadzone = Input.DefaultDeadzone)
            {
                this.GamepadIndex = gamepadIndex;
                this.Deadzone = deadzone;
            }

            public override float Value
            {
                get
                {
                    var multiplier = InvertResult ? -1 : 1;
                    return multiplier *
                           Mathf.SignThreshold(Input.GamePads[GamepadIndex].GetLeftStick(Deadzone).Y, Deadzone);
                }
            }
        }


        public class GamePadRightStickX : Node
        {
            public float Deadzone;
            public int GamepadIndex;


            public GamePadRightStickX(int gamepadIndex = 0, float deadzone = Input.DefaultDeadzone)
            {
                this.GamepadIndex = gamepadIndex;
                this.Deadzone = deadzone;
            }

            public override float Value =>
                Mathf.SignThreshold(Input.GamePads[GamepadIndex].GetRightStick(Deadzone).X, Deadzone);
        }


        public class GamePadRightStickY : Node
        {
            public float Deadzone;
            public int GamepadIndex;


            public GamePadRightStickY(int gamepadIndex = 0, float deadzone = Input.DefaultDeadzone)
            {
                this.GamepadIndex = gamepadIndex;
                this.Deadzone = deadzone;
            }

            public override float Value =>
                Mathf.SignThreshold(Input.GamePads[GamepadIndex].GetRightStick(Deadzone).Y, Deadzone);
        }


        public class GamePadDpadLeftRight : Node
        {
            public int GamepadIndex;


            public GamePadDpadLeftRight(int gamepadIndex = 0)
            {
                this.GamepadIndex = gamepadIndex;
            }


            public override float Value
            {
                get
                {
                    if (Input.GamePads[GamepadIndex].DpadRightDown)
                        return 1f;
                    if (Input.GamePads[GamepadIndex].DpadLeftDown)
                        return -1f;
                    return 0f;
                }
            }
        }


        public class GamePadDpadUpDown : Node
        {
            public int GamepadIndex;


            public GamePadDpadUpDown(int gamepadIndex = 0)
            {
                this.GamepadIndex = gamepadIndex;
            }


            public override float Value
            {
                get
                {
                    if (Input.GamePads[GamepadIndex].DpadDownDown)
                        return 1f;
                    if (Input.GamePads[GamepadIndex].DpadUpDown)
                        return -1f;
                    return 0f;
                }
            }
        }


        public class KeyboardKeys : Node
        {
            private bool _turned;

            private float _value;
            public Keys Negative;
            public OverlapBehavior OverlapBehavior;
            public Keys Positive;


            public KeyboardKeys(OverlapBehavior overlapBehavior, Keys negative, Keys positive)
            {
                this.OverlapBehavior = overlapBehavior;
                this.Negative = negative;
                this.Positive = positive;
            }


            public override float Value => _value;


            public override void Update()
            {
                if (Input.IsKeyDown(Positive))
                {
                    if (Input.IsKeyDown(Negative))
                    {
                        switch (OverlapBehavior)
                        {
                            default:
                            case OverlapBehavior.CancelOut:
                                _value = 0;
                                break;

                            case OverlapBehavior.TakeNewer:
                                if (!_turned)
                                {
                                    _value *= -1;
                                    _turned = true;
                                }
                                break;
                            case OverlapBehavior.TakeOlder:
                                //value stays the same
                                break;
                        }
                    }
                    else
                    {
                        _turned = false;
                        _value = 1;
                    }
                }
                else if (Input.IsKeyDown(Negative))
                {
                    _turned = false;
                    _value = -1;
                }
                else
                {
                    _turned = false;
                    _value = 0;
                }
            }
        }

        #endregion
    }
}