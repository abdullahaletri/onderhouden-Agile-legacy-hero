ALTER TABLE reservering
    ADD COLUMN betaalwijze TINYINT NULL,
    ADD COLUMN totaal_bedrag DECIMAL(8,2) NULL;