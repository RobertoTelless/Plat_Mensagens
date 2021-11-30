using System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;
using EntitiesServices.Work_Classes;
using ApplicationServices.Interfaces;
using ModelServices.Interfaces.EntitiesServices;
using CrossCutting;
using System.Text.RegularExpressions;

namespace ApplicationServices.Services
{
    public class CRMAppService : AppServiceBase<CRM>, ICRMAppService
    {
        private readonly ICRMService _baseService;
        private readonly INotificacaoService _notiService;

        public CRMAppService(ICRMService baseService, INotificacaoService notiService): base(baseService)
        {
            _baseService = baseService;
            _notiService = notiService;
        }

        public List<CRM> GetAllItens(Int32 idAss)
        {
            List<CRM> lista = _baseService.GetAllItens(idAss);
            return lista;
        }

        public List<CRM> GetAllItensAdm(Int32 idAss)
        {
            List<CRM> lista = _baseService.GetAllItensAdm(idAss);
            return lista;
        }

        public List<CRM> GetTarefaStatus(Int32 tipo, Int32 idAss)
        {
            List<CRM> lista = _baseService.GetTarefaStatus(tipo, idAss);
            return lista;
        }

        public List<CRM> GetByDate(DateTime data, Int32 idAss)
        {
            List<CRM> lista = _baseService.GetByDate(data, idAss);
            return lista;
        }

        public List<CRM> GetByUser(Int32 user)
        {
            List<CRM> lista = _baseService.GetByUser(user);
            return lista;
        }

        public CRM GetItemById(Int32 id)
        {
            CRM item = _baseService.GetItemById(id);
            return item;
        }

        public USUARIO GetUserById(Int32 id)
        {
            USUARIO item = _baseService.GetUserById(id);
            return item;
        }

        public CRM CheckExist(CRM tarefa, Int32 idUsu, Int32 idAss)
        {
            CRM item = _baseService.CheckExist(tarefa, idUsu, idAss);
            return item;
        }

        public List<TIPO_CRM> GetAllTipos()
        {
            List<TIPO_CRM> lista = _baseService.GetAllTipos();
            return lista;
        }

        public List<TIPO_ACAO> GetAllTipoAcao()
        {
            List<TIPO_ACAO> lista = _baseService.GetAllTipoAcao();
            return lista;
        }

        public List<MOTIVO_CANCELAMENTO> GetAllMotivoCancelamento()
        {
            List<MOTIVO_CANCELAMENTO> lista = _baseService.GetAllMotivoCancelamento();
            return lista;
        }

        public List<MOTIVO_ENCERRAMENTO> GetAllMotivoEncerramento()
        {
            List<MOTIVO_ENCERRAMENTO> lista = _baseService.GetAllMotivoEncerramento();
            return lista;
        }

        public List<CRM_ORIGEM> GetAllOrigens()
        {
            List<CRM_ORIGEM> lista = _baseService.GetAllOrigens();
            return lista;
        }

        public CRM_ANEXO GetAnexoById(Int32 id)
        {
            CRM_ANEXO lista = _baseService.GetAnexoById(id);
            return lista;
        }

        public CRM_COMENTARIO GetComentarioById(Int32 id)
        {
            CRM_COMENTARIO lista = _baseService.GetComentarioById(id);
            return lista;
        }

