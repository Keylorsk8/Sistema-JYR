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
    public class PedidoController : Controller
    {
        private SistemaJYREntities db = new SistemaJYREntities();

        // GET: Pedido

        public ActionResult Index()
        {

            var pedidos = db.Pedidos.Include(p => p.EstadoPedido);
            return View(pedidos.ToList());
        }

        // GET: Pedido/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                Session["Pedido"] = "Pedido inválido. Especifique un pedido";

                return RedirectToAction("Index");
            }

            Pedidos pedido = db.Pedidos.Find(id);
            if (pedido != null)
            {
                List<PedidoDetalle> detalles = db.PedidoDetalle.Where(x => x.IdPedido == id).ToList();
                pedido.PedidoDetalle = detalles;
                ViewBag.Id = pedido.Id;
            }


            if (pedido == null)
            {
                Session["Pedido"] = "No existe el pedido";
                return RedirectToAction("Index");
            }


            return View(pedido);
        }

        // GET: Pedido/Create
        public ActionResult Create()
        {
            ViewBag.IdUsuario = new SelectList(db.AspNetUsers, "Id", "Nombre");
            ViewBag.IdEstado = new SelectList(db.EstadoPedido, "Id", "Descripcion");
            return View();
        }

        // POST: Pedido/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,IdUsuario,IdEstado,Fecha,TotalPagar,TotalDescuento,TotalImpuesto")] Pedidos pedidos)
        {
            if (ModelState.IsValid)
            {
                db.Pedidos.Add(pedidos);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.IdUsuario = new SelectList(db.AspNetUsers, "Id", "Nombre", pedidos.IdUsuario);
            ViewBag.IdEstado = new SelectList(db.EstadoPedido, "Id", "Descripcion", pedidos.IdEstado);
            return View(pedidos);
        }

        // GET: Pedido/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                Session["Pedido"] = "Pedido inválido. Especifique un pedido";
                return RedirectToAction("Index");
            }

            Pedidos pedidos = db.Pedidos.Find(id);
            if (pedidos == null)
            {
                Session["Pedido"] = "No existe el pedido";
                return RedirectToAction("Index");
            }

            if(pedidos.NumeroProforma == null)
            {
                ViewBag.NumeroProforma = "N/A";
            }

            else
            {
                ViewBag.NumeroProforma = pedidos.NumeroProforma;
            }

            ViewBag.TotalPagar = pedidos.TotalPagar;
            ViewBag.TotalDescuento = pedidos.TotalDescuento;
            ViewBag.TotalImpuesto = pedidos.TotalImpuesto;
            ViewBag.NumeroProforma = pedidos.NumeroProforma;
            ViewBag.Id = pedidos.Id;
            ViewBag.Fecha = pedidos.Fecha;
            List<PedidoDetalle> detalles = db.PedidoDetalle.Where(x => x.IdPedido == id).ToList();
            pedidos.PedidoDetalle = detalles;
            ViewBag.IdUsuario = new SelectList(db.AspNetUsers, "Id", "Nombre", pedidos.IdUsuario);
            ViewBag.IdEstado = new SelectList(db.EstadoPedido, "Id", "Descripcion", pedidos.IdEstado);
            return View(pedidos);
        }

        // POST: Pedido/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,IdUsuario,IdEstado,Fecha,TotalPagar,TotalDescuento,TotalImpuesto, NumeroProforma")] Pedidos pedidos)
        {
            if (ModelState.IsValid)
            {

                db.Entry(pedidos).State = EntityState.Modified;

                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.IdUsuario = new SelectList(db.AspNetUsers, "Id", "Nombre", pedidos.IdUsuario);

            ViewBag.IdEstado = new SelectList(db.EstadoPedido, "Id", "Descripcion", pedidos.IdEstado);
            return View(pedidos);
        }

        // GET: Pedido/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Pedidos pedidos = db.Pedidos.Find(id);
            if (pedidos == null)
            {
                return HttpNotFound();
            }
            return View(pedidos);
        }

        // POST: Pedido/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Pedidos pedidos = db.Pedidos.Find(id);
            db.Pedidos.Remove(pedidos);
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

        public ActionResult filtrarPedidosAjax(string terminoBusqueda)
        {
            if (terminoBusqueda != null)
            {
                var lista = db.Pedidos.Where(x => x.AspNetUsers.Nombre.Contains(terminoBusqueda)
                || x.EstadoPedido.Descripcion.Contains(terminoBusqueda) && x.AspNetUsers.Rol == 2 || x.AspNetUsers.Rol == 1);
                return PartialView("_ListaPedidos", lista.ToList());
            }


            return View();
        }

        public ActionResult EliminarCarrito(int? idD, int IdPedido)
        {
            Pedidos pedidos = db.Pedidos.Find(IdPedido);
            PedidoDetalle detalles = db.PedidoDetalle.Find(idD);
            var prod = db.Productos.Where(x => x.Id == detalles.IdProducto);
            double totalPagar = 0;
            double impuesto = 0;
            double descuento = 0;
            double desc = 0;
            double imp = 0;

            db.PedidoDetalle.Remove(detalles);
            db.SaveChanges();


            List<PedidoDetalle> det = db.PedidoDetalle.ToList();
            pedidos.Id = pedidos.Id;
            pedidos.IdUsuario = pedidos.IdUsuario;
            pedidos.IdEstado = pedidos.IdEstado;
            pedidos.Fecha = pedidos.Fecha;

            foreach (var item in det)
            {

                descuento = (item.PrecioUnitario * item.Cantidad) * item.Descuento;
                desc += descuento;
                impuesto = (item.PrecioUnitario * item.Cantidad) * (double)item.Productos.Impuesto / 100;
                imp += impuesto;
                totalPagar += ((item.PrecioUnitario * item.Cantidad) + impuesto) - descuento;

            
            }

            pedidos.TotalDescuento = desc;
            pedidos.TotalImpuesto = imp;
            pedidos.TotalPagar = totalPagar;
            db.Entry(pedidos).State = EntityState.Modified;
            db.SaveChanges();

            ViewBag.TotalPagar = pedidos.TotalPagar;
            ViewBag.TotalDescuento = pedidos.TotalDescuento;
            ViewBag.TotalImpuesto = pedidos.TotalImpuesto;
            db.Entry(pedidos).State = EntityState.Modified;
            db.SaveChanges();


            ViewBag.TotalPagar = pedidos.TotalPagar;
            ViewBag.TotalDescuento = pedidos.TotalDescuento;
            ViewBag.TotalImpuesto = pedidos.TotalImpuesto;
            return PartialView("_ListaPedidoCarrito");
    }



        public ActionResult CambiarCantidadEnviada(AjaxCambio objet)
        {

            int id = Convert.ToInt32(objet.productoId);
            int cantidad = Convert.ToInt32(objet.cantidadEnviada);
            List<PedidoDetalle> detalles = db.PedidoDetalle.ToList();


            foreach (var item in detalles)
            {
                PedidoDetalle detalle = db.PedidoDetalle.Find(item.Id);

                Pedidos ped = db.Pedidos.Find(item.IdPedido);
         
                if (item.IdProducto == id)
                {

                    detalle.Id = item.Id;
                    detalle.Cantidad = item.Cantidad;
                    detalle.IdPedido = item.IdPedido;
                    detalle.IdProducto = item.IdProducto;
                    detalle.PrecioUnitario = item.PrecioUnitario;
                    detalle.Descuento = item.Descuento;
                    detalle.CantidadEnviada = cantidad;
                    db.Entry(detalle).State = EntityState.Modified;
                    db.SaveChanges();
                    ViewBag.TotalPagar = ped.TotalPagar;
                    ViewBag.TotalDescuento = ped.TotalDescuento;
                    ViewBag.TotalImpuesto = ped.TotalImpuesto;

                }
                {
                }
               
            }

            return PartialView("_ListaPedidoCarrito");
        }


        public ActionResult CambiarCantidadPedida(Ajax objeto)
        {

            int id = Convert.ToInt32(objeto.id);
            int cantidadCambio = Convert.ToInt32(objeto.terminoBusqueda);
            List<PedidoDetalle> detalles = db.PedidoDetalle.ToList();
            double totalPagar = 0;
            double impuesto = 0;
            double descuento = 0;
            double desc = 0;
            double imp = 0;
            Pedidos pedidos = null;
            foreach (var item in detalles)
            {
                pedidos = db.Pedidos.Find(item.IdPedido);
                if (item.IdProducto == id)
                {
                    PedidoDetalle detalle = db.PedidoDetalle.Find(item.Id);
                    if (cantidadCambio == 0)
                    {
                        db.PedidoDetalle.Remove(detalle);

                    }

                    detalle.Id = item.Id;
                    detalle.Cantidad = cantidadCambio;
                    detalle.IdPedido = item.IdPedido;
                    detalle.IdProducto = item.IdProducto;
                    detalle.PrecioUnitario = item.PrecioUnitario;
                    detalle.Descuento = item.Descuento;
                    detalle.CantidadEnviada = item.CantidadEnviada;
                    db.Entry(detalle).State = EntityState.Modified;
                    db.SaveChanges();

                }
                descuento = (item.PrecioUnitario * item.Cantidad) * item.Descuento;
                desc += descuento;
                impuesto = (item.PrecioUnitario * item.Cantidad) * (double)item.Productos.Impuesto / 100;
                imp += impuesto;
                totalPagar += ((item.PrecioUnitario * item.Cantidad) + impuesto) - descuento;
            }

            pedidos.TotalDescuento = desc;
            pedidos.TotalImpuesto = imp;
            pedidos.TotalPagar = totalPagar;
            db.Entry(pedidos).State = EntityState.Modified;
            db.SaveChanges();

            ViewBag.TotalPagar = pedidos.TotalPagar;
            ViewBag.TotalDescuento = pedidos.TotalDescuento;
            ViewBag.TotalImpuesto = pedidos.TotalImpuesto;
            return PartialView("_ListaPedidoCarrito");
        }


        public ActionResult AgregarDetalle(AjaxDetalle objeto)
        {
            int idPedido = Convert.ToInt32(objeto.idPedido);
            int nuevoId = Convert.ToInt32(objeto.idProducto);
            int cant = Convert.ToInt32(objeto.cantidad);


            Pedidos ped = db.Pedidos.Find(idPedido);
            double totalPagar = 0;
            double impuesto = 0;
            double descuento = 0;
            double desc = 0;
            double imp = 0;

            var validacion = db.PedidoDetalle.Where(x => x.IdPedido == idPedido && x.IdProducto == nuevoId).First();

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

                    PedidoDetalle detalle = new PedidoDetalle();

                    detalle.IdPedido = idPedido;
                    detalle.IdProducto = nuevoId;
                    detalle.Productos = db.Productos.Where(x => x.Id == detalle.IdProducto).First();
                    detalle.Cantidad = cant;
                    detalle.CantidadEnviada = 0;
                    detalle.PrecioUnitario = detalle.Productos.Precio;
                    detalle.Descuento = 0;
                    db.PedidoDetalle.Add(detalle);
                    db.SaveChanges();
                    
                }
                catch (Exception)
                {

                    throw;
                }

            }
           
            List<PedidoDetalle> pedD = db.PedidoDetalle.ToList();

            foreach (var item in pedD)
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

            ViewBag.TotalPagar = ped.TotalPagar;
            ViewBag.TotalDescuento = ped.TotalDescuento;
            ViewBag.TotalImpuesto = ped.TotalImpuesto;
            return PartialView("_ListaPedidoCarrito");

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


        }

        public class AjaxCambio
        {
            public string productoId
            {
                get;
                set;
            }
            public string cantidadEnviada
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
            public string idPedido
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
    }
}
