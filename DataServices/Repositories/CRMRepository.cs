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
    public class CRMRepository : RepositoryBase<CRM>, ICRMRepository
    {
        public CRM CheckExist(CRM tarefa, Int32 idUsu, Int32 idAss)
        {
            IQueryable<CRM> query = Db.CRM;
            query = query.Where(p => p.CRM1_NM_NOME == tarefa.CRM1_NM_NOME);
            query = query.Where(p => p.CLIE_CD_ID == tarefa.CLIE_CD_ID);
            query = query.Where(p => p.USUA_CD_ID == idUsu);
            query = query.Where(p => p.ASSI_CD_ID == idAss);
            return query.FirstOrDefault();
        }

        public List<CRM> GetByDate(DateTime data, Int32 idAss)
        {
            IQueryable<CRM> query = Db.CRM.Where(p => p.CRM1_IN_ATIVO == 1);
            query = query.Where(p => p.CRM1_DT_CRIACAO == data);
            query = query.Where(p => p.ASSI_CD_ID == idAss);
            return query.ToList();
        }

        public List<CRM> GetByUser(Int32 user)
        {
            IQueryable<CRM> query = Db.CRM.Where(p => p.CRM1_IN_ATIVO == 1);
            query = query.Where(p => p.USUA_CD_ID == user);
            return query.ToList();
        }

        public List<CRM> GetTarefaStatus(Int32 tipo, Int32 idAss)
        {
            IQueryable<CRM> query = Db.CRM.Where(p => p.CRM1_IN_ATIVO == 1);
            query = query.Where(p => p.ASSI_CD_ID == idAss);
            query = query.Where(p => p.CRM1_IN_STATUS == tipo);
            return query.ToList();
        }

        public CRM GetItemById(Int32 id)
        {
            IQueryable<CRM> query = Db.CRM;
            query = query.Where(p => p.CRM1_CD_ID == id);
            query = query.Include(p => p.CRM_COMENTARIO);
            return query.FirstOrDefault();
        }

        public List<CRM> GetAllItens(Int32 idUsu)
        {
            IQueryable<CRM> query = Db.CRM.Where(p => p.CRM1_IN_ATIVO == 1);
            query = query.Where(p => p.ASSI_CD_ID == idUsu);
            return query.ToList();
        }

        public List<CRM> GetAllItensAdm(Int32 idUsu)
        {
            IQueryable<CRM> query = Db.CRM;
            query = query.Where(p => p.ASSI_CD_ID == idUsu);
            return query.ToList();
        }

        public List<CRM> ExecuteFilter(Int32? tipoId, String nome, String descricao, Int32? idCli, DateTime? data, Int32? status, Int32? usuario, Int32 idAss)
        {
            List<CRM> lista = new List<CRM>();
            IQueryable<CRM> query = Db.CRM;
            if (tipoId != null)
            {
                query = query.Where(p => p.TICR_CD_ID == tipoId);
            }
            if (!String.IsNullOrEmpty(nome))
            {
                query = query.Where(p => p.CRM1_NM_NOME.Contains(nome));
            }
            if (!String.IsNullOrEmpty(descricao))
            {
                query = query.Where(p => p.CRM1_DS_DESCRICAO.Contains(descricao));
            }
            if (idCli != null)
            {
                query = query.Where(p => p.CLIE_CD_ID == idCli);
            }
            if (data != DateTime.MinValue)
            {
                query = query.Where(p => p.CRM1_DT_CRIACAO == data);
            }
            if (status != null)
            {
                query = query.Where(p => p.CRM1_IN_STATUS == status);
            }
            if (usuario != null)
            {
                query = query.Where(p => p.USUA_CD_ID == usuario);
            }
            if (query != null)
            {
                query = query.Where(p => p.ASSI_CD_ID == idAss);
                query = query.OrderBy(a => a.CRM1_DT_CRIACAO);
                lista = query.ToList<CRM>();
            }
            return lista;
        }
    }
}
 