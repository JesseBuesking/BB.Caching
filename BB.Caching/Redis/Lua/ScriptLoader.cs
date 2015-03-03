namespace BB.Caching.Redis.Lua
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;

    /// <summary>
    /// Manages loading lua scripts from files.
    /// </summary>
    public class ScriptLoader
    {
        /// <summary>
        /// Lazily loads the instance.
        /// </summary>
        private static readonly Lazy<ScriptLoader> _Lazy = new Lazy<ScriptLoader>(
            () => new ScriptLoader(), LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// A cache to store the contents of the lua files.
        /// </summary>
        private readonly Dictionary<string, string> _cache = new Dictionary<string, string>();

        /// <summary>
        /// Prevents a default instance of the <see cref="ScriptLoader"/> class from being created.
        /// </summary>
        private ScriptLoader()
        {
        }

        /// <summary>
        /// Gets the instance.
        /// </summary>
        public static ScriptLoader Instance
        {
            get { return _Lazy.Value; }
        }

        /// <summary>
        /// Index to get a script for a particular key.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <returns>
        /// The lua script contents as a string.
        /// </returns>
        public string this[string key]
        {
            get
            {
                string result;
                if (this._cache.TryGetValue(key, out result))
                {
                    return result;
                }

                this._cache[key] = ScriptLoader.LuaFileToString(key);
                return this._cache[key];
            }
        }

        /// <summary>
        /// Returns the lua file's contents as a string.
        /// </summary>
        /// <param name="fileName">
        /// The filename of the lua script.
        /// </param>
        /// <returns>
        /// The lua script contents as a string.
        /// </returns>
        private static string LuaFileToString(string fileName)
        {
            var assembly = Assembly.GetExecutingAssembly();

            string luaResource = string.Format("BB.Caching.Redis.Lua.Scripts.{0}.lua", fileName);
            if (!assembly.GetManifestResourceNames().Contains(luaResource))
            {
                throw new ArgumentException(string.Format("Requested resource {0} was not found", luaResource));
            }

            var resourceStream = assembly.GetManifestResourceStream(luaResource);
            if (null == resourceStream)
            {
                throw new ArgumentException(string.Format("Unable to get resource stream for {0}", luaResource));
            }

            using (var stream = new StreamReader(resourceStream))
            {
                return stream.ReadToEnd();
            }
        }
    }
}