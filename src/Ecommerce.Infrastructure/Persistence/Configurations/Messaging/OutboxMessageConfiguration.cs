using Ecommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecommerce.Infrastructure.Persistence.Configurations.Messaging;

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages", "messaging");
        builder.HasKey(message => message.Id);

        builder.Property(message => message.Type).HasMaxLength(500).IsRequired();
        builder.Property(message => message.Payload).HasColumnType("jsonb").IsRequired();
        builder.Property(message => message.Error).HasMaxLength(2000);

        builder.HasIndex(message => message.ProcessedAt);
    }
}
