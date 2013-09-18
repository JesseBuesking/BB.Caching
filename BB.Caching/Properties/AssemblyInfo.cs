using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.

[assembly: AssemblyTitle("BB.Caching")]
[assembly: AssemblyDescription("Simple caching for .NET applications")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("BB.Caching")]
[assembly: AssemblyCopyright("Copyright ©  2013")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
#if (DEBUG || TEST)
// Generates the PublicKey
// C:\Program Files (x86)\Microsoft Visual Studio 11.0\VC>sn -Tp C:\Repositories\BB
// \BB.Caching\BB.Caching.Tests\bin\Debug\BB.Caching.Tests.dll

[assembly: InternalsVisibleTo("BB.Caching.Tests, PublicKey=" +
    "0024000004800000940000000602000000240000525341310004000001000100a71c48bf8c5e13" +
    "38468d891c5dd3f7e97a2fbe77fdc621c360278316f25ea72515e970ac7264ec7e8c0423aec151" +
    "6e228aa044a2ef0700e01625d4b58df994301c6bb0d81a389cad2987a4eff722f6de17e2d19383" +
    "ec36c9c97c07435778cd5e249fc439fc651ce46e1b701a74d5090d3684b23ca81e07b012e4ca4c" +
    "89167dcc"
    )]
#endif

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.

[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM

[assembly: Guid("ffa0c3e2-9e8a-4869-94fe-5727de90e50c")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]

[assembly: AssemblyVersion("0.0.0.1")]
[assembly: AssemblyFileVersion("0.0.0.1")]