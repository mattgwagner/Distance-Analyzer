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

        Task<Node> Get(Guid id);

        Task Store(Node node);
    }

    public class DocumentDbStorageService : IStorageService
    {
        // FIXME: These are the static settings for the local db emulator

        private IDocumentClient client { get; } = new DocumentClient(new Uri("https://localhost:8081"), "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==");

        static DocumentDbStorageService()
        {

        }

        public Task<Node> Get(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Node>> GetAll()
        {
            throw new NotImplementedException();
        }

        public Task Store(Node node)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Node>> SuperNodes()
        {
            throw new NotImplementedException();
        }
    }

    public class InMemoryStorageService : IStorageService
    {
        private static IDictionary<Guid, Node> Nodes { get; } = new ConcurrentDictionary<Guid, Node>();

        static InMemoryStorageService()
        {
            foreach (var addr in new[] { "2289 Chesterfield Circle, Lakeland, FL 33813" })
            {
                var id = Guid.NewGuid();

                Nodes.Add(id, new Node
                {
                    Id = id,
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

        public Task<Node> Get(Guid id)
        {
            return Task.FromResult(Nodes[id]);
        }

        public Task Store(Node node)
        {
            Nodes[node.Id] = node;
            return Task.FromResult(0);
        }
    }
}