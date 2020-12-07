ALTER TABLE "Company" ADD COLUMN externalAccessToken VARCHAR(8000) NULL;
ALTER TABLE "Company" ADD COLUMN externalRefreshToken VARCHAR(8000) NULL;
ALTER TABLE "Company" ADD COLUMN externalTokenExpiry timestamp(0) WITH TIME ZONE NULL;

ALTER TABLE "Venue" ADD COLUMN externalLocationId VARCHAR(8000) NULL;
