using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace RubiksCube {
    public interface ICamera {
        Matrix ViewMatrix { get; }
        Matrix ProjectionMatrix { get; }
    }
}
