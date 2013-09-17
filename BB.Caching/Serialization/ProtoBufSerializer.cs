using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using ProtoBuf.Meta;

namespace BB.Caching.Serialization
{
    public class ProtoBufSerializer : ISerializer
    {
        private static readonly Lazy<ProtoBufSerializer> _lazy = new Lazy<ProtoBufSerializer>(
            () => new ProtoBufSerializer(), LazyThreadSafetyMode.ExecutionAndPublication);

        private const string _protoIndicesRedisKey = "protoindices";

        public static ISerializer Instance
        {
            get { return ProtoBufSerializer._lazy.Value; }
        }

        private readonly Dictionary<string, TypeModel> _serializers = new Dictionary<string, TypeModel>();

        private Dictionary<string, Dictionary<string, int>> _typeIndices =
            new Dictionary<string, Dictionary<string, int>>();

        private ProtoBufSerializer()
        {
        }

        /// <summary>
        /// Takes care of updating the shared cache.
        /// </summary>
        private void UpdateSharedCacheUsingTypeIndices()
        {
            Cache.Config.Set(ProtoBufSerializer._protoIndicesRedisKey, this._typeIndices, true);
        }

        /// <summary>
        /// Gets the current set of indices from the cache (so that the servers stay in sync).
        /// </summary>
        private void UpdateTypeIndicesFromSharedCache()
        {
            var typeIndices = Cache.Config.Get<Dictionary<string, Dictionary<string, int>>>(
                ProtoBufSerializer._protoIndicesRedisKey);

            this._typeIndices = typeIndices
                ?? this._typeIndices
                    ?? new Dictionary<string, Dictionary<string, int>>();
        }

