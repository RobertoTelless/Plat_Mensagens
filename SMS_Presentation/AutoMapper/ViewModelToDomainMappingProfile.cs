using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AutoMapper;
using EntitiesServices.Model;
using PlatMensagem_Solution.ViewModels;

namespace MvcMapping.Mappers
{
    public class ViewModelToDomainMappingProfile : Profile
    {
        public ViewModelToDomainMappingProfile()
        {
            CreateMap<UsuarioViewModel, USUARIO>();
            CreateMap<UsuarioLoginViewModel, USUARIO>();
            CreateMap<LogViewModel, LOG>();
            CreateMap<ConfiguracaoViewModel, CONFIGURACAO>();
            CreateMap<NotificacaoViewModel, NOTIFICACAO>();
            CreateMap<ClienteViewModel, CLIENTE>();
            CreateMap<ClienteContatoViewModel, CLIENTE_CONTATO>();
            CreateMap<MensagemViewModel, MENSAGENS>();
            CreateMap<GrupoViewModel, GRUPO>();
            CreateMap<GrupoContatoViewModel, GRUPO_CLIENTE>();
            CreateMap<CategoriaClienteViewModel, CATEGORIA_CLIENTE>();
            CreateMap<PosicaoViewModel, POSICAO>();
        }
    }
}