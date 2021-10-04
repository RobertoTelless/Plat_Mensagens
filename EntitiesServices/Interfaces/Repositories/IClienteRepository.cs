using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;

namespace ModelServices.Interfaces.Repositories
{
    public interface IClienteRepository : IRepositoryBase<CLIENTE>
    {
        CLIENTE CheckExist(CLIENTE item, Int32 idAss);
        CLIENTE GetByEmail(String email, Int32 idAss);
        CLIENTE GetItemById(Int32 id);
        List<CLIENTE> GetAllItens(Int32 idAss);
        List<CLIENTE> GetAllItensAdm(Int32 idAss);
        List<CLIENTE> ExecuteFilter(Int32? id, Int32? catId, String razao, String nome, String cpf, String cnpj, String email, String cidade, Int32? uf, Int32? status, Int32? ativo, Int32 idAss);
    }
}
