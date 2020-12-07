--
-- Table structure for table `Company`
--

DROP TABLE IF EXISTS "Company";

CREATE SEQUENCE Company_seq;

CREATE TABLE "Company" (
  companyId int NOT NULL DEFAULT NEXTVAL ('Company_seq'),
  companyName varchar(100) NOT NULL,
  companyAddress varchar(8000) NOT NULL,
  companyPostCode varchar(10) NOT NULL,
  companyContact varchar(100) DEFAULT NULL,
  companyPhone varchar(20) NOT NULL,
  companyEmail varchar(100) NOT NULL,
  externalAccountId varchar(100) NULL,
  externalCustomerId varchar(100) NULL,
  PRIMARY KEY (companyId)
) ;

--
-- Table structure for table `Image`
--

DROP TABLE IF EXISTS "Image";

CREATE SEQUENCE Image_seq;

CREATE TABLE "Image" (
  imageId int NOT NULL DEFAULT NEXTVAL ('Image_seq'),
  companyId int NOT Null,
  imageKey varchar(100) NOT Null,
  imageTitle varchar(100) NOT Null,
  PRIMARY KEY (imageId)
 ,
  CONSTRAINT FK_Image_Company FOREIGN KEY (companyId) REFERENCES "Company" (companyId) ON DELETE CASCADE ON UPDATE CASCADE
) ;

CREATE INDEX FK_Image_Company_idx ON "Image" (companyId);

--
-- Table structure for table `Venue`
--

DROP TABLE IF EXISTS "Venue";

CREATE SEQUENCE Venue_seq;

CREATE TABLE "Venue" (
  venueId int NOT NULL DEFAULT NEXTVAL ('Venue_seq'),
  companyId int NOT NULL,
  venueCode varchar(20) NOT NULL,
  venueName varchar(100) NOT NULL,
  venueAddress varchar(8000) NOT NULL,
  venuePostCode varchar(10) NOT NULL,
  venuePhone varchar(15) NOT NULL,
  venueContact varchar(30) DEFAULT NULL,
  venueDescription varchar(8000),
  venueNotes varchar(8000),
  venueImage varchar(8000),
  imageId int NULL,
  venueLatitude varchar(15) DEFAULT NULL,
  venueLongitude varchar(15) DEFAULT NULL,
  venueCoordinate GEOMETRY DEFAULT NULL,
  PRIMARY KEY (venueId)
 ,
  CONSTRAINT FK_Venue_Company FOREIGN KEY (companyId) REFERENCES "Company" (companyId) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT FK_Venue_Image FOREIGN KEY (imageId) REFERENCES "Image" (imageId) ON DELETE CASCADE ON UPDATE CASCADE
) ;

CREATE INDEX FK_Venue_Company_idx ON "Venue" (companyId);
CREATE INDEX FK_Venue_Image_idx ON "Venue" (imageId);

DROP TABLE IF EXISTS "SubscriptionPackage";

CREATE SEQUENCE SubscriptionPackage_seq;

CREATE TABLE "SubscriptionPackage" (
  packageId int NOT NULL DEFAULT NEXTVAL ('SubscriptionPackage_seq'),
  packageName varchar(100) NOT NULL,
  externalPackageId varchar(100) NOT NULL,
  PRIMARY KEY (packageId)
) ;

--
-- Table structure for table `EmployeeRole`
--

DROP TABLE IF EXISTS "EmployeeRole";

CREATE SEQUENCE EmployeeRole_seq;

CREATE TABLE "EmployeeRole" (
  roleId int NOT NULL DEFAULT NEXTVAL ('EmployeeRole_seq'),
  roleName varchar(200) NOT NULL,
  canWorkWithVenue BOOL NOT NULL,
  canAdministerVenue BOOL NOT NULL,
  canWorkWithCompany BOOL NOT NULL,
  canAdministerCompany BOOL NOT NULL,
  canAdministerSystem BOOL NOT NULL,
  PRIMARY KEY (roleId),
  CONSTRAINT UQ_EmployeeRole_RoleName UNIQUE (roleName)
) ;

