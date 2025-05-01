-- MySQL dump 10.13  Distrib 8.0.36, for Win64 (x86_64)
--
-- Host: localhost    Database: crud
-- ------------------------------------------------------
-- Server version	8.0.36

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
-- Table structure for table `transacoes`
--

DROP TABLE IF EXISTS `transacoes`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `transacoes` (
  `id` int NOT NULL AUTO_INCREMENT,
  `valor` decimal(15,2) NOT NULL,
  `tipo` enum('saldo','saque','deposito','transferencia') CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `contaId` int NOT NULL,
  `dataTransacao` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `contaDestinoId` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `transacoes_ibfk_1` (`contaId`),
  KEY `fk_conta_destino` (`contaDestinoId`),
  CONSTRAINT `fk_conta_destino` FOREIGN KEY (`contaDestinoId`) REFERENCES `conta` (`id`) ON DELETE CASCADE,
  CONSTRAINT `transacoes_ibfk_1` FOREIGN KEY (`contaId`) REFERENCES `conta` (`id`) ON DELETE CASCADE,
  CONSTRAINT `check_conta_destino` CHECK ((((`tipo` <> _utf8mb4'transferencia') and (`contaDestinoId` is null)) or ((`tipo` = _utf8mb4'transferencia') and (`contaDestinoId` is not null))))
) ENGINE=InnoDB AUTO_INCREMENT=117 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `transacoes`
--

LOCK TABLES `transacoes` WRITE;
/*!40000 ALTER TABLE `transacoes` DISABLE KEYS */;
INSERT INTO `transacoes` VALUES (109,100.00,'deposito',27,'2025-05-01 21:37:47',NULL),(110,100.00,'deposito',28,'2025-05-01 21:37:53',NULL),(111,100.00,'deposito',28,'2025-05-01 21:37:56',NULL),(112,100.00,'deposito',27,'2025-05-01 21:38:00',NULL),(113,100.00,'transferencia',27,'2025-05-01 21:38:53',28),(114,100.00,'deposito',28,'2025-05-01 21:38:53',NULL),(115,50.00,'saque',27,'2025-05-01 21:39:19',NULL),(116,300.00,'saque',28,'2025-05-01 21:39:43',NULL);
/*!40000 ALTER TABLE `transacoes` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-05-01 19:13:43
