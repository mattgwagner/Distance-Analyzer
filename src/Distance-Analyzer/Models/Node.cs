using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Distance_Analyzer.Models
{
    public class Node
    {
        public string id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// A generic description, i.e. name, for the node
        /// </summary>
        public String Description { get; set; }

        /// <summary>
        /// The node's raw address entry
        /// </summary>
        public String Raw { get; set; }

        /// <summary>
        /// The node's scrubbed address
        /// </summary>
        public String Address { get; set; }

        public Double Latitude { get; set; }

        public Double Longitude { get; set; }

        /// <summary>
        /// A list of tags applied to this node for identification
        /// </summary>
        [NotMapped]
        public ICollection<String> Tags { get; set; } = new List<String>();

        public String TagsList
        {
            get { return JsonConvert.SerializeObject(Tags); }
            set { Tags = JsonConvert.DeserializeObject<List<String>>(value); }
        }

        /// <summary>
        /// True if the node should be considered a super node, where all other nodes are calculated in relation to
        /// </summary>
        [Display(Name = "Is a Super Node?")]
        public Boolean Is_Super_Node { get; set; }

        /// <summary>
        /// A list of distance mapping between this node and the super nodes
        /// </summary>
        [NotMapped]
        public ICollection<Distance> Mappings { get; set; } = new List<Distance>();

        public String MappingsList
        {
            get { return JsonConvert.SerializeObject(Mappings); }
            set { Mappings = JsonConvert.DeserializeObject<List<Distance>>(value); }
        }
    }

    public sealed class Distance
    {
        /// <summary>
        /// Which node this mapped distance is in relation to
        /// </summary>
        public String To { get; set; }

        /// <summary>
        /// The calculated driving time under usual conditions to this node
        /// </summary
        [Display(Name = "Drive Time")]
        public TimeSpan Driving_Time { get; set; }

        /// <summary>
        /// The driving distance in miles to this node
        /// </summary>
        [Display(Name = "Distance (Meters)")]
        public Decimal Distance_Meters { get; set; }
    }

    public sealed class Address
    {
        public String Street { get; set; }

        public String City { get; set; }

        public String State { get; set; }

        public String PostalCode { get; set; }

        public override string ToString()
        {
            return $"{Street}, {City}, {State} {PostalCode}";
        }
    }
}