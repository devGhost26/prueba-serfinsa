using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace GestionProductos.Api.Data;

// Marca como UTC las fechas que vienen de la base.
public class UtcDateTimeConverter() : ValueConverter<DateTime, DateTime>(
    value => value.ToUniversalTime(),
    value => DateTime.SpecifyKind(value, DateTimeKind.Utc));
