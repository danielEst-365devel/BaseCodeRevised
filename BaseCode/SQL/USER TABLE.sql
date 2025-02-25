/*
SQLyog Professional
MySQL - 10.4.32-MariaDB : Database - BASE
*********************************************************************
*/

/*!40101 SET NAMES utf8 */;

/*!40101 SET SQL_MODE=''*/;

/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;
CREATE DATABASE /*!32312 IF NOT EXISTS*/`BASE` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci */;

USE `BASE`;

/*Table structure for table `FAILED_LOGINS` */

CREATE TABLE `FAILED_LOGINS` (
  `ID` int(11) NOT NULL AUTO_INCREMENT,
  `USER_ID` int(11) NOT NULL,
  `ATTEMPTDATE` datetime NOT NULL DEFAULT current_timestamp(),
  PRIMARY KEY (`ID`),
  KEY `idx_user_id_failed_logins` (`USER_ID`),
  CONSTRAINT `failed_logins_ibfk_1` FOREIGN KEY (`USER_ID`) REFERENCES `USERS` (`USER_ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

/*Data for the table `FAILED_LOGINS` */

/*Table structure for table `PERMISSIONS` */

CREATE TABLE `PERMISSIONS` (
  `PERMISSION_ID` int(11) NOT NULL AUTO_INCREMENT,
  `PERMISSION_NAME` varchar(100) NOT NULL,
  `DESCRIPTION` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`PERMISSION_ID`),
  UNIQUE KEY `PERMISSION_NAME` (`PERMISSION_NAME`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

/*Data for the table `PERMISSIONS` */

insert  into `PERMISSIONS`(`PERMISSION_ID`,`PERMISSION_NAME`,`DESCRIPTION`) values (1,'CreateUser','Can create users.');
insert  into `PERMISSIONS`(`PERMISSION_ID`,`PERMISSION_NAME`,`DESCRIPTION`) values (2,'ViewActiveUsers','View the coplete list of all active users.');
insert  into `PERMISSIONS`(`PERMISSION_ID`,`PERMISSION_NAME`,`DESCRIPTION`) values (3,'UpdateUserDetails','Updates the details of a user.');
insert  into `PERMISSIONS`(`PERMISSION_ID`,`PERMISSION_NAME`,`DESCRIPTION`) values (4,'DeleteUser','Turns the account status of a user to inactive.');

/*Table structure for table `ROLES` */

CREATE TABLE `ROLES` (
  `ROLE_ID` int(11) NOT NULL AUTO_INCREMENT,
  `ROLE_NAME` varchar(50) NOT NULL,
  `DESCRIPTION` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`ROLE_ID`),
  UNIQUE KEY `ROLE_NAME` (`ROLE_NAME`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

/*Data for the table `ROLES` */

insert  into `ROLES`(`ROLE_ID`,`ROLE_NAME`,`DESCRIPTION`) values (1,'Admin','System administrator.');
insert  into `ROLES`(`ROLE_ID`,`ROLE_NAME`,`DESCRIPTION`) values (2,'Customer',NULL);

/*Table structure for table `ROLE_PERMISSIONS` */

CREATE TABLE `ROLE_PERMISSIONS` (
  `ROLE_ID` int(11) NOT NULL,
  `PERMISSION_ID` int(11) NOT NULL,
  PRIMARY KEY (`ROLE_ID`,`PERMISSION_ID`),
  KEY `PERMISSION_ID` (`PERMISSION_ID`),
  CONSTRAINT `role_permissions_ibfk_1` FOREIGN KEY (`ROLE_ID`) REFERENCES `ROLES` (`ROLE_ID`) ON DELETE CASCADE,
  CONSTRAINT `role_permissions_ibfk_2` FOREIGN KEY (`PERMISSION_ID`) REFERENCES `PERMISSIONS` (`PERMISSION_ID`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

/*Data for the table `ROLE_PERMISSIONS` */

insert  into `ROLE_PERMISSIONS`(`ROLE_ID`,`PERMISSION_ID`) values (1,1);
insert  into `ROLE_PERMISSIONS`(`ROLE_ID`,`PERMISSION_ID`) values (1,2);
insert  into `ROLE_PERMISSIONS`(`ROLE_ID`,`PERMISSION_ID`) values (1,3);
insert  into `ROLE_PERMISSIONS`(`ROLE_ID`,`PERMISSION_ID`) values (1,4);

/*Table structure for table `SESSIONS` */

CREATE TABLE `SESSIONS` (
  `SESSION_ID` varchar(768) NOT NULL,
  `USER_ID` int(11) NOT NULL,
  `EXPIRES_AT` datetime NOT NULL,
  `CREATED_AT` datetime NOT NULL DEFAULT current_timestamp(),
  PRIMARY KEY (`SESSION_ID`),
  KEY `USER_ID` (`USER_ID`),
  KEY `idx_session_id` (`SESSION_ID`),
  CONSTRAINT `sessions_ibfk_1` FOREIGN KEY (`USER_ID`) REFERENCES `USERS` (`USER_ID`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

/*Data for the table `SESSIONS` */

/*Table structure for table `USERS` */

CREATE TABLE `USERS` (
  `USER_ID` int(11) NOT NULL AUTO_INCREMENT,
  `USER_NAME` varchar(50) NOT NULL,
  `FIRST_NAME` varchar(100) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  `LAST_NAME` varchar(100) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  `EMAIL` varchar(200) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  `PASSWORD` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  `PHONE_NUMBER` varchar(15) DEFAULT NULL,
  `BIRTHDAY` date NOT NULL,
  `AGE` int(11) GENERATED ALWAYS AS (timestampdiff(YEAR,`BIRTHDAY`,curdate())) VIRTUAL,
  `ACCOUNT_STATUS` enum('A','I') NOT NULL DEFAULT 'A',
  `CIVIL_STATUS` enum('SINGLE','MARRIED','DIVORCED','WIDOWED') NOT NULL,
  `CREATEDATE` datetime NOT NULL DEFAULT current_timestamp(),
  `UPDATEDATE` datetime DEFAULT NULL,
  PRIMARY KEY (`USER_ID`),
  UNIQUE KEY `USER_NAME` (`USER_NAME`),
  UNIQUE KEY `EMAIL` (`EMAIL`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

/*Data for the table `USERS` */

insert  into `USERS`(`USER_ID`,`USER_NAME`,`FIRST_NAME`,`LAST_NAME`,`EMAIL`,`PASSWORD`,`PHONE_NUMBER`,`BIRTHDAY`,`ACCOUNT_STATUS`,`CIVIL_STATUS`,`CREATEDATE`,`UPDATEDATE`) values (1,'daniel.est.03','Daniel Anthony','Estrella','daniel.estrella.xentra@gmail.com','u1oxzS/BrIPLA/SxEeuWxOt7SFii6BDCSyYBIih/CuHVSDsF47fUSxdV/P4MH2uL','+18777804236','2003-05-11','A','SINGLE','2025-02-25 10:09:36',NULL);

/*Table structure for table `USERS_OTP` */

CREATE TABLE `USERS_OTP` (
  `OTP_ID` int(11) NOT NULL AUTO_INCREMENT,
  `USER_ID` int(11) NOT NULL,
  `OTP` varchar(8) NOT NULL,
  `STATUS` char(1) NOT NULL DEFAULT 'A' CHECK (`STATUS` in ('A','E','U')),
  `CREATED_AT` datetime NOT NULL DEFAULT current_timestamp(),
  `EXPIRY_DATE` datetime NOT NULL,
  PRIMARY KEY (`OTP_ID`),
  KEY `idx_user_id_otp` (`USER_ID`),
  CONSTRAINT `users_otp_ibfk_1` FOREIGN KEY (`USER_ID`) REFERENCES `USERS` (`USER_ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

/*Data for the table `USERS_OTP` */

/*Table structure for table `USER_ADDRESSES` */

CREATE TABLE `USER_ADDRESSES` (
  `ADDRESS_ID` int(11) NOT NULL AUTO_INCREMENT,
  `USER_ID` int(11) NOT NULL,
  `STREET` varchar(200) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  `CITY` varchar(100) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  `STATE` varchar(100) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  `ZIPCODE` varchar(20) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  `COUNTRY` varchar(100) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  `CREATEDATE` datetime NOT NULL DEFAULT current_timestamp(),
  `UPDATEDATE` datetime DEFAULT NULL,
  PRIMARY KEY (`ADDRESS_ID`),
  UNIQUE KEY `USER_ID` (`USER_ID`),
  KEY `idx_user_id_addresses` (`USER_ID`),
  CONSTRAINT `user_addresses_ibfk_1` FOREIGN KEY (`USER_ID`) REFERENCES `USERS` (`USER_ID`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

/*Data for the table `USER_ADDRESSES` */

insert  into `USER_ADDRESSES`(`ADDRESS_ID`,`USER_ID`,`STREET`,`CITY`,`STATE`,`ZIPCODE`,`COUNTRY`,`CREATEDATE`,`UPDATEDATE`) values (1,1,'Ruby','Marilao','Bulacan','3019','Philippines','2025-02-25 10:09:36',NULL);

/*Table structure for table `USER_ROLES` */

CREATE TABLE `USER_ROLES` (
  `USER_ID` int(11) NOT NULL,
  `ROLE_ID` int(11) NOT NULL,
  PRIMARY KEY (`USER_ID`,`ROLE_ID`),
  KEY `ROLE_ID` (`ROLE_ID`),
  CONSTRAINT `user_roles_ibfk_1` FOREIGN KEY (`USER_ID`) REFERENCES `USERS` (`USER_ID`) ON DELETE CASCADE,
  CONSTRAINT `user_roles_ibfk_2` FOREIGN KEY (`ROLE_ID`) REFERENCES `ROLES` (`ROLE_ID`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

/*Data for the table `USER_ROLES` */

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;
