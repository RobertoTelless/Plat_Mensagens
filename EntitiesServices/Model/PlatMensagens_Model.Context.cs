﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace EntitiesServices.Model
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class PlatMensagensEntities : DbContext
    {
        public PlatMensagensEntities()
            : base("name=PlatMensagensEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<ASSINANTE> ASSINANTE { get; set; }
        public virtual DbSet<ASSINANTE_ANEXO> ASSINANTE_ANEXO { get; set; }
        public virtual DbSet<CARGO> CARGO { get; set; }
        public virtual DbSet<CATEGORIA_CLIENTE> CATEGORIA_CLIENTE { get; set; }
        public virtual DbSet<CATEGORIA_NOTIFICACAO> CATEGORIA_NOTIFICACAO { get; set; }
        public virtual DbSet<CATEGORIA_USUARIO> CATEGORIA_USUARIO { get; set; }
        public virtual DbSet<CLIENTE> CLIENTE { get; set; }
        public virtual DbSet<CLIENTE_ANEXO> CLIENTE_ANEXO { get; set; }
        public virtual DbSet<CLIENTE_CONTATO> CLIENTE_CONTATO { get; set; }
        public virtual DbSet<CLIENTE_QUADRO_SOCIETARIO> CLIENTE_QUADRO_SOCIETARIO { get; set; }
        public virtual DbSet<CONFIGURACAO> CONFIGURACAO { get; set; }
        public virtual DbSet<GRUPO> GRUPO { get; set; }
        public virtual DbSet<GRUPO_CLIENTE> GRUPO_CLIENTE { get; set; }
        public virtual DbSet<LOG> LOG { get; set; }
        public virtual DbSet<MENSAGEM_ANEXO> MENSAGEM_ANEXO { get; set; }
        public virtual DbSet<MENSAGENS> MENSAGENS { get; set; }
        public virtual DbSet<MENSAGENS_DESTINOS> MENSAGENS_DESTINOS { get; set; }
        public virtual DbSet<NOTIFICACAO> NOTIFICACAO { get; set; }
        public virtual DbSet<NOTIFICACAO_ANEXO> NOTIFICACAO_ANEXO { get; set; }
        public virtual DbSet<PERFIL> PERFIL { get; set; }
        public virtual DbSet<POSICAO> POSICAO { get; set; }
        public virtual DbSet<TEMPLATE> TEMPLATE { get; set; }
        public virtual DbSet<TIPO_PESSOA> TIPO_PESSOA { get; set; }
        public virtual DbSet<UF> UF { get; set; }
        public virtual DbSet<USUARIO> USUARIO { get; set; }
        public virtual DbSet<USUARIO_ANEXO> USUARIO_ANEXO { get; set; }
        public virtual DbSet<EMAIL_AGENDAMENTO> EMAIL_AGENDAMENTO { get; set; }
        public virtual DbSet<CRM> CRM { get; set; }
        public virtual DbSet<CRM_ANEXO> CRM_ANEXO { get; set; }
        public virtual DbSet<CRM_COMENTARIO> CRM_COMENTARIO { get; set; }
        public virtual DbSet<TIPO_CRM> TIPO_CRM { get; set; }
        public virtual DbSet<CRM_ACAO> CRM_ACAO { get; set; }
        public virtual DbSet<CRM_CONTATO> CRM_CONTATO { get; set; }
        public virtual DbSet<CRM_ORIGEM> CRM_ORIGEM { get; set; }
        public virtual DbSet<MOTIVO_CANCELAMENTO> MOTIVO_CANCELAMENTO { get; set; }
        public virtual DbSet<MOTIVO_ENCERRAMENTO> MOTIVO_ENCERRAMENTO { get; set; }
        public virtual DbSet<TIPO_ACAO> TIPO_ACAO { get; set; }
        public virtual DbSet<AGENDA> AGENDA { get; set; }
        public virtual DbSet<AGENDA_ANEXO> AGENDA_ANEXO { get; set; }
        public virtual DbSet<AGENDA_VINCULO> AGENDA_VINCULO { get; set; }
        public virtual DbSet<CATEGORIA_AGENDA> CATEGORIA_AGENDA { get; set; }
    }
}
