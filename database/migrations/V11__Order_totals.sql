ALTER TABLE "CustomerOrder" ADD COLUMN total decimal(5,2) NOT NULL DEFAULT 0;
ALTER TABLE "CustomerOrder" ADD COLUMN serviceCharge decimal(5,2) NOT NULL DEFAULT 0;
