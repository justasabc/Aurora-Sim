﻿using Nini.Config;

namespace Aurora.Framework.Physics
{
    public interface IMeshingPlugin
    {
        string GetName();
        IMesher GetMesher(IConfigSource config);
    }
}