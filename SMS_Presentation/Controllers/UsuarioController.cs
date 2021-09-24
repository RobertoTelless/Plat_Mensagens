using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ApplicationServices.Interfaces;
using EntitiesServices.Model;
using System.Globalization;
using SMS_Presentation.App_Start;
using EntitiesServices.Work_Classes;
using AutoMapper;
using PlatMensagem_Solution.ViewModels;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Collections;
using System.Web.UI.WebControls;
using System.Runtime.Caching;
using Image = iTextSharp.text.Image;
using EntitiesServices.WorkClasses;

namespace SMS_Presentation.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly IUsuarioAppService baseApp;
        private readonly ILogAppService logApp;
        private readonly INotificacaoAppService notiApp;
        private readonly IConfiguracaoAppService confApp;

        private String msg;
        private Exception exception;
        USUARIO objeto = new USUARIO();
        USUARIO objetoAntes = new USUARIO();
        List<USUARIO> listaMaster = new List<USUARIO>();
        LOG objLog = new LOG();
        LOG objLogAntes = new LOG();
        List<LOG> listaMasterLog = new List<LOG>();
        String extensao;

        public UsuarioController(IUsuarioAppService baseApps, ILogAppService logApps, INotificacaoAppService notiApps, IConfiguracaoAppService confApps)
        {
            baseApp = baseApps;
            logApp = logApps;
            notiApp = notiApps;
            confApp = confApps;
        }

        [HttpGet]
        public ActionResult Index()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            return View();
        }

        [HttpGet]
        public ActionResult MontarTelaUsuario()
        {
            // Verifica se tem usuario logado
            USUARIO usuario = new USUARIO();
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            if ((USUARIO)Session["UserCredentials"] != null)
            {
                usuario = (USUARIO)Session["UserCredentials"];

                // Verfifica permissão
                if (usuario.PERFIL.PERF_SG_SIGLA == "OPR" || usuario.PERFIL.PERF_SG_SIGLA == "VIS")
                {
                    Session["MensUsuario"] = 2;
                    return RedirectToAction("CarregarBase", "BaseAdmin");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Carrega listas
            ViewBag.Perfis = new SelectList((List<PERFIL>)Session["Perfis"], "PERF_CD_ID", "PERF_NM_NOME");
            ViewBag.Cargos = new SelectList(baseApp.GetAllCargos(), "CARG_CD_ID", "CARG_NM_NOME");

            // Carrega listas
            if ((List<USUARIO>)Session["ListaUsuario"] == null)
            {
                listaMaster = baseApp.GetAllItens(idAss);
                Session["ListaUsuario"] = listaMaster;
                Session["FiltroUsuario"] = null;
            }
            List<USUARIO> listaUsu = (List<USUARIO>)Session["ListaUsuario"];
            ViewBag.Listas = listaUsu;
            ViewBag.Usuarios = listaUsu.Count;
            ViewBag.Perfil = usuario.PERFIL.PERF_SG_SIGLA;

            ViewBag.UsuariosBloqueados = listaUsu.Where(p => p.USUA_IN_BLOQUEADO == 1).ToList().Count;
            ViewBag.UsuariosHoje = listaUsu.Where(p => p.USUA_IN_BLOQUEADO == 0 && p.USUA_DT_ACESSO == DateTime.Today.Date).ToList().Count;
            ViewBag.Title = "Usuários";

            // Recupera numero de usuarios do assinante
            Session["NumUsuarios"] = listaUsu.Count;

            // Mensagens
            if (Session["MensUsuario"] != null)
            {
                if ((Int32)Session["MensUsuario"] == 2)
                {
                    ModelState.AddModelError("", PlatMensagens_Resources.ResourceManager.GetString("M0011", CultureInfo.CurrentCulture));
                }
                if ((Int32)Session["MensUsuario"] == 3)
                {
                    ModelState.AddModelError("", PlatMensagens_Resources.ResourceManager.GetString("M0009", CultureInfo.CurrentCulture));
                }
                if ((Int32)Session["MensUsuario"] == 4)
                {
                    ModelState.AddModelError("", PlatMensagens_Resources.ResourceManager.GetString("M0001", CultureInfo.CurrentCulture));
                }
                if ((Int32)Session["MensUsuario"] == 5)
                {
                    ModelState.AddModelError("", PlatMensagens_Resources.ResourceManager.GetString("M0110", CultureInfo.CurrentCulture));
                }
                if ((Int32)Session["MensUsuario"] == 6)
                {
                    ModelState.AddModelError("", PlatMensagens_Resources.ResourceManager.GetString("M0111", CultureInfo.CurrentCulture));
                }
                if ((Int32)Session["MensUsuario"] == 9)
                {
                    ModelState.AddModelError("", PlatMensagens_Resources.ResourceManager.GetString("M0045", CultureInfo.CurrentCulture));
                }
            }

            // Abre view
            objeto = new USUARIO();
            return View(objeto);
        }

        public ActionResult Voltar()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            return RedirectToAction("CarregarBase", "BaseAdmin");
        }

        public ActionResult VoltarBase()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            return RedirectToAction("MontarTelaUsuario");
        }

        [HttpPost]
        public ActionResult FiltrarUsuario(USUARIO item)
        {
            try
            {
                // Executa a operação
                if ((String)Session["Ativa"] == null)
                {
                    return RedirectToAction("Login", "ControleAcesso");
                }
                Int32 idAss = (Int32)Session["IdAssinante"];
                List<USUARIO> listaObj = new List<USUARIO>();
                Int32 volta = baseApp.ExecuteFilter(item.PERF_CD_ID, item.CARG_CD_ID, item.USUA_NM_NOME, item.USUA_NM_LOGIN, item.USUA_NM_EMAIL, idAss, out listaObj);
                Session["FiltroUsuario"] = item;

                // Verifica retorno
                if (volta == 1)
                {
                    Session["MensUsuario"] = 1;
                }

                // Sucesso
                Session["MensUsuario"] = 0;
                listaMaster = listaObj;
                Session["ListaUsuario"] = listaObj;
                return RedirectToAction("MontarTelaUsuario");
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return RedirectToAction("MontarTelaUsuario");
            }
        }

        public ActionResult RetirarFiltroUsuario()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            Session["ListaUsuario"] = null;
            return RedirectToAction("MontarTelaUsuario");
        }

        public ActionResult MostrarTudoUsuario()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            USUARIO usuario = (USUARIO)Session["UserCredentials"];
            listaMaster = baseApp.GetAllUsuariosAdm(idAss);
            Session["ListaUsuario"] = listaMaster;
            return RedirectToAction("MontarTelaUsuario");
        }

        [HttpGet]
        public ActionResult VerAnexoUsuario(Int32 id)
        {
            // Prepara view
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            USUARIO_ANEXO item = baseApp.GetAnexoById(id);
            return View(item);
        }

        public FileResult DownloadUsuario(Int32 id)
        {
            USUARIO_ANEXO item = baseApp.GetAnexoById(id);
            String arquivo = item.USAN_AQ_ARQUIVO;
            Int32 pos = arquivo.LastIndexOf("/") + 1;
            String nomeDownload = arquivo.Substring(pos);
            String contentType = string.Empty;
            if (arquivo.Contains(".pdf"))
            {
                contentType = "application/pdf";
            }
            else if (arquivo.Contains(".jpg"))
            {
                contentType = "image/jpg";
            }
            else if (arquivo.Contains(".png"))
            {
                contentType = "image/png";
            }
            return File(arquivo, contentType, nomeDownload);
        }

        public ActionResult VoltarAnexoUsuario()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idUsu = (Int32)Session["IdUsuario"];
            return RedirectToAction("EditarUsuario", new { id = idUsu });
        }

        [HttpGet]
        public ActionResult VerUsuario(Int32 id)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Session["IdUsuario"] = id;
            USUARIO item = baseApp.GetItemById(id);
            UsuarioViewModel vm = Mapper.Map<USUARIO, UsuarioViewModel>(item);
            return View(vm);
        }

        [HttpGet]
        public ActionResult IncluirUsuario()
        {
            USUARIO usuario = new USUARIO();
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            if ((USUARIO)Session["UserCredentials"] != null)
            {
                usuario = (USUARIO)Session["UserCredentials"];

                // Verfifica permissão
                if (usuario.PERFIL.PERF_SG_SIGLA == "OPR" || usuario.PERFIL.PERF_SG_SIGLA == "VIS")
                {
                    Session["MensUsuario"] = 2;
                    return RedirectToAction("MontarTelaUsuario", "Usuario");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Prepara listas
            ViewBag.Perfis = new SelectList((List<PERFIL>)Session["Perfis"], "PERF_CD_ID", "PERF_NM_NOME");
            ViewBag.Cargos = new SelectList(baseApp.GetAllCargos(), "CARG_CD_ID", "CARG_NM_NOME");

            // Prepara view
            USUARIO item = new USUARIO();
            UsuarioViewModel vm = Mapper.Map<USUARIO, UsuarioViewModel>(item);
            vm.USUA_DT_CADASTRO = DateTime.Today.Date;
            vm.USUA_IN_ATIVO = 1;
            vm.ASSI_CD_ID = usuario.ASSI_CD_ID;
            vm.USUA_IN_BLOQUEADO = 0;
            vm.USUA_IN_LOGADO = 0;
            vm.USUA_IN_LOGIN_PROVISORIO = 0;
            vm.USUA_IN_PROVISORIO = 0;
            vm.USUA_IN_SISTEMA = 1;
            vm.USUA_NR_ACESSOS = 0;
            vm.USUA_NR_FALHAS = 0;
            return View(vm);
        }

        [HttpPost]
        public ActionResult IncluirUsuario(UsuarioViewModel vm)
        {

            ViewBag.Perfis = new SelectList((List<PERFIL>)Session["Perfis"], "PERF_CD_ID", "PERF_NM_NOME");
            ViewBag.Cargos = new SelectList(baseApp.GetAllCargos(), "CARG_CD_ID", "CARG_NM_NOME");
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    USUARIO item = Mapper.Map<UsuarioViewModel, USUARIO>(vm);
                    USUARIO usuarioLogado = (USUARIO)Session["UserCredentials"];
                    Int32 volta = baseApp.ValidateCreate(item, usuarioLogado);

                    // Verifica retorno
                    if (volta == 1)
                    {
                        Session["MensUsuario"] = 3;
                        return RedirectToAction("MontarTelaUsuario");
                    }
                    if (volta == 2)
                    {
                        Session["MensUsuario"] = 4;
                        return RedirectToAction("MontarTelaUsuario");
                    }
                    if (volta == 3)
                    {
                        Session["MensUsuario"] = 5;
                        return RedirectToAction("MontarTelaUsuario");
                    }
                    if (volta == 4 )
                    {
                        Session["MensUsuario"] = 6;
                        return RedirectToAction("MontarTelaUsuario");
                    }

                    // Carrega foto e processa alteracao
                    item.USUA_AQ_FOTO = "~/Imagens/Base/icone_imagem.jpg";
                    volta = baseApp.ValidateEdit(item, usuarioLogado);

                    // Cria pastas
                    String caminho = "/Imagens/" + item.ASSI_CD_ID.ToString() + "/Usuarios/" + item.USUA_CD_ID.ToString() + "/Fotos/";
                    Directory.CreateDirectory(Server.MapPath(caminho));
                    caminho = "/Imagens/" + item.ASSI_CD_ID.ToString() + "/Usuarios/" + item.USUA_CD_ID.ToString() + "/Anexos/";
                    Directory.CreateDirectory(Server.MapPath(caminho));

                    // Sucesso
                    listaMaster = new List<USUARIO>();
                    Session["ListaUsuario"] = null;
                    Session["IdUsuario"] = item.USUA_CD_ID;

                    if (Session["FileQueueUsuario"] != null)
                    {
                        List<FileQueue> fq = (List<FileQueue>)Session["FileQueueUsuario"];

                        foreach (var file in fq)
                        {
                            if (file.Profile == null)
                            {
                                UploadFileQueueUsuario(file);
                            }
                            else
                            {
                                UploadFotoQueueUsuario(file);
                            }
                        }

                        Session["FileQueueUsuario"] = null;
                    }

                    return RedirectToAction("MontarTelaUsuario");
                }
                catch (Exception ex)
                {
                    ViewBag.Message = ex.Message;
                    return View(vm);
                }
            }
            else
            {
                return View(vm);
            }
        }

        [HttpGet]
        public ActionResult EditarUsuario(Int32 id)
        {
            // Verifica se tem usuario logado
            USUARIO usuario = new USUARIO();
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            if ((USUARIO)Session["UserCredentials"] != null)
            {
                usuario = (USUARIO)Session["UserCredentials"];

                // Verfifica permissão
                if (usuario.PERFIL.PERF_SG_SIGLA == "OPR" || usuario.PERFIL.PERF_SG_SIGLA == "VIS")
                {
                    Session["MensUsuario"] = 2;
                    return RedirectToAction("MontarTelaUsuario", "Usuario");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Prepara view
            ViewBag.Perfis = new SelectList((List<PERFIL>)Session["Perfis"], "PERF_CD_ID", "PERF_NM_NOME");
            ViewBag.Cargos = new SelectList(baseApp.GetAllCargos(), "CARG_CD_ID", "CARG_NM_NOME");
            ViewBag.UsuarioLogado = usuario;
            ViewBag.Perfil = usuario.PERFIL.PERF_SG_SIGLA;
            USUARIO item = baseApp.GetItemById(id);
            objetoAntes = item;
            Session["Usuario"] = item;
            Session["IdUsuario"] = id;
            UsuarioViewModel vm = Mapper.Map<USUARIO, UsuarioViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        public ActionResult EditarUsuario(UsuarioViewModel vm)
        {

            ViewBag.Perfis = new SelectList((List<PERFIL>)Session["Perfis"], "PERF_CD_ID", "PERF_NM_NOME");
            ViewBag.Cargos = new SelectList(baseApp.GetAllCargos(), "CARG_CD_ID", "CARG_NM_NOME");
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    USUARIO usuarioLogado = (USUARIO)Session["UserCredentials"];
                    USUARIO item = Mapper.Map<UsuarioViewModel, USUARIO>(vm);
                    Int32 volta = baseApp.ValidateEdit(item, objetoAntes, usuarioLogado);

                    // Verifica retorno
                    if (volta == 1)
                    {
                        Session["MensUsuario"] = 4;
                        return RedirectToAction("MontarTelaUsuario");
                    }
                    if (volta == 2)
                    {
                        Session["MensUsuario"] = 5;
                        return RedirectToAction("MontarTelaUsuario");
                    }

                    // Mensagens
                    if (Session["MensUsuario"] != null)
                    {
                        if ((Int32)Session["MensUsuario"] == 10)
                        {
                            ModelState.AddModelError("", PlatMensagens_Resources.ResourceManager.GetString("M0019", CultureInfo.CurrentCulture));
                        }
                        if ((Int32)Session["MensUsuario"] == 11)
                        {
                            ModelState.AddModelError("", PlatMensagens_Resources.ResourceManager.GetString("M0024", CultureInfo.CurrentCulture));
                        }
                    }

                    // Sucesso
                    listaMaster = new List<USUARIO>();
                    Session["ListaUsuario"] = null;
                    Session["MensUsuario"] = 0;
                    return RedirectToAction("MontarTelaUsuario");
                }
                catch (Exception ex)
                {
                    ViewBag.Message = ex.Message;
                    return View(vm);
                }
            }
            else
            {
                return View(vm);
            }
        }

       [HttpGet]
        public ActionResult BloquearUsuario(Int32 id)
        {
            // Verifica se tem usuario logado
            USUARIO usuario = new USUARIO();
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            if ((USUARIO)Session["UserCredentials"] != null)
            {
                usuario = (USUARIO)Session["UserCredentials"];

                // Verfifica permissão
                if (usuario.PERFIL.PERF_SG_SIGLA == "OPR" || usuario.PERFIL.PERF_SG_SIGLA == "VIS")
                {
                    Session["MensUsuario"] = 2;
                    return RedirectToAction("MontarTelaUsuario", "Usuario");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Executar
            USUARIO item = baseApp.GetItemById(id);
            objetoAntes = (USUARIO)Session["Usuario"];

            USUARIO block = new USUARIO();
            block.USUA_CD_ID = item.USUA_CD_ID;
            block.ASSI_CD_ID = item.ASSI_CD_ID;
            block.PERF_CD_ID = item.PERF_CD_ID;
            block.CARG_CD_ID = item.CARG_CD_ID;
            block.USUA_NM_NOME = item.USUA_NM_NOME;
            block.USUA_NM_LOGIN = item.USUA_NM_LOGIN;
            block.USUA_NM_EMAIL = item.USUA_NM_EMAIL;
            block.USUA_NR_TELEFONE = item.USUA_NR_TELEFONE;
            block.USUA_NR_CELULAR = item.USUA_NR_CELULAR;
            block.USUA_NM_SENHA = item.USUA_NM_SENHA;
            block.USUA_NM_SENHA_CONFIRMA = item.USUA_NM_SENHA_CONFIRMA;
            block.USUA_NM_NOVA_SENHA = item.USUA_NM_NOVA_SENHA;
            block.USUA_IN_PROVISORIO = item.USUA_IN_PROVISORIO;
            block.USUA_IN_LOGIN_PROVISORIO = item.USUA_IN_LOGIN_PROVISORIO;
            block.USUA_IN_ATIVO = item.USUA_IN_ATIVO;
            block.USUA_DT_ALTERACAO = item.USUA_DT_ALTERACAO;
            block.USUA_DT_TROCA_SENHA = item.USUA_DT_TROCA_SENHA;
            block.USUA_DT_ACESSO = item.USUA_DT_ACESSO;
            block.USUA_DT_ULTIMA_FALHA = item.USUA_DT_ULTIMA_FALHA;
            block.USUA_DT_CADASTRO = item.USUA_DT_CADASTRO;
            block.USUA_NR_ACESSOS = item.USUA_NR_ACESSOS;
            block.USUA_NR_FALHAS = item.USUA_NR_FALHAS;
            block.USUA_TX_OBSERVACOES = item.USUA_TX_OBSERVACOES;
            block.USUA_AQ_FOTO = item.USUA_AQ_FOTO;
            block.USUA_IN_LOGADO = item.USUA_IN_LOGADO;
            block.USUA_IN_BLOQUEADO = 1;
            block.USUA_DT_BLOQUEADO = DateTime.Today;

            Int32 volta = baseApp.ValidateBloqueio(block, usuario);
            listaMaster = new List<USUARIO>();
            Session["ListaUsuario"] = null;
            if (Session["FiltroUsuario"] != null)
            {
                FiltrarUsuario((USUARIO)Session["FiltroUsuario"]);
            }
            return RedirectToAction("MontarTelaUsuario");
        }

        [HttpGet]
        public ActionResult DesbloquearUsuario(Int32 id)
        {
            // Verifica se tem usuario logado
            USUARIO usuario = new USUARIO();
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            if ((USUARIO)Session["UserCredentials"] != null)
            {
                usuario = (USUARIO)Session["UserCredentials"];

                // Verfifica permissão
                if (usuario.PERFIL.PERF_SG_SIGLA == "OPR" || usuario.PERFIL.PERF_SG_SIGLA == "VIS")
                {
                    Session["MensUsuario"] = 2;
                    return RedirectToAction("MontarTelaUsuario", "Usuario");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Executar
            USUARIO item = baseApp.GetItemById(id);
            objetoAntes = (USUARIO)Session["Usuario"];

            USUARIO unblock = new USUARIO();
            unblock.USUA_CD_ID = item.USUA_CD_ID;
            unblock.ASSI_CD_ID = item.ASSI_CD_ID;
            unblock.PERF_CD_ID = item.PERF_CD_ID;
            unblock.CARG_CD_ID = item.CARG_CD_ID;
            unblock.USUA_NM_NOME = item.USUA_NM_NOME;
            unblock.USUA_NM_LOGIN = item.USUA_NM_LOGIN;
            unblock.USUA_NM_EMAIL = item.USUA_NM_EMAIL;
            unblock.USUA_NR_TELEFONE = item.USUA_NR_TELEFONE;
            unblock.USUA_NR_CELULAR = item.USUA_NR_CELULAR;
            unblock.USUA_NM_SENHA = item.USUA_NM_SENHA;
            unblock.USUA_NM_SENHA_CONFIRMA = item.USUA_NM_SENHA_CONFIRMA;
            unblock.USUA_NM_NOVA_SENHA = item.USUA_NM_NOVA_SENHA;
            unblock.USUA_IN_PROVISORIO = item.USUA_IN_PROVISORIO;
            unblock.USUA_IN_LOGIN_PROVISORIO = item.USUA_IN_LOGIN_PROVISORIO;
            unblock.USUA_IN_ATIVO = item.USUA_IN_ATIVO;
            unblock.USUA_DT_ALTERACAO = item.USUA_DT_ALTERACAO;
            unblock.USUA_DT_TROCA_SENHA = item.USUA_DT_TROCA_SENHA;
            unblock.USUA_DT_ACESSO = item.USUA_DT_ACESSO;
            unblock.USUA_DT_ULTIMA_FALHA = item.USUA_DT_ULTIMA_FALHA;
            unblock.USUA_DT_CADASTRO = item.USUA_DT_CADASTRO;
            unblock.USUA_NR_ACESSOS = item.USUA_NR_ACESSOS;
            unblock.USUA_NR_FALHAS = item.USUA_NR_FALHAS;
            unblock.USUA_TX_OBSERVACOES = item.USUA_TX_OBSERVACOES;
            unblock.USUA_AQ_FOTO = item.USUA_AQ_FOTO;
            unblock.USUA_IN_LOGADO = item.USUA_IN_LOGADO;
            unblock.USUA_IN_BLOQUEADO = 0;
            unblock.USUA_DT_BLOQUEADO = null;

            Int32 volta = baseApp.ValidateDesbloqueio(unblock, usuario);
            listaMaster = new List<USUARIO>();
            Session["ListaUsuario"] = null;
            if (Session["FiltroUsuario"] != null)
            {
                FiltrarUsuario((USUARIO)Session["FiltroUsuario"]);
            }
            return RedirectToAction("MontarTelaUsuario");
        }

        [HttpGet]
        public ActionResult DesativarUsuario(Int32 id)
        {
            // Verifica se tem usuario logado
            USUARIO usuario = new USUARIO();
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            if ((USUARIO)Session["UserCredentials"] != null)
            {
                usuario = (USUARIO)Session["UserCredentials"];

                // Verfifica permissão
                if (usuario.PERFIL.PERF_SG_SIGLA == "OPR" || usuario.PERFIL.PERF_SG_SIGLA == "VIS")
                {
                    Session["MensUsuario"] = 2;
                    return RedirectToAction("MontarTelaUsuario", "Usuario");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Executar
            USUARIO item = baseApp.GetItemById(id);
            objetoAntes = (USUARIO)Session["Usuario"];

            USUARIO dis = new USUARIO();
            dis.USUA_CD_ID = item.USUA_CD_ID;
            dis.ASSI_CD_ID = item.ASSI_CD_ID;
            dis.PERF_CD_ID = item.PERF_CD_ID;
            dis.CARG_CD_ID = item.CARG_CD_ID;
            dis.USUA_NM_NOME = item.USUA_NM_NOME;
            dis.USUA_NM_LOGIN = item.USUA_NM_LOGIN;
            dis.USUA_NM_EMAIL = item.USUA_NM_EMAIL;
            dis.USUA_NR_TELEFONE = item.USUA_NR_TELEFONE;
            dis.USUA_NR_CELULAR = item.USUA_NR_CELULAR;
            dis.USUA_NM_SENHA = item.USUA_NM_SENHA;
            dis.USUA_NM_SENHA_CONFIRMA = item.USUA_NM_SENHA_CONFIRMA;
            dis.USUA_NM_NOVA_SENHA = item.USUA_NM_NOVA_SENHA;
            dis.USUA_IN_PROVISORIO = item.USUA_IN_PROVISORIO;
            dis.USUA_IN_LOGIN_PROVISORIO = item.USUA_IN_LOGIN_PROVISORIO;
            dis.USUA_DT_TROCA_SENHA = item.USUA_DT_TROCA_SENHA;
            dis.USUA_DT_ACESSO = item.USUA_DT_ACESSO;
            dis.USUA_DT_ULTIMA_FALHA = item.USUA_DT_ULTIMA_FALHA;
            dis.USUA_DT_CADASTRO = item.USUA_DT_CADASTRO;
            dis.USUA_NR_ACESSOS = item.USUA_NR_ACESSOS;
            dis.USUA_NR_FALHAS = item.USUA_NR_FALHAS;
            dis.USUA_TX_OBSERVACOES = item.USUA_TX_OBSERVACOES;
            dis.USUA_AQ_FOTO = item.USUA_AQ_FOTO;
            dis.USUA_IN_LOGADO = item.USUA_IN_LOGADO;
            dis.USUA_IN_ATIVO = 0;
            dis.USUA_DT_ALTERACAO = DateTime.Today;

            Int32 volta = baseApp.ValidateDelete(dis, usuario);
            listaMaster = new List<USUARIO>();
            Session["ListaUsuario"] = null;
            if (Session["FiltroUsuario"] != null)
            {
                FiltrarUsuario((USUARIO)Session["FiltroUsuario"]);
            }
            return RedirectToAction("MontarTelaUsuario");
        }

        [HttpGet]
        public ActionResult ReativarUsuario(Int32 id)
        {
            // Verifica se tem usuario logado
            USUARIO usuario = new USUARIO();
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            if ((USUARIO)Session["UserCredentials"] != null)
            {
                usuario = (USUARIO)Session["UserCredentials"];

                // Verfifica permissão
                if (usuario.PERFIL.PERF_SG_SIGLA == "OPR" || usuario.PERFIL.PERF_SG_SIGLA == "VIS")
                {
                    Session["MensUsuario"] = 2;
                    return RedirectToAction("MontarTelaUsuario", "Usuario");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Executar
            USUARIO item = baseApp.GetItemById(id);
            objetoAntes = (USUARIO)Session["Usuario"];

            USUARIO en = new USUARIO();
            en.USUA_CD_ID = item.USUA_CD_ID;
            en.ASSI_CD_ID = item.ASSI_CD_ID;
            en.PERF_CD_ID = item.PERF_CD_ID;
            en.CARG_CD_ID = item.CARG_CD_ID;
            en.USUA_NM_NOME = item.USUA_NM_NOME;
            en.USUA_NM_LOGIN = item.USUA_NM_LOGIN;
            en.USUA_NM_EMAIL = item.USUA_NM_EMAIL;
            en.USUA_NR_TELEFONE = item.USUA_NR_TELEFONE;
            en.USUA_NR_CELULAR = item.USUA_NR_CELULAR;
            en.USUA_NM_SENHA = item.USUA_NM_SENHA;
            en.USUA_NM_SENHA_CONFIRMA = item.USUA_NM_SENHA_CONFIRMA;
            en.USUA_NM_NOVA_SENHA = item.USUA_NM_NOVA_SENHA;
            en.USUA_IN_PROVISORIO = item.USUA_IN_PROVISORIO;
            en.USUA_IN_LOGIN_PROVISORIO = item.USUA_IN_LOGIN_PROVISORIO;
            en.USUA_DT_TROCA_SENHA = item.USUA_DT_TROCA_SENHA;
            en.USUA_DT_ACESSO = item.USUA_DT_ACESSO;
            en.USUA_DT_ULTIMA_FALHA = item.USUA_DT_ULTIMA_FALHA;
            en.USUA_DT_CADASTRO = item.USUA_DT_CADASTRO;
            en.USUA_NR_ACESSOS = item.USUA_NR_ACESSOS;
            en.USUA_NR_FALHAS = item.USUA_NR_FALHAS;
            en.USUA_TX_OBSERVACOES = item.USUA_TX_OBSERVACOES;
            en.USUA_AQ_FOTO = item.USUA_AQ_FOTO;
            en.USUA_IN_LOGADO = item.USUA_IN_LOGADO;
            en.USUA_IN_ATIVO = 1;
            en.USUA_DT_ALTERACAO = DateTime.Today;

            Int32 volta = baseApp.ValidateReativar(en, usuario);
            listaMaster = new List<USUARIO>();
            Session["ListaUsuario"] = null;
            if (Session["FiltroUsuario"] != null)
            {
                FiltrarUsuario((USUARIO)Session["FiltroUsuario"]);
            }
            return RedirectToAction("MontarTelaUsuario");
        }

        [HttpPost]
        public void UploadFileToSession(IEnumerable<HttpPostedFileBase> files, String profile)
        {
            List<FileQueue> queue = new List<FileQueue>();

            foreach (var file in files)
            {
                FileQueue f = new FileQueue();
                f.Name = Path.GetFileName(file.FileName);
                f.ContentType = Path.GetExtension(file.FileName);

                MemoryStream ms = new MemoryStream();
                file.InputStream.CopyTo(ms);
                f.Contents = ms.ToArray();

                if (profile != null)
                {
                    if (file.FileName.Equals(profile))
                    {
                        f.Profile = 1;
                    }
                }

                queue.Add(f);
            }

            Session["FileQueueUsuario"] = queue;
        }

        [HttpPost]
        public ActionResult UploadFileQueueUsuario(FileQueue file)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idNot = (Int32)Session["IdUsuario"];
            Int32 idAss = (Int32)Session["IdAssinante"];

            if (file == null)
            {
                ModelState.AddModelError("", PlatMensagens_Resources.ResourceManager.GetString("M0019", CultureInfo.CurrentCulture));
                Session["MensUsuario"] = 10;
                return RedirectToAction("VoltarAnexoUsuario");
            }

            USUARIO item = baseApp.GetById(idNot);
            USUARIO usu = (USUARIO)Session["UserCredentials"];
            var fileName = file.Name;
            if (fileName.Length > 250)
            {
                ModelState.AddModelError("", PlatMensagens_Resources.ResourceManager.GetString("M0024", CultureInfo.CurrentCulture));
                Session["MensUsuario"] = 11;
                return RedirectToAction("VoltarAnexoUsuario");
            }
            String caminho = "/Imagens/" + idAss.ToString() + "/Usuario/" + item.USUA_CD_ID.ToString() + "/Anexos/";
            String path = Path.Combine(Server.MapPath(caminho), fileName);
            System.IO.Directory.CreateDirectory(Server.MapPath(caminho));
            System.IO.File.WriteAllBytes(path, file.Contents);

            //Recupera tipo de arquivo
            extensao = Path.GetExtension(fileName);
            String a = extensao;

            // Gravar registro
            USUARIO_ANEXO foto = new USUARIO_ANEXO();
            foto.USAN_AQ_ARQUIVO = "~" + caminho + fileName;
            foto.USAN_DT_ANEXO = DateTime.Today;
            foto.USAN_IN_ATIVO = 1;
            Int32 tipo = 3;
            if (extensao.ToUpper() == ".JPG" || extensao.ToUpper() == ".GIF" || extensao.ToUpper() == ".PNG" || extensao.ToUpper() == ".JPEG")
            {
                tipo = 1;
            }
            if (extensao.ToUpper() == ".MP4" || extensao.ToUpper() == ".AVI" || extensao.ToUpper() == ".MPEG")
            {
                tipo = 2;
            }
            if (extensao.ToUpper() == ".PDF")
            {
                tipo = 3;
            }
            foto.USAN_IN_TIPO = tipo;
            foto.USAN_NM_TITULO = fileName;
            foto.USUA_CD_ID = item.USUA_CD_ID;

            item.USUARIO_ANEXO.Add(foto);
            objetoAntes = item;
            Int32 volta = baseApp.ValidateEdit(item, item);
            return RedirectToAction("VoltarAnexoUsuario");
        }

       [HttpPost]
        public ActionResult UploadFileUsuario(HttpPostedFileBase file)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idNot = (Int32)Session["IdUsuario"];
            Int32 idAss = (Int32)Session["IdAssinante"];

            if (file == null)
            {
                ModelState.AddModelError("", PlatMensagens_Resources.ResourceManager.GetString("M0019", CultureInfo.CurrentCulture));
                Session["MensUsuario"] = 10;
                return RedirectToAction("VoltarAnexoUsuario");
            }

            USUARIO item = baseApp.GetById(idNot);
            USUARIO usu = (USUARIO)Session["UserCredentials"];
            var fileName = Path.GetFileName(file.FileName);
            if (fileName.Length > 250)
            {
                ModelState.AddModelError("", PlatMensagens_Resources.ResourceManager.GetString("M0024", CultureInfo.CurrentCulture));
                Session["MensUsuario"] = 11;
                return RedirectToAction("VoltarAnexoUsuario");
            }
            String caminho = "/Imagens/" + idAss.ToString() + "/Usuario/" + item.USUA_CD_ID.ToString() + "/Anexos/";
            String path = Path.Combine(Server.MapPath(caminho), fileName);
            System.IO.Directory.CreateDirectory(Server.MapPath(caminho));
            file.SaveAs(path);

            //Recupera tipo de arquivo
            extensao = Path.GetExtension(fileName);
            String a = extensao;

            // Gravar registro
            USUARIO_ANEXO foto = new USUARIO_ANEXO();
            foto.USAN_AQ_ARQUIVO = "~" + caminho + fileName;
            foto.USAN_DT_ANEXO = DateTime.Today;
            foto.USAN_IN_ATIVO = 1;
            Int32 tipo = 3;
            if (extensao.ToUpper() == ".JPG" || extensao.ToUpper() == ".GIF" || extensao.ToUpper() == ".PNG" || extensao.ToUpper() == ".JPEG")
            {
                tipo = 1;
            }
            if (extensao.ToUpper() == ".MP4" || extensao.ToUpper() == ".AVI" || extensao.ToUpper() == ".MPEG")
            {
                tipo = 2;
            }
            if (extensao.ToUpper() == ".PDF")
            {
                tipo = 3;
            }
            foto.USAN_IN_TIPO = tipo;
            foto.USAN_NM_TITULO = fileName;
            foto.USUA_CD_ID = item.USUA_CD_ID;

            item.USUARIO_ANEXO.Add(foto);
            objetoAntes = item;
            Int32 volta = baseApp.ValidateEdit(item, item);
            return RedirectToAction("VoltarAnexoUsuario");
        }

        [HttpPost]
        public ActionResult UploadFotoQueueUsuario(FileQueue file)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idUsu = (Int32)Session["IdUsuario"];
            Int32 idAss = (Int32)Session["IdAssinante"];

            if (file == null)
            {
                ModelState.AddModelError("", PlatMensagens_Resources.ResourceManager.GetString("M0019", CultureInfo.CurrentCulture));
                Session["MensUsuario"] = 10;
                return RedirectToAction("VoltarAnexoUsuario");
            }

            USUARIO item = baseApp.GetById(idUsu);
            USUARIO usu = (USUARIO)Session["UserCredentials"];
            var fileName = file.Name;
            if (fileName.Length > 250)
            {
                ModelState.AddModelError("", PlatMensagens_Resources.ResourceManager.GetString("M0024", CultureInfo.CurrentCulture));
                Session["MensUsuario"] = 11;
                return RedirectToAction("VoltarAnexoUsuario");
            }
            String caminho = "/Imagens/" + idAss.ToString() + "/Usuario/" + item.USUA_CD_ID.ToString() + "/Fotos/";
            String path = Path.Combine(Server.MapPath(caminho), fileName);
            System.IO.File.WriteAllBytes(path, file.Contents);

            //Recupera tipo de arquivo
            extensao = Path.GetExtension(fileName);
            String a = extensao;

            // Checa extensão
            if (extensao.ToUpper() == ".JPG" || extensao.ToUpper() == ".GIF" || extensao.ToUpper() == ".PNG" || extensao.ToUpper() == ".JPEG")
            {
                // Salva arquivo
                System.IO.File.WriteAllBytes(path, file.Contents);

                // Gravar registro
                item.USUA_AQ_FOTO = "~" + caminho + fileName;
                objeto = item;
                Int32 volta = baseApp.ValidateEdit(item, objeto);
            }
            return RedirectToAction("VoltarAnexoUsuario");
        }

        [HttpPost]
        public ActionResult UploadFotoUsuario(HttpPostedFileBase file)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idNot = (Int32)Session["IdUsuario"];
            Int32 idAss = (Int32)Session["IdAssinante"];

            if (file == null)
            {
                ModelState.AddModelError("", PlatMensagens_Resources.ResourceManager.GetString("M0019", CultureInfo.CurrentCulture));
                Session["MensUsuario"] = 10;
                return RedirectToAction("VoltarAnexoUsuario");
            }

            USUARIO item = baseApp.GetById(idNot);
            USUARIO usu = (USUARIO)Session["UserCredentials"];
            var fileName = Path.GetFileName(file.FileName);
            if (fileName.Length > 250)
            {
                ModelState.AddModelError("", PlatMensagens_Resources.ResourceManager.GetString("M0024", CultureInfo.CurrentCulture));
                Session["MensUsuario"] = 11;
                return RedirectToAction("VoltarAnexoUsuario");
            }
            String caminho = "/Imagens/" + idAss.ToString() + "/Usuario/" + item.USUA_CD_ID.ToString() + "/Fotos/";
            String path = Path.Combine(Server.MapPath(caminho), fileName);
            file.SaveAs(path);

            //Recupera tipo de arquivo
            extensao = Path.GetExtension(fileName);
            String a = extensao;

            // Checa extensão
            if (extensao.ToUpper() == ".JPG" || extensao.ToUpper() == ".GIF" || extensao.ToUpper() == ".PNG" || extensao.ToUpper() == ".JPEG")
            {
                // Salva arquivo
                file.SaveAs(path);

                // Gravar registro
                item.USUA_AQ_FOTO = "~" + caminho + fileName;
                objeto = item;
                Int32 volta = baseApp.ValidateEdit(item, objeto);
            }
            return RedirectToAction("VoltarAnexoUsuario");
        }
    }
}