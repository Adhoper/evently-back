# 🔌 Evently – API REST

API REST desarrollada para **Evently**, encargada de gestionar usuarios, eventos, entradas, autenticación, check-in, estadísticas y administración de la plataforma.

El backend fue desarrollado con **ASP.NET Core**, **Entity Framework Core** y **SQL Server**.

## 🚀 API

🔗 **API publicada:**  
`Agregar aquí el enlace cuando esté publicado`

🔗 **Swagger:**  
`Agregar aquí el enlace de Swagger cuando esté publicado`

## 📸 Vista de la API

![Evently API](./screenshots/swagger.png)

## ✨ Funcionalidades

- 🔐 Registro e inicio de sesión mediante JWT.
- 🔑 Recuperación de contraseña mediante correo electrónico.
- 👥 Roles de **User, Organizer y Admin**.
- 📅 Gestión de eventos.
- 🖼️ Carga de imágenes para eventos.
- 🎫 Reserva y cancelación de entradas.
- 🔢 Control de disponibilidad y capacidad.
- 📱 Generación de códigos únicos para tickets.
- ✅ Check-in de asistentes.
- 🚫 Prevención de doble check-in.
- 👥 Consulta de asistentes por evento.
- 📊 Estadísticas de reservas, ocupación y asistencia.
- 🛡️ Administración de usuarios, eventos y categorías.
- 🔒 Validación de permisos y propiedad de recursos.

## 📚 Endpoints Principales

- `/api/auth` → Autenticación y recuperación de contraseña.
- `/api/users` → Información y operaciones del usuario.
- `/api/events` → Gestión de eventos.
- `/api/tickets` → Reservas, tickets y check-in.
- `/api/organizer` → Estadísticas y asistentes.
- `/api/categories` → Categorías.
- `/api/admin` → Administración general.

## 🔐 Seguridad

- Autenticación mediante JWT.
- Autorización basada en roles.
- Contraseñas almacenadas mediante hashing.
- Tokens de recuperación temporales y de un solo uso.
- Validación de propiedad de eventos.
- Control de capacidad desde el backend.
- Prevención de reutilización de entradas.
- API Keys y credenciales sensibles fuera del repositorio.

## 📧 Recuperación de Contraseña

Evently utiliza **Brevo Transactional Email API** para enviar enlaces seguros de recuperación de contraseña.

Los enlaces:

- Tienen tiempo de expiración.
- Solo pueden utilizarse una vez.
- Utilizan tokens cuyo hash se almacena en la base de datos.

## 🛠️ Tecnologías

- ASP.NET Core
- Entity Framework Core
- SQL Server
- JWT Authentication
- Swagger / OpenAPI
- Brevo Transactional Email API
- LINQ

## 🌐 Frontend

La API es consumida por un frontend desarrollado con **React + TypeScript**.

🔗 **Aplicación:**  
`Agregar aquí el enlace del frontend cuando esté publicado`

---

© 2026 Evently. Todos los derechos reservados.

Hecho con ❤️ por **Adhoper**.
