using Evently.Api.Data;
using Evently.Api.Models.Enums;
using Evently.Api.Services.Interfaces;
using Evently.Api.Services.Results;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Evently.Api.Services
{
    public class EventImageService : IEventImageService
    {
        private readonly EventlyDbContext _context;
        private readonly IWebHostEnvironment _environment;

        private const long MaxFileSize =
            5 * 1024 * 1024;

        private static readonly Dictionary<
            string,
            string[]
        > AllowedFileTypes =
            new(StringComparer.OrdinalIgnoreCase)
            {
                {
                    ".jpg",
                    new[] { "image/jpeg" }
                },
                {
                    ".jpeg",
                    new[] { "image/jpeg" }
                },
                {
                    ".png",
                    new[] { "image/png" }
                },
                {
                    ".webp",
                    new[] { "image/webp" }
                }
            };

        public EventImageService(
            EventlyDbContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // =====================================================
        // UPLOAD / REPLACE
        // =====================================================

        public async Task<ServiceResult<string>>
            UploadAsync(
                int eventId,
                int organizerId,
                IFormFile file)
        {
            // =================================================
            // EVENT
            // =================================================

            var eventEntity =
                await _context.Events
                    .FirstOrDefaultAsync(e =>
                        e.Id == eventId &&
                        e.OrganizerId == organizerId);

            if (eventEntity is null)
            {
                return ServiceResult<string>
                    .Missing(
                        "El evento no existe o no pertenece al organizador.");
            }

            if (
                eventEntity.Status ==
                    EventStatus.Cancelled ||
                eventEntity.Status ==
                    EventStatus.Finished)
            {
                return ServiceResult<string>
                    .Failure(
                        "No se puede modificar la imagen de este evento.");
            }

            // =================================================
            // FILE VALIDATION
            // =================================================

            if (
                file is null ||
                file.Length == 0)
            {
                return ServiceResult<string>
                    .Failure(
                        "Debes seleccionar una imagen.");
            }

            if (
                file.Length >
                MaxFileSize)
            {
                return ServiceResult<string>
                    .Failure(
                        "La imagen no puede superar los 5 MB.");
            }

            var extension =
                Path.GetExtension(
                        file.FileName)
                    .ToLowerInvariant();

            if (
                string.IsNullOrWhiteSpace(
                    extension) ||
                !AllowedFileTypes.TryGetValue(
                    extension,
                    out var allowedContentTypes))
            {
                return ServiceResult<string>
                    .Failure(
                        "Solo se permiten imágenes JPG, JPEG, PNG o WEBP.");
            }

            if (
                string.IsNullOrWhiteSpace(
                    file.ContentType) ||
                !allowedContentTypes.Contains(
                    file.ContentType,
                    StringComparer.OrdinalIgnoreCase))
            {
                return ServiceResult<string>
                    .Failure(
                        "El tipo del archivo seleccionado no es válido.");
            }

            // =================================================
            // DIRECTORY
            // =================================================

            var webRootPath =
                _environment.WebRootPath;

            if (
                string.IsNullOrWhiteSpace(
                    webRootPath))
            {
                webRootPath =
                    Path.Combine(
                        _environment.ContentRootPath,
                        "wwwroot");
            }

            var uploadsDirectory =
                Path.Combine(
                    webRootPath,
                    "uploads",
                    "events");

            Directory.CreateDirectory(
                uploadsDirectory);

            // =================================================
            // NEW FILE
            // =================================================

            var fileName =
                $"{Guid.NewGuid():N}{extension}";

            var fullFilePath =
                Path.Combine(
                    uploadsDirectory,
                    fileName);

            var relativePath =
                $"/uploads/events/{fileName}";

            var previousImageUrl =
                eventEntity.ImageUrl;

            try
            {
                await using (
                    var stream =
                        new FileStream(
                            fullFilePath,
                            FileMode.CreateNew,
                            FileAccess.Write,
                            FileShare.None))
                {
                    await file.CopyToAsync(
                        stream);
                }

                // =============================================
                // SAVE PATH
                // =============================================

                eventEntity.ImageUrl =
                    relativePath;

                try
                {
                    await _context
                        .SaveChangesAsync();
                }
                catch
                {
                    DeletePhysicalImage(
                        relativePath);

                    throw;
                }

                // =============================================
                // DELETE PREVIOUS FILE
                // =============================================

                if (
                    !string.IsNullOrWhiteSpace(
                        previousImageUrl) &&
                    !string.Equals(
                        previousImageUrl,
                        relativePath,
                        StringComparison.OrdinalIgnoreCase))
                {
                    DeletePhysicalImage(
                        previousImageUrl);
                }

                return ServiceResult<string>
                    .Ok(relativePath);
            }
            catch
            {
                if (
                    File.Exists(
                        fullFilePath))
                {
                    try
                    {
                        File.Delete(
                            fullFilePath);
                    }
                    catch
                    {
                        // Si la limpieza falla,
                        // no ocultamos el error original.
                    }
                }

                throw;
            }
        }

        // =====================================================
        // REMOVE
        // =====================================================

        public async Task<ServiceResult<string>>
            RemoveAsync(
                int eventId,
                int organizerId)
        {
            var eventEntity =
                await _context.Events
                    .FirstOrDefaultAsync(e =>
                        e.Id == eventId &&
                        e.OrganizerId == organizerId);

            if (eventEntity is null)
            {
                return ServiceResult<string>
                    .Missing(
                        "El evento no existe o no pertenece al organizador.");
            }

            if (
                eventEntity.Status ==
                    EventStatus.Cancelled ||
                eventEntity.Status ==
                    EventStatus.Finished)
            {
                return ServiceResult<string>
                    .Failure(
                        "No se puede modificar la imagen de este evento.");
            }

            // Si no tiene imagen, consideramos que
            // la operación ya está completada.
            if (
                string.IsNullOrWhiteSpace(
                    eventEntity.ImageUrl))
            {
                return ServiceResult<string>
                    .Ok(
                        "El evento no tiene una imagen asignada.");
            }

            var previousImageUrl =
                eventEntity.ImageUrl;

            eventEntity.ImageUrl =
                null;

            await _context
                .SaveChangesAsync();

            DeletePhysicalImage(
                previousImageUrl);

            return ServiceResult<string>
                .Ok(
                    "Imagen eliminada correctamente.");
        }

        // =====================================================
        // DELETE PHYSICAL FILE
        // =====================================================

        private void DeletePhysicalImage(
            string imageUrl)
        {
            /*
             * Solo eliminamos archivos pertenecientes
             * a nuestra carpeta de eventos.
             *
             * De esta manera tampoco intentamos
             * eliminar antiguas URLs externas.
             */
            if (
                string.IsNullOrWhiteSpace(
                    imageUrl) ||
                !imageUrl.StartsWith(
                    "/uploads/events/",
                    StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var fileName =
                Path.GetFileName(
                    imageUrl);

            if (
                string.IsNullOrWhiteSpace(
                    fileName))
            {
                return;
            }

            var webRootPath =
                _environment.WebRootPath;

            if (
                string.IsNullOrWhiteSpace(
                    webRootPath))
            {
                webRootPath =
                    Path.Combine(
                        _environment.ContentRootPath,
                        "wwwroot");
            }

            var fullPath =
                Path.Combine(
                    webRootPath,
                    "uploads",
                    "events",
                    fileName);

            if (!File.Exists(
                fullPath))
            {
                return;
            }

            try
            {
                File.Delete(
                    fullPath);
            }
            catch
            {
                /*
                 * La BD ya dejó de utilizar
                 * la imagen, por lo que no
                 * hacemos fallar la operación
                 * si la limpieza física falla.
                 */
            }
        }
    }
}