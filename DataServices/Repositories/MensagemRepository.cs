using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;
using ModelServices.Interfaces.Repositories;
using EntitiesServices.Work_Classes;
using System.Data.Entity;
using CrossCutting;

namespace DataServices.Repositories
{
    public class MensagemRepository : RepositoryBase<MENSAGENS>, IMensagemRepository
    {
        public MENSAGENS CheckExist(MENSAGENS conta, Int32 idAss)
        {
            IQueryable<MENSAGENS> query = Db.MENSAGENS;
            query = query.Where(p => p.MENS_DT_CRIACAO == conta.MENS_DT_CRIACAO);
            query = query.Where(p => p.ASSI_CD_ID == idAss);
            return query.FirstOrDefault();
        }

        public MENSAGENS GetItemById(Int32 id)
        {
            IQueryable<MENSAGENS> query = Db.MENSAGENS;
            query = query.Where(p => p.MENS_CD_ID == id);
            query = query.Include(p => p.MENSAGENS_DESTINOS);
            return query.FirstOrDefault();
        }

        public List<MENSAGENS> GetAllItens(Int32 idAss)
        {
            IQueryable<MENSAGENS> query = Db.MENSAGENS.Where(p => p.MENS_IN_ATIVO == 1);
            query = query.Where(p => p.ASSI_CD_ID == idAss);
            query = query.OrderBy(a => a.MENS_DT_CRIACAO);
            return query.ToList();
        }

        public List<MENSAGENS> GetAllItensAdm(Int32 idAss)
        {
            IQueryable<MENSAGENS> query = Db.MENSAGENS;
            query = query.Where(p => p.ASSI_CD_ID == idAss);
            query = query.OrderBy(a => a.MENS_DT_CRIACAO);
            return query.ToList();
        }

        public List<MENSAGENS> ExecuteFilter(DateTime? criacao, DateTime? envio, String campanha, String texto, Int32? tipo, Int32 idAss)
        {
            List<MENSAGENS> lista = new List<MENSAGENS>();
            IQueryable<MENSAGENS> query = Db.MENSAGENS;
            if (tipo != null)
            {
                query = query.Where(p => p.MENS_IN_TIPO == tipo);
            }
            if (!String.IsNullOrEmpty(campanha))
            {
                query = query.Where(p => p.MENS_NM_CAMPANHA.Contains(campanha));
            }
            if (!String.IsNullOrEmpty(texto))
            {
                query = query.Where(p => p.MENS_TX_TEXTO.Contains(texto));
            }
            if (criacao != null)
            {
                query = query.Where(p => DbFunctions.TruncateTime(p.MENS_DT_CRIACAO) == DbFunctions.TruncateTime(criacao));
            }
            if (envio != null)
            {
                query = query.Where(p => DbFunctions.TruncateTime(p.MENS_DT_ENVIO) == DbFunctions.TruncateTime(envio));
            }
            if (query != null)
            {
                query = query.Where(p => p.ASSI_CD_ID == idAss);
                query = query.OrderBy(a => a.MENS_DT_CRIACAO);
                lista = query.ToList<MENSAGENS>();
            }
            return lista;
        }
    }
}
 