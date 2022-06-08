using PriceGas.Server.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PriceGas.Shared.Entidades;
using PriceGas.Shared.Entidades.Cursos;

namespace PriceGas.Server.Datos
{
    //esta clase nos permite comunicarnos con la base de datos y ademas construir las tablas 
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {

        }

        //sobreescribimos el metodo SaveChangesAsync, para que acepte el id del usuario y mas modificaciones custom
        public virtual async Task<int> SaveChangesAsync(string userId = null)
        {
            OnBeforeSaveChanges(userId);//le pasamos el nuevo metodo creado
            var result = await base.SaveChangesAsync();
            return result;
        }
        private void OnBeforeSaveChanges(string userId)
        {
            ChangeTracker.DetectChanges();//analiza las entidades en busca de cambios. el changetracker es un seguimiento de cambios y el metodo los detecta

            var auditEntries = new List<AuditEntry>();

            //recorre la colección de todas las Entidades modificadas,en nuestro caso, el ciclo siempre tendrá UNA sola iteración
            foreach (var item in ChangeTracker.Entries())
            {
                if (item.Entity is Auditoria || item.State == EntityState.Detached || item.State == EntityState.Unchanged)
                    continue;

                var auditEntry = new AuditEntry(item);

                auditEntry.TableName = item.Entity.GetType().Name;//obtiene el nombre de la tabla
                auditEntry.UserId = userId;//obtiene el id del usuario
                auditEntries.Add(auditEntry);//guarda la propiedades de cada entidad en la lista de entidades

                foreach (var property in item.Properties)
                {
                    string propertyName = property.Metadata.Name;

                    //si la propiedad actual es una clave principal, agréguela al diccionario de claves principales y sáltela.
                    if (property.Metadata.IsPrimaryKey())
                    {
                        auditEntry.KeyValues[propertyName] = property.CurrentValue;
                        continue;
                    }

                    //en el switch detectamos el estado de la entidad (Agregado, Eliminado o Modificado)
                    //y por cada caso agregamos nuevos datos a cada campo de la tabla auditoria
                    switch (item.State)
                    {
                        case EntityState.Added:
                            auditEntry.AuditType = Shared.Enums.AuditType.Create;
                            auditEntry.NewValues[propertyName] = property.CurrentValue;
                            break;
                        case EntityState.Deleted:
                            auditEntry.AuditType = Shared.Enums.AuditType.Delete;
                            auditEntry.OldValues[propertyName] = property.OriginalValue;
                            break;
                        case EntityState.Modified:
                            if (property.IsModified)
                            {
                                auditEntry.ChangedColumns.Add(propertyName);
                                auditEntry.AuditType = Shared.Enums.AuditType.Update;
                                auditEntry.OldValues[propertyName] = property.OriginalValue;
                                auditEntry.NewValues[propertyName] = property.CurrentValue;
                            }
                            break;
                    }
                }
            }
            //convertimos todas las Entradas de Auditoría a Auditorías y guardamos los cambios en el metodo original: var result = await base.SaveChangesAsync();
            foreach (var auditEntry in auditEntries)
            {
                Auditorias.Add(auditEntry.ToAudit());
            }
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //creamos el modelo entity, creamos un objeto anonimo con new y le pasamos el haskey para crear una llave compuesta con categoriaid y equipoid        
            //modelBuilder.Entity<ZonaEstado>().HasKey(x => new { x.ZonaId, x.EstadoId });

            //aqui cremos los roles de tipo identityrole
            var roleAdmin = new IdentityRole()
            //creamos las propiedades id y nombre en este caso admin y utilizamos un id de tipo guid
            { Id = "54202d5b-deb1-417d-a05e-5b2d8fe48e4d", Name = "Administrador", NormalizedName = "Administrador" };
            //creamos el rolutilizando hasdata y le pasamos la variable de arriba
            modelBuilder.Entity<IdentityRole>().HasData(roleAdmin);

            var Usuario = new IdentityRole()
            { Id = "06a0e103-5645-40dd-b94c-044d6573821c", Name = "Usuario", NormalizedName = "Usuario" };
            modelBuilder.Entity<IdentityRole>().HasData(Usuario);         

            //aqui es donde hacemos la configuracion de nuestra llave compuesta
            base.OnModelCreating(modelBuilder);
        }

        //creamos una propiedad dbset la cual dice a partir de que modelo queremos crear la bd

        //cursos
        public DbSet<Curso> Cursos { get; set; }
        public DbSet<Tema> Temas { get; set; }
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<Pregunta> Preguntas { get; set; }
        public DbSet<Respuesta> Respuestas { get; set; }
        public DbSet<Carrusel> Carrusel { get; set; }

        //modelo de auditoria       
        public DbSet<Auditoria> Auditorias { get; set; }
        public DbSet<ArchivoAdjunto> ArchivoAdjuntos { get; set; }

        //public DbSet<Logs> Logs { get; set; }
    }
}
