using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using EntitiesServices.Model;

namespace PlatMensagem_Solution.ViewModels
{
    public class CRMViewModel
    {
        [Key]
        public int CRM1_CD_ID { get; set; }
        public int ASSI_CD_ID { get; set; }
        [Required(ErrorMessage = "Campo CONTATO obrigatorio")]
        public int CLIE_CD_ID { get; set; }
        public Nullable<int> TICR_CD_ID { get; set; }
        [Required(ErrorMessage = "Campo DATA DE CRIA��O obrigatorio")]
        [DataType(DataType.Date, ErrorMessage = "A DATA DE CRIA��O deve ser uma data v�lida")]
        public Nullable<System.DateTime> CRM1_DT_CRIACAO { get; set; }
        [StringLength(500, ErrorMessage = "A DESCRI��O deve conter no m�ximo 500 caracteres.")]
        public string CRM1_DS_DESCRICAO { get; set; }
        [StringLength(5000, ErrorMessage = "AS INFORMA��ES GERAIS devem conter no m�ximo 5000 caracteres.")]
        public string CRM1_TX_INFORMACOES_GERAIS { get; set; }
        [Required(ErrorMessage = "Campo STATUS obrigatorio")]
        public int CRM1_IN_STATUS { get; set; }
        [DataType(DataType.Date, ErrorMessage = "A DATA DE CANCELAMENTO deve ser uma data v�lida")]
        public Nullable<System.DateTime> CRM1_DT_CANCELAMENTO { get; set; }
        [StringLength(500, ErrorMessage = "O MOTIVO DE CANCELAMENTO deve conter no m�ximo 500 caracteres.")]
        public string CRM1_DS_MOTIVO_CANCELAMENTO { get; set; }
        [DataType(DataType.Date, ErrorMessage = "A DATA DE ENCERRAMENTO deve ser uma data v�lida")]
        public Nullable<System.DateTime> CRM1_DT_ENCERRAMENTO { get; set; }
        [StringLength(5000, ErrorMessage = "AS INFORMA��ES DE ENCERRAMENTO devem conter no m�ximo 5000 caracteres.")]
        public string CRM1_DS_INFORMACOES_ENCERRAMENTO { get; set; }
        public Nullable<int> CRM1_IN_ATIVO { get; set; }
        [Required(ErrorMessage = "Campo NOME obrigatorio")]
        [StringLength(150, MinimumLength = 2, ErrorMessage = "O NOME deve conter no minimo 2 e no m�ximo 150 caracteres.")]
        public string CRM1_NM_NOME { get; set; }
        public Nullable<int> USUA_CD_ID { get; set; }

        public virtual ASSINANTE ASSINANTE { get; set; }
        public virtual CLIENTE CLIENTE { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CRM_ANEXO> CRM_ANEXO { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CRM_COMENTARIO> CRM_COMENTARIO { get; set; }
        public virtual TIPO_CRM TIPO_CRM { get; set; }
        public virtual USUARIO USUARIO { get; set; }
    }
}