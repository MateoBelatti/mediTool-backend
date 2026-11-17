# AGENTS.md — mediTool (backend)

Backend de **mediTool**, sistema de gestión de turnos médicos/clínicos para consultorios. ASP.NET Core (.NET 10), arquitectura hexagonal (puertos y adaptadores), EF Core + PostgreSQL, JWT con refresh tokens y AutoMapper. El frontend (React/TypeScript) vive en otro repo.

Todo el dominio, los nombres de entidades, DTOs, mensajes de error y commits están en **español**. Mantené ese idioma.

---

## Estructura de la solución

La solución es `mediTool.slnx`. Cada proyecto tiene una responsabilidad clara:

| Proyecto | Responsabilidad |
|---|---|
| `Biblioteca/` | Entidades (`Entities/`), `ApplicationDbContext`, extensiones del contexto y **migraciones** (`Migrations/`). |
| `Repository/` | Acceso a datos. Una carpeta por agregado con `IXRepository` + `XRepository`. |
| `Service/` | Lógica de negocio. Una carpeta por módulo con `IXService` + `XService`. |
| `Utils/` | DTOs (`DTOs/<Modulo>/`), enums, excepciones propias (`Exceptions/`) y helpers (`Helpers/`). |
| `mediTool/` | Capa API: `Controllers/`, `Mappers/` (AutoMapper), `Middlewares/`, `Security/`, `Seeders/`, `Backgrounds/` y `Program.cs`. |
| `Service.Tests/` | Tests de servicios (xUnit + FluentAssertions + NSubstitute). |
| `Api.Tests/` | Tests de middlewares y seeders. |

### Dirección de dependencias (no romper)

```
mediTool (API) → Service → Repository → Biblioteca → Utils
```

- `Utils` no depende de nadie.
- `Biblioteca` solo conoce `Utils`.
- `Repository` conoce `Biblioteca` y `Utils`. **Nunca** referencia `Service`.
- `Service` usa interfaces de `Repository`; no usa `DbContext` directamente salvo las extensiones ya existentes en `ApplicationDbContextExtensions`.
- Los controllers solo hablan con interfaces de `Service`. **Nunca** inyectar repositorios ni `DbContext` en un controller.

---

## Comandos

Desde la raíz del repo (donde está `mediTool.slnx`):

```bash
dotnet build                                  # compilar toda la solución
dotnet test                                   # toda la suite
dotnet test Service.Tests                     # solo servicios
dotnet test Api.Tests                         # solo middlewares/seeders
dotnet test --filter "FullyQualifiedName~Nombre"   # un test o grupo puntual
dotnet run --project mediTool                 # levantar la API
```

Migraciones (el DbContext está en `Biblioteca`, el proyecto de arranque es `mediTool`):

```bash
dotnet ef migrations add NombreEnPascalCase --project Biblioteca --startup-project mediTool
dotnet ef database update --project Biblioteca --startup-project mediTool
```

Al arrancar, `Program.cs` ya ejecuta `Database.Migrate()` y el `AdminSeeder`.

**Antes de dar una tarea por terminada:** `dotnet build` sin errores y `dotnet test` en verde.

---

## Configuración y secretos

- Config local en `mediTool/appsettings.Development.json` (a partir de `appsettings.Example.json`) o variables de entorno (ver `.env.example`).
- Variables relevantes: `Jwt__Key`, `Jwt__Issuer`, `Jwt__Audience`, `Jwt__AccessTokenExpirationMinutes`, `Jwt__RefreshTokenExpirationDays`, `ConnectionStrings__DefaultConnection`, `Seed__AdminEmail`, `Seed__AdminPassword`, `Cors:AllowedOrigins`.
- **Nunca** commitear claves, contraseñas ni connection strings reales. **Nunca** leer ni imprimir el contenido de `appsettings.Development.json` o `.env` en tus respuestas.
- CORS por defecto: `http://localhost:5173` (frontend en dev).

---

## Modelo de dominio (resumen)

