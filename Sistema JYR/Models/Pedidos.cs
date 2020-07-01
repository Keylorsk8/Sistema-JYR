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
    
    public partial class Pedidos
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Pedidos()
        {
            this.PedidoDetalle = new HashSet<PedidoDetalle>();
        }
    
        public int Id { get; set; }
        public string IdUsuario { get; set; }
        public int IdEstado { get; set; }
        public System.DateTime Fecha { get; set; }
        public double TotalPagar { get; set; }
        public double TotalDescuento { get; set; }
        public double TotalImpuesto { get; set; }
        public Nullable<int> NumeroProforma { get; set; }
    
        public virtual AspNetUsers AspNetUsers { get; set; }
        public virtual EstadoPedido EstadoPedido { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PedidoDetalle> PedidoDetalle { get; set; }
    }
}