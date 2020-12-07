ALTER TABLE "Employee" ADD COLUMN securityStamp VARCHAR(8000) NULL;
UPDATE "Employee" SET securitystamp = 'a';

