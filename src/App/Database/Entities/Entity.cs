using System.Text.Json;

namespace App.Database.Entities;

internal sealed class Entity
{
    public Guid Id { get; set; }
    public required JsonDocument Json { get; set; }
}