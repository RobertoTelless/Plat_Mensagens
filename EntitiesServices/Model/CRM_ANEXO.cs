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
    
    public partial class CRM_ANEXO
    {
        public int CRAN_CD_ID { get; set; }
        public int CRM1_CD_ID { get; set; }
        public string CRAN_NM_TITULO { get; set; }
        public Nullable<System.DateTime> CRAN_DT_ANEXO { get; set; }
        public Nullable<int> CRAN_IN_TIPO { get; set; }
        public string CRAN_AQ_ARQUIVO { get; set; }
        public Nullable<int> CRAN_IN_ATIVO { get; set; }
    
        public virtual CRM CRM { get; set; }
    }
}