--
-- Default records for table `EmployeeRole`
--

INSERT INTO "EmployeeRole" (roleId, roleName, canWorkWithVenue, canAdministerVenue, canWorkWithCompany, canAdministerCompany, canAdministerSystem)
VALUES
	(1, 'None', FALSE, FALSE, FALSE, FALSE, FALSE),
	(2, 'Bar Staff', TRUE, FALSE, FALSE, FALSE, FALSE),
	(3, 'Bar Manager', TRUE, TRUE, FALSE, FALSE, FALSE),
	(4, 'Company Staff', TRUE, TRUE, TRUE, FALSE, FALSE),
	(5, 'Company Manager', TRUE, TRUE, TRUE, TRUE, FALSE),
	(6, 'Sysadmin', TRUE, TRUE, TRUE, TRUE, TRUE);

--
-- Table structure for table `Employee`
--

DROP TABLE IF EXISTS "Employee";

CREATE SEQUENCE Employee_seq;

CREATE TABLE "Employee" (
  employeeId int NOT NULL DEFAULT NEXTVAL ('Employee_seq'),
  employeeName varchar(200) NULL,
  username varchar(200) NOT NULL,
  passwordHash varchar(128) NOT NULL,
  companyId int NOT NULL,
  venueId int NOT NULL,
  roleId int NOT NULL DEFAULT 1, -- Defaults to role 'None' which has no permissions
  PRIMARY KEY (employeeId)
 ,
  CONSTRAINT UQ_Employee_Username UNIQUE (username, companyId),
  CONSTRAINT FK_Employee_Company FOREIGN KEY (companyId) REFERENCES "Company" (companyId) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT FK_Employee_Venue FOREIGN KEY (venueId) REFERENCES "Venue" (venueId) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT FK_Employee_Role FOREIGN KEY (roleId) REFERENCES "EmployeeRole" (roleId) ON UPDATE CASCADE
) ;

CREATE INDEX FK_Employee_Company_idx ON "Employee" (companyId);
CREATE INDEX FK_Employee_Venu_idx ON "Employee" (venueId);
CREATE INDEX FK_Employee_Role_idx ON "Employee" (roleId);

--
-- Table structure for table `EmployeeSession`
--

DROP TABLE IF EXISTS "EmployeeSession";

CREATE SEQUENCE EmployeeSession_seq;

CREATE TABLE "EmployeeSession" (
  sessionId int NOT NULL DEFAULT NEXTVAL ('EmployeeSession_seq'),
  employeeId int NOT NULL,
  authToken varchar(8000) NOT NULL,
  refreshToken varchar(8000) NOT NULL,
  expiry timestamp(0) WITH TIME ZONE NOT NULL,
  refreshExpiry timestamp(0) WITH TIME ZONE NOT NULL,
  PRIMARY KEY (sessionId),
  CONSTRAINT FK_EmployeeSession_Employee FOREIGN KEY (employeeId) REFERENCES "Employee" (employeeId) ON DELETE CASCADE ON UPDATE CASCADE
) ;

--
-- Table structure for table `User`
--

DROP TABLE IF EXISTS "User";

CREATE SEQUENCE User_seq;

CREATE TABLE "User" (
  userId int NOT NULL DEFAULT NEXTVAL ('User_seq'),
  registrationId varchar(64) NOT NULL,
  firstName varchar(50) NULL,
  lastName varchar(50) NULL,
  email varchar(100) NULL,
  dob varchar(20) NULL,
  isVerified BOOL NOT NULL DEFAULT FALSE,
  wantAdvertising BOOL NOT NULL DEFAULT FALSE,
  externalPaymentProcessorId varchar(256) NULL,
PRIMARY KEY (userId)
) ;

--
-- Table structure for table `Order`
--

DROP TABLE IF EXISTS "CustomerOrder";

