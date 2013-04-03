#region File Description
//-----------------------------------------------------------------------------
// InputState.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
#if WINRT || WINDOWS_PHONE
using Windows.Devices.Sensors;
#endif
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
#if WINDOWS_PHONE || WINRT || TOUCH
using Microsoft.Xna.Framework.Input.Touch;
#endif
using System.Collections.Generic;
#endregion

namespace Psuedo3DRacer
{
    /// <summary>
    /// Helper for reading input from keyboard, gamepad, and touch input. This class 
    /// tracks both the current and previous state of the input devices, and implements 
    /// query methods for high level input actions such as "move up through the menu"
    /// or "pause the game".
    /// </summary>
    public class InputState
    {
        #region Fields

        public const int MaxInputs = 1;

        public readonly KeyboardState[] CurrentKeyboardStates;
        public readonly GamePadState[] CurrentGamePadStates;
        public MouseState CurrentMouseState;

        public readonly KeyboardState[] LastKeyboardStates;
        public readonly GamePadState[] LastGamePadStates;
        public MouseState LastMouseState;

        public readonly bool[] GamePadWasConnected;

#if WINDOWS_PHONE || WINRT || TOUCH
        public TouchCollection TouchState;

        public readonly List<GestureSample> Gestures = new List<GestureSample>();

        public GestureSample? TapGesture;
        public GestureSample? PinchGesture;
        public GestureSample? DragGesture;
#endif

        private int mouseHoldCount;
        public bool MouseDragging;
        public bool MouseLeftClick;
        public Vector2 MouseDelta;

        public Vector2? TapPosition;

        public Vector3 AccelerometerVect;

#if WINRT || WINDOWS_PHONE
        private Accelerometer accelerometer;
        private uint getReadingInterval = 0;
#endif


        #endregion

        #region Initialization


        /// <summary>
        /// Constructs a new input state.
        /// </summary>
        public InputState()
        {
            CurrentKeyboardStates = new KeyboardState[MaxInputs];
            CurrentGamePadStates = new GamePadState[MaxInputs];

            LastKeyboardStates = new KeyboardState[MaxInputs];
            LastGamePadStates = new GamePadState[MaxInputs];

            GamePadWasConnected = new bool[MaxInputs];

#if WINRT || WINDOWS_PHONE
            initAccel();
#endif
        }


        #endregion

        #region Public Methods


        /// <summary>
        /// Reads the latest state of the keyboard and gamepad.
        /// </summary>
        public void Update()
        {
            for (int i = 0; i < MaxInputs; i++)
            {
                LastKeyboardStates[i] = CurrentKeyboardStates[i];
                LastGamePadStates[i] = CurrentGamePadStates[i];


                CurrentKeyboardStates[i] = Keyboard.GetState((PlayerIndex)i);
#if !WINDOWS_PHONE
                CurrentGamePadStates[i] = GamePad.GetState((PlayerIndex)i);
#endif

                // Keep track of whether a gamepad has ever been
                // connected, so we can detect if it is unplugged.
                if (CurrentGamePadStates[i].IsConnected)
                {
                    GamePadWasConnected[i] = true;
                }
            }

            LastMouseState = CurrentMouseState;
            CurrentMouseState = Mouse.GetState();

            //TouchState = TouchPanel.GetState();

#if WINDOWS_PHONE || WINRT || TOUCH
            Gestures.Clear();
            while (TouchPanel.IsGestureAvailable)
            {
                Gestures.Add(TouchPanel.ReadGesture());
            }

            TapGesture = null;
            DragGesture = null;
            PinchGesture = null;
            foreach (GestureSample gest in Gestures)
            {
                if (gest.GestureType == GestureType.Tap)
                    TapGesture = gest;
                if (gest.GestureType == GestureType.FreeDrag)
                    DragGesture = gest;
                if (gest.GestureType == GestureType.Pinch)
                    PinchGesture = gest;

            }


#endif

            MouseDelta = new Vector2(CurrentMouseState.X - LastMouseState.X, CurrentMouseState.Y - LastMouseState.Y);

            MouseLeftClick = false;
            MouseDragging = false;
            if (LastMouseState.LeftButton == ButtonState.Pressed && CurrentMouseState.LeftButton == ButtonState.Pressed)
            {
                mouseHoldCount++;
                if (mouseHoldCount > 0 && MouseDelta.Length() > 5f) MouseDragging = true;
                if (mouseHoldCount > 30) MouseDragging = true;
            }
            else
            {
                if (mouseHoldCount > 0 && mouseHoldCount < 10) MouseLeftClick = true;
                mouseHoldCount = 0;
            }

            TapPosition = null;
#if WINDOWS_PHONE || WINRT || TOUCH
            if (TapGesture.HasValue) TapPosition = TapGesture.Value.Position;
#endif
            if (MouseLeftClick) TapPosition = new Vector2(CurrentMouseState.X, CurrentMouseState.Y);
           
        }


