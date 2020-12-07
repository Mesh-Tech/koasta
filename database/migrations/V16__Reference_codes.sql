ALTER TABLE "Company" ADD COLUMN referencecode VARCHAR(500) NOT NULL DEFAULT md5(random()::text || clock_timestamp()::text)::uuid;
ALTER TABLE "Venue" ADD COLUMN referencecode VARCHAR(500) NOT NULL DEFAULT md5(random()::text || clock_timestamp()::text)::uuid;
