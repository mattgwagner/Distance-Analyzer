using Distance_Analyzer.Models;
using Distance_Analyzer.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Distance_Analyzer.Controllers
{
    public class HomeController : Controller
    {
        private IStorageService Storage { get; }

        private IMapsService Maps { get; }

        public HomeController(IStorageService storage, IMapsService maps)
        {
            Storage = storage;
            Maps = maps;
        }

        // TODO Login/Logout

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        [Route("~/Nodes")]
        public async Task<IActionResult> Nodes()
        {
            // Returns a list of nodes

            return View(await Storage.GetAll());
        }

        [Route("~/Nodes/{id}")]
        public async Task<IActionResult> Node(Guid id)
        {
            // Return details of a single node

            // Provide handler to 'Process' node

            return View(await Storage.Get(id));
        }

        [Route("~/Nodes/Scrub")]
        public IActionResult Scrub()
        {
            // Provide a raw address input form

            return View(Enumerable.Empty<MapsService.Result>());
        }

        [HttpPost]
        public async Task<IActionResult> Scrub(String raw_address)
        {
            // Scrub the incoming raw address, provide selection if multiple or create new node

            var results = await Maps.Scrub(raw_address);

            var count = results.Count();

            if (count == 1)
            {
                var first = results.First();

                return View(nameof(New), new Node
                {
                    Address = first.Address,
                    Latitude = first.Latitude,
                    Longitude = first.Longitude
                });
            }

            // 0 or many matches

            return View(results);
        }

        [Route("~/Nodes/New")]
        public IActionResult New()
        {
            // New node form

            return View(new Node { });
        }

        [HttpPost]
        public async Task<IActionResult> New(Node node)
        {
            // Store a new node to the list

            await Storage.Store(node);

            return RedirectToAction(nameof(Node), node.Id);
        }

        [HttpPost]
        public async Task<IActionResult> Process(Guid id)
        {
            // Process the given node id with respect to the various super nodes, storing results

            var node = await Storage.Get(id);

            var superNodes = await Storage.SuperNodes();

            var processed = await Maps.Process(node, superNodes);

            await Storage.Store(processed);

            return RedirectToAction(nameof(Node), id);
        }

        [HttpPost]
        public async Task<IActionResult> Process_All()
        {
            // Process all of the nodes

            var super_nodes = await Storage.SuperNodes();

            foreach (var node in (await Storage.GetAll()).Where(_ => !_.Is_Super_Node))
            {
                var processed = await Maps.Process(node, super_nodes);

                await Storage.Store(processed);
            }

            return RedirectToAction(nameof(Nodes));
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}