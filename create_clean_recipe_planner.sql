-- Complete SQL Script to Create Clean recipe_planner Database with UTF-8 Encoding
-- Run this in MySQL Workbench or mysql command line
-- This will DROP existing database and create a clean one with proper Russian text

-- Step 1: Drop and create database with correct encoding
DROP DATABASE IF EXISTS recipe_planner;
CREATE DATABASE recipe_planner
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

USE recipe_planner;

-- Step 2: Create Users table
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
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Insert test user
INSERT INTO `Users` VALUES 
(1,'testuser','test@example.com','Test User',X'48617368656450617373776f7264',X'53616C7456616C7565','глютен,лактоза','2025-11-13 10:00:00');

-- Step 3: Create Ingredients table
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
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Insert Russian ingredients with correct UTF-8
INSERT INTO `Ingredients` VALUES 
(1,'Куриная грудка','Мясо','г',''),
(2,'Рис','Крупы','г',''),
(3,'Помидоры','Овощи','шт',''),
(4,'Лук','Овощи','шт',''),
(5,'Яйца','Молочные','шт','яйца,лактоза'),
(6,'Молоко','Молочные','мл','лактоза'),
(7,'Мука','Бакалея','г','глютен'),
(8,'Сахар','Бакалея','г',''),
(9,'Соль','Специи','г',''),
(10,'Перец','Специи','г',''),
(11,'Овсянка','Крупы','г',''),
(12,'Бекон','Мясо','г',''),
(13,'Пармезан','Сыр','г','лактоза'),
(14,'Оливковое масло','Масла','мл',''),
(15,'Чеснок','Овощи','зубчик',''),
(16,'Фрукты','Фрукты','г',''),
(17,'Спагетти','Паста','г','глютен'),
(18,'Крем','Молочные','мл','лактоза');

-- Step 4: Create Recipes table
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
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Insert Russian recipes with correct UTF-8 encoding
INSERT INTO `Recipes` VALUES 
(1,'Курица с рисом','Простое блюдо из курицы и риса','1. Обжарить куриную грудку на сковороде до золотистой корочки (10 минут)\n2. Сварить рис в подсоленной воде до готовности (15 минут)\n3. Смешать курицу с рисом, добавить специи по вкусу\n4. Подавать горячим с овощным салатом',15,30,4,450.00,'https://example.com/chicken_rice.jpg','lunch,dinner,main,simple','яйца,лактоза','russian','easy',1,NOW()),
(2,'Омлет с овощами','Легкий и быстрый завтрак','1. Взбить 2-3 яйца с небольшим количеством молока\n2. Нарезать помидоры, лук и другие овощи\n3. Разогреть сковороду с маслом\n4. Вылить яичную смесь и добавить овощи\n5. Жарить на среднем огне 5-7 минут до готовности',5,10,2,250.00,'https://example.com/omelet.jpg','breakfast,quick,vegetarian','яйца,лактоза','russian','easy',1,NOW()),
(3,'Салат из помидоров','Свежий овощной салат','1. Нарезать помидоры, огурцы и лук тонкими ломтиками\n2. Добавить нарезанную зелень (укроп, петрушка)\n3. Заправить оливковым маслом, солью и перцем\n4. Перемешать и дать настояться 5 минут',10,0,2,150.00,'https://example.com/tomato_salad.jpg','lunch,salad,vegetarian,quick','Нет аллергенов','mediterranean','easy',1,NOW()),
(4,'Паста Карбонара','Классическая итальянская паста','1. Сварить спагетти аль денте (8-10 минут)\n2. Обжарить бекон или панчетту до хрустящей корочки\n3. Взбить яйца с пармезаном и сливками\n4. Смешать горячие спагетти с беконом и яичной смесью\n5. Посыпать дополнительным сыром',10,15,2,550.00,'https://example.com/carbonara.jpg','dinner,italian,pasta,main','яйца,глютен,лактоза','italian','medium',1,NOW()),
(5,'Овсяная каша','Полезный завтрак на каждый день','1. Вскипятить молоко в кастрюле\n2. Добавить овсяные хлопья (пропорция 1:3)\n3. Варить на медленном огне 5 минут, помешивая\n4. Добавить сахар, соль и масло по вкусу\n5. Подавать с фруктами или орехами',5,10,1,200.00,'https://example.com/oatmeal.jpg','breakfast,healthy,quick','лактоза','russian','easy',1,NOW()),
(6,'Борщ украинский','Традиционный суп','1. Обжарить лук, морковь и свеклу\n2. Добавить томатную пасту и тушить 5 минут\n3. В кипящий бульон добавить зажарку\n4. Добавить картофель, капусту, фасоль\n5. Варить 30-40 минут, добавить специи',20,40,6,350.00,'https://example.com/borscht.jpg','lunch,soup,traditional','Нет аллергенов','ukrainian','medium',1,NOW()),
(7,'Плов узбекский','Ароматное блюдо из риса и мяса','1. Обжарить мясо с луком и морковью\n2. Добавить специи (зира, барбарис, шафран)\n3. Засыпать рис и залить бульоном (1:2)\n4. Тушить под крышкой 20-25 минут\n5. Дать настояться 10 минут',30,40,6,600.00,'https://example.com/plov.jpg','dinner,main,rice,traditional','Нет аллергенов','uzbek','medium',1,NOW()),
(8,'Щи с капустой','Русский суп','1. Обжарить лук и морковь\n2. Добавить томатную пасту и потушить\n3. В кипящий бульон добавить картофель\n4. Добавить капусту и зажарку\n5. Варить 20 минут, добавить зелень',15,25,4,200.00,'https://example.com/shchi.jpg','lunch,soup,vegetarian','Нет аллергенов','russian','easy',1,NOW()),
(9,'Пельмени домашние','Сытное блюдо','1. Смешать фарш с луком и специями\n2. Замесить тесто из муки и воды\n3. Начинить тесто фаршем\n4. Сварить пельмени в подсоленной воде\n5. Подавать со сметаной',40,10,4,500.00,'https://example.com/pelmeni.jpg','dinner,main,traditional','глютен','russian','hard',1,NOW()),
(10,'Блины с начинкой','Универсальное блюдо','1. Взбить яйца с молоком и сахаром\n2. Добавить муку и соль, перемешать\n3. Жарить тонкие блины на сковороде\n4. Начинка: творог с сахаром или мясо\n5. Сложить и запекать 5 минут',15,20,4,300.00,'https://example.com/blini.jpg','breakfast,dinner,dessert','яйца,глютен,лактоза','russian','medium',1,NOW());

