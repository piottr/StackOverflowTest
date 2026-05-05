using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT * 
                    FROM sys.objects 
                    WHERE object_id = OBJECT_ID(N'[dbo].[Tags]') 
                    AND type = 'U'
                )
                BEGIN
                    CREATE TABLE [dbo].[Tags]
                    (
                        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        [Name] NVARCHAR(MAX) NOT NULL,
                        [Count] INT NOT NULL,
                        [Percentage] DECIMAL(18,6) NOT NULL
                    )
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tags");
        }
    }
}
