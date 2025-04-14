	Go
	Create Database GZMSs;
	Go


	USE GZMSs;

	CREATE TABLE Tbl_Role (
		ID INT IDENTITY(1,1) PRIMARY KEY, -- Auto-increment ID for Role
		Role VARCHAR(2) NOT NULL -- Defines the Role
	);

	CREATE TABLE Tbl_Users (
		ID INT IDENTITY(1,1) PRIMARY KEY, -- Auto-increment ID for User
		Name VARCHAR(65) NOT NULL, -- Name of the user
		DOB DATE NOT NULL, -- Date of Birth
		Password VARCHAR(64) NOT NULL, -- Password
		Email VARCHAR(256) NOT NULL, -- Email address
		Phone VARCHAR(10) NOT NULL, -- Phone number
		Gender CHAR(1) NOT NULL, -- Gender
		RoleID INT NOT NULL, -- Role ID
		Status Bit,
		CONSTRAINT FK_Tbl_Users_Role FOREIGN KEY (RoleID) REFERENCES Tbl_Role(ID) -- FK to Tbl_Role
	);

	CREATE TABLE Tbl_Credits (
		ID INT IDENTITY(1,1) PRIMARY KEY, -- Auto-increment ID for Credits
		Credits SMALLMONEY NOT NULL, -- Credit amount
		UserID INT NOT NULL, -- User ID
		Status bit Default(1),
		CONSTRAINT FK_Tbl_Credit_User FOREIGN KEY (UserID) REFERENCES Tbl_Users(ID) -- FK to Tbl_Users
	);

	CREATE TABLE Tbl_Payments (
		ID INT IDENTITY(1,1) PRIMARY KEY, -- Auto-increment ID for Payments
		TransactionID VARCHAR(35) NOT NULL UNIQUE, -- Transaction ID
		Type BIT NOT NULL, -- Payment type
		UserID INT NOT NULL, -- User ID
		Date date,
		Status bit Default(1),
		CONSTRAINT FK_Tbl_Payment_User FOREIGN KEY (UserID) REFERENCES Tbl_Users(ID) -- FK to Tbl_Users
	);

	CREATE TABLE Tbl_Area (
		ID INT IDENTITY(1,1) PRIMARY KEY, -- Auto-increment ID for Area
		Area VARCHAR(20) NOT NULL, -- Name of the area
		Status bit Default(1)
	);

	CREATE TABLE Tbl_Employees (
		ID INT IDENTITY(1,1) PRIMARY KEY, -- Auto-increment ID for Employees
		UserID INT NOT NULL, -- User ID
		Status bit Default(1),
		CONSTRAINT FK_Tbl_Employees_User FOREIGN KEY (UserID) REFERENCES Tbl_Users(ID) -- FK to Tbl_Users
	);

	CREATE TABLE Tbl_Games_Category (
		ID INT IDENTITY(1,1) PRIMARY KEY, -- Auto-increment ID for Game Categories
		CategoryName VARCHAR(12) NOT NULL, -- Name of the category
		Status bit Default(1)
	);

	CREATE TABLE Tbl_Games_Sub_Category (
		ID INT IDENTITY(1,1) PRIMARY KEY, -- Auto-increment ID for Subcategories
		Sub_Category_Name VARCHAR(12) NOT NULL, -- Name of the subcategory
		CategoryID INT NOT NULL, -- FK to Tbl_Games_Category
		Status bit Default(1),
		CONSTRAINT FK_Tbl_Games_Sub_Category_Category FOREIGN KEY (CategoryID) REFERENCES Tbl_Games_Category(ID)
	);

	CREATE TABLE Tbl_Game (
		ID INT IDENTITY(1,1) PRIMARY KEY, -- Auto-increment ID for Games
		Game VARCHAR(20) NOT NULL, -- Game name
		Game_Description NVARCHAR(MAX) NOT NULL, -- Game description
		SubCatID INT NOT NULL, 
		Image VARCHAR(200) NOT NULL, -- Image URL or path
		Status bit Default(1),
		CONSTRAINT FK_Tbl_Game_Branch FOREIGN KEY (SubCatID) REFERENCES Tbl_Games_Sub_Category(ID)
	);

	CREATE TABLE Tbl_Day (
		ID INT IDENTITY(1,1) PRIMARY KEY, -- Auto-increment ID for Days
		Day VARCHAR(10) NOT NULL, -- Day name
		Status bit Default(1)
	);

	CREATE TABLE Tbl_Time (
		ID INT IDENTITY(1,1) PRIMARY KEY, -- Auto-increment ID for Time slots
		Start_Time TIME NOT NULL, -- Start time
		End_Time TIME NOT NULL, -- End time
		Status bit Default(1)
	);

	CREATE TABLE Tbl_Slot (
		ID INT IDENTITY(1,1) PRIMARY KEY, -- Auto-increment ID for Slots
		DayID INT NOT NULL, -- FK to Tbl_Day
		TimeID INT NOT NULL, -- FK to Tbl_Time
		CONSTRAINT FK_Tbl_Slot_Day FOREIGN KEY (DayID) REFERENCES Tbl_Day(ID),
		CONSTRAINT FK_Tbl_Slot_Time FOREIGN KEY (TimeID) REFERENCES Tbl_Time(ID)
	);

	CREATE TABLE Tbl_Game_Slot (
		ID INT IDENTITY(1,1) PRIMARY KEY, -- Auto-increment ID for Game Slots
		GameID INT NOT NULL, -- FK to Tbl_Game
		SlotID INT NOT NULL,-- FK to Tbl_Slot
		Status bit Default(1),
		CONSTRAINT FK_Tbl_Game_Slot_Game FOREIGN KEY (GameID) REFERENCES Tbl_Game(ID),
		CONSTRAINT FK_Tbl_Game_Slot_Slot FOREIGN KEY (SlotID) REFERENCES Tbl_Slot(ID)
	);

	CREATE TABLE Tbl_Price (
		ID INT IDENTITY(1,1) PRIMARY KEY, -- Auto-increment ID for Prices
		GameID INT NOT NULL, -- FK to Tbl_Game
		Credits SMALLMONEY NOT NULL, -- Price in credits
		CONSTRAINT FK_Tbl_Price_Game FOREIGN KEY (GameID) REFERENCES Tbl_Game(ID)
	);

	CREATE TABLE Tbl_Game_Slot_Booking (
		ID INT IDENTITY(1,1) PRIMARY KEY, -- Auto-increment ID for Game Slot Bookings
		Game_SlotID INT NOT NULL, -- FK to Tbl_Game_Slot
		UserID INT NOT NULL, -- FK to Tbl_Users
		Date date,
		Status bit Default(1),
		CONSTRAINT FK_Tbl_Game_Slot_Booking_Game_Slot FOREIGN KEY (Game_SlotID) REFERENCES Tbl_Game_Slot(ID),
		CONSTRAINT FK_Tbl_Game_Slot_Booking_User FOREIGN KEY (UserID) REFERENCES Tbl_Users(ID)
	);

	CREATE TABLE Tbl_Game_Played (
		ID INT IDENTITY(1,1) PRIMARY KEY, -- Auto-increment ID for Played Games
		GameID INT NOT NULL, -- FK to Tbl_Game
		UserID INT NOT NULL,
		Status bit Default(1),-- FK to Tbl_Users
		CONSTRAINT FK_Tbl_Game_Played_Game FOREIGN KEY (GameID) REFERENCES Tbl_Game(ID),
		CONSTRAINT FK_Tbl_Game_Played_User FOREIGN KEY (UserID) REFERENCES Tbl_Users(ID)
	);
	insert into Tbl_Role(Role) values('Ad');
	insert into Tbl_Role(Role) values('C');
	insert into Tbl_Role(Role) values('E');
	insert into Tbl_Role(Role) values('M');
	insert into Tbl_Day(Day) values('Monday');	
	insert into Tbl_Day(Day) values('Tuesday');
	insert into Tbl_Day(Day) values('Wednesday');
	insert into Tbl_Day(Day) values('Thursday');
	insert into Tbl_Day(Day) values('Friday');
	insert into Tbl_Day(Day) values('Saturday');
	insert into Tbl_Day(Day) values('Sunday');
	INSERT INTO Tbl_Time (Start_Time, End_Time) VALUES 
