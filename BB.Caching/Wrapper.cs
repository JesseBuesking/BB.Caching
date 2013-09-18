using System;
using System.Threading.Tasks;

namespace BB.Caching
{
    /// <summary>
    /// An object used to wrap data for use in asynchronous methods.
    /// <remarks>
    /// See http://stackoverflow.com/questions/18117287/async-tryblah-pattern
    /// </remarks>
    /// </summary>
    /// <typeparam name="TType"></typeparam>
    /// <typeparam name="TTask"></typeparam>
    public class Wrapper<TType, TTask>
    {
        /// <summary>
        /// Used to verify that the data in the wrapper was set before accessing it.
        /// <remarks>
        /// This way the <see cref="Wrapper{TType, TTask}"/> object can enforce the same behavior as an <code>out</code>
        /// param, although this enforcement will now happen at runtime instead of compile time.
        /// </remarks>
        /// </summary>
        public Task<bool> IsNilAsync
        {
// ReSharper disable MemberCanBePrivate.Global
            get;
// ReSharper restore MemberCanBePrivate.Global
            set;
        }

        /// <summary>
        /// Used to verify that the data in the wrapper was set before accessing it.
        /// <remarks>
        /// This way the <see cref="Wrapper{TType, TTask}"/> object can enforce the same behavior as an <code>out</code>
        /// param, although this enforcement will now happen at runtime instead of compile time.
        /// </remarks>
        /// </summary>
        public bool IsNil
        {
            get { return this.IsNilAsync.Result; }
        }

        private bool _set;

        /// <summary>
        /// The value of the resulting async operation.
        /// </summary>
        public Task<TType> ValueAsync
        {
            get
            {
                if (!this._set)
                    throw new Exception("value was never set");

                return this._valueAsync;
            }
            set
            {
                this._valueAsync = value;
                this._set = true;
            }
        }

        private Task<TType> _valueAsync;

        /// <summary>
        /// The value of the resulting async operation.
        /// </summary>
        public TType Value
        {
            get { return this.ValueAsync.Result; }
        }

        public Task<TTask> TaskResult
        {
            get;
            set;
        }

        /// <summary>
        /// Allows accessing the data stored in the wrapper without having to explicitly access the Value property.
        /// <example>
        /// <code>
        /// var value = new Wrapper&lt;string&gt;();
        /// // accessing the data via the property
        /// string stored = value.Value;
        /// // accessing the data without going through the property
        /// string stored = value;
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static implicit operator Task<TType>(Wrapper<TType, TTask> instance)
        {
            return instance.ValueAsync;
        }
    }
}