--
-- Table structure for table `Review`
--

DROP TABLE IF EXISTS "Review";

CREATE SEQUENCE Review_seq;

CREATE TABLE "Review" (
  reviewId int NOT NULL DEFAULT NEXTVAL ('Review_seq'),
  venueId int NOT Null,
  userId int Null,
  reviewSummary varchar(100) Null,
  reviewDetail varchar(600) Null,
  rating int null,
  registeredInterest boolean not null default false,
  approved boolean not null default false,
  created timestamp(0) WITH TIME ZONE NOT NULL default now(),
  updated timestamp(0) WITH TIME ZONE NOT NULL default now(),
  PRIMARY KEY (reviewId),
  UNIQUE (venueId, userId),
  CONSTRAINT FK_Review_Venue FOREIGN KEY (venueId) REFERENCES "Venue" (venueId) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT FK_Review_User FOREIGN KEY (userId) REFERENCES "User" (userId) ON DELETE CASCADE ON UPDATE CASCADE
) ;

CREATE INDEX FK_Review_Venue_idx ON "Venue" (venueId);

--
-- Table structure for table `Tag`
--

DROP TABLE IF EXISTS "Tag";

CREATE SEQUENCE Tag_seq;

CREATE TABLE "Tag" (
  tagId int NOT NULL DEFAULT NEXTVAL ('Tag_seq'),
  tagName varchar(100) UNIQUE NOT Null ,
  PRIMARY KEY (tagId)
) ;

--
-- Table structure for table `VenueTag`
--

DROP TABLE IF EXISTS "VenueTag";

CREATE SEQUENCE VenueTag_seq;

CREATE TABLE "VenueTag" (
  venueTagId int NOT NULL DEFAULT NEXTVAL ('VenueTag_seq'),
  venueId int NOT Null,
  tagId int NOT Null,
  PRIMARY KEY (venueTagId)
 ,
  CONSTRAINT FK_VenueTag_Venue FOREIGN KEY (venueId) REFERENCES "Venue" (venueId) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT FK_VenueTag_Tag FOREIGN KEY (tagId) REFERENCES "Tag" (tagId) ON DELETE CASCADE ON UPDATE CASCADE
) ;

CREATE INDEX FK_VenueTag_Venue_idx ON "Venue" (venueId);


--
-- Updates to table structure for table `Venue`
--

ALTER TABLE "Venue"
ADD COLUMN venueProgress int NOT NULL DEFAULT 0;

CREATE INDEX FK_VenueProgress_Venue_idx ON "Venue" (venueProgress);
