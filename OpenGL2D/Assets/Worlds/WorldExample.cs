using System;
using System.Collections.Generic;
using System.Text;

namespace OpenGL2D.Core
{
    class WorldExample : World
    {
        public override void Prepare()
        {
            ActorExample tta1 = new ActorExample();
            tta1.SetPosition(128, 128);
            AddObject(tta1);
        }
    }
}
