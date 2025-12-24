CREATE DATABASE  IF NOT EXISTS `recipe_planner` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci */ /*!80016 DEFAULT ENCRYPTION='N' */;
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
-- Table structure for table `ingredients`
--

DROP TABLE IF EXISTS `ingredients`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `ingredients` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Category` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Unit` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_ingredients_Name` (`Name`)
) ENGINE=InnoDB AUTO_INCREMENT=9 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `ingredients`
--

LOCK TABLES `ingredients` WRITE;
/*!40000 ALTER TABLE `ingredients` DISABLE KEYS */;
INSERT INTO `ingredients` VALUES (4,'Куриное филе','Мясо','г'),(5,'Рис','Крупы','г'),(6,'Молоко','Молочные продукты','мл'),(7,'Яйца','Яйца','шт'),(8,'Картофель','Овощи','кг');
/*!40000 ALTER TABLE `ingredients` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `menu_items`
--

DROP TABLE IF EXISTS `menu_items`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `menu_items` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `MenuId` int NOT NULL,
  `RecipeId` int NOT NULL,
  `Date` datetime(6) NOT NULL,
  `MealType` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_menu_items_MenuId` (`MenuId`),
  KEY `IX_menu_items_RecipeId` (`RecipeId`),
  CONSTRAINT `FK_menu_items_menus_MenuId` FOREIGN KEY (`MenuId`) REFERENCES `menus` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_menu_items_recipes_RecipeId` FOREIGN KEY (`RecipeId`) REFERENCES `recipes` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=14 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `menu_items`
--

LOCK TABLES `menu_items` WRITE;
/*!40000 ALTER TABLE `menu_items` DISABLE KEYS */;
INSERT INTO `menu_items` VALUES (1,1,1,'2025-12-25 15:57:35.000000','Обед'),(2,1,2,'2025-12-25 15:57:35.000000','Ужин'),(3,1,3,'2025-12-26 15:57:35.000000','Завтрак'),(4,1,4,'2025-12-26 15:57:35.000000','Обед'),(5,1,5,'2025-12-27 15:57:35.000000','Ужин'),(6,2,1,'2025-12-25 15:57:35.000000','Обед'),(7,2,1,'2025-12-26 15:57:35.000000','Обед'),(8,2,5,'2025-12-27 15:57:35.000000','Ужин'),(9,4,1,'2025-12-25 15:57:35.000000','Обед'),(10,4,2,'2025-12-25 15:57:35.000000','Ужин'),(11,4,3,'2025-12-26 15:57:35.000000','Завтрак'),(12,4,4,'2025-12-26 15:57:35.000000','Обед'),(13,4,5,'2025-12-27 15:57:35.000000','Ужин');
/*!40000 ALTER TABLE `menu_items` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `menus`
--

DROP TABLE IF EXISTS `menus`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `menus` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `UserId` int NOT NULL,
  `Name` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `CreatedAt` datetime(6) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_menus_UserId` (`UserId`),
  CONSTRAINT `FK_menus_users_UserId` FOREIGN KEY (`UserId`) REFERENCES `users` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `menus`
--

