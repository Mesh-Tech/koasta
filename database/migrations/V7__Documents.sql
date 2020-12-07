--
-- Table structure for table `Document`
--

DROP TABLE IF EXISTS "Document";

CREATE SEQUENCE Document_seq;

CREATE TABLE "Document" (
  documentId int NOT NULL DEFAULT NEXTVAL ('Document_seq'),
  companyId int NOT Null,
  documentKey varchar(100) NOT NULL,
  documentTitle varchar(100) NOT NULL,
  documentDescription varchar(8000) NULL,
  PRIMARY KEY (documentId)
 ,
  CONSTRAINT FK_Document_Company FOREIGN KEY (companyId) REFERENCES "Company" (companyId) ON DELETE CASCADE ON UPDATE CASCADE
) ;

CREATE INDEX FK_Document_Company_idx ON "Document" (companyId);

--
-- Table structure for table `VenueDocuments`
--

DROP TABLE IF EXISTS "VenueDocuments";

CREATE TABLE "VenueDocuments" (
  venueId int NOT NULL,
  documentId int NOT NULL,
  PRIMARY KEY (venueId, documentId)
 ,
  CONSTRAINT FK_VenueDocuments_Venue FOREIGN KEY (venueId) REFERENCES "Venue" (venueId) ON DELETE CASCADE ON UPDATE CASCADE
 ,
  CONSTRAINT FK_VenueDocuments_Document FOREIGN KEY (documentId) REFERENCES "Document" (documentId) ON DELETE CASCADE ON UPDATE CASCADE
) ;

CREATE INDEX FK_VenueDocuments_Venue_idx ON "VenueDocuments" (venueId);
CREATE INDEX FK_VenueDocuments_Document_idx ON "VenueDocuments" (documentId);

ALTER TABLE "Venue" ADD COLUMN verificationStatus INT NOT NULL DEFAULT 0;
UPDATE "Venue" SET verificationStatus = 1;
