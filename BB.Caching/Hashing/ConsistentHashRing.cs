// ReSharper disable once CheckNamespace
namespace BB.Caching
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Methods and classes for hashing data.
    /// </summary>
    public static partial class Hashing
    {
        /// <summary>
        /// A hash ring that performs consistent hashing.
        /// </summary>
        /// <remarks>
        /// Modified from https://code.google.com/p/consistent-hash/
        /// </remarks>
        /// <typeparam name="TNode">
        /// The type of a node.
        /// </typeparam>
        public class ConsistentHashRing<TNode>
        {
            /// <summary>
            /// The continuum onto which values are hashed into specific TNodes.
            /// </summary>
            private readonly SortedDictionary<uint, TNode> _continuum = new SortedDictionary<uint, TNode>();

            /// <summary>
            /// The nodes that are hashed and their respective weights.
            /// </summary>
            private readonly Dictionary<TNode, int> _nodeWeights = new Dictionary<TNode, int>();

            /// <summary>
            /// How many times we should replicate a single node.
            /// <remarks>
            /// Not literally (aka physical machine replication), just on the hash ring.
            /// </remarks>
            /// </summary>
            private int _replications = 512;

            /// <summary>
            /// Cache the ordered keys for better performance.
            /// </summary>
            private uint[] _arrayKeys;

            /// <summary>
            /// The maximum node hash stored.
            /// </summary>
            private uint _max;

            /// <summary>
            /// Initializes the nodes.
            /// <remarks>
            /// Override the ToString method of <see cref="TNode"/>, since we'll use it to identify the node.
            /// </remarks>
            /// </summary>
            /// <param name="nodes">
            /// The nodes.
            /// </param>
            public void Init(Dictionary<TNode, int> nodes)
            {
                this.Init(nodes, this._replications);
            }

            /// <summary>
            /// Initializes the nodes.
            /// <remarks>
            /// Override the ToString method of <see cref="TNode"/>, since we'll use it to identify the node.
            /// </remarks>
            /// </summary>
            /// <param name="nodes">
            /// The nodes.
            /// </param>
            /// <param name="replications">
            /// The number of replications.
            /// </param>
            public void Init(Dictionary<TNode, int> nodes, int replications)
            {
                this._replications = replications;

                if (null == nodes)
                {
                    return;
                }

                foreach (var kvp in nodes)
                {
                    this.Add(kvp.Key, kvp.Value);
                }
            }

            /// <summary>
            /// Adds a <see cref="TNode"/> to the hash ring.
            /// </summary>
            /// <param name="node">
            /// The node.
            /// </param>
            public void Add(TNode node)
            {
                this.Add(node, 1);
            }

            /// <summary>
            /// Adds a <see cref="TNode"/> to the hash ring.
            /// </summary>
            /// <param name="node">
            /// The node.
            /// </param>
            /// <param name="weight">
            /// The weight of the node.
            /// </param>
            public void Add(TNode node, int weight)
            {
                this._nodeWeights.Add(node, weight);
                for (int i = 0; i < (this._replications * weight); i++)
                {
                    uint hash = Hashing.Murmur3.ComputeInt(node.ToString() + i);
                    this._continuum[hash] = node;
                }

                this._arrayKeys = this._continuum.Keys.ToArray();
                this._max = this._arrayKeys[this._arrayKeys.Length - 1];
            }

            /// <summary>
            /// Removes a <see cref="TNode"/> from the hash ring.
            /// </summary>
            /// <param name="node">
            /// The node.
            /// </param>
            public void Remove(TNode node)
            {
                int weight = this._nodeWeights[node];
                for (int i = 0; i < (this._replications * weight); i++)
                {
                    uint hash = Hashing.Murmur3.ComputeInt(node.ToString() + i);
                    this._continuum.Remove(hash);
                }

                this._arrayKeys = this._continuum.Keys.ToArray();
                this._max = (0 < this._arrayKeys.Length)
                    ? this._arrayKeys[this._arrayKeys.Length - 1]
                    : 0;
            }

            /// <summary>
            /// Gets the <see cref="TNode"/> associated to the <paramref name="key"/>.
            /// </summary>
            /// <param name="key">
            /// The key where the node is.
            /// </param>
            /// <returns>
            /// The <see cref="TNode"/>.
            /// </returns>
            public TNode GetNode(string key)
            {
                if (null == this._continuum)
                {
                    throw new Exception("continuum cannot be null");
                }

                uint hash = Hashing.Murmur3.ComputeInt(key);

                int first = this.GetFirstIndex(this._arrayKeys, hash);

                return this._continuum[this._arrayKeys[first]];
            }

            /// <summary>
            /// The nodes that are currently stored.
            /// </summary>
            /// <returns>
            /// The nodes that are available.
            /// </returns>
            public HashSet<TNode> GetAvailableNodes()
            {
                return new HashSet<TNode>(this._continuum.Values);
            }

            /// <summary>
            /// Returns the index of the first item &gt;= val, or 0.
            /// </summary>
            /// <param name="array">
            /// An ordered array of unsigned ints.
            /// </param>
            /// <param name="val">
            /// The val.
            /// </param>
            /// <returns>
            /// The first index of the node.
            /// </returns>
            private int GetFirstIndex(uint[] @array, uint val)
            {
                if (this._max < val)
                {
                    return 0;
                }

                int binarySearch = Array.BinarySearch(@array, val);
                return (0 < binarySearch)
                    ? binarySearch
                    : ~binarySearch;
            }
        }
    }
}