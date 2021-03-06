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
    
    public partial class CLIENTE
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public CLIENTE()
        {
            this.CLIENTE_ANEXO = new HashSet<CLIENTE_ANEXO>();
            this.CLIENTE_CONTATO = new HashSet<CLIENTE_CONTATO>();
            this.CLIENTE_QUADRO_SOCIETARIO = new HashSet<CLIENTE_QUADRO_SOCIETARIO>();
            this.GRUPO_CLIENTE = new HashSet<GRUPO_CLIENTE>();
            this.MENSAGENS_DESTINOS = new HashSet<MENSAGENS_DESTINOS>();
            this.EMAIL_AGENDAMENTO = new HashSet<EMAIL_AGENDAMENTO>();
            this.CRM = new HashSet<CRM>();
        }
    
        public int CLIE_CD_ID { get; set; }
        public int ASSI_CD_ID { get; set; }
        public Nullable<int> CACL_CD_ID { get; set; }
        public Nullable<int> USUA_CD_ID { get; set; }
        public Nullable<int> TIPE_CD_ID { get; set; }
        public string CLIE_NM_NOME { get; set; }
        public string CLIE_NM_RAZAO { get; set; }
        public string CLIE_NR_CPF { get; set; }
        public string CLIE_NR_CNPJ { get; set; }
        public string CLIE_NM_EMAIL { get; set; }
        public string CLIE_NR_TELEFONE { get; set; }
        public string CLIE_NM_REDES_SOCIAIS { get; set; }
        public string CLIE_NM_ENDERECO { get; set; }
        public string CLIE_NR_NUMERO { get; set; }
        public string CLIE_NM_BAIRRO { get; set; }
        public string CLIE_NM_CIDADE { get; set; }
        public string CLIE_NR_CEP { get; set; }
        public System.DateTime CLIE_DT_CADASTRO { get; set; }
        public int CLIE_IN_ATIVO { get; set; }
        public string CLIE_AQ_FOTO { get; set; }
        public string CLIE_NR_INSCRICAO_ESTADUAL { get; set; }
        public string CLIE_NR_INSCRICAO_MUNICIPAL { get; set; }
        public string CLIE_NR_CELULAR { get; set; }
        public string CLIE_NM_WEBSITE { get; set; }
        public string CLIE_NM_PAI { get; set; }
        public string CLIE_NM_MAE { get; set; }
        public Nullable<System.DateTime> CLIE_DT_NASCIMENTO { get; set; }
        public string CLIE_NM_NATURALIDADE { get; set; }
        public string CLIE_SG_NATURALIADE_UF { get; set; }
        public string CLIE_NM_NACIONALIDADE { get; set; }
        public string CLIE_TX_OBSERVACOES { get; set; }
        public string CLIE_NR_TELEFONE_ADICIONAL { get; set; }
        public Nullable<int> UF_CD_ID { get; set; }
        public string CLIE_NM_COMPLEMENTO { get; set; }
        public string CLIE_NR_WHATSAPP { get; set; }
        public Nullable<int> CLIE_IN_SEXO { get; set; }
        public string CLIE_NM_SITUACAO { get; set; }
        public Nullable<int> CLIE_IN_STATUS { get; set; }
    
        public virtual ASSINANTE ASSINANTE { get; set; }
        public virtual CATEGORIA_CLIENTE CATEGORIA_CLIENTE { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CLIENTE_ANEXO> CLIENTE_ANEXO { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CLIENTE_CONTATO> CLIENTE_CONTATO { get; set; }
        public virtual POSICAO POSICAO { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CLIENTE_QUADRO_SOCIETARIO> CLIENTE_QUADRO_SOCIETARIO { get; set; }
        public virtual TIPO_PESSOA TIPO_PESSOA { get; set; }
        public virtual UF UF { get; set; }
        public virtual USUARIO USUARIO { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<GRUPO_CLIENTE> GRUPO_CLIENTE { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<MENSAGENS_DESTINOS> MENSAGENS_DESTINOS { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<EMAIL_AGENDAMENTO> EMAIL_AGENDAMENTO { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CRM> CRM { get; set; }
    }
}
