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
 *  Parsa resterande av data som behövs från Avanza
 * Istället för knappar, gör menyer 
 * 
 * 
 * 
 * 
 * */
