CREATE DATABASE IF NOT EXISTS `recipe_planner` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci */ /*!80016 DEFAULT ENCRYPTION='N' */;
USE `recipe_planner`;

-- MySQL dump 10.13  Distrib 8.0.40, for Win64 (x86_64)
-- Host: 127.0.0.1    Database: recipe_planner
-- ------------------------------------------------------
-- Server version	8.0.30

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40101 SET TIME_ZONE='+00:00' */;

/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

-- Table structure for table `Ingredients`

DROP TABLE IF EXISTS `Ingredients`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Ingredients` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `Category` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `StandardUnit` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `Allergens` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `Name` (`Name`),
  KEY `IX_Ingredients_Name` (`Name`),
  KEY `IX_Ingredients_Category` (`Category`)
) ENGINE=InnoDB AUTO_INCREMENT=11 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Fixed data for Ingredients (with correct Russian encoding)
LOCK TABLES `Ingredients` WRITE;
/*!40000 ALTER TABLE `Ingredients` DISABLE KEYS */;
INSERT INTO `Ingredients` VALUES 
(1,'Куриная грудка','Мясо','г',''),
(2,'Рис','Крупы','г',''),
(3,'Помидоры','Овощи','шт',''),
(4,'Лук','Овощи','шт',''),
(5,'Яйца','Молочные','шт','яйца'),
(6,'Молоко','Молочные','мл','лактозa'),
(7,'Мука','Бакалея','г','глютен'),
(8,'Сахар','Бакалея','г',''),
(9,'Соль','Специи','г',''),
(10,'Перец','Специи','г','');
/*!40000 ALTER TABLE `Ingredients` ENABLE KEYS */;
UNLOCK TABLES;

-- Table structure for table `Recipes`

DROP TABLE IF EXISTS `Recipes`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Recipes` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Title` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `Description` varchar(1000) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `Instructions` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
  `PrepTime` int NOT NULL DEFAULT '0',
  `CookTime` int NOT NULL DEFAULT '0',
  `Servings` int NOT NULL DEFAULT '1',
  `Calories` decimal(10,2) NOT NULL DEFAULT '0.00',
  `ImageUrl` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `Tags` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
  `Allergens` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
  `CuisineType` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `Difficulty` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `IsPublic` tinyint(1) NOT NULL DEFAULT '1',
  `CreatedAt` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  KEY `IX_Recipes_Title` (`Title`),
  KEY `IX_Recipes_CuisineType` (`CuisineType`)
) ENGINE=InnoDB AUTO_INCREMENT=11 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Fixed data for Recipes (correct Russian encoding)
LOCK TABLES `Recipes` WRITE;
/*!40000 ALTER TABLE `Recipes` DISABLE KEYS */;
INSERT INTO `Recipes` VALUES 
(2,'Курица с рисом','Простое блюдо из курицы и риса','1. Обжарить курицу\n2. Сварить рис\n3. Подавать вместе',15,30,4,450.00,'','lunch,dinner,main','','russian','Easy',1,'2025-11-01 07:29:52'),
(3,'Омлет','Легкий завтрак','1. Взбить яйца с молоком\n2. Жарить на сковороде',5,10,2,250.00,'','breakfast,quick','яйца,лактоза','russian','Easy',1,'2025-11-01 07:29:52'),
(4,'Салат из помидоров','Свежий овощной салат','1. Нарезать помидоры и лук\n2. Заправить маслом',10,0,2,150.00,'','lunch,salad,vegetarian','','mediterranean','Easy',1,'2025-11-01 07:29:52'),
(5,'Тестовый рецепт 1','Описание тестового рецепта 1','Инструкции для тестового рецепта 1',15,30,4,350.00,'','testtag1,lunch,main','','italian','hard',1,'2025-11-01 08:39:33'),
(6,'Тестовый рецепт 2','Описание тестового рецепта 2','Инструкции для тестового рецепта 2',10,20,2,250.00,'','testtag2,breakfast,quick','яйца','russian','medium',1,'2025-11-01 08:39:33'),
(7,'Тестовый рецепт 3','Описание тестового рецепта 3','Инструкции для тестового рецепта 3',20,40,6,500.00,'','testtag1,testtag2,dinner','глютен,лактоза','french','hard',1,'2025-11-01 08:39:33'),
(8,'Паста Карбонара','Классическая итальянская паста','1. Сварить пасту\n2. Обжарить бекон\n3. Смешать с соусом',10,15,2,450.00,'','lunch,dinner,italian','яйца,глютен','italian','medium',1,'2025-11-01 08:39:33'),
(9,'Овсяная каша','Полезный завтрак','1. Сварить овсянку с молоком\n2. Добавить фрукты',5,10,1,200.00,'','breakfast,healthy','лактоза','russian','easy',1,'2025-11-01 08:39:33'),
(10,'Спагетти Карбонара','Классическая итальянская паста с беконом и сыром','1. Сварить спагетти аль денте\n2. Обжарить бекон до хрустящей корочки\n3. Взбить яйца с пармезаном\n4. Смешать все ингредиенты',10,15,2,550.00,'','dinner,italian,pasta,main','яйца,глютен,лактоза','italian','medium',1,'2025-11-01 05:41:32');
/*!40000 ALTER TABLE `Recipes` ENABLE KEYS */;
UNLOCK TABLES;

-- Остальные таблицы (упрощённо, с исправленной кодировкой)

-- MenuMeals (без текста)
DROP TABLE IF EXISTS `MenuMeals`;
CREATE TABLE `MenuMeals` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `MenuId` int NOT NULL,
  `RecipeId` int NOT NULL,
  `MealDate` date NOT NULL,
  `MealType` int NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `MenuId` (`MenuId`),
  KEY `RecipeId` (`RecipeId`),
  KEY `IX_MenuMeals_Date` (`MealDate`),
  CONSTRAINT `menumeals_ibfk_1` FOREIGN KEY (`MenuId`) REFERENCES `WeeklyMenus` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `menumeals_ibfk_2` FOREIGN KEY (`RecipeId`) REFERENCES `Recipes` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=14 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

INSERT INTO `MenuMeals` VALUES 
(2,2,3,'2025-11-01',1),(3,2,2,'2025-11-01',2),(4,2,4,'2025-11-02',2),(5,3,3,'2025-11-01',1),(6,3,4,'2025-11-01',2),(7,3,2,'2025-11-01',3),(8,4,4,'2025-11-01',2),(9,5,4,'2025-11-01',2),(10,6,2,'2025-11-01',2),(11,7,4,'2025-11-01',2),(12,8,9,'2025-11-05',1),(13,8,4,'2025-11-05',2);

-- RecipeIngredients (без текста)
DROP TABLE IF EXISTS `RecipeIngredients`;
CREATE TABLE `RecipeIngredients` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `RecipeId` int NOT NULL,
  `IngredientId` int NOT NULL,
  `Quantity` decimal(10,3) NOT NULL,
  `Unit` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `RecipeId` (`RecipeId`),
  KEY `IngredientId` (`IngredientId`),
  CONSTRAINT `recipeingredients_ibfk_1` FOREIGN KEY (`RecipeId`) REFERENCES `Recipes` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `recipeingredients_ibfk_2` FOREIGN KEY (`IngredientId`) REFERENCES `Ingredients` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=32 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

INSERT INTO `RecipeIngredients` VALUES 
(13,2,1,500.000,'г'),(14,2,2,300.000,'г'),(15,3,5,4.000,'шт'),(16,3,6,100.000,'мл'),(17,4,3,4.000,'шт'),(18,4,4,1.000,'шт'),(19,5,1,200.000,'г'),(20,5,2,150.000,'г'),(21,6,5,2.000,'шт'),(22,6,6,50.000,'мл'),(23,7,1,300.000,'г'),(24,7,3,3.000,'шт'),(25,8,2,200.000,'г'),(26,8,7,100.000,'г'),(27,9,2,100.000,'г'),(28,9,6,200.000,'мл'),(29,10,2,200.000,'г'),(30,10,5,3.000,'шт'),(31,10,6,50.000,'мл');

-- ShoppingListItems
DROP TABLE IF EXISTS `ShoppingListItems`;
CREATE TABLE `ShoppingListItems` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `ShoppingListId` int NOT NULL,
  `IngredientId` int NOT NULL,
  `Quantity` decimal(10,3) NOT NULL,
  `Unit` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `Category` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `IsPurchased` tinyint(1) NOT NULL DEFAULT '0',
  PRIMARY KEY (`Id`),
  KEY `ShoppingListId` (`ShoppingListId`),
  KEY `IngredientId` (`IngredientId`),
  KEY `IX_ShoppingListItems_Purchased` (`IsPurchased`),
  CONSTRAINT `shoppinglistitems_ibfk_1` FOREIGN KEY (`ShoppingListId`) REFERENCES `ShoppingLists` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `shoppinglistitems_ibfk_2` FOREIGN KEY (`IngredientId`) REFERENCES `Ingredients` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

INSERT INTO `ShoppingListItems` VALUES 
(1,1,1,1.000,'шт','Мясо',1),(2,2,1,1.000,'шт','Мясо',0);

-- ShoppingLists
DROP TABLE IF EXISTS `ShoppingLists`;
CREATE TABLE `ShoppingLists` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `MenuId` int NOT NULL,
  `UserId` int DEFAULT NULL,
  `Name` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `CreatedAt` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `IsCompleted` tinyint(1) NOT NULL DEFAULT '0',
  PRIMARY KEY (`Id`),
  KEY `UserId` (`UserId`),
  CONSTRAINT `shoppinglists_ibfk_1` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

INSERT INTO `ShoppingLists` VALUES 
(1,7,NULL,'Shopping List 112327','2025-11-01 06:23:27',0),(2,7,NULL,'Shopping List 152004','2025-11-05 10:20:04',0);

-- UserFavorites (без текста)
DROP TABLE IF EXISTS `UserFavorites`;
CREATE TABLE `UserFavorites` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `UserId` int NOT NULL,
  `RecipeId` int NOT NULL,
  `AddedAt` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `UX_UserFavorites_UserRecipe` (`UserId`,`RecipeId`),
  KEY `RecipeId` (`RecipeId`),
  KEY `IX_UserFavorites_UserId` (`UserId`),
  CONSTRAINT `userfavorites_ibfk_1` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `userfavorites_ibfk_2` FOREIGN KEY (`RecipeId`) REFERENCES `Recipes` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=10 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

INSERT INTO `UserFavorites` VALUES 
(1,1,10,'2025-11-01 05:41:59'),(7,1,7,'2025-11-05 11:04:24'),(9,1,2,'2025-11-05 12:08:08');

-- UserInventories (без текста)
DROP TABLE IF EXISTS `UserInventories`;
CREATE TABLE `UserInventories` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `UserId` int NOT NULL,
  `IngredientId` int NOT NULL,
  `Quantity` decimal(10,3) NOT NULL,
  `Unit` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `ExpiryDate` date DEFAULT NULL,
  `AddedAt` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `UX_UserInventories_UserIngredient` (`UserId`,`IngredientId`),
  KEY `IngredientId` (`IngredientId`),
  CONSTRAINT `userinventories_ibfk_1` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `userinventories_ibfk_2` FOREIGN KEY (`IngredientId`) REFERENCES `Ingredients` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Users (исправленная аллергия)
DROP TABLE IF EXISTS `Users`;
CREATE TABLE `Users` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Username` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `Email` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `FullName` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `PasswordHash` longblob NOT NULL,
  `PasswordSalt` longblob NOT NULL,
  `Allergies` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
  `CreatedAt` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `Username` (`Username`),
  UNIQUE KEY `Email` (`Email`),
  KEY `IX_Users_Email` (`Email`),
  KEY `IX_Users_Username` (`Username`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

INSERT INTO `Users` VALUES 
(1,'asd','asd','asd',_binary '@x\u00E9
sw&“ДyШ*Я5И©нtЭYûñ¢Ж2–ІАІсwЯ@BrGXQ…4Ш-EњП!ШП3RК3Я¬',_binary 'kњ~@MшРАвнДЖшHX‹>_п»ЩЖO ¤tЕ«!N¶ИЕ~pFычКooЮр~UЯsДНwяlКä¢"! КВ±ыКИ\<Т+<!4\'LШt)6ѝ”Кл 8VGBЇњлg‹ 38-\u0430RЭYі{C¶~€КLN>Й+ ќLдл·','asd','2025-11-01 04:08:50');

-- WeeklyMenus
DROP TABLE IF EXISTS `WeeklyMenus`;
CREATE TABLE `WeeklyMenus` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `UserId` int NOT NULL,
  `Name` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `StartDate` date NOT NULL,
  `EndDate` date NOT NULL,
  `TotalCalories` decimal(10,2) NOT NULL DEFAULT '0.00',
  `CreatedAt` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  KEY `IX_WeeklyMenus_UserId` (`UserId`),
  KEY `IX_WeeklyMenus_DateRange` (`StartDate`,`EndDate`),
  CONSTRAINT `weeklymenus_ibfk_1` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=9 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

INSERT INTO `WeeklyMenus` VALUES 
(2,1,'Menu Plan 2025-11-01','2025-11-01','2025-11-03',850.00,'2025-11-01 04:44:30'),
(3,1,'Menu Plan 2025-11-01','2025-11-01','2025-11-03',850.00,'2025-11-01 04:46:53'),
(4,1,'Menu Plan 2025-11-01','2025-11-01','2025-11-01',150.00,'2025-11-01 04:49:54'),
(5,1,'Menu Plan 2025-11-01','2025-11-01','2025-11-01',150.00,'2025-11-01 04:52:32'),
(6,1,'Menu Plan 2025-11-01','2025-11-01','2025-11-01',450.00,'2025-11-01 05:00:47'),
(7,1,'Menu Plan 2025-11-01','2025-11-01','2025-11-01',150.00,'2025-11-01 05:04:20'),
(8,1,'Menu Plan 2025-11-05','2025-11-05','2025-11-11',350.00,'2025-11-05 10:57:29');

/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-11-13 with fixed UTF-8 encoding for all Russian text