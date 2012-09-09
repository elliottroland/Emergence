using System;

namespace Emergence {
    static class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args) {
            using (CoreEngine game = new CoreEngine()) {
                game.Run();
            }
        }
    }
}