-- Step 5: Create RecipeIngredients table
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
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Insert recipe ingredients
INSERT INTO `RecipeIngredients` VALUES 
-- Recipe 1: Курица с рисом
(1,1,1,300.000,'г'),
(2,1,2,200.000,'г'),
(3,1,9,5.000,'г'),
(4,1,10,3.000,'г'),
(5,1,14,20.000,'мл'),
-- Recipe 2: Омлет
(6,2,5,3.000,'шт'),
(7,2,6,50.000,'мл'),
(8,2,3,2.000,'шт'),
(9,2,4,1.000,'шт'),
(10,2,9,2.000,'г'),
-- Recipe 3: Салат
(11,3,3,3.000,'шт'),
(12,3,2,1.000,'шт'),
(13,3,14,15.000,'мл'),
(14,3,9,2.000,'г'),
(15,3,10,1.000,'г'),
-- Recipe 4: Паста Карбонара
(16,4,17,200.000,'г'),
(17,4,12,100.000,'г'),
(18,4,5,2.000,'шт'),
(19,4,13,50.000,'г'),
(20,4,6,100.000,'мл'),
-- Recipe 5: Овсяная каша
(21,5,11,50.000,'г'),
(22,5,6,200.000,'мл'),
(23,5,8,20.000,'г'),
(24,5,9,1.000,'г'),
-- Recipe 6: Борщ
(25,6,1,400.000,'г'),
(26,6,3,3.000,'шт'),
(27,6,4,2.000,'шт'),
(28,6,2,150.000,'г'),
(29,6,9,10.000,'г'),
-- Recipe 7: Плов
(30,7,1,500.000,'г'),
(31,7,2,300.000,'г'),
(32,7,4,2.000,'шт'),
(33,7,9,8.000,'г'),
(34,7,10,5.000,'г'),
-- Recipe 8: Щи
(35,8,3,2.000,'шт'),
(36,8,2,200.000,'г'),
(37,8,4,1.000,'шт'),
(38,8,9,5.000,'г'),
-- Recipe 9: Пельмени
(39,9,1,300.000,'г'),
(40,9,7,200.000,'г'),
(41,9,4,1.000,'шт'),
(42,9,9,5.000,'г'),
-- Recipe 10: Блины
(43,10,7,150.000,'г'),
(44,10,5,2.000,'шт'),
(45,10,6,200.000,'мл'),
(46,10,8,30.000,'г');

-- Step 6: Create WeeklyMenus table
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
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

INSERT INTO `WeeklyMenus` VALUES 
(1,1,'Недельное меню на 13 ноября','2025-11-13','2025-11-19',3500.00,NOW()),
(2,1,'Тестовое меню','2025-11-01','2025-11-07',2800.00,NOW());

-- Step 7: Create MenuMeals table
CREATE TABLE `MenuMeals` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `MenuId` int NOT NULL,
  `RecipeId` int NOT NULL,
  `MealDate` date NOT NULL,
  `MealType` int NOT NULL COMMENT '1=Breakfast,2=Lunch,3=Dinner',
  PRIMARY KEY (`Id`),
  KEY `MenuId` (`MenuId`),
  KEY `RecipeId` (`RecipeId`),
  KEY `IX_MenuMeals_Date` (`MealDate`),
  CONSTRAINT `menumeals_ibfk_1` FOREIGN KEY (`MenuId`) REFERENCES `WeeklyMenus` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `menumeals_ibfk_2` FOREIGN KEY (`RecipeId`) REFERENCES `Recipes` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Insert menu meals
