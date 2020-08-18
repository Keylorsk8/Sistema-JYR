using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Xml.Schema;
using iText.IO.Font.Constants;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Events;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.AspNet.Identity;
using Sistema_JYR.Models;
using Sistema_JYR.Models.Session;

namespace Sistema_JYR.Controllers
{
    public class PedidoController : Controller
    {
        private readonly SistemaJYREntities db = new SistemaJYREntities();

        // GET: Pedido

        public ActionResult Index()
        {

            var pedidos = db.Pedidos.Where(x => x.IdEstado == 7).ToList();
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


        public ActionResult DetalleCliente(int? id)
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

        public ActionResult DetalleEmpresa(int? id)
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

        public ActionResult Pdf(int id)
        {
            MemoryStream ms = new MemoryStream();
            PdfWriter pw = new PdfWriter(ms);
            PdfDocument pdfDocument = new PdfDocument(pw);
            Document doc = new Document(pdfDocument, PageSize.LETTER);
            doc.SetMargins(122, 35, 70, 35);
            string pathLogo = Server.MapPath("~/Content/imagenes/LOGO2.png");
            Image img = new Image(ImageDataFactory.Create(pathLogo));
            pdfDocument.AddEventHandler(PdfDocumentEvent.START_PAGE, new HeaderEventHandler(img));
            pdfDocument.AddEventHandler(PdfDocumentEvent.END_PAGE, new FooterEventHandler());

            List<Pedidos> model = db.Pedidos.Where(x => x.Id == id).ToList();

            foreach (var item in model)
            {
                List<AspNetUsers> user = db.AspNetUsers.Where(x => x.Id == item.IdCliente).ToList();
                List<Telefonos> tel = db.Telefonos.Where(x => x.IdUsuario == item.IdCliente).ToList();
                Table enc = new Table(4).UseAllAvailableWidth();

                Cell cellenc = new Cell().Add(new Paragraph("Pedido No." + id).SetFontSize(11)).
                    SetTextAlignment(TextAlignment.LEFT).SetBorder(Border.NO_BORDER);
                enc.AddCell(cellenc);
                cellenc = new Cell().Add(new Paragraph("Día").SetFontSize(9)).
                            SetTextAlignment(TextAlignment.CENTER).SetWidth(25);
                enc.AddCell(cellenc);
                cellenc = new Cell().Add(new Paragraph("Mes").SetFontSize(9)).
                           SetTextAlignment(TextAlignment.CENTER).SetWidth(40);
                enc.AddCell(cellenc);
                cellenc = new Cell().Add(new Paragraph("Año").SetFontSize(9)).
                           SetTextAlignment(TextAlignment.CENTER).SetWidth(30);
                enc.AddCell(cellenc);
                doc.Add(enc);
                Table enc2 = new Table(4).UseAllAvailableWidth();
                Cell cellenc2 = new Cell().Add(new Paragraph(item.NombrePedido).SetFontSize(9)).
                    SetTextAlignment(TextAlignment.LEFT).SetBorder(Border.NO_BORDER);
                enc2.AddCell(cellenc2);
                cellenc2 = new Cell().Add(new Paragraph(item.Fecha.Day.ToString()).SetFontSize(9)).
                            SetTextAlignment(TextAlignment.CENTER).SetWidth(25).SetHorizontalAlignment(HorizontalAlignment.RIGHT);
                enc2.AddCell(cellenc2);
                cellenc2 = new Cell().Add(new Paragraph(item.Fecha.Month.ToString()).SetFontSize(9)).
                           SetTextAlignment(TextAlignment.CENTER).SetWidth(40).SetHorizontalAlignment(HorizontalAlignment.RIGHT);
                enc2.AddCell(cellenc2);
                cellenc2 = new Cell().Add(new Paragraph(item.Fecha.Year.ToString()).SetFontSize(9)).
                           SetTextAlignment(TextAlignment.CENTER).SetWidth(30).SetHorizontalAlignment(HorizontalAlignment.RIGHT);
                enc2.AddCell(cellenc2);
                doc.Add(enc2);

                Table _enc = new Table(2).UseAllAvailableWidth();
                Cell _cellEnc = new Cell(2, 1).Add(new Paragraph("Información de Cliente").SetFontSize(9)).SetTextAlignment(TextAlignment.LEFT).SetWidth(100).SetBorderRight(Border.NO_BORDER);
                _enc.AddCell(_cellEnc);
                _cellEnc = new Cell(2, 1).Add(new Paragraph("Detalles").SetFontSize(9)).SetTextAlignment(TextAlignment.LEFT).SetWidth(100).SetBorderLeft(Border.NO_BORDER);
                _enc.AddCell(_cellEnc);

                Cell _cellDet1 = new Cell(1, 1).Add(new Paragraph(item.NombreCliente).SetFontSize(9)).
                     SetTextAlignment(TextAlignment.LEFT).SetBorderBottom(Border.NO_BORDER).SetBorderRight(Border.NO_BORDER);
                Cell _cellDet2 = new Cell(1, 1).Add(new Paragraph("Vendedor: " + item.AspNetUsers.Nombre + " " + item.AspNetUsers.Apellido1).SetFontSize(9)).
                       SetTextAlignment(TextAlignment.LEFT).SetBorderBottom(Border.NO_BORDER).SetWidth(114).SetBorderLeft(Border.NO_BORDER);
                Cell _cellDet3 = new Cell(1, 1).Add(new Paragraph(item.DireccionEntrega).SetFontSize(9)).
                     SetTextAlignment(TextAlignment.LEFT).SetBorderTop(Border.NO_BORDER)
                     .SetBorderBottom(Border.NO_BORDER).SetWidth(100).SetBorderRight(Border.NO_BORDER);
                Cell _cellDet4 = new Cell(1, 1).Add(new Paragraph("Estado:" + item.EstadoPedido.Descripcion).SetFontSize(9)).
                     SetTextAlignment(TextAlignment.LEFT).SetBorderTop(Border.NO_BORDER).SetBorderBottom(Border.NO_BORDER).SetWidth(125)
                     .SetBorderLeft(Border.NO_BORDER);
                Cell _cellDet5 = new Cell(1, 2).Add(new Paragraph("Moneda: Colones ( ¢ )").SetFontSize(9)).SetTextAlignment(TextAlignment.LEFT).SetBorderTop(Border.NO_BORDER).SetBorderBottom(Border.NO_BORDER);

                _enc.AddCell(_cellDet1);
                _enc.AddCell(_cellDet2);
                _enc.AddCell(_cellDet3);
                _enc.AddCell(_cellDet4);
                _enc.AddCell(_cellDet5);
                doc.Add(_enc);


                Table tels = new Table(2).UseAllAvailableWidth().SetBorderRight(new SolidBorder(ColorConstants.BLACK, 1)).SetBorderLeft(new SolidBorder(ColorConstants.BLACK, 1));
                Cell ctels1 = new Cell(1, 2).Add(new Paragraph("Teléfonos").SetFontSize(9));
                tels.AddCell(ctels1);
                if (tel.Count() == 0)
                {
                    Cell ctels2 = new Cell(1, 2).Add(new Paragraph("Tel: N/A").SetFontSize(9)).SetTextAlignment(TextAlignment.LEFT).SetBorderTop(Border.NO_BORDER);
                    tels.AddCell(ctels2);
                    doc.Add(tels);
                }

                else
                {
                    foreach (var t in tel)
                    {

                        Cell ctels2 = new Cell(1, 1).Add(new Paragraph("Propietario: " + t.Propietario).SetFontSize(9)).SetBorderTop(Border.NO_BORDER).SetBorderBottom(Border.NO_BORDER).SetBorderRight(Border.NO_BORDER);
                        Cell ctels3 = new Cell(1, 1).Add(new Paragraph("Tel: " + t.Telefono).SetFontSize(9)).SetTextAlignment(TextAlignment.LEFT).SetBorderTop(Border.NO_BORDER).SetBorderBottom(Border.NO_BORDER).SetBorderLeft(Border.NO_BORDER);

                        tels.AddCell(ctels2);
                        tels.AddCell(ctels3);

                    }
                    doc.Add(tels);

                }

            }


            //Productos


            Style styleCell = new Style()
                .SetBackgroundColor(WebColors.GetRGBColor("#042c3c"))
                .SetFontColor(WebColors.GetRGBColor("#f68c25"))
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(10);
            Table _table = new Table(6).UseAllAvailableWidth();
            Cell _cell = new Cell(2, 1).Add(new Paragraph("Id"));
            _table.AddHeaderCell(_cell.AddStyle(styleCell));
            _cell = new Cell(2, 1).Add(new Paragraph("Producto"));
            _table.AddHeaderCell(_cell.AddStyle(styleCell));
            _cell = new Cell(2, 1).Add(new Paragraph("Cantidad"));
            _table.AddHeaderCell(_cell.AddStyle(styleCell));
            _cell = new Cell(2, 1).Add(new Paragraph("Precio Unitario"));
            _table.AddHeaderCell(_cell.AddStyle(styleCell));
            _cell = new Cell(2, 1).Add(new Paragraph("Descuento"));
            _table.AddHeaderCell(_cell.AddStyle(styleCell));
            _cell = new Cell(2, 1).Add(new Paragraph("Cantidad Enviada"));
            _table.AddHeaderCell(_cell.AddStyle(styleCell));
            Style sty = new Style()
               .SetTextAlignment(TextAlignment.CENTER)
               .SetFontSize(9);
            List<PedidoDetalle> det = db.PedidoDetalle.Where(x => x.IdPedido == id).ToList();
            foreach (var item in det)
            {
                List<Productos> prod = db.Productos.Where(p => p.Id == item.IdProducto).ToList();
                foreach (var p in prod)
                {
                    _cell = new Cell().Add(new Paragraph(item.IdProducto.ToString())).SetBorderRight(Border.NO_BORDER).
                        SetBorderTop(Border.NO_BORDER).SetBorderBottom(Border.NO_BORDER);
                    _table.AddCell(_cell.AddStyle(sty));
                    _cell = new Cell().Add(new Paragraph(item.Productos.Nombre)).SetBorder(Border.NO_BORDER);
                    _table.AddCell(_cell.AddStyle(sty));
                    _cell = new Cell().Add(new Paragraph(item.Cantidad.ToString())).SetBorder(Border.NO_BORDER);
                    _table.AddCell(_cell.AddStyle(sty));
                    _cell = new Cell().Add(new Paragraph(item.PrecioUnitario.ToString("₡0,#.00"))).SetBorder(Border.NO_BORDER);
                    _table.AddCell(_cell.AddStyle(sty));
                    _cell = new Cell().Add(new Paragraph(item.Descuento.ToString("₡0,#.00"))).SetBorderLeft(Border.NO_BORDER).
                        SetBorderTop(Border.NO_BORDER).SetBorderBottom(Border.NO_BORDER).SetBorderRight(Border.NO_BORDER);
                    _table.AddCell(_cell.AddStyle(sty));
                    _cell = new Cell().Add(new Paragraph(item.CantidadEnviada.ToString())).SetBorderLeft(Border.NO_BORDER).
                       SetBorderTop(Border.NO_BORDER).SetBorderBottom(Border.NO_BORDER);
                    _table.AddCell(_cell.AddStyle(sty));
                }
            }

            doc.Add(_table);
            foreach (var item in model)
            {

                Table foot = new Table(5).UseAllAvailableWidth();
                Cell f1 = new Cell(1, 3).Add(new Paragraph("Términos y Condiciones").SetFontSize(9)).SetTextAlignment(TextAlignment.LEFT).SetBorderRight(Border.NO_BORDER);
                Cell f2 = new Cell(1, 1).Add(new Paragraph("Total Descuento").SetFontSize(9)).SetTextAlignment(TextAlignment.CENTER).SetBorderRight(Border.NO_BORDER);
                Cell f3 = new Cell(1, 1).Add(new Paragraph(item.TotalDescuento.ToString("₡0,#.00")).SetFontSize(9)).SetTextAlignment(TextAlignment.CENTER);
                Cell f4 = new Cell(1, 3).Add(new Paragraph("La devolución de inventario debe realizarse con la factura,").SetFontSize(9)).SetTextAlignment(TextAlignment.LEFT).SetBorderBottom(Border.NO_BORDER).SetBorderTop(Border.NO_BORDER);
                Cell f5 = new Cell(1, 1).Add(new Paragraph("Total Impuesto").SetFontSize(9)).SetTextAlignment(TextAlignment.CENTER).SetBorderRight(Border.NO_BORDER);
                Cell f6 = new Cell(1, 1).Add(new Paragraph(item.TotalImpuesto.ToString("₡0,#.00")).SetFontSize(9)).SetTextAlignment(TextAlignment.CENTER);
                Cell f7 = new Cell(1, 3).Add(new Paragraph("se contará con 15 días de plazo a partir de su compra (" + item.Fecha.ToShortDateString() + ").").SetFontSize(9)).SetTextAlignment(TextAlignment.LEFT).SetBorderTop(Border.NO_BORDER);
                Cell f8 = new Cell(1, 1).Add(new Paragraph("Total Pagar").SetFontSize(9)).SetTextAlignment(TextAlignment.CENTER).SetBorderRight(Border.NO_BORDER);
                Cell f9 = new Cell(1, 1).Add(new Paragraph(item.TotalPagar.ToString("₡0,#.00")).SetFontSize(9)).SetTextAlignment(TextAlignment.CENTER);
                foot.AddCell(f1);
                foot.AddCell(f2);
                foot.AddCell(f3);
                foot.AddCell(f4);
                foot.AddCell(f5);
                foot.AddCell(f6);
                foot.AddCell(f7);
                foot.AddCell(f8);
                foot.AddCell(f9);
                doc.Add(foot);
            }



            doc.Close();
            byte[] bytesStream = ms.ToArray();
            ms = new MemoryStream();
            ms.Write(bytesStream, 0, bytesStream.Length);
            ms.Position = 0;

            return new FileStreamResult(ms, "application/pdf");

        }







        // GET: Pedido/Create
        public ActionResult Create()
        {
            ViewBag.Fecha = DateTime.Now.ToShortDateString();
            ViewBag.NumeroProforma = "N/A";
            ViewBag.IdUsuario = new SelectList(db.AspNetUsers.Where(x => x.Rol == 1 || x.Rol == 2 && x.Estado == true), "Id", "Nombre");
            ViewBag.IdEstado = new SelectList(db.EstadoPedido.Where(x => x.Descripcion.Equals("Nuevo")), "Id", "Descripcion");
            return View();
        }

        // POST: Pedido/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "IdUsuario,IdEstado,Fecha,TotalPagar,TotalDescuento,TotalImpuesto,IdCliente,NombreCliente,DireccionEntrega,NombrePedido,NumeroProforma")] Pedidos pedidos)
        {
            pedidos.Fecha = DateTime.Now;
            pedidos.TotalDescuento = 0;
            pedidos.TotalImpuesto = 0;
            pedidos.TotalPagar = 0;

            if (pedidos.DireccionEntrega == null)
            {
                pedidos.DireccionEntrega = "Retirar en la ferretería";
            }
            if (pedidos.NombrePedido == null)
            {
                pedidos.NombrePedido = "Pedido #";
            }
            if (ModelState.IsValid)
            {
                db.Pedidos.Add(pedidos);
                db.SaveChanges();
                Session["Pedido"] = "¡Pedido creado con éxito!";
                if (pedidos.NombrePedido == "Pedido #")
                {
                    pedidos.NombrePedido += pedidos.Id;
                }
                db.Entry(pedidos).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.IdUsuario = new SelectList(db.AspNetUsers.Where(x => x.Rol == 1 || x.Rol == 2 && x.Estado == true), "Id", "Nombre", pedidos.IdUsuario);
            ViewBag.IdEstado = new SelectList(db.EstadoPedido.Where(x => x.Descripcion.Equals("Nuevo")), "Id", "Descripcion");
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
        public ActionResult Edit([Bind(Include = "Id,IdUsuario,IdEstado,Fecha,TotalPagar,TotalDescuento,TotalImpuesto,NumeroProforma,IdCliente,NombreCliente,DireccionEntrega,NombrePedido")] Pedidos pedidos)
        {

            double desc = 0;
            double imp = 0;
            double totalPagar = 0;
            List<PedidoDetalle> detalles = db.PedidoDetalle.Where(x => x.IdPedido == pedidos.Id).ToList();

            foreach (var item in detalles)
            {

                double precioBase = item.PrecioUnitario;
                double precioConIVA = precioBase * ((Convert.ToDouble(item.Productos.Impuesto) / 100) + 1);
                double precioConDescuento = precioConIVA - (precioConIVA * (item.Descuento / 100));
                double iva = precioConDescuento - (precioConDescuento / ((Convert.ToDouble(item.Productos.Impuesto) / 100) + 1));
                double descuento = precioBase - (precioConDescuento - iva);
                double subTotal = precioBase + iva - descuento;

                desc += descuento * item.Cantidad;
                imp += iva * item.Cantidad;
                totalPagar += subTotal * item.Cantidad;


            }

            pedidos.TotalDescuento = desc;
            pedidos.TotalImpuesto = imp;
            pedidos.TotalPagar = totalPagar;

            if (ModelState.IsValid)
            {

                db.Entry(pedidos).State = EntityState.Modified;

                db.SaveChanges();
                Session["Pedido"] = "¡Pedido actualizado correctamente!";
                if (Session["Documento"] != null)
                {
                    Documento doc = ((Documento)Session["Documento"]);
                    if (doc.TipoDocumento == TipoDocumento.Pedido && doc.NumerosDocumento == pedidos.Id)
                    {
                        Session["Documento"] = null;
                    }
                }
                return RedirectToAction("Index");
            }

            ViewBag.Id = pedidos.Id;
            ViewBag.IdUsuario = new SelectList(db.AspNetUsers.Where(x => x.Rol == 1 || x.Rol == 2 && x.Estado == true), "Id", "Nombre", pedidos.IdUsuario);
            ViewBag.IdEstado = new SelectList(db.EstadoPedido, "Id", "Descripcion", pedidos.IdEstado);
            return View(pedidos);
        }

        public ActionResult SeleccionarDocumento(int id)
        {
            try
            {
                Pedidos pedido = db.Pedidos.Find(id);
            }
            catch (Exception)
            {
                Session["Pedido"] = "No existe el pedido";
                return RedirectToAction("Index");
            }
            Session["Documento"] = new Documento() { TipoDocumento = TipoDocumento.Pedido, NumerosDocumento = id };
            Session["Pedido"] = "Seleccionado";
            return RedirectToAction("Index");
        }

        public ActionResult CancelarPedido(int? id)
        {
            if (id == null)
            {
                Session["Pedido"] = "Pedido inválido. Especifique un Pedido";
                return RedirectToAction("Index");
            }
            Pedidos pedido = db.Pedidos.Find(id);
            if (pedido == null)
            {
                Session["Pedido"] = "No existe el pedido";
                return RedirectToAction("Index");
            }
            pedido.IdEstado = 4;
            db.Entry(pedido).State = EntityState.Modified;
            db.SaveChanges();
            Session["Pedido"] = "Cancelado";
            Documento doc = ((Documento)Session["Documento"]);
            if (doc.TipoDocumento == TipoDocumento.Pedido && doc.NumerosDocumento == id)
            {
                Session["Documento"] = null;
            }
            return RedirectToAction("Index");
        }

        public ActionResult SeleccionarDocumentoCliente(int id)
        {
            try
            {
                Pedidos pedido = db.Pedidos.Find(id);
            }
            catch (Exception)
            {

                Session["Pedido"] = "No existe el pedido";
                return RedirectToAction("ListaPedidos", User.Identity.GetUserId());
            }
            Session["Documento"] = new Documento() { TipoDocumento = TipoDocumento.Proforma, NumerosDocumento = id };
            Session["Pedido"] = "Seleccionado";
            return RedirectToAction("ListaPedidos", HttpContext.User.Identity.GetUserId());
        }

        public ActionResult CancelarPedidoCliente(int? id)
        {
            if (id == null)
            {
                Session["Pedido"] = "Pedido inválido. Especifique un pedido";
                return RedirectToAction("ListaPedidos", User.Identity.GetUserId());
            }
            Proformas proforma = db.Proformas.Find(id);
            if (proforma == null)
            {
                Session["Pedido"] = "No existe el pedido";
                return RedirectToAction("ListaPedidos", User.Identity.GetUserId());
            }
            proforma.IdEstado = 4;
            db.Entry(proforma).State = EntityState.Modified;
            db.SaveChanges();
            Session["Pedido"] = "Cancelado";
            Documento doc = ((Documento)Session["Documento"]);
            if (doc.TipoDocumento == TipoDocumento.Pedido && doc.NumerosDocumento == id)
            {
                Session["Documento"] = null;
            }
            return RedirectToAction("ListaPedidos", User.Identity.GetUserId());
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


            Pedidos pedidoDuplicado = new Pedidos
            {
                IdUsuario = pedidos.IdUsuario,
                IdEstado = pedidos.IdEstado,
                Fecha = pedidos.Fecha,
                TotalPagar = pedidos.TotalPagar,
                TotalDescuento = pedidos.TotalDescuento,
                TotalImpuesto = pedidos.TotalImpuesto,
                NumeroProforma = pedidos.NumeroProforma,
                IdCliente = pedidos.IdCliente,
                NombreCliente = pedidos.NombreCliente,
                NombrePedido = pedidos.NombrePedido,
                DireccionEntrega = pedidos.DireccionEntrega
            };
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

        public ActionResult cantidadAEnviar(int? id)
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


        public ActionResult CambiarcantidadAEnviar(AjaxCantidad objeto)
        {
            int idPedido = Convert.ToInt32(objeto.pedidoId);
            Pedidos pedido = db.Pedidos.Find(idPedido);
            List<PedidoDetalle> detalles = db.PedidoDetalle.Where(x => x.IdPedido == idPedido).ToList();
            pedido.PedidoDetalle = detalles;

            if (objeto.cantEnviada != null)
            {
                int cant = Convert.ToInt32(objeto.cantEnviada);

                int idDetalle = Convert.ToInt32(objeto.detalleId);


                List<PedidoDetalle> deta = db.PedidoDetalle.Where(x => x.IdPedido == idPedido && x.Id == idDetalle).ToList();

                foreach (var item in deta)
                {
                    Productos p = db.Productos.Find(item.IdProducto);

                    if (p.CantidadEnInventario < cant)
                    {
                        Session["Pedidos"] = "Cantidad en inventario menor";
                        return PartialView("_detalle", pedido);
                    }


                    if (item.CantidadEnviada < item.Cantidad)
                    {

                        if (cant <= item.Cantidad)
                        {
                            if ((item.Cantidad - item.CantidadEnviada) >= cant)
                            {

                                PedidoDetalle det = db.PedidoDetalle.Find(item.Id);
                                det.CantidadEnviada += cant;
                                db.Entry(det).State = EntityState.Modified;
                                db.SaveChanges();

                                p.CantidadEnInventario -= cant;
                                db.Entry(p).State = EntityState.Modified;
                                db.SaveChanges();


                                List<PedidoDetalle> d = db.PedidoDetalle.Where(x => x.IdPedido == idPedido).ToList();
                                int count = 0;
                                foreach (var de in d)
                                {
                                    if (de.CantidadEnviada == de.Cantidad)
                                    {
                                        count++;
                                    }


                                }

                                if (count == d.Count())
                                {
                                    pedido.IdEstado = 1;
                                    db.Entry(pedido).State = EntityState.Modified;
                                    db.SaveChanges();
                                }

                                pedido.PedidoDetalle = d;
                                return PartialView("_detalle", pedido);
                            }
                            else
                            {
                                Session["Pedidos"] = 2;
                                return PartialView("_detalle", pedido);
                            }

                        }

                        else
                        {
                            Session["Pedidos"] = 2;
                            return PartialView("_detalle", pedido);

                        }

                    }

                    else
                    {
                        Session["Pedidos"] = 2;
                        return PartialView("_detalle", pedido);
                    }

                }

                //validar 

            }

            return RedirectToAction("cantidadAEnviar", pedido);
        }

        public ActionResult SeguimientoPedidoEmpresa()
        {
            var list = db.Pedidos.Where(x => x.IdEstado == 7).ToList();
            return View(list);
        }

        [HttpPost]
        public ActionResult SeguimientoPedidoEmpresa(AjaxPed objet)
        {
            if (objet.estado != null)
            {
                if (objet.estado.Equals("nuevo"))
                {

                    var pedidos = db.Pedidos.Where(x => x.IdEstado == 7).ToList();
                    return PartialView("_SeguimientoPedidoEmpresa", pedidos.ToList());

                }

                if (objet.estado.Equals("procesando"))
                {

                    var pedidos = db.Pedidos.Where(x => x.IdEstado == 3).ToList();
                    return PartialView("_SeguimientoPedidoEmpresa", pedidos.ToList());
                }

                if (objet.estado.Equals("finalizado"))
                {

                    var pedidos = db.Pedidos.Where(x => x.IdEstado == 1).ToList();
                    return PartialView("_SeguimientoPedidoEmpresa", pedidos.ToList());
                }

                if (objet.estado.Equals("cancelado"))
                {
                    var pedidos = db.Pedidos.Where(x => x.IdEstado == 4 || x.IdEstado == 6).ToList();
                    return PartialView("_SeguimientoPedidoEmpresa", pedidos.ToList());
                }

            }


            return View();
        }

        public ActionResult SeguimientoPedido()
        {
            string idUsuario = User.Identity.GetUserId();
            var list = db.Pedidos.Where(x => x.IdUsuario == idUsuario && x.IdEstado == 7).ToList();
            return View(list);
        }




        [HttpPost]
        public ActionResult SeguimientoPedido(AjaxSeguimiento objet)
        {
            string idUsuario = User.Identity.GetUserId();
            if (objet.usuarioId != null)
            {
                if (objet.estado.Equals("nuevo"))
                {
                    var pedidos = db.Pedidos.Where(x => x.IdUsuario == idUsuario && x.IdEstado == 7).ToList();
                    return PartialView("_SeguimientoPedido", pedidos);
                }
                if (objet.estado.Equals("procesando"))
                {
                    var pedidos = db.Pedidos.Where(x => x.IdUsuario == idUsuario && x.IdEstado == 3).ToList();
                    return PartialView("_SeguimientoPedido", pedidos);
                }
                if (objet.estado.Equals("finalizado"))
                {
                    var pedidos = db.Pedidos.Where(x => x.IdUsuario == idUsuario && x.IdEstado == 1).ToList();
                    return PartialView("_SeguimientoPedido", pedidos);
                }
                if (objet.estado.Equals("cancelado"))
                {
                    var pedidos = db.Pedidos.Where(x => x.IdUsuario == idUsuario && x.IdEstado == 4 || x.IdUsuario == idUsuario && x.IdEstado == 6).ToList();
                    return PartialView("_SeguimientoPedido", pedidos);
                }
            }
            return View();
        }

        public ActionResult Cancelar(int Id)
        {
            Pedidos pedidos = db.Pedidos.Find(Id);
            pedidos.IdEstado = 4;


            if (ModelState.IsValid)
            {

                db.Entry(pedidos).State = EntityState.Modified;

                db.SaveChanges();
                Session["Pedido"] = "¡Orden Cancelada Correctamente!";
                return RedirectToAction("SeguimientoPedido");
            }

            return View();
        }

        public ActionResult Confirmar(int Id)
        {
            Pedidos pedidos = db.Pedidos.Find(Id);
            pedidos.IdEstado = 3;


            if (ModelState.IsValid)
            {

                db.Entry(pedidos).State = EntityState.Modified;

                db.SaveChanges();
                Session["Pedido"] = "¡Orden Confirmada Correctamente!";
                var list = db.Pedidos.Where(x => x.IdEstado == 7).ToList();

                return RedirectToAction("SeguimientoPedidoEmpresa", list);

            }

            return View("SeguimientoPedidoEmpresa");
        }

        public ActionResult CancelarFerreteria(int Id)
        {
            Pedidos pedidos = db.Pedidos.Find(Id);
            pedidos.IdEstado = 6;


            if (ModelState.IsValid)
            {

                db.Entry(pedidos).State = EntityState.Modified;

                db.SaveChanges();
                Session["Pedido"] = "¡Orden Cancelada Correctamente!";

                var list = db.Pedidos.Where(x => x.IdEstado == 7).ToList();
                return RedirectToAction("SeguimientoPedidoEmpresa", list);

            }

            return View("SeguimientoPedidoEmpresa");
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public ActionResult FiltrarPedidosAjax(AjaxFiltrarPedido objeto)
        {

            var terminoBusqueda = objeto.TerminoBusqueda;
            var idPedido = Convert.ToInt32(objeto.IdPedido);
            List<Pedidos> pedidos = new List<Pedidos>();
            List<Pedidos> pedidosFiltradasNombre = new List<Pedidos>();
            List<Pedidos> pedidosFiltradaId = new List<Pedidos>();
            pedidos = db.Pedidos.Where(x => x.AspNetUsers.Rol == 2 && x.IdEstado == 7 || x.AspNetUsers.Rol == 1 && x.IdEstado == 7).ToList();

            if (terminoBusqueda != null)
            {
                foreach (var item in pedidos)
                {
                    if (item.NombrePedido.ToLower().Contains(terminoBusqueda.ToLower()))
                    {
                        pedidosFiltradasNombre.Add(item);
                    }
                }
                pedidos = pedidosFiltradasNombre;
            }


            if (idPedido != 0)
            {
                foreach (var item in pedidos)
                {
                    if (item.Id == idPedido)
                    {
                        pedidosFiltradaId = new List<Pedidos>
                        {
                            item
                        };
                        break;
                    }
                }
                pedidos = pedidosFiltradaId;
            }

            return PartialView("_ListaPedidos", pedidos);
        }

        public ActionResult EliminarCarrito(int? idD, int IdPedido)
        {
            Pedidos pedidos = db.Pedidos.Find(IdPedido);
            PedidoDetalle detalles = db.PedidoDetalle.Find(idD);


            var prod = db.Productos.Where(x => x.Id == detalles.IdProducto);

            if (detalles.CantidadEnviada > 0)
            {
                Session["Pedidos"] = "No se puede eliminar";
                return PartialView("_ListaPedidoCarrito", pedidos);
            }


            double totalPagar = 0;
            double impuesto = 0;
            double descuento = 0;
            double desc = 0;
            double imp = 0;

            db.PedidoDetalle.Remove(detalles);
            db.SaveChanges();




            pedidos.Id = pedidos.Id;
            pedidos.IdUsuario = pedidos.IdUsuario;
            pedidos.IdEstado = pedidos.IdEstado;
            pedidos.Fecha = pedidos.Fecha;

            List<PedidoDetalle> detallesPedido = db.PedidoDetalle.Where(x => x.IdPedido == IdPedido).ToList();

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


        public ActionResult CambiarDescuento(AjaxDescuento objet)
        {

            int pedidoId = Convert.ToInt32(objet.pedidoId);
            int id = Convert.ToInt32(objet.productoId);
            int descuentoP = Convert.ToInt32(objet.descuento);
            if (descuentoP < 0)
            {
                descuentoP = 0;
            }
            double totalPagar = 0;
            double desc = 0;
            double imp = 0;

            Pedidos p = db.Pedidos.Find(pedidoId);
            List<PedidoDetalle> detalles = db.PedidoDetalle.Where(x => x.IdPedido == pedidoId).ToList();

            foreach (var item in detalles)
            {
                PedidoDetalle detalle = db.PedidoDetalle.Find(item.Id);


                if (item.IdProducto == id)
                {
                    detalle.Descuento = descuentoP;

                    db.Entry(detalle).State = EntityState.Modified;
                    db.SaveChanges();


                }
                {
                }
                double precioBase = item.PrecioUnitario;
                double precioConIVA = precioBase * ((Convert.ToDouble(item.Productos.Impuesto) / 100) + 1);
                double precioConDescuento = precioConIVA - (precioConIVA * (item.Descuento / 100));
                double iva = precioConDescuento - (precioConDescuento / ((Convert.ToDouble(item.Productos.Impuesto) / 100) + 1));
                double descuento = precioBase - (precioConDescuento - iva);
                double subTotal = precioBase + iva - descuento;

                desc += descuento * item.Cantidad;
                imp += iva * item.Cantidad;
                totalPagar += subTotal * item.Cantidad;

            }

            p.TotalDescuento = desc;
            p.TotalImpuesto = imp;
            p.TotalPagar = totalPagar;
            db.Entry(p).State = EntityState.Modified;
            db.SaveChanges();


            p.PedidoDetalle = detalles;
            ViewBag.TotalPagar = p.TotalPagar;
            ViewBag.TotalDescuento = p.TotalDescuento;
            ViewBag.TotalImpuesto = p.TotalImpuesto;
            return PartialView("_ListaPedidoCarrito", p);
        }
        public ActionResult CambiarCantidadEnviada(AjaxCambio objet)
        {

            int pedidoId = Convert.ToInt32(objet.pedidoId);
            int id = Convert.ToInt32(objet.productoId);
            int cantidad = Convert.ToInt32(objet.cantidadEnviada);

            if (cantidad < 0)
            {
                cantidad = 0;
            }

            double desc = 0;
            double imp = 0;
            double totalPagar = 0;
            Pedidos ped = db.Pedidos.Find(pedidoId);
            Productos p = db.Productos.Find(id);

            if (p.CantidadEnInventario < cantidad)
            {
                Session["Pedidos"] = "Cantidad en inventario menor";
                return PartialView("_ListaPedidoCarrito", ped);
            }
            List<PedidoDetalle> detalles = db.PedidoDetalle.Where(x => x.IdPedido == pedidoId).ToList();

            foreach (var item in detalles)
            {
                PedidoDetalle detalle = db.PedidoDetalle.Find(item.Id);


                if (item.IdProducto == id)
                {
                    if (cantidad > item.Cantidad)
                    {

                        Session["Pedidos"] = "Debe digitar una cantidad menor a la cantidad";
                        return PartialView("_ListaPedidoCarrito", ped);
                    }

                    double precioBase = item.PrecioUnitario;
                    double precioConIVA = precioBase * ((Convert.ToDouble(item.Productos.Impuesto) / 100) + 1);
                    double precioConDescuento = precioConIVA - (precioConIVA * (item.Descuento / 100));
                    double iva = precioConDescuento - (precioConDescuento / ((Convert.ToDouble(item.Productos.Impuesto) / 100) + 1));
                    double descuento = precioBase - (precioConDescuento - iva);
                    double subTotal = precioBase + iva - descuento;

                    desc += descuento * item.Cantidad;
                    imp += iva * item.Cantidad;
                    totalPagar += subTotal * item.Cantidad;
                    if (item.CantidadEnviada > cantidad)
                    {
                        int diferencia = (int)(item.CantidadEnviada - cantidad);
                        p.CantidadEnInventario += diferencia;
                        db.Entry(p).State = EntityState.Modified;
                        db.SaveChanges();
                    }

                    if (cantidad >= item.CantidadEnviada)
                    {
                        int diferencia = (int)(cantidad - item.CantidadEnviada);
                        p.CantidadEnInventario -= diferencia;
                        db.Entry(p).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    detalle.CantidadEnviada = cantidad;
                    db.Entry(detalle).State = EntityState.Modified;
                    db.SaveChanges();

                }

            }
            ped.TotalDescuento = desc;
            ped.TotalImpuesto = imp;
            ped.TotalPagar = totalPagar;
            db.Entry(ped).State = EntityState.Modified;
            db.SaveChanges();
            ped.PedidoDetalle = detalles;

            return PartialView("_ListaPedidoCarrito", ped);
        }


        public ActionResult CambiarCantidadPedida(Ajax objeto)
        {
            int idPedido = Convert.ToInt32(objeto.idPedido);
            int id = Convert.ToInt32(objeto.id);
            int cantidadCambio = 0;
            try
            {
                cantidadCambio = Convert.ToInt32(objeto.terminoBusqueda);

                if (cantidadCambio < 0)
                {
                    cantidadCambio = 1;
                }
            }
            catch (Exception)
            {
                cantidadCambio = 2000000000;
                if (cantidadCambio < 0)
                {
                    cantidadCambio = 1;
                }
            }

            double totalPagar = 0;
            double desc = 0;
            double imp = 0;

            Pedidos pedidos = db.Pedidos.Find(idPedido);
            List<PedidoDetalle> detalles = db.PedidoDetalle.Where(x => x.IdPedido == idPedido).ToList();

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
                        Session["Pedidos"] = "Debe digitar una cantidad mayor a la cantidad enviada";
                        return PartialView("_ListaPedidoCarrito", pedidos);
                    }

                    detalle.Cantidad = cantidadCambio;
                    detalle.CantidadEnviada = item.CantidadEnviada;
                    db.Entry(detalle).State = EntityState.Modified;
                    db.SaveChanges();

                }
                double precioBase = item.PrecioUnitario;
                double precioConIVA = precioBase * ((Convert.ToDouble(item.Productos.Impuesto) / 100) + 1);
                double precioConDescuento = precioConIVA - (precioConIVA * (item.Descuento / 100));
                double iva = precioConDescuento - (precioConDescuento / ((Convert.ToDouble(item.Productos.Impuesto) / 100) + 1));
                double descuento = precioBase - (precioConDescuento - iva);
                double subTotal = precioBase + iva - descuento;

                desc += descuento * item.Cantidad;
                imp += iva * item.Cantidad;
                totalPagar += subTotal * item.Cantidad;
            }

            pedidos.TotalDescuento = desc;
            pedidos.TotalImpuesto = imp;
            pedidos.TotalPagar = totalPagar;
            db.Entry(pedidos).State = EntityState.Modified;
            db.SaveChanges();
            pedidos.PedidoDetalle = detalles;

            return PartialView("_ListaPedidoCarrito", pedidos);
        }


        public ActionResult AgregarDetalle(AjaxDetalle objeto)
        {
            int idPedido = Convert.ToInt32(objeto.idPedido);
            int nuevoId = Convert.ToInt32(objeto.idProducto);
            int cant = Convert.ToInt32(objeto.cantidad);



            Pedidos ped = db.Pedidos.Find(idPedido);
            Productos p = db.Productos.Find(nuevoId);
            double totalPagar = 0;
            double desc = 0;
            double imp = 0;



            if (cant == 0)
            {
                Session["Pedidos"] = "Debe digitar una cantidad mayor a 0";
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

                    PedidoDetalle detalle = new PedidoDetalle
                    {
                        IdPedido = idPedido,
                        IdProducto = nuevoId
                    };
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
                    Session["Pedidos"] = "Producto es inválido. Digite un código de producto nuevamente";
                    return PartialView("_ListaPedidoCarrito", ped);

                }

            }

            List<PedidoDetalle> detallesPedido = db.PedidoDetalle.Where(x => x.IdPedido == idPedido).ToList();
            ped.PedidoDetalle = detallesPedido;

            foreach (var item in detallesPedido)
            {
                double precioBase = item.PrecioUnitario;
                double precioConIVA = precioBase * ((Convert.ToDouble(item.Productos.Impuesto) / 100) + 1);
                double precioConDescuento = precioConIVA - (precioConIVA * (item.Descuento / 100));
                double iva = precioConDescuento - (precioConDescuento / ((Convert.ToDouble(item.Productos.Impuesto) / 100) + 1));
                double descuento = precioBase - (precioConDescuento - iva);
                double subTotal = precioBase + iva - descuento;

                desc += descuento * item.Cantidad;
                imp += iva * item.Cantidad;
                totalPagar += subTotal * item.Cantidad;

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

        public class AjaxDescuento
        {
            public string descuento
            {
                get;
                set;
            }
            public string productoId
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
        public class AjaxFiltrarPedido
        {
            public string TerminoBusqueda
            {
                get;
                set;
            }
            public string IdPedido
            {
                get;
                set;
            }
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

        public class AjaxPed
        {
            public string estado
            {
                get;
                set;
            }



        }

        public class AjaxCantidad
        {
            public string pedidoId
            {
                get;
                set;
            }
            public string detalleId
            {
                get;
                set;
            }

            public string cantEnviada
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
        public class AjaxSeguimiento
        {

            public string usuarioId
            {
                get;
                set;
            }

            public string estado
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
        public class AjaxFiltro
        {
            public string numeroPedido
            {
                get;
                set;
            }



        }

        public ActionResult reporteTopProducto(string fechaAnterior, string fechaActual, string IdCategoria)
        {
            var list = db.CategoriasProducto.ToList();
            CategoriasProducto cat = new CategoriasProducto();
            cat.Descripcion = "Todas";
            cat.Id = 90000;
            list.Insert(0, cat);
            SelectList listaCat = new SelectList(list, "Id", "Descripcion");
            ViewBag.IdCategoria = listaCat;


            if (fechaAnterior != null && fechaActual != null)
            {
                DateTime anterior = Convert.ToDateTime(fechaAnterior);
                DateTime actual = Convert.ToDateTime(fechaActual);

                if (anterior.Year > actual.Year)
                {
                    Session["Pedido"] = "Seleccione una fecha válida";
                    TempData["mensajeReporte"] = "Seleccione una fecha inválida";

                }
                else
                {
                    if (anterior.Year == actual.Year && anterior.Month > actual.Month)
                    {
                        Session["Pedido"] = "Seleccione una fecha válida";
                        TempData["mensajeReporte"] = "Seleccione una fecha inválida";

                    }
                    else
                    {
                        if (anterior.Year == actual.Year && anterior.Month == actual.Month && anterior.Day > actual.Day)
                        {
                            Session["Pedido"] = "Seleccione una fecha válida";
                            TempData["mensajeReporte"] = "Seleccione una fecha inválida";

                        }

                        else
                        {
                            if (actual > DateTime.Now)
                            {
                                Session["Pedido"] = "Seleccione una fecha válida";
                                TempData["mensajeReporte"] = "Seleccione una fecha inválida";
                            }

                            else
                            {
                                int categoria = Convert.ToInt32(IdCategoria);
                                if (categoria == 90000)
                                {
                                    var query = from d in db.PedidoDetalle
                                                join p in db.Productos on d.IdProducto equals p.Id
                                                join c in db.CategoriasProducto on p.IdCategoria equals c.Id
                                                join o in db.Pedidos on d.IdPedido equals o.Id
                                                where o.Fecha >= anterior && o.Fecha <= actual
                                                group new { d.Cantidad, d.PrecioUnitario }
                                                by new { p.Id, p.Nombre, c.Descripcion }
                                                into g
                                                orderby g.Sum(x => x.PrecioUnitario * x.Cantidad) descending

                                                select new
                                                {
                                                    Id = g.Key.Id,
                                                    Producto = g.Key.Nombre,
                                                    Categoria = g.Key.Descripcion,
                                                    cantidadTotal = g.Sum(x => x.Cantidad),
                                                    Total = g.Sum(x => x.PrecioUnitario * x.Cantidad)

                                                };

                                    ViewBag.ReportViewer = Reporte.reporte(query.ToList(), "", "ReporteTopProducto.rdlc");
                                    return PartialView("_reporteTopProducto", query.ToList());
                                }

                                var querys = from d in db.PedidoDetalle
                                             join p in db.Productos on d.IdProducto equals p.Id
                                             join c in db.CategoriasProducto on p.IdCategoria equals c.Id
                                             join o in db.Pedidos on d.IdPedido equals o.Id
                                             where o.Fecha >= anterior && o.Fecha <= actual && p.IdCategoria == categoria
                                             group new { d.Cantidad, d.PrecioUnitario }
                                            by new { p.Id, p.Nombre, c.Descripcion }
                                               into g
                                             orderby g.Sum(x => x.PrecioUnitario * x.Cantidad) descending

                                             select new
                                             {
                                                 Id = g.Key.Id,
                                                 Producto = g.Key.Nombre,
                                                 Categoria = g.Key.Descripcion,
                                                 cantidadTotal = g.Sum(x => x.Cantidad),
                                                 Total = g.Sum(x => x.PrecioUnitario * x.Cantidad)

                                             };



                                ViewBag.ReportViewer = Reporte.reporte(querys.ToList(), "", "ReporteTopProducto.rdlc");



                                return PartialView("_reporteTopProducto", querys.ToList());
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

        public ActionResult reporteVentasPorEmpleado(string fechaAnterior, string fechaActual)
        {
            if (fechaAnterior != null && fechaActual != null)
            {
                DateTime anterior = Convert.ToDateTime(fechaAnterior);
                DateTime actual = Convert.ToDateTime(fechaActual);

                if (anterior.Year > actual.Year)
                {
                    Session["Pedidos"] = "Seleccione una fecha válida";
                    TempData["mensajeReporte"] = "Seleccione una fecha inválida";

                }
                else
                {
                    if (anterior.Year == actual.Year && anterior.Month > actual.Month)
                    {
                        Session["Pedidos"] = "Seleccione una fecha válida";
                        TempData["mensajeReporte"] = "Seleccione una fecha inválida";

                    }
                    else
                    {
                        if (anterior.Year == actual.Year && anterior.Month == actual.Month && anterior.Day > actual.Day)
                        {
                            Session["Pedidos"] = "Seleccione una fecha válida";
                            TempData["mensajeReporte"] = "Seleccione una fecha inválida";

                        }

                        else
                        {
                            if (actual > DateTime.Now)
                            {
                                Session["Pedidos"] = "Seleccione una fecha válida";
                                TempData["mensajeReporte"] = "Seleccione una fecha inválida";
                            }

                            else
                            {

                                var query = from u in db.AspNetUsers
                                            join p in db.Pedidos on u.Id equals p.IdUsuario
                                            where u.Rol == 1 || u.Rol == 2 && p.Fecha >= anterior && p.Fecha <= actual
                                            group new { p.TotalPagar, u.Id }
                                            by new { u.Nombre, u.Apellido1, u.Apellido2, u.Cedula, u.Email }
                                           into g
                                            orderby g.Sum(x => x.TotalPagar) descending

                                            select new
                                            {
                                                g.Key.Cedula,
                                                g.Key.Nombre,
                                                g.Key.Apellido1,
                                                g.Key.Apellido2,
                                                g.Key.Email,
                                                Producto = g.Key.Nombre,
                                                CantidadPedidos = g.Count(),
                                                Acumulado = g.Sum(x => x.TotalPagar)
                                            };

                                ViewBag.ReportViewer = Reporte.reporte(query.ToList(), "", "ReporteUsuario.rdlc");

                                return PartialView("_ReporteUsuario", query.ToList());
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

        public class HeaderEventHandler : IEventHandler
        {
            Image Img;
            public HeaderEventHandler(Image img)
            {
                Img = img;
            }

            public void HandleEvent(Event @event)
            {
                PdfDocumentEvent docEvent = (PdfDocumentEvent)@event;
                PdfDocument pdfDoc = docEvent.GetDocument();
                PdfPage page = docEvent.GetPage();
                Rectangle rootArea = new iText.Kernel.Geom.Rectangle(35, page.GetPageSize().GetTop() - 130, page.GetPageSize().GetRight() - 70, 100);
                iText.Kernel.Pdf.Canvas.PdfCanvas canvas1 = new iText.Kernel.Pdf.Canvas.PdfCanvas(page.NewContentStreamBefore(), page.GetResources(), pdfDoc);
                new Canvas(canvas1, pdfDoc, rootArea).Add(getTable(docEvent));
            }


            public Table getTable(PdfDocumentEvent docEvent)
            {

                Table tableEvent = new Table(2).UseAllAvailableWidth().SetHeight(600);

                Style styleCell = new Style()
                    .SetBorder(Border.NO_BORDER);

                Style styleText = new Style()
                    .SetTextAlignment(TextAlignment.LEFT);

                PdfFont bold = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                Cell cell = new Cell()
                    .Add(new Paragraph("Ferretería y Materiales JYR S.A\n").SetFont(bold).SetFontSize(12))
                    .Add(new Paragraph("Guapalupe de Alajuela. 300 metros norte\n").SetFont(bold).SetFontSize(8))
                    .Add(new Paragraph("de la iglesia Santa Eduviges,\n").SetFont(bold).SetFontSize(8))
                     .Add(new Paragraph("carretera a Carrizal\n").SetFont(bold).SetFontSize(8))
                     .Add(new Paragraph("Tel: 2430-1131 / 2430-6876\n").SetFont(bold).SetFontSize(8))
                        .Add(new Paragraph("Cédula Jurídica: 3-101-659718\n").SetFont(bold).SetFontSize(8))
                           .Add(new Paragraph("Email: ferreteriaymaterialesjyr@gmail.com\n").SetFont(bold).SetFontSize(8))
                    .AddStyle(styleText).AddStyle(styleCell)
                    .SetBorder(Border.NO_BORDER).SetWidth(450);


                tableEvent.AddCell(cell);

                cell = new Cell().Add(Img.SetAutoScale(true)).SetBorder(Border.NO_BORDER)
                     .SetHorizontalAlignment(HorizontalAlignment.RIGHT).SetTextAlignment(TextAlignment.RIGHT);

                tableEvent.AddCell(cell).
                    SetTextAlignment(TextAlignment.RIGHT);

                return tableEvent;
            }

        }



        public class FooterEventHandler : IEventHandler
        {
            public void HandleEvent(Event @event)
            {
                PdfDocumentEvent docEvent = (PdfDocumentEvent)@event;
                PdfDocument pdfDoc = docEvent.GetDocument();
                PdfPage page = docEvent.GetPage();
                Rectangle rootArea = new iText.Kernel.Geom.Rectangle(32, 20, page.GetPageSize().GetWidth() - 70, 50);
                iText.Kernel.Pdf.Canvas.PdfCanvas canvas1 = new iText.Kernel.Pdf.Canvas.PdfCanvas(page.NewContentStreamAfter(), page.GetResources(), pdfDoc);
                new Canvas(canvas1, pdfDoc, rootArea)
                    .Add(getTable(docEvent));



            }


            public Table getTable(PdfDocumentEvent docEvent)
            {
                float[] cellWidth = { 92f, 8f };
                Table tableEvent = new Table(UnitValue.CreatePercentArray(cellWidth)).UseAllAvailableWidth();

                PdfPage page = docEvent.GetPage();
                int pageNum = docEvent.GetDocument().GetPageNumber(page);
                int pageAll = docEvent.GetDocument().GetNumberOfPages();

                Style styleCell = new Style()
                    .SetBorder(Border.NO_BORDER)
                    .SetPadding(5)
                    .SetBorderTop(new SolidBorder(ColorConstants.BLACK, 2));

                Style styleText = new Style()
                    .SetTextAlignment(TextAlignment.RIGHT);

                PdfFont bold = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                Cell cell = new Cell()
                    .Add(new Paragraph("Página " + pageNum + " de " + pageAll).SetFont(bold).SetFontSize(8))
                    .Add(new Paragraph("Pedido emitido el: " + DateTime.Now).SetFont(bold).SetFontSize(8))
                    .AddStyle(styleText).AddStyle(styleCell)
                    .SetBorder(Border.NO_BORDER).SetWidth(450);


                tableEvent.AddCell(cell);

                return tableEvent;
            }
        }
    }
}
