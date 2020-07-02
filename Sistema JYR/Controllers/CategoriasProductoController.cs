 using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Sistema_JYR.Models;

namespace Sistema_JYR.Controllers
{
    public class CategoriasProductoController : Controller
    {
        private SistemaJYREntities db = new SistemaJYREntities();

        // GET: CategoriasProducto
        [Authorize(Roles = "Admin")]
        public ActionResult Index()
        {
            return View(db.CategoriasProducto.ToList());
        }

        // GET: CategoriasProducto/Create
        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: CategoriasProducto/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Descripcion")] CategoriasProducto categoriasProducto)
        {
            if (ModelState.IsValid)
            {
                db.CategoriasProducto.Add(categoriasProducto);
                db.SaveChanges();
                Session["Categoria"] = "¡La Categoria ha sido creada con éxito!";
                return RedirectToAction("Index");
            }

            return View(categoriasProducto);
        }

        // GET: CategoriasProducto/Edit/5
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CategoriasProducto categoriasProducto = db.CategoriasProducto.Find(id);
            if (categoriasProducto == null)
            {
                return HttpNotFound();
            }
            return View(categoriasProducto);
        }

        // POST: CategoriasProducto/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Descripcion")] CategoriasProducto categoriasProducto)
        {
            if (ModelState.IsValid)
            {
                db.Entry(categoriasProducto).State = EntityState.Modified;
                db.SaveChanges();
                Session["Categoria"] = "La Categoria ha sido actualizada correctamente!";
                return RedirectToAction("Index");
            }
            return View(categoriasProducto);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
