DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'Files') THEN
        CREATE SCHEMA "Files";
    END IF;
END $EF$;
CREATE TABLE IF NOT EXISTS "Files"."__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'Files') THEN
        CREATE SCHEMA "Files";
    END IF;
END $EF$;

CREATE TABLE "Files"."Files" (
    "Id" uuid NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "FileName" text NOT NULL DEFAULT '',
    CONSTRAINT "PK_Files" PRIMARY KEY ("Id")
);

INSERT INTO "Files"."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260611085349_InitialFiles', '9.0.16');

COMMIT;

