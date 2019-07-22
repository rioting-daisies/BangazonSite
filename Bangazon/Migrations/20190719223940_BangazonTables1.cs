using Microsoft.EntityFrameworkCore.Migrations;

namespace Bangazon.Migrations
{
    public partial class BangazonTables1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "PaymentType",
                keyColumn: "PaymentTypeId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "PaymentType",
                keyColumn: "PaymentTypeId",
                keyValue: 2);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "PaymentType",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "00000000-ffff-ffff-ffff-ffffffffffff",
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "c16a9ee3-e88e-4180-99f9-21f89827aa8a", "AQAAAAEAACcQAAAAEKMWzeUDlF9Jb1usVW29zbLlw6N7xlh1gUl8jScxk9vIPGZib+dc+Awb/BqJy6OGhA==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "00000001-ffff-ffff-ffff-ffffffffffff",
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "02bef034-d531-45a6-a458-101520cff619", "AQAAAAEAACcQAAAAEJfBWjgno3ovwCpi8Yims5mVoGpBTTP2p9pvgrPBb+sC+2BrUBUJn+L+u2BKUG8m/g==" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "PaymentType");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "00000000-ffff-ffff-ffff-ffffffffffff",
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "b20e338b-b4f7-4506-9d51-cca1b8f37703", "AQAAAAEAACcQAAAAEMR3B4LxO2VgGw+hpC0h7F9uim2a/9oNoDPRmkbWM5tbY4Hngl/twlRa6UItoUmcjg==" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "00000001-ffff-ffff-ffff-ffffffffffff",
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "00d78c96-8839-4e1f-947b-deaae130e52a", "AQAAAAEAACcQAAAAEGuo8vqtX+oihPnhtVsPLIndPb2ecdehmvnjKhrmO4xMI2UesiPbbJyRrotQxGvqJw==" });

            migrationBuilder.InsertData(
                table: "PaymentType",
                columns: new[] { "PaymentTypeId", "AccountNumber", "Description", "UserId" },
                values: new object[] { 1, "86753095551212", "American Express", "00000000-ffff-ffff-ffff-ffffffffffff" });

            migrationBuilder.InsertData(
                table: "PaymentType",
                columns: new[] { "PaymentTypeId", "AccountNumber", "Description", "UserId" },
                values: new object[] { 2, "4102948572991", "Discover", "00000000-ffff-ffff-ffff-ffffffffffff" });
        }
    }
}
