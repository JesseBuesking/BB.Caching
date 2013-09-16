using System.Collections.Generic;

namespace BB.Caching.Hashing
{
    /// <summary>
    /// An interface for a hash ring.
    /// </summary>
    /// <typeparam name="TNode">The type of object to store as a node.</typeparam>
    public interface IHashRing<TNode>
    {
        /// <summary>
        /// Adds a <see cref="TNode"/> to the hash ring.
        /// </summary>
        /// <param name="node"></param>
        void Add(TNode node);

        /// <summary>
        /// Adds a <see cref="TNode"/> to the hash ring, with the supplied weight.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="weight"></param>
        void Add(TNode node, int weight);

        /// <summary>
        /// Removes a <see cref="TNode"/> from the hash ring.
        /// </summary>
        /// <param name="node"></param>
        void Remove(TNode node);

        /// <summary>
        /// Gets the <see cref="TNode"/> associated to the <paramref name="key"/>.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        TNode GetNode(string key);

        /// <summary>
        /// Gets all distinct <see cref="TNode">TNodes</see> that are being used in the current
        /// <see cref="IHashRing{TNode}"/>.
        /// </summary>
        /// <returns></returns>
        HashSet<TNode> GetAvailableNodes();
    }
}