        public Int32 ExecuteFilter(Int32? status, DateTime? inicio, DateTime? final, Int32? origem, Int32? adic, String nome, String busca,  Int32? estrela, Int32 idAss, out List<CRM> objeto)
        {
            try
            {
                objeto = new List<CRM>();
                Int32 volta = 0;

                // Processa filtro
                objeto = _baseService.ExecuteFilter(status, inicio, final, origem, adic, nome, busca, estrela, idAss);
                if (objeto.Count == 0)
                {
                    volta = 1;
                }
                return volta;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public Int32 ValidateCreate(CRM item, USUARIO usuario)
        {
            try
            {
                //Verifica Campos
                if (item.TIPO_CRM != null)
                {
                    item.TIPO_CRM = null;
                }
                if (item.USUARIO != null)
                {
                    item.USUARIO = null;
                }
                if (item.ASSINANTE != null)
                {
                    item.ASSINANTE = null;
                }

                // Verifica existencia prévia
                if (_baseService.CheckExist(item, usuario.USUA_CD_ID, usuario.ASSI_CD_ID) != null)
                {
                    return 1;
                }

                // Completa objeto
                item.CRM1_IN_ATIVO = 1;
                item.USUA_CD_ID = usuario.USUA_CD_ID;
                item.CRM1_IN_STATUS = 1;

                // Monta Log
                LOG log = new LOG
                {
                    LOG_DT_DATA = DateTime.Now,
                    USUA_CD_ID = usuario.USUA_CD_ID,
                    ASSI_CD_ID = usuario.ASSI_CD_ID,
                    LOG_NM_OPERACAO = "AddCRM",
                    LOG_IN_ATIVO = 1,
                    LOG_TX_REGISTRO = Serialization.SerializeJSON<CRM>(item)
                };
                
                // Persiste
                Int32 volta = _baseService.Create(item, log);
                return volta;
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        public Int32 ValidateEdit(CRM item, CRM itemAntes, USUARIO usuario)
        {
            try
            {
                // Verificação
                if (item.CRM1_DT_ENCERRAMENTO < item.CRM1_DT_CRIACAO)
                {
                    return 1;
                }
                if (item.CRM1_DT_ENCERRAMENTO > DateTime.Today.Date)
                {
                    return 2;
                }
                if (item.CRM1_DT_CANCELAMENTO < item.CRM1_DT_CRIACAO)
                {
                    return 3;
                }
                if (item.CRM1_DT_CANCELAMENTO > DateTime.Today.Date)
                {
                    return 4;
                }

                // Monta Log
                LOG log = new LOG
                {
                    LOG_DT_DATA = DateTime.Now,
                    USUA_CD_ID = usuario.USUA_CD_ID,
                    ASSI_CD_ID = usuario.ASSI_CD_ID,
                    LOG_NM_OPERACAO = "EditCRM",
                    LOG_IN_ATIVO = 1,
                    LOG_TX_REGISTRO = Serialization.SerializeJSON<CRM>(item),
                    LOG_TX_REGISTRO_ANTES = Serialization.SerializeJSON<CRM>(itemAntes)
                };

                // Persiste
                Int32 volta = _baseService.Edit(item, log);
                return volta;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public Int32 ValidateEdit(CRM item, CRM itemAntes)
        {
            try
            {
                if (item.CRM1_DT_ENCERRAMENTO < item.CRM1_DT_CRIACAO)
                {
                    return 1;
                }
                if (item.CRM1_DT_ENCERRAMENTO > DateTime.Today.Date)
                {
                    return 2;
                }
                if (item.CRM1_DT_CANCELAMENTO < item.CRM1_DT_CRIACAO)
                {
                    return 3;
                }
                if (item.CRM1_DT_CANCELAMENTO > DateTime.Today.Date)
                {
                    return 4;
                }

                // Persiste
                Int32 volta = _baseService.Edit(item);

                return volta;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public Int32 ValidateDelete(CRM item, USUARIO usuario)
        {
            try
            {
                // Verifica integridade referencial
                // Acerta campos
                item.CRM1_IN_ATIVO = 0;

                //Evita erros de serialização
                if (item.CRM_COMENTARIO != null)
                {
                    item.CRM_COMENTARIO = null;
                }
                if (item.CRM_ANEXO != null)
                {
                    item.CRM_ANEXO = null;
                }
                if (item.TIPO_CRM != null)
                {
                    item.TIPO_CRM = null;
                }
                if (item.USUARIO != null)
                {
                    item.USUARIO = null;
                }

                // Verifica integridade
                List<CRM_ACAO> acao = item.CRM_ACAO.Where(p => p.CRAC_DT_PREVISTA.Value > DateTime.Today.Date).ToList();
                if (acao.Count > 0)
                {
                    return 1;
                }

                // Monta Log
                LOG log = new LOG
                {
                    LOG_DT_DATA = DateTime.Now,
                    USUA_CD_ID = usuario.USUA_CD_ID,
                    ASSI_CD_ID = usuario.ASSI_CD_ID,
                    LOG_IN_ATIVO = 1,
                    LOG_NM_OPERACAO = "DelCRM",
                    LOG_TX_REGISTRO = Serialization.SerializeJSON<CRM>(item)
                };

                // Persiste
                Int32 volta =  _baseService.Edit(item, log);
                return volta;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public Int32 ValidateReativar(CRM item, USUARIO usuario)
        {
            try
            {
                // Verifica integridade referencial

                // Acerta campos
                item.CRM1_IN_ATIVO = 1;

                //Evita erros de serialização
                if (item.CRM_COMENTARIO != null)
                {
                    item.CRM_COMENTARIO = null;
                }
                if (item.CRM_ANEXO != null)
                {
                    item.CRM_ANEXO = null;
                }
                if (item.TIPO_CRM != null)
                {
                    item.TIPO_CRM = null;
                }
                if (item.USUARIO != null)
                {
                    item.USUARIO = null;
                }

                // Monta Log
                LOG log = new LOG
                {
                    LOG_DT_DATA = DateTime.Now,
                    USUA_CD_ID = usuario.USUA_CD_ID,
                    ASSI_CD_ID = usuario.ASSI_CD_ID,
                    LOG_IN_ATIVO = 1,
                    LOG_NM_OPERACAO = "ReatCRM",
                    LOG_TX_REGISTRO = Serialization.SerializeJSON<CRM>(item)
                };

                // Persiste
                Int32 volta =  _baseService.Edit(item, log);
                return volta;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public Int32 ValidateEditContato(CRM_CONTATO item)
        {
            try
            {
                // Persiste
                return _baseService.EditContato(item);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public Int32 ValidateCreateContato(CRM_CONTATO item)
        {
            try
            {
                item.CRCO_IN_ATIVO = 1;

                // Persiste
                Int32 volta = _baseService.CreateContato(item);
                return volta;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public Int32 ValidateEditAcao(CRM_ACAO item)
        {
            try
            {
                // Persiste
                return _baseService.EditAcao(item);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public Int32 ValidateCreateAcao(CRM_ACAO item)
        {
            try
            {
                item.CRAC_IN_ATIVO = 1;

                // Persiste
                Int32 volta = _baseService.CreateAcao(item);
                return volta;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

    }
}
