using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Seguridad_autorizacion_autenticacion.DTOs
{
    public class EditarAminDTO
    {
        [Required]
        [EmailAddress]
        public string Email{ get; set; }
    }
}
