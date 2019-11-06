using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rip.Net;

namespace RipAdmin
{
    public class AppState
    {
        List<RipClient> ripClients = new List<RipClient>();

        private AppState()
        {            
            Instance = this;
        }

        public static void CreateInstance()
        {
            if (Instance != null)
                throw new Exception("AppState already has instance.");

            Instance = new AppState();
        }

        public static AppState Instance { get; private set; }

        public async Task<RipClient> ConnectRipConnectionAsync(string host, int port)
        {
            var rc = new Rip.Net.RipClient(host, port);
            await rc.ConnectAsync();
            ripClients.Add(rc);
            return rc;
        }

        public void DisconnectAsync(RipClient rc)
        {
            rc.Disconnect();
            ripClients.Remove(rc);
        }
        
    }
}
