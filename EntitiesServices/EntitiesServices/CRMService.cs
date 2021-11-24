using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using EntitiesServices.Model;
using EntitiesServices.Work_Classes;
using ModelServices.Interfaces.Repositories;
using ModelServices.Interfaces.EntitiesServices;
using CrossCutting;
using System.Data.Entity;
using System.Data;

namespace ModelServices.EntitiesServices
{
    public class CRMService : ServiceBase<CRM>, ICRMService
    {
        private readonly ICRMRepository _baseRepository;
        private readonly ILogRepository _logRepository;
        private readonly ITipoCRMRepository _tipoRepository;
        private readonly ICRMAnexoRepository _anexoRepository;
        private readonly IUsuarioRepository _usuRepository;
        protected PlatMensagensEntities Db = new PlatMensagensEntities();

        public CRMService(ICRMRepository baseRepository, ILogRepository logRepository, ITipoCRMRepository tipoRepository, ICRMAnexoRepository anexoRepository, IUsuarioRepository usuRepository) : base(baseRepository)
        {
            _baseRepository = baseRepository;
            _logRepository = logRepository;
            _tipoRepository = tipoRepository;
            _anexoRepository = anexoRepository;
            _usuRepository = usuRepository;
        }

        public CRM CheckExist(CRM tarefa, Int32 idUsu,  Int32 idAss)
        {
            CRM item = _baseRepository.CheckExist(tarefa, idUsu, idAss);
            return item;
        }

        public CRM GetItemById(Int32 id)
        {
            CRM item = _baseRepository.GetItemById(id);
            return item;
        }

        public List<CRM> GetByDate(DateTime data, Int32 idAss)
        {
            return _baseRepository.GetByDate(data, idAss);
        }

        public USUARIO GetUserById(Int32 id)
        {
            USUARIO item = _usuRepository.GetItemById(id);
            return item;
        }

        public List<CRM> GetByUser(Int32 user)
        {
            return _baseRepository.GetByUser(user);
        }

        public List<CRM> GetTarefaStatus(Int32 tipo, Int32 idAss)
        {
            return _baseRepository.GetTarefaStatus(tipo, idAss);
        }

        public List<CRM> GetAllItens(Int32 idAss)
        {
            return _baseRepository.GetAllItens(idAss);
        }

        public List<CRM> GetAllItensAdm(Int32 idAss)
        {
            return _baseRepository.GetAllItensAdm(idAss);
        }

        public List<TIPO_CRM> GetAllTipos()
        {
            return _tipoRepository.GetAllItens();
        }

        public List<USUARIO> GetAllUsers(Int32 idAss)
        {
            return _usuRepository.GetAllItens(idAss);
        }

        public CRM_ANEXO GetAnexoById(Int32 id)
        {
            return _anexoRepository.GetItemById(id);
        }

        public List<CRM> ExecuteFilter(Int32? tipoId, String nome, String descricao, Int32? idCli, DateTime? data, Int32? status, Int32? usuario, Int32 idAss)
        {
            return _baseRepository.ExecuteFilter(tipoId, nome, descricao, idCli, data, status, usuario, idAss);

        }

        public Int32 Create(CRM item, LOG log)
        {
            using (DbContextTransaction transaction = Db.Database.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                try
                {
                    _logRepository.Add(log);
                    _baseRepository.Add(item);
                    transaction.Commit();
                    return 0;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw ex;
                }
            }
        }

        public Int32 Create(CRM item)
        {
            using (DbContextTransaction transaction = Db.Database.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                try
                {
                    _baseRepository.Add(item);
                    transaction.Commit();
                    return 0;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw ex;
                }
            }
        }


        public Int32 Edit(CRM item, LOG log)
        {
            using (DbContextTransaction transaction = Db.Database.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                try
                {
                    item.USUARIO = null;
                    CRM obj = _baseRepository.GetById(item.CRM1_CD_ID);
                    _baseRepository.Detach(obj);
                    _logRepository.Add(log);
                    _baseRepository.Update(item);
                    transaction.Commit();
                    return 0;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw ex;
                }
            }
        }

        public Int32 Edit(CRM item)
        {
            using (DbContextTransaction transaction = Db.Database.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                try
                {
                    CRM obj = _baseRepository.GetById(item.CRM1_CD_ID);
                    _baseRepository.Detach(obj);
                    _baseRepository.Update(item);
                    transaction.Commit();
                    return 0;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw ex;
                }
            }
        }

        public Int32 Delete(CRM item, LOG log)
        {
            using (DbContextTransaction transaction = Db.Database.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                try
                {
                    _logRepository.Add(log);
                    _baseRepository.Remove(item);
                    transaction.Commit();
                    return 0;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw ex;
                }
            }
        }


    }
}
