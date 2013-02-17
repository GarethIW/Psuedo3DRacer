using System;

namespace Psuedo3DRacer
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (Psuedo3DRacer game = new Psuedo3DRacer())
            {
                game.Run();
            }
        }
    }
#endif
}

