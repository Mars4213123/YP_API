CREATE DATABASE  IF NOT EXISTS `recipe_planner` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci */ /*!80016 DEFAULT ENCRYPTION='N' */;
USE `recipe_planner`;
-- MySQL dump 10.13  Distrib 8.0.41, for Win64 (x86_64)
--
-- Host: MySQL-8.2    Database: recipe_planner
-- ------------------------------------------------------
-- Server version	8.2.0

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `Ingredients`
--

DROP TABLE IF EXISTS `Ingredients`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Ingredients` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Name` varchar(100) COLLATE utf8mb4_unicode_ci NOT NULL,
  `Category` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL,
  `StandardUnit` varchar(20) COLLATE utf8mb4_unicode_ci NOT NULL,
  `Allergens` text COLLATE utf8mb4_unicode_ci,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_Ingredients_Name` (`Name`)
) ENGINE=InnoDB AUTO_INCREMENT=26 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `Ingredients`
--

LOCK TABLES `Ingredients` WRITE;
/*!40000 ALTER TABLE `Ingredients` DISABLE KEYS */;
INSERT INTO `Ingredients` VALUES (1,'Картофель','Овощи','кг',NULL),(2,'Морковь','Овощи','кг',NULL),(3,'Лук репчатый','Овощи','кг',NULL),(4,'Куриное филе','Мясо','кг',NULL),(5,'Рис','Крупы','кг','глютен'),(6,'Говядина','Мясо','кг',NULL),(7,'Помидоры','Овощи','кг',NULL),(8,'Огурцы','Овощи','кг',NULL),(9,'Яйца','Яйца','шт','яйца'),(10,'Молоко','Молочные продукты','л','молоко'),(11,'Мука пшеничная','Бакалея','кг','глютен'),(12,'Сахар','Бакалея','кг',NULL),(13,'Соль','Приправы','г',NULL),(14,'Перец черный','Приправы','г',NULL),(15,'Масло растительное','Масла','л',NULL),(16,'Сметана','Молочные продукты','г','молоко'),(17,'Гречка','Крупы','кг',NULL),(18,'Фарш мясной','Мясо','кг',NULL),(19,'Сыр твердый','Молочные продукты','г','молоко'),(20,'Чеснок','Овощи','г',NULL),(21,'Укроп','Зелень','г',NULL),(22,'Петрушка','Зелень','г',NULL),(23,'Томатная паста','Соусы','г',NULL),(24,'Лавровый лист','Приправы','шт',NULL),(25,'Вода','Напитки','л',NULL);
/*!40000 ALTER TABLE `Ingredients` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `MenuMeals`
--

DROP TABLE IF EXISTS `MenuMeals`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `MenuMeals` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `MenuId` int NOT NULL,
  `RecipeId` int NOT NULL,
  `MealDate` date NOT NULL,
  `MealType` int NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `MenuId` (`MenuId`),
  KEY `RecipeId` (`RecipeId`),
  CONSTRAINT `menumeals_ibfk_1` FOREIGN KEY (`MenuId`) REFERENCES `WeeklyMenus` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `menumeals_ibfk_2` FOREIGN KEY (`RecipeId`) REFERENCES `Recipes` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=137 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `MenuMeals`
--

LOCK TABLES `MenuMeals` WRITE;
/*!40000 ALTER TABLE `MenuMeals` DISABLE KEYS */;
INSERT INTO `MenuMeals` VALUES (74,12,1,'2025-12-25',1),(75,12,6,'2025-12-25',2),(76,12,6,'2025-12-25',3),(77,12,10,'2025-12-26',1),(78,12,8,'2025-12-26',2),(79,12,8,'2025-12-26',3),(80,12,5,'2025-12-27',1),(81,12,11,'2025-12-27',2),(82,12,1,'2025-12-27',3),(83,12,11,'2025-12-28',1),(84,12,3,'2025-12-28',2),(85,12,2,'2025-12-28',3),(86,12,4,'2025-12-29',1),(87,12,9,'2025-12-29',2),(88,12,11,'2025-12-29',3),(89,12,1,'2025-12-30',1),(90,12,7,'2025-12-30',2),(91,12,5,'2025-12-30',3),(92,12,10,'2025-12-31',1),(93,12,2,'2025-12-31',2),(94,12,7,'2025-12-31',3),(116,14,1,'2025-12-25',1),(117,14,8,'2025-12-25',2),(118,14,1,'2025-12-25',3),(119,14,10,'2025-12-26',1),(120,14,7,'2025-12-26',2),(121,14,9,'2025-12-26',3),(122,14,5,'2025-12-27',1),(123,14,9,'2025-12-27',2),(124,14,7,'2025-12-27',3),(125,14,4,'2025-12-28',1),(126,14,11,'2025-12-28',2),(127,14,2,'2025-12-28',3),(128,14,11,'2025-12-29',1),(129,14,2,'2025-12-29',2),(130,14,11,'2025-12-29',3),(131,14,1,'2025-12-30',1),(132,14,3,'2025-12-30',2),(133,14,3,'2025-12-30',3),(134,14,10,'2025-12-31',1),(135,14,6,'2025-12-31',2),(136,14,8,'2025-12-31',3);
/*!40000 ALTER TABLE `MenuMeals` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `RecipeIngredients`
--

DROP TABLE IF EXISTS `RecipeIngredients`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `RecipeIngredients` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `RecipeId` int NOT NULL,
  `IngredientId` int NOT NULL,
  `Quantity` decimal(10,3) NOT NULL,
  `Unit` varchar(20) COLLATE utf8mb4_unicode_ci NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `RecipeId` (`RecipeId`),
  KEY `IngredientId` (`IngredientId`),
  CONSTRAINT `recipeingredients_ibfk_1` FOREIGN KEY (`RecipeId`) REFERENCES `Recipes` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `recipeingredients_ibfk_2` FOREIGN KEY (`IngredientId`) REFERENCES `Ingredients` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=49 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `RecipeIngredients`
--

LOCK TABLES `RecipeIngredients` WRITE;
/*!40000 ALTER TABLE `RecipeIngredients` DISABLE KEYS */;
INSERT INTO `RecipeIngredients` VALUES (1,1,1,1.000,'кг'),(2,1,10,0.200,'л'),(3,1,12,10.000,'г'),(4,1,13,5.000,'г'),(5,2,4,0.500,'кг'),(6,2,1,0.300,'кг'),(7,2,2,0.200,'кг'),(8,2,3,0.100,'кг'),(9,2,13,10.000,'г'),(10,2,14,2.000,'г'),(11,3,1,0.800,'кг'),(12,3,3,0.200,'кг'),(13,3,15,0.050,'л'),(14,3,13,5.000,'г'),(15,4,17,0.300,'кг'),(16,4,25,0.600,'л'),(17,4,13,5.000,'г'),(18,5,9,4.000,'шт'),(19,5,10,0.100,'л'),(20,5,13,3.000,'г'),(21,5,15,20.000,'мл'),(22,6,7,0.400,'кг'),(23,6,8,0.300,'кг'),(24,6,3,0.100,'кг'),(25,6,15,30.000,'мл'),(26,6,13,5.000,'г'),(27,6,21,10.000,'г'),(28,7,6,0.800,'кг'),(29,7,3,0.150,'кг'),(30,7,11,0.100,'кг'),(31,7,10,0.050,'л'),(32,7,13,8.000,'г'),(33,7,14,3.000,'г'),(34,8,5,0.400,'кг'),(35,8,25,0.600,'л'),(36,8,13,5.000,'г'),(37,9,6,1.000,'кг'),(38,9,2,0.300,'кг'),(39,9,3,0.200,'кг'),(40,9,23,50.000,'г'),(41,9,24,2.000,'шт'),(42,9,13,10.000,'г'),(43,9,14,5.000,'г'),(44,10,9,6.000,'шт'),(45,10,10,0.500,'л'),(46,10,11,0.400,'кг'),(47,10,13,5.000,'г'),(48,10,12,30.000,'г');
/*!40000 ALTER TABLE `RecipeIngredients` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `Recipes`
--

DROP TABLE IF EXISTS `Recipes`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Recipes` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Title` varchar(200) COLLATE utf8mb4_unicode_ci NOT NULL,
  `Description` varchar(1000) COLLATE utf8mb4_unicode_ci DEFAULT '',
  `Instructions` text COLLATE utf8mb4_unicode_ci,
  `PrepTime` int NOT NULL,
  `CookTime` int NOT NULL,
  `Servings` int NOT NULL,
  `Calories` decimal(10,2) NOT NULL,
  `ImageUrl` varchar(500) COLLATE utf8mb4_unicode_ci DEFAULT '',
  `Tags` text COLLATE utf8mb4_unicode_ci,
  `Allergens` text COLLATE utf8mb4_unicode_ci,
  `CuisineType` varchar(50) COLLATE utf8mb4_unicode_ci DEFAULT '',
  `Difficulty` varchar(20) COLLATE utf8mb4_unicode_ci DEFAULT '',
  `CreatedAt` datetime NOT NULL,
  `IsPublic` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=13 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `Recipes`
--

LOCK TABLES `Recipes` WRITE;
/*!40000 ALTER TABLE `Recipes` DISABLE KEYS */;
INSERT INTO `Recipes` VALUES (1,'Картофельное пюре','Нежное картофельное пюре с молоком и маслом','1. Очистить картофель и нарезать кубиками\n2. Отварить картофель в подсоленной воде до готовности\n3. Слить воду, добавить горячее молоко и сливочное масло\n4. Размять картофель до однородной массы',15,25,4,250.50,'','breakfast,dinner,side','молоко','Русская','Легко','2025-12-25 12:08:54',1),(2,'Куриный суп','Ароматный куриный суп с овощами','1. Залить курицу водой и довести до кипения\n2. Добавить нарезанные овощи (картофель, морковь, лук)\n3. Варить на медленном огне 40 минут\n4. Добавить соль, перец и зелень',20,50,6,180.75,'','lunch,dinner,soup','','Русская','Легко','2025-12-25 12:08:54',1),(3,'Жареная картошка','Хрустящая жареная картошка с луком','1. Картофель очистить и нарезать брусочками\n2. Разогреть масло на сковороде\n3. Обжарить картофель до золотистой корочки\n4. Добавить лук и жарить еще 5 минут',10,20,3,320.00,'','lunch,dinner,side','','Русская','Легко','2025-12-25 12:08:54',1),(4,'Гречневая каша','Рассыпчатая гречневая каша','1. Перебрать и промыть гречку\n2. Обжарить гречку на сухой сковороде 2 минуты\n3. Залить водой в соотношении 1:2\n4. Варить под крышкой 20 минут',5,25,2,150.25,'','breakfast,side','','Русская','Легко','2025-12-25 12:08:54',1),(5,'Омлет','Пышный омлет с молоком','1. Взбить яйца с молоком и солью\n2. Разогреть масло на сковороде\n3. Вылить яичную смесь\n4. Готовить под крышкой 7-10 минут',5,10,2,210.50,'','breakfast,dinner','яйца,молоко','Европейская','Легко','2025-12-25 12:08:54',1),(6,'Салат из свежих овощей','Легкий салат из помидоров и огурцов','1. Нарезать помидоры и огурцы\n2. Добавить мелко нарезанный лук\n3. Заправить маслом и посолить\n4. Добавить зелень',15,0,4,80.00,'','lunch,dinner,salad','','Средиземноморская','Легко','2025-12-25 12:08:54',1),(7,'Котлеты из говядины','Сочные домашние котлеты','1. Приготовить фарш из говядины с луком\n2. Добавить хлеб, размоченный в молоке\n3. Сформировать котлеты\n4. Обжарить с двух сторон до готовности',20,25,5,280.75,'','lunch,dinner,main','молоко,глютен','Русская','Средне','2025-12-25 12:08:54',1),(8,'Рис отварной','Рассыпчатый отварной рис','1. Промыть рис до прозрачной воды\n2. Залить водой в соотношении 1:1.5\n3. Варить под крышкой 15 минут\n4. Дать постоять 10 минут',5,20,4,130.00,'','lunch,dinner,side','глютен','Азиатская','Легко','2025-12-25 12:08:54',1),(9,'Тушеная говядина','Нежная говядина, тушенная с овощами','1. Обжарить мясо до румяной корочки\n2. Добавить нарезанные овощи\n3. Залить водой и тушить 1.5 часа\n4. Добавить томатную пасту за 15 минут до готовности',30,90,6,350.25,'','lunch,dinner,main','','Русская','Сложно','2025-12-25 12:08:54',1),(10,'Блины','Тонкие блины на молоке','1. Смешать яйца, молоко, муку и соль\n2. Дать тесту постоять 30 минут\n3. Выпекать на разогретой сковороде\n4. Подавать со сметаной или вареньем',20,30,8,220.50,'','breakfast,dessert','молоко,яйца,глютен','Русская','Средне','2025-12-25 12:08:54',1),(11,'Тестовый рецепт','Тестовый рецепт','Тестовый рецепт',10,10,10,250.00,'','breakfast,lunch,dinner','','Русская','Легко','2025-12-25 07:30:40',1),(12,'Тестовый рецепт2','Тестовый рецепт2','Тестовый рецепт2',10,25,20,1200.00,'','','','','Легко','2025-12-25 08:23:49',1);
/*!40000 ALTER TABLE `Recipes` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `ShoppingListItems`
--

DROP TABLE IF EXISTS `ShoppingListItems`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `ShoppingListItems` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `ShoppingListId` int NOT NULL,
  `IngredientId` int NOT NULL,
  `Quantity` decimal(10,3) NOT NULL,
  `Unit` varchar(20) COLLATE utf8mb4_unicode_ci NOT NULL,
  `IsPurchased` tinyint(1) NOT NULL DEFAULT '0',
  `Category` varchar(50) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `ShoppingListId` (`ShoppingListId`),
  KEY `IngredientId` (`IngredientId`),
  CONSTRAINT `shoppinglistitems_ibfk_1` FOREIGN KEY (`ShoppingListId`) REFERENCES `ShoppingLists` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `shoppinglistitems_ibfk_2` FOREIGN KEY (`IngredientId`) REFERENCES `Ingredients` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=51 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `ShoppingListItems`
--

LOCK TABLES `ShoppingListItems` WRITE;
/*!40000 ALTER TABLE `ShoppingListItems` DISABLE KEYS */;
INSERT INTO `ShoppingListItems` VALUES (31,5,1,5.200,'кг',1,'Овощи'),(32,5,10,1.800,'л',0,'Молочные продукты'),(33,5,12,90.000,'г',0,'Бакалея'),(34,5,13,114.000,'г',0,'Приправы'),(35,5,5,0.800,'кг',0,'Крупы'),(36,5,25,1.800,'л',0,'Напитки'),(37,5,9,16.000,'шт',0,'Яйца'),(38,5,11,1.000,'кг',0,'Бакалея'),(39,5,6,3.600,'кг',0,'Мясо'),(40,5,3,1.400,'кг',0,'Овощи'),(41,5,14,20.000,'г',0,'Приправы'),(42,5,2,1.000,'кг',0,'Овощи'),(43,5,23,100.000,'г',0,'Соусы'),(44,5,24,4.000,'шт',0,'Приправы'),(45,5,15,50.100,'мл',0,'Масла'),(46,5,17,0.300,'кг',0,'Крупы'),(47,5,4,1.000,'кг',0,'Мясо'),(48,5,7,0.400,'кг',0,'Овощи'),(49,5,8,0.300,'кг',0,'Овощи'),(50,5,21,10.000,'г',0,'Зелень');
/*!40000 ALTER TABLE `ShoppingListItems` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `ShoppingLists`
--

DROP TABLE IF EXISTS `ShoppingLists`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `ShoppingLists` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `MenuId` int NOT NULL,
  `UserId` int NOT NULL,
  `Name` varchar(200) COLLATE utf8mb4_unicode_ci NOT NULL,
  `CreatedAt` datetime NOT NULL,
  `IsCompleted` tinyint(1) NOT NULL DEFAULT '0',
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_ShoppingLists_MenuId` (`MenuId`),
  KEY `UserId` (`UserId`),
  CONSTRAINT `shoppinglists_ibfk_1` FOREIGN KEY (`MenuId`) REFERENCES `WeeklyMenus` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `shoppinglists_ibfk_2` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `ShoppingLists`