INSERT INTO `MenuMeals` VALUES 
-- Menu 1, Day 1 (Friday Nov 13)
(1,1,2,'2025-11-13',1), -- Breakfast: Омлет
(2,1,1,'2025-11-13',2), -- Lunch: Курица с рисом
(3,1,4,'2025-11-13',3), -- Dinner: Паста Карбонара
-- Menu 1, Day 2 (Saturday)
(4,1,5,'2025-11-14',1), -- Breakfast: Овсяная каша
(5,1,3,'2025-11-14',2), -- Lunch: Салат
(6,1,1,'2025-11-14',3), -- Dinner: Курица с рисом
-- Menu 1, Day 3 (Sunday)
(7,1,2,'2025-11-15',1), -- Breakfast: Омлет
(8,1,6,'2025-11-15',2), -- Lunch: Борщ
(9,1,7,'2025-11-15',3), -- Dinner: Плов
-- More days...
(10,1,5,'2025-11-16',1),
(11,1,8,'2025-11-16',2),
(12,1,4,'2025-11-16',3),
(13,1,2,'2025-11-17',1),
(14,1,3,'2025-11-17',2),
(15,1,9,'2025-11-17',3),
(16,1,10,'2025-11-18',1),
(17,1,1,'2025-11-18',2),
(18,1,4,'2025-11-18',3),
(19,1,5,'2025-11-19',1),
(20,1,6,'2025-11-19',2),
(21,1,7,'2025-11-19',3);

-- Step 8: Create ShoppingLists table
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
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

INSERT INTO `ShoppingLists` VALUES 
(1,1,1,'Список покупок для недели 13-19 ноября',NOW(),0),
(2,2,1,'Тестовый список покупок',NOW(),0);

-- Step 9: Create ShoppingListItems table
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
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Insert shopping items for first menu
INSERT INTO `ShoppingListItems` VALUES 
-- For Menu 1 (week menu)
(1,1,1,1.500,'кг','Мясо',0), -- Куриная грудка
(2,1,2,1.500,'кг','Крупы',0), -- Рис
(3,1,5,12.000,'шт','Молочные',0), -- Яйца
(4,1,6,0.500,'л','Молочные',0), -- Молоко
(5,1,3,6.000,'шт','Овощи',0), -- Помидоры
(6,1,4,3.000,'шт','Овощи',0), -- Лук
(7,1,14,0.100,'л','Масла',0), -- Оливковое масло
(8,1,9,0.050,'кг','Специи',0), -- Соль
(9,1,10,0.030,'кг','Специи',0), -- Перец
(10,1,17,1.000,'кг','Паста',0), -- Спагетти
(11,1,12,0.500,'кг','Мясо',0), -- Бекон
(12,1,13,0.300,'кг','Сыр',0), -- Пармезан
(13,1,11,0.500,'кг','Крупы',0), -- Овсянка
-- Test items for second list
(14,2,1,0.500,'кг','Мясо',1), -- Already purchased
(15,2,2,0.500,'кг','Крупы',0);

-- Step 10: Create UserFavorites table
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
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

INSERT INTO `UserFavorites` VALUES 
(1,1,1,NOW()), -- User 1 favorites Recipe 1 (Курица с рисом)
(2,1,4,NOW()), -- Паста Карбонара
(3,1,6,NOW()); -- Борщ

-- Step 11: Create UserInventories table
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

INSERT INTO `UserInventories` VALUES 
(1,1,1,0.800,'кг','2025-11-20',NOW()), -- Куриная грудка в наличии
(2,1,2,1.200,'кг',NULL,NOW()), -- Рис
(3,1,5,6.000,'шт','2025-11-18',NOW()), -- Яйца
(4,1,9,0.200,'кг',NULL,NOW()); -- Соль

-- Step 12: Verification queries
SELECT 'Database created successfully' AS Status;
SELECT 'Users' AS TableName, COUNT(*) AS Rows FROM Users;
SELECT 'Ingredients' AS TableName, COUNT(*) AS Rows FROM Ingredients;
SELECT 'Recipes' AS TableName, COUNT(*) AS Rows FROM Recipes;
SELECT 'WeeklyMenus' AS TableName, COUNT(*) AS Rows FROM WeeklyMenus;
SELECT 'ShoppingLists' AS TableName, COUNT(*) AS Rows FROM ShoppingLists;

-- Test Russian text
SELECT 'Test Russian text:' AS Test;
SELECT Name FROM Ingredients WHERE Id = 1; -- Should show 'Куриная грудка'
SELECT Title FROM Recipes WHERE Id = 1; -- Should show 'Курица с рисом'
SELECT Name FROM ShoppingLists WHERE Id = 1; -- Should show 'Список покупок для недели'

-- Check encoding
SHOW CREATE DATABASE recipe_planner;
SHOW TABLE STATUS FROM recipe_planner LIKE 'Recipes';
