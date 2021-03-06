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
    public class CategoriaClienteRepository : RepositoryBase<CATEGORIA_CLIENTE>, ICategoriaClienteRepository
    {
        public CATEGORIA_CLIENTE CheckExist(CATEGORIA_CLIENTE conta)
        {
            IQueryable<CATEGORIA_CLIENTE> query = Db.CATEGORIA_CLIENTE;
            query = query.Where(p => p.CACL_NM_NOME == conta.CACL_NM_NOME);
            return query.FirstOrDefault();
        }

        public CATEGORIA_CLIENTE GetItemById(Int32 id)
        {
            IQueryable<CATEGORIA_CLIENTE> query = Db.CATEGORIA_CLIENTE;
            query = query.Where(p => p.CACL_CD_ID == id);
            return query.FirstOrDefault();
        }

        public List<CATEGORIA_CLIENTE> GetAllItensAdm()
        {
            IQueryable<CATEGORIA_CLIENTE> query = Db.CATEGORIA_CLIENTE;
            return query.ToList();
        }

        public List<CATEGORIA_CLIENTE> GetAllItens()
        {
            IQueryable<CATEGORIA_CLIENTE> query = Db.CATEGORIA_CLIENTE.Where(p => p.CACL_IN_ATIVO == 1);
            return query.ToList();
        }

    }
}
 