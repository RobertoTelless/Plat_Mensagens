using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;
using ModelServices.Interfaces.Repositories;
using EntitiesServices.Work_Classes;
using System.Data.Entity;

namespace DataServices.Repositories
{
    public class CRMAcaoRepository : RepositoryBase<CRM_ACAO>, ICRMAcaoRepository
    {
        public List<CRM_ACAO> GetAllItens()
        {
            return Db.CRM_ACAO.ToList();
        }

        public CRM_ACAO GetItemById(Int32 id)
        {
            IQueryable<CRM_ACAO> query = Db.CRM_ACAO.Where(p => p.CRAC_CD_ID == id);
            return query.FirstOrDefault();
        }
    }
}
 