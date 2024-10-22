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

// TODO Lägg till så att jag kan börja filtrera bort aktier i en separat dialog, sätt hide hidden, till default Enabl.
// TODO, när jag lägger upp resultat och historik, i vanliga fönstret, kan jag jämföra hur simuleringar har gått.
// TODO, en dialog med simuleringsresultat enbart, där jag kan se historiken och hur det gått. 
// TODO, export av master databas till sql, kan importeras in sedan, för att fylla Simulerings / test databas vid behov.
