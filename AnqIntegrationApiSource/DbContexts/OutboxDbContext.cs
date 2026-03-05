using AnqIntegrationApi.Models.Outbox;
using AnqIntegrationApi.Models.Settings;
using Microsoft.EntityFrameworkCore;

namespace AnqIntegrationApi.DbContexts
{
    public class OutboxDbContext : DbContext
    {
        public OutboxDbContext(DbContextOptions<OutboxDbContext> options)
            : base(options)
        {
        }

        public DbSet<BrevoOutboxMessage> BrevoOutbox => Set<BrevoOutboxMessage>();

        // Optional DbSets (useful for querying; inserts can still be raw SQL)
        public DbSet<BrevoOutboxAttempt> BrevoOutboxAttempts => Set<BrevoOutboxAttempt>();
        public DbSet<BrevoWhatsappEvent> BrevoWhatsappEvents => Set<BrevoWhatsappEvent>();
        public DbSet<BrevoSyncState> BrevoSyncStates => Set<BrevoSyncState>();

        // WhatsApp Opt-in workflow
        public DbSet<WhatsAppOptinRequest> WhatsAppOptinRequests => Set<WhatsAppOptinRequest>();
        public DbSet<WhatsappOptInToken> WhatsappOptInTokens => Set<WhatsappOptInToken>();
        public DbSet<WhatsAppOptinSendQueueItem> WhatsAppOptinSendQueue => Set<WhatsAppOptinSendQueueItem>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            /* ----------------------------
               BrevoOutboxMessage
               ---------------------------- */
            modelBuilder.Entity<BrevoOutboxMessage>(e =>
            {
                e.ToTable("BrevoOutbox");
                e.HasKey(x => x.Id);
            });

            /* ----------------------------
               BrevoOutboxAttempt
               ---------------------------- */
            modelBuilder.Entity<BrevoOutboxAttempt>(e =>
            {
                e.ToTable("BrevoOutboxAttempt");
                e.HasKey(x => x.AttemptId);

                e.HasIndex(x => new { x.OutboxId, x.AttemptNo })
                    .IsUnique()
                    .HasDatabaseName("UX_BrevoOutboxAttempt_OutboxId_AttemptNo");

                e.HasIndex(x => new { x.OutboxId, x.AttemptUtc })
                    .HasDatabaseName("IX_BrevoOutboxAttempt_OutboxId_AttemptUtc");
            });

            /* ----------------------------
               BrevoSyncState
               ---------------------------- */
            modelBuilder.Entity<BrevoSyncState>(e =>
            {
                e.ToTable("BrevoSyncState");
                e.HasKey(x => x.Name);
            });

            /* ----------------------------
               BrevoWhatsappEvent
               ---------------------------- */
            modelBuilder.Entity<BrevoWhatsappEvent>(e =>
            {
                e.ToTable("BrevoWhatsappEvent");
                e.HasKey(x => x.Id);

                e.HasIndex(x => x.MessageId)
                    .HasDatabaseName("IX_BrevoWhatsappEvent_MessageId");

                e.HasIndex(x => x.ContactNumber)
                    .HasDatabaseName("IX_BrevoWhatsappEvent_ContactNumber");

                // IMPORTANT:
                // Do NOT map shadow computed columns (MessageIdKey/ContactNumberKey).
                // They are created by SQL upgrade scripts and are only used by SQL indexes.
                // Mapping them causes EF to reference them in INSERT OUTPUT, which fails if
                // the columns are missing in a DB (e.g., dev not upgraded yet).
            });

            /* ----------------------------
               WhatsAppOptinRequest
               ---------------------------- */
            modelBuilder.Entity<WhatsAppOptinRequest>(e =>
            {
                e.ToTable("WhatsAppOptinRequests");
                e.HasKey(x => x.Id);

                e.Property(x => x.Email).HasMaxLength(320).IsRequired();
                e.Property(x => x.FirstName).HasMaxLength(100);
                e.Property(x => x.LastName).HasMaxLength(100);
                e.Property(x => x.Phone).HasMaxLength(50);
                e.Property(x => x.WhatsApp).HasMaxLength(50);

                // Token stored in plaintext here for tracking
                // Base64Url(32 bytes) is typically ~43 chars, so 100 is safe.
                e.Property(x => x.Token).HasMaxLength(100).IsRequired();

                e.Property(x => x.Status).HasMaxLength(30).IsRequired();
                e.Property(x => x.ConsumeIp).HasMaxLength(64);
                e.Property(x => x.ConsumeUserAgent).HasMaxLength(256);

                e.HasIndex(x => x.Token)
                    .IsUnique()
                    .HasDatabaseName("UX_WhatsAppOptinRequests_Token");

                e.HasIndex(x => x.RequestNumber)
                    .IsUnique()
                    .HasDatabaseName("UX_WhatsAppOptinRequests_RequestNumber");

                e.HasIndex(x => new { x.Email, x.CreatedOnUtc })
                    .HasDatabaseName("IX_WhatsAppOptinRequests_Email_CreatedOnUtc");
            });

            /* ----------------------------
               WhatsappOptInToken  (Option A action token)
               Stored in Outbox DB ONLY
               ---------------------------- */
            modelBuilder.Entity<WhatsappOptInToken>(e =>
            {
                e.ToTable("WhatsappOptInTokens", "dbo");
                e.HasKey(x => x.Id);

                e.Property(x => x.ApiClientId).IsRequired();
                e.Property(x => x.CustomerId).IsRequired();

                e.Property(x => x.Email).HasMaxLength(320).IsRequired();
                e.Property(x => x.Purpose).HasMaxLength(64).IsRequired();
                e.Property(x => x.TokenHash).HasMaxLength(256).IsRequired();

                e.Property(x => x.CreatedUtc).IsRequired();
                e.Property(x => x.ExpiresUtc).IsRequired();
                e.Property(x => x.UsedUtc).IsRequired(false);

                // Helpful indexes for validation and cleanup jobs
                e.HasIndex(x => x.ExpiresUtc).HasDatabaseName("IX_WhatsappOptInTokens_ExpiresUtc");
                e.HasIndex(x => new { x.ApiClientId, x.CreatedUtc }).HasDatabaseName("IX_WhatsappOptInTokens_ApiClientId_CreatedUtc");
            });

            modelBuilder.Entity<WhatsAppOptinSendQueueItem>(e =>
            {
                e.ToTable("WhatsAppOptinSendQueue", "dbo");
                e.HasKey(x => x.Id);
                e.Property(x => x.Username).HasMaxLength(100).IsUnicode(false);

                e.Property(x => x.Email).HasMaxLength(320);
                e.Property(x => x.FirstName).HasMaxLength(100);
                e.Property(x => x.LastName).HasMaxLength(100);
                e.Property(x => x.Phone).HasMaxLength(50);
                e.Property(x => x.WhatsApp).HasMaxLength(50);

                e.Property(x => x.BaseUrl).HasMaxLength(500).IsRequired();

                e.Property(x => x.Status).HasMaxLength(30).IsRequired();
                e.Property(x => x.LastError).HasMaxLength(1000);
                e.Property(x => x.LockedBy).HasMaxLength(100);
            });

        }
    }
}
