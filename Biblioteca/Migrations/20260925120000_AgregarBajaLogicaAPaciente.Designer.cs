using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Migrations
{
    partial class AgregarBajaLogicaAPaciente
    {
        private sealed class SnapshotAccessor : ApplicationDbContextModelSnapshot
        {
            public void BuildSnapshot(ModelBuilder modelBuilder)
            {
                BuildModel(modelBuilder);
            }
        }

        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            new SnapshotAccessor().BuildSnapshot(modelBuilder);
        }
    }
}
