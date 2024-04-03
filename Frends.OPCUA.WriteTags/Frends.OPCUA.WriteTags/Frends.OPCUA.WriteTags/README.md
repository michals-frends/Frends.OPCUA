# Frends.OPCUA.WriteTags
FRENDS Tasks for OPC UA Write Data operation

- [Installing](#installing)
- [Tasks](#tasks)
    - [WriteTags](#writetags)
- [License](#license)
- [Building](#building)
- [Contributing](#contributing)

Installing
==========

You can install the task via FRENDS UI Task view, by searching for packages.

Tasks
=====

## WriteTags

The OPCUA.WriteTags task is simple UPC AU Write Data operation.

Input:

| Property    | Type                                  | Description                             | Example                                                       |
|-------------|---------------------------------------|-----------------------------------------|---------------------------------------------------------------|
| Url         | string                                | The URL with protocol and path to call. | `opc.tcp://foo.example.org:62541/Quickstarts/ReferenceServer` |
| WriteValues | Array {NodeId: string, Value: object} | List of tags to be written.             | `NodeId = ns=2;s=some_scalar_int, Value = 42`                 |

Options:

| Property                       | Type                           | Description                                                                    |
|--------------------------------|--------------------------------|--------------------------------------------------------------------------------|
| Application Name               | string                         | Name of application to be sent with request                                    |
| Authentication                 | Enum(Anonymous, UserIdentity ) | Different options for authentication for the OPCUA request.                    |
| Username                       | string                         | This field is available for UserIdentity. Specify username required by server. |
| Password                       | string                         | This field is available for UserIdentity Authentication.                       |
| Trust Server Certificate       | bool                           | Should client accept any certificate used by server                            |
| Trusted Certificate Thumbprint | string                         | Thumbprint of trusted certificate used by server                               |

Result:

| Property         | Type                                                     | Description                      |
|------------------|----------------------------------------------------------|----------------------------------|
| IsAllSuccess     | bool                                                     | Are all write operations success |
| WriteValueResult | Array{NodeId: string, Value: object, ResultCode: string} | Status of each write operation   |
| Reason Code      | string                                                   | Message set in case of any error | 

License
=======
This project is licensed under the MIT License - see the LICENSE file for details

Building
========

Clone a copy of the repo

`git clone https://github.com/FrendsPlatform/Frends.OPCUA.git`

Restore dependencies

`dotnet restore`

Rebuild the project

`dotnet build`

Run tests

`dotnet test Frends.Web.Tests`

Create a nuget package

`dotnet pack Frends.Web`

Contributing
============
When contributing to this repository, please first discuss the change you wish to make via issue, email, or any other method with the owners of this repository before making a change.

1. Fork the repo on GitHub
2. Clone the project to your own machine
3. Commit changes to your own branch
4. Push your work back up to your fork
5. Submit a Pull request so that we can review your changes

NOTE: Be sure to merge the latest from "upstream" before making a pull request!