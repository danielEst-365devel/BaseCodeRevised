﻿-- Assume the database 'BASE' is already created in Azure SQL Database and you're connected to it.

-- Create USERS table (unchanged)
CREATE TABLE USERS (
    USER_ID INT NOT NULL IDENTITY(1,1),
    USER_NAME NVARCHAR(50) NOT NULL,
    FIRST_NAME NVARCHAR(100) NOT NULL,
    LAST_NAME NVARCHAR(100) NOT NULL,
    EMAIL NVARCHAR(200) NOT NULL,
    PASSWORD NVARCHAR(255) NOT NULL,
    PHONE_NUMBER NVARCHAR(15) NULL,
    BIRTHDAY DATE NOT NULL,
    AGE AS (DATEDIFF(YEAR, BIRTHDAY, GETDATE())),
    ACCOUNT_STATUS CHAR(1) NOT NULL DEFAULT 'A' CHECK (ACCOUNT_STATUS IN ('A', 'I')),
    CIVIL_STATUS NVARCHAR(20) NOT NULL CHECK (CIVIL_STATUS IN ('SINGLE', 'MARRIED', 'DIVORCED', 'WIDOWED')),
    CREATEDATE DATETIME NOT NULL DEFAULT GETDATE(),
    UPDATEDATE DATETIME NULL,
    CONSTRAINT PK_USERS PRIMARY KEY (USER_ID),
    CONSTRAINT UQ_USER_NAME UNIQUE (USER_NAME),
    CONSTRAINT UQ_EMAIL UNIQUE (EMAIL)
);

-- Insert into USERS (unchanged)
INSERT INTO USERS (USER_NAME, FIRST_NAME, LAST_NAME, EMAIL, PASSWORD, PHONE_NUMBER, BIRTHDAY, ACCOUNT_STATUS, CIVIL_STATUS, CREATEDATE, UPDATEDATE)
VALUES ('daniel.est.03', 'Daniel Anthony', 'Estrella', 'daniel.estrella.xentra@gmail.com', 'u1oxzS/BrIPLA/SxEeuWxOt7SFii6BDCSyYBIih/CuHVSDsF47fUSxdV/P4MH2uL', '+18777804236', '2003-05-11', 'A', 'SINGLE', '2025-02-25 10:09:36', NULL);

-- Create FAILED_LOGINS table (unchanged)
CREATE TABLE FAILED_LOGINS (
    ID INT NOT NULL IDENTITY(1,1),
    USER_ID INT NOT NULL,
    ATTEMPTDATE DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT PK_FAILED_LOGINS PRIMARY KEY (ID),
    CONSTRAINT FK_FAILED_LOGINS_USERS FOREIGN KEY (USER_ID) REFERENCES USERS (USER_ID)
);

-- Create PERMISSIONS table (unchanged)
CREATE TABLE PERMISSIONS (
    PERMISSION_ID INT NOT NULL IDENTITY(1,1),
    PERMISSION_NAME NVARCHAR(100) NOT NULL,
    DESCRIPTION NVARCHAR(255) NULL,
    CONSTRAINT PK_PERMISSIONS PRIMARY KEY (PERMISSION_ID),
    CONSTRAINT UQ_PERMISSION_NAME UNIQUE (PERMISSION_NAME)
);

-- Insert into PERMISSIONS (remove explicit PERMISSION_ID values)
INSERT INTO PERMISSIONS (PERMISSION_NAME, DESCRIPTION)
VALUES 
('CreateUser', 'Can create users.'),
('ViewActiveUsers', 'View the complete list of all active users.'),
('UpdateUserDetails', 'Updates the details of a user.'),
('DeleteUser', 'Turns the account status of a user to inactive.');

-- Create ROLES table (unchanged)
CREATE TABLE ROLES (
    ROLE_ID INT NOT NULL IDENTITY(1,1),
    ROLE_NAME NVARCHAR(50) NOT NULL,
    DESCRIPTION NVARCHAR(255) NULL,
    CONSTRAINT PK_ROLES PRIMARY KEY (ROLE_ID),
    CONSTRAINT UQ_ROLE_NAME UNIQUE (ROLE_NAME)
);

-- Insert into ROLES (remove explicit ROLE_ID values)
INSERT INTO ROLES (ROLE_NAME, DESCRIPTION)
VALUES 
('Admin', 'System administrator.'),
('Customer', NULL);

-- Create ROLE_PERMISSIONS table (unchanged schema)
CREATE TABLE ROLE_PERMISSIONS (
    ROLE_ID INT NOT NULL,
    PERMISSION_ID INT NOT NULL,
    CONSTRAINT PK_ROLE_PERMISSIONS PRIMARY KEY (ROLE_ID, PERMISSION_ID),
    CONSTRAINT FK_ROLE_PERMISSIONS_ROLES FOREIGN KEY (ROLE_ID) REFERENCES ROLES (ROLE_ID) ON DELETE CASCADE,
    CONSTRAINT FK_ROLE_PERMISSIONS_PERMISSIONS FOREIGN KEY (PERMISSION_ID) REFERENCES PERMISSIONS (PERMISSION_ID) ON DELETE CASCADE
);

