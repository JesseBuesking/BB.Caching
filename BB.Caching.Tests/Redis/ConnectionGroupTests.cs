namespace BB.Caching.Tests.Redis
{
    using System;
    using System.Configuration;
    using System.IO;
    using System.Linq;

    using BB.Caching.Redis;

    using Xunit;

    public class ConnectionGroupTests
    {
        public class Programmatic
        {
            [Fact]
            public void WriteOnly()
            {
                var connectionGroup = new ConnectionGroup("node-0");
                var connectionString = string.Format("{0}:{1},allowAdmin=True", "12.34.56.78", "1234");

                connectionGroup.AddWriteConnection(connectionString, false);

                Assert.Equal("node-0", connectionGroup.Name);
                Assert.Equal(0, connectionGroup.ReadConnections.Count);

                Assert.Equal(connectionString, connectionGroup.WriteConnection);
            }

            [Fact]
            public void ReadOnly()
            {
                var connectionGroup = new ConnectionGroup("node-0");
                var connectionString = string.Format("{0}:{1},allowAdmin=True", "12.34.56.78", "1234");

                connectionGroup.AddReadConnection(connectionString, false);

                Assert.Equal("node-0", connectionGroup.Name);
                Assert.Equal(null, connectionGroup.WriteConnection);
                Assert.Equal(1, connectionGroup.ReadConnections.Count);

                Assert.Equal(connectionString, connectionGroup.ReadConnections[0]);
            }

            [Fact]
            public void Both()
            {
                var connectionGroup = new ConnectionGroup("node-0");
                var connectionString = string.Format("{0}:{1},allowAdmin=True", "12.34.56.78", "1234");

                connectionGroup.AddReadConnection(connectionString, false);
                connectionGroup.AddWriteConnection(connectionString, false);

                Assert.Equal("node-0", connectionGroup.Name);
                Assert.Equal(1, connectionGroup.ReadConnections.Count);

                Assert.Equal(connectionString, connectionGroup.ReadConnections[0]);
                Assert.Equal(connectionString, connectionGroup.WriteConnection);
            }
        }

        public class ConfigFile
        {
            [Fact]
            public void WriteOnly()
            {
                var filename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "writeonly.config");
                var fileMap = new ConfigurationFileMap(filename);
                var configuration = ConfigurationManager.OpenMappedMachineConfiguration(fileMap);
                var section = configuration.Sections.Get("BB.Caching");
                var connectionGroups = Cache.LoadFromConfig(section, false);

                var connectionString = string.Format("{0}:{1},allowAdmin=True", "12.34.56.78", "1234");

                Assert.Equal(1, connectionGroups.Count);

                var connectionGroup = connectionGroups.First();

                Assert.Equal("node-0", connectionGroup.Name);
                Assert.Equal(0, connectionGroup.ReadConnections.Count);

                Assert.Equal(connectionString, connectionGroup.WriteConnection);
            }

            [Fact]
            public void ReadOnly()
            {
                var filename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "readonly.config");
                var fileMap = new ConfigurationFileMap(filename);
                var configuration = ConfigurationManager.OpenMappedMachineConfiguration(fileMap);
                var section = configuration.Sections.Get("BB.Caching");
                var connectionGroups = Cache.LoadFromConfig(section, false);

                var connectionString = string.Format("{0}:{1},allowAdmin=True", "12.34.56.78", "1234");

                Assert.Equal(1, connectionGroups.Count);

                var connectionGroup = connectionGroups.First();

                Assert.Equal("node-0", connectionGroup.Name);
                Assert.Equal(null, connectionGroup.WriteConnection);
                Assert.Equal(1, connectionGroup.ReadConnections.Count);

                Assert.Equal(connectionString, connectionGroup.ReadConnections[0]);
            }

            [Fact]
            public void Both()
            {
                var filename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "readandwrite.config");
                var fileMap = new ConfigurationFileMap(filename);
                var configuration = ConfigurationManager.OpenMappedMachineConfiguration(fileMap);
                var section = configuration.Sections.Get("BB.Caching");
                var connectionGroups = Cache.LoadFromConfig(section, false);

                var connectionString = string.Format("{0}:{1},allowAdmin=True", "12.34.56.78", "1234");

                Assert.Equal(1, connectionGroups.Count);

                var connectionGroup = connectionGroups.First();

                Assert.Equal("node-0", connectionGroup.Name);
                Assert.Equal(1, connectionGroup.ReadConnections.Count);

                Assert.Equal(connectionString, connectionGroup.ReadConnections[0]);
                Assert.Equal(connectionString, connectionGroup.WriteConnection);
            }
        }
    }
}
