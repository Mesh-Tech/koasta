--
-- Table structure for table `UserDevice`
--

DROP TABLE IF EXISTS "UserDevice";

CREATE SEQUENCE UserDevice_seq;

CREATE TABLE "UserDevice" (
  deviceId int NOT NULL DEFAULT NEXTVAL ('UserDevice_seq'),
  userId int not null,
  deviceToken varchar(1000) not null,
  platform int not null,
  created timestamp(0) WITH TIME ZONE NOT NULL default now(),
  updated timestamp(0) WITH TIME ZONE NOT NULL default now(),
  PRIMARY KEY (deviceId),
  UNIQUE (deviceToken),
  CONSTRAINT FK_UserDevice_User FOREIGN KEY (userId) REFERENCES "User" (userId) ON DELETE CASCADE ON UPDATE CASCADE
) ;

CREATE INDEX FK_UserDevice_User_idx ON "User" (userId);