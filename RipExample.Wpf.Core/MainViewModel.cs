using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using RipExample.Lib.Core;

namespace RipExample.Wpf.Core
{
    public class MainViewModel : Observable
    {

        ContactData data = new ContactData();

        public void Initialize()
        {
            RipperPort = 1234;
            RipperHost = "localhost";
            DataServerName = "Local1";
            DatabaseName = "ContactApp";
            ConnectionStatus = "DISCONNECTED";
        }

        string ripperHost;
        public string RipperHost
        {
            get => ripperHost;
            set => Set(ref ripperHost, value);
        }

        int ripperPort;
        public int RipperPort
        {
            get => ripperPort;
            set => Set(ref ripperPort, value);
        }

        string dataServerName = "";
        public string DataServerName
        {
            get => dataServerName;
            set => Set(ref dataServerName, value);
        }

        string databaseName = "";
        public string DatabaseName
        {
            get => databaseName;
            set => Set(ref databaseName, value);
        }

        string connectionStatus = "";
        public string ConnectionStatus
        {
            get => connectionStatus;
            set => Set(ref connectionStatus, value);
        }

        public async Task ConnectAsync()
        {
            try
            {
                if (data != null)
                    data.Disconnect();

                ConnectionStatus = "CONNECTING";

                await data.ConnectAsync(RipperHost, RipperPort, DataServerName, DatabaseName);

                ConnectionStatus = "CONNECTED";

            }
            catch(Exception x)
            {
                MessageBox.Show(x.Message);
            }
        }

        // queries
        string qFirstNameContains = "";
        public string QFirstNameContains
        {
            get => qFirstNameContains;
            set => Set(ref qFirstNameContains, value);
        }

        int qAgeGreaterThan = 0;
        public int QAgeGreaterThan
        {
            get => qAgeGreaterThan;
            set => Set(ref qAgeGreaterThan, value);
        }

        public ObservableCollection<string> QueryResults { get; } = new ObservableCollection<string>();

        public async Task QueryAsync()
        {
            try
            {                
                QueryResults.Clear();

                var contacts = new List<Contact>();                

                await foreach (var r in data.GetContactsFirstNameContainsAndAgeGreaterAsync(QFirstNameContains, QAgeGreaterThan))
                    contacts.Add(r);
                
                foreach (var c in contacts)
                {
                    var phones = new List<Phone>();
                    await foreach (var r in data.GetContactPhonesAsync(c.Id))
                        phones.Add(r);

                    var sb = new StringBuilder();
                    var contactAndPhones = $"Contact: {JsonConvert.SerializeObject(c)}\n";
                    sb.Append(contactAndPhones);
                    foreach (var p in phones)
                        sb.Append($"  Phone: {JsonConvert.SerializeObject(p)}\n");

                    QueryResults.Add(sb.ToString());
                }
            }
            catch(Exception x)
            {
                MessageBox.Show(x.Message);
            }
        }
    }
}
