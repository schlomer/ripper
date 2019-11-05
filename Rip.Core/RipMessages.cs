using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rip.Core
{
    public class RipDataServerDescription
    {
        [JsonProperty("sn")]
        public string Name { get; set; }
    }

    public class RipDatabaseDescription
    {        
        [JsonProperty("dn")]
        public string Name { get; set; }
    }

    public class RipContainerDescription
    {
        [JsonProperty("cn")]
        public string Name { get; set; }

        [JsonProperty("pkp")]
        public string PartitionKeyPath { get; set; }

        [JsonProperty("idp")]
        public string IdPath { get; set; }

        [JsonProperty("ixp")]
        public string[] IndexPaths { get; set; }
    }
}

namespace Rip.Core.Messages
{
    public enum RequestTypes
    {
        AddDataServer = 1,
        AddDatabase = 2,
        AddContainer = 3,
        SetRecord = 4,
        DeleteRecords = 5,
        AddIndex = 6,
        GetRecord = 7,
        GetRecords = 8,
        GetDataServer = 9,
        GetDatabase = 10,
        GetContainer = 11,
        ListDataServers = 12,
        ListDatabases = 13,
        ListContainers = 14
    }

    public class Request
    {
        protected Request(RequestTypes requestType)
        {
            RequestType = (int)requestType;
        }

        [JsonRequired]
        [JsonProperty("rt")]
        public int RequestType { get; set; }
    }

    public class Response
    {
        [JsonProperty("e")]
        public string Error { get; set; }
    }

    public class ContainerRequest : Request
    {
        protected ContainerRequest(RequestTypes requestType, string dataServerName, string databaseName, string containerName) : base(requestType)
        {
            DataServerName = dataServerName;
            DatabaseName = databaseName;
            ContainerName = containerName;            
        }

        [JsonRequired]
        [JsonProperty("sn")]
        public string DataServerName { get; set; }

        [JsonRequired]
        [JsonProperty("dn")]
        public string DatabaseName { get; set; }

        [JsonRequired]
        [JsonProperty("cn")]
        public string ContainerName { get; set; }
    }

    // AddDataServer
    public class AddDataServerRequest : Request
    {
        public AddDataServerRequest(string dataServerName) : base(RequestTypes.AddDataServer)
        {
            DataServerName = dataServerName;            
        }

        [JsonRequired]
        [JsonProperty("sn")]
        public string DataServerName { get; set; }        
    }

    public class AddDataServerResponse : Response
    {
    }

    // GetDataServer
    public class GetDataServerRequest : Request
    {
        public GetDataServerRequest(string dataServerName) : base(RequestTypes.GetDataServer)
        {
            DataServerName = dataServerName;
        }

        [JsonRequired]
        [JsonProperty("sn")]
        public string DataServerName { get; set; }
    }

    public class GetDataServerResponse : Response
    {
    }

    // AddDatabase
    public class AddDatabaseRequest : Request
    {
        public AddDatabaseRequest(string dataServerName, string databaseName) : base(RequestTypes.AddDatabase)
        {
            DataServerName = dataServerName;
            DatabaseName = databaseName;
        }

        [JsonRequired]
        [JsonProperty("sn")]
        public string DataServerName { get; set; }

        [JsonRequired]
        [JsonProperty("dn")]
        public string DatabaseName { get; set; }   
    }

    public class AddDatabaseResponse : Response
    {
    }

    // GetDatabase
    public class GetDatabaseRequest : Request
    {
        public GetDatabaseRequest(string dataServerName, string databaseName) : base(RequestTypes.GetDatabase)
        {
            DataServerName = dataServerName;
            DatabaseName = databaseName;
        }

        [JsonRequired]
        [JsonProperty("sn")]
        public string DataServerName { get; set; }

        [JsonRequired]
        [JsonProperty("dn")]
        public string DatabaseName { get; set; }
    }

    public class GetDatabaseResponse : Response
    {
    }

    // AddContainer
    public class AddContainerRequest : ContainerRequest
    {
        public AddContainerRequest(string dataServerName, string databaseName, string containerName,
            string partitionKeyPath, string idPath) : base(RequestTypes.AddContainer, dataServerName, databaseName, containerName)
        {         
            PartitionKeyPath = partitionKeyPath;
            IdPath = idPath;
        }     

        [JsonRequired]
        [JsonProperty("pkp")]
        public string PartitionKeyPath { get; set; }

        [JsonRequired]
        [JsonProperty("idp")]
        public string IdPath { get; set; }
    }

    public class AddContainerResponse : Response
    {
    }

    // GetContainer
    public class GetContainerRequest : ContainerRequest
    {
        public GetContainerRequest(string dataServerName, string databaseName, string containerName) 
            : base(RequestTypes.GetContainer, dataServerName, databaseName, containerName)
        {          
        }        
    }