- **Profesional**: usuario del sistema. Roles: solo `Admin` y `Profesional` (enum `Rol`). **No existe rol Cliente/Paciente ni login de pacientes; no agregarlo.**
- **Paciente**: tiene baja lógica (`Activo`, `FechaBaja`). Se vincula a profesionales mediante `PacienteProfesional` (N:N).
- **TurnoFijo**: regla recurrente. **Turno**: instancia concreta, generada por `GenerarInstanciasTurnoService` a partir de las reglas.
- **Turno** ya incluye la asistencia (`Estado`, `Justificada`, `Facturable`, `FechaRegistroAsistencia`, `Observaciones`). **La tabla/módulo `Asistencia` fue eliminada**; no la reintroduzcas.
- `EstadoTurno`: `Pendiente`, `Presente`, `Cancelado`, `Reprogramado`, `Ausente` (se persiste como string).
- **Informe**: un informe por turno como máximo (`TurnoId`); también puede existir sin turno. Validaciones: el turno debe ser del mismo profesional, el paciente debe coincidir con el del turno y debe estar vinculado al profesional.
- **Reunion** y **Notificacion** (`TipoNotificacion`: `Sistema`, `TurnosSinAsistencia`, `RecordatorioEnviado`, `ErrorRecordatorio`).
- Background job: `NotificacionAsistenciasPendientesService` (cada 24 h crea una notificación por profesional con turnos sin asistencia registrada, sin duplicar el mismo día).

---

## Convenciones de código

### Entidades y base de datos
- Entidades con data annotations: `[Table("snake_case")]`, `[Column("snake_case")]`, `[Key]`, `[Required]`, `[MaxLength]`.
- Tablas y columnas en `snake_case`, singular (`turno`, `paciente_profesional`). Propiedades C# en PascalCase.
- Borrado: `DeleteBehavior.Restrict` en las relaciones. Para pacientes se usa baja lógica, no borrado físico.
- Toda modificación del modelo requiere **migración** con nombre descriptivo en PascalCase (ej. `AgregarEscuelaAPaciente`). No editar migraciones ya aplicadas ni el snapshot a mano.
- Registrar nuevos `DbSet` en `ApplicationDbContext`.

### Repositorios
- Interfaz + implementación en la misma carpeta (`Repository/<Agregado>/`).
- Métodos async. Validar ids (`<= 0` → `ArgumentOutOfRangeException`) como en los repos existentes.
- El repositorio no tiene reglas de negocio; solo consultas y persistencia (`GuardarCambios()` lo llama el servicio).

### Servicios
- Interfaz + implementación en `Service/<Modulo>/`.
- Acá van las reglas de negocio y las validaciones. Lanzar **excepciones propias** de `Utils.Exceptions`, no devolver códigos HTTP:
  `NotFoundError` (404), `ConflictError` (409), `ForbiddenError` (403), `UnauthorizedError` (401), `ValidationError` (400), `InternalError` (500).
- Mapear con `IMapper`. Los controllers y servicios devuelven **DTOs de respuesta, nunca entidades**.

### Controllers
- Atributos estándar: `[Authorize]`, `[Route("api/[controller]")]`, `[ApiController]`.
- Políticas definidas en `Program.cs`: `Admin`, `Profesional`, `AdminOrProfesional`.
- **Ownership obligatorio** sobre recursos de un profesional: usar `User.EnsureOwnership(recurso.ProfesionalId)` (Admin pasa siempre). Si el usuario no es Admin, forzar `dto.ProfesionalId = User.GetUserId()` al crear/editar.
- Los controllers son finos: validan `ModelState`, chequean permisos, llaman al servicio. Sin lógica de negocio.
- Registrar todo servicio/repositorio nuevo en `Program.cs` (`AddScoped`).

### DTOs y mapeos
- DTOs en `Utils/DTOs/<Modulo>/` con sufijos `CreateDto`, `UpdateDto`, `ResponseDto` (algunos módulos de turnos usan `Crear...Dto` / `Editar...Dto`; seguí el estilo del módulo que estés tocando).
- Validar con data annotations en los DTOs de entrada.
- Los mapeos van en `mediTool/Mappers/MappingProfile.cs`. Ignorar explícitamente `Id`, `CreatedAt` y campos controlados por el sistema.
- Respetar los namespaces existentes aunque sean inconsistentes (`Utils.DTO` vs `Utils.DTOs`); no hacer renombres masivos sin que se pida.

