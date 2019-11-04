# Ripper
In-memory partitioned JSON database, with a transaction log.

Ripper is a simple to use database for JSON objects and supports partitioned, indexed, and filtered look-ups.
Layout is similar to commercial, large scale JSON databases you might know.

**DO NOT consider Ripper to be industrial grade as testing has been limited. USE AT YOUR OWN RISK.**

Host -> Data Servers -> Databases -> Containers -> JSON Objects

Records stored in containers are put in buckets by partition key. The combination of partition key and ID form the 'Primary Key' of the JSON object in a container.
In addition to fast lookups by partition key, filter objects can be created to post-filter records after retrieving from memory, but before sending over the wire.
(More documentation on filters to come.)

The included Ripper server (**rips**) keeps a transaction log of changes on disk. 
If transaction logs are detected upon initial server startup, they get played into memory. (In this version, that happens when a server receives its first request after startup.)

## Projects
* **rips** - C++/WinRT command line server that listens on a configurable TCP port for incoming messages.
* **Rip.Core** - .NET Core 3.0 class library to easily make requests from a .NET Core app to the **rips** server.
* **RipExample.Lib.Core** - Example .NET Core 3.0 data access layer library using Contact and Phone as example business object models. Uses Rip.Core
* **RipExample.Con.Core** - Example .NET Core 3.0 console app using RipExample.Lib.Core
* **RipExample.Wpf.Core** - Example .NET Core 3.0 WPF app using RipExample.Lib.Core

Additional information about the **rips** server:
* configurable with the rip-config.json file that must be located in the **rips** startup directory
* currently a Windows 10 console app server that uses the C++/WinRT networking libraries. Should be easy enough to use the server classes directly embedded in other C++ code, wrapped in a Windows service, or converted to Linux.


**More information to come. In the meantime, check it out!**
