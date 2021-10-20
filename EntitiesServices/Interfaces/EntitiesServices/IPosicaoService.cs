using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;
using EntitiesServices.Work_Classes;

namespace ModelServices.Interfaces.EntitiesServices
{
    public interface IPosicaoService : IServiceBase<POSICAO>
    {
        Int32 Create(POSICAO item, LOG log);
        Int32 Create(POSICAO item);
        Int32 Edit(POSICAO item, LOG log);
        Int32 Edit(POSICAO item);
        Int32 Delete(POSICAO item, LOG log);

        POSICAO CheckExist(POSICAO item);
        POSICAO GetItemById(Int32 id);
        List<POSICAO> GetAllItens();
        List<POSICAO> GetAllItensAdm();

    }
}
