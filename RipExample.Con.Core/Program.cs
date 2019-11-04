using System;
using System.Threading.Tasks;
using Newtonsoft.Json;

using RipExample.Lib.Core;

namespace RipExample.Con.Core
{
    class Program
    {
        static async Task Main(string[] args)
        {

            var data = new ContactData();
            await data.ConnectAsync("localhost", 1234, "Local1", "ContactApp");

            Console.WriteLine();

            Console.WriteLine("--------");
            Console.WriteLine("Contacts");
            Console.WriteLine("--------");

            await foreach (var r in data.GetContactsAsync())
            {
                Console.WriteLine(JsonConvert.SerializeObject(r));
                Console.WriteLine();
            }

            Console.WriteLine("------");
            Console.WriteLine("Phones");
            Console.WriteLine("------");

            await foreach (var r in data.GetPhonesAsync())
            {
                Console.WriteLine(JsonConvert.SerializeObject(r));
                Console.WriteLine();
            }
        }
    }
}