-- Insert into ROLE_PERMISSIONS dynamically using subqueries
INSERT INTO ROLE_PERMISSIONS (ROLE_ID, PERMISSION_ID)
SELECT 
    (SELECT ROLE_ID FROM ROLES WHERE ROLE_NAME = 'Admin'),
    PERMISSION_ID
FROM PERMISSIONS
WHERE PERMISSION_NAME IN ('CreateUser', 'ViewActiveUsers', 'UpdateUserDetails', 'DeleteUser');

-- Create SESSIONS table (reduce SESSION_ID size to NVARCHAR(450))
CREATE TABLE SESSIONS (
    SESSION_ID NVARCHAR(450) NOT NULL, -- Reduced from 768 to fit 900-byte limit
    USER_ID INT NOT NULL,
    EXPIRES_AT DATETIME NOT NULL,
    CREATED_AT DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT PK_SESSIONS PRIMARY KEY (SESSION_ID),
    CONSTRAINT FK_SESSIONS_USERS FOREIGN KEY (USER_ID) REFERENCES USERS (USER_ID) ON DELETE CASCADE
);

-- Create USERS_OTP table (unchanged)
CREATE TABLE USERS_OTP (
    OTP_ID INT NOT NULL IDENTITY(1,1),
    USER_ID INT NOT NULL,
    OTP NVARCHAR(8) NOT NULL,
    STATUS CHAR(1) NOT NULL DEFAULT 'A' CHECK (STATUS IN ('A', 'E', 'U')),
    CREATED_AT DATETIME NOT NULL DEFAULT GETDATE(),
    EXPIRY_DATE DATETIME NOT NULL,
    CONSTRAINT PK_USERS_OTP PRIMARY KEY (OTP_ID),
    CONSTRAINT FK_USERS_OTP_USERS FOREIGN KEY (USER_ID) REFERENCES USERS (USER_ID)
);

-- Create USER_ADDRESSES table (unchanged)
CREATE TABLE USER_ADDRESSES (
    ADDRESS_ID INT NOT NULL IDENTITY(1,1),
    USER_ID INT NOT NULL,
    STREET NVARCHAR(200) NOT NULL,
    CITY NVARCHAR(100) NOT NULL,
    STATE NVARCHAR(100) NOT NULL,
    ZIPCODE NVARCHAR(20) NOT NULL,
    COUNTRY NVARCHAR(100) NOT NULL,
    CREATEDATE DATETIME NOT NULL DEFAULT GETDATE(),
    UPDATEDATE DATETIME NULL,
    CONSTRAINT PK_USER_ADDRESSES PRIMARY KEY (ADDRESS_ID),
    CONSTRAINT UQ_USER_ADDRESSES_USER_ID UNIQUE (USER_ID),
    CONSTRAINT FK_USER_ADDRESSES_USERS FOREIGN KEY (USER_ID) REFERENCES USERS (USER_ID)
);

-- Insert into USER_ADDRESSES (unchanged)
INSERT INTO USER_ADDRESSES (USER_ID, STREET, CITY, STATE, ZIPCODE, COUNTRY, CREATEDATE, UPDATEDATE)
VALUES (1, 'Ruby', 'Marilao', 'Bulacan', '3019', 'Philippines', '2025-02-25 10:09:36', NULL);

-- Create USER_ROLES table (unchanged)
CREATE TABLE USER_ROLES (
    USER_ID INT NOT NULL,
    ROLE_ID INT NOT NULL,
    CONSTRAINT PK_USER_ROLES PRIMARY KEY (USER_ID, ROLE_ID),
    CONSTRAINT FK_USER_ROLES_USERS FOREIGN KEY (USER_ID) REFERENCES USERS (USER_ID) ON DELETE CASCADE,
    CONSTRAINT FK_USER_ROLES_ROLES FOREIGN KEY (ROLE_ID) REFERENCES ROLES (ROLE_ID) ON DELETE CASCADE
);

-- Create Indexes (unchanged)
CREATE INDEX idx_user_id_failed_logins ON FAILED_LOGINS (USER_ID);
CREATE INDEX idx_permission_id ON ROLE_PERMISSIONS (PERMISSION_ID);
CREATE INDEX idx_user_id ON SESSIONS (USER_ID);
CREATE INDEX idx_session_id ON SESSIONS (SESSION_ID);
CREATE INDEX idx_user_id_otp ON USERS_OTP (USER_ID);
CREATE INDEX idx_user_id_addresses ON USER_ADDRESSES (USER_ID);
CREATE INDEX idx_role_id ON USER_ROLES (ROLE_ID);