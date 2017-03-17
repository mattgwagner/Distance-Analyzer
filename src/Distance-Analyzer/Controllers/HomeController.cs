using Distance_Analyzer.Models;
using Distance_Analyzer.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Distance_Analyzer.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private Database Db { get; }

        private IMapsService Maps { get; }

        public HomeController(Database db, IMapsService maps)
        {
            Db = db;
            Maps = maps;
        }

        public async Task<IActionResult> Index()
        {
            // Returns a list of nodes

            return View(await Db.Nodes.ToListAsync());
        }

        [Route("~/Nodes/{id}")]
        public async Task<IActionResult> Node(String id)
        {
            // Return details of a single node

            var node = await Db.Nodes.FindAsync(id);

            if (node.Is_Super_Node)
            {
                // If it's a super node, go grab the details for other nodes mapped to it

                var mappings = from sub in Db.Nodes
                               from map in sub.Mappings
                               where map.To == node.id
                               select new Distance
                               {
                                   To = sub.id,
                                   Distance_Meters = map.Distance_Meters,
                                   Driving_Time = map.Driving_Time
                               };

                node.Mappings = mappings.ToList();
            }

            // Provide handler to 'Process' node

            return View(node);
        }

        [Route("~/Nodes/Scrub")]
        public IActionResult Scrub()
        {
            // Provide a raw address input form

            return View(Enumerable.Empty<MapsService.Result>());
        }

        [Route("~/Nodes/Scrub"), HttpPost]
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
                    Raw = raw_address,
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

        [Route("~/Nodes/New"), HttpPost]
        public async Task<IActionResult> New(Node node)
        {
            // Store a new node to the list

            await Db.Nodes.AddAsync(node);

            await Db.SaveChangesAsync();

            return RedirectToAction(nameof(Node), new { node.id });
        }

        [Route("~/Nodes/{id}/Process"), HttpPost]
        public async Task<IActionResult> Process(String id)
        {
            // Process the given node id with respect to the various super nodes, storing results

            var node = await Db.Nodes.FindAsync(id);

            var superNodes = await Db.Nodes.Where(_ => _.Is_Super_Node).ToListAsync();

            await Maps.Process(node, superNodes);

            await Db.SaveChangesAsync();

            return RedirectToAction(nameof(Node), id);
        }

        [HttpPost]
        public async Task<IActionResult> Process_All()
        {
            // Process all of the nodes

            var super_nodes = await Db.Nodes.Where(_ => _.Is_Super_Node).ToListAsync();

            foreach (var node in (await Db.Nodes.ToListAsync()).Where(_ => !_.Is_Super_Node))
            {
                await Maps.Process(node, super_nodes);
            }

            await Db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [AllowAnonymous]
        public IActionResult Login(String returnUrl = "/")
        {
            return new ChallengeResult("Auth0", new AuthenticationProperties { RedirectUri = returnUrl });
        }

        public async Task Logout()
        {
            await HttpContext.Authentication.SignOutAsync("Auth0", new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(Index))
            });

            await HttpContext.Authentication.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }

        public IActionResult Error()
        {
            return View();
        }
    }

    public class MapViewComponent : ViewComponent
    {
        private readonly GoogleMapsSettings Settings;

        private readonly Database Db;

        public MapViewComponent(GoogleMapsSettings settings, Database db)
        {
            this.Settings = settings;
            this.Db = db;
        }

        public async Task<IViewComponentResult> InvokeAsync(string id)
        {
            ViewBag.APIKey = Settings.Key;

            // TODO What do we need to generate a map?

            return View();
        }
    }

    public class NodeViewComponent : ViewComponent
    {
        private readonly Database db;

        public NodeViewComponent(Database db)
        {
            this.db = db;
        }

        public async Task<IViewComponentResult> InvokeAsync(string id)
        {
            return View(await db.Nodes.FindAsync(id));
        }
    }
}