LOCK TABLES `menus` WRITE;
/*!40000 ALTER TABLE `menus` DISABLE KEYS */;
INSERT INTO `menus` VALUES (1,1,'Сбалансированное питание на неделю','2025-12-24 15:57:35.000000'),(2,1,'Вегетарианское меню','2025-12-23 15:57:35.000000'),(3,1,'Быстрые ужины','2025-12-22 15:57:35.000000'),(4,1,'Сбалансированное питание на неделю (Выбрано)','2025-12-24 10:58:19.746903');
/*!40000 ALTER TABLE `menus` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `recipe_ingredients`
--

DROP TABLE IF EXISTS `recipe_ingredients`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `recipe_ingredients` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `RecipeId` int NOT NULL,
  `IngredientId` int NOT NULL,
  `Quantity` decimal(65,30) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_recipe_ingredients_IngredientId` (`IngredientId`),
  KEY `IX_recipe_ingredients_RecipeId` (`RecipeId`),
  CONSTRAINT `FK_recipe_ingredients_ingredients_IngredientId` FOREIGN KEY (`IngredientId`) REFERENCES `ingredients` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_recipe_ingredients_recipes_RecipeId` FOREIGN KEY (`RecipeId`) REFERENCES `recipes` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=21 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `recipe_ingredients`
--

LOCK TABLES `recipe_ingredients` WRITE;
/*!40000 ALTER TABLE `recipe_ingredients` DISABLE KEYS */;
INSERT INTO `recipe_ingredients` VALUES (15,2,4,300.000000000000000000000000000000),(16,3,5,200.000000000000000000000000000000),(17,4,7,3.000000000000000000000000000000),(18,4,6,50.000000000000000000000000000000),(19,5,8,1.000000000000000000000000000000),(20,5,6,100.000000000000000000000000000000);
/*!40000 ALTER TABLE `recipe_ingredients` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `recipes`
--

DROP TABLE IF EXISTS `recipes`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `recipes` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Title` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Description` varchar(1000) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Instructions` text CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Calories` decimal(10,2) NOT NULL,
  `ImageUrl` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `PrepTime` int NOT NULL,
  `CookTime` int NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `recipes`
--

LOCK TABLES `recipes` WRITE;
/*!40000 ALTER TABLE `recipes` DISABLE KEYS */;
INSERT INTO `recipes` VALUES (1,'Салат из свежих овощей','Легкий и полезный салат','1. Помыть овощи\n2. Нарезать кубиками\n3. Заправить маслом\n4. Посолить по вкусу',150.00,'',10,0),(2,'Курица с овощами','Диетическое блюдо','1. Нарезать курицу\n2. Обжарить с луком\n3. Добавить овощи\n4. Тушить 20 минут',350.00,'',15,30),(3,'Рис отварной','Простой гарнир','1. Промыть рис\n2. Залить водой 1:2\n3. Варить 20 минут\n4. Дать настояться',200.00,'',5,20),(4,'Омлет','Классический завтрак','1. Взбить яйца с молоком\n2. Посолить\n3. Вылить на сковороду\n4. Жарить 5-7 минут',250.00,'',5,10),(5,'Картофель пюре','Традиционный гарнир','1. Отварить картофель\n2. Слить воду\n3. Размять с молоком\n4. Добавить масло',300.00,'',10,25);
/*!40000 ALTER TABLE `recipes` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `shopping_list_items`
--

DROP TABLE IF EXISTS `shopping_list_items`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `shopping_list_items` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `ShoppingListId` int NOT NULL,
  `Name` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Quantity` decimal(65,30) NOT NULL,
  `Unit` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `IsPurchased` tinyint(1) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_shopping_list_items_ShoppingListId` (`ShoppingListId`),
  CONSTRAINT `FK_shopping_list_items_shopping_lists_ShoppingListId` FOREIGN KEY (`ShoppingListId`) REFERENCES `shopping_lists` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `shopping_list_items`
--

LOCK TABLES `shopping_list_items` WRITE;
/*!40000 ALTER TABLE `shopping_list_items` DISABLE KEYS */;
INSERT INTO `shopping_list_items` VALUES (1,1,'Куриное филе',300.000000000000000000000000000000,'г',1),(2,1,'Рис',200.000000000000000000000000000000,'г',0),(3,1,'Молоко',150.000000000000000000000000000000,'мл',0),(4,1,'Яйца',3.000000000000000000000000000000,'шт',0),(5,1,'Картофель',1.000000000000000000000000000000,'кг',0);
/*!40000 ALTER TABLE `shopping_list_items` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `shopping_lists`
--

DROP TABLE IF EXISTS `shopping_lists`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `shopping_lists` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `UserId` int NOT NULL,
  `Name` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `IsCompleted` tinyint(1) NOT NULL,
  `CreatedAt` datetime(6) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_shopping_lists_UserId` (`UserId`),
  CONSTRAINT `FK_shopping_lists_users_UserId` FOREIGN KEY (`UserId`) REFERENCES `users` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `shopping_lists`
--

LOCK TABLES `shopping_lists` WRITE;
/*!40000 ALTER TABLE `shopping_lists` DISABLE KEYS */;
INSERT INTO `shopping_lists` VALUES (1,1,'Список для Сбалансированное питание на неделю',0,'2025-12-24 10:59:35.324866');
/*!40000 ALTER TABLE `shopping_lists` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `user_favorites`
--

DROP TABLE IF EXISTS `user_favorites`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `user_favorites` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `UserId` int NOT NULL,
  `RecipeId` int NOT NULL,
  `AddedAt` datetime(6) NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_user_favorites_UserId_RecipeId` (`UserId`,`RecipeId`),
  KEY `IX_user_favorites_RecipeId` (`RecipeId`),
  CONSTRAINT `FK_user_favorites_recipes_RecipeId` FOREIGN KEY (`RecipeId`) REFERENCES `recipes` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_user_favorites_users_UserId` FOREIGN KEY (`UserId`) REFERENCES `users` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `user_favorites`
--

LOCK TABLES `user_favorites` WRITE;
/*!40000 ALTER TABLE `user_favorites` DISABLE KEYS */;
INSERT INTO `user_favorites` VALUES (1,1,1,'2025-12-24 10:59:17.000458');
/*!40000 ALTER TABLE `user_favorites` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `users`
--

DROP TABLE IF EXISTS `users`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `users` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Username` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Password` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Email` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_users_Username` (`Username`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `users`
--

LOCK TABLES `users` WRITE;
/*!40000 ALTER TABLE `users` DISABLE KEYS */;
INSERT INTO `users` VALUES (1,'asd','asd','asd@gmail.com');
/*!40000 ALTER TABLE `users` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-12-24 16:50:07
