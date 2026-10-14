using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TP_Product.Helpers;
using TP_Product.Models;
using TP_Product.Models.Repositories;
using TP_Product.ViewModels;

namespace TP_Product.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
        readonly IProductRepository ProductRepository;
        readonly ICategorieRepository CategRepository;
        private readonly IWebHostEnvironment hostingEnvironment;

        public ProductController(IProductRepository ProdRepository, ICategorieRepository categRepository, IWebHostEnvironment hostingEnvironment)
        {
            ProductRepository = ProdRepository;
            CategRepository = categRepository;
            this.hostingEnvironment = hostingEnvironment;
        }

        // GET: ProductController  (filtres + tri + pagination)
        [AllowAnonymous]
        public ActionResult Index(string? val, int? categoryId, bool inStock = false, string? sort = null, int page = 1)
        {
            IEnumerable<Product> products = string.IsNullOrWhiteSpace(val)
                ? ProductRepository.GetAll()
                : ProductRepository.FindByName(val);

            if (categoryId.HasValue)
                products = products.Where(p => p.CategoryId == categoryId.Value);

            if (inStock)
                products = products.Where(p => p.QteStock > 0);

            products = sort switch
            {
                "price_asc" => products.OrderBy(p => p.Price),
                "price_desc" => products.OrderByDescending(p => p.Price),
                "name" => products.OrderBy(p => p.Name),
                _ => products
            };

            const int pageSize = 9;
            int total = products.Count();
            int totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)pageSize));
            page = Math.Clamp(page, 1, totalPages);

            var paged = products.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.Categories = new SelectList(CategRepository.GetAll(), "CategoryId", "CategoryName", categoryId);
            ViewBag.Val = val;
            ViewBag.CategoryId = categoryId;
            ViewBag.InStock = inStock;
            ViewBag.Sort = sort;
            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.Total = total;

            return View(paged);
        }

        // GET: ProductController/Details/5
        [AllowAnonymous]
        public ActionResult Details(int id)
        {
            Product product = ProductRepository.GetById(id);
            if (product == null)
                return NotFound();
            return View(product);
        }

        // GET: ProductController/Create
        public ActionResult Create()
        {
            ViewBag.CategoryId = new SelectList(CategRepository.GetAll(), "CategoryId", "CategoryName");
            return View();
        }

        // POST: ProductController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateViewModel model)
        {
            ModelState.Remove("Category");
            ModelState.Remove("Price");   // le prix est saisi via PriceText

            bool priceOk = PriceHelper.TryParse(model.PriceText, out float parsedPrice);
            if (priceOk && parsedPrice <= 0)
                ModelState.AddModelError("PriceText", "Le prix doit être supérieur à 0");

            if (ModelState.IsValid && priceOk)
            {
                string uniqueFileName = null;

                if (model.ImagePath != null)
                {
                    string uploadsFolder = Path.Combine(hostingEnvironment.WebRootPath, "images");
                    uniqueFileName = Guid.NewGuid().ToString() + "_" + model.ImagePath.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        model.ImagePath.CopyTo(fileStream);
                    }
                }

                Product newProduct = new Product
                {
                    Name = model.Name,
                    Price = parsedPrice,
                    QteStock = model.QteStock,
                    CategoryId = model.CategoryId,
                    Image = uniqueFileName
                };

                ProductRepository.Add(newProduct);
                TempData["Success"] = "Produit ajouté avec succès";
                return RedirectToAction("Details", new { id = newProduct.ProductId });
            }
            ViewBag.CategoryId = new SelectList(CategRepository.GetAll(), "CategoryId", "CategoryName");
            return View(model);
        }

        // GET: ProductController/Edit/5
        public ActionResult Edit(int id, string? returnUrl = null)
        {
            ViewBag.CategoryId = new SelectList(CategRepository.GetAll(), "CategoryId", "CategoryName");
            Product product = ProductRepository.GetById(id);
            if (product == null)
                return NotFound();

            EditViewModel productEditViewModel = new EditViewModel
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Price = product.Price,
                PriceText = PriceHelper.Format(product.Price),
                QteStock = product.QteStock,
                CategoryId = product.CategoryId,
                ExistingImagePath = product.Image
            };
            ViewBag.ReturnUrl = returnUrl ?? Request.Headers["Referer"].ToString();
            return View(productEditViewModel);
        }

        // POST: ProductController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(EditViewModel model, string? returnUrl = null)
        {
            ModelState.Remove("Category");
            ModelState.Remove("ImagePath");
            ModelState.Remove("Price");   // le prix est saisi via PriceText
            ViewBag.CategoryId = new SelectList(CategRepository.GetAll(), "CategoryId", "CategoryName");
            ViewBag.ReturnUrl = returnUrl;

            bool priceOk = PriceHelper.TryParse(model.PriceText, out float parsedPrice);
            if (priceOk && parsedPrice <= 0)
                ModelState.AddModelError("PriceText", "Le prix doit être supérieur à 0");

            if (ModelState.IsValid && priceOk)
            {
                Product product = ProductRepository.GetById(model.ProductId);
                if (product == null)
                    return NotFound();

                product.Name = model.Name;
                product.Price = parsedPrice;
                product.QteStock = model.QteStock;
                product.CategoryId = model.CategoryId;

                if (model.ImagePath != null)
                {
                    if (model.ExistingImagePath != null)
                    {
                        string oldFilePath = Path.Combine(hostingEnvironment.WebRootPath, "images", model.ExistingImagePath);
                        if (System.IO.File.Exists(oldFilePath))
                            System.IO.File.Delete(oldFilePath);
                    }
                    product.Image = ProcessUploadedFile(model);
                }

                Product updatedProduct = ProductRepository.Update(product);
                if (updatedProduct != null)
                {
                    TempData["Success"] = "Produit modifié avec succès";
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        return LocalRedirect(returnUrl);
                    return RedirectToAction(nameof(Index));
                }
                else
                    return NotFound();
            }
            return View(model);
        }

        [NonAction]
        private string ProcessUploadedFile(EditViewModel model)
        {
            string uniqueFileName = null;
            if (model.ImagePath != null)
            {
                string uploadsFolder = Path.Combine(hostingEnvironment.WebRootPath, "images");
                uniqueFileName = Guid.NewGuid().ToString() + "_" + model.ImagePath.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    model.ImagePath.CopyTo(fileStream);
                }
            }
            return uniqueFileName;
        }

        // GET: ProductController/Search  (la barre du navbar passe par Index)
        [AllowAnonymous]
        public ActionResult Search(string val)
        {
            return RedirectToAction(nameof(Index), new { val });
        }

        // GET: ProductController/Delete/5
        public ActionResult Delete(int id, string? returnUrl = null)
        {
            Product product = ProductRepository.GetById(id);
            if (product == null)
                return NotFound();
            ViewBag.ReturnUrl = returnUrl ?? Request.Headers["Referer"].ToString();
            return View(product);
        }

        // POST: ProductController/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id, string? returnUrl = null)
        {
            ProductRepository.Delete(id);
            TempData["Success"] = "Produit supprimé avec succès";
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return LocalRedirect(returnUrl);
            return RedirectToAction(nameof(Index));
        }
    }
}