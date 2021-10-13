using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;
using EntitiesServices.Work_Classes;

namespace ModelServices.Interfaces.EntitiesServices
{
    public interface IMensagemService : IServiceBase<MENSAGENS>
    {
        Int32 Create(MENSAGENS item, LOG log);
        Int32 Create(MENSAGENS item);
        Int32 Edit(MENSAGENS item, LOG log);
        Int32 Edit(MENSAGENS item);
        Int32 Delete(MENSAGENS item, LOG log);

        MENSAGENS CheckExist(MENSAGENS item, Int32 idAss);
        MENSAGENS GetItemById(Int32 id);
        List<MENSAGENS> GetAllItens(Int32 idAss);
        List<MENSAGENS> GetAllItensAdm(Int32 idAss);
        MENSAGEM_ANEXO GetAnexoById(Int32 id);
        List<MENSAGENS> ExecuteFilter(DateTime? criacao, DateTime? envio, String campanha, String texto, Int32? tipo, Int32 idAss);

        List<TEMPLATE> GetAllTemplates();
        List<UF> GetAllUF();
        UF GetUFbySigla(String sigla);
        List<CATEGORIA_CLIENTE> GetAllTipos();
    }
}