    public class GetContainerResponse : Response
    {        
        [JsonProperty("pkp")]
        public string PartitionKeyPath { get; set; }

        [JsonProperty("idp")]
        public string IdPath { get; set; }
    }

    // SetRecord
    public class SetRecordRequest : ContainerRequest
    {
        public SetRecordRequest(string dataServerName, string databaseName, string containerName,
            string recordJson) : base(RequestTypes.SetRecord, dataServerName, databaseName, containerName)
        {            
            RecordJson = recordJson;
        }        

        [JsonRequired]
        [JsonProperty("j")]
        public string RecordJson { get; set; }
    }

    public class SetRecordResponse : Response
    {
    }

    // DeleteRecords
    public class DeleteRecordsRequest : ContainerRequest
    {
        public DeleteRecordsRequest(string dataServerName, string databaseName, string containerName,
            string partitionKey, string id) : base(RequestTypes.DeleteRecords, dataServerName, databaseName, containerName)
        {
            PartitionKey = partitionKey;
            Id = id;
        }

        [JsonProperty("pk")]
        public string PartitionKey { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class DeleteRecordsResponse : Response
    {
    }

    // AddIndex
    public class AddIndexRequest : ContainerRequest
    {
        public AddIndexRequest(string dataServerName, string databaseName, string containerName,
            string indexPath) : base(RequestTypes.AddIndex, dataServerName, databaseName, containerName)
        {
            IndexPath = indexPath;
        }

        [JsonRequired]
        [JsonProperty("ixp")]
        public string IndexPath { get; set; }
    }

    public class AddIndexResponse : Response
    {
    }

    // GetRecord
    public class GetRecordRequest : ContainerRequest
    {
        public GetRecordRequest(string dataServerName, string databaseName, string containerName,
            string partitionKey, string id) : base(RequestTypes.GetRecord, dataServerName, databaseName, containerName)
        {            
            PartitionKey = partitionKey;
            Id = id;
        }      

        [JsonRequired]
        [JsonProperty("pk")]
        public string PartitionKey { get; set; }

        [JsonRequired]
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class GetRecordResponse : Response
    {
        [JsonProperty("j")]
        public string ResponseJson { get; set; }
    }

    public class GetRecordsRequest : ContainerRequest
    {
        public GetRecordsRequest(string dataServerName, string databaseName, string containerName,
            string partitionKey, string indexPath, object beginValue, object endValue) : base(RequestTypes.GetRecords, dataServerName, databaseName, containerName)
        {
            PartitionKey = partitionKey;
            IndexPath = indexPath;
            BeginValue = beginValue;
            EndValue = endValue;
        }
                
        [JsonProperty("pk")]
        public string PartitionKey { get; set; }
                
        [JsonProperty("ixp")]
        public string IndexPath { get; set; }

        [JsonProperty("bv")]
        public object BeginValue { get; set; }

        [JsonProperty("ev")]
        public object EndValue { get; set; }
    }

    public class GetRecordsResponse : Response
    {
        [JsonProperty("j")]
        public string[] ResponseJsons { get; set; }
    }  

    // ListDataServers
    public class ListDataServersRequest : Request
    {
        public ListDataServersRequest() : base(RequestTypes.ListDataServers)
        {            
        }        
    }

    public class ListDataServersResponse : Response
    {
        [JsonProperty("dsd")]
        public RipDataServerDescription[] DataServers;
    }

    // ListDatabases
    public class ListDatabasesRequest : Request
    {
        public ListDatabasesRequest(string dataServerName) : base(RequestTypes.ListDatabases)
        {
            DataServerName = dataServerName;
        }

        [JsonRequired]
        [JsonProperty("sn")]
        public string DataServerName { get; set; }
    }

    public class ListDatabasesResponse : Response
    {
        [JsonProperty("sn")]
        public string DataServerName { get; set; }

        [JsonProperty("dd")]
        public RipDatabaseDescription[] Databases { get; set; }
    }

    // ListContainers
    public class ListContainersRequest : Request
    {
        public ListContainersRequest(string dataServerName, string databaseName) : base(RequestTypes.ListContainers)
        {
            DataServerName = dataServerName;
            DatabaseName = databaseName;
        }

        [JsonRequired]
        [JsonProperty("sn")]
        public string DataServerName { get; set; }

        [JsonRequired]
        [JsonProperty("dn")]
        public string DatabaseName { get; set; }
    }

    public class ListContainersResponse : Response
    {
        [JsonProperty("sn")]
        public string DataServerName { get; set; }

        [JsonProperty("dn")]
        public string DatabaseName { get; set; }

        [JsonProperty("cd")]
        public RipContainerDescription[] Containers { get; set; }
    }
}
