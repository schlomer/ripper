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


## Instructions
1. Compile a Release build of the solution
2. In the x64\Release directory, run rips.exe from the command line and leave running.  To change the listening port number (defaults to 1234) before starting **rips.exe**, edit **rip-config.json** and set the tcpPort.  Upon starting **rips.exe**, Windows may warn about the port and you may need to let rips.exe through the Windows firewall. **If you change the port, you will need to use that port with the RipExample apps so they know where to send requests.**
3. In the RipExample.Wpf.Core\bin\Release\netcoreapp3.0 directory, run RipExample.Wpf.Core.exe
4. Click Connect (connecting for the first time will fill Contact and Phone containers with random data)
5. Enter Joh in the First Name Contains text box
6. Click Query and you should see some results with Contact FirstName starting with Joh and having ages greater than 0
... See RipExample.Lib.Core/ContactData.cs for the first and last name arrays used to randomly populate the Contacts container


**More information to come. In the meantime, check it out! Suggestions and improvements are always welcome.**

Thanks to the developers of JSON for Modern C++ and their excellent, header only, C++ JSON library: https://github.com/nlohmann/json