        /// <summary>
        /// Helper for checking if a key was newly pressed during this update. The
        /// controllingPlayer parameter specifies which player to read input for.
        /// If this is null, it will accept input from any player. When a keypress
        /// is detected, the output playerIndex reports which player pressed it.
        /// </summary>
        public bool IsNewKeyPress(Keys key, PlayerIndex? controllingPlayer,
                                            out PlayerIndex playerIndex)
        {
            if (controllingPlayer.HasValue)
            {
                // Read input from the specified player.
                playerIndex = controllingPlayer.Value;

                int i = (int)playerIndex;

                return (CurrentKeyboardStates[i].IsKeyDown(key) &&
                        LastKeyboardStates[i].IsKeyUp(key));
            }
            else
            {
                // Accept input from any player.
                return (IsNewKeyPress(key, PlayerIndex.One, out playerIndex));// ||
                        //IsNewKeyPress(key, PlayerIndex.Two, out playerIndex) ||
                        //IsNewKeyPress(key, PlayerIndex.Three, out playerIndex) ||
                        //IsNewKeyPress(key, PlayerIndex.Four, out playerIndex));
            }
        }


        /// <summary>
        /// Helper for checking if a button was newly pressed during this update.
        /// The controllingPlayer parameter specifies which player to read input for.
        /// If this is null, it will accept input from any player. When a button press
        /// is detected, the output playerIndex reports which player pressed it.
        /// </summary>
        public bool IsNewButtonPress(Buttons button, PlayerIndex? controllingPlayer,
                                                     out PlayerIndex playerIndex)
        {
            if (controllingPlayer.HasValue)
            {
                // Read input from the specified player.
                playerIndex = controllingPlayer.Value;

                int i = (int)playerIndex;

                return (CurrentGamePadStates[i].IsButtonDown(button) &&
                        LastGamePadStates[i].IsButtonUp(button));
            }
            else
            {
                // Accept input from any player.
                return (IsNewButtonPress(button, PlayerIndex.One, out playerIndex));// ||
                        //IsNewButtonPress(button, PlayerIndex.Two, out playerIndex) ||
                        //IsNewButtonPress(button, PlayerIndex.Three, out playerIndex) ||
                        //IsNewButtonPress(button, PlayerIndex.Four, out playerIndex));
            }
        }


        /// <summary>
        /// Checks for a "menu select" input action.
        /// The controllingPlayer parameter specifies which player to read input for.
        /// If this is null, it will accept input from any player. When the action
        /// is detected, the output playerIndex reports which player pressed it.
        /// </summary>
        public bool IsMenuSelect(PlayerIndex? controllingPlayer,
                                 out PlayerIndex playerIndex)
        {
            return IsNewKeyPress(Keys.Enter, controllingPlayer, out playerIndex) ||
                   IsNewButtonPress(Buttons.A, controllingPlayer, out playerIndex) ||
                   IsNewButtonPress(Buttons.Start, controllingPlayer, out playerIndex);
        }


        /// <summary>
        /// Checks for a "menu cancel" input action.
        /// The controllingPlayer parameter specifies which player to read input for.
        /// If this is null, it will accept input from any player. When the action
        /// is detected, the output playerIndex reports which player pressed it.
        /// </summary>
        public bool IsMenuCancel(PlayerIndex? controllingPlayer,
                                 out PlayerIndex playerIndex)
        {
            return IsNewKeyPress(Keys.Escape, controllingPlayer, out playerIndex) ||
                   IsNewButtonPress(Buttons.B, controllingPlayer, out playerIndex) ||
                   IsNewButtonPress(Buttons.Back, controllingPlayer, out playerIndex);
        }


