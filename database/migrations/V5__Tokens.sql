--
-- Table structure for table `Token`
--

DROP TABLE IF EXISTS "ApiToken";

CREATE SEQUENCE Token_seq;

CREATE TABLE "ApiToken" (
  apiTokenId int NOT NULL DEFAULT NEXTVAL ('Token_seq') PRIMARY KEY,
  apiTokenValue varchar(100) NOT NULL,
  description varchar(8000) NOT NULL,
  expiry DATE NOT NULL
) ;

