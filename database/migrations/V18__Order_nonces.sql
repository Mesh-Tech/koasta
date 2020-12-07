ALTER TABLE "CustomerOrder" ADD COLUMN nonce VARCHAR(500) NOT NULL DEFAULT md5(random()::text || clock_timestamp()::text)::uuid;
CREATE UNIQUE INDEX uq_customerorder_nonce ON "CustomerOrder" (nonce);
