using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Seguridad_autorizacion_autenticacion.DTOs
{
    public class ResultadoHash
    {
        public string Hash { get; set; }
        public byte[] Sal { get; set; }
    }
}
