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
            ViewBag.Fecha = DateTime.Now.ToShortDateString();
            ViewBag.NumeroProforma= "N/A";
            ViewBag.IdUsuario = new SelectList(db.AspNetUsers.Where(x => x.Rol ==1 || x.Rol == 2 && x.Estado == true), "Id", "Nombre");
            ViewBag.IdEstado = new SelectList(db.EstadoPedido, "Id", "Descripcion");
            return View();
        }

        // POST: Pedido/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "IdUsuario,IdEstado,Fecha,TotalPagar,TotalDescuento,TotalImpuesto")] Pedidos pedidos)
        {
            pedidos.Fecha = DateTime.Now;
            pedidos.TotalDescuento = 0;
            pedidos.TotalImpuesto = 0;
            pedidos.TotalPagar = 0;
            if (ModelState.IsValid)
            {
                db.Pedidos.Add(pedidos);
                db.SaveChanges();
                Session["Pedido"] = "¡Pedido creado con éxito!";
                return RedirectToAction("Index");
            }

            ViewBag.IdUsuario = new SelectList(db.AspNetUsers.Where(x => x.Rol == 1 || x.Rol == 2 && x.Estado == true), "Id", "Nombre", pedidos.IdUsuario);
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

           

            ViewBag.TotalPagar = pedidos.TotalPagar;
            ViewBag.TotalDescuento = pedidos.TotalDescuento;
            ViewBag.TotalImpuesto = pedidos.TotalImpuesto;
            ViewBag.NumeroProforma = pedidos.NumeroProforma;
            ViewBag.Id = pedidos.Id;
            ViewBag.Fecha = pedidos.Fecha.ToShortDateString();
            List<PedidoDetalle> detalles = db.PedidoDetalle.Where(x => x.IdPedido == id).ToList();
            pedidos.PedidoDetalle = detalles;
            ViewBag.IdUsuario = new SelectList(db.AspNetUsers.Where(x => x.Rol == 1 || x.Rol == 2 && x.Estado == true), "Id", "Nombre", pedidos.IdUsuario);
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

            double descuento = 0;
            double desc = 0;
            double impuesto = 0;
            double imp = 0;
            double totalPagar = 0;
            List<PedidoDetalle> detalles = db.PedidoDetalle.Where(x => x.IdPedido == pedidos.Id).ToList();

            foreach (var item in detalles)
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

            if (ModelState.IsValid)
            {

                db.Entry(pedidos).State = EntityState.Modified;

                db.SaveChanges();
                Session["Pedido"] = "¡Pedido actualizado correctamente!";
                return RedirectToAction("Index");
            }

            ViewBag.Id = pedidos.Id;
            ViewBag.IdUsuario = new SelectList(db.AspNetUsers.Where(x => x.Rol == 1 || x.Rol == 2 && x.Estado== true), "Id", "Nombre", pedidos.IdUsuario);
            ViewBag.IdEstado = new SelectList(db.EstadoPedido, "Id", "Descripcion", pedidos.IdEstado);
            return View(pedidos);
        }




        public ActionResult DuplicarPedido(int? id)
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

            List<PedidoDetalle> detalles = db.PedidoDetalle.Where(x => x.IdPedido == id).ToList();
            pedidos.PedidoDetalle = detalles;


            Pedidos pedidoDuplicado = new Pedidos();
            pedidoDuplicado.IdUsuario = pedidos.IdUsuario;
            pedidoDuplicado.IdEstado = pedidos.IdEstado;
            pedidoDuplicado.Fecha = pedidos.Fecha;
            pedidoDuplicado.TotalPagar = pedidos.TotalPagar;
            pedidoDuplicado.TotalDescuento = pedidos.TotalDescuento;
            pedidoDuplicado.TotalImpuesto = pedidos.TotalImpuesto;
            pedidoDuplicado.NumeroProforma = pedidos.NumeroProforma;
            db.Pedidos.Add(pedidoDuplicado);
            db.SaveChanges();

            PedidoDetalle detalleDuplicado = new PedidoDetalle();

            foreach (var item in detalles)
            {
                detalleDuplicado.IdPedido = pedidoDuplicado.Id;
                detalleDuplicado.IdProducto = item.IdProducto;
                detalleDuplicado.Cantidad = item.Cantidad;
                detalleDuplicado.PrecioUnitario = item.PrecioUnitario;
                detalleDuplicado.Descuento = item.Descuento;
                detalleDuplicado.CantidadEnviada = item.CantidadEnviada;
                db.PedidoDetalle.Add(detalleDuplicado);
                db.SaveChanges();
                Session["Pedido"] = "¡Pedido duplicado correctamente!";
            }

            return RedirectToAction("Index");

            
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
                || x.EstadoPedido.Descripcion.Contains(terminoBusqueda) || x.Id == Convert.ToInt32(terminoBusqueda) && x.AspNetUsers.Rol == 2 || x.AspNetUsers.Rol == 1);
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


            List<PedidoDetalle> detallesPedido = db.PedidoDetalle.Where(x => x.IdPedido == IdPedido).ToList();
         
            pedidos.Id = pedidos.Id;
            pedidos.IdUsuario = pedidos.IdUsuario;
            pedidos.IdEstado = pedidos.IdEstado;
            pedidos.Fecha = pedidos.Fecha;

            foreach (var item in detallesPedido)
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
            pedidos.PedidoDetalle = detallesPedido;
            return PartialView("_ListaPedidoCarrito", pedidos);
    }



        public ActionResult CambiarCantidadEnviada(AjaxCambio objet)
        {
         
            int pedidoId = Convert.ToInt32(objet.pedidoId);
            int id = Convert.ToInt32(objet.productoId);
            int cantidad = Convert.ToInt32(objet.cantidadEnviada);
            double totalPagar = 0;
            double impuesto = 0;
            double descuento = 0;
            double desc = 0;
            double imp = 0;
            double total = 0;
            double impuestoVenta = 0;
            double descuentoCant = 0;
            double impuestoC = 0;
            double descCant = 0;
            Pedidos ped = db.Pedidos.Find(pedidoId);
            List<PedidoDetalle> detalles = db.PedidoDetalle.Where(x=> x.IdPedido == pedidoId).ToList();

            foreach (var item in detalles)
            {
                descuentoCant = (item.PrecioUnitario * item.Cantidad) * item.Descuento;
                descCant += descuentoCant;
                impuestoVenta = (item.PrecioUnitario * item.Cantidad) * (double)item.Productos.Impuesto / 100;
                impuestoC += impuestoVenta;
                total += ((item.PrecioUnitario * item.Cantidad) + impuestoVenta) - descuentoCant;
            
             }
                foreach (var item in detalles)
            {
                PedidoDetalle detalle = db.PedidoDetalle.Find(item.Id);
               

                if (item.IdProducto == id)
                {
                    if (cantidad > item.Cantidad)
                    {

                        ViewBag.TotalPagar = total;
                        ViewBag.TotalDescuento = descCant;
                        ViewBag.TotalImpuesto = impuestoC;
                        Session["Pedido"] = "Debe digitar una cantidad menor a la cantidad";
                        return PartialView("_ListaPedidoCarrito", ped);
                    }

                    detalle.Id = item.Id;
                    detalle.Cantidad = item.Cantidad;
                    detalle.IdPedido = item.IdPedido;
                    detalle.IdProducto = item.IdProducto;
                    detalle.PrecioUnitario = item.PrecioUnitario;
                    detalle.Descuento = item.Descuento;
                    detalle.CantidadEnviada = cantidad;
                    db.Entry(detalle).State = EntityState.Modified;
                    db.SaveChanges();
                    

                }
                {
                }
                descuento = (item.PrecioUnitario * item.Cantidad) * item.Descuento;
                desc += descuento;
                impuesto = (item.PrecioUnitario * item.Cantidad) * (double)item.Productos.Impuesto / 100;
                imp += impuesto;
                totalPagar += ((item.PrecioUnitario * item.Cantidad) + impuesto) - descuento;

            }

            ped.PedidoDetalle = detalles;
            ViewBag.TotalPagar = ped.TotalPagar;
            ViewBag.TotalDescuento = ped.TotalDescuento;
            ViewBag.TotalImpuesto = ped.TotalImpuesto;
            return PartialView("_ListaPedidoCarrito", ped);
        }


        public ActionResult CambiarCantidadPedida(Ajax objeto)
        {
            int idPedido = Convert.ToInt32(objeto.idPedido);
            int id = Convert.ToInt32(objeto.id);
            int cantidadCambio = Convert.ToInt32(objeto.terminoBusqueda);
            
            double totalPagar = 0;
            double impuesto = 0;
            double descuento = 0;
            double desc = 0;
            double imp = 0;
            double total = 0;
            double impuestoVenta = 0;
            double descuentoCant = 0;
            double impuestoC = 0;
            double descCant = 0;
            Pedidos pedidos = db.Pedidos.Find(idPedido);
            List<PedidoDetalle> detalles = db.PedidoDetalle.Where(x=> x.IdPedido == idPedido).ToList();

            foreach (var item in detalles)
            {
                descuentoCant = (item.PrecioUnitario * item.Cantidad) * item.Descuento;
                descCant += descuentoCant;
                impuestoVenta = (item.PrecioUnitario * item.Cantidad) * (double)item.Productos.Impuesto / 100;
                impuestoC += impuestoVenta;
                total += ((item.PrecioUnitario * item.Cantidad) + impuestoVenta) - descuentoCant;
            }
            foreach (var item in detalles)
            {
                
                if (item.IdProducto == id)
                {
                    PedidoDetalle detalle = db.PedidoDetalle.Find(item.Id);

                    if (cantidadCambio == 0)
                    {
                        pedidos.TotalDescuento = desc;
                        pedidos.TotalImpuesto = imp;
                        pedidos.TotalPagar = totalPagar;
                        db.Entry(pedidos).State = EntityState.Modified;
                        db.SaveChanges();
                        pedidos.PedidoDetalle = detalles;
                        ViewBag.TotalPagar = pedidos.TotalPagar;
                        ViewBag.TotalDescuento = pedidos.TotalDescuento;
                        ViewBag.TotalImpuesto = pedidos.TotalImpuesto;
                        db.PedidoDetalle.Remove(detalle);
                        db.SaveChanges();
                        return PartialView("_ListaPedidoCarrito", pedidos);
                    }

                    if (cantidadCambio < item.CantidadEnviada)
                    {
                     
                        ViewBag.TotalPagar = total;
                        ViewBag.TotalDescuento = descCant;
                        ViewBag.TotalImpuesto = impuestoC;
                        Session["Pedido"] = "Debe digitar una cantidad mayor a la cantidad enviada";
                 
                        return PartialView("_ListaPedidoCarrito", pedidos);
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
            pedidos.PedidoDetalle = detalles;

            ViewBag.TotalPagar = pedidos.TotalPagar;
            ViewBag.TotalDescuento = pedidos.TotalDescuento;
            ViewBag.TotalImpuesto = pedidos.TotalImpuesto;
            return PartialView("_ListaPedidoCarrito", pedidos);
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

            if(cant == 0)
            {
                ViewBag.TotalPagar = ped.TotalPagar;
                ViewBag.TotalDescuento = ped.TotalDescuento;
                ViewBag.TotalImpuesto = ped.TotalImpuesto;
                Session["Pedido"] = "Debe digitar una cantidad mayor a 0";
                return PartialView("_ListaPedidoCarrito", ped);
            }

            PedidoDetalle validacion = null;

            try
            {
                validacion = db.PedidoDetalle.Where(x => x.IdPedido == idPedido && x.IdProducto == nuevoId).First();
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
                    ViewBag.TotalPagar = ped.TotalPagar;
                    ViewBag.TotalDescuento = ped.TotalDescuento;
                    ViewBag.TotalImpuesto = ped.TotalImpuesto;
                    Session["Pedido"] = "Producto es inválido. Digite un código de producto nuevamente";
                    return PartialView("_ListaPedidoCarrito", ped);
                   
                }

            }

            List<PedidoDetalle> detallesPedido = db.PedidoDetalle.Where(x => x.IdPedido == idPedido).ToList();
            ped.PedidoDetalle = detallesPedido;

            foreach (var item in detallesPedido)
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
            return PartialView("_ListaPedidoCarrito", ped);

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

            public string idPedido
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

            public string pedidoId
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
