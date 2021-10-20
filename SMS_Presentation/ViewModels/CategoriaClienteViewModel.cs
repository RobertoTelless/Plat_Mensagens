using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using EntitiesServices.Model;

namespace PlatMensagem_Solution.ViewModels
{
    public class CategoriaClienteViewModel
    {
        [Key]
        public int CACL_CD_ID { get; set; }
        [Required(ErrorMessage = "Campo NOME obrigatorio")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "O NOME deve conter no minimo 2 e no máximo 50 caracteres.")]
        public string CACL_NM_NOME { get; set; }
        public Nullable<int> CACL_IN_ATIVO { get; set; }
        public Nullable<int> ASSI_CD_ID { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CLIENTE> CLIENTE { get; set; }
        public virtual ASSINANTE ASSINANTE { get; set; }
    }
}