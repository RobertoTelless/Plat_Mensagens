using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;
using EntitiesServices.Work_Classes;

namespace ModelServices.Interfaces.EntitiesServices
{
    public interface ICRMService : IServiceBase<CRM>
    {
        Int32 Create(CRM tarefa, LOG log);
        Int32 Create(CRM tarefa);
        Int32 Edit(CRM tarefa, LOG log);
        Int32 Edit(CRM tarefa);
        Int32 Delete(CRM tarefa, LOG log);

        CRM CheckExist(CRM item, Int32 idUsu, Int32 idAss);
        List<CRM> GetByDate(DateTime data, Int32 idAss);
        List<CRM> GetByUser(Int32 user);
        List<CRM> GetTarefaStatus(Int32 tipo, Int32 idAss);
        CRM GetItemById(Int32 id);
        List<CRM> GetAllItens(Int32 idAss);
        List<CRM> GetAllItensAdm(Int32 idAss);
        List<CRM> ExecuteFilter(Int32? tipoId, String nome, String descricao, Int32? idCli, DateTime? data, Int32? status, Int32? usuario, Int32 idAss);


        List<USUARIO> GetAllUsers(Int32 idAss);
        List<TIPO_CRM> GetAllTipos();
        CRM_ANEXO GetAnexoById(Int32 id);
        USUARIO GetUserById(Int32 id);
    }
}
