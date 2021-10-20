using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;

namespace ModelServices.Interfaces.Repositories
{
    public interface IPosicaoRepository : IRepositoryBase<POSICAO>
    {
        POSICAO CheckExist(POSICAO item);
        List<POSICAO> GetAllItens();
        POSICAO GetItemById(Int32 id);
        List<POSICAO> GetAllItensAdm();
    }
}
