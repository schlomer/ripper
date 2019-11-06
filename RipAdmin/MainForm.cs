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
    public partial class MainForm : Form
    {
        public MainForm()
        {
            AppState.CreateInstance();

            InitializeComponent();            
        }

        private void miActionExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private async void miActionConnectRipper_Click(object sender, EventArgs e)
        {
            var d = new ConnectRipperDialog();
            var dr = d.ShowDialog();
            if(dr == DialogResult.OK && d.RipClient != null)
            {
                var rip = d.RipClient;
                var tvRip = new TreeNode($"{rip.Server} : {rip.Port}", 0, 0);

                tvConnections.Nodes.Add(tvRip);
                                
                foreach (var dsd in (await rip.ListDataServersAsync()).OrderBy(x => x.Name))
                {
                    var tvDs = new TreeNode(dsd.Name, 1, 1);
                    var ds = await rip.GetDataServerAsync(dsd.Name);
                    tvDs.Tag = ds;
                    tvRip.Nodes.Add(tvDs);

                    foreach(var dbd in (await rip.ListDatabasesAsync(ds.Name)).OrderBy(x => x.Name))
                    {
                        var tvD = new TreeNode(dbd.Name, 2, 2);
                        var db = await ds.GetDatabaseAsync(dbd.Name);
                        tvD.Tag = db;
                        tvDs.Nodes.Add(tvD);

                        foreach(var cd in (await rip.ListContainersAsync(ds.Name, db.Name)).OrderBy(x => x.Name))
                        {
                            var tvC = new TreeNode(cd.Name, 3, 3);
                            var c = await db.GetContainerAsync(cd.Name);
                            tvC.Tag = c;
                            tvD.Nodes.Add(tvC);
                            

                            var tvPK = new TreeNode(cd.PartitionKeyPath, 4, 4);
                            tvC.Nodes.Add(tvPK);
                            var tvId = new TreeNode(cd.IdPath, 5, 5);
                            tvC.Nodes.Add(tvId);

                            if (cd.IndexPaths != null)
                            {
                                foreach (var i in cd.IndexPaths)
                                {
                                    var tvI = new TreeNode(i, 6, 6);
                                    tvC.Nodes.Add(tvI);
                                }
                            }
                        }
                    }
                }
            }
        }

        private async void miRecordsGetRecords_Click(object sender, EventArgs e)
        {
            var tv = tvConnections.SelectedNode;
            if (tv == null)
                return;

            var c = tv.Tag as RipContainer;
            if (c == null)
                return;

            recordsTextBox.Clear();


            var lines = new List<string>();

            await foreach(var r in c.GetRecordsAsync(null))
            {
                lines.Add(r);
            }

            recordsTextBox.Lines = lines.ToArray();
        }

        private void cmRecords_Opening(object sender, CancelEventArgs e)
        {
            var tv = tvConnections.SelectedNode;
            if (tv == null)
            {
                e.Cancel = true;
                return;
            }

            var c = tv.Tag as RipContainer;
            if (c == null)
            {
                e.Cancel = true;
                return;
            }
        }

        private void tvConnections_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right) tvConnections.SelectedNode = e.Node;
        }
    }
}
