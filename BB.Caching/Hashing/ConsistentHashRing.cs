using System;
using System.Collections.Generic;
using System.Linq;

namespace BB.Caching.Hashing
{
    /// <summary>
    /// A hash ring that performs consistent hashing.
    /// </summary>
    /// <remarks>
    /// Modified from https://code.google.com/p/consistent-hash/
    /// </remarks>
    /// <typeparam name="TNode"></typeparam>
    public class ConsistentHashRing<TNode> : IHashRing<TNode>
    {
        /// <summary>
        /// The continuum onto which values are hashed into specific TNodes.
        /// </summary>
        private readonly SortedDictionary<uint, TNode> _continuum = new SortedDictionary<uint, TNode>();

        /// <summary>
        /// The hashing algorithm that's used to perform the consistent hashing.
        /// </summary>
        private IHashAlgorithm _hashAlgorithm;

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
        private uint[] _ayKeys;

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
        /// <param name="nodes"></param>
        /// <param name="hashAlgorithm"></param>
        public void Init(Dictionary<TNode, int> nodes, IHashAlgorithm hashAlgorithm)
        {
            this.Init(nodes, hashAlgorithm, this._replications);
        }

        /// <summary>
        /// Initializes the nodes.
        /// <remarks>
        /// Override the ToString method of <see cref="TNode"/>, since we'll use it to identify the node.
        /// </remarks>
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="hashAlgorithm"></param>
        /// <param name="replications"></param>
        public void Init(Dictionary<TNode, int> nodes, IHashAlgorithm hashAlgorithm, int replications)
        {
            this._replications = replications;
            this._hashAlgorithm = hashAlgorithm;

            if (null == nodes)
                return;

            foreach (var kvp in nodes)
                this.Add(kvp.Key, kvp.Value);
        }

        /// <summary>
        /// Adds a <see cref="TNode"/> to the hash ring.
        /// </summary>
        /// <param name="node"></param>
        public void Add(TNode node)
        {
            this.Add(node, 1);
        }

        /// <summary>
        /// Adds a <see cref="TNode"/> to the hash ring.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="weight"></param>
        public void Add(TNode node, int weight)
        {
            this._nodeWeights.Add(node, weight);
            for (int i = 0; i < (this._replications*weight); i++)
            {
                uint hash = this._hashAlgorithm.ComputeInt(node.ToString() + i);
                this._continuum[hash] = node;
            }

            this._ayKeys = this._continuum.Keys.ToArray();
            this._max = this._ayKeys[this._ayKeys.Length - 1];
        }

        /// <summary>
        /// Removes a <see cref="TNode"/> from the hash ring.
        /// </summary>
        /// <param name="node"></param>
        public void Remove(TNode node)
        {
            int weight = this._nodeWeights[node];
            for (int i = 0; i < (this._replications*weight); i++)
            {
                uint hash = this._hashAlgorithm.ComputeInt(node.ToString() + i);
                this._continuum.Remove(hash);
            }

            this._ayKeys = this._continuum.Keys.ToArray();
            this._max = (0 < this._ayKeys.Length)
                ? this._ayKeys[this._ayKeys.Length - 1]
                : 0;
        }

        /// <summary>
        /// Returns the index of the first item >= val, or 0.
        /// </summary>
        /// <param name="ay">An ordered array of unsigned ints.</param>
        /// <param name="val"></param>
        /// <returns></returns>
        private int GetFirstIndex(uint[] ay, uint val)
        {
            if (this._max < val)
                return 0;

            int binarySearch = Array.BinarySearch(ay, val);
            return (0 < binarySearch)
                ? binarySearch
                : ~binarySearch;
        }

        /// <summary>
        /// Gets the <see cref="TNode"/> associated to the <paramref name="key"/>.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public TNode GetNode(String key)
        {
            if (null == this._continuum)
                throw new Exception("continuum cannot be null");

            uint hash = this._hashAlgorithm.ComputeInt(key);

            int first = this.GetFirstIndex(this._ayKeys, hash);

            return this._continuum[this._ayKeys[first]];
        }

        /// <summary>
        /// The nodes that are currently stored.
        /// </summary>
        public HashSet<TNode> GetAvailableNodes()
        {
            return new HashSet<TNode>(this._continuum.Values);
        } 
    }
}