--

LOCK TABLES `ShoppingLists` WRITE;
/*!40000 ALTER TABLE `ShoppingLists` DISABLE KEYS */;
INSERT INTO `ShoppingLists` VALUES (5,14,6,'Список покупок для Меню на 7 дней','2025-12-25 08:25:31',0);
/*!40000 ALTER TABLE `ShoppingLists` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `UserFavorites`
--

DROP TABLE IF EXISTS `UserFavorites`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `UserFavorites` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `UserId` int NOT NULL,
  `RecipeId` int NOT NULL,
  `AddedAt` datetime NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_UserFavorites_UserId_RecipeId` (`UserId`,`RecipeId`),
  KEY `RecipeId` (`RecipeId`),
  CONSTRAINT `userfavorites_ibfk_1` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `userfavorites_ibfk_2` FOREIGN KEY (`RecipeId`) REFERENCES `Recipes` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=11 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `UserFavorites`
--

LOCK TABLES `UserFavorites` WRITE;
/*!40000 ALTER TABLE `UserFavorites` DISABLE KEYS */;
INSERT INTO `UserFavorites` VALUES (8,5,11,'2025-12-25 07:31:39');
/*!40000 ALTER TABLE `UserFavorites` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `UserInventories`
--

DROP TABLE IF EXISTS `UserInventories`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `UserInventories` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `UserId` int NOT NULL,
  `IngredientId` int NOT NULL,
  `Quantity` decimal(10,3) NOT NULL,
  `Unit` varchar(20) COLLATE utf8mb4_unicode_ci NOT NULL,
  `ExpiryDate` date DEFAULT NULL,
  `AddedAt` datetime NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_UserInventories_UserId_IngredientId` (`UserId`,`IngredientId`),
  KEY `IngredientId` (`IngredientId`),
  CONSTRAINT `userinventories_ibfk_1` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `userinventories_ibfk_2` FOREIGN KEY (`IngredientId`) REFERENCES `Ingredients` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=14 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `UserInventories`
--

LOCK TABLES `UserInventories` WRITE;
/*!40000 ALTER TABLE `UserInventories` DISABLE KEYS */;
INSERT INTO `UserInventories` VALUES (11,5,10,5.000,'л',NULL,'2025-12-25 07:22:51'),(13,6,10,10.000,'л',NULL,'2025-12-25 08:21:30');
/*!40000 ALTER TABLE `UserInventories` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `Users`
--

DROP TABLE IF EXISTS `Users`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Users` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Username` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL,
  `Email` varchar(100) COLLATE utf8mb4_unicode_ci NOT NULL,
  `FullName` varchar(100) COLLATE utf8mb4_unicode_ci NOT NULL,
  `PasswordHash` longblob NOT NULL,
  `PasswordSalt` longblob NOT NULL,
  `CreatedAt` datetime NOT NULL,
  `Allergies` text COLLATE utf8mb4_unicode_ci,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_Users_Username` (`Username`),
  UNIQUE KEY `IX_Users_Email` (`Email`)
) ENGINE=InnoDB AUTO_INCREMENT=7 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `Users`
--

LOCK TABLES `Users` WRITE;
/*!40000 ALTER TABLE `Users` DISABLE KEYS */;
INSERT INTO `Users` VALUES (5,'asd','asd@gmail.com','asd',_binary '�O�+^7\r�����Q\0h]�%��pN~�t\�@V��&�PP\�\��\�\�<٥\�\�1\��U�E',_binary '*��f\\\�\�Aп3�$Ώ��Qr��z\�\�%\�5�a\�Pu�Y{\��\n�\�|�V\�\��7{��\�\�Q��\\�gčik7x{\�\Z\"�\�\�V�0.�s���\��O20L\"\��:�\�\"�\\*�p�>�m��/�C\�swt-\�Z,C��','2025-12-25 07:15:01','Молоко,Рыба'),(6,'test1','asdasd@gmail.com','asdasd',_binary '��\�\�\�\�[�wK\'V�\0ߥ���\�\�%F6\�%.��\�u\\\�,H�\r\�u�s��gtN\��8]pχZ�\�',_binary 'ۛ�Ge\\\���\\�\�\�\�G\"O��\�v9�V�o)@1\�;\\��}r\Z��H\�\�ZuHl�4�\�v�<\'�\�Z.\�\�\�|�1���\��g-\�@2�G\��MS�waph�AHY\�\�lI�H�`\�\Z&�1zL\�-wl%~�6��','2025-12-25 08:20:15','Яйца,Рыба');
/*!40000 ALTER TABLE `Users` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `WeeklyMenus`
--

DROP TABLE IF EXISTS `WeeklyMenus`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `WeeklyMenus` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `UserId` int NOT NULL,
  `Name` varchar(200) COLLATE utf8mb4_unicode_ci NOT NULL,
  `StartDate` date NOT NULL,
  `EndDate` date NOT NULL,
  `TotalCalories` decimal(10,2) NOT NULL,
  `CreatedAt` datetime NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `UserId` (`UserId`),
  CONSTRAINT `weeklymenus_ibfk_1` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=15 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `WeeklyMenus`
--

LOCK TABLES `WeeklyMenus` WRITE;
/*!40000 ALTER TABLE `WeeklyMenus` DISABLE KEYS */;
INSERT INTO `WeeklyMenus` VALUES (12,5,'Меню на 7 дней','2025-12-25','2025-12-31',4527.00,'2025-12-25 08:13:23'),(14,6,'Меню на 7 дней','2025-12-25','2025-12-31',4906.75,'2025-12-25 08:25:20');
/*!40000 ALTER TABLE `WeeklyMenus` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-12-25 13:28:43
