CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
) CHARACTER SET=utf8mb4;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260217001916_InitialCreate') THEN

    ALTER DATABASE CHARACTER SET utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260217001916_InitialCreate') THEN

    CREATE TABLE `Clientes` (
        `ClienteId` int NOT NULL AUTO_INCREMENT,
        `Nome` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Telefone` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Email` longtext CHARACTER SET utf8mb4 NOT NULL,
        `DataNascimento` datetime(6) NOT NULL,
        `Endereco` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Observacoes` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Alergias` longtext CHARACTER SET utf8mb4 NOT NULL,
        CONSTRAINT `PK_Clientes` PRIMARY KEY (`ClienteId`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260217001916_InitialCreate') THEN

    CREATE TABLE `Empresas` (
        `EmpresaId` int NOT NULL AUTO_INCREMENT,
        `NomeEmpresa` longtext CHARACTER SET utf8mb4 NOT NULL,
        `CNPJ` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Endereco` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Telefone` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Email` longtext CHARACTER SET utf8mb4 NOT NULL,
        `DataCadastro` datetime(6) NOT NULL,
        CONSTRAINT `PK_Empresas` PRIMARY KEY (`EmpresaId`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260217001916_InitialCreate') THEN

    CREATE TABLE `NivelAcesso` (
        `NivelAcessoId` int NOT NULL AUTO_INCREMENT,
        `Descricao` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Permissoes` longtext CHARACTER SET utf8mb4 NOT NULL,
        `DataCriacao` datetime(6) NOT NULL,
        CONSTRAINT `PK_NivelAcesso` PRIMARY KEY (`NivelAcessoId`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260217001916_InitialCreate') THEN

    CREATE TABLE `Produtos` (
        `ProdutoId` int NOT NULL AUTO_INCREMENT,
        `NomeProduto` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Descricao` longtext CHARACTER SET utf8mb4 NOT NULL,
        `PrecoVenda` decimal(65,30) NOT NULL,
        `QuantidadeEmEstoque` int NOT NULL,
        `Categoria` longtext CHARACTER SET utf8mb4 NOT NULL,
        CONSTRAINT `PK_Produtos` PRIMARY KEY (`ProdutoId`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260217001916_InitialCreate') THEN

    CREATE TABLE `Profissionais` (
        `ProfissionalId` int NOT NULL AUTO_INCREMENT,
        `Nome` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Especializacao` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Telefone` longtext CHARACTER SET utf8mb4 NOT NULL,
        CONSTRAINT `PK_Profissionais` PRIMARY KEY (`ProfissionalId`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260217001916_InitialCreate') THEN

    CREATE TABLE `Servicos` (
        `ServicoId` int NOT NULL AUTO_INCREMENT,
        `NomeServico` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Preco` decimal(65,30) NOT NULL,
        `Descricao` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Duracao` int NOT NULL,
        `Categoria` longtext CHARACTER SET utf8mb4 NOT NULL,
        CONSTRAINT `PK_Servicos` PRIMARY KEY (`ServicoId`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260217001916_InitialCreate') THEN

    CREATE TABLE `Usuarios` (
        `UsuarioId` int NOT NULL AUTO_INCREMENT,
        `NomeUsuario` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Email` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Senha` longtext CHARACTER SET utf8mb4 NOT NULL,
        `NivelAcesso` longtext CHARACTER SET utf8mb4 NOT NULL,
        CONSTRAINT `PK_Usuarios` PRIMARY KEY (`UsuarioId`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260217001916_InitialCreate') THEN

    CREATE TABLE `Fidelidade` (
        `FidelidadeId` int NOT NULL AUTO_INCREMENT,
        `ClienteId` int NOT NULL,
        `PontosAcumulados` int NOT NULL,
        `DataAtualizacao` datetime(6) NOT NULL,
        CONSTRAINT `PK_Fidelidade` PRIMARY KEY (`FidelidadeId`),
        CONSTRAINT `FK_Fidelidade_Clientes_ClienteId` FOREIGN KEY (`ClienteId`) REFERENCES `Clientes` (`ClienteId`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260217001916_InitialCreate') THEN

    CREATE TABLE `EstoqueMovimentacao` (
        `MovimentacaoId` int NOT NULL AUTO_INCREMENT,
        `ProdutoId` int NOT NULL,
        `TipoMovimentacao` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Quantidade` int NOT NULL,
        `DataMovimentacao` datetime(6) NOT NULL,
        CONSTRAINT `PK_EstoqueMovimentacao` PRIMARY KEY (`MovimentacaoId`),
        CONSTRAINT `FK_EstoqueMovimentacao_Produtos_ProdutoId` FOREIGN KEY (`ProdutoId`) REFERENCES `Produtos` (`ProdutoId`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260217001916_InitialCreate') THEN

    CREATE TABLE `ItensVenda` (
        `ItemVendaId` int NOT NULL AUTO_INCREMENT,
        `ProdutoId` int NOT NULL,
        `Quantidade` int NOT NULL,
        `PrecoUnitario` decimal(65,30) NOT NULL,
        CONSTRAINT `PK_ItensVenda` PRIMARY KEY (`ItemVendaId`),
        CONSTRAINT `FK_ItensVenda_Produtos_ProdutoId` FOREIGN KEY (`ProdutoId`) REFERENCES `Produtos` (`ProdutoId`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260217001916_InitialCreate') THEN

    CREATE TABLE `Salarios` (
        `SalarioId` int NOT NULL AUTO_INCREMENT,
        `ProfissionalId` int NOT NULL,
        `SalarioFixo` decimal(10,2) NOT NULL,
        `DataInicio` datetime(6) NOT NULL,
        `DataFim` datetime(6) NULL,
        CONSTRAINT `PK_Salarios` PRIMARY KEY (`SalarioId`),
        CONSTRAINT `FK_Salarios_Profissionais_ProfissionalId` FOREIGN KEY (`ProfissionalId`) REFERENCES `Profissionais` (`ProfissionalId`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260217001916_InitialCreate') THEN

    CREATE TABLE `Agendamentos` (
        `AgendamentoId` int NOT NULL AUTO_INCREMENT,
        `ClienteId` int NOT NULL,
        `ProfissionalId` int NOT NULL,
        `ServicoId` int NOT NULL,
        `DataHora` datetime(6) NOT NULL,
        `Status` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Observacoes` longtext CHARACTER SET utf8mb4 NOT NULL,
        CONSTRAINT `PK_Agendamentos` PRIMARY KEY (`AgendamentoId`),
        CONSTRAINT `FK_Agendamentos_Clientes_ClienteId` FOREIGN KEY (`ClienteId`) REFERENCES `Clientes` (`ClienteId`) ON DELETE CASCADE,
        CONSTRAINT `FK_Agendamentos_Profissionais_ProfissionalId` FOREIGN KEY (`ProfissionalId`) REFERENCES `Profissionais` (`ProfissionalId`) ON DELETE CASCADE,
        CONSTRAINT `FK_Agendamentos_Servicos_ServicoId` FOREIGN KEY (`ServicoId`) REFERENCES `Servicos` (`ServicoId`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260217001916_InitialCreate') THEN

    CREATE TABLE `Feedbacks` (
        `FeedbackId` int NOT NULL AUTO_INCREMENT,
        `ClienteId` int NOT NULL,
        `ServicoId` int NOT NULL,
        `ProfissionalId` int NOT NULL,
        `Avaliacao` int NOT NULL,
        `Comentario` longtext CHARACTER SET utf8mb4 NOT NULL,
        `DataFeedback` datetime(6) NOT NULL,
        CONSTRAINT `PK_Feedbacks` PRIMARY KEY (`FeedbackId`),
        CONSTRAINT `FK_Feedbacks_Clientes_ClienteId` FOREIGN KEY (`ClienteId`) REFERENCES `Clientes` (`ClienteId`) ON DELETE CASCADE,
        CONSTRAINT `FK_Feedbacks_Profissionais_ProfissionalId` FOREIGN KEY (`ProfissionalId`) REFERENCES `Profissionais` (`ProfissionalId`) ON DELETE CASCADE,
        CONSTRAINT `FK_Feedbacks_Servicos_ServicoId` FOREIGN KEY (`ServicoId`) REFERENCES `Servicos` (`ServicoId`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260217001916_InitialCreate') THEN

    CREATE TABLE `Vendas` (
        `VendaId` int NOT NULL AUTO_INCREMENT,
        `DataVenda` datetime(6) NOT NULL,
        `ClienteId` int NULL,
        `ProfissionalId` int NULL,
        `TotalVenda` decimal(65,30) NOT NULL,
        `ServicoId` int NULL,
        CONSTRAINT `PK_Vendas` PRIMARY KEY (`VendaId`),
        CONSTRAINT `FK_Vendas_Clientes_ClienteId` FOREIGN KEY (`ClienteId`) REFERENCES `Clientes` (`ClienteId`),
        CONSTRAINT `FK_Vendas_Servicos_ServicoId` FOREIGN KEY (`ServicoId`) REFERENCES `Servicos` (`ServicoId`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260217001916_InitialCreate') THEN

    CREATE TABLE `Comissoes` (
        `ComissaoId` int NOT NULL AUTO_INCREMENT,
        `ProfissionalId` int NOT NULL,
        `VendaId` int NOT NULL,
        `ValorComissao` decimal(65,30) NOT NULL,
        `DataComissao` datetime(6) NOT NULL,
        CONSTRAINT `PK_Comissoes` PRIMARY KEY (`ComissaoId`),
        CONSTRAINT `FK_Comissoes_Profissionais_ProfissionalId` FOREIGN KEY (`ProfissionalId`) REFERENCES `Profissionais` (`ProfissionalId`) ON DELETE CASCADE,
        CONSTRAINT `FK_Comissoes_Vendas_VendaId` FOREIGN KEY (`VendaId`) REFERENCES `Vendas` (`VendaId`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260217001916_InitialCreate') THEN

    CREATE TABLE `Pagamentos` (
        `PagamentoId` int NOT NULL AUTO_INCREMENT,
        `VendaId` int NOT NULL,
        `ValorPago` decimal(65,30) NOT NULL,
        `DataPagamento` datetime(6) NOT NULL,
        `FormaPagamento` longtext CHARACTER SET utf8mb4 NOT NULL,
        CONSTRAINT `PK_Pagamentos` PRIMARY KEY (`PagamentoId`),
        CONSTRAINT `FK_Pagamentos_Vendas_VendaId` FOREIGN KEY (`VendaId`) REFERENCES `Vendas` (`VendaId`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260217001916_InitialCreate') THEN

    CREATE INDEX `IX_Agendamentos_ClienteId` ON `Agendamentos` (`ClienteId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260217001916_InitialCreate') THEN

    CREATE INDEX `IX_Agendamentos_ProfissionalId` ON `Agendamentos` (`ProfissionalId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260217001916_InitialCreate') THEN

    CREATE INDEX `IX_Agendamentos_ServicoId` ON `Agendamentos` (`ServicoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260217001916_InitialCreate') THEN

    CREATE INDEX `IX_Comissoes_ProfissionalId` ON `Comissoes` (`ProfissionalId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260217001916_InitialCreate') THEN

    CREATE INDEX `IX_Comissoes_VendaId` ON `Comissoes` (`VendaId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260217001916_InitialCreate') THEN

    CREATE INDEX `IX_EstoqueMovimentacao_ProdutoId` ON `EstoqueMovimentacao` (`ProdutoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260217001916_InitialCreate') THEN

    CREATE INDEX `IX_Feedbacks_ClienteId` ON `Feedbacks` (`ClienteId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260217001916_InitialCreate') THEN

    CREATE INDEX `IX_Feedbacks_ProfissionalId` ON `Feedbacks` (`ProfissionalId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260217001916_InitialCreate') THEN

    CREATE INDEX `IX_Feedbacks_ServicoId` ON `Feedbacks` (`ServicoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260217001916_InitialCreate') THEN

    CREATE INDEX `IX_Fidelidade_ClienteId` ON `Fidelidade` (`ClienteId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260217001916_InitialCreate') THEN

    CREATE INDEX `IX_ItensVenda_ProdutoId` ON `ItensVenda` (`ProdutoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260217001916_InitialCreate') THEN

    CREATE INDEX `IX_Pagamentos_VendaId` ON `Pagamentos` (`VendaId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260217001916_InitialCreate') THEN

    CREATE INDEX `IX_Salarios_ProfissionalId` ON `Salarios` (`ProfissionalId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260217001916_InitialCreate') THEN

    CREATE INDEX `IX_Vendas_ClienteId` ON `Vendas` (`ClienteId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260217001916_InitialCreate') THEN

    CREATE INDEX `IX_Vendas_ServicoId` ON `Vendas` (`ServicoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260217001916_InitialCreate') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260217001916_InitialCreate', '8.0.8');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

