﻿using Distance_Analyzer.Models;
using Distance_Analyzer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Distance_Analyzer.Controllers
{
    public class HomeController : Controller
    {
        private Database Db { get; }

        private IMapsService Maps { get; }

        public HomeController(Database db, IMapsService maps)
        {
            Db = db;
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

            return View(await Db.Nodes.ToListAsync());
        }

        [Route("~/Nodes/{id}")]
        public async Task<IActionResult> Node(String id)
        {
            // Return details of a single node

            // Provide handler to 'Process' node

            return View(await Db.Nodes.FindAsync(id));
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

            return RedirectToAction(nameof(Nodes));
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}