CREATE SEQUENCE CustomerOrder_seq;

CREATE TABLE "CustomerOrder" (
  orderId int NOT NULL DEFAULT NEXTVAL ('CustomerOrder_seq'),
  orderNumber int NOT NULL,
  userId int NOT NULL,
  venueId int NOT NULL,
  orderStatus int NOT NULL,
  employeeId int DEFAULT NULL,
  orderTimeStamp timestamp(0) WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP NOT NULL,
  externalPaymentId  varchar(100) NULL,
  PRIMARY KEY (orderId)
 ,
  CONSTRAINT FK_Order_Employee FOREIGN KEY (employeeId) REFERENCES "Employee" (employeeId) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT FK_Order_User FOREIGN KEY (userId) REFERENCES "User" (userId) ON DELETE NO ACTION ON UPDATE CASCADE,
  CONSTRAINT FK_Order_Venue FOREIGN KEY (venueId) REFERENCES "Venue" (venueId) ON DELETE CASCADE ON UPDATE CASCADE
) ;

CREATE INDEX FK_Order_User_idx ON "CustomerOrder" (userId);
CREATE INDEX FK_Order_Venue_idx ON "CustomerOrder" (venueId);
CREATE INDEX FK_Order_Employee_idx ON "CustomerOrder" (employeeId);
--
-- Table structure for table `Menu`
--

DROP TABLE IF EXISTS "Menu";

CREATE SEQUENCE Menu_seq;

CREATE TABLE "Menu" (
  menuId int NOT NULL DEFAULT NEXTVAL ('Menu_seq'),
  venueId int NOT NULL,
  menuDescription varchar(8000),
  menuName varchar(255) NOT NULL,
  menuImage varchar(255) DEFAULT NULL,
  PRIMARY KEY (menuId)
 ,
  CONSTRAINT FK_Menu_Venue FOREIGN KEY (venueId) REFERENCES "Venue" (venueId) ON DELETE CASCADE ON UPDATE CASCADE
) ;

CREATE INDEX FK_Menu_Venue_idx ON "Menu" (venueId);

--
-- Table structure for table `MenuItem`
--

DROP TABLE IF EXISTS "MenuItem";

CREATE SEQUENCE MenuItem_seq;

CREATE TABLE "MenuItem" (
  menuItemId int NOT NULL DEFAULT NEXTVAL ('MenuItem_seq'),
  venueId int NOT NULL,
  menuId int NOT NULL,
  productId int NOT NULL,
  PRIMARY KEY (menuItemId)
 ,
  CONSTRAINT FK_MenuItem_Venue FOREIGN KEY (venueId) REFERENCES "Venue" (venueId) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT FK_MenuItem_Menu FOREIGN KEY (menuId) REFERENCES "Menu" (menuId) ON DELETE CASCADE ON UPDATE CASCADE
) ;

CREATE INDEX FK_MenuItem_Venue_idx ON "MenuItem" (venueId);
CREATE INDEX FK_MenuItem_Menu_idx ON "MenuItem" (menuId);

--
-- Table structure for table `MenuAvailability`
--

DROP TABLE IF EXISTS "MenuAvailability";

CREATE SEQUENCE MenuAvailability_seq;

CREATE TABLE "MenuAvailability" (
  menuAvailabilityId int NOT NULL DEFAULT NEXTVAL ('MenuAvailability_seq'),
  menuId int NOT NULL,
  timeStart time(6) NOT NULL,
  timeEnd time(6) NOT NULL,
  day int NOT NULL,
  PRIMARY KEY (menuAvailabilityId)
 ,
  CONSTRAINT FK_MenuAvailability_Menu FOREIGN KEY (menuId) REFERENCES "Menu" (menuId) ON DELETE CASCADE ON UPDATE CASCADE
) ;

CREATE INDEX FK_MenuAvailability_Menu_idx ON "MenuAvailability" (menuId);



--
-- Table structure for table `ProductType`
--

DROP TABLE IF EXISTS "ProductType";