        /// <summary>
        /// Checks for a "menu up" input action.
        /// The controllingPlayer parameter specifies which player to read
        /// input for. If this is null, it will accept input from any player.
        /// </summary>
        public bool IsMenuUp(PlayerIndex? controllingPlayer)
        {
            PlayerIndex playerIndex;

            return IsNewKeyPress(Keys.Up, controllingPlayer, out playerIndex) ||
                   IsNewButtonPress(Buttons.DPadUp, controllingPlayer, out playerIndex) ||
                   IsNewButtonPress(Buttons.LeftThumbstickDown, controllingPlayer, out playerIndex);
        }


        /// <summary>
        /// Checks for a "menu down" input action.
        /// The controllingPlayer parameter specifies which player to read
        /// input for. If this is null, it will accept input from any player.
        /// </summary>
        public bool IsMenuDown(PlayerIndex? controllingPlayer)
        {
            PlayerIndex playerIndex;

            return IsNewKeyPress(Keys.Down, controllingPlayer, out playerIndex) ||
                   IsNewButtonPress(Buttons.DPadDown, controllingPlayer, out playerIndex) ||
                   IsNewButtonPress(Buttons.LeftThumbstickUp, controllingPlayer, out playerIndex);
        }


        /// <summary>
        /// Checks for a "pause the game" input action.
        /// The controllingPlayer parameter specifies which player to read
        /// input for. If this is null, it will accept input from any player.
        /// </summary>
        public bool IsPauseGame(PlayerIndex? controllingPlayer)
        {
            PlayerIndex playerIndex;

            return IsNewKeyPress(Keys.Escape, controllingPlayer, out playerIndex) ||
                   IsNewButtonPress(Buttons.Back, controllingPlayer, out playerIndex) ||
                   IsNewButtonPress(Buttons.Start, controllingPlayer, out playerIndex);
        }
        
#if WINDOWS_PHONE || WINRT || TOUCH
        public float GetScaleFactor(GestureSample gest)
        {
            return GetScaleFactor(gest.Position, gest.Position2, gest.Delta, gest.Delta2);
        }
#endif
        public float GetScaleFactor(Vector2 position1, Vector2 position2, Vector2 delta1, Vector2 delta2)
        {
            Vector2 oldPosition1 = position1 - delta1;
            Vector2 oldPosition2 = position2 - delta2;

            float distance = Vector2.Distance(position1, position2);
            float oldDistance = Vector2.Distance(oldPosition1, oldPosition2);

            if (oldDistance == 0 || distance == 0)
            {
                return 1.0f;
            }

            return (distance / oldDistance);
        }

#if WINRT || WINDOWS_PHONE
        private void initAccel()
        {

            accelerometer = Accelerometer.GetDefault();
            if (accelerometer!=null)
            {
                // Choose a report interval supported by the sensor
                uint minimumReportInterval = accelerometer.MinimumReportInterval;
                uint reportInterval = minimumReportInterval > 16 ? minimumReportInterval : 16;
                accelerometer.ReportInterval = reportInterval;
                accelerometer.ReadingChanged += accelerometer_ReadingChanged;
            }
            
        }

        void accelerometer_ReadingChanged(Accelerometer sender, AccelerometerReadingChangedEventArgs args)
        {
            AccelerometerReading ar = sender.GetCurrentReading();
            AccelerometerVect = new Vector3((float)ar.AccelerationX, (float)ar.AccelerationY, (float)ar.AccelerationZ);

#if WINDOWS_PHONE
            AccelerometerVect = new Vector3(-(float)ar.AccelerationY, (float)ar.AccelerationX, (float)ar.AccelerationZ);
#endif
        }

    
#endif

//#if WINDOWS_PHONE
//        private void initAccel()
//        {
//            accelerometer = new Accelerometer();
//            accelerometer.Start();
//            accelerometer.CurrentValueChanged += acc_CurrentValueChanged;
//        }

//        void acc_CurrentValueChanged(object sender, SensorReadingEventArgs<AccelerometerReading> e)
//        {
//            AccelerometerVect = e.SensorReading.Acceleration;
//        }
//#endif



        #endregion
    }
}
