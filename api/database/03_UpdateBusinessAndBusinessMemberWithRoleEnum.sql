START TRANSACTION;
ALTER TABLE "CashbookMembers" ALTER COLUMN "Role" TYPE integer;

ALTER TABLE "BusinessMembers" ALTER COLUMN "Role" TYPE integer;

ALTER TABLE "Businesses" ADD "Description" text;

ALTER TABLE "Businesses" ADD "UpdatedAt" timestamp with time zone NOT NULL DEFAULT TIMESTAMPTZ '-infinity';

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20251227200722_UpdateBusinessAndBusinessMemberWithRoleEnum', '10.0.0');

COMMIT;


