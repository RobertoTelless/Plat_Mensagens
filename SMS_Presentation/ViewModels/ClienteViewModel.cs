using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using EntitiesServices.Model;
using EntitiesServices.Attributes;

namespace PlatMensagem_Solution.ViewModels
{
    public class ClienteViewModel
    {
        [Key]
        public int CLIE_CD_ID { get; set; }
        public int ASSI_CD_ID { get; set; }
        public Nullable<int> USUA_CD_ID { get; set; }
        [Required(ErrorMessage = "Campo CATEGORIA obrigatorio")]
        public Nullable<int> CACL_CD_ID { get; set; }
        [Required(ErrorMessage = "Campo TIPO DE PESSOA obrigatorio")]
        public Nullable<int> TIPE_CD_ID { get; set; }
        [Required(ErrorMessage = "Campo NOME obrigatorio")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "O NOME deve conter no minimo 1  e no máximo 50 caracteres.")]
        public string CLIE_NM_NOME { get; set; }
        [StringLength(100, ErrorMessage = "A RAZÃO SOCIAL deve conter no máximo 100 caracteres.")]
        public string CLIE_NM_RAZAO { get; set; }
        [StringLength(20, MinimumLength = 11, ErrorMessage = "O CPF deve conter no minimo 11 e no máximo 20 caracteres.")]
        [CustomValidationCPF(ErrorMessage = "CPF inválido")]
        public string CLIE_NR_CPF { get; set; }
        [StringLength(20, MinimumLength = 14, ErrorMessage = "O CNPJ deve conter no minimo 14  e no máximo 20 caracteres.")]
        [CustomValidationCNPJ(ErrorMessage = "CNPJ inválido")]
        public string CLIE_NR_CNPJ { get; set; }
        [StringLength(20, MinimumLength = 1, ErrorMessage = "O RG deve conter no minimo 1 e no máximo 20 caracteres.")]
        public string CLIE_NR_RG { get; set; }
        [Required(ErrorMessage = "Campo E-MAIL obrigatorio")]
        [StringLength(150, MinimumLength = 1, ErrorMessage = "O E-MAIL deve conter no minimo 1 e no máximo 150 caracteres.")]
        [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "Deve ser um e-mail válido")]
        public string CLIE_NM_EMAIL { get; set; }
        [StringLength(50, ErrorMessage = "O TELEFONE deve conter no máximo 50 caracteres.")]
        public string CLIE_NR_TELEFONE { get; set; }
        [StringLength(50, ErrorMessage = "AS REDES SOCIAIS deve conter no máximo 50 caracteres.")]
        public string CLIE_NM_REDES_SOCIAIS { get; set; }
        public System.DateTime CLIE_DT_CADASTRO { get; set; }
        public Nullable<int> CLIE_IN_ATIVO { get; set; }
        public string CLIE_AQ_FOTO { get; set; }
        [StringLength(50, ErrorMessage = "A INSCRIÇÃO ESTADUAL deve conter no máximo 50.")]
        public string CLIE_NR_INSCRICAO_ESTADUAL { get; set; }
        [StringLength(50, ErrorMessage = "A INSCRIÇÃO MUNICIPAL deve conter no máximo 50.")]
        public string CLIE_NR_INSCRICAO_MUNICIPAL { get; set; }
        [StringLength(50, ErrorMessage = "O CELULAR deve conter no máximo 50 caracteres.")]
        public string CLIE_NR_CELULAR { get; set; }
        [StringLength(50, ErrorMessage = "O WEBSITE deve conter no máximo 50 caracteres.")]
        public string CLIE_NM_WEBSITE { get; set; }
        [DataType(DataType.Date, ErrorMessage = "A DATA DE NASCIMENTO deve ser uma data válida")]
        public Nullable<System.DateTime> CLIE_DT_NASCIMENTO { get; set; }
        public string CLIE_TX_OBSERVACOES { get; set; }
        [StringLength(100, ErrorMessage = "O ENDEREÇO deve conter no máximo 100 caracteres.")]
        public string CLIE_NM_ENDERECO { get; set; }
        [StringLength(50, ErrorMessage = "O BAIRRO deve conter no máximo 50 caracteres.")]
        public string CLIE_NM_BAIRRO { get; set; }
        [StringLength(50, ErrorMessage = "A CIDADE deve conter no máximo 50 caracteres.")]
        public string CLIE_NM_CIDADE { get; set; }
        [StringLength(10, ErrorMessage = "O CEP deve conter no máximo 10 caracteres.")]
        public string CLIE_NR_CEP { get; set; }
        public Nullable<int> UF_CD_ID { get; set; }
        [StringLength(50, ErrorMessage = "O NOME DO PAI deve conter no máximo 50 caracteres.")]
        public string CLIE_NM_PAI { get; set; }
        [StringLength(50, ErrorMessage = "O NOME DA MÃE deve conter no máximo 50 caracteres.")]
        public string CLIE_NM_MAE { get; set; }
        [StringLength(50, ErrorMessage = "A NATURALIDADE deve conter no máximo 50 caracteres.")]
        public string CLIE_NM_NATURALIDADE { get; set; }
        public string CLIE_SG_NATURALIADE_UF { get; set; }
        [StringLength(50, ErrorMessage = "A NACIONALIDADE deve conter no máximo 50 caracteres.")]
        public string CLIE_NM_NACIONALIDADE { get; set; }
        [StringLength(50, ErrorMessage = "O COMPLEMENTO deve conter no máximo 50 caracteres.")]
        public string CLIE_NM_COMPLEMENTO { get; set; }
        [StringLength(50, ErrorMessage = "A SITUAÇÃO deve conter no máximo 50 caracteres.")]
        public string CLIE_NM_SITUACAO { get; set; }
        [StringLength(50, ErrorMessage = "O NÚMERO deve conter no máximo 50 caracteres.")]
        public string CLIE_NR_NUMERO { get; set; }
        [StringLength(50, ErrorMessage = "O WHATSAPP deve conter no máximo 50 caracteres.")]
        public string CLIE_NR_WHATSAPP { get; set; }
        public Nullable<int> CLIE_IN_SEXO { get; set; }
        [Required(ErrorMessage = "Campo STATUS obrigatorio")]
        public Nullable<int> CLIE_IN_STATUS { get; set; }

        public virtual ASSINANTE ASSINANTE { get; set; }
        public virtual CATEGORIA_CLIENTE CATEGORIA_CLIENTE { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CLIENTE_ANEXO> CLIENTE_ANEXO { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CLIENTE_CONTATO> CLIENTE_CONTATO { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CLIENTE_QUADRO_SOCIETARIO> CLIENTE_QUADRO_SOCIETARIO { get; set; }
        public virtual TIPO_PESSOA TIPO_PESSOA { get; set; }
        public string CLIE_NR_TELEFONE_ADICIONAL { get; set; }
        public virtual UF UF { get; set; }
        public virtual USUARIO USUARIO { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<GRUPO_CLIENTE> GRUPO_CLIENTE { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<MENSAGENS_DESTINOS> MENSAGENS_DESTINOS { get; set; }
    }
}