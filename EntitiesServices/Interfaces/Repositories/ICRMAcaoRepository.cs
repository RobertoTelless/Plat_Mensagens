using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;

namespace ModelServices.Interfaces.Repositories
{
    public interface ICRMAcaoRepository : IRepositoryBase<CRM_ACAO>
    {
        List<CRM_ACAO> GetAllItens();
        CRM_ACAO GetItemById(Int32 id);
    }
}
