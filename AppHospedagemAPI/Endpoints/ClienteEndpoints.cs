using AppHospedagemAPI.Models;
using AppHospedagemAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace AppHospedagemAPI.Endpoints
{
    public static class ClienteEndpoints
    {
        public static void MapClienteEndpoints(this WebApplication app)
        {
            // ➕ Criar um novo cliente
            app.MapPost("/clientes", async (Cliente cliente, AppDbContext db) =>
            {
                db.Clientes.Add(cliente);
                await db.SaveChangesAsync();
                return Results.Created($"/clientes/{cliente.Id}", cliente);
            });

            // 📋 Listar todos os clientes
            app.MapGet("/clientes", async (AppDbContext db) =>
            {
                var clientes = await db.Clientes.ToListAsync();
                return Results.Ok(clientes);
            });

            // ✏️ Atualizar um cliente existente
            app.MapPut("/clientes/{id}", async (int id, Cliente clienteAtualizado, AppDbContext db) =>
            {
                var cliente = await db.Clientes.FindAsync(id);

                if (cliente is null)
                    return Results.NotFound("Cliente não encontrado");

                cliente.Nome = clienteAtualizado.Nome;
                cliente.Documento = clienteAtualizado.Documento;
                cliente.Telefone = clienteAtualizado.Telefone;

                await db.SaveChangesAsync();
                return Results.Ok(cliente);
            });

            // ❌ Excluir um cliente
            app.MapDelete("/clientes/{id}", async (int id, AppDbContext db) =>
            {
                var cliente = await db.Clientes.FindAsync(id);

                if (cliente is null)
                    return Results.NotFound("Cliente não encontrado");

                db.Clientes.Remove(cliente);
                await db.SaveChangesAsync();
                return Results.NoContent();
            });
        }
    }
}
