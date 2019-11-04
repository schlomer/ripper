using Rip.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RipExample.Lib.Core
{
    public class Contact
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
    }

    public class Phone
    {
        public string Id { get; set; }
        public string ContactId { get; set; }
        public string Type { get; set; }
        public string Number { get; set; }
    }

    public class ContactData
    {
        string dataServerName;
        string databaseName;

        RipClient rip;
        RipDataServer dataServer;
        RipDatabase database;
        RipContainer contacts;
        RipContainer phones;

        RipFilterCommand fFirstNameContains;
        RipFilterCommand fAgeGreater;

        public void Disconnect()
        {
            if(rip != null)
            {
                rip.Disconnect();
                rip.Dispose();
                rip = null;
            }
        }

        public async Task ConnectAsync(string hostServer, int ripperPort, string dataServerName, string databaseName)
        {   
            this.dataServerName = dataServerName;
            this.databaseName = databaseName;
            var contactsContainerName = "Contacts";
            var phonesContainerName = "Phones";

            rip = new RipClient(hostServer, ripperPort);

            await rip.ConnectAsync();
            // connect to the data server
            dataServer = await rip.ConnectOrCreateDataServerAsync(dataServerName);

            // get or add the database
            database = await dataServer.GetDatabaseAsync(databaseName);
            if (database == null)
                database = await dataServer.AddDatabaseAsync(databaseName);

            // get or add contacts container, partitioned by /Id using /Id as the Id
            contacts = await database.GetContainerAsync(contactsContainerName);
            if (contacts == null)
                contacts = await database.AddContainerAsync(contactsContainerName, "/Id", "/Id");

            // get or add phones container, partitioned by /ContactId using /Id as the Id
            phones = await database.GetContainerAsync(phonesContainerName);
            if (phones == null)
            {
                phones = await database.AddContainerAsync(phonesContainerName, "/ContactId", "/Id");

                // prime the database with random contact info if empty
                await FillWithRandomContactsAsync();
            }

            // build filter parts
            fFirstNameContains = new RipFilterCommand
            {
                Command = RipFilterCommands.Contains,
                Left = new RipFilterParameter { Path = "/FirstName" },
                Right = new RipFilterValue { Value = "" }
            };

            fAgeGreater = new RipFilterCommand
            {
                Command = RipFilterCommands.Greater,
                Left = new RipFilterParameter { Path = "/Age" },
                Right = new RipFilterValue { Value = 0 }
            };
        }

        async Task FillWithRandomContactsAsync()
        {            
            string[] firstNames = { "Jim", "Jane", "Sarah", "Mary", "Bob", "Tim", "Floki", "John" };
            string[] lastNames = { "Hanson", "Smith", "Johnson", "Flake", "Bauer", "VanWinkle", "Stark" };

            var rnd = new Random();

            for (int i = 0; i < 100; ++i)
            {
                var fn = firstNames[rnd.Next(firstNames.Length)];
                var ln = lastNames[rnd.Next(lastNames.Length)];
                var age = rnd.Next(1, 100);
                var newContact = new Contact
                {
                    Id = Guid.NewGuid().ToString("N"), // partition key and id for Contact
                    Age = age,
                    FirstName = fn,
                    LastName = ln
                };

                await contacts.SetRecordAsync(newContact);

                for (int j = 0; j < rnd.Next(0,3); ++j)
                {
                    var n1 = rnd.Next(999);
                    var n2 = rnd.Next(9999);
                    var newPhone = new Phone
                    {
                        Id = Guid.NewGuid().ToString("N"), // each phone has its own id
                        ContactId = newContact.Id, // partition key for Phone
                        Number = $"555-{n1:000}-{n2:0000}",
                        Type = (rnd.Next(100) % 2) == 0 ? "Home" : "Work"
                    };

                    await phones.SetRecordAsync(newPhone);
                }
            }  
        }

        public async Task SetContactAsync(Contact contact)
        {
            await contacts.SetRecordAsync(contact);
        }

        public async Task SetPhoneAsync(Phone phone)
        {
            await phones.SetRecordAsync(phone);
        }

        public async IAsyncEnumerable<Contact> GetContactsAsync()
        {
            await foreach (var c in contacts.GetRecordsAsync<Contact>())
                yield return c;
        }

        public async IAsyncEnumerable<Phone> GetPhonesAsync()
        {
            await foreach (var c in phones.GetRecordsAsync<Phone>())
                yield return c;
        }

        public async IAsyncEnumerable<Contact> GetContactsFirstNameContainsAsync(string contains)
        {
            fFirstNameContains.Right = new RipFilterValue { Value = contains };
            var filterJson = fFirstNameContains.ToStringifiedJson();

            await foreach (var c in contacts.GetRecordsAsync<Contact>(filterJson))
                yield return c;
        }

        public async IAsyncEnumerable<Contact> GetContactsFirstNameContainsAndAgeGreaterAsync(string contains, int age)
        {
            // modify the filters with the contains and age parameters
            fFirstNameContains.Right = new RipFilterValue { Value = contains };
            fAgeGreater.Right = new RipFilterValue { Value = age };

            // 'and' the contains and age filters together
            RipFilterCommand and = new RipFilterCommand
            {
                Command = RipFilterCommands.And,
                Left = fFirstNameContains,
                Right = fAgeGreater
            };

            // build the json version or the filter
            var fb = new RipFilterBuilder();
            fb.Filter = and;

            var filterJson = fb.ToStringifiedJson();

            // get all contact records filtered by the filter
            await foreach (var c in contacts.GetRecordsAsync<Contact>(filterJson))
                yield return c;
        }

        public async IAsyncEnumerable<Phone> GetContactPhonesAsync(string contactId)
        {
            // get phone records associated with the partition on the contact container (contactId)
            await foreach (var p in phones.GetRecordsAsync<Phone>(partitionKey: contactId, filterJson: null))
                yield return p;
        }
    }
}