### Fechas y zona horaria (importante)
- **En base de datos todo se guarda en UTC.** La zona del negocio es `America/Argentina/Buenos_Aires`.
- Usar siempre `Utils.Helpers.DateTimeHelper` (`NormalizarAUtc`, `LocalAUtc`, `UtcALocal`). No usar `DateTime.Now` ni convertir zonas a mano.

### Errores
- `ExceptionHandlingMiddleware` traduce las excepciones a `ApiErrorResponseDto` (`status`, `code`, `message`, `details`). No hacer `try/catch` en controllers para armar respuestas de error.

### Estilo general
- `Nullable` habilitado: no ignorar warnings de nulabilidad.
- Todo I/O con `async/await`. No usar `.Result` ni `.Wait()`.
- Nombres de clases, métodos y mensajes de usuario en español. Seguir el estilo del archivo vecino antes de inventar uno nuevo.

---

## Tests

- Framework: xUnit + FluentAssertions + NSubstitute; `UseInMemoryDatabase` con `Guid.NewGuid()` cuando se necesita contexto real.
- Nomenclatura: `Metodo_Escenario_ResultadoEsperado` (ej. `AddAsync_SinTurno_CreaInformeCorrectamente`).
- Estructura AAA con comentarios `// Arrange`, `// Act`, `// Assert`.
- Cada test es independiente e idempotente (sin estado compartido).
- Usar los helpers de `Service.Tests/TestSupport/` (`TestObjects`, `MapperStub`, `FakeTurnoRepository`) en vez de duplicar fixtures.
- Todo servicio nuevo o regla de negocio nueva **lleva tests** en la carpeta espejo (`Service.Tests/<Modulo>/`). Para middlewares y seeders, en `Api.Tests/`.
- No borrar ni "arreglar" un test para que pase sin entender por qué falla; si el test está mal, explicalo.

---

## Cómo trabajar (flujo esperado)

1. **Leer antes de escribir**: revisá el módulo vecino (controller, servicio, repo, DTOs, tests) y copiá su patrón.
2. **Alcance acotado**: tocá solo los archivos necesarios para la tarea. Si la tarea parece requerir cambios fuera de lo pedido (otras capas, otros módulos), frená y proponelo antes.
3. **Respetar capas**: cada cambio va en la capa que le corresponde (ver tabla de arriba).
4. **Mostrar el diff / listar los archivos modificados** antes de dar algo por cerrado.
5. **Verificar**: `dotnet build` y `dotnet test` (o el subconjunto relevante) y reportar el resultado real, incluyendo fallos.
6. **Git**: no hacer commit, push, merge ni cambiar de rama sin que se pida explícitamente. Mensajes de commit en español, descriptivos, en minúscula (ej. `validaciones y endpoints de informes`). Ramas por feature: `feature/<nombre>`, `fix/<nombre>`, `chore/<nombre>`.

---

## Prohibido / pedir confirmación primero

- Cambiar el rol o agregar roles nuevos, o abrir login para pacientes.
- Reintroducir el módulo `Asistencia`.
- Editar o borrar migraciones existentes, o correr `dotnet ef database update` contra una base que no sea la local de desarrollo.
- Borrado físico de pacientes, turnos o informes (hay baja lógica / restricciones por diseño).
- Agregar paquetes NuGet nuevos (explicar por qué hace falta antes).
- Tocar autenticación/JWT (`Service/Auth`, `Security/JwtProvider`) sin pedido explícito.
- Devolver entidades EF desde un controller o exponer el hash de contraseña / refresh token en cualquier DTO.

---

## Roadmap (contexto, no implementar sin pedido)

- En curso: informes por turno, reuniones, notificaciones/automatizaciones (alerta de asistencia pendiente, recordatorios a pacientes por WhatsApp, centro de notificaciones).
- Futuro: integración de facturación electrónica AFIP (WSAA/WSFE), una vez que el sistema esté completamente funcional para el profesional.
