using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StocksMonitor
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
            
        }
    }
}


/*
 * TODO
 * REfactorera
 * fler testfall
 * Fixa så att när jag markerar en aktie så blir den kopierad till clipboard, för att klistra in sök i Avanza
 * Lägg till så att jag kan börja filtrera bort aktier i en separat dialog, sätt hide hidden, till default Enabl.
 * */
