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
        [Authorize(Roles = "Admin,Vendedor")]
        public ActionResult Index()
        {
            var productos = db.Productos.Include(p => p.CategoriasProducto);
            return View(productos.ToList());
        }

        //GET: Productos/ListaProductos
        public ActionResult ListaProductos()
        {
            var listaProductos = db.Productos.Where(x => x.Estado == true).ToList();
            return View(listaProductos);
        }

        [Authorize(Roles = "Admin,Vendedor")]
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
        [Authorize(Roles = "Admin,Vendedor")]
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
        public ActionResult Create([Bind(Include = "Nombre,Descripcion,UnidadDeMedida,Precio,CantidadEnInventario,IdCategoria,FechaVencimiento,Impuesto,Estado,imagen")] Productos producto)
        {
            HttpPostedFileBase FileBase = Request.Files[0];

            try
            {
                WebImage image = new WebImage(FileBase.InputStream);
                producto.imagen = image.GetBytes();
            }
            catch (Exception)
            {
                WebImage image = new WebImage("~/Content/imagenes/Sin_Imagen.jpg");
                producto.imagen = image.GetBytes();  
            }

            if (ModelState.IsValid)
            {
                db.Productos.Add(producto);
                db.SaveChanges();
                Session["Producto"] = "¡Producto Creado Correctamente!";
                return RedirectToAction("Index");
            }

            ViewBag.IdCategoria = new SelectList(db.CategoriasProducto, "Id", "Descripcion", producto.IdCategoria);
            return View(producto);
        }

        // GET: Productos/Edit/5
        [Authorize(Roles = "Admin,Vendedor")]
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
        public ActionResult Edit([Bind(Include = "Id,Nombre,Descripcion,UnidadDeMedida,Precio,CantidadEnInventario,IdCategoria,FechaVencimiento,Impuesto,Estado,imagen")] Productos productos)
        {
            byte[] imageActual = null;

            HttpPostedFileBase FileBase = Request.Files[0];

            if (FileBase.ContentLength == 0)
            {
                imageActual = db.Productos.SingleOrDefault(t => t.Id == productos.Id).imagen;
            }
            else
            {
                imageActual = new WebImage(FileBase.InputStream).GetBytes();

            }
            if (ModelState.IsValid)
            {
                Productos pro = db.Productos.Where(x => x.Id == productos.Id).First();
                pro.imagen = imageActual;
                pro.IdCategoria = productos.IdCategoria;
                pro.Impuesto = productos.Impuesto;
                pro.Nombre = productos.Nombre;
                pro.Precio = productos.Precio;
                pro.UnidadDeMedida = productos.UnidadDeMedida;
                pro.Estado = productos.Estado;
                db.Entry(pro).State = EntityState.Modified;
                db.SaveChanges();
                Session["Producto"] = "¡Información del producto actualizada correctamente!";
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

        public ActionResult FiltrarProductos(Ajax objeto)
        {
            int idProducto = Convert.ToInt32(objeto.productoId);
            string nombre = Convert.ToString(objeto.nombreProducto);
            int idCategoria = Convert.ToInt32(objeto.idCategoria);

            List<Productos> productos = new List<Productos>();
            List<Productos> productosFiltradaNombre = new List<Productos>();
            List<Productos> productosFiltradaId = new List<Productos>();

            if (idCategoria == 0)
            {
                productos = db.Productos.Where(x => x.Estado == true).ToList();
            }
            else
            {
                productos = db.Productos.Where(x => x.IdCategoria == idCategoria && x.Estado == true).ToList();
            }

            if (nombre != null)
            {
                foreach (var item in productos)
                {
                    if (item.Nombre.ToLower().Contains(nombre.ToLower()))
                    {
                        productosFiltradaNombre.Add(item);
                    }
                }
                productos = productosFiltradaNombre;
            }

            if (idProducto != 0)
            {
                foreach (var item in productos)
                {
                    if (item.Id == idProducto)
                    {
                        productosFiltradaId = new List<Productos>
                        {
                            item
                        };
                        break;
                    }
                }
                productos = productosFiltradaId;
            }

            return PartialView("_ListaProductosPartialView",productos);
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

        public class Ajax
        {
            public string productoId
            {
                get;
                set;
            }
            public string nombreProducto
            {
                get;
                set;
            }

            public string idCategoria
            {
                get;
                set;
            }
        }
    }
}
