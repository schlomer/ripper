using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RipAdmin
{
    static class Program
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


            //var rc = new Rip.Net.RipClient("localhost", 1234);
            //await rc.ConnectAsync();
            //var ds = await rc.ConnectOrCreateDataServerAsync("Local1");            
            //var db = await ds.GetDatabaseAsync("ContactApp");
            //var c = await db.GetContainerAsync("Contacts");
            //await foreach (var x in c.GetRecordsAsync(null))
            //{
            //    int a = 0;
            //}

            //rc.Disconnect();



        }
    }
}
