START TRANSACTION;
ALTER TABLE "Users" ADD "EmailVerified" boolean NOT NULL DEFAULT FALSE;

ALTER TABLE "Users" ADD "HasCompletedOnboarding" boolean NOT NULL DEFAULT FALSE;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20251213215350_UpdateUser', '10.0.0');

COMMIT;


