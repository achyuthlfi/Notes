using Microsoft.EntityFrameworkCore;
using NotesPOC.Models;

namespace NotesPOC.Data
{
    public class NoteContext : DbContext
    {
        public NoteContext(DbContextOptions<NoteContext> options) : base(options) { }

        public DbSet<Note> Notes { get; set; }

        public DbSet<User> Users { get; set; } // Add this line
    }
}
