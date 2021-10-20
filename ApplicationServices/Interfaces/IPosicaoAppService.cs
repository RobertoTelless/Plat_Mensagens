using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;

namespace ApplicationServices.Interfaces
{
    public interface IPosicaoAppService : IAppServiceBase<POSICAO>
    {
        Int32 ValidateCreate(POSICAO item, USUARIO usuario);
        Int32 ValidateEdit(POSICAO item, POSICAO perfilAntes, USUARIO usuario);
        Int32 ValidateEdit(POSICAO item, POSICAO itemAntes);
        Int32 ValidateDelete(POSICAO item, USUARIO usuario);
        Int32 ValidateReativar(POSICAO item, USUARIO usuario);

        List<POSICAO> GetAllItens();
        List<POSICAO> GetAllItensAdm();
        POSICAO GetItemById(Int32 id);
        POSICAO CheckExist(POSICAO conta);
    }
}
