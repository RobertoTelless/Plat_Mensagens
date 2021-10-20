using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;

namespace ApplicationServices.Interfaces
{
    public interface ICategoriaClienteAppService : IAppServiceBase<CATEGORIA_CLIENTE>
    {
        Int32 ValidateCreate(CATEGORIA_CLIENTE item, USUARIO usuario);
        Int32 ValidateEdit(CATEGORIA_CLIENTE item, CATEGORIA_CLIENTE perfilAntes, USUARIO usuario);
        Int32 ValidateEdit(CATEGORIA_CLIENTE item, CATEGORIA_CLIENTE itemAntes);
        Int32 ValidateDelete(CATEGORIA_CLIENTE item, USUARIO usuario);
        Int32 ValidateReativar(CATEGORIA_CLIENTE item, USUARIO usuario);

        List<CATEGORIA_CLIENTE> GetAllItens();
        List<CATEGORIA_CLIENTE> GetAllItensAdm();
        CATEGORIA_CLIENTE GetItemById(Int32 id);
        CATEGORIA_CLIENTE CheckExist(CATEGORIA_CLIENTE conta);
    }
}
