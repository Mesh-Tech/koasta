ALTER TABLE "Employee" ADD COLUMN confirmed BOOL NOT NULL DEFAULT FALSE;

UPDATE "Employee" SET confirmed = TRUE;
