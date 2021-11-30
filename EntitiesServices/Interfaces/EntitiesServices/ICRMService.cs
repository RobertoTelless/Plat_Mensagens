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
        List<CRM> ExecuteFilter(Int32? status, DateTime? inicio, DateTime? final, Int32? origem, Int32? adic, String nome, String busca, Int32? estrela, Int32 idAss);


        List<USUARIO> GetAllUsers(Int32 idAss);
        List<TIPO_CRM> GetAllTipos();
        List<TIPO_ACAO> GetAllTipoAcao();
        List<CRM_ORIGEM> GetAllOrigens();
        List<MOTIVO_CANCELAMENTO> GetAllMotivoCancelamento();
        List<MOTIVO_ENCERRAMENTO> GetAllMotivoEncerramento();
        CRM_ANEXO GetAnexoById(Int32 id);
        USUARIO GetUserById(Int32 id);
        CRM_COMENTARIO GetComentarioById(Int32 id);

        Int32 EditContato(CRM_CONTATO item);
        Int32 CreateContato(CRM_CONTATO item);
        Int32 EditAcao(CRM_ACAO item);
        Int32 CreateAcao(CRM_ACAO item);
    }
}
