using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ApplicationServices.Interfaces;
using EntitiesServices.Model;
using System.Globalization;
using EntitiesServices.Work_Classes;
using AutoMapper;
using PlatMensagem_Solution.ViewModels;
using SMS_Presentation.App_Start;
using System.IO;
using System.Collections;
using System.Web.UI.WebControls;
using System.Runtime.Caching;

namespace SMS_Presentation.Controllers
{
    public class BaseAdminController : Controller
    {
        private readonly IUsuarioAppService baseApp;
        private readonly ILogAppService logApp;
        private readonly INotificacaoAppService notfApp;
        private readonly IUsuarioAppService usuApp;
        private readonly IConfiguracaoAppService confApp;
        private readonly IAssinanteAppService assApp;
        private readonly IClienteAppService cliApp;
        private readonly IMensagemAppService menApp;
        private readonly IEMailAgendaAppService emApp;

        private String msg;
        private Exception exception;
        USUARIO objeto = new USUARIO();
        USUARIO objetoAntes = new USUARIO();
        List<USUARIO> listaMaster = new List<USUARIO>();

        public BaseAdminController(IUsuarioAppService baseApps, ILogAppService logApps, INotificacaoAppService notfApps, IUsuarioAppService usuApps, IConfiguracaoAppService confApps, IAssinanteAppService assApps, IClienteAppService cliApps, IMensagemAppService menApps, IEMailAgendaAppService emApps)
        {
            baseApp = baseApps;
            logApp = logApps;
            notfApp = notfApps;
            usuApp = usuApps;
            confApp = confApps;
            assApp = assApps;
            cliApp = cliApps;
            menApp = menApps;
            emApp = emApps;
        }

        public ActionResult CarregarLandingPage()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            return View();
        }

        public JsonResult GetConfigNotificacoes()
        {
            Int32? idAss = (Int32)Session["IdAssinante"];
            bool hasNotf;
            var hash = new Hashtable();
            USUARIO usu = (USUARIO)Session["Usuario"];
            CONFIGURACAO conf = confApp.GetById(1);

            if (baseApp.GetAllItensUser(usu.USUA_CD_ID, idAss.Value).Count > 0)
            {
                hasNotf = true;
            }
            else
            {
                hasNotf = false;
            }

            hash.Add("CONF_NM_ARQUIVO_ALARME", conf.CONF_NM_ARQUIVO_ALARME);
            hash.Add("NOTIFICACAO", hasNotf);
            return Json(hash);
        }

        public ActionResult CarregarDesenvolvimento()
        {
            return View();
        }

