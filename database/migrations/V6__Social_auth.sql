DELETE FROM "UserSession";

ALTER TABLE "UserSession" DROP COLUMN refreshToken;
ALTER TABLE "UserSession" DROP COLUMN refreshTokenExpiry;
ALTER TABLE "UserSession" ADD COLUMN type INT NOT NULL DEFAULT 0;
ALTER TABLE "UserSession" ALTER COLUMN authToken TYPE VARCHAR(8000);

ALTER TABLE "User" ADD COLUMN appleUserIdentifier VARCHAR(8000) NULL;
ALTER TABLE "User" ADD COLUMN facebookUserIdentifier VARCHAR(8000) NULL;
ALTER TABLE "User" ADD COLUMN googleUserIdentifier VARCHAR(8000) NULL;

CREATE INDEX idx_user_appleuseridentifier ON "User"(appleUserIdentifier);
CREATE INDEX idx_user_facebookuseridentifier ON "User"(facebookUserIdentifier);
CREATE INDEX idx_user_googleuseridentifier ON "User"(googleUserIdentifier);

DROP TABLE "UserVerification";

