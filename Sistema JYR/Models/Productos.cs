//------------------------------------------------------------------------------
// <auto-generated>
//     Este código se generó a partir de una plantilla.
//
//     Los cambios manuales en este archivo pueden causar un comportamiento inesperado de la aplicación.
//     Los cambios manuales en este archivo se sobrescribirán si se regenera el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Sistema_JYR.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;

    public partial class Productos
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Productos()
        {
            this.PedidoDetalle = new HashSet<PedidoDetalle>();
            this.ProductosPrecioUsuarios = new HashSet<ProductosPrecioUsuarios>();
            this.ProformaDetalle = new HashSet<ProformaDetalle>();
        }
    
        public int Id { get; set; }

        [Required(ErrorMessage = "Este campo es requerido")]
        public string Nombre { get; set; }

        [DisplayName("Descripción")]
        public string Descripcion { get; set; }

        [DisplayName("Unidad de Medida")]
        public string UnidadDeMedida { get; set; }

        [Range(0.01,99999999999999, ErrorMessage = "El precio debe ser mayor a ₡0.00")]
        [Required(ErrorMessage = "Este campo es requerido")]
        public double Precio { get; set; }

        [Required(ErrorMessage = "Este campo es requerido")]
        [DisplayName("Cantidad en Inventario")]
        [Range(0, 9999999999999999999, ErrorMessage = "Cantidad en Inventario debe ser mayor o igual a 0")]
        public double CantidadEnInventario { get; set; }
        public int IdCategoria { get; set; }

        [DisplayName("Fecha Vencimiento")]
        public Nullable<System.DateTime> FechaVencimiento { get; set; }

        [Range(0, 99999999999999, ErrorMessage = "El impuesto debe ser mayor a 0")]
        public Nullable<int> Impuesto { get; set; }
        public bool Estado { get; set; }
        [DisplayName("Imagen")]
        public byte[] imagen { get; set; }
    
        public virtual CategoriasProducto CategoriasProducto { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PedidoDetalle> PedidoDetalle { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProductosPrecioUsuarios> ProductosPrecioUsuarios { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProformaDetalle> ProformaDetalle { get; set; }
    }
}
