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
            tta1.SetBloom(1, 0, 1, 1f);
            AddObject(tta1);
        }
    }
}
