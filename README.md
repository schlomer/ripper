# Ripper
**In-memory partitioned JSON database, with a transaction log.**

Ripper is a simple to use database for JSON objects and supports partitioned, indexed, and filtered look-ups.
Layout is similar to commercial, large scale JSON databases you might know.

**DO NOT consider Ripper to be industrial/commercial grade as testing has been limited. USE AT YOUR OWN RISK.**

**General Hierarchy:** Host Machine Ripper service(s) contain Data Servers that contain Databases that contain Containers that contain JSON Objects

Records stored in containers are put in buckets by partition key. The combination of partition key and ID form the 'Primary Key' of the JSON object in a container.
In addition to fast lookups by partition key, filter objects can be created to post-filter records after retrieving from memory, but before sending over the wire.
(More documentation on filters to come.)

The included Ripper server (**rips**) keeps a transaction log of changes on disk. 
If transaction logs are detected upon initial server startup, they get played into memory. (In this version, that happens when a server receives its first request after startup.)

## VS 2019 Solution Projects
* **rips** - C++/WinRT command line server that listens on a configurable TCP port for incoming messages.
* **Rip.Core** - .NET Core 3.0 class library to easily make requests from a .NET Core app to the **rips** server.
* **RipExample.Lib.Core** - Example .NET Core 3.0 data access layer library using Contact and Phone as example business object models. Uses Rip.Core
* **RipExample.Con.Core** - Example .NET Core 3.0 console app using RipExample.Lib.Core
* **RipExample.Wpf.Core** - Example .NET Core 3.0 WPF app using RipExample.Lib.Core

Additional information about the **rips** server:
* configurable with the rip-config.json file that must be located in the **rips** startup directory
* currently a Windows 10 console app server that uses the C++/WinRT networking libraries. Should be easy enough to use the server classes directly embedded in other C++ code, wrapped in a Windows service, or converted to Linux.


## Build and Run Instructions
1. Compile an x64 Release build of the solution
2. In the x64\Release directory, run rips.exe from the command line and leave running.  To change the listening port number (defaults to 1234) before starting **rips.exe**, edit **rip-config.json** and set the tcpPort.  Upon starting **rips.exe**, Windows may warn about the port and you may need to let rips.exe through the Windows firewall. **If you change the port, you will need to use that port with the RipExample apps so they know where to send requests.**
3. In the RipExample.Wpf.Core\bin\Release\netcoreapp3.0 directory, run RipExample.Wpf.Core.exe
4. Click Connect (connecting for the first time will fill Contact and Phone containers with random data)
5. Enter Joh in the First Name Contains text box
6. Click Query and you should see some results with Contact FirstName starting with Joh and having ages greater than 0
... See RipExample.Lib.Core/ContactData.cs for the first and last name arrays used to randomly populate the Contacts container

## Container Partition Keys and ID Paths
When creating a container, a partition key path and ID path need to be provided. JSON Pointer notation is used. For the examples, Phone is partitioned by /ContactId and has /Id for the ID path. In this release, partition key and ID members must be strings. The partition key and ID combination must be unique within a container.

Example Phone JSON: 

```
{
  "Id": "b1358f046683473e8f783950a82f538a",
  "ContactId": "3d6fb736d4804bf5959c739a403721d8",
  "Type": "Work",
  "Number": "555-694-0317"
}

Partition Key Path: /ContactId

ID Path: /Id
```

## Indexes
Indexes on containers can speed up record retrieval and use JSON Pointer notation for partition key and ID paths. If one adds CarrierInfo to the example Phone JSON and wants to index on the carrier's name, /CarrierInfo/Name would be used as the index path. Members pointed to by an index path can be any type, although it probably makes the most sense to have index paths point to primitively typed members (i.e. bools, numbers, strings).

```
{
  "Id": "b1358f046683473e8f783950a82f538a",
  "ContactId": "3d6fb736d4804bf5959c739a403721d8",
  "Type": "Mobile",
  "Number": "555-696-0663"
  "CarrierInfo" : {
    "Name":"ATT"
  }
}

Indexing on CarrierInfo Name: /CarrierInfo/Name
```

## Filters
In this release, filters do not make use of indexes, but can be used post record retrieval to reduce traffic over the TCP connection. Getting records based on an index and applying a filter will help with both record retrieval performance and network traffic reduction. Filters can be applied to primitively typed members.

Filters are ultimately JSON objects, but the Rip.Core library has helper classes (RipFilterBuilder, RipFilterOp, RipFilterParameter, RipFilterValue, RipFilterCommand) to contruct the JSON filter objects. See RipExample.Lib.Core - ContactData.cs for examples on how to construct and use filters using these helper classes.

```
Filter operations supported:

and - And expression
or  - Or expression
=   - Equal (case sensitive)
<>  - Not Equal (case sensitive)
>   - Greater Than (case sensitive)
<   - Less Than (case sensitive)
>=  - Greater Than or Equal (case sensitive)
<=  - Less Than or Equal (case sensitive)
=*  - Starts With (case insensitive)
*=  - Ends With (case insensitive)
*=* - Contains (case insensitive)

Example filter JSON representing "Return records where FirstName contains 'Joh' and Age is greater than 30:

{"f":{"and":[{"*=*":{"/FirstName":"Joh"}},{">":{"/Age":30}}]}
```



**More information to come. In the meantime, check it out! Suggestions and improvements are always welcome.**

Thanks to the developers of JSON for Modern C++ and their excellent, header only, C++ JSON library: https://github.com/nlohmann/json