       public ActionResult CarregarBase()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }

            // Carrega listas
            Int32 idAss = (Int32)Session["IdAssinante"];
            if ((Int32)Session["Login"] == 1)
            {
                Session["Perfis"] = baseApp.GetAllPerfis();
                Session["TiposPessoas"] = assApp.GetAllTiposPessoa();
                Session["UFs"] = assApp.GetAllUF();
            }

            Session["MensNotificacao"] = 0;
            Session["MensUsuario"] = 0;
            Session["MensLog"] = 0;
            Session["MensUsuarioAdm"] = 0;
            Session["MensTemplate"] = 0;
            Session["MensConfiguracao"] = 0;
            Session["MensCliente"] = 0;
            Session["MensMensagem"] = 0;
            Session["MensGrupo"] = 0;
            Session["MensCRM"] = 0;

            Session["VoltaNotificacao"] = 3;
            Session["VoltaCRM"] = 0;

            // Configuracao
            CONFIGURACAO conf = confApp.GetItemById(idAss);

            USUARIO usu = new USUARIO();
            UsuarioViewModel vm = new UsuarioViewModel();
            List<NOTIFICACAO> noti = new List<NOTIFICACAO>();

            ObjectCache cache = MemoryCache.Default;
            USUARIO usuContent = cache["usuario" + ((USUARIO)Session["UserCredentials"]).USUA_CD_ID] as USUARIO;

            if (usuContent == null)
            {
                usu = usuApp.GetItemById(((USUARIO)Session["UserCredentials"]).USUA_CD_ID);
                vm = Mapper.Map<USUARIO, UsuarioViewModel>(usu);
                noti = notfApp.GetAllItens(idAss);
                DateTime expiration = DateTime.Now.AddDays(15);
                cache.Set("usuario" + ((USUARIO)Session["UserCredentials"]).USUA_CD_ID, usu, expiration);
                cache.Set("vm" + ((USUARIO)Session["UserCredentials"]).USUA_CD_ID, vm, expiration);
                cache.Set("noti" + ((USUARIO)Session["UserCredentials"]).USUA_CD_ID, noti, expiration);
            }

            usu = cache.Get("usuario" + ((USUARIO)Session["UserCredentials"]).USUA_CD_ID) as USUARIO;
            vm = cache.Get("vm" + ((USUARIO)Session["UserCredentials"]).USUA_CD_ID) as UsuarioViewModel;
            noti = cache.Get("noti" + ((USUARIO)Session["UserCredentials"]).USUA_CD_ID) as List<NOTIFICACAO>;

            noti = notfApp.GetAllItensUser(usu.USUA_CD_ID, usu.ASSI_CD_ID);
            Session["Notificacoes"] = noti; 
            Session["ListaNovas"] = noti.Where(p => p.NOTI_IN_VISTA == 0).ToList();
            Session["NovasNotificacoes"] = noti.Where(p => p.NOTI_IN_VISTA == 0).Count();
            Session["Nome"] = usu.USUA_NM_NOME;

            Session["Logs"] = usu.LOG.Count;

            //Int32 numCli = cliApp.GetAllItens(idAss).Where(p => p.CLIE_IN_ATIVO == 1 & p.ASSI_CD_ID == idAss).ToList().Count;
            //ViewBag.NumClientes = numCli;

            List<MENSAGENS> lt = menApp.GetAllItens(idAss);
            List<MENSAGENS> lm = lt.Where(p => p.MENS_DT_ENVIO != null).ToList();
            ViewBag.SMS = lm.Where(p => p.MENS_IN_TIPO == 2).ToList().Count;
            ViewBag.Emails = lm.Where(p => p.MENS_IN_TIPO == 1).ToList().Count;
            ViewBag.Total = lm.Count;
            ViewBag.Falhas = lt.Where(p => p.MENS_TX_RETORNO != null).ToList().Count;

            List<MENSAGENS> agSMS = lm.Where(p => p.MENS_DT_AGENDAMENTO > DateTime.Now).ToList();
            Session["SMSAgenda"] = agSMS;
            ViewBag.SMSAgenda = agSMS.Count;

            List<EMAIL_AGENDAMENTO> emAg = emApp.GetAllItens(idAss).Where(p => p.EMAG_IN_ENVIADO == 0 & p.EMAG_DT_AGENDAMENTO > DateTime.Now).ToList();
            Session["EMailAgenda"] = emAg;
            ViewBag.SMSEmail = emAg.Count;

            String frase = String.Empty;
            String nome = usu.USUA_NM_NOME.Substring(0, usu.USUA_NM_NOME.IndexOf(" "));
            Session["NomeGreeting"] = nome;
            if (DateTime.Now.Hour <= 12)
            {
                frase = "Bom dia, " + nome;
            }
            else if (DateTime.Now.Hour > 12 & DateTime.Now.Hour <= 18)
            {
                frase = "Boa tarde, " + nome;
            }
            else
            {
                frase = "Boa noite, " + nome;
            }
            Session["Greeting"] = frase;
            Session["Foto"] = usu.USUA_AQ_FOTO;

            // Mensagens
            if ((Int32)Session["MensNotificacao"] == 2)
            {
                ModelState.AddModelError("", PlatMensagens_Resources.ResourceManager.GetString("M0011", CultureInfo.CurrentCulture));
            }
            if ((Int32)Session["MensUsuario"] == 2)
            {
                ModelState.AddModelError("", PlatMensagens_Resources.ResourceManager.GetString("M0011", CultureInfo.CurrentCulture));
            }
            if ((Int32)Session["MensLog"] == 2)
            {
                ModelState.AddModelError("", PlatMensagens_Resources.ResourceManager.GetString("M0011", CultureInfo.CurrentCulture));
            }
            if ((Int32)Session["MensUsuarioAdm"] == 2)
            {
                ModelState.AddModelError("", PlatMensagens_Resources.ResourceManager.GetString("M0011", CultureInfo.CurrentCulture));
            }
            if ((Int32)Session["MensTemplate"] == 2)
            {
                ModelState.AddModelError("", PlatMensagens_Resources.ResourceManager.GetString("M0011", CultureInfo.CurrentCulture));
            }
            if ((Int32)Session["MensConfiguracao"] == 2)
            {
                ModelState.AddModelError("", PlatMensagens_Resources.ResourceManager.GetString("M0011", CultureInfo.CurrentCulture));
            }
            if ((Int32)Session["MensCliente"] == 2)
            {
                ModelState.AddModelError("", PlatMensagens_Resources.ResourceManager.GetString("M0011", CultureInfo.CurrentCulture));
            }
            if ((Int32)Session["MensMensagem"] == 2)
            {
                ModelState.AddModelError("", PlatMensagens_Resources.ResourceManager.GetString("M0011", CultureInfo.CurrentCulture));
            }
            if ((Int32)Session["MensGrupo"] == 2)
            {
                ModelState.AddModelError("", PlatMensagens_Resources.ResourceManager.GetString("M0011", CultureInfo.CurrentCulture));
            }
            if ((Int32)Session["MensCRM"] == 2)
            {
                ModelState.AddModelError("", PlatMensagens_Resources.ResourceManager.GetString("M0011", CultureInfo.CurrentCulture));
            }
            return View(vm);
        }

        public ActionResult VerEMailExpansao()
        {
            // Prepara grid
            Int32 idAss = (Int32)Session["IdAssinante"];
            List<MENSAGENS> listaBase1 = menApp.GetAllItens(idAss).Where(p => p.MENS_IN_TIPO == 1 & p.MENS_DT_ENVIO != null).ToList();
            List<MENSAGENS> listaBase = listaBase1.Where(p => p.MENS_DT_ENVIO.Value.Month == DateTime.Today.Month).ToList();
            List<DateTime> datas = listaBase.Select(p => p.MENS_DT_ENVIO.Value.Date).Distinct().ToList();
            List<ModeloViewModel> lista = new List<ModeloViewModel>();
            foreach (DateTime item in datas)
            {
                Int32 conta = listaBase.Where(p => p.MENS_DT_ENVIO.Value.Date == item).Count();
                ModeloViewModel mod = new ModeloViewModel();
                mod.DataEmissao = item;
                mod.Valor = conta;
                lista.Add(mod);
            }     
            ViewBag.Lista = lista;
            ViewBag.Conta = listaBase.Count;
            Session["ListaEMail"] = listaBase;
            Session["ListaDatas"] = datas;
            return View();
        }

        public JsonResult GetDadosGraficoEmail()
        {
            List<MENSAGENS> listaCP1 = (List<MENSAGENS>)Session["ListaEMail"];
            List<DateTime> datas = (List<DateTime>)Session["ListaDatas"];
            List<MENSAGENS> listaDia = new List<MENSAGENS>();
            List<String> dias = new List<String>();
            List<Int32> valor = new List<Int32>();
            dias.Add(" ");
            valor.Add(0);

            foreach (DateTime item in datas)
            {
                listaDia = listaCP1.Where(p => p.MENS_DT_ENVIO.Value.Date == item).ToList();
                Int32 contaDia = listaDia.Count();
                dias.Add(item.ToShortDateString());
                valor.Add(contaDia);
            }

            Hashtable result = new Hashtable();
            result.Add("dias", dias);
            result.Add("valores", valor);
            return Json(result);
        }

        public JsonResult GetDadosGraficoEmailTodos()
        {
            List<MENSAGENS> listaCP1 = (List<MENSAGENS>)Session["ListaEMailTodas"];
            List<DateTime> datas = (List<DateTime>)Session["ListaDatasTodas"];
            List<MENSAGENS> listaDia = new List<MENSAGENS>();
            List<String> dias = new List<String>();
            List<Int32> valor = new List<Int32>();
            dias.Add(" ");
            valor.Add(0);

            foreach (DateTime item in datas)
            {
                listaDia = listaCP1.Where(p => p.MENS_DT_ENVIO.Value.Date == item).ToList();
                Int32 contaDia = listaDia.Count();
                dias.Add(item.ToShortDateString());
                valor.Add(contaDia);
            }

            Hashtable result = new Hashtable();
            result.Add("dias", dias);
            result.Add("valores", valor);
            return Json(result);
        }

        public ActionResult VerEMailExpansaoTodos()
        {
            // Prepara grid
            Int32 idAss = (Int32)Session["IdAssinante"];
            List<MENSAGENS> listaBase = menApp.GetAllItens(idAss).Where(p => p.MENS_IN_TIPO == 1 & p.MENS_DT_ENVIO != null).ToList();
            List<DateTime> datas = listaBase.Select(p => p.MENS_DT_ENVIO.Value.Date).Distinct().ToList();
            List<ModeloViewModel> lista = new List<ModeloViewModel>();
            foreach (DateTime item in datas)
            {
                Int32 conta = listaBase.Where(p => p.MENS_DT_ENVIO.Value.Date == item).Count();
                ModeloViewModel mod = new ModeloViewModel();
                mod.DataEmissao = item;
                mod.Valor = conta;
                lista.Add(mod);
            }
            ViewBag.Lista = lista;
            ViewBag.Conta = listaBase.Count;
            Session["ListaEMailTodas"] = listaBase;
            Session["ListaDatasTodas"] = datas;
            return View();
        }

        public ActionResult VerSMSExpansao()
        {
            // Prepara grid
            Int32 idAss = (Int32)Session["IdAssinante"];
            List<MENSAGENS> listaBase1 = menApp.GetAllItens(idAss).Where(p => p.MENS_IN_TIPO == 2 & p.MENS_DT_ENVIO != null).ToList();
            List<MENSAGENS> listaBase = listaBase1.Where(p => p.MENS_DT_ENVIO.Value.Month == DateTime.Today.Month).ToList();
            List<DateTime> datas = listaBase.Select(p => p.MENS_DT_ENVIO.Value.Date).Distinct().ToList();
            List<ModeloViewModel> lista = new List<ModeloViewModel>();
            foreach (DateTime item in datas)
            {
                Int32 conta = listaBase.Where(p => p.MENS_DT_ENVIO.Value.Date == item).Count();
                ModeloViewModel mod = new ModeloViewModel();
                mod.DataEmissao = item;
                mod.Valor = conta;
                lista.Add(mod);
            }
            ViewBag.Lista = lista;
            ViewBag.Conta = listaBase.Count;
            Session["ListaSMS"] = listaBase;
            Session["ListaDatasSMS"] = datas;
            return View();
        }

        public JsonResult GetDadosGraficoSMS()
        {
            List<MENSAGENS> listaCP1 = (List<MENSAGENS>)Session["ListaSMS"];
            List<DateTime> datas = (List<DateTime>)Session["ListaDatasSMS"];
            List<MENSAGENS> listaDia = new List<MENSAGENS>();
            List<String> dias = new List<String>();
            List<Int32> valor = new List<Int32>();
            dias.Add(" ");
            valor.Add(0);

            foreach (DateTime item in datas)
            {
                listaDia = listaCP1.Where(p => p.MENS_DT_ENVIO.Value.Date == item).ToList();
                Int32 contaDia = listaDia.Count();
                dias.Add(item.ToShortDateString());
                valor.Add(contaDia);
            }

            Hashtable result = new Hashtable();
            result.Add("dias", dias);
            result.Add("valores", valor);
            return Json(result);
        }

        public JsonResult GetDadosGraficoSMSTodos()
        {
            List<MENSAGENS> listaCP1 = (List<MENSAGENS>)Session["ListaSMSTodas"];
            List<DateTime> datas = (List<DateTime>)Session["ListaDatasSMSTodas"];
            List<MENSAGENS> listaDia = new List<MENSAGENS>();
            List<String> dias = new List<String>();
            List<Int32> valor = new List<Int32>();
            dias.Add(" ");
            valor.Add(0);

            foreach (DateTime item in datas)
            {
                listaDia = listaCP1.Where(p => p.MENS_DT_ENVIO.Value.Date == item).ToList();
                Int32 contaDia = listaDia.Count();
                dias.Add(item.ToShortDateString());
                valor.Add(contaDia);
            }

            Hashtable result = new Hashtable();
            result.Add("dias", dias);
            result.Add("valores", valor);
            return Json(result);
        }

        public ActionResult VerSMSExpansaoTodos()
        {
            // Prepara grid
            Int32 idAss = (Int32)Session["IdAssinante"];
            List<MENSAGENS> listaBase = menApp.GetAllItens(idAss).Where(p => p.MENS_IN_TIPO == 2 & p.MENS_DT_ENVIO != null).ToList();
            List<DateTime> datas = listaBase.Select(p => p.MENS_DT_ENVIO.Value.Date).Distinct().ToList();
            List<ModeloViewModel> lista = new List<ModeloViewModel>();
            foreach (DateTime item in datas)
            {
                Int32 conta = listaBase.Where(p => p.MENS_DT_ENVIO.Value.Date == item).Count();
                ModeloViewModel mod = new ModeloViewModel();
                mod.DataEmissao = item;
                mod.Valor = conta;
                lista.Add(mod);
            }
            ViewBag.Lista = lista;
            ViewBag.Conta = listaBase.Count;
            Session["ListaSMSTodas"] = listaBase;
            Session["ListaDatasSMSTodas"] = datas;
            return View();
        }

        public ActionResult VerTotalExpansao()
        {
            // Prepara grid
            Int32 idAss = (Int32)Session["IdAssinante"];
            List<MENSAGENS> listaBase1 = menApp.GetAllItens(idAss).Where(p => p.MENS_DT_ENVIO != null).ToList();
            List<MENSAGENS> listaBase = listaBase1.Where(p => p.MENS_DT_ENVIO.Value.Month == DateTime.Today.Month).ToList();
            List<DateTime> datas = listaBase.Select(p => p.MENS_DT_ENVIO.Value.Date).Distinct().ToList();
            List<ModeloViewModel> lista = new List<ModeloViewModel>();
            foreach (DateTime item in datas)
            {
                Int32 conta = listaBase.Where(p => p.MENS_DT_ENVIO.Value.Date == item).Count();
                ModeloViewModel mod = new ModeloViewModel();
                mod.DataEmissao = item;
                mod.Valor = conta;
                lista.Add(mod);
            }
            ViewBag.Lista = lista;
            ViewBag.Conta = listaBase.Count;
            Session["ListaTotal"] = listaBase;
            Session["ListaDatasTotal"] = datas;
            return View();
        }

        public JsonResult GetDadosGraficoTotal()
        {
            List<MENSAGENS> listaCP1 = (List<MENSAGENS>)Session["ListaTotalTodas"];
            List<DateTime> datas = (List<DateTime>)Session["ListaDatasTotalTodas"];
            List<MENSAGENS> listaDia = new List<MENSAGENS>();
            List<String> dias = new List<String>();
            List<Int32> valor = new List<Int32>();
            dias.Add(" ");
            valor.Add(0);

            foreach (DateTime item in datas)
            {
                listaDia = listaCP1.Where(p => p.MENS_DT_ENVIO.Value.Date == item).ToList();
                Int32 contaDia = listaDia.Count();
                dias.Add(item.ToShortDateString());
                valor.Add(contaDia);
            }

            Hashtable result = new Hashtable();
            result.Add("dias", dias);
            result.Add("valores", valor);
            return Json(result);
        }

        public JsonResult GetDadosGraficoTotalTodos()
        {
            List<MENSAGENS> listaCP1 = (List<MENSAGENS>)Session["ListaTotal"];
            List<DateTime> datas = (List<DateTime>)Session["ListaDatasTotal"];
            List<MENSAGENS> listaDia = new List<MENSAGENS>();
            List<String> dias = new List<String>();
            List<Int32> valor = new List<Int32>();
            dias.Add(" ");
            valor.Add(0);

            foreach (DateTime item in datas)
            {
                listaDia = listaCP1.Where(p => p.MENS_DT_ENVIO.Value.Date == item).ToList();
                Int32 contaDia = listaDia.Count();
                dias.Add(item.ToShortDateString());
                valor.Add(contaDia);
            }

            Hashtable result = new Hashtable();
            result.Add("dias", dias);
            result.Add("valores", valor);
            return Json(result);
        }

        public ActionResult VerTotalExpansaoTodos()
        {
            // Prepara grid
            Int32 idAss = (Int32)Session["IdAssinante"];
            List<MENSAGENS> listaBase = menApp.GetAllItens(idAss).Where(p => p.MENS_DT_ENVIO != null).ToList();
            List<DateTime> datas = listaBase.Select(p => p.MENS_DT_ENVIO.Value.Date).Distinct().ToList();
            List<ModeloViewModel> lista = new List<ModeloViewModel>();
            foreach (DateTime item in datas)
            {
                Int32 conta = listaBase.Where(p => p.MENS_DT_ENVIO.Value.Date == item).Count();
                ModeloViewModel mod = new ModeloViewModel();
                mod.DataEmissao = item;
                mod.Valor = conta;
                lista.Add(mod);
            }
            ViewBag.Lista = lista;
            ViewBag.Conta = listaBase.Count;
            Session["ListaTotalTodas"] = listaBase;
            Session["ListaDatasTotalTodas"] = datas;
            return View();
        }

        public ActionResult VerFalhasExpansao()
        {
            // Prepara grid
            Int32 idAss = (Int32)Session["IdAssinante"];
            List<MENSAGENS> listaBase1 = menApp.GetAllItens(idAss).Where(p => p.MENS_TX_RETORNO != null).ToList();
            List<MENSAGENS> listaBase = listaBase1.Where(p => p.MENS_DT_CRIACAO.Value.Month == DateTime.Today.Month).ToList();
            List<DateTime> datas = listaBase.Select(p => p.MENS_DT_CRIACAO.Value.Date).Distinct().ToList();
            List<ModeloViewModel> lista = new List<ModeloViewModel>();
            foreach (DateTime item in datas)
            {
                Int32 conta = listaBase.Where(p => p.MENS_DT_CRIACAO.Value.Date == item).Count();
                ModeloViewModel mod = new ModeloViewModel();
                mod.DataEmissao = item;
                mod.Valor = conta;
                lista.Add(mod);
            }
            ViewBag.Lista = lista;
            ViewBag.Conta = listaBase.Count;
            Session["ListaFalha"] = listaBase;
            Session["ListaDatasFalha"] = datas;
            return View();
        }

        public JsonResult GetDadosGraficoFalhas()
        {
            List<MENSAGENS> listaCP1 = (List<MENSAGENS>)Session["ListaFalha"];
            List<DateTime> datas = (List<DateTime>)Session["ListaDatasFalha"];
            List<MENSAGENS> listaDia = new List<MENSAGENS>();
            List<String> dias = new List<String>();
            List<Int32> valor = new List<Int32>();
            dias.Add(" ");
            valor.Add(0);

            foreach (DateTime item in datas)
            {
                listaDia = listaCP1.Where(p => p.MENS_DT_CRIACAO.Value.Date == item).ToList();
                Int32 contaDia = listaDia.Count();
                dias.Add(item.ToShortDateString());
                valor.Add(contaDia);
            }

            Hashtable result = new Hashtable();
            result.Add("dias", dias);
            result.Add("valores", valor);
            return Json(result);
        }

        public JsonResult GetDadosGraficoFalhasTodos()
        {
            List<MENSAGENS> listaCP1 = (List<MENSAGENS>)Session["ListaFalhaTodas"];
            List<DateTime> datas = (List<DateTime>)Session["ListaDatasFalhaTodas"];
            List<MENSAGENS> listaDia = new List<MENSAGENS>();
            List<String> dias = new List<String>();
            List<Int32> valor = new List<Int32>();
            dias.Add(" ");
            valor.Add(0);

            foreach (DateTime item in datas)
            {
                listaDia = listaCP1.Where(p => p.MENS_DT_CRIACAO.Value.Date == item).ToList();
                Int32 contaDia = listaDia.Count();
                dias.Add(item.ToShortDateString());
                valor.Add(contaDia);
            }

            Hashtable result = new Hashtable();
            result.Add("dias", dias);
            result.Add("valores", valor);
            return Json(result);
        }

        public ActionResult VerFalhasExpansaoTodos()
        {
            // Prepara grid
            Int32 idAss = (Int32)Session["IdAssinante"];
            List<MENSAGENS> listaBase = menApp.GetAllItens(idAss).Where(p => p.MENS_TX_RETORNO != null).ToList();
            List<DateTime> datas = listaBase.Select(p => p.MENS_DT_CRIACAO.Value.Date).Distinct().ToList();
            List<ModeloViewModel> lista = new List<ModeloViewModel>();
            foreach (DateTime item in datas)
            {
                Int32 conta = listaBase.Where(p => p.MENS_DT_CRIACAO.Value.Date == item).Count();
                ModeloViewModel mod = new ModeloViewModel();
                mod.DataEmissao = item;
                mod.Valor = conta;
                lista.Add(mod);
            }
            ViewBag.Lista = lista;
            ViewBag.Conta = listaBase.Count;
            Session["ListaFalha"] = listaBase;
            Session["ListaDatasFalha"] = datas;
            return View();
        }



    }
}