CREATE SEQUENCE ProductType_seq;

CREATE TABLE "ProductType" (
  productTypeId int NOT NULL DEFAULT NEXTVAL ('ProductType_seq'),
  productTypeName varchar(100) NOT NULL,
  PRIMARY KEY (productTypeId)
) ;

INSERT INTO "ProductType" (productTypeName)
VALUES ('Soft Drink'), ('Hot Drink'), ('Mixer'), ('Beer'), ('Wine'), ('Spirit');

--
-- Table structure for table `Product`
--

DROP TABLE IF EXISTS "Product";

CREATE SEQUENCE Product_seq;

CREATE TABLE "Product" (
  productId int NOT NULL DEFAULT NEXTVAL ('Product_seq'),
  productTypeId int NOT NULL,
  venueId int NOT NULL,
  menuId int NULL,
  productName varchar(50) NOT NULL,
  productDescription varchar(8000),
  price decimal(5,2) NOT NULL DEFAULT 0,
  image varchar(800) NULL,
  ageRestricted boolean NOT NULL,
  parentProductId int NULL,
  PRIMARY KEY (productId)
 ,
  CONSTRAINT FK_Product_Venue FOREIGN KEY (venueId) REFERENCES "Venue" (venueId) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT FK_Product_Menu FOREIGN KEY (menuId) REFERENCES "Menu" (menuId) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT FK_Product_ProductType FOREIGN KEY (productTypeId) REFERENCES "ProductType" (productTypeId) ON DELETE CASCADE ON UPDATE CASCADE
) ;

CREATE INDEX FK_Product_Menu_idx ON "Product" (menuId);

--
-- Table structure for table `OrderLine`
--

DROP TABLE IF EXISTS "OrderLine";

CREATE SEQUENCE OrderLine_seq;

CREATE TABLE "OrderLine" (
  orderLineId int NOT NULL DEFAULT NEXTVAL ('OrderLine_seq'),
  orderId int NOT NULL,
  productId INT NULL,
  quantity int NOT NULL,
  amount decimal(5,2) NOT NULL,
  PRIMARY KEY (orderLineId)
 ,
  CONSTRAINT FK_OrderLine_Order FOREIGN KEY (orderId) REFERENCES "CustomerOrder" (orderId) ON DELETE CASCADE ON UPDATE CASCADE
  
 ,
  CONSTRAINT FK_OrderLine_Product FOREIGN KEY (productId) REFERENCES "Product" (productId) ON DELETE CASCADE

) ;

CREATE INDEX FK_OrderLine_Order_idx ON "OrderLine" (orderId);
CREATE INDEX FK_OrderLine_Product_idx ON "OrderLine" (productId);

--
-- Table structure for table `UserNumber`
--

DROP TABLE IF EXISTS "UserNumber";

CREATE SEQUENCE UserNumber_seq;

CREATE TABLE "UserNumber" (
  numberId int NOT NULL DEFAULT NEXTVAL ('UserNumber_seq'),
  userId int NOT NULL,
  phoneNumber varchar(20) DEFAULT NULL,
PRIMARY KEY (numberId)
,
CONSTRAINT FK_UserNumber_User FOREIGN KEY (userId) REFERENCES "User" (userId) ON DELETE CASCADE ON UPDATE CASCADE
) ;

CREATE INDEX FK_UserNumber_User_idx ON "UserNumber" (userId);

--
-- Table structure for table `UserSession`
--

DROP TABLE IF EXISTS "UserSession";

CREATE SEQUENCE UserSession_seq;

