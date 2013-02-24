#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace Psuedo3DRacer.Mono.Windows
{
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        private static Psuedo3DRacer game;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            game = new Psuedo3DRacer();
            game.Run();
        }
    }
}
