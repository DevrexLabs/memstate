using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Memstate.Models.Graph
{
    public partial class GraphModel
    {
        /// <summary>
        /// Unique id generator, shared by nodes and edges
        /// </summary>
        private long _lastId;

        //Nodes and edges
        private readonly SortedDictionary<long, Node> _nodesById;
        private readonly SortedDictionary<long, Edge> _edgesById;

        private SortedDictionary<string, SortedSet<Node>> _nodesByLabel;
        private SortedDictionary<string, SortedSet<Edge>> _edgesByLabel;


        public IEnumerable<Node> Nodes
        {
            get { return _nodesById.Values; }
        }

        public IEnumerable<Edge> Edges
        {
            get { return _edgesById.Values; }
        }

        public GraphModel()
        {
            var ignoreCase = StringComparer.OrdinalIgnoreCase;
            _edgesById = new SortedDictionary<long, Edge>();
            _edgesByLabel = new SortedDictionary<string, SortedSet<Edge>>(ignoreCase);
            _nodesById = new SortedDictionary<long, Node>();
            _nodesByLabel = new SortedDictionary<string, SortedSet<Node>>(ignoreCase);
        }

        public long CreateNode(string label)
        {
            var id = ++_lastId;
            var node = new Node(id, label);
            _nodesById[id] = node;
            AddByLabel(_nodesByLabel, node, label);
            return id;
        }

        public long CreateEdge(long fromId, long toId, string label)
        {
            Node from = NodeById(fromId);
            Node to = NodeById(toId);
            var id = ++_lastId;
            var edge = new Edge(id, label) { From = from, To = to };
            _edgesById[id] = edge;
            AddByLabel(_edgesByLabel, edge, label);
            from.Out.Add(edge);
            to.In.Add(edge);
            return id;
        }

        public void RemoveEdge(long id)
        {
            var edge = EdgeById(id);
            _edgesById.Remove(id);
            edge.From.Out.Remove(edge);
            edge.To.In.Remove(edge);
            _edgesByLabel[edge.Label].Remove(edge);
        }

        public void RemoveNode(long id)
        {
            var node = NodeById(id);
            foreach (var edge in node.Out) RemoveEdge(edge.Id);
            foreach (var edge in node.In) RemoveEdge(edge.Id);
            _nodesById.Remove(id);
            _nodesByLabel[node.Label].Remove(node);
        }

        public T Query<T>(Expression<Func<GraphModel, T>> query)
        {
            return query.Compile().Invoke(this);
        }

        private Node NodeById(long id)
        {
            return GetById(_nodesById, id);
        }

        private Edge EdgeById(long id)
        {
            return GetById(_edgesById, id);
        }

        private T GetById<T>(IDictionary<long, T> items, long id)
        {
            if (items.TryGetValue(id, out var item)) return item;
            throw new ArgumentException("No such node: " + id);
        }

        private static void AddByLabel<T>(IDictionary<string, SortedSet<T>> index, T item, string label)
        {
            SortedSet<T> set;
            if (!index.TryGetValue(label, out set))
            {
                set = new SortedSet<T>();
                index[label] = set;
            }
            set.Add(item);
        }

    }
}
