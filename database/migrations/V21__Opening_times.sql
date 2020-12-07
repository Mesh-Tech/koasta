--
-- Table structure for table `VenueOpeningTime`
--

DROP TABLE IF EXISTS "VenueOpeningTime";

CREATE SEQUENCE VenueOpeningTime_seq;

CREATE TABLE "VenueOpeningTime" (
  venueOpeningTimeId int NOT NULL DEFAULT NEXTVAL ('VenueOpeningTime_seq'),
  venueId int NOT NULL,
  startTime time(0) NOT NULL,
  endTime time(0) NOT NULL,
  dayOfWeek int NOT NULL,
  PRIMARY KEY (venueOpeningTimeId)
 ,
  CONSTRAINT FK_VenueOpeningTime_Venue FOREIGN KEY (venueId) REFERENCES "Venue" (venueId) ON DELETE CASCADE ON UPDATE CASCADE
) ;

CREATE INDEX FK_VenueOpeningTime_Venue_idx ON "Venue" (venueId);
CREATE UNIQUE INDEX uq_VenueOpeningTime_dayOfWeek ON "VenueOpeningTime" (venueId, dayOfWeek);
