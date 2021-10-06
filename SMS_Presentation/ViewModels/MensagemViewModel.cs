    using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using EntitiesServices.Model;
using EntitiesServices.Attributes;

namespace PlatMensagem_Solution.ViewModels
{
    public class MensagemViewModel
    {
        [Key]
        public int MENS_CD_ID { get; set; }
        public int ASSI_CD_ID { get; set; }
        public int USUA_CD_ID { get; set; }
        public Nullable<int> TEMP_CD_ID { get; set; }
        public Nullable<int> MENS_IN_ATIVO { get; set; }
        [Required(ErrorMessage = "Campo DATA DE CRIAÇÃO obrigatorio")]
        [DataType(DataType.Date, ErrorMessage = "A DATA DE CRIAÇÂO deve ser uma data válida")]
        public Nullable<System.DateTime> MENS_DT_CRIACAO { get; set; }
        [StringLength(50, ErrorMessage = "O NOME DA CAMPANHA deve conter no máximo 50 caracteres.")]
        public string MENS_NM_CAMPANHA { get; set; }
        [Required(ErrorMessage = "Campo TEXTO DA MENSAGEM obrigatorio")]
        [StringLength(5000, MinimumLength = 2, ErrorMessage = "O TEXTO DA MENSAGEM deve conter no minimo 2 e no máximo 5000 caracteres.")]
        public string MENS_TX_TEXTO { get; set; }
        [Required(ErrorMessage = "Campo TIPO DE MENSAGEM obrigatorio")]
        public Nullable<int> MENS_IN_TIPO { get; set; }
        [DataType(DataType.Date, ErrorMessage = "A DATA DE AGENDAMENTO deve ser uma data válida")]
        public Nullable<System.DateTime> MENS_DT_AGENDAMENTO { get; set; }
        [DataType(DataType.Date, ErrorMessage = "A DATA DE ENVIO deve ser uma data válida")]
        public Nullable<System.DateTime> MENS_DT_ENVIO { get; set; }
        public string MENS_TX_RETORNO { get; set; }
        public string MENS_NM_CABECALHO { get; set; }
        public string MENS_NM_RODAPE { get; set; }

        public Int32? SEXO { get; set; }
        public string NOME { get; set; }
        public Int32? ID { get; set; }
        public string CIDADE { get; set; }
        public Nullable<System.DateTime> DATA_NASC{ get; set; }
        public Int32? UF { get; set; }
        public Int32? CATEGORIA { get; set; }
        public Int32? STATUS { get; set; }

        public virtual ASSINANTE ASSINANTE { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<MENSAGENS_DESTINOS> MENSAGENS_DESTINOS { get; set; }
        public virtual TEMPLATE TEMPLATE { get; set; }
        public virtual USUARIO USUARIO { get; set; }
    }
}