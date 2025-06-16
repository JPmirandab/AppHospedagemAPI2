using AppHospedagemAPI.Models;
using AppHospedagemAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace AppHospedagemAPI.Endpoints
{
    public static class UsuarioEndpoints
    {
        public static void MapUsuarioEndpoints(this WebApplication app)
        {
            // 🟢 Cadastrar novo usuário
            app.MapPost("/usuarios/cadastrar", async (Usuario usuario, AppDbContext db) =>
            {
                var loginExistente = await db.Usuarios.AnyAsync(u => u.Login == usuario.Login);
                if (loginExistente)
                    return Results.BadRequest("Login já existe.");

                db.Usuarios.Add(usuario);
                await db.SaveChangesAsync();
                return Results.Created($"/usuarios/{usuario.Id}", usuario);
            });

            // 🟡 Login de usuário
            app.MapPost("/usuarios/login", async (Usuario usuarioLogin, AppDbContext db) =>
            {
                var usuario = await db.Usuarios
                    .FirstOrDefaultAsync(u => u.Login == usuarioLogin.Login && u.Senha == usuarioLogin.Senha);

                if (usuario == null)
                    return Results.Unauthorized();

                return Results.Ok("Login realizado com sucesso.");
            });
        }
    }
}
