using OpenTK;
using OpenTK.Input;
using System;
using System.Collections.Generic;

namespace OpenGL2D.Core
{
    class ActorExample : Actor
    {
        public override void Act(KeyboardState keyboardState, MouseState mouseState)
        {
            Vector2 pos = GetPosition();
            // Example:
            if (keyboardState.IsKeyDown(Key.Right))
            {
                pos.X++;
            }
            if (keyboardState.IsKeyDown(Key.Left))
            {
                pos.X--;
            }
            SetPosition(pos);
        }
    }
}
