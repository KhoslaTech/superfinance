using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ASKSource.Migrations
{
    public partial class BankModels : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserType",
                table: "User",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Bank",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(maxLength: 60, nullable: false),
                    Address = table.Column<string>(nullable: true),
                    FirewallEnabled = table.Column<bool>(nullable: false),
                    EnforceMFA = table.Column<bool>(nullable: false),
                    SkipMFAInsideNetwork = table.Column<bool>(nullable: false),
                    PasswordExpiresInDays = table.Column<int>(nullable: true),
                    OwningUserId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bank", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bank_User_OwningUserId",
                        column: x => x.OwningUserId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AccountType",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(maxLength: 30, nullable: false),
                    InterestRate = table.Column<double>(nullable: false),
                    Kind = table.Column<int>(nullable: false),
                    BankId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountType", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountType_Bank_BankId",
                        column: x => x.BankId,
                        principalTable: "Bank",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Branch",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(maxLength: 60, nullable: false),
                    Code = table.Column<string>(maxLength: 16, nullable: false),
                    Address = table.Column<string>(nullable: true),
                    BankId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Branch", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Branch_Bank_BankId",
                        column: x => x.BankId,
                        principalTable: "Bank",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Account",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Number = table.Column<string>(maxLength: 30, nullable: false),
                    IdentityNumber = table.Column<string>(maxLength: 24, nullable: false),
                    Status = table.Column<int>(nullable: false),
                    Reason = table.Column<string>(nullable: true),
                    AccountTypeId = table.Column<Guid>(nullable: false),
                    BranchId = table.Column<Guid>(nullable: false),
                    OwningUserId = table.Column<Guid>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Account", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Account_AccountType_AccountTypeId",
                        column: x => x.AccountTypeId,
                        principalTable: "AccountType",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Account_Branch_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branch",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Account_User_OwningUserId",
                        column: x => x.OwningUserId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AccountNominee",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    AccountId = table.Column<Guid>(nullable: false),
                    NomineeUserId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountNominee", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountNominee_Account_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Account",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AccountNominee_User_NomineeUserId",
                        column: x => x.NomineeUserId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Transaction",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    Amount = table.Column<double>(nullable: false),
                    TransactionType = table.Column<int>(nullable: false),
                    Remarks = table.Column<string>(nullable: true),
                    AccountId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transaction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transaction_Account_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Account",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserInvitation",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    EmailAddress = table.Column<string>(maxLength: 100, nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    AccountId = table.Column<Guid>(nullable: false),
                    UserId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserInvitation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserInvitation_Account_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Account",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserInvitation_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Transfer",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    DebitTransactionId = table.Column<Guid>(nullable: false),
                    CreditTransactionId = table.Column<Guid>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transfer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transfer_Transaction_CreditTransactionId",
                        column: x => x.CreditTransactionId,
                        principalTable: "Transaction",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Transfer_Transaction_DebitTransactionId",
                        column: x => x.DebitTransactionId,
                        principalTable: "Transaction",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Account_AccountTypeId",
                table: "Account",
                column: "AccountTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Account_BranchId",
                table: "Account",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Account_OwningUserId",
                table: "Account",
                column: "OwningUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountNominee_AccountId",
                table: "AccountNominee",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountNominee_NomineeUserId",
                table: "AccountNominee",
                column: "NomineeUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountType_BankId",
                table: "AccountType",
                column: "BankId");

            migrationBuilder.CreateIndex(
                name: "IX_Bank_Name",
                table: "Bank",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bank_OwningUserId",
                table: "Bank",
                column: "OwningUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Branch_BankId",
                table: "Branch",
                column: "BankId");

            migrationBuilder.CreateIndex(
                name: "IX_Branch_Code",
                table: "Branch",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_AccountId",
                table: "Transaction",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Transfer_CreditTransactionId",
                table: "Transfer",
                column: "CreditTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_Transfer_DebitTransactionId",
                table: "Transfer",
                column: "DebitTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserInvitation_AccountId",
                table: "UserInvitation",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_UserInvitation_UserId",
                table: "UserInvitation",
                column: "UserId");

            InsertActivityPermissions(migrationBuilder);
            InsertRolePermissions(migrationBuilder);
            InsertImpliedPermissions(migrationBuilder);
            InsertSuspensionRules(migrationBuilder);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountNominee");

            migrationBuilder.DropTable(
                name: "Transfer");

            migrationBuilder.DropTable(
                name: "UserInvitation");

            migrationBuilder.DropTable(
                name: "Transaction");

            migrationBuilder.DropTable(
                name: "Account");

            migrationBuilder.DropTable(
                name: "AccountType");

            migrationBuilder.DropTable(
                name: "Branch");

            migrationBuilder.DropTable(
                name: "Bank");

            migrationBuilder.DropColumn(
                name: "UserType",
                table: "User");
        }

        private void InsertActivityPermissions(MigrationBuilder migrationBuilder)
        {
            var permissions = new[]
            {
                "('ManageBank', 'BANK', 'Manage bank', 1)", // instance permission
				"('ManageBranch', 'BANK', 'Manage branches', 1)", // instance permission
				"('ManageAccountType', 'BANK', 'Manage account types', 1)", // instance permission

				"('IndexCustomerAccount', 'ACCOUNT', 'View account', 0)", // general permission
				"('CreateCustomerAccount', 'ACCOUNT', 'Add new accounts', 1)", // instance permission
				"('ChangeStatus', 'ACCOUNT', 'edit account', 1)", // instance permission
				"('CreateDeposit', 'ACCOUNT', 'Create deposit', 1)", // instance permission
				"('CreateWithdrawal', 'ACCOUNT', 'Create withdrawal', 1)", // instance permission

				"('OpenAccount', 'ACCOUNT', 'Add new accounts', 0)", // general permission
				"('IndexAccount', 'ACCOUNT', 'View account', 0)", // general permission
				"('CreateTransfer', 'ACCOUNT', 'Create transfer', 1)", // instance permission
				"('AddNominee', 'ACCOUNT', 'Create new nominee', 1)", // instance permission
				"('DeleteNominee', 'ACCOUNT', 'Delete nominee', 1)", // instance permission
				"('IndexNominee', 'ACCOUNT', 'Create new nominee', 1)", // instance permission

				"('IndexAccountDetails', 'ACCOUNT', 'View account details', 1)", // general permission
			};

            migrationBuilder.Sql(@"insert into [dbo].[Permission]
				(PermissionCode, EntityTypeCode, Description, Kind)
				values" + string.Join(",\r\n", permissions));
        }

        private void InsertRolePermissions(MigrationBuilder migrationBuilder)
        {
            var permissions = new[]
            {
                "('BankOwner', 'BANK', 'Bank admin permission', 1)", // instance permission
				"('BranchManager', 'BRANCH', 'Branch admin permission', 1)", // instance permission
				"('BranchStaff', 'BRANCH', 'Branch staff permission', 1)", // instance permission
				"('AccountHolder', 'ACCOUNT', 'Account holder permission', 1)", // instance permission
				"('AccountNominee', 'ACCOUNT', 'Account nominee permission', 1)", // instance permission
				"('Customer', 'USER', 'Customer permission', 0)", // instance permission
            };

            migrationBuilder.Sql(@"insert into [dbo].[Permission]
				(PermissionCode, EntityTypeCode, Description, Kind)
				values" + string.Join(",\r\n", permissions));
        }

        private void InsertImpliedPermissions(MigrationBuilder migrationBuilder)
        {
            var impliedPermissions = new[]
            {
                "('BankOwner', 'BranchManager')",
                "('BankOwner', 'ManageBranch')",
                "('BankOwner', 'ManageAccountType')",
                "('BankOwner', 'ManageFirewall')",
                "('BankOwner', 'ManageBank')",

                "('BranchManager', 'BranchStaff')",

                "('BranchStaff', 'IndexCustomerAccount')",
                "('BranchStaff', 'CreateCustomerAccount')",
                "('BranchStaff', 'ChangeStatus')",
                "('BranchStaff', 'CreateDeposit')",
                "('BranchStaff', 'CreateWithdrawal')",
                "('BranchStaff', 'IndexAccountDetails')",
                "('BranchStaff', 'IndexNominee')",

                "('AccountHolder', 'OpenAccount')",
                "('AccountHolder', 'IndexAccount')",
                "('AccountHolder', 'CreateTransfer')",
                "('AccountHolder', 'AddNominee')",
                "('AccountHolder', 'DeleteNominee')",
                "('AccountHolder', 'IndexNominee')",
                "('AccountHolder', 'IndexAccountDetails')",

                "('AccountNominee', 'IndexAccount')",
                "('AccountNominee', 'IndexAccountDetails')",

                "('Customer', 'OpenAccount')"
            };

            migrationBuilder.Sql(@"insert into [dbo].[ImpliedPermission]
				(PermissionCode, ImpliedPermissionCode)
				values" + string.Join(",\r\n", impliedPermissions));
        }

        private void InsertSuspensionRules(MigrationBuilder migrationBuilder)
        {
	        migrationBuilder.Sql(@"insert into SuspensionExclusionRule
				(Id, EntityTypePattern, SuspensionTypePattern, VerbPattern, OperationPattern, PossessesAnyOfThePermissions)
				 values
				 --Can view = all get operations permitted on suspended entities regardless of reason/user role. But not MVC view retrieval actions which are really the post ones.
				(newid(), 'Account', '.*', 'GET', '^(?:(?<!ChangeStatus|Deposit|Withdrawal|Transfer).)*$', null),
				 --list* operations are also get only but called with post verb by jtable etc.
				(newid(), 'Account', '.*', 'POST', 'List.*', null),
				 --deposit allowed to all on suspended entity regardless of reason/user role (except on close accounts). Note - only staff is allowed but that's handled by permit authorization (ADA).
				(newid(), 'Account', '^(?:(?<!close).)*$', '.*', 'Deposit', null),
				--transfer/withdrawal is not allowed on pendingApproval. so allowing only deposit.
		        (newid(), 'Account', 'PendingApproval', '.*', 'Deposit', null),
				 --changeStatus allowed to owner on any status other than close.
				(newid(), 'Account', '^(?:(?<!close).)*$', '.*', 'ChangeStatus', 'BankOwner'),
				 --changeStatus allowed to manager on dormant/pendingApproval.
				(newid(), 'Account', 'Dormant|PendingApproval', '.*', 'ChangeStatus', 'BranchManager'),
				 --changeStatus allowed to manager/staff on KYC required.
				(newid(), 'Account', 'KYCRequired', '.*', 'ChangeStatus', 'BranchManager|BranchStaff')");
        }
    }
}
