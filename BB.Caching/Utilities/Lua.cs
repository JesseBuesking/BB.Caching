using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace BB.Caching.Utilities
{
    public class Lua
    {
        private static readonly Lazy<Lua> _lazy = new Lazy<Lua>(
            () => new Lua(), LazyThreadSafetyMode.ExecutionAndPublication);

        public static Lua Instance
        {
            get { return _lazy.Value; }
        }

        private readonly Dictionary<string, string> _cache = new Dictionary<string, string>();

        public string this[string key]
        {
            get
            {
                string result;
                if (this._cache.TryGetValue(key, out result))
                    return result;

                this._cache[key] = Lua.LuaFileToString(key);
                return this._cache[key];
            }
        }

        private Lua()
        {
        }

        /// <summary>
        /// Returns the lua file's contents as a string.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private static string LuaFileToString(string fileName)
        {
            var assembly = Assembly.GetExecutingAssembly();

            string luaResource = String.Format("BB.Caching.Lua.{0}.lua", fileName);
            if (!assembly.GetManifestResourceNames().Contains(luaResource))
                throw new ArgumentException(string.Format("Requested resource {0} was not found", luaResource));

            using (var resourceStream = assembly.GetManifestResourceStream(luaResource))
            {
                if (null == resourceStream)
                    throw new ArgumentException(string.Format("Unable to get resource stream for {0}", luaResource));

                using (var stream = new StreamReader(resourceStream))
                    return stream.ReadToEnd();
            }
        }
    }
}