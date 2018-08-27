﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace OnlinePermissionSlips.Models.DAL
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class OnlinePermissionSlipEntities : DbContext
    {
        public OnlinePermissionSlipEntities()
            : base("name=OnlinePermissionSlipEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<ClassRoom> ClassRooms { get; set; }
        public virtual DbSet<GuardianApproval> GuardianApprovals { get; set; }
        public virtual DbSet<PermissionSlip> PermissionSlips { get; set; }
        public virtual DbSet<School> Schools { get; set; }
        public virtual DbSet<Student> Students { get; set; }
        public virtual DbSet<Template> Templates { get; set; }
        public virtual DbSet<AspNetUser> AspNetUsers { get; set; }
        public virtual DbSet<GuardianTempEmail> GuardianTempEmails { get; set; }
        public virtual DbSet<AspNetRole> AspNetRoles { get; set; }
        public virtual DbSet<GuardianPermissionSlipsView> GuardianPermissionSlipsViews { get; set; }
        public virtual DbSet<TeacherPermissionSlipsView> TeacherPermissionSlipsViews { get; set; }
        public virtual DbSet<SystemConfiguration> SystemConfigurations { get; set; }
    }
}