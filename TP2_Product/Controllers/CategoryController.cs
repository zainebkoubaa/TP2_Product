using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TP_Product.Models;
using TP_Product.Models.Repositories;

namespace TP_Product.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CategoryController : Controller
    {
        readonly ICategorieRepository CategRepository;

        public CategoryController(ICategorieRepository categRepository)
        {
            CategRepository = categRepository;
        }

        // GET: CategoryController
        [AllowAnonymous]
        public ActionResult Index()
        {
            var Categories = CategRepository.GetAll();
            return View(Categories);
        }

        // GET: CategoryController/Details/5
        [AllowAnonymous]
        public ActionResult Details(int id)
        {
            Category category = CategRepository.GetById(id);
            if (category == null)
                return NotFound();
            return View(category);
        }

        // GET: CategoryController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: CategoryController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Category category)
        {
            ModelState.Remove("Products");
            if (ModelState.IsValid)
            {
                CategRepository.Add(category);
                TempData["Success"] = "Catégorie ajoutée avec succès";
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: CategoryController/Edit/5
        public ActionResult Edit(int id)
        {
            Category category = CategRepository.GetById(id);
            if (category == null)
                return NotFound();
            return View(category);
        }

        // POST: CategoryController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, Category category)
        {
            ModelState.Remove("Products");
            if (ModelState.IsValid)
            {
                Category updated = CategRepository.Update(category);
                if (updated != null)
                {
                    TempData["Success"] = "Catégorie modifiée avec succès";
                    return RedirectToAction(nameof(Index));
                }
                else
                    return NotFound();
            }
            return View(category);
        }

        // GET: CategoryController/Delete/5
        public ActionResult Delete(int id)
        {
            Category category = CategRepository.GetById(id);
            if (category == null)
                return NotFound();
            return View(category);
        }

        // POST: CategoryController/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            CategRepository.Delete(id);
            TempData["Success"] = "Catégorie supprimée avec succès";
            return RedirectToAction(nameof(Index));
        }
    }
}