        /// <summary>
        /// Serializes the object into a byte array.
        /// <para>
        /// Uses protobuf to serialize the object supplied.
        /// (No need to decorate the class being deserialized, as this will use an auto-generated serializer).
        /// </para>
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public byte[] Serialize<TType>(TType value)
        {
            TypeModel model;
            Type type = value.GetType();
            if (!this._serializers.TryGetValue(type.FullName, out model))
            {
                RuntimeTypeModel rtm = null;
                this.CreateModelForType(type, ref rtm);
                if (null != rtm)
                {
//                    rtm.Freeze(); // TODO: What is modifying this on initial load?
                    model = rtm.Compile();
                    this._serializers.Add(type.FullName, model);
                }
            }

            if (null == model)
                throw new Exception("model cannot be null");

            using (MemoryStream ms = new MemoryStream())
            {
                model.Serialize(ms, value);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Deserializes the object from the byte array.
        /// <para>
        /// Uses protobuf to deserialize the byte array supplied.
        /// (No need to decorate the class being deserialized, as this will use an auto-generated serializer).
        /// </para>
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public TType Deserialize<TType>(byte[] value)
        {
            TypeModel model;
            Type type = typeof (TType);
            if (!this._serializers.TryGetValue(type.FullName, out model))
            {
                RuntimeTypeModel rtm = null;
                this.CreateModelForType(type, ref rtm);
                if (null != rtm)
                {
//                    rtm.Freeze(); // TODO: What is modifying this on initial load?
                    model = rtm.Compile();
                    this._serializers.Add(type.FullName, model);
                }
            }

            if (null == model)
                return default (TType);

            using (MemoryStream ms = new MemoryStream(value))
            {
                TType res = default (TType);
                res = (TType) model.Deserialize(ms, res, type);

                return res;
            }
        }

        /// <summary>
        /// Generates the necessary meta data to serialize / deserialize the <see cref="Type"/> supplied.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="rtm"></param>
        private void CreateModelForType(Type type, ref RuntimeTypeModel rtm)
        {
            bool required = this.RequiresSharedCacheLookup(type);
            if (required)
                this.UpdateTypeIndicesFromSharedCache();

            this.CreateModelForTypeInternal(type, ref rtm);

            if (required)
                this.UpdateSharedCacheUsingTypeIndices();
        }

        /// <summary>
        /// If there's something that we've had to assign an index for, return true so that we can sync the indice up
        /// across all servers. Otherwise, avoid the lookup (and potentially a circular-reference) and return false.
        /// TODO: should probably utilize the same logic that's used to assign indices to unknown objects
        /// (see SetInterfaceImplementationIndices)
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private bool RequiresSharedCacheLookup(Type type)
        {
            if (type == null)
                return false;

            if (type.IsPrimitive)
                return false;

            if (type.IsEnum)
                return true;

            if ("System.String" == type.FullName)
                return false;

            var args = type.GetGenericArguments();
            return args.Any(this.RequiresSharedCacheLookup);
        }

        /// <summary>
        /// Automagically assigns protobuf indices to objects and updates the runtime type model so that it can be used
        /// to serialize an object using protobuf.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="rtm"></param>
        private void CreateModelForTypeInternal(Type type, ref RuntimeTypeModel rtm)
        {
            if (null == rtm)
                rtm = TypeModel.Create();

            try
            {
                TypeModel.Create().Add(type, false);
            }
            catch (ArgumentException ex)
            {
                if (ex.Message.Contains("Data of this type has inbuilt behaviour"))
                    return;
            }

            if (!ShouldModel(type))
                return;

            IEnumerable<Type> pTypes = type.GetProperties()
                .Where(p => p.CanWrite)
                .Where(p => p.PropertyType.IsClass || p.PropertyType.IsInterface)
                .Select(p => p.PropertyType);

            IEnumerable<Type> fTypes = type.GetFields()
                .Where(f => f.FieldType.IsClass || f.FieldType.IsInterface)
                .Select(f => f.FieldType);

            IOrderedEnumerable<Type> orderedTypes = pTypes
                .Union(fTypes)
                .Distinct()
                .OrderBy(t => t.Name);

            foreach (Type oType in orderedTypes)
                this.CreateModelForTypeInternal(oType, ref rtm);

            MetaType metaType = this.SetInterfaceImplementationIndices(type, rtm);
// ReSharper disable ConvertIfStatementToNullCoalescingExpression
            if (null == metaType)
// ReSharper restore ConvertIfStatementToNullCoalescingExpression
                metaType = rtm.Add(type, false);

            Tuple<Type, string>[] members;
            if (TryGetMemberNames(type, out members))
                this.SetMemberIndices(type, members, metaType);

            // No need to specify a default constructor!
            metaType.UseConstructor = false;

            // I believe this will save some additional space when default values are stored! Hizzah!
            rtm.UseImplicitZeroDefaults = true;
        }

        /// <summary>
        /// Assigns a unique indice to each implementation of an interface and adds them as subtypes of the interface
        /// on the <see cref="MetaType"/> object.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="rtm"></param>
        private MetaType SetInterfaceImplementationIndices(Type type, RuntimeTypeModel rtm)
        {
            if (!type.IsInterface)
                return null;

            List<Type> implementationTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(type.IsAssignableFrom)
                .Where(at => at.IsClass)
                .Distinct()
                .ToList();

            // Get the already created MetaType if we've already added this type to the model.
            MetaType metaType = null;
            bool found = false;
            var enumerator = rtm.GetTypes().GetEnumerator();
            while (enumerator.MoveNext())
            {
                metaType = (MetaType) enumerator.Current;
                if (metaType.Type != type)
                    continue;

                found = true;
                break;
            }

            // Add the type to the model and get the MetaType if we haven't already.
            if (!found)
                metaType = rtm.Add(type, false);

            foreach (Type implementationType in implementationTypes)
            {
                // Add the model data for the concrete type so we can serialize/deserialize it correctly.
                this.CreateModelForTypeInternal(implementationType, ref rtm);

                Dictionary<string, int> indices;
                if (!this._typeIndices.TryGetValue(type.FullName, out indices))
                    indices = new Dictionary<string, int>();

                string key = "concrete-" + implementationType.Name;

                if (!indices.ContainsKey(key))
                {
                    int nextNumber = this.GetNextTypeIndice(type);
                    indices.Add(key, nextNumber);
                    this._typeIndices[type.FullName] = indices;

                    metaType.AddSubType(nextNumber, implementationType);
                }
                else
                {
                    metaType.AddSubType(indices[key], implementationType);
                }
            }

            return metaType;
        }

        /// <summary>
        /// Adds the members for the <see cref="Type"/> on the <see cref="MetaType"/>, specifying the ProtoMember
        /// indices for them.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="members"></param>
        /// <param name="metaType"></param>
        private void SetMemberIndices(Type type, IEnumerable<Tuple<Type, string>> members, MetaType metaType)
        {
            foreach (Tuple<Type, string> tuple in members)
            {
                Dictionary<string, int> indices;
                if (!this._typeIndices.TryGetValue(type.FullName, out indices))
                    indices = new Dictionary<string, int>();

                string key = "member-" + tuple.Item1.Name + "-" + tuple.Item2;

                if (!indices.ContainsKey(key))
                {
                    int nextNumber = this.GetNextTypeIndice(type);
                    indices.Add(key, nextNumber);
                    this._typeIndices[type.FullName] = indices;

                    metaType.Add(nextNumber, tuple.Item2);
                }
                else
                {
                    metaType.Add(indices[key], tuple.Item2);
                }
            }
        }

        /// <summary>
        /// Gets the next available ProtoMember index for they supplied <see cref="Type"/>.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private int GetNextTypeIndice(Type type)
        {
            int start = 1;
            Dictionary<string, int> value;
            if (this._typeIndices.TryGetValue(type.FullName, out value))
            {
                while (value.Values.Contains(start))
                    start++;
            }

            return start;
        }

        /// <summary>
        /// Tries to get the member names (<see cref="string">strings</see>) for the supplied type.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="names"></param>
        /// <returns></returns>
        private static bool TryGetMemberNames(Type type, out Tuple<Type, string>[] names)
        {
            var props = type.GetProperties()
                .Where(p => p.CanWrite)
                .Select(p => new Tuple<Type, string>(p.PropertyType, p.Name));

            var fields = type.GetFields()
                .Select(f => new Tuple<Type, string>(f.FieldType, f.Name));

            List<Tuple<Type, string>> list = fields.Union(props).OrderBy(v => v.Item2).ToList();
            if (null != type.BaseType && "Enum" == type.BaseType.Name)
                list.Remove(new Tuple<Type, string>(typeof (int), "value__"));

            names = list.ToArray();

            return names.Length > 0 && (names.Length != 1 || names[0].Item2 != "Empty");
        }

        /// <summary>
        /// Tries to get the member names (<see cref="string">strings</see>) for the supplied type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static bool ShouldModel(Type type)
        {
            IEnumerable<string> props = type.GetProperties()
                .Where(p => p.CanWrite)
                .Select(p => p.Name);

            IEnumerable<string> fields = type.GetFields()
                .Select(f => f.Name);

            List<string> listOfNames = fields.Union(props).OrderBy(v => v).ToList();
            if (null != type.BaseType && "Enum" == type.BaseType.Name)
                listOfNames.Remove("value__");

            string[] names = listOfNames.ToArray();

            bool isString = names.Length == 1 && names[0] == "Empty";
            return !isString;
        }
    }
}