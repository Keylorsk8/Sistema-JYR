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
        public ActionResult Index()
        {
            return View(db.CategoriasProducto.ToList());
        }

        // GET: CategoriasProducto/Details/5
        public ActionResult Details(int? id)
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

        // GET: CategoriasProducto/Create
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
                return RedirectToAction("Index");
            }

            return View(categoriasProducto);
        }

        // GET: CategoriasProducto/Edit/5
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
                return RedirectToAction("Index");
            }
            return View(categoriasProducto);
        }

        // GET: CategoriasProducto/Delete/5
        public ActionResult Delete(int? id)
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

        // POST: CategoriasProducto/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            CategoriasProducto categoriasProducto = db.CategoriasProducto.Find(id);
            db.CategoriasProducto.Remove(categoriasProducto);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
        public ActionResult filtrarCategoriasAjax(string terminoBusqueda)
        {
            if (terminoBusqueda != null)
            {
                var lista = db.CategoriasProducto.Where(x => x.Descripcion.Contains(terminoBusqueda)
               );
                return PartialView("_ListaCategorias", lista.ToList());
            }
            return View();
        }
    }
}
