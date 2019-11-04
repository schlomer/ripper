using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Rip.Core
{
    public class RipException : Exception
    {
        public RipException(string message) : base(message) { }
    }

    public class RipContainer
    {
        RipDatabase database;
        string name;
        string partitionKeyPath;
        string idPath;

        internal RipContainer(RipDatabase database, string name, string partitionKeyPath, string idPath)
        {
            this.database = database;
            this.partitionKeyPath = partitionKeyPath;
            this.idPath = idPath;
            this.name = name;
        }

        public string Name { get => name; }
        public RipDatabase Database { get => database; }
        public string ParititionKeyPath { get => partitionKeyPath; }
        public string IdPath { get => idPath; }

        public async Task SetRecordAsync(string json)
        {
            var response = await database.DataServer.Client.RemoteSetRecordAsync(database.DataServer.Name, database.Name, Name, json);
            database.DataServer.Client.ThrowIfError(response);
        }

        public async Task SetRecordAsync<T>(T value)
        {
            await SetRecordAsync(JsonConvert.SerializeObject(value));
        }

        public async Task<string> GetRecordAsync(string partitionKey, string id)
        {            
            var response = await database.DataServer.Client.RemoteGetRecordAsync(database.DataServer.Name, database.Name, Name, partitionKey, id);
            database.DataServer.Client.ThrowIfError(response);

            return response.ResponseJson;
        }

        public async IAsyncEnumerable<string> GetRecordsAsync(string filterJson)
        {
            await foreach (var r in database.DataServer.Client.RemoteGetRecordsAsync(database.DataServer.Name, database.Name, Name,
                null, null, null, null, filterJson))
                yield return r;
        }

        public async IAsyncEnumerable<T> GetRecordsAsync<T>(string filterJson = null)
        {
            await foreach (var r in GetRecordsAsync(filterJson))
                yield return JsonConvert.DeserializeObject<T>(r);
        }

        public async IAsyncEnumerable<string> GetRecordsAsync(string partitionKey, string filterJson)
        {
            await foreach (var r in database.DataServer.Client.RemoteGetRecordsAsync(database.DataServer.Name, database.Name, Name,
                partitionKey, null, null, null, filterJson))
                yield return r;
        }

        public async IAsyncEnumerable<T> GetRecordsAsync<T>(string partitionKey, string filterJson = null)
        {
            await foreach (var r in GetRecordsAsync(partitionKey, filterJson))
                yield return JsonConvert.DeserializeObject<T>(r);
        }

        public async IAsyncEnumerable<string> GetRecordsAsync(string indexPath, object beginValue, object endValue, string filterJson)
        {
            await foreach (var r in database.DataServer.Client.RemoteGetRecordsAsync(database.DataServer.Name, database.Name, Name,
                null, indexPath, beginValue, endValue, filterJson))
                yield return r;
        }

        public async IAsyncEnumerable<T> GetRecordsAsync<T>(string indexPath, object beginValue, object endValue, string filterJson = null)
        {
            await foreach (var r in GetRecordsAsync(indexPath, beginValue, endValue, filterJson))
                yield return JsonConvert.DeserializeObject<T>(r);
        }
        public async IAsyncEnumerable<string> GetRecordsAsync(string partitionKey, string indexPath, object beginValue, object endValue,
            string filterJson = null)
        {
            await foreach (var r in database.DataServer.Client.RemoteGetRecordsAsync(database.DataServer.Name, database.Name, Name,
                partitionKey, indexPath, beginValue, endValue, filterJson))
                yield return r;
        }

        public async IAsyncEnumerable<T> GetRecordsAsync<T>(string partitionKey, string indexPath, object beginValue, object endValue,
            string filterJson = null)
        {
            await foreach (var r in GetRecordsAsync(partitionKey, indexPath, beginValue, endValue, filterJson))
                yield return JsonConvert.DeserializeObject<T>(r);
        }


        public async Task<T> GetRecordAsync<T>(string partitionKey, string id)
        {
            var json = await GetRecordAsync(partitionKey, id);
            if (string.IsNullOrWhiteSpace(json))
                return default;

            return JsonConvert.DeserializeObject<T>(json);
        }

        public async Task DeleteRecordsAsync(string partitionKey = null, string id = null)
        {
            var response = await database.DataServer.Client.RemoteDeleteRecordsAsync(database.DataServer.Name, database.Name, Name, partitionKey, id);
            database.DataServer.Client.ThrowIfError(response);            
        }

        public async Task AddIndexAsync(string indexPath)
        {
            var response = await database.DataServer.Client.RemoteAddIndexAsync(database.DataServer.Name, database.Name, Name, indexPath);
            database.DataServer.Client.ThrowIfError(response);
        }
    }

    public class RipDatabase
    {
        RipDataServer dataServer;
        string name;

        internal RipDatabase(RipDataServer dataServer, string name)
        {
            this.dataServer = dataServer;
            this.name = name;
        }
        public RipDataServer DataServer { get => dataServer; }
        public string Name { get => name; }

        public async Task<RipContainer> AddContainerAsync(string containerName, string partitionKeyPath, string idPath)
        {
            var response = await dataServer.Client.RemoteAddContainerAsync(dataServer.Name, Name, containerName, partitionKeyPath, idPath);
            dataServer.Client.ThrowIfError(response);

            return new RipContainer(this, containerName, partitionKeyPath, idPath);
        }

        public async Task<RipContainer> GetContainerAsync(string containerName)
        {
            var response = await dataServer.Client.RemoteGetContainerAsync(dataServer.Name, Name, containerName);
            if (response == null)
                throw new RipException("No response.");
            if (string.IsNullOrWhiteSpace(response.Error) == false)
                return null;

            return new RipContainer(this, containerName, response.PartitionKeyPath, response.IdPath);
        }
    }

    public class RipDataServer
    {
        RipClient client;
        string name;
        internal RipDataServer(RipClient client, string name)
        {
            this.client = client;
            this.name = name;
        }

        public string Name { get => name; }
        internal RipClient Client { get => client; }
        public async Task<RipDatabase> AddDatabaseAsync(string databaseName)
        {
            var response = await client.RemoteAddDatabaseAsync(name, databaseName);
            client.ThrowIfError(response);

            return new RipDatabase(this, databaseName);
        }

        public async Task<RipDatabase> GetDatabaseAsync(string databaseName)
        {
            var response = await client.RemoteGetDatabaseAsync(Name, databaseName);
            if (response == null)
                throw new RipException("No response.");
            if (string.IsNullOrWhiteSpace(response.Error) == false)
                return null;

            return new RipDatabase(this, databaseName);
        }
    }

    public class RipClient : IDisposable
    {
        int port;
        string server;
        TcpClient tcp;        
        public RipClient(string server, int port)
        {
            this.server = server;
            this.port = port;
        }

        public string Server { get => server; }
        public int Port { get => port; }

        public async Task ConnectAsync()
        {
            tcp = new TcpClient();
            await tcp.ConnectAsync(server, port);            
        }

        public void Disconnect()
        {
            if (tcp != null)
            {
                tcp.Close();
                tcp.Dispose();
                tcp = null;
            }
        }

        async Task SendRequestAsync(Messages.Request request, string filterJson)
        {
            var requestJson = JsonConvert.SerializeObject(request, Formatting.None);

            if(string.IsNullOrWhiteSpace(filterJson) == false)
            {
                requestJson = requestJson.Substring(0, requestJson.Length - 1);
                requestJson = string.Concat(requestJson, ",\"q\":", filterJson, "}}");
            }

            var requestBytes = Encoding.UTF8.GetBytes(requestJson);

            var lengthBytes = BitConverter.GetBytes((uint)requestBytes.Length);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(lengthBytes);
            var stream = tcp.GetStream();
            await stream.WriteAsync(lengthBytes);
            await stream.WriteAsync(requestBytes);
            await stream.FlushAsync();            
        }

        async Task<T> GetResponseAsync<T>()
        {
            var lengthBytes = new byte[4];

            var stream = tcp.GetStream();

            await stream.ReadAsync(lengthBytes, 0, lengthBytes.Length);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(lengthBytes);
            var length = BitConverter.ToUInt32(lengthBytes);

            if(length > 0)
            {
                var responseBytes = new byte[length];
                await stream.ReadAsync(responseBytes, 0, responseBytes.Length);

                var responseJson = Encoding.UTF8.GetString(responseBytes);
                if (string.IsNullOrWhiteSpace(responseJson))
                    return default;

                return JsonConvert.DeserializeObject<T>(responseJson);
            }

            return default;
        }

        async IAsyncEnumerable<string> GetStreamedResponseAsync()
        {
            var lengthBytes = new byte[4];

            var stream = tcp.GetStream();
            
            while(true)
            { 
                await stream.ReadAsync(lengthBytes, 0, lengthBytes.Length);
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(lengthBytes);
                var length = BitConverter.ToUInt32(lengthBytes);
                if (length == 0)
                    break;
                
                var responseBytes = new byte[length];
                await stream.ReadAsync(responseBytes, 0, responseBytes.Length);

                var responseJson = Encoding.UTF8.GetString(responseBytes);
                if (string.IsNullOrWhiteSpace(responseJson))
                    yield return null;

                yield return responseJson;
            }            
        }

        internal void ThrowIfError(Messages.Response response)
        {
            if (response == null)
                throw new RipException("No response.");
            if (string.IsNullOrWhiteSpace(response.Error) == false)
                throw new RipException(response.Error);
        }

        public async Task<RipDataServer> ConnectOrCreateDataServerAsync(string dataServerName)
        {
            var server = await GetDataServerAsync(dataServerName);
            if (server == null)
                server = await AddDataServerAsync(dataServerName);
            return server;

        }

        public async Task<RipDataServer> AddDataServerAsync(string dataServerName)
        {
            var response = await RemoteAddDataServerAsync(dataServerName);
            ThrowIfError(response);

            return new RipDataServer(this, dataServerName);
        }

        public async Task<RipDataServer> GetDataServerAsync(string dataServerName)
        {
            var response = await RemoteGetDataServerAsync(dataServerName);
            if (response == null)
                throw new RipException("No response.");
            if (string.IsNullOrWhiteSpace(response.Error) == false)
                return null;

            return new RipDataServer(this, dataServerName);
        }

        internal async Task<Messages.AddDataServerResponse> RemoteAddDataServerAsync(string dataServerName)
        {
            var request = new Messages.AddDataServerRequest(dataServerName);
            await SendRequestAsync(request, null);
            return await GetResponseAsync<Messages.AddDataServerResponse>();
        }
        internal async Task<Messages.GetDataServerResponse> RemoteGetDataServerAsync(string dataServerName)
        {
            var request = new Messages.GetDataServerRequest(dataServerName);
            await SendRequestAsync(request, null);
            return await GetResponseAsync<Messages.GetDataServerResponse>();
        }

        internal async Task<Messages.AddDatabaseResponse> RemoteAddDatabaseAsync(string dataServerName, string databaseName)
        {
            var request = new Messages.AddDatabaseRequest(dataServerName, databaseName);                                    
            await SendRequestAsync(request, null);
            return await GetResponseAsync<Messages.AddDatabaseResponse>();
        }

        internal async Task<Messages.GetDatabaseResponse> RemoteGetDatabaseAsync(string dataServerName, string databaseName)
        {
            var request = new Messages.GetDatabaseRequest(dataServerName, databaseName);
            await SendRequestAsync(request, null);
            return await GetResponseAsync<Messages.GetDatabaseResponse>();
        }

        internal async Task<Messages.AddContainerResponse> RemoteAddContainerAsync(string dataServerName, string databaseName
            , string containerName, string partitionKeyPath, string idPath)
        {
            var request = new Messages.AddContainerRequest(dataServerName, databaseName, containerName
                , partitionKeyPath, idPath);
            await SendRequestAsync(request, null);
            return await GetResponseAsync<Messages.AddContainerResponse>();
        }

        internal async Task<Messages.GetContainerResponse> RemoteGetContainerAsync(string dataServerName, string databaseName, string containerName)
        {
            var request = new Messages.GetContainerRequest(dataServerName, databaseName, containerName);
            await SendRequestAsync(request, null);
            return await GetResponseAsync<Messages.GetContainerResponse>();
        }

        internal async Task<Messages.SetRecordResponse> RemoteSetRecordAsync(string dataServerName, string databaseName
           , string containerName, string recordJson)
        {
            var request = new Messages.SetRecordRequest(dataServerName, databaseName, containerName
                , recordJson);
            await SendRequestAsync(request, null);
            return await GetResponseAsync<Messages.SetRecordResponse>();
        }

        internal async Task<Messages.GetRecordResponse> RemoteGetRecordAsync(string dataServerName, string databaseName
          , string containerName, string partitionKey, string id, string filterJson = null)
        {
            var request = new Messages.GetRecordRequest(dataServerName, databaseName, containerName
                , partitionKey, id);
            await SendRequestAsync(request, filterJson);
            return await GetResponseAsync<Messages.GetRecordResponse>();
        }

        internal async Task<Messages.DeleteRecordsResponse> RemoteDeleteRecordsAsync(string dataServerName, string databaseName
         , string containerName, string partitionKey, string id, string filterJson = null)
        {
            var request = new Messages.DeleteRecordsRequest(dataServerName, databaseName, containerName
                , partitionKey, id);
            await SendRequestAsync(request, filterJson);
            return await GetResponseAsync<Messages.DeleteRecordsResponse>();
        }

        internal async Task<Messages.AddIndexResponse> RemoteAddIndexAsync(string dataServerName, string databaseName
        , string containerName, string indexPath, string filterJson = null)
        {
            var request = new Messages.AddIndexRequest(dataServerName, databaseName, containerName, indexPath);
            await SendRequestAsync(request, filterJson);
            return await GetResponseAsync<Messages.AddIndexResponse>();
        }

        internal async IAsyncEnumerable<string> RemoteGetRecordsAsync(string dataServerName, string databaseName
        , string containerName, string partitionKey, string indexPath, object beginValue, object endValue, string filterJson = null)
        {
            var request = new Messages.GetRecordsRequest(dataServerName, databaseName, containerName
                , partitionKey, indexPath, beginValue, endValue);
            await SendRequestAsync(request, filterJson);
            await foreach (var r in GetStreamedResponseAsync())
                yield return r;
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Disconnect();
                }

                disposedValue = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
