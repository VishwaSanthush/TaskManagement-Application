using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System.Diagnostics;
using ToDoApp.Models;

namespace ToDoApp.Controllers
{
    public class HomeController : Controller
    {
        private ToDoContext context;

        public HomeController(ToDoContext ctx) => context = ctx;
        

        public IActionResult Index(string id)
        {
            var filters = new Filters(id);
            ViewBag.Filters = filters;

            ViewBag.Categories = context.Category.ToList();
            ViewBag.Statuses = context.Statuses.ToList();
            ViewBag.DueFilters = Filters.DueFilterValues;

            IQueryable<ToDo> query = context.ToDos
                .Include(t => t.Category)
                .Include(t => t.Status);

            if(filters.HasCategory)
            {
                query = query.Where(t => t.CategoryId == filters.CategoryId);
            }
            if (filters.HasStatus)
            {
                query = query.Where(t => t.StstusId == filters.StatusId);
            }
            if (filters.HasDue)
            {
                var today = DateTime.Now;
                if (filters.Ispast)
                {
                    query = query.Where(t => t.DueDate < today);
                }
                else if (filters.IsFeature)
                {
                    query = query.Where(t => t.DueDate > today);
                }
                else if (filters.IsToday)
                {
                    {
                        query = query.Where(t => t.DueDate > today);
                    }

                }
            }
            var tasks = query.OrderBy(t => t.DueDate).ToList();
            return View(tasks);


        }
        [HttpGet]
        public IActionResult Add()
        {
            ViewBag.Categories = context.Category.ToList();
            ViewBag.Statuses = context.Statuses.ToList();
            var task = new ToDo { StstusId = "open" };
            return View(task);
        }

        [HttpPost]

        public IActionResult Add (ToDo task)
        {
            if(ModelState.IsValid)
            {
                context.ToDos.Add(task);
                context.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                ViewBag.Categories = context.Category.ToList();
                ViewBag.Statuses = context.Statuses.ToList();
                return View(task);
            }
        }
        
        [HttpPost]
        public IActionResult Filter(string[] filter)
        {
            string id = string.Join('-', filter);
            return RedirectToAction("Index", new{ ID=id});
        }

        [HttpPost]
        public IActionResult MarkComplete([FromRoute] string id, ToDo selected)
        {
            selected = context.ToDos.Find(selected.Id)!;

            if(selected != null)
            {
                selected.StstusId = "closed";
                context.SaveChanges();
            }
            return RedirectToAction("Index", new {ID = id});
        }

        [HttpPost]

        public IActionResult DeleteCompleate(string id)
        {
            var toDelete = context.ToDos.Where(t => t.StstusId == "closed").ToList();

            foreach(var task in toDelete)
            {
                context.ToDos.Remove(task);
            }
            context.SaveChanges();

            return RedirectToAction("Index", new {ID=id});
        }

    }
}
