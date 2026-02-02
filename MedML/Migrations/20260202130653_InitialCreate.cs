using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MedML.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "heart_disease_records",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    age = table.Column<int>(type: "integer", nullable: false),
                    sex = table.Column<string>(type: "text", nullable: false),
                    chest_pain_type = table.Column<string>(type: "text", nullable: false),
                    resting_bp = table.Column<int>(type: "integer", nullable: false),
                    cholesterol = table.Column<int>(type: "integer", nullable: false),
                    fasting_bs = table.Column<int>(type: "integer", nullable: false),
                    resting_ecg = table.Column<string>(type: "text", nullable: false),
                    max_hr = table.Column<int>(type: "integer", nullable: false),
                    exercise_angina = table.Column<string>(type: "text", nullable: false),
                    oldpeak = table.Column<float>(type: "real", nullable: false),
                    st_slope = table.Column<string>(type: "text", nullable: false),
                    heart_disease = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_heart_disease_records", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "heart_disease_records");
        }
    }
}
