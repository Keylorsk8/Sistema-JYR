using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Sistema_JYR.Models;
using WebGrease.Css.ImageAssemblyAnalysis.LogModel;

namespace Sistema_JYR.Controllers
{
    public class ProformasController : Controller
    {
        private readonly SistemaJYREntities db = new SistemaJYREntities();

        #region Metodos de Admin - Vendedor
        // GET: Proformas
        public ActionResult Index()
        {
            var proformas = db.Proformas.Where(x => x.AspNetUsers.Rol == 2 && x.IdEstado == 2 || x.AspNetUsers.Rol == 1 && x.IdEstado == 2).OrderByDescending(x => x.Id);
            return View(proformas.ToList());
        }

        // GET: Proformas/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                Session["Proforma"] = "Proforma inválida. Especifique una proforma";
                return RedirectToAction("Index");
            }

            Proformas proformas = db.Proformas.Find(id);
            if (proformas != null)
            {
                List<ProformaDetalle> detalles = db.ProformaDetalle.Where(x => x.IdProforma == id).ToList();
                proformas.ProformaDetalle = detalles;
                ViewBag.Id = proformas.Id;
            }
            if (proformas == null)
            {
                Session["Proforma"] = "No existe la proforma";
                return RedirectToAction("Index");
            }
            return View(proformas);
        }

        // GET: Proformas/Create
        public ActionResult Create()
        {
            ViewBag.Fecha = DateTime.Now.ToShortDateString();
            ViewBag.IdUsuario = new SelectList(db.AspNetUsers.Where(x => x.Rol == 1 || x.Rol == 2 && x.Estado == true), "Id", "Nombre");
            ViewBag.IdEstado = new SelectList(db.EstadoProforma.Where(x => x.Descripcion.Equals("Nueva")), "Id", "Descripcion");
            return View();
        }

        // POST: Proformas/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,IdUsuario,IdEstado,Fecha,TotalPagar,TotalDescuento,TotalImpuesto,IdCliente,NombreCliente,DireccionEntrega,NombreProforma")] Proformas proformas)
        {
            proformas.Fecha = DateTime.Now;
            proformas.TotalDescuento = 0;
            proformas.TotalImpuesto = 0;
            proformas.TotalPagar = 0;
            if (proformas.DireccionEntrega == null)
            {
                proformas.DireccionEntrega = "Retirar en la ferretería";
            }
            if (proformas.NombreProforma == null)
            {
                proformas.NombreProforma = "Proforma #";
            }
            if (ModelState.IsValid)
            {
                db.Proformas.Add(proformas);
                db.SaveChanges();
                Session["Proforma"] = "¡Proforma creada con éxito!";
                if (proformas.NombreProforma == "Proforma #")
                {
                    proformas.NombreProforma += proformas.Id;
                }
                db.Entry(proformas).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Id = proformas.Id;
            ViewBag.IdUsuario = new SelectList(db.AspNetUsers.Where(x => x.Rol == 1 || x.Rol == 2 && x.Estado == true), "Id", "Nombre", proformas.IdUsuario);
            ViewBag.IdEstado = new SelectList(db.EstadoProforma.Where(x => x.Descripcion.Equals("Nueva")), "Id", "Descripcion");
            return View(proformas);
        }

        // GET: Proformas/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                Session["Proforma"] = "Proforma inválida. Especifique una proforma";
                return RedirectToAction("Index");
            }
            Proformas proformas = db.Proformas.Find(id);
            if (proformas == null)
            {
                Session["Proforma"] = "No existe la proforma";
                return RedirectToAction("Index");
            }
            ViewBag.Id = proformas.Id;
            ViewBag.TotalPagar = proformas.TotalPagar;
            ViewBag.TotalDescuento = proformas.TotalDescuento;
            ViewBag.TotalImpuesto = proformas.TotalImpuesto;
            List<ProformaDetalle> detalles = db.ProformaDetalle.Where(x => x.IdProforma == id).ToList();
            proformas.ProformaDetalle = detalles;
            ViewBag.IdUsuario = new SelectList(db.AspNetUsers.Where(x => x.Rol == 2 || x.Rol == 1 && x.Estado == true), "Id", "Nombre", proformas.IdUsuario);
            ViewBag.IdEstado = new SelectList(db.EstadoProforma, "Id", "Descripcion", proformas.IdEstado);
            return View(proformas);
        }

        // POST: Proformas/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,IdUsuario,IdEstado,Fecha,TotalPagar,TotalDescuento,TotalImpuesto,IdCliente,NombreCliente,DireccionEntrega,NombreProforma")] Proformas proformas)
        {
            double descuento = 0;
            double desc = 0;
            double impuesto = 0;
            double imp = 0;
            double totalPagar = 0;
            List<ProformaDetalle> detalles = db.ProformaDetalle.Where(x => x.IdProforma == proformas.Id).ToList();
            foreach (var item in detalles)
            {
                descuento = (item.PrecioUnitario * item.Cantidad) * item.Descuento;
                desc += descuento;
                impuesto = (item.PrecioUnitario * item.Cantidad) * (double)item.Productos.Impuesto / 100;
                imp += impuesto;
                totalPagar += ((item.PrecioUnitario * item.Cantidad) + impuesto) - descuento;
            }
            proformas.TotalDescuento = desc;
            proformas.TotalImpuesto = imp;
            proformas.TotalPagar = totalPagar;
            if (ModelState.IsValid)
            {
                db.Entry(proformas).State = EntityState.Modified;
                db.SaveChanges();
                Session["Proforma"] = "¡Proforma actualizada correctamente!";
                return RedirectToAction("Index");
            }
            ViewBag.IdUsuario = new SelectList(db.AspNetUsers.Where(x => x.Rol == 2 || x.Rol == 1 && x.Estado == true), "Id", "Nombre", proformas.IdUsuario);
            ViewBag.IdEstado = new SelectList(db.EstadoProforma, "Id", "Descripcion", proformas.IdEstado);
            return View(proformas);
        }
        #endregion

        #region Metodos de Cliente
        public ActionResult ListaProformas(string idUser)
        {
            var proformas = db.Proformas.Where(x => x.IdUsuario == idUser && x.IdEstado == 2).OrderByDescending(x => x.Id);
            return View(proformas);
        }

        // GET: Proformas/Edit/5
        public ActionResult EditProformaCliente(int? id, string idUser)
        {
            if (id == null)
            {
                Session["Proforma"] = "Proforma inválida. Especifique una proforma";
                return RedirectToAction("Index");
            }
            var usuario = db.AspNetUsers.Where(x => x.Id == idUser).First();
            if (usuario.Rol != 3 )
            {
                Session["Proforma"] = "Proforma inválida. Especifique una proforma";
                return RedirectToAction("Index");
            }
            Proformas proformas = db.Proformas.Find(id);
            if (proformas == null)
            {
                Session["Proforma"] = "No existe la proforma";
                return RedirectToAction("Index");
            }
            ViewBag.Id = proformas.Id;
            ViewBag.TotalPagar = proformas.TotalPagar;
            ViewBag.TotalDescuento = proformas.TotalDescuento;
            ViewBag.TotalImpuesto = proformas.TotalImpuesto;
            List<ProformaDetalle> detalles = db.ProformaDetalle.Where(x => x.IdProforma == id).ToList();
            proformas.ProformaDetalle = detalles;
            ViewBag.IdUsuario = new SelectList(db.AspNetUsers.Where(x => x.Id == idUser), "Id", "Nombre");
            ViewBag.IdEstado = new SelectList(db.EstadoProforma.Where(x => x.Descripcion.Equals("Nueva")), "Id", "Descripcion");
            return View(proformas);
        }

        // POST: Proformas/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditProformaCliente([Bind(Include = "Id,IdUsuario,IdEstado,Fecha,TotalPagar,TotalDescuento,TotalImpuesto,IdCliente,NombreCliente,DireccionEntrega,NombreProforma")] Proformas proformas)
        {
            double descuento = 0;
            double desc = 0;
            double impuesto = 0;
            double imp = 0;
            double totalPagar = 0;
            List<ProformaDetalle> detalles = db.ProformaDetalle.Where(x => x.IdProforma == proformas.Id).ToList();
            foreach (var item in detalles)
            {
                descuento = (item.PrecioUnitario * item.Cantidad) * item.Descuento;
                desc += descuento;
                impuesto = (item.PrecioUnitario * item.Cantidad) * (double)item.Productos.Impuesto / 100;
                imp += impuesto;
                totalPagar += ((item.PrecioUnitario * item.Cantidad) + impuesto) - descuento;
            }
            proformas.TotalDescuento = desc;
            proformas.TotalImpuesto = imp;
            proformas.TotalPagar = totalPagar;
            if (ModelState.IsValid)
            {
                db.Entry(proformas).State = EntityState.Modified;
                db.SaveChanges();
                Session["Proforma"] = "¡Proforma actualizada correctamente!";
                return RedirectToAction("Index");
            }
            ViewBag.IdUsuario = new SelectList(db.AspNetUsers.Where(x => x.Id == proformas.IdCliente), "Id", "Nombre");
            ViewBag.IdEstado = new SelectList(db.EstadoProforma.Where(x => x.Descripcion.Equals("Nueva")), "Id", "Descripcion");
            return View(proformas);
        }

        /// <summary>
        /// Detalles de Proforma con usuario cliente
        /// </summary>
        /// <param name="id">Id de la Proforma</param>
        /// <param name="idCliente">Id del usuario en session</param>
        /// <returns></returns>
        public ActionResult DetailsProformaCliente(int? id, string idCliente)
        {
            if (id == null)
            {
                Session["Proforma"] = "Proforma inválida. Especifique una proforma";
                return RedirectToAction("ListaProformas");
            }
            var cliente = db.AspNetUsers.Where(x => x.Id == idCliente).First();
            if (cliente.Rol != 3)
            {
                Session["Proforma"] = "Proforma inválida. Especifique una proforma";
                return RedirectToAction("ListaProformas");
            }

            Proformas proformas = db.Proformas.Find(id);
            if (proformas != null)
            {
                if (proformas.IdUsuario == idCliente)
                {
                    List<ProformaDetalle> detalles = db.ProformaDetalle.Where(x => x.IdProforma == id).ToList();
                    proformas.ProformaDetalle = detalles;
                    ViewBag.Id = proformas.Id;
                }
                else
                {
                    Session["Proforma"] = "Proforma inválida. Especifique una proforma";
                    return RedirectToAction("ListaProformas");
                }
            }
            if (proformas == null)
            {
                Session["Proforma"] = "No existe la proforma";
                return RedirectToAction("ListaProformas");
            }
            return View(proformas);
        }

        // GET: Proformas/Create
        public ActionResult CreateProfromaCliente(string id)
        {
            ViewBag.Fecha = DateTime.Now.ToShortDateString();
            ViewBag.IdUsuario = new SelectList(db.AspNetUsers.Where(x => x.Id == id), "Id", "Nombre");
            ViewBag.IdEstado = new SelectList(db.EstadoProforma.Where(x => x.Descripcion.Equals("Nueva")), "Id", "Descripcion");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateProfromaCliente([Bind(Include = "IdUsuario,IdEstado,Fecha,TotalPagar,TotalDescuento,TotalImpuesto,IdCliente,NombreCliente,DireccionEntrega,NombreProforma")] Proformas proformas)
        {
            proformas.Fecha = DateTime.Now;
            proformas.TotalDescuento = 0;
            proformas.TotalImpuesto = 0;
            proformas.TotalPagar = 0;
            if (proformas.DireccionEntrega == null)
            {
                proformas.DireccionEntrega = "Retirar en la ferretería";
            }
            if (proformas.NombreProforma == null)
            {
                proformas.NombreProforma = "Proforma #";
            }
            if (ModelState.IsValid)
            {
                db.Proformas.Add(proformas);
                db.SaveChanges();
                Session["Proforma"] = "¡Proforma creada con éxito!";
                if (proformas.NombreProforma == "Proforma #")
                {
                    proformas.NombreProforma += proformas.Id;
                }
                db.Entry(proformas).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("ListaProformas", new { idUser = proformas.IdCliente });
            }
            ViewBag.Id = proformas.Id;
            ViewBag.IdUsuario = new SelectList(db.AspNetUsers.Where(x => x.Id == proformas.IdUsuario), "Id", "Nombre");
            ViewBag.IdEstado = new SelectList(db.EstadoProforma.Where(x => x.Descripcion.Equals("Nueva")), "Id", "Descripcion");
            return View(proformas);
        }
        #endregion

        public ActionResult DuplicarProforma(int? id)
        {
            if (id == null)
            {
                Session["Proforma"] = "Proforma inválida. Especifique una proforma";
                return RedirectToAction("Index");
            }
            Proformas proformas = db.Proformas.Find(id);
            if (proformas == null)
            {
                Session["Proforma"] = "No existe la proforma";
                return RedirectToAction("Index");
            }
            List<ProformaDetalle> detalles = db.ProformaDetalle.Where(x => x.IdProforma == id).ToList();
            proformas.ProformaDetalle = detalles;
            Proformas proformaDuplicado = new Proformas
            {
                IdUsuario = proformas.IdUsuario,
                IdEstado = proformas.IdEstado,
                Fecha = proformas.Fecha,
                TotalPagar = proformas.TotalPagar,
                TotalDescuento = proformas.TotalDescuento,
                TotalImpuesto = proformas.TotalImpuesto,
                IdCliente = proformas.IdCliente,
                NombreCliente = proformas.NombreCliente,
                DireccionEntrega = proformas.DireccionEntrega,
                NombreProforma = proformas.NombreProforma
            };
            db.Proformas.Add(proformaDuplicado);
            db.SaveChanges();
            ProformaDetalle detalleDuplicado = new ProformaDetalle();
            foreach (var item in detalles)
            {
                detalleDuplicado.IdProforma = proformaDuplicado.Id;
                detalleDuplicado.IdProducto = item.IdProducto;
                detalleDuplicado.Cantidad = item.Cantidad;
                detalleDuplicado.PrecioUnitario = item.PrecioUnitario;
                detalleDuplicado.Descuento = item.Descuento;
                db.ProformaDetalle.Add(detalleDuplicado);
                db.SaveChanges();
            }
            Session["Proforma"] = "¡Proforma duplicada correctamente!";
            return RedirectToAction("ListaProformas", new { idUser = proformas.IdCliente });
        }

        // GET: Proformas/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Proformas proformas = db.Proformas.Find(id);
            if (proformas == null)
            {
                return HttpNotFound();
            }
            return View(proformas);
        }

        // POST: Proformas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Proformas proformas = db.Proformas.Find(id);
            db.Proformas.Remove(proformas);
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

        public ActionResult EliminarProdProforma(int? idD, int IdProforma)
        {
            Proformas proforma = db.Proformas.Find(IdProforma);
            ProformaDetalle detalles = db.ProformaDetalle.Find(idD);
            var prod = db.Productos.Where(x => x.Id == detalles.IdProducto);
            double totalPagar = 0;
            double impuesto = 0;
            double descuento = 0;
            double desc = 0;
            double imp = 0;
            db.ProformaDetalle.Remove(detalles);
            db.SaveChanges();
            List<ProformaDetalle> detallesPedido = db.ProformaDetalle.Where(x => x.IdProforma == IdProforma).ToList();
            proforma.Id = proforma.Id;
            proforma.IdUsuario = proforma.IdUsuario;
            proforma.IdEstado = proforma.IdEstado;
            proforma.Fecha = proforma.Fecha;
            foreach (var item in detallesPedido)
            {
                descuento = (item.PrecioUnitario * item.Cantidad) * item.Descuento;
                desc += descuento;
                impuesto = (item.PrecioUnitario * item.Cantidad) * (double)item.Productos.Impuesto / 100;
                imp += impuesto;
                totalPagar += ((item.PrecioUnitario * item.Cantidad) + impuesto) - descuento;
            }
            proforma.TotalDescuento = desc;
            proforma.TotalImpuesto = imp;
            proforma.TotalPagar = totalPagar;
            db.Entry(proforma).State = EntityState.Modified;
            db.SaveChanges();
            ViewBag.TotalPagar = proforma.TotalPagar;
            ViewBag.TotalDescuento = proforma.TotalDescuento;
            ViewBag.TotalImpuesto = proforma.TotalImpuesto;
            proforma.ProformaDetalle = detallesPedido;
            return PartialView("_ListaProformaCarrito", proforma);
        }

        //Cambiar la cantidad del producto
        public ActionResult CambiarCantidadPedida(Ajax objeto)
        {
            int idProforma = Convert.ToInt32(objeto.idProforma);
            int id = Convert.ToInt32(objeto.id);
            int cantidadCambio = Convert.ToInt32(objeto.terminoBusqueda);
            double totalPagar = 0;
            double impuesto = 0;
            double descuento = 0;
            double desc = 0;
            double imp = 0;
            Proformas proforma = db.Proformas.Find(idProforma);
            List<ProformaDetalle> detalles = db.ProformaDetalle.Where(x => x.IdProforma == idProforma).ToList();
            foreach (var item in detalles)
            {
                if (item.IdProducto == id)
                {
                    ProformaDetalle detalle = db.ProformaDetalle.Find(item.Id);
                    if (cantidadCambio == 0)
                    {
                        proforma.TotalDescuento = desc;
                        proforma.TotalImpuesto = imp;
                        proforma.TotalPagar = totalPagar;
                        db.Entry(proforma).State = EntityState.Modified;
                        db.SaveChanges();
                        proforma.ProformaDetalle = detalles;
                        ViewBag.TotalPagar = proforma.TotalPagar;
                        ViewBag.TotalDescuento = proforma.TotalDescuento;
                        ViewBag.TotalImpuesto = proforma.TotalImpuesto;
                        db.ProformaDetalle.Remove(detalle);
                        db.SaveChanges();
                        return PartialView("_ListaProformaCarrito", proforma);
                    }
                    detalle.Id = item.Id;
                    detalle.Cantidad = cantidadCambio;
                    detalle.IdProforma = item.IdProforma;
                    detalle.IdProducto = item.IdProducto;
                    detalle.PrecioUnitario = item.PrecioUnitario;
                    detalle.Descuento = item.Descuento;
                    db.Entry(detalle).State = EntityState.Modified;
                    db.SaveChanges();
                }
                descuento = (item.PrecioUnitario * item.Cantidad) * item.Descuento;
                desc += descuento;
                impuesto = (item.PrecioUnitario * item.Cantidad) * (double)item.Productos.Impuesto / 100;
                imp += impuesto;
                totalPagar += ((item.PrecioUnitario * item.Cantidad) + impuesto) - descuento;
            }
            proforma.TotalDescuento = desc;
            proforma.TotalImpuesto = imp;
            proforma.TotalPagar = totalPagar;
            db.Entry(proforma).State = EntityState.Modified;
            db.SaveChanges();
            proforma.ProformaDetalle = detalles;
            ViewBag.TotalPagar = proforma.TotalPagar;
            ViewBag.TotalDescuento = proforma.TotalDescuento;
            ViewBag.TotalImpuesto = proforma.TotalImpuesto;
            return PartialView("_ListaProformaCarrito", proforma);
        }

        //Agregar un nuevo producto en la página editar
        public ActionResult AgregarDetalle(AjaxDetalle objeto)
        {
            int idProforma = Convert.ToInt32(objeto.idProforma);
            int nuevoId = Convert.ToInt32(objeto.idProducto);
            int cant = Convert.ToInt32(objeto.cantidad);
            Proformas ped = db.Proformas.Find(idProforma);
            double totalPagar = 0;
            double impuesto = 0;
            double descuento = 0;
            double desc = 0;
            double imp = 0;
            if (cant == 0)
            {
                ViewBag.TotalPagar = ped.TotalPagar;
                ViewBag.TotalDescuento = ped.TotalDescuento;
                ViewBag.TotalImpuesto = ped.TotalImpuesto;
                Session["Proforma"] = "Debe digitar una cantidad mayor a 0";
                return PartialView("_ListaProformaCarrito", ped);
            }
            ProformaDetalle validacion = null;
            try
            {
                validacion = db.ProformaDetalle.Where(x => x.IdProforma == idProforma && x.IdProducto == nuevoId).First();
            }
            catch (Exception)
            {
                validacion = null;
            }
            if (validacion != null)
            {
                validacion.Cantidad += cant;
                db.Entry(validacion).State = EntityState.Modified;
                db.SaveChanges();
            }
            else
            {
                try
                {
                    ProformaDetalle detalle = new ProformaDetalle
                    {
                        IdProforma = idProforma,
                        IdProducto = nuevoId
                    };
                    detalle.Productos = db.Productos.Where(x => x.Id == detalle.IdProducto).First();
                    detalle.Cantidad = cant;
                    detalle.PrecioUnitario = detalle.Productos.Precio;
                    detalle.Descuento = 0;
                    db.ProformaDetalle.Add(detalle);
                    db.SaveChanges();
                }
                catch (Exception)
                {
                    ViewBag.TotalPagar = ped.TotalPagar;
                    ViewBag.TotalDescuento = ped.TotalDescuento;
                    ViewBag.TotalImpuesto = ped.TotalImpuesto;
                    Session["Proforma"] = "Producto es inválido. Digite un código de producto nuevamente";
                    return PartialView("_ListaProformaCarrito", ped);
                }
            }
            List<ProformaDetalle> detallesProforma = db.ProformaDetalle.Where(x => x.IdProforma == idProforma).ToList();
            foreach (var item in detallesProforma)
            {
                descuento = (item.PrecioUnitario * item.Cantidad) * item.Descuento;
                desc += descuento;
                impuesto = (item.PrecioUnitario * item.Cantidad) * (double)item.Productos.Impuesto / 100;
                imp += impuesto;
                totalPagar += ((item.PrecioUnitario * item.Cantidad) + impuesto) - descuento;
            }
            ped.TotalDescuento = desc;
            ped.TotalImpuesto = imp;
            ped.TotalPagar = totalPagar;
            db.Entry(ped).State = EntityState.Modified;
            db.SaveChanges();
            ped.ProformaDetalle = detallesProforma;
            ViewBag.TotalPagar = ped.TotalPagar;
            ViewBag.TotalDescuento = ped.TotalDescuento;
            ViewBag.TotalImpuesto = ped.TotalImpuesto;
            return PartialView("_ListaProformaCarrito", ped);
        }

        //Filtros de la página
        public ActionResult filtrarProformasAjax(string terminoBusqueda)
        {
            if (terminoBusqueda != null)
            {
                var lista = db.Proformas.Where(x => x.AspNetUsers.Nombre.Contains(terminoBusqueda)
                || x.EstadoProforma.Descripcion.Contains(terminoBusqueda) || x.Id == Convert.ToInt32(terminoBusqueda) && x.AspNetUsers.Rol == 2 || x.AspNetUsers.Rol == 1);
                return PartialView("_ListaProformas", lista.ToList());
            }
            return View();
        }

        /// <summary>
        /// Busqueda avanzada de Usuario para nueva proforma
        /// </summary>
        /// <param name="cedula">Identicación del usuario</param>
        /// <returns></returns>
        public ActionResult filtrarCliente(string cedula)
        {
            try
            {
                int ced = Convert.ToInt32(cedula);
                AspNetUsers usuario = db.AspNetUsers.Where(x => x.Cedula == ced && x.Rol == 3).First();
                return PartialView("_Cliente", usuario);
            }
            catch (InvalidOperationException e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Retorna la vista para clientes no registrados creando una nueva proforma
        /// </summary>
        /// <returns></returns>
        public ActionResult ClienteSinCuenta()
        {
            return PartialView("_ClienteSinCuenta");
        }

        /// <summary>
        /// Convierte una proforma en un pedido
        /// </summary>
        /// <returns></returns>
        public ActionResult ConvertiraPedido(int? id)
        {
            if (id == null)
            {
                Session["Proforma"] = "Proforma inválida. Especifique una proforma";
                return RedirectToAction("Index");
            }
            Proformas proformas = db.Proformas.Find(id);
            if (proformas == null)
            {
                Session["Proforma"] = "No existe la proforma";
                return RedirectToAction("Index");
            }
            List<ProformaDetalle> detalles = db.ProformaDetalle.Where(x => x.IdProforma == id).ToList();
            proformas.ProformaDetalle = detalles;
            Pedidos pedido = new Pedidos
            {
                IdUsuario = proformas.IdUsuario,
                IdEstado = 7,
                Fecha = DateTime.Now,
                TotalPagar = proformas.TotalPagar,
                TotalDescuento = proformas.TotalDescuento,
                TotalImpuesto = proformas.TotalImpuesto,
                IdCliente = proformas.IdCliente,
                NombreCliente = proformas.NombreCliente,
                DireccionEntrega = proformas.DireccionEntrega,
                NombrePedido = proformas.NombreProforma,
                NumeroProforma = proformas.Id
            };
            db.Pedidos.Add(pedido);
            db.SaveChanges();
            PedidoDetalle pedidoDetalle = new PedidoDetalle();
            foreach (var item in detalles)
            {
                pedidoDetalle.IdPedido = pedido.Id;
                pedidoDetalle.IdProducto = item.IdProducto;
                pedidoDetalle.Cantidad = item.Cantidad;
                pedidoDetalle.PrecioUnitario = item.PrecioUnitario;
                pedidoDetalle.Descuento = item.Descuento;
                db.PedidoDetalle.Add(pedidoDetalle);
                db.SaveChanges();
            }
            Session["Proforma"] = "¡Proforma convertida en Pedido exitosamente!";
            Session["NumPedido"] = pedido.Id;

            proformas.IdEstado = 4;
            db.Entry(proformas).State = EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        public class Ajax
        {
            public string terminoBusqueda
            {
                get;
                set;
            }
            public string id
            {
                get;
                set;
            }

            public string idProforma
            {
                get;
                set;
            }
        }
        public class AjaxDetalle
        {
            public string cantidad
            {
                get;
                set;
            }
            public string idProforma
            {
                get;
                set;
            }

            public string idProducto
            {
                get;
                set;
            }
        }

        public ActionResult reporteProformaAPedido(string fechaAnterior, string fechaActual)
        {
            if (fechaAnterior != null && fechaActual != null)
            {
                DateTime anterior = Convert.ToDateTime(fechaAnterior);
                DateTime actual = Convert.ToDateTime(fechaActual);

                if (anterior.Year > actual.Year)
                {
                    Session["Proforma"] = "Seleccione una fecha válida";
                    TempData["mensajeReporte"] = "Seleccione una fecha inválida";

                }
                else
                {
                    if (anterior.Year == actual.Year && anterior.Month > actual.Month)
                    {
                        Session["Proforma"] = "Seleccione una fecha válida";
                        TempData["mensajeReporte"] = "Seleccione una fecha inválida";

                    }
                    else
                    {
                        if (anterior.Year == actual.Year && anterior.Month == actual.Month && anterior.Day > actual.Day)
                        {
                            Session["Proforma"] = "Seleccione una fecha válida";
                            TempData["mensajeReporte"] = "Seleccione una fecha inválida";

                        }

                        else
                        {
                            if (actual > DateTime.Now)
                            {
                                Session["Proforma"] = "Seleccione una fecha válida";
                                TempData["mensajeReporte"] = "Seleccione una fecha inválida";
                            }

                            else
                            {



                                var querys = from p in db.Proformas
                                             join e in db.EstadoProforma on p.IdEstado equals e.Id
                                             where p.Fecha >= anterior && p.Fecha <= actual
                                             group new { p.IdEstado, p.TotalPagar, p.TotalImpuesto, p.TotalDescuento }
                                             by new { e.Descripcion, e.Id, p.IdEstado }
                                             into g
                                             orderby g.Sum(x=> x.TotalPagar)

                                             select new
                                             {
                                                 TotalPagar = g.Sum(x => x.TotalPagar),
                                                 TotalDescuento = g.Sum(x => x.TotalDescuento),
                                                 TotalImpuesto = g.Sum(x => x.TotalImpuesto),
                                                 Cantidad = g.Count(),
                                                 Estado = g.Key.Descripcion,                                              
                                                 Porcentaje = g.Count() * 100 / db.Proformas.Count()
                                             };

                           
                                ViewBag.ReportViewer = Reporte.reporte(querys.ToList(), "", "ReporteProformaAPedido.rdlc");


                                return PartialView("_reporteProformaAPedido", querys.ToList());
                            }


                        }
                    }
                }
                if (TempData.ContainsKey("mensajeReporte"))
                {

                    return PartialView("_aviso");

                }

            }

            return View();
        }
    }
}
