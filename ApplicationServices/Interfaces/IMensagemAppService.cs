using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;

namespace ApplicationServices.Interfaces
{
    public interface IMensagemAppService : IAppServiceBase<MENSAGENS>
    {
        Int32 ValidateCreate(MENSAGENS item, USUARIO usuario);
        Int32 ValidateEdit(MENSAGENS item, MENSAGENS perfilAntes, USUARIO usuario);
        Int32 ValidateEdit(MENSAGENS item, MENSAGENS itemAntes);
        Int32 ValidateDelete(MENSAGENS item, USUARIO usuario);
        Int32 ValidateReativar(MENSAGENS item, USUARIO usuario);

        List<MENSAGENS> GetAllItens(Int32 idAss);
        List<MENSAGENS> GetAllItensAdm(Int32 idAss);
        MENSAGENS GetItemById(Int32 id);
        MENSAGENS CheckExist(MENSAGENS conta, Int32 idAss);
        Int32 ExecuteFilter(DateTime? criacao, DateTime? envio, String campanha, String texto, Int32? tipo, Int32 idAss, out List<MENSAGENS> objeto);

        List<CATEGORIA_CLIENTE> GetAllTipos();
        List<UF> GetAllUF();
        UF GetUFbySigla(String sigla);
        List<TEMPLATE> GetAllTemplates();
    }
}