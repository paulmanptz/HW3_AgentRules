CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'Auth') THEN
        CREATE SCHEMA "Auth";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'Auth') THEN
        CREATE SCHEMA "Auth";
    END IF;
END $EF$;

CREATE TYPE "Auth"."RoleType" AS ENUM ('admin', 'dispatcher', 'master');

CREATE TABLE "Auth"."Users" (
    "Id" uuid NOT NULL,
    "Login" text,
    "PasswordHash" text,
    "Phone" text,
    "FirstName" text,
    "LastName" text,
    "Patronymic" text,
    "IsActive" boolean NOT NULL,
    "Role" "Auth"."RoleType" NOT NULL,
    "DomokeyOrgId" integer,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "DeletedAt" timestamp with time zone,
    CONSTRAINT "PK_Users" PRIMARY KEY ("Id")
);

CREATE TABLE "Auth"."ActivationCodes" (
    "Id" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "Code" text NOT NULL,
    "ExpireAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_ActivationCodes" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_ActivationCodes_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Auth"."Users" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Auth"."Devices" (
    "Id" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "Token" text,
    "DeviceId" text NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "DeletedAt" timestamp with time zone,
    CONSTRAINT "PK_Devices" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Devices_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Auth"."Users" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Auth"."RefreshTokens" (
    "Id" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "DeviceId" uuid,
    "Token" text NOT NULL,
    "ExpireAt" timestamp with time zone NOT NULL,
    "IssuedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_RefreshTokens" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_RefreshTokens_Devices_DeviceId" FOREIGN KEY ("DeviceId") REFERENCES "Auth"."Devices" ("Id"),
    CONSTRAINT "FK_RefreshTokens_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Auth"."Users" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_ActivationCodes_UserId" ON "Auth"."ActivationCodes" ("UserId");

CREATE INDEX "IX_Devices_UserId" ON "Auth"."Devices" ("UserId");

CREATE INDEX "IX_RefreshTokens_DeviceId" ON "Auth"."RefreshTokens" ("DeviceId");

CREATE INDEX "IX_RefreshTokens_UserId" ON "Auth"."RefreshTokens" ("UserId");

CREATE UNIQUE INDEX "IX_Users_Login" ON "Auth"."Users" ("Login");

CREATE UNIQUE INDEX "IX_Users_Phone" ON "Auth"."Users" ("Phone");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260611085328_InitialAuth', '9.0.16');

COMMIT;

