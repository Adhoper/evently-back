# Evently API

API REST de Evently desarrollada con ASP.NET Core, Entity Framework Core y SQL Server.

## Funcionalidades

- Autenticación JWT
- Roles User, Organizer y Admin
- CRUD y publicación de eventos
- Carga local de imágenes de eventos
- Reservas, QR y check-in
- Estadísticas de organizadores
- Administración de usuarios, eventos y categorías

## Ejecución local

Configura la cadena `DefaultConnection` y los valores JWT en `Evently.Api/appsettings.json`, aplica las migraciones y ejecuta la API.

Para habilitar la primera cuenta administradora, edita el correo en `Scripts/PromoteAdmin.sql` y ejecuta el script sobre la base de datos.
