using Distance_Analyzer.Models;
using GoogleMapsApi;
using GoogleMapsApi.Entities.DistanceMatrix.Request;
using GoogleMapsApi.Entities.Geocoding.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Distance_Analyzer.Services
{
    public interface IMapsService
    {
        Task<IEnumerable<MapsService.Result>> Scrub(String raw_address);

        /// <summary>
        /// Process a node and calculate the driving distances to all of the super nodes
        /// </summary>
        Task<Node> Process(Node node, IEnumerable<Node> superNodes);
    }

    public class MapsService : IMapsService
    {
        public virtual async Task<IEnumerable<MapsService.Result>> Scrub(string raw_address)
        {
            var response = await GoogleMaps.Geocode.QueryAsync(new GeocodingRequest
            {
                Address = raw_address
            });

            return from address in response.Results
                   select new MapsService.Result
                   {
                       Address = address.FormattedAddress,
                       Latitude = address.Geometry.Location.Latitude,
                       Longitude = address.Geometry.Location.Longitude
                   };
        }

        public virtual async Task<Node> Process(Node node, IEnumerable<Node> superNodes)
        {
            foreach (var superNode in superNodes)
            {
                // Check if we've already got a calculated distance to this superNode

                if (node.Mappings.Any(_ => _.To == superNode.id)) continue;

                var response = await GoogleMaps.DistanceMatrix.QueryAsync(new DistanceMatrixRequest
                {
                    // We can actually probably calculate more than one destination at a time...?

                    Origins = new[] { node.Address },
                    Destinations = new[] { superNode.Address }
                });

                foreach (var row in response.Rows)
                {
                    // Distance Text 61km Value 61748
                    // Duration Text 0:38:46 Value 3854

                    foreach (var element in row.Elements)
                    {
                        node
                            .Mappings
                            .Add(new Distance
                            {
                                To = superNode.id,
                                Distance_Meters = element.Distance.Value,
                                Driving_Time = element.Duration.Value
                            });
                    }
                }
            }

            return node;
        }

        public class Result
        {
            public String Address { get; set; }

            public Double Latitude { get; set; }

            public Double Longitude { get; set; }
        }
    }
}