START TRANSACTION;
ALTER TABLE "Businesses" ADD "Description" text;

ALTER TABLE "Businesses" ADD "UpdatedAt" timestamp with time zone NOT NULL DEFAULT TIMESTAMPTZ '-infinity';

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20251227202035_UpdateBusinessAndBusinessMemberWithRoleEnum', '10.0.0');

COMMIT;


