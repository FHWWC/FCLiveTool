using System;
using System.Collections.Generic;
using System.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace FCLiveTool.Models.VideoListModel
{
    public partial class WAAccountContext : DbContext
    {
        public WAAccountContext()
        {
        }

        public WAAccountContext(DbContextOptions<WAAccountContext> options)
            : base(options)
        {

        }

        public virtual DbSet<AllModel.VideoList> VideoLists { get; set; }
        public virtual DbSet<AllModel.RecentVList>  RecentVLists { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AllModel.VideoList>(entity =>
            {
                entity.ToTable("VideoList");

                entity.Property(e => e.SourceLink).IsRequired();

                entity.Property(e => e.SourceName).IsRequired();
            });
            modelBuilder.Entity<AllModel.RecentVList>(entity =>
            {
                entity.ToTable("RecentVList");

                entity.Property(e => e.LogoLink).IsRequired();

                entity.Property(e => e.SourceLink).IsRequired();

                entity.Property(e => e.SourceName).IsRequired();

                entity.Property(e => e.AddDT).IsRequired();
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
