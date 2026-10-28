using System.Net;
using System.Text.Json;
using mediTool.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Utils.DTO.Api;
using Utils.Exceptions;

namespace Api.Tests.Middlewares;

public class ExceptionHandlingMiddlewareTests
{
    private static async Task<(DefaultHttpContext Context, ApiErrorResponseDto? ErrorResponse)> EjecutarMiddleware(
        RequestDelegate next,
        string environment = "Development")
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var env = Substitute.For<IHostEnvironment>();
        env.EnvironmentName.Returns(environment);

        var middleware = new ExceptionHandlingMiddleware(
            next,
            NullLogger<ExceptionHandlingMiddleware>.Instance,
            env);

        await middleware.InvokeAsync(context);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();

        ApiErrorResponseDto? responseDto = null;
        if (!string.IsNullOrWhiteSpace(body))
        {
            responseDto = JsonSerializer.Deserialize<ApiErrorResponseDto>(
                body,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        return (context, responseDto);
    }

    [Fact]
    public async Task InvokeAsync_SinExcepcion_EjecutaElSiguienteMiddlewareYNoAlteraRespuesta()
    {
        // Arrange
        var fueLlamado = false;
        RequestDelegate next = ctx =>
        {
            fueLlamado = true;
            ctx.Response.StatusCode = StatusCodes.Status200OK;
            return Task.CompletedTask;
        };

        // Act
        var (context, errorResponse) = await EjecutarMiddleware(next);

        // Assert
        fueLlamado.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        errorResponse.Should().BeNull();
    }

    // ----- Excepciones de Dominio / Aplicación (AppException) -----

    [Fact]
    public async Task InvokeAsync_NotFoundError_Devuelve404YFormatoCorrecto()
    {
        // Arrange
        RequestDelegate next = _ => throw new NotFoundError("Paciente no encontrado", new List<string> { "id: 99" });

        // Act
        var (context, error) = await EjecutarMiddleware(next);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        context.Response.ContentType.Should().Contain("application/json");
        error.Should().NotBeNull();
        error!.Status.Should().Be("error");
        error.Code.Should().Be(404);
        error.Message.Should().Be("Paciente no encontrado");
        error.Details.Should().ContainSingle().Which.Should().Be("id: 99");
    }

    [Fact]
    public async Task InvokeAsync_ConflictError_Devuelve409YFormatoCorrecto()
    {
        // Arrange
        RequestDelegate next = _ => throw new ConflictError("Ya existe un turno en ese horario");

        // Act
        var (context, error) = await EjecutarMiddleware(next);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.Conflict);
        error.Should().NotBeNull();
        error!.Code.Should().Be(409);
        error.Message.Should().Be("Ya existe un turno en ese horario");
    }

    [Fact]
    public async Task InvokeAsync_ValidationError_Devuelve400YFormatoCorrecto()
    {
        // Arrange
        RequestDelegate next = _ => throw new ValidationError("Datos de entrada inválidos", new List<string> { "La hora es requerida" });

        // Act
        var (context, error) = await EjecutarMiddleware(next);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        error.Should().NotBeNull();
        error!.Code.Should().Be(400);
        error.Message.Should().Be("Datos de entrada inválidos");
        error.Details.Should().Contain("La hora es requerida");
    }

