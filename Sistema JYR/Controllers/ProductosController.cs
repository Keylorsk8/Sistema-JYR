using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using Sistema_JYR.Models;
using System.IO;
using System.Drawing;
using System.Web.Helpers;


namespace Sistema_JYR.Controllers
{
    public class ProductosController : Controller
    {
        private SistemaJYREntities db = new SistemaJYREntities();

        // GET: Productos
        public ActionResult Index()
        {
            var productos = db.Productos.Include(p => p.CategoriasProducto);
            return View(productos.ToList());
        }

        // GET: Productos/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Productos productos = db.Productos.Find(id);
            if (productos == null)
            {
                return HttpNotFound();
            }
            return View(productos);
        }

        // GET: Productos/Create
        public ActionResult Create()
        {
            ViewBag.IdCategoria = new SelectList(db.CategoriasProducto, "Id", "Descripcion");
            return View();
        }

        // POST: Productos/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Nombre,Descripcion,UnidadDeMedida,Precio,CantidadEnInventario,IdCategoria,FechaVencimiento,Impuesto,Estado,imagen")] Productos producto)
        {
            HttpPostedFileBase FileBase = Request.Files[0];

                WebImage image = new WebImage(FileBase.InputStream);
                producto.imagen = image.GetBytes();

            
            if (ModelState.IsValid)
            {
                db.Productos.Add(producto);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.IdCategoria = new SelectList(db.CategoriasProducto, "Id", "Descripcion", producto.IdCategoria);
            return View(producto);
        }

        // GET: Productos/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Productos productos = db.Productos.Find(id);
            if (productos == null)
            {
                return HttpNotFound();
            }
            ViewBag.IdCategoria = new SelectList(db.CategoriasProducto, "Id", "Descripcion", productos.IdCategoria);
            return View(productos);
        }

        // POST: Productos/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Nombre,Descripcion,UnidadDeMedida,Precio,CantidadEnInventario,IdCategoria,FechaVencimiento,Impuesto,Estado")] Productos productos)
        {
            if (ModelState.IsValid)
            {
                db.Entry(productos).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.IdCategoria = new SelectList(db.CategoriasProducto, "Id", "Descripcion", productos.IdCategoria);
            return View(productos);
        }

        // GET: Productos/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Productos productos = db.Productos.Find(id);
            if (productos == null)
            {
                return HttpNotFound();
            }
            return View(productos);
        }

        // POST: Productos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Productos productos = db.Productos.Find(id);
            db.Productos.Remove(productos);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult filtrarProductosAjax(string terminoBusqueda)
        {
            if (terminoBusqueda != null)
            {
                var lista = db.Productos.Where(x => x.Nombre.Contains(terminoBusqueda));
                return PartialView("_ListaProductos", lista.ToList());
            }
            return View();
        }

        public ActionResult getImage(int id)
        {
            Productos pro = db.Productos.Find(id);
            byte[] byteImage = pro.imagen;

            System.IO.MemoryStream memory = new MemoryStream(byteImage);
            Image image = Image.FromStream(memory);

            memory = new MemoryStream();
            image.Save(memory, System.Drawing.Imaging.ImageFormat.Jpeg);
            memory.Position = 0;
            return File(memory, "image/jpg");
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
