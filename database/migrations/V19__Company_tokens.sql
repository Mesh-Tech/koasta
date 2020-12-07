ALTER TABLE "ApiToken" ADD COLUMN companyId INT NULL;
ALTER TABLE "ApiToken" ADD CONSTRAINT FK_ApiToken_Company FOREIGN KEY (companyId) REFERENCES "Company" (companyId) ON DELETE CASCADE ON UPDATE CASCADE;

INSERT INTO "ApiToken" (apitokenvalue, description, expiry, companyid)
VALUES ('CHANGEME', 'API key used for Sysadmin operations and the Bar UI', '3000-01-01', NULL);
