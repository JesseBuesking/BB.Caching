using System;
using System.Runtime.Caching;
using System.Threading;

namespace BB.Caching
{
	/// <summary>
	/// Singleton containing a reference to the <see cref="ObjectCache"/> that is used for storing data in the L1 cache.
	/// </summary>
	public class L1
	{
		private static readonly Lazy<L1> _lazy = new Lazy<L1>(
			() => new L1(), LazyThreadSafetyMode.ExecutionAndPublication);

		public static L1 Instance
		{
			get
			{
				return L1._lazy.Value;
			}
		}

		/// <summary>
		/// The <see cref="ObjectCache"/> used to store our L1 cache's data.
		/// </summary>
		public ObjectCache Cache
		{
			get;
			private set;
		}

		private L1()
		{
			this.Cache = new MemoryCache("l1-cache");
		}
	}
}