    [Fact]
    public async Task InvokeAsync_UnauthorizedError_Devuelve401YFormatoCorrecto()
    {
        // Arrange
        RequestDelegate next = _ => throw new UnauthorizedError("Credenciales inválidas.");

        // Act
        var (context, error) = await EjecutarMiddleware(next);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.Unauthorized);
        error.Should().NotBeNull();
        error!.Code.Should().Be(401);
        error.Message.Should().Be("Credenciales inválidas.");
    }

    [Fact]
    public async Task InvokeAsync_ForbiddenError_Devuelve403YFormatoCorrecto()
    {
        // Arrange
        RequestDelegate next = _ => throw new ForbiddenError("No tiene permisos para realizar esta acción.");

        // Act
        var (context, error) = await EjecutarMiddleware(next);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.Forbidden);
        error.Should().NotBeNull();
        error!.Code.Should().Be(403);
        error.Message.Should().Be("No tiene permisos para realizar esta acción.");
    }

    // ----- Excepciones de Infraestructura y Framework -----

    [Fact]
    public async Task InvokeAsync_DbUpdateException_Devuelve409YMensajeDeConflictoDeRegistros()
    {
        // Arrange
        RequestDelegate next = _ => throw new DbUpdateException("violación de clave foránea");

        // Act
        var (context, error) = await EjecutarMiddleware(next);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.Conflict);
        error.Should().NotBeNull();
        error!.Code.Should().Be(409);
        error.Message.Should().Be("No se puede eliminar el recurso porque tiene registros asociados.");
        error.Details.Should().Contain("violación de clave foránea");
    }

    [Fact]
    public async Task InvokeAsync_KeyNotFoundException_Devuelve404()
    {
        // Arrange
        RequestDelegate next = _ => throw new KeyNotFoundException("Entidad no hallada");

        // Act
        var (context, error) = await EjecutarMiddleware(next);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        error.Should().NotBeNull();
        error!.Code.Should().Be(404);
        error.Message.Should().Be("El recurso solicitado no existe.");
    }

    [Theory]
    [InlineData(typeof(ArgumentException))]
    [InlineData(typeof(InvalidOperationException))]
    public async Task InvokeAsync_ArgumentOInvalidOperationException_Devuelve400(Type exceptionType)
    {
        // Arrange
        var ex = (Exception)Activator.CreateInstance(exceptionType, "Parámetro inválido")!;
        RequestDelegate next = _ => throw ex;

        // Act
        var (context, error) = await EjecutarMiddleware(next);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        error.Should().NotBeNull();
        error!.Code.Should().Be(400);
        error.Message.Should().Be("La solicitud no es válida.");
        error.Details.Should().Contain("Parámetro inválido");
    }

    [Fact]
    public async Task InvokeAsync_UnauthorizedAccessException_Devuelve401()
    {
        // Arrange
        RequestDelegate next = _ => throw new UnauthorizedAccessException("Acceso denegado");

        // Act
        var (context, error) = await EjecutarMiddleware(next);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.Unauthorized);
        error.Should().NotBeNull();
        error!.Code.Should().Be(401);
        error.Message.Should().Be("No está autorizado para acceder a este recurso.");
    }

    [Fact]
    public async Task InvokeAsync_DbUpdateException_EnProduccion_NoExponeDetallesTecnicos()
    {
        // Arrange
        RequestDelegate next = _ => throw new DbUpdateException("constraint \"fk_turno_paciente\" violada");

        // Act
        var (context, error) = await EjecutarMiddleware(next, environment: Environments.Production);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.Conflict);
        error.Should().NotBeNull();
        error!.Code.Should().Be(409);
        error.Message.Should().Be("No se puede eliminar el recurso porque tiene registros asociados.");
        error.Details.Should().BeEmpty();
    }

    [Fact]
    public async Task InvokeAsync_KeyNotFoundException_EnProduccion_NoExponeDetallesTecnicos()
    {
        // Arrange
        RequestDelegate next = _ => throw new KeyNotFoundException("Entidad no hallada en tabla turno");

        // Act
        var (context, error) = await EjecutarMiddleware(next, environment: Environments.Production);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        error.Should().NotBeNull();
        error!.Code.Should().Be(404);
        error.Message.Should().Be("El recurso solicitado no existe.");
        error.Details.Should().BeEmpty();
    }

    [Theory]
    [InlineData(typeof(ArgumentException))]
    [InlineData(typeof(InvalidOperationException))]
    public async Task InvokeAsync_ArgumentOInvalidOperationException_EnProduccion_NoExponeDetallesTecnicos(Type exceptionType)
    {
        // Arrange
        var ex = (Exception)Activator.CreateInstance(exceptionType, "Detalle interno del parámetro")!;
        RequestDelegate next = _ => throw ex;

        // Act
        var (context, error) = await EjecutarMiddleware(next, environment: Environments.Production);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        error.Should().NotBeNull();
        error!.Code.Should().Be(400);
        error.Message.Should().Be("La solicitud no es válida.");
        error.Details.Should().BeEmpty();
    }

    [Fact]
    public async Task InvokeAsync_UnauthorizedAccessException_EnProduccion_NoExponeDetallesTecnicos()
    {
        // Arrange
        RequestDelegate next = _ => throw new UnauthorizedAccessException("Acceso denegado al recurso interno");

        // Act
        var (context, error) = await EjecutarMiddleware(next, environment: Environments.Production);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.Unauthorized);
        error.Should().NotBeNull();
        error!.Code.Should().Be(401);
        error.Message.Should().Be("No está autorizado para acceder a este recurso.");
        error.Details.Should().BeEmpty();
    }

    [Fact]
    public async Task InvokeAsync_AppException_EnProduccion_MantieneDetallesDeValidacion()
    {
        // Arrange
        RequestDelegate next = _ => throw new ValidationError("Datos de entrada inválidos", new List<string> { "La hora es requerida" });

        // Act
        var (context, error) = await EjecutarMiddleware(next, environment: Environments.Production);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        error.Should().NotBeNull();
        error!.Code.Should().Be(400);
        error.Message.Should().Be("Datos de entrada inválidos");
        error.Details.Should().Contain("La hora es requerida");
    }

    // ----- Diferenciación por Entorno (Development vs Production) -----

    [Fact]
    public async Task InvokeAsync_InternalError_EnDesarrollo_IncluyeDetallesTecnicos()
    {
        // Arrange
        RequestDelegate next = _ => throw new InternalError("Error de base de datos", new List<string> { "timeout a las 12:00" });

        // Act
        var (context, error) = await EjecutarMiddleware(next, environment: Environments.Development);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
        error.Should().NotBeNull();
        error!.Code.Should().Be(500);
        error.Message.Should().Be("Error interno del servidor.");
        error.Details.Should().Contain("Error de base de datos");
        error.Details.Should().Contain("timeout a las 12:00");
    }

    [Fact]
    public async Task InvokeAsync_InternalError_EnProduccion_NoExponeDetallesTecnicos()
    {
        // Arrange
        RequestDelegate next = _ => throw new InternalError("Error interno sensible", new List<string> { "string de conexion expuesto" });

        // Act
        var (context, error) = await EjecutarMiddleware(next, environment: Environments.Production);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
        error.Should().NotBeNull();
        error!.Code.Should().Be(500);
        error.Message.Should().Be("Error interno del servidor.");
        error.Details.Should().BeEmpty();
    }

    [Fact]
    public async Task InvokeAsync_ExcepcionNoManejada_EnDesarrollo_IncluyeMensajeYStackTrace()
    {
        // Arrange
        RequestDelegate next = _ => throw new NullReferenceException("Referencia nula inesperada");

        // Act
        var (context, error) = await EjecutarMiddleware(next, environment: Environments.Development);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
        error.Should().NotBeNull();
        error!.Code.Should().Be(500);
        error.Message.Should().Be("Ocurrió un error inesperado en el servidor.");
        error.Details.Should().Contain(d => d.Contains("Referencia nula inesperada"));
    }

    [Fact]
    public async Task InvokeAsync_ExcepcionNoManejada_EnProduccion_OcultaDetallesTecnicos()
    {
        // Arrange
        RequestDelegate next = _ => throw new NullReferenceException("Referencia nula inesperada");

        // Act
        var (context, error) = await EjecutarMiddleware(next, environment: Environments.Production);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
        error.Should().NotBeNull();
        error!.Code.Should().Be(500);
        error.Message.Should().Be("Ocurrió un error inesperado en el servidor.");
        error.Details.Should().BeEmpty();
    }
}
