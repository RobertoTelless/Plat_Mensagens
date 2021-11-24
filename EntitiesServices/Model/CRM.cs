//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace EntitiesServices.Model
{
    using System;
    using System.Collections.Generic;
    
    public partial class CRM
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public CRM()
        {
            this.CRM_ANEXO = new HashSet<CRM_ANEXO>();
            this.CRM_COMENTARIO = new HashSet<CRM_COMENTARIO>();
        }
    
        public int CRM1_CD_ID { get; set; }
        public int ASSI_CD_ID { get; set; }
        public int CLIE_CD_ID { get; set; }
        public Nullable<int> TICR_CD_ID { get; set; }
        public Nullable<System.DateTime> CRM1_DT_CRIACAO { get; set; }
        public string CRM1_DS_DESCRICAO { get; set; }
        public string CRM1_TX_INFORMACOES_GERAIS { get; set; }
        public int CRM1_IN_STATUS { get; set; }
        public Nullable<System.DateTime> CRM1_DT_CANCELAMENTO { get; set; }
        public string CRM1_DS_MOTIVO_CANCELAMENTO { get; set; }
        public Nullable<System.DateTime> CRM1_DT_ENCERRAMENTO { get; set; }
        public string CRM1_DS_INFORMACOES_ENCERRAMENTO { get; set; }
        public Nullable<int> CRM1_IN_ATIVO { get; set; }
        public string CRM1_NM_NOME { get; set; }
        public Nullable<int> USUA_CD_ID { get; set; }
        public Nullable<int> MENS_CD_ID { get; set; }
    
        public virtual ASSINANTE ASSINANTE { get; set; }
        public virtual CLIENTE CLIENTE { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CRM_ANEXO> CRM_ANEXO { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CRM_COMENTARIO> CRM_COMENTARIO { get; set; }
        public virtual TIPO_CRM TIPO_CRM { get; set; }
        public virtual USUARIO USUARIO { get; set; }
        public virtual MENSAGENS MENSAGENS { get; set; }
    }
}