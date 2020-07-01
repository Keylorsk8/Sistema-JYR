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
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Pedidos pedido = db.Pedidos.Find(id);

           List<PedidoDetalle> detalles = db.PedidoDetalle.Where(x => x.IdPedido == id).ToList();
            pedido.PedidoDetalle = detalles;
            if (pedido == null)
            {
                return HttpNotFound();
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
        public ActionResult Create([Bind(Include = "Id,IdUsuario,IdEstado,Fecha,TotalPagar,TotalDescuento,TotalImpuesto,NumeroProforma")] Pedidos pedidos)
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
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
          
            Pedidos pedidos = db.Pedidos.Find(id);
            if (pedidos == null)
            {
                return HttpNotFound();
            }
            ViewBag.IdUsuario = new SelectList(db.AspNetUsers, "Id", "Nombre", pedidos.IdUsuario);
            ViewBag.IdEstado = new SelectList(db.EstadoPedido, "Id", "Descripcion", pedidos.IdEstado);
            return View(pedidos);
        }

        // POST: Pedido/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,IdUsuario,IdEstado,Fecha,TotalPagar,TotalDescuento,TotalImpuesto,NumeroProforma")] Pedidos pedidos)
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
    }
}