CREATE TABLE "UserSession" (
  sessionId int NOT NULL DEFAULT NEXTVAL ('UserSession_seq'),
  userId int NOT NULL,
  authToken varchar(512) NOT NULL,
  refreshToken varchar(512) NOT NULL,
  authTokenExpiry timestamp(0) WITH TIME ZONE NOT NULL,
  refreshTokenExpiry timestamp(0) WITH TIME ZONE NOT NULL,
PRIMARY KEY (sessionId)
,
CONSTRAINT FK_UserSession_AuthToken_idx UNIQUE  (authToken),
CONSTRAINT FK_UserSession_RefreshToken_idx UNIQUE  (refreshToken),
CONSTRAINT FK_UserSession_User FOREIGN KEY (userId) REFERENCES "User" (userId) ON DELETE CASCADE ON UPDATE CASCADE
) ;

CREATE INDEX FK_UserSession_User_idx ON "UserSession" (userId);

--
-- Table structure for table `UserVerification`
--

DROP TABLE IF EXISTS "UserVerification";

CREATE SEQUENCE UserVerification_seq;

CREATE TABLE "UserVerification" (
  verificationId int NOT NULL DEFAULT NEXTVAL ('UserVerification_seq'),
  phoneNumber varchar(20) NOT NULL,
  userId int NULL,
  verificationToken varchar(64) NOT NULL,
  expiry timestamp(0) WITH TIME ZONE NOT NULL,
PRIMARY KEY (verificationId)
,
CONSTRAINT FK_UserVerification_verificationToken_idx UNIQUE  (verificationToken),
CONSTRAINT FK_UserVerification_User FOREIGN KEY (userId) REFERENCES "User" (userId) ON DELETE CASCADE ON UPDATE CASCADE
) ;

CREATE INDEX FK_UserVerification_User_idx ON "UserVerification" (userId);

DROP TABLE IF EXISTS "SubscriptionPlan";

CREATE SEQUENCE SubscriptionPlan_seq;

CREATE TABLE "SubscriptionPlan" (
  planId int NOT NULL DEFAULT NEXTVAL ('SubscriptionPlan_seq'),
  companyId int NOT NULL,
  externalPlanId varchar(100) NULL,
  PRIMARY KEY (planId)
 ,
  CONSTRAINT FK_SubscriptionPlan_Company FOREIGN KEY (companyId) REFERENCES "Company" (companyId) ON DELETE CASCADE ON UPDATE CASCADE
) ;

CREATE INDEX FK_SubscriptionPlan_Company_idx ON "SubscriptionPlan" (companyId);

DROP TABLE IF EXISTS "SubscriptionPlanEntries";

CREATE TABLE "SubscriptionPlanEntries" (
  planId int NOT NULL,
  packageId int NOT NULL,
  PRIMARY KEY (planId, packageId)
 ,
  CONSTRAINT FK_SubscriptionPlanEntries_Plan FOREIGN KEY (planId) REFERENCES "SubscriptionPlan" (planId) ON DELETE CASCADE ON UPDATE CASCADE
 ,
  CONSTRAINT FK_SubscriptionPlanEntries_SubscriptionPackage FOREIGN KEY (packageId) REFERENCES "SubscriptionPackage" (packageId) ON DELETE CASCADE ON UPDATE CASCADE
) ;

CREATE INDEX FK_SubscriptionPlanEntries_Plan_idx ON "SubscriptionPlanEntries" (planId);
CREATE INDEX FK_SubscriptionPlanEntries_SubscriptionPackage_idx ON "SubscriptionPlanEntries" (packageId);

INSERT INTO "Company"
(companyName, companyAddress, companyPostCode, companyContact, companyPhone, companyEmail)
Values	('Sysadmins', '', '', '', '', '');

INSERT INTO "Venue"
(companyId, venueCode, venueName, venueAddress, venuePostCode, venuePhone, venueContact, venueDescription, venueNotes, venueImage)
Values	(1, '±±±±§§§§', '', '', '', '', '', '', '', '');

INSERT INTO "Employee"
(employeeName, companyId, venueId, roleId, username, passwordHash) -- Password is Password_1
VALUES ('John Doe', 1, 1, 6, 'jdoe', '$2y$10$Ylb/38PcivY8zBYj99PIF.6Zc3/ngQOdCSooWRjVP8W4HDPYoNlj2');
