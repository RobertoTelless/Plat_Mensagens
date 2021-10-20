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

namespace DataServices.Repositories
{
    public class PosicaoRepository : RepositoryBase<POSICAO>, IPosicaoRepository
    {
        public POSICAO CheckExist(POSICAO conta)
        {
            IQueryable<POSICAO> query = Db.POSICAO;
            query = query.Where(p => p.POSI_NM_NOME == conta.POSI_NM_NOME);
            return query.FirstOrDefault();
        }

        public POSICAO GetItemById(Int32 id)
        {
            IQueryable<POSICAO> query = Db.POSICAO;
            query = query.Where(p => p.POSI_CD_ID == id);
            return query.FirstOrDefault();
        }

        public List<POSICAO> GetAllItensAdm()
        {
            IQueryable<POSICAO> query = Db.POSICAO;
            return query.ToList();
        }

        public List<POSICAO> GetAllItens()
        {
            IQueryable<POSICAO> query = Db.POSICAO.Where(p => p.POSI_IN_ATIVO == 1);
            return query.ToList();
        }

    }
}
 