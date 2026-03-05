using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using AnqIntegrationApi.Models.AM;
namespace AnqIntegrationApi.DbContexts;

public partial class AccountMateDbContext : DbContext
{
    public AccountMateDbContext()
    {
    }

    public AccountMateDbContext(DbContextOptions<AccountMateDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Arcadr> Arcadr { get; set; }

    public virtual DbSet<Arcapp> Arcapp { get; set; }

    public virtual DbSet<Arcash> Arcashe { get; set; }

    public virtual DbSet<Arcust> Arcust { get; set; }

    public virtual DbSet<Arinvc> Arinvc { get; set; }

    public virtual DbSet<Aritrk> Aritrk { get; set; }

    public virtual DbSet<Icikit> Icikit { get; set; }

    public virtual DbSet<Icitem> Icitem { get; set; }

    public virtual DbSet<Soship> Soship { get; set; }

    public virtual DbSet<Soskit> Soskit { get; set; }

    public virtual DbSet<Sosord> Sosord { get; set; }

    public virtual DbSet<Sosptr> Sosptr { get; set; }

    public virtual DbSet<Sostr> Sostrs { get; set; }

    public virtual DbSet<Soxitem> Soxitem { get; set; }

  
}
