using System;

namespace RubiksCube {
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (CubeGame game = new CubeGame())
            {
                game.Run();
            }
        }
    }
#endif
}

