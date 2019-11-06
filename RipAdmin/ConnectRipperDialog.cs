using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Rip.Net;

namespace RipAdmin
{
    public partial class ConnectRipperDialog : Form
    {
        public ConnectRipperDialog()
        {
            InitializeComponent();
        }

        public RipClient RipClient { get; set; }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private async void connectButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.None;
            var sHost = hostTextBox.Text;
            var sPort = portTextBox.Text;
            var sDataServer = dataServerTextBox.Text;

            if(string.IsNullOrWhiteSpace(sHost))
            {
                hostTextBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(sPort))
            {
                portTextBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(sDataServer))
            {
                dataServerTextBox.Focus();
                return;
            }
                        
            if (int.TryParse(sPort, out int port) == false || port < 0)
            {
                portTextBox.Focus();
                return;
            }

            try
            {
                RipClient = null;
                RipClient = await AppState.Instance.ConnectRipConnectionAsync(sHost, port);
                // need to connect to data server to force tlog loading on server instance
                await RipClient.ConnectOrCreateDataServerAsync(sDataServer);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch(Exception x)
            {
                MessageBox.Show(x.Message, "Connect Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
