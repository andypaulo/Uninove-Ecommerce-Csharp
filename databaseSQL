-- MySQL Workbench Forward Engineering

SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0;
SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0;
SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION';

-- -----------------------------------------------------
-- Schema mydb
-- -----------------------------------------------------
DROP SCHEMA IF EXISTS `mydb` ;

-- -----------------------------------------------------
-- Schema mydb
-- -----------------------------------------------------
CREATE SCHEMA IF NOT EXISTS `mydb` DEFAULT CHARACTER SET utf8 ;
USE `mydb` ;

-- -----------------------------------------------------
-- Table `mydb`.`itens do carrinho`
-- -----------------------------------------------------
DROP TABLE IF EXISTS `mydb`.`itens do carrinho` ;

CREATE TABLE IF NOT EXISTS `mydb`.`itens do carrinho` (
  `id` INT NOT NULL,
  `id_carrinho` INT NOT NULL,
  `id_produto` INT NOT NULL,
  `quantidade` INT NULL,
  `itens do carrinhocol` VARCHAR(45) NOT NULL,
  PRIMARY KEY (`id`, `id_carrinho`, `id_produto`))
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `mydb`.`carrinho`
-- -----------------------------------------------------
DROP TABLE IF EXISTS `mydb`.`carrinho` ;

CREATE TABLE IF NOT EXISTS `mydb`.`carrinho` (
  `id` INT NOT NULL,
  `criado-em` DATETIME NULL,
  PRIMARY KEY (`id`))
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `mydb`.`produto`
-- -----------------------------------------------------
DROP TABLE IF EXISTS `mydb`.`produto` ;

CREATE TABLE IF NOT EXISTS `mydb`.`produto` (
  `id` INT NOT NULL,
  `id_produto` INT NOT NULL,
  `discricao_produto` VARCHAR(255) NULL,
  `preco` DECIMAL(10,2) NULL,
  `imagem` VARCHAR(255) NULL,
  `id_categoria` INT NULL,
  `nome` VARCHAR(255) NOT NULL,
  PRIMARY KEY (`id`, `id_produto`))
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `mydb`.`categoria`
-- -----------------------------------------------------
DROP TABLE IF EXISTS `mydb`.`categoria` ;

CREATE TABLE IF NOT EXISTS `mydb`.`categoria` (
  `id` INT NOT NULL,
  `nome` VARCHAR(255) NOT NULL,
  PRIMARY KEY (`id`))
ENGINE = InnoDB;


SET SQL_MODE=@OLD_SQL_MODE;
SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS;
SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS;
