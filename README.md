-- ══════════════════════════════════════════════════════
--  TECHNOBANKS – SEMAFOROS_INTELIGENTES
--  Compatível com MySQL 5.7+ / MariaDB (XAMPP)
--  Execute no phpMyAdmin ou via terminal MySQL
-- ══════════════════════════════════════════════════════

CREATE DATABASE IF NOT EXISTS SEMAFOROS_INTELIGENTES
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

USE SEMAFOROS_INTELIGENTES;

-- ──────────────────────────────────────────────────────
--  1. CLIENTES
-- ──────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS clientes (
    id          INT          NOT NULL AUTO_INCREMENT,
    nome        VARCHAR(150) NOT NULL COMMENT 'Nome ou razão social',
    documento   VARCHAR(20)  NOT NULL COMMENT 'CNPJ ou CPF',
    responsavel VARCHAR(100)          COMMENT 'Nome do contato',
    telefone    VARCHAR(20),
    email       VARCHAR(100),
    regiao      VARCHAR(80)  NOT NULL COMMENT 'Subprefeitura de São Paulo',
    criado_em   DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ──────────────────────────────────────────────────────
--  2. EQUIPAMENTOS
-- ──────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS equipamentos (
    id           INT         NOT NULL AUTO_INCREMENT,
    tipo         VARCHAR(80) NOT NULL COMMENT 'Ex: Sensor de Fluxo Veicular',
    modelo       VARCHAR(80) NOT NULL COMMENT 'Referência comercial',
    numero_serie VARCHAR(60)          COMMENT 'Número de série físico',
    status       ENUM(
                     'Ativo',
                     'Inativo',
                     'Em Manutenção',
                     'Com Falha'
                 ) NOT NULL DEFAULT 'Ativo',
    localizacao  VARCHAR(150) NOT NULL COMMENT 'Cruzamento em São Paulo',
    cliente_id   INT                  COMMENT 'FK opcional para clientes',
    criado_em    DATETIME    NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (id),
    CONSTRAINT fk_equip_cliente
        FOREIGN KEY (cliente_id) REFERENCES clientes (id)
        ON DELETE SET NULL
        ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ──────────────────────────────────────────────────────
--  3. ORDENS DE SERVIÇO
-- ──────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS ordens (
    id             INT          NOT NULL AUTO_INCREMENT,
    tipo           VARCHAR(80)  NOT NULL COMMENT 'Ex: Manutenção Corretiva',
    urgencia       ENUM(
                       'Alta',
                       'Média',
                       'Baixa'
                   ) NOT NULL DEFAULT 'Média',
    localizacao    VARCHAR(150) NOT NULL COMMENT 'Cruzamento / rua em SP',
    equip_tipo     VARCHAR(80)           COMMENT 'Equipamento necessário',
    tecnico        VARCHAR(100)          COMMENT 'Técnico responsável',
    descricao      TEXT                  COMMENT 'Detalhes do serviço',
    status         ENUM(
                       'Aguardando',
                       'Em Andamento',
                       'Concluído',
                       'Cancelado'
                   ) NOT NULL DEFAULT 'Aguardando',
    cliente_id     INT                   COMMENT 'FK para clientes',
    equipamento_id INT                   COMMENT 'FK para equipamentos',
    criado_em      DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    atualizado_em  DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP
                                ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (id),
    CONSTRAINT fk_ordem_cliente
        FOREIGN KEY (cliente_id) REFERENCES clientes (id)
        ON DELETE SET NULL
        ON UPDATE CASCADE,
    CONSTRAINT fk_ordem_equipamento
        FOREIGN KEY (equipamento_id) REFERENCES equipamentos (id)
        ON DELETE SET NULL
        ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ──────────────────────────────────────────────────────
--  4. HISTÓRICO DE MANUTENÇÕES
--     Gerado automaticamente via TRIGGER ao concluir OS
-- ──────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS historico_manutencoes (
    id             INT          NOT NULL AUTO_INCREMENT,
    ordem_id       INT          NOT NULL COMMENT 'OS de origem',
    tipo_servico   VARCHAR(80)  NOT NULL,
    localizacao    VARCHAR(150) NOT NULL,
    tecnico        VARCHAR(100),
    equip_tipo     VARCHAR(80),
    observacao     TEXT,
    concluido_em   DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (id),
    CONSTRAINT fk_hist_ordem
        FOREIGN KEY (ordem_id) REFERENCES ordens (id)
        ON DELETE CASCADE
        ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ──────────────────────────────────────────────────────
--  5. TRIGGER — Grava histórico ao concluir uma OS
-- ──────────────────────────────────────────────────────
DELIMITER $$

CREATE TRIGGER trg_historico_manutencao
AFTER UPDATE ON ordens
FOR EACH ROW
BEGIN
    IF NEW.status = 'Concluído' AND OLD.status <> 'Concluído' THEN
        INSERT INTO historico_manutencoes
            (ordem_id, tipo_servico, localizacao, tecnico, equip_tipo, observacao, concluido_em)
        VALUES
            (NEW.id, NEW.tipo, NEW.localizacao, NEW.tecnico,
             NEW.equip_tipo, NEW.descricao, NOW());
    END IF;
END$$

DELIMITER ;

-- ──────────────────────────────────────────────────────
--  6. DADOS DE EXEMPLO
-- ──────────────────────────────────────────────────────

-- Clientes
INSERT INTO clientes (nome, documento, responsavel, telefone, email, regiao) VALUES
('Prefeitura de São Paulo – CET',  '46.392.130/0001-60', 'Carlos Mendes',   '(11) 3392-7000', 'cet@prefeitura.sp.gov.br',      'Centro – Sé'),
('Siemens Mobility Brasil',        '60.643.228/0001-21', 'Ana Fujita',      '(11) 3305-5000', 'mobility@siemens.com.br',        'Pinheiros'),
('Engelog Engenharia Viária Ltda', '12.345.678/0001-99', 'Roberto Salave',  '(11) 94567-8901', 'contato@engelog.com.br',        'Tatuapé'),
('EMDURB – Santo André',          '44.555.111/0001-33', 'Patrícia Sousa',  '(11) 4433-0000', 'emdurb@santoandre.sp.gov.br',    'Santo André');

-- Equipamentos
INSERT INTO equipamentos (tipo, modelo, numero_serie, status, localizacao, cliente_id) VALUES
('Sensor de Fluxo Veicular',   'TB-SFV-300', 'SN-001-2024', 'Ativo',         'Av. Paulista × R. da Consolação',          1),
('Semáforo Veicular LED',      'TB-SVL-100', 'SN-002-2024', 'Ativo',         'Av. Brigadeiro Faria Lima × R. Iguatemi',  1),
('Controlador Semafórico',     'TB-CS-500',  'SN-003-2024', 'Em Manutenção', 'Av. Radial Leste × R. do Oriente',         3),
('Módulo IoT / Gateway',       'TB-IOT-200', 'SN-004-2024', 'Com Falha',     'Marginal Pinheiros × Av. das Nações Unidas',2),
('Câmera de Monitoramento',    'TB-CAM-400', 'SN-005-2024', 'Ativo',         'R. Vergueiro × R. Domingos de Morais',     2),
('Sensor de Velocidade',       'TB-SV-150',  'SN-006-2024', 'Ativo',         'Av. Celso Garcia × R. Bresser',            3);

-- Ordens de Serviço
INSERT INTO ordens (tipo, urgencia, localizacao, equip_tipo, tecnico, descricao, status, cliente_id, equipamento_id) VALUES
('Reparo Emergencial',    'Alta',  'Marginal Pinheiros × Av. das Nações Unidas', 'Módulo IoT / Gateway',     'João Silva',    'Módulo sem comunicação desde 27/05. Sinal intermitente detectado.',         'Em Andamento', 2, 4),
('Manutenção Corretiva',  'Alta',  'Av. Radial Leste × R. do Oriente',           'Controlador Semafórico',   'Marcos Oliveira','Controlador travado no ciclo vermelho. Risco de acidente.',                 'Aguardando',   3, 3),
('Manutenção Preventiva', 'Baixa', 'Av. Paulista × R. da Consolação',            'Sensor de Fluxo Veicular', 'Fernanda Lima', 'Revisão semestral programada. Limpeza de lentes e calibração.',             'Aguardando',   1, 1),
('Instalação de Sensor',  'Média', 'Av. Aricanduva × R. Gervásio Leite Rebelo', 'Sensor de Velocidade',     'Ricardo Matos', 'Novo ponto de monitoramento aprovado pela CET. Instalação do TB-SV-150.',   'Aguardando',   1, NULL);

-- ──────────────────────────────────────────────────────
--  FIM DO SCRIPT
-- ──────────────────────────────────────────────────────
