--
-- Table structure for table `FeatureFlag`
--

DROP TABLE IF EXISTS "FeatureFlag";

CREATE SEQUENCE FeatureFlag_seq;

CREATE TABLE "FeatureFlag" (
  flagId int NOT NULL DEFAULT NEXTVAL ('FeatureFlag_seq'),
  name varchar(100) NOT NULL,
  description varchar(8000) NULL,
  value boolean NOT NULL DEFAULT false,
  
  PRIMARY KEY (flagId)
) ;

CREATE INDEX FK_FeatureFlag_Name_idx ON "FeatureFlag" (name);