('10:00:00', '10:30:00'),
('10:30:00', '11:00:00'),
('11:00:00', '11:30:00'),
('11:30:00', '12:00:00'),
('12:00:00', '12:30:00'),
('12:30:00', '13:00:00'),
('13:00:00', '13:30:00'),
('13:30:00', '14:00:00'),
('14:00:00', '14:30:00'),
('14:30:00', '15:00:00'),
('15:00:00', '15:30:00'),
('15:30:00', '16:00:00'),
('16:00:00', '16:30:00'),
('16:30:00', '17:00:00'),
('17:00:00', '17:30:00'),
('17:30:00', '18:00:00'),
('18:00:00', '18:30:00'),
('18:30:00', '19:00:00'),
('19:00:00', '19:30:00'),
('19:30:00', '20:00:00'),
('20:00:00', '20:30:00'),
('20:30:00', '21:00:00');

	insert into Tbl_Users(Name,DOB,Password,Email,Phone,Gender,RoleID,Status) values('Varun Dhankhara','08-09-2004','4d1523191588e66ee85ff4c3d707040aa24762d7633fd19e4b2cf08056bb37f9','22bmiit031@gmail.com','9773472368','M',1,1);
	insert into Tbl_Users(Name,DOB,Password,Email,Phone,Gender,RoleID,Status) values('Harsh','05-02-2004','4d1523191588e66ee85ff4c3d707040aa24762d7633fd19e4b2cf08056bb37f9','22bmiit016@gmail.com','9773472369','M',2,1);
	insert into Tbl_Games_Category(CategoryName) VALUES('Indoor')
	insert into Tbl_Games_Category(CategoryName) VALUES('Outdoor')
	insert into Tbl_Games_Sub_Category(CategoryID,Sub_Category_Name) VALUES(1,'Arcade');
	insert into Tbl_Games_Sub_Category(CategoryID,Sub_Category_Name) VALUES(2,'Sports');