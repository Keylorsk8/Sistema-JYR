using EllipticCurve.Utils;
using Microsoft.AspNet.Identity;
using Sistema_JYR.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web;
using System.Web.Mvc;

namespace Sistema_JYR.Controllers
{
    public class RecomendacionesController : Controller
    {
        // GET: Recomendaciones
        private readonly SistemaJYREntities db = new SistemaJYREntities();
        public List<Productos> RecomendarProductos(string id)
        {
            string idUsuario = id;
            var list = db.Pedidos.Where(x => x.IdUsuario == idUsuario).ToList();
            List<PedidoDetalle> det = new List<PedidoDetalle>();
            List < Productos> p = new List<Productos>();
            foreach (var item in list)
            {
              List<PedidoDetalle> lista = db.PedidoDetalle.Where(x => x.IdPedido == item.Id).ToList();
                foreach (var d in lista)
                {
                    det.Add(d);
                }
            }
            List<PedidoDetalle> detallesAgrupados = new List<PedidoDetalle>();
            foreach (var dp in det)
            {
                if (detallesAgrupados.Count == 0)
                {
                    detallesAgrupados.Add(dp);
                }
                else
                {
                    bool existe = false;
                    foreach (var item in detallesAgrupados)
                    {
                        if (item.IdProducto == dp.IdProducto)
                        {
                            existe = true;
                            item.Cantidad += dp.Cantidad;
                        }
                    }
                    if (!existe)
                    {
                        detallesAgrupados.Add(dp);
                    }
                }
            }
            var prueba = detallesAgrupados.OrderByDescending(x => x.Cantidad).Take(5);
            foreach (var d in prueba)
            {
                p.Add(d.Productos);
            }
            return p;
        }

        public List<string> RecomendarBusquedas(string id)
        {
            string idUsuario = id;
            var list = db.Pedidos.Where(x => x.IdUsuario == idUsuario).ToList();
            List<PedidoDetalle> det = new List<PedidoDetalle>();
            List<string> p = new List<string>();
            foreach (var item in list)
            {
                List<PedidoDetalle> lista = db.PedidoDetalle.Where(x => x.IdPedido == item.Id).ToList();
                foreach (var d in lista)
                {
                    det.Add(d);
                }
            }
            List<PedidoDetalle> detallesAgrupados = new List<PedidoDetalle>();
            foreach (var dp in det)
            {
                if (detallesAgrupados.Count == 0)
                {
                    detallesAgrupados.Add(dp);
                }
                else
                {
                    bool existe = false;
                    foreach (var item in detallesAgrupados)
                    {
                        if (item.IdProducto == dp.IdProducto)
                        {
                            existe = true;
                            item.Cantidad += dp.Cantidad;
                        }
                    }
                    if (!existe)
                    {
                        detallesAgrupados.Add(dp);
                    }
                }
            }
            var prueba = detallesAgrupados.OrderByDescending(x => x.Cantidad).Take(5);
            foreach (var d in prueba)
            {
                p.Add(d.Productos.Nombre);
            }
            return p;
        }
    }
}