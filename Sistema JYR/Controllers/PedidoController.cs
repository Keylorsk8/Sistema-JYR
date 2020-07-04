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

            db.PedidoDetalle.Remove(detalles);
            db.SaveChanges();


            List<PedidoDetalle> det = db.PedidoDetalle.ToList();
            pedidos.Id = pedidos.Id;
            pedidos.IdUsuario = pedidos.IdUsuario;
            pedidos.IdEstado = pedidos.IdEstado;
            pedidos.Fecha = pedidos.Fecha;

            foreach (var item in det)
            {
                pedidos.TotalDescuento = (item.PrecioUnitario * item.Cantidad) * item.Descuento;
                double subtotal = item.PrecioUnitario * item.Cantidad;
                decimal impuesto =(decimal)item.Productos.Impuesto / 100;
                pedidos.TotalImpuesto = subtotal * (double)impuesto;
                pedidos.TotalPagar = (item.PrecioUnitario * item.Cantidad) + pedidos.TotalImpuesto - pedidos.TotalDescuento;
            }

           
                db.Entry(pedidos).State = EntityState.Modified;
                db.SaveChanges();


            ViewBag.TotalPagar = pedidos.TotalPagar;
            ViewBag.TotalDescuento = pedidos.TotalDescuento;
            ViewBag.TotalImpuesto = pedidos.TotalImpuesto;
            return PartialView("_ListaCarrito");
        }




        public ActionResult CambiarCantidad(Ajax objeto)
        {
            
            int id = Convert.ToInt32(objeto.id);
            int cantidad = Convert.ToInt32(objeto.terminoBusqueda);
            List<PedidoDetalle> detalles = db.PedidoDetalle.ToList();

           
            db.SaveChanges();
            foreach (var item in detalles)
            {    
                PedidoDetalle detalle = db.PedidoDetalle.Find(item.Id);
              
                if (cantidad == 0)
                {
                    db.PedidoDetalle.Remove(detalle);
                    db.SaveChanges();

                }
                if (item.IdProducto == id)
                {

                    detalle.Id = item.Id;
                    detalle.Cantidad = item.Cantidad;
                    detalle.IdPedido = item.IdPedido;
                    detalle.IdProducto = item.IdProducto;
                    detalle.PrecioUnitario = item.PrecioUnitario;
                    detalle.Descuento = item.Descuento;
                    detalle.CantidadEnviada = cantidad;
                }
                {
                }
                db.Entry(detalle).State = EntityState.Modified;
                db.SaveChanges();
            }

            return PartialView("_ListaCarrito"); 
        }


        public ActionResult CambiarCantidadPedida(Ajax objeto)
        {

            int id = Convert.ToInt32(objeto.id);
            int cantidad = Convert.ToInt32(objeto.terminoBusqueda);
            List<PedidoDetalle> detalles = db.PedidoDetalle.ToList();
         
            double totalPagar = 0;
            double impuesto = 0;
            double descuento = 0;

             foreach (var item in detalles)
            {
                Pedidos pedidos = db.Pedidos.Find(item.IdPedido);
                PedidoDetalle detalle = db.PedidoDetalle.Find(item.Id);
    
                if (cantidad == 0)
                    {
                        db.PedidoDetalle.Remove(detalle);
                        db.SaveChanges();

                    }
                    if (item.IdProducto == id)
                    {
                   
                    detalle.Id = item.Id;
                        detalle.Cantidad = cantidad;
                        detalle.IdPedido = item.IdPedido;
                        detalle.IdProducto = item.IdProducto;
                        detalle.PrecioUnitario = item.PrecioUnitario;
                        detalle.Descuento = item.Descuento;
                        detalle.CantidadEnviada = item.CantidadEnviada;
                        db.Entry(detalle).State = EntityState.Modified;
                        db.SaveChanges();

                    descuento += (item.PrecioUnitario * item.Cantidad) * item.Descuento;                
                    impuesto += (item.PrecioUnitario * item.Cantidad) * (double)item.Productos.Impuesto / 100;
                    totalPagar += (item.PrecioUnitario * detalle.Cantidad) + pedidos.TotalImpuesto - pedidos.TotalDescuento;
                  
                }
                pedidos.TotalDescuento = descuento;
                pedidos.TotalImpuesto = impuesto;
                pedidos.TotalPagar = totalPagar;
                db.Entry(pedidos).State = EntityState.Modified;
                db.SaveChanges();

                ViewBag.TotalPagar = pedidos.TotalPagar;
                ViewBag.TotalDescuento = pedidos.TotalDescuento;
                ViewBag.TotalImpuesto = pedidos.TotalImpuesto;
            }

           
            return PartialView("_ListaCarrito");
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
    }
}
