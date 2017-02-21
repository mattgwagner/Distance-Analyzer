using Distance_Analyzer.Models;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Distance_Analyzer.Services
{
    public interface IStorageService
    {
        Task<IEnumerable<Node>> GetAll();

        Task<IEnumerable<Node>> SuperNodes();

        Task<Node> Get(String id);

        Task Store(Node node);
    }

    public class DocumentDbStorageService : IStorageService
    {
        // FIXME: These are the static settings for the local db emulator

        private IDocumentClient Db { get; } = new DocumentClient(new Uri("https://localhost:8081"), "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==");

        private static Uri NodesUri { get; } = UriFactory.CreateDocumentCollectionUri("Distance-Analyzer", nameof(Node));

        public async Task<Node> Get(String id)
        {
            var results =
                Db
                .CreateDocumentQuery<Node>(NodesUri)
                .Where(node => node.id == id)
                .ToList();

            return results.SingleOrDefault();
        }

        public async Task<IEnumerable<Node>> GetAll()
        {
            return
                Db
                .CreateDocumentQuery<Node>(NodesUri)
                .ToList();
        }

        public async Task Store(Node node)
        {
            await Db.UpsertDocumentAsync(NodesUri, node);
        }

        public async Task<IEnumerable<Node>> SuperNodes()
        {
            return
                Db
                .CreateDocumentQuery<Node>(NodesUri)
                .Where(node => node.Is_Super_Node)
                .ToList();
        }
    }

    public class InMemoryStorageService : IStorageService
    {
        private static IDictionary<String, Node> Nodes { get; } = new ConcurrentDictionary<String, Node>();

        static InMemoryStorageService()
        {
            foreach (var addr in new[] { "2289 Chesterfield Circle, Lakeland, FL 33813" })
            {
                var id = Guid.NewGuid().ToString();

                Nodes.Add(id, new Node
                {
                    id = id,
                    Raw = addr,
                    Address = "2289 Chesterfield Circle, Lakeland, FL 33813, USA",
                    Is_Super_Node = true,
                    Latitude = 27.9472729,
                    Longitude = -81.92191439999991
                });
            }
        }

        // private IDocumentClient client { get; } = new DocumentClient(new Uri("localhost"), "authKey");

        public Task<IEnumerable<Node>> GetAll()
        {
            return Task.FromResult(Nodes.Values.AsEnumerable());
        }

        public Task<IEnumerable<Node>> SuperNodes()
        {
            return Task.FromResult(Nodes.Values.Where(_ => _.Is_Super_Node).AsEnumerable());
        }

        public Task<Node> Get(String id)
        {
            return Task.FromResult(Nodes[id]);
        }

        public Task Store(Node node)
        {
            Nodes[node.id] = node;
            return Task.FromResult(0);
        }
    }
}