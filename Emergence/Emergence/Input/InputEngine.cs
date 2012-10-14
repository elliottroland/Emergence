using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace Emergence
{
    public class InputEngine
    {

        public class Binding
        {

            //Game Controls, keyboard and Controller bindings

            public Actions[] gameControls = { Actions.Jump, Actions.Downgrade, Actions.Reload, Actions.Unbound,
                                    Actions.Scoreboard,Actions.Scoreboard, Actions.Unbound, Actions.Sprint,
                                    Actions.Aim, Actions.Fire, Actions.Pause, Actions.Unbound,
                                    Actions.Unbound,Actions.Unbound,Actions.Unbound,Actions.Unbound};


            public Buttons[] gameButtons = { Buttons.A, Buttons.B, Buttons.X, Buttons.Y, 
                                    Buttons.RightShoulder, Buttons.LeftShoulder, Buttons.RightStick, Buttons.LeftStick,
                                    Buttons.RightTrigger, Buttons.LeftTrigger, Buttons.Start, Buttons.Back,
                                    Buttons.DPadUp, Buttons.DPadDown, Buttons.DPadLeft, Buttons.DPadRight};


            public Keys[] gameKeys = { Keys.Space, Keys.Q, Keys.R, Keys.T, 
                                    Keys.Tab, Keys.Tab, Keys.T, Keys.LeftShift};


            //Menu Controls, Keyboard and controller bindings
            public MenuAction[] menuControls = {
                                    MenuAction.Select, MenuAction.Back, MenuAction.EditConfig, MenuAction.Join,
                                    MenuAction.Up, MenuAction.Down};

            public Buttons[] menuButtons = {Buttons.A, Buttons.B, Buttons.X, Buttons.Start,
                                    Buttons.LeftThumbstickUp, Buttons.LeftThumbstickDown,
                                    };

            public Keys[] menuKeys = {Keys.Enter, Keys.Escape, Keys.Space, Keys.Enter,
                                         Keys.Up, Keys.Down                                      
                                     };

            public Binding()
            {
            }
        }






        Vector2 mouseOut = Vector2.Zero;
        Vector2 screenCenter = new Vector2(400, 300);

        List<Actions>[] playerActions = new List<Actions>[5];
        List<MenuAction>[] playerMenuActions = new List<MenuAction>[5];

        GamePadState[] oldGPStates = new GamePadState[4];

        GamePadState current;
        GamePadState old;

        int firstConnected = -1;

        KeyboardState currentKB = Keyboard.GetState();
        KeyboardState oldKB = Keyboard.GetState();

        MouseState oldMouse = Mouse.GetState();

        Binding[] playerBindings = new Binding[5];

        CoreEngine core;


        public InputEngine(CoreEngine c)
        {
            core = c;
            for (int i = 0; i < 5; i++)
            {
                playerBindings[i] = new Binding();
                playerActions[i] = new List<Actions>();
                playerMenuActions[i] = new List<MenuAction>();
            }
        }

        //triggers-----------------------------------------------------

        float getLT()
        {
            return current.Triggers.Left;
        }
        float getRT()
        {
            return current.Triggers.Right;
        }

        //Button checks-----------------------------------------------------
        bool isPressed(Keys key)
        {
            if (currentKB.IsKeyDown(key) && oldKB.IsKeyUp(key))
            {
                return true;
            }
            return false;
        }
        bool isPressed(Buttons button)
        {
            if (current.IsButtonDown(button) && old.IsButtonUp(button))
            {
                return true;
            }
            return false;
        }
        public bool isDown(Keys key)
        {
            return currentKB.IsKeyDown(key);
        }
        bool isDown(Buttons button)
        {
            return current.IsButtonDown(button);
        }

        //Keyboard returns
        public List<Actions> getGameKeys()
        {
            return playerActions[4];
        }
        public List<MenuAction> getMenuKeys()
        {
            return playerMenuActions[4];
        }
        public Vector2 getMove()
        {
            double[] move = new double[2];
            //----------------------------------------
            if (isDown(Keys.W))
                move[1] = 1;
            else if (isDown(Keys.S))
                move[1] = -1;
            if (isDown(Keys.A))
                move[0] = -1;
            else if (isDown(Keys.D))
                move[0] = 1;
            //----------------------------------------
            //float moveAngle = (float)Math.Atan2(move[0], move[1]);

            return new Vector2((float)move[0], (float)move[1]);
        }
        public Vector2 getLook()
        {
            return mouseOut;
        }

        //Controller returns
        public List<Actions> getGameButtons(PlayerIndex p)
        {
            return playerActions[(int)p];
        }
        public List<MenuAction> getMenuButtons(PlayerIndex p)
        {
            return playerMenuActions[(int)p];
        }
        public Vector2 getMove(PlayerIndex p)
        {
            return new Vector2(GamePad.GetState(p).ThumbSticks.Left.X, GamePad.GetState(p).ThumbSticks.Left.Y);
        }
        public Vector2 getLook(PlayerIndex p)
        {
            return new Vector2(GamePad.GetState(p).ThumbSticks.Right.X, -GamePad.GetState(p).ThumbSticks.Right.Y);
        }

        //return the menu owner
        public int getFirstController()
        {
            return firstConnected;
        }


        public void Update()
        {
            if (core.currentState == GameState.MenuScreen)
            {
                UpdateMenuInput();
            }
            if (core.currentState == GameState.GameScreen)
            {
                UpdateGameInput();
            }
        }

        public void UpdateGameInput()
        {
            //Check for PC or XBOX
            bool onXBOX = false;

            //reset action lists
            for (int i = 0; i < 5; i++)
            {
                playerActions[i].Clear();
            }

            //Controller input
            for (int pIndex = 0; pIndex < 4; pIndex++)
            {
                current = GamePad.GetState((PlayerIndex)pIndex);

                old = oldGPStates[pIndex];


                for (int i = 0; i < playerBindings[0].gameButtons.Length; i++)
                {
                    if (playerBindings[pIndex].gameControls[i] == Actions.Downgrade) {
                        if (isPressed(playerBindings[pIndex].gameButtons[i]))//if button is down             
                            playerActions[pIndex].Add(playerBindings[pIndex].gameControls[i]);//add action bound to button   
                    }
                    else {
                        if (isDown(playerBindings[pIndex].gameButtons[i]))//if button is down             
                            playerActions[pIndex].Add(playerBindings[pIndex].gameControls[i]);//add action bound to button   
                    }
                }

                //set old state (not necessary if only using isDown)
                oldGPStates[pIndex] = current;
            }


            //keyboard input
            if (!onXBOX)
            {
                //Console.WriteLine(playerActions.Length);
                currentKB = Keyboard.GetState();

                for (int i = 0; i < playerBindings[0].gameKeys.Length; i++)
                {
                    if (playerBindings[4].gameControls[i] == Actions.Downgrade) {
                        if (isPressed(playerBindings[4].gameKeys[i]))
                            playerActions[4].Add(playerBindings[4].gameControls[i]);
                    }
                    else {
                        if (isDown(playerBindings[4].gameKeys[i]))
                            playerActions[4].Add(playerBindings[4].gameControls[i]);
                    }
                }


                //Update mouse data
                Vector2 mousePos = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
                mouseOut = new Vector2(mousePos.X - screenCenter.X, mousePos.Y - screenCenter.Y);
                Mouse.SetPosition((int)screenCenter.X, (int)screenCenter.Y);

                //test shooting
                if (Mouse.GetState().LeftButton == ButtonState.Pressed)// && oldMouse.LeftButton == ButtonState.Released)
                    playerActions[0].Add(Actions.Fire);

                //set old state
                oldKB = currentKB;
                oldMouse = Mouse.GetState();
            }
        }

        public void UpdateMenuInput()
        {
            //Check for PC or XBOX
            bool onXBOX = false;

            //reset action lists
            for (int i = 0; i < 5; i++)
            {
                playerMenuActions[i].Clear();
            }

            //Controller input
            for (int pIndex = 3; pIndex >= 0; pIndex--)
            {

                old = oldGPStates[pIndex];



                current = GamePad.GetState((PlayerIndex)pIndex);

                for (int i = 0; i < playerBindings[0].menuButtons.Length; i++)
                {
                    if (isPressed(playerBindings[pIndex].menuButtons[i]))//if button is down
                        playerMenuActions[pIndex].Add(playerBindings[pIndex].menuControls[i]);//add action bound to button
                }


                //set old state (not necessary if only using isDown)
                oldGPStates[pIndex] = current;
            }


            //keyboard input
            if (!onXBOX)
            {

                currentKB = Keyboard.GetState();

                for (int i = 0; i < playerBindings[0].menuKeys.Length; i++)
                {
                    if (isPressed(playerBindings[4].menuKeys[i]))
                        playerMenuActions[4].Add(playerBindings[4].menuControls[i]);
                }


                //Update mouse data
                Vector2 mousePos = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
                mouseOut = new Vector2(200, mousePos.Y - 450);
                if (Mouse.GetState().LeftButton == ButtonState.Pressed && oldMouse.LeftButton == ButtonState.Released)
                    playerMenuActions[4].Add(MenuAction.Select);
                //Mouse.SetPosition((int)screenCenter.X, (int)screenCenter.Y);

                //set old state
                oldKB = currentKB;
                oldMouse = Mouse.GetState();
            }
        }

    }
}