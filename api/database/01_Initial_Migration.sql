CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;
CREATE TABLE "Users" (
    "Id" uuid NOT NULL,
    "GoogleUserId" text NOT NULL,
    "Email" text NOT NULL,
    "Name" text NOT NULL,
    "PhotoUrl" text,
    "CreatedAt" timestamp with time zone NOT NULL,
    "LastLoginAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Users" PRIMARY KEY ("Id")
);

CREATE TABLE "ActivityLogs" (
    "Id" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "Action" text NOT NULL,
    "BusinessId" uuid,
    "CashbookId" uuid,
    "EntityId" uuid,
    "OccurredAt" timestamp with time zone NOT NULL,
    "MetadataJson" jsonb,
    CONSTRAINT "PK_ActivityLogs" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_ActivityLogs_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Businesses" (
    "Id" uuid NOT NULL,
    "Name" text NOT NULL,
    "CreatedByUserId" uuid NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Businesses" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Businesses_Users_CreatedByUserId" FOREIGN KEY ("CreatedByUserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE TABLE "BusinessMembers" (
    "Id" uuid NOT NULL,
    "BusinessId" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "Role" text NOT NULL,
    CONSTRAINT "PK_BusinessMembers" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_BusinessMembers_Businesses_BusinessId" FOREIGN KEY ("BusinessId") REFERENCES "Businesses" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_BusinessMembers_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Cashbooks" (
    "Id" uuid NOT NULL,
    "BusinessId" uuid NOT NULL,
    "Name" text NOT NULL,
    "Currency" character varying(10) NOT NULL,
    "CreatedByUserId" uuid NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Cashbooks" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Cashbooks_Businesses_BusinessId" FOREIGN KEY ("BusinessId") REFERENCES "Businesses" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Cashbooks_Users_CreatedByUserId" FOREIGN KEY ("CreatedByUserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Categories" (
    "Id" uuid NOT NULL,
    "BusinessId" uuid NOT NULL,
    "Name" text NOT NULL,
    "Type" text NOT NULL,
    "CreatedByUserId" uuid NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Categories" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Categories_Businesses_BusinessId" FOREIGN KEY ("BusinessId") REFERENCES "Businesses" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Categories_Users_CreatedByUserId" FOREIGN KEY ("CreatedByUserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Contacts" (
    "Id" uuid NOT NULL,
    "BusinessId" uuid NOT NULL,
    "Name" text NOT NULL,
    "Type" text NOT NULL,
    "Phone" text,
    "Email" text,
    "CreatedByUserId" uuid NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Contacts" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Contacts_Businesses_BusinessId" FOREIGN KEY ("BusinessId") REFERENCES "Businesses" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Contacts_Users_CreatedByUserId" FOREIGN KEY ("CreatedByUserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE TABLE "PaymentMethods" (
    "Id" uuid NOT NULL,
    "BusinessId" uuid NOT NULL,
    "Name" text NOT NULL,
    "CreatedByUserId" uuid NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_PaymentMethods" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_PaymentMethods_Businesses_BusinessId" FOREIGN KEY ("BusinessId") REFERENCES "Businesses" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_PaymentMethods_Users_CreatedByUserId" FOREIGN KEY ("CreatedByUserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE TABLE "CashbookMembers" (
    "Id" uuid NOT NULL,
    "CashbookId" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "Role" text NOT NULL,
    CONSTRAINT "PK_CashbookMembers" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_CashbookMembers_Cashbooks_CashbookId" FOREIGN KEY ("CashbookId") REFERENCES "Cashbooks" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_CashbookMembers_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Transactions" (
    "Id" uuid NOT NULL,
    "BusinessId" uuid NOT NULL,
    "CashbookId" uuid NOT NULL,
    "Amount" numeric NOT NULL,
    "Type" text NOT NULL,
    "CategoryId" uuid,
    "ContactId" uuid,
    "PaymentMethodId" uuid,
    "Description" text,
    "TransactionDate" timestamp with time zone NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    "CreatedByUserId" uuid NOT NULL,
    "UpdatedByUserId" uuid NOT NULL,
    "Version" integer NOT NULL,
    CONSTRAINT "PK_Transactions" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Transactions_Businesses_BusinessId" FOREIGN KEY ("BusinessId") REFERENCES "Businesses" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Transactions_Cashbooks_CashbookId" FOREIGN KEY ("CashbookId") REFERENCES "Cashbooks" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Transactions_Categories_CategoryId" FOREIGN KEY ("CategoryId") REFERENCES "Categories" ("Id"),
    CONSTRAINT "FK_Transactions_Contacts_ContactId" FOREIGN KEY ("ContactId") REFERENCES "Contacts" ("Id"),
    CONSTRAINT "FK_Transactions_PaymentMethods_PaymentMethodId" FOREIGN KEY ("PaymentMethodId") REFERENCES "PaymentMethods" ("Id"),
    CONSTRAINT "FK_Transactions_Users_CreatedByUserId" FOREIGN KEY ("CreatedByUserId") REFERENCES "Users" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Transactions_Users_UpdatedByUserId" FOREIGN KEY ("UpdatedByUserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE TABLE "TransactionChanges" (
    "Id" uuid NOT NULL,
    "TransactionId" uuid NOT NULL,
    "ChangedByUserId" uuid NOT NULL,
    "ChangedAt" timestamp with time zone NOT NULL,
    "ChangeType" text NOT NULL,
    "ChangesJson" jsonb NOT NULL,
    CONSTRAINT "PK_TransactionChanges" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_TransactionChanges_Transactions_TransactionId" FOREIGN KEY ("TransactionId") REFERENCES "Transactions" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_TransactionChanges_Users_ChangedByUserId" FOREIGN KEY ("ChangedByUserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_ActivityLogs_UserId" ON "ActivityLogs" ("UserId");

CREATE INDEX "IX_Businesses_CreatedByUserId" ON "Businesses" ("CreatedByUserId");

CREATE UNIQUE INDEX "IX_BusinessMembers_BusinessId_UserId" ON "BusinessMembers" ("BusinessId", "UserId");

CREATE INDEX "IX_BusinessMembers_UserId" ON "BusinessMembers" ("UserId");

CREATE UNIQUE INDEX "IX_CashbookMembers_CashbookId_UserId" ON "CashbookMembers" ("CashbookId", "UserId");

CREATE INDEX "IX_CashbookMembers_UserId" ON "CashbookMembers" ("UserId");

CREATE INDEX "IX_Cashbooks_BusinessId" ON "Cashbooks" ("BusinessId");

CREATE INDEX "IX_Cashbooks_CreatedByUserId" ON "Cashbooks" ("CreatedByUserId");

CREATE INDEX "IX_Categories_BusinessId" ON "Categories" ("BusinessId");

CREATE INDEX "IX_Categories_CreatedByUserId" ON "Categories" ("CreatedByUserId");

CREATE INDEX "IX_Contacts_BusinessId" ON "Contacts" ("BusinessId");

CREATE INDEX "IX_Contacts_CreatedByUserId" ON "Contacts" ("CreatedByUserId");

CREATE INDEX "IX_PaymentMethods_BusinessId" ON "PaymentMethods" ("BusinessId");

CREATE INDEX "IX_PaymentMethods_CreatedByUserId" ON "PaymentMethods" ("CreatedByUserId");

CREATE INDEX "IX_TransactionChanges_ChangedByUserId" ON "TransactionChanges" ("ChangedByUserId");

CREATE INDEX "IX_TransactionChanges_TransactionId" ON "TransactionChanges" ("TransactionId");

CREATE INDEX "IX_Transactions_BusinessId_CashbookId_TransactionDate" ON "Transactions" ("BusinessId", "CashbookId", "TransactionDate");

CREATE INDEX "IX_Transactions_CashbookId" ON "Transactions" ("CashbookId");

CREATE INDEX "IX_Transactions_CategoryId" ON "Transactions" ("CategoryId");

CREATE INDEX "IX_Transactions_ContactId" ON "Transactions" ("ContactId");

CREATE INDEX "IX_Transactions_CreatedByUserId" ON "Transactions" ("CreatedByUserId");

CREATE INDEX "IX_Transactions_PaymentMethodId" ON "Transactions" ("PaymentMethodId");

CREATE INDEX "IX_Transactions_UpdatedByUserId" ON "Transactions" ("UpdatedByUserId");

CREATE UNIQUE INDEX "IX_Users_Email" ON "Users" ("Email");

CREATE UNIQUE INDEX "IX_Users_GoogleUserId" ON "Users" ("GoogleUserId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20251129072423_InitialCreate', '10.0.0');

COMMIT;


