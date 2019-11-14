using System;
using System.Runtime.InteropServices;
using OpenTK;
using OpenGL2D.Helpers;

namespace OpenGL2D
{
    class Program 
    {
        [STAThread]
        static void Main(string[] args)
        {
            using (MainWindow gameWindow = new MainWindow(1024, 768))
            {
                gameWindow.Run();
            }
